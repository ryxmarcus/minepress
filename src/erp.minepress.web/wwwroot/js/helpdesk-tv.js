/**
 * MinePress Helpdesk TV Display — Live Dashboard
 * Auto-refreshes, auto-cycles tabs, generates AI insights,
 * and announces events via speech synthesis.
 */
const HelpdeskTV = (() => {
    'use strict';

    // ── Config ──
    const REFRESH_INTERVAL = 30000;     // 30s data refresh
    const CYCLE_INTERVAL   = 15000;     // 15s tab auto-cycle
    const STAGE_CYCLE_INTERVAL = 8000;  // 8s stage auto-cycle
    const CLOCK_INTERVAL   = 1000;
    const EVENTS_INTERVAL  = 35000;     // 35s events poll (offset from data refresh)
    const TOAST_DURATION   = 6000;      // 6s toast display
    const API_URL          = '/api/production/tv-display';
    const EVENTS_URL       = '/api/production/tv-events';

    let _data        = null;
    let _prevData    = null;
    let _cycleTimer  = null;
    let _refreshTimer = null;
    let _eventsTimer = null;
    let _stageCycleTimer = null;
    let _tabIndex    = 0;
    let _stageIndex  = 0;
    let _stageFilter = 'all';
    const _tabs      = ['running', 'queue', 'machines', 'workforce'];
    const _stageTabs = ['all', 'designing', 'ctp', 'printing', 'binding', 'finishing', 'delivery'];

    // ── Speech State ──
    let _speechEnabled  = true;
    let _speechQueue    = [];
    let _isSpeaking     = false;
    let _lastEventTime  = null;
    let _seenEventIds   = new Set();
    let _firstLoad      = true;

    // ── Init ──
    function init() {
        _startClock();
        _bindTabs();
        _bindStageTabs();
        _initSpeech();
        _loadData();
        _startAutoRefresh();
        _startAutoCycle();
        _startStageAutoCycle();
        _startEventsPolling();
        document.documentElement.setAttribute('data-bs-theme', 'dark');
    }

    function _bindStageTabs() {
        document.querySelectorAll('.tv-stage-tab').forEach((btn, idx) => {
            btn.addEventListener('click', () => {
                _stageIndex = idx;
                _setStageFilter((btn.dataset.stage || 'all').toLowerCase());
                _resetStageCycle();
            });
        });
    }

    function _setStageFilter(stage) {
        _stageFilter = stage;
        document.querySelectorAll('.tv-stage-tab').forEach(b => b.classList.toggle('active', (b.dataset.stage || '').toLowerCase() === stage));

        if (_data) {
            _renderRunning(_data.runningJobs || []);
            _updateRunningSummaryCounts(_data);
            _updateStageTabCounts(_data.runningJobs || []);
        }
    }

    // ── Clock ──
    function _startClock() {
        const el = document.getElementById('tvClock');
        if (!el) return;
        const tick = () => {
            const now = new Date();
            el.textContent = now.toLocaleTimeString('en-IN', { hour: '2-digit', minute: '2-digit', second: '2-digit', hour12: true }).toUpperCase();
        };
        tick();
        setInterval(tick, CLOCK_INTERVAL);
    }

    // ── Tab Binding ──
    function _bindTabs() {
        document.querySelectorAll('.tv-tab').forEach((btn, idx) => {
            btn.addEventListener('click', () => {
                _tabIndex = idx;
                _activateTab(_tabs[idx]);
                _resetCycle();
            });
        });
    }

    function _activateTab(tabId) {
        const idx = _tabs.indexOf(tabId);
        if (idx >= 0) _tabIndex = idx;

        document.querySelectorAll('.tv-tab').forEach(t => t.classList.toggle('active', t.dataset.tab === tabId));
        document.querySelectorAll('.tv-panel').forEach(p => p.classList.toggle('active', p.id === 'panel' + _capitalize(tabId)));

        const stageTabs = document.getElementById('tvStageTabs');
        if (stageTabs) {
            stageTabs.style.display = tabId === 'running' ? 'flex' : 'none';
        }
    }

    function _capitalize(s) { return s.charAt(0).toUpperCase() + s.slice(1); }

    // ── Auto Cycle ──
    function _startAutoCycle() {
        const bar = document.getElementById('tvCycleBar');
        if (bar) {
            bar.style.setProperty('--cycle-duration', CYCLE_INTERVAL + 'ms');
            bar.classList.add('cycling');
        }
        _cycleTimer = setInterval(() => {
            // Running tab is controlled by stage-cycle progression.
            if (_tabs[_tabIndex] === 'running') return;
            _tabIndex = (_tabIndex + 1) % _tabs.length;
            _activateTab(_tabs[_tabIndex]);
        }, CYCLE_INTERVAL);
    }

    function _resetCycle() {
        clearInterval(_cycleTimer);
        const bar = document.getElementById('tvCycleBar');
        if (bar) { bar.classList.remove('cycling'); void bar.offsetWidth; bar.classList.add('cycling'); }
        _startAutoCycle();
    }

    function _startStageAutoCycle() {
        _stageCycleTimer = setInterval(() => {
            // Stage auto-cycle applies only while Running tab is active.
            if (_tabs[_tabIndex] !== 'running') return;

            _stageIndex = (_stageIndex + 1) % _stageTabs.length;

            // After finishing all stage tabs, jump to Jobs in Queue tab.
            if (_stageIndex === 0) {
                _setStageFilter('all');
                _activateTab('queue');
                return;
            }

            _setStageFilter(_stageTabs[_stageIndex]);
        }, STAGE_CYCLE_INTERVAL);
    }

    function _resetStageCycle() {
        clearInterval(_stageCycleTimer);
        _startStageAutoCycle();
    }

    // ── Auto Refresh ──
    function _startAutoRefresh() {
        _refreshTimer = setInterval(_loadData, REFRESH_INTERVAL);
    }

    // ── Events Polling ──
    function _startEventsPolling() {
        _eventsTimer = setInterval(_pollEvents, EVENTS_INTERVAL);
    }

    // ── Load Data ──
    function _loadData() {
        $.getJSON(API_URL)
            .done(data => {
                _prevData = _data;
                _data = data;
                _renderKpis(data.stats);
                _renderRunning(data.runningJobs);
                _renderQueue(data.queueJobs);
                _renderMachines(data.machines);
                _renderWorkforce(data.workforce);
                _updateBadges(data);
                _updateRunningSummaryCounts(data);
                _updateStageTabCounts(data.runningJobs || []);
                _generateAiInsights(data);
                if (!_firstLoad) {
                    _detectDataChanges(_prevData, data);
                }
                _firstLoad = false;
            })
            .fail(() => {
                console.warn('[HelpdeskTV] Failed to load data');
            });
    }

    // ── Poll Server Events ──
    function _pollEvents() {
        var params = _lastEventTime ? '?since=' + encodeURIComponent(_lastEventTime) : '';
        $.getJSON(EVENTS_URL + params)
            .done(resp => {
                if (resp.serverTime) _lastEventTime = resp.serverTime;
                if (!resp.events || resp.events.length === 0) return;
                resp.events.forEach(evt => {
                    if (_seenEventIds.has(evt.activityLogId)) return;
                    _seenEventIds.add(evt.activityLogId);
                    _announceServerEvent(evt);
                });
                // Keep set bounded
                if (_seenEventIds.size > 200) {
                    var arr = Array.from(_seenEventIds);
                    _seenEventIds = new Set(arr.slice(arr.length - 100));
                }
            })
            .fail(() => console.warn('[HelpdeskTV] Failed to poll events'));
    }

    // ── Announce Server Event ──
    function _announceServerEvent(evt) {
        var mod = (evt.module || '').toUpperCase();
        var type = (evt.activityType || '').toUpperCase();
        var title = evt.title || '';
        var desc = evt.description || '';
        var code = evt.entityCode || '';
        var msg = '';
        var icon = 'bi-bell';
        var severity = 'info';

        if (mod === 'ENQUIRY' && type === 'CREATE') {
            msg = 'New enquiry received' + (code ? ', ' + code : '') + '. ' + title;
            icon = 'bi-envelope-paper'; severity = 'success';
        } else if (mod === 'JOB' && type === 'CREATE') {
            msg = 'New job created' + (code ? ', ' + code : '') + '. ' + title;
            icon = 'bi-plus-circle'; severity = 'success';
        } else if (mod === 'JOB' && type === 'STATUS_CHANGE') {
            msg = 'Job status updated' + (code ? ' for ' + code : '') + '. ' + title;
            icon = 'bi-arrow-repeat'; severity = 'info';
        } else if (mod === 'PAYMENT') {
            msg = 'Payment received' + (code ? ' for ' + code : '') + '. ' + title;
            icon = 'bi-cash-stack'; severity = 'success';
        } else if (mod === 'DISPATCH') {
            msg = (desc || title || 'Dispatch update') + (code ? ', ' + code : '');
            icon = 'bi-truck'; severity = 'info';
        } else if (mod === 'QUOTATION' && type === 'CREATE') {
            msg = 'New quotation created' + (code ? ', ' + code : '') + '. ' + title;
            icon = 'bi-file-earmark-text'; severity = 'info';
        } else if (mod === 'QUOTATION' && type === 'APPROVE') {
            msg = 'Quotation approved' + (code ? ', ' + code : '') + '. ' + title;
            icon = 'bi-check-circle'; severity = 'success';
        } else if (mod === 'QUALITY') {
            msg = 'Quality check update. ' + title;
            icon = 'bi-shield-check'; severity = 'info';
        } else if (mod === 'PRODUCTION' && type === 'ASSIGN') {
            msg = title || 'Production assignment updated';
            icon = 'bi-gear'; severity = 'info';
        } else {
            msg = title || (mod + ' ' + type + ' ' + code);
            icon = 'bi-info-circle'; severity = 'info';
        }

        if (msg) {
            _speak(msg);
            _showToast(msg, mod, icon, severity);
        }
    }

    // ── Detect Data Changes (Diff) ──
    function _detectDataChanges(prev, curr) {
        if (!prev) return;

        // Build lookup maps
        var prevRunIds = new Set((prev.runningJobs || []).map(j => j.jobId));
        var currRunIds = new Set((curr.runningJobs || []).map(j => j.jobId));
        var prevQueueIds = new Set((prev.queueJobs || []).map(j => j.jobId));
        var currQueueIds = new Set((curr.queueJobs || []).map(j => j.jobId));

        // New running jobs (appeared in running that weren't before)
        (curr.runningJobs || []).forEach(j => {
            if (!prevRunIds.has(j.jobId)) {
                var msg = 'Job ' + j.jobNo + ' is now running on ' + (j.machineName || 'machine') + '.';
                if (j.productName) msg += ' Product: ' + j.productName + '.';
                _speak(msg);
                _showToast(msg, 'PRODUCTION', 'bi-play-circle-fill', 'success');
            }
        });

        // Jobs removed from running (completed or deallocated)
        (prev.runningJobs || []).forEach(j => {
            if (!currRunIds.has(j.jobId) && !currQueueIds.has(j.jobId)) {
                var msg = 'Job ' + j.jobNo + ' has been completed or dispatched. Well done!';
                _speak(msg);
                _showToast(msg, 'JOB', 'bi-check-circle-fill', 'success');
            }
        });

        // New jobs in queue
        var newQueueCount = 0;
        (curr.queueJobs || []).forEach(j => {
            if (!prevQueueIds.has(j.jobId) && !prevRunIds.has(j.jobId)) newQueueCount++;
        });
        if (newQueueCount > 0) {
            var msg = newQueueCount + ' new job' + (newQueueCount > 1 ? 's' : '') + ' added to the queue.';
            _speak(msg);
            _showToast(msg, 'JOB', 'bi-collection', 'info');
        }

        // Machine status changes
        var prevMachMap = {};
        (prev.machines || []).forEach(m => { prevMachMap[m.machineId] = m; });
        (curr.machines || []).forEach(m => {
            var pm = prevMachMap[m.machineId];
            if (!pm) return;
            if (pm.status !== m.status) {
                if (m.status === 'BREAKDOWN') {
                    var msg = 'Alert! ' + m.machineName + ' has gone into breakdown.' + (m.breakdownFault ? ' Fault: ' + m.breakdownFault + '.' : '') + ' Immediate attention required.';
                    _speak(msg);
                    _showToast(msg, 'MACHINE', 'bi-exclamation-triangle-fill', 'danger');
                } else if (m.status === 'RUNNING' && pm.status === 'IDLE') {
                    var msg = m.machineName + ' is now running.' + (m.currentJob ? ' Job ' + m.currentJob + '.' : '');
                    _speak(msg);
                    _showToast(msg, 'MACHINE', 'bi-gear-wide-connected', 'success');
                } else if (m.status === 'RUNNING' && pm.status === 'BREAKDOWN') {
                    var msg = 'Good news! ' + m.machineName + ' is back online and running.';
                    _speak(msg);
                    _showToast(msg, 'MACHINE', 'bi-check-circle', 'success');
                } else if (m.status === 'IDLE' && pm.status === 'RUNNING') {
                    var msg = m.machineName + ' is now idle.';
                    _speak(msg);
                    _showToast(msg, 'MACHINE', 'bi-pause-circle', 'warning');
                }
            }
        });

        // AI Predictions — overdue job warnings
        var prevStats = prev.stats || {};
        var currStats = curr.stats || {};
        if (currStats.overdueJobs > (prevStats.overdueJobs || 0)) {
            var newOverdue = currStats.overdueJobs - (prevStats.overdueJobs || 0);
            var msg = 'Warning! ' + newOverdue + ' additional job' + (newOverdue > 1 ? 's are' : ' is') + ' now overdue. Escalation recommended.';
            _speak(msg);
            _showToast(msg, 'AI', 'bi-alarm', 'danger');
        }

        // AI Prediction — queue pressure increase
        if (currStats.queuedJobs > currStats.activeJobs && currStats.queuedJobs > (prevStats.queuedJobs || 0) + 2) {
            var msg = 'AI Insight: Queue pressure is rising. ' + currStats.queuedJobs + ' jobs waiting versus ' + currStats.activeJobs + ' active. Consider adding a shift.';
            _speak(msg);
            _showToast(msg, 'AI', 'bi-graph-up-arrow', 'warning');
        }

        // AI Prediction — estimate completion for urgent running jobs
        (curr.runningJobs || []).forEach(j => {
            if ((j.priority === 'Urgent' || j.priority === 'Critical') && j.progressPercent > 0 && j.progressPercent < 100) {
                var prevJob = (prev.runningJobs || []).find(pj => pj.jobId === j.jobId);
                if (prevJob && j.progressPercent > prevJob.progressPercent) {
                    var rate = j.progressPercent - prevJob.progressPercent;
                    var remaining = 100 - j.progressPercent;
                    var cyclesLeft = Math.ceil(remaining / rate);
                    var minsLeft = cyclesLeft * (REFRESH_INTERVAL / 60000);
                    if (minsLeft > 0 && minsLeft < 480) {
                        var msg = 'AI Prediction: ' + j.priority + ' job ' + j.jobNo + ' is at ' + j.progressPercent + ' percent. Estimated completion in approximately ' + Math.round(minsLeft) + ' minutes.';
                        _speak(msg);
                        _showToast(msg, 'AI', 'bi-stars', 'info');
                    }
                }
            }
        });
    }

    // ── KPIs ──
    function _renderKpis(s) {
        _setText('kpiRunning', s.running);
        _setText('kpiIdle', s.idle);
        _setText('kpiBreakdown', s.breakdown);
        _setText('kpiActiveJobs', s.activeJobs);
        _setText('kpiQueued', s.queuedJobs);
        _setText('kpiOverdue', s.overdueJobs);
        _setText('kpiWorkers', s.totalWorkers);
    }

    function _updateBadges(d) {
        _setText('badgeQueue', d.queueJobs ? d.queueJobs.length : 0);
        _setText('badgeMachines', d.machines ? d.machines.length : 0);
        _setText('badgeWorkforce', d.workforce ? d.workforce.length : 0);
    }

    function _updateRunningSummaryCounts(data) {
        const runningJobs = data?.runningJobs || [];
        const totalRunningCount = runningJobs.length;
        const runningCount = _stageFilter === 'all'
            ? totalRunningCount
            : _applyStageFilter(runningJobs, _stageFilter).length;

        _setText('badgeRunning', runningCount);
        _setText('kpiActiveJobs', _num(totalRunningCount));
    }

    function _updateStageTabCounts(jobs) {
        const list = jobs || [];
        const counts = {
            all: list.length,
            designing: _applyStageFilter(list, 'designing').length,
            ctp: _applyStageFilter(list, 'ctp').length,
            printing: _applyStageFilter(list, 'printing').length,
            binding: _applyStageFilter(list, 'binding').length,
            finishing: _applyStageFilter(list, 'finishing').length,
            delivery: _applyStageFilter(list, 'delivery').length
        };

        document.querySelectorAll('.tv-stage-tab').forEach(btn => {
            const stageKey = (btn.dataset.stage || 'all').toLowerCase();
            const labelEl = btn.querySelector('span');
            if (!labelEl) return;

            if (!btn.dataset.baseLabel) {
                btn.dataset.baseLabel = labelEl.textContent.replace(/\s*\(\d+\)\s*$/, '').trim();
            }

            const baseLabel = btn.dataset.baseLabel || labelEl.textContent;
            const count = counts[stageKey] ?? 0;
            labelEl.textContent = `${baseLabel} (${count})`;
        });
    }

    // ── Running Jobs ──
    function _renderRunning(jobs) {
        const grid = document.getElementById('gridRunning');
        if (!grid) return;

        const filteredJobs = _applyStageFilter(jobs || []);

        if (!filteredJobs || filteredJobs.length === 0) {
            const stageLabel = _stageFilterLabel();
            grid.innerHTML = `<div class="tv-empty-state"><i class="bi bi-inbox"></i><span>No running jobs${_stageFilter === 'all' ? '' : ` for ${_esc(stageLabel)}`}</span></div>`;
            return;
        }

        grid.innerHTML = filteredJobs.map(j => {
            const prio = j.priority || 'Normal';
            const pct = j.progressPercent || 0;
            const delivery = _fmtDate(j.deliveryDate);
            const isOverdue = j.deliveryDate && new Date(j.deliveryDate) < new Date(new Date().toDateString());
            const isWorkspaceTask = (j.cardSource || '').toUpperCase() === 'WORKSPACE_TASK';
            const stage = (j.taskStage || '').toUpperCase();
            const workers = (j.workers || []).map(w =>
                `<span class="tv-worker-chip">${_esc(w.employeeName || '')}${w.roleCode ? ' · ' + _esc(w.roleCode) : ''}</span>`
            ).join('');

            const workspaceMeta = isWorkspaceTask
                ? `<div class="tv-job-meta">
                        <span class="tv-job-tag"><i class="bi bi-diagram-3"></i> ${_esc(stage || 'RUNNING')}</span>
                        <span class="tv-job-tag"><i class="bi bi-person"></i> ${_esc(j.taskUserName || '—')}${j.taskUserCode ? ' · ' + _esc(j.taskUserCode) : ''}</span>
                        <span class="tv-job-tag"><i class="bi bi-clock-history"></i> ${_fmtDateTime(j.taskStartedOn)}</span>
                        <span class="tv-job-tag"><i class="bi bi-flag"></i> ${_esc(j.taskStatus || 'IN_PROGRESS')}</span>
                    </div>`
                : '';

            const workspaceStageDetail = !isWorkspaceTask
                ? ''
                : stage === 'DESIGNING'
                    ? `<div class="tv-job-customer"><i class="bi bi-pencil-square"></i> Design Type: ${_esc(j.taskWorkType || '—')}</div>`
                    : stage === 'CTP'
                        ? `<div class="tv-job-customer"><i class="bi bi-layers"></i> Plate Name: ${_esc(j.plateName || '—')}</div>`
                        : stage === 'PRINTING'
                            ? `<div class="tv-job-customer"><i class="bi bi-printer"></i> Machine: ${_esc(j.machineName || '—')}</div>`
                            : '';

            return `<div class="tv-job-card priority-${_esc(prio)}">
                <div class="tv-job-row">
                    <span class="tv-job-no">${_esc(j.jobNo)}</span>
                    <span class="tv-job-machine"><i class="bi ${isWorkspaceTask ? 'bi-play-circle' : 'bi-cpu'}"></i> ${_esc(isWorkspaceTask ? (j.taskStage || 'TASK') : (j.machineName || '—'))}</span>
                </div>
                <div class="tv-job-product" title="${_esc(j.productName || '')}">${_esc(j.productName || '—')}</div>
                <div class="tv-job-customer"><i class="bi bi-building"></i> ${_esc(j.partyName || '—')}</div>
                ${workspaceStageDetail}
                <div class="tv-job-meta">
                    <span class="tv-job-tag tag-qty"><i class="bi bi-stack"></i> ${_num(j.quantity)}</span>
                    <span class="tv-job-tag tag-type">${_esc(j.jobTypeName || '—')}</span>
                    <span class="tv-job-tag tag-priority-${_esc(prio)}">${_esc(prio)}</span>
                    <span class="tv-job-tag tag-delivery${isOverdue ? ' tag-overdue' : ''}">
                        <i class="bi bi-calendar-event"></i> ${delivery}${isOverdue ? ' ⚠' : ''}
                    </span>
                    ${j.aiPriorityScore ? `<span class="tv-job-tag"><i class="bi bi-stars"></i> AI ${j.aiPriorityScore}</span>` : ''}
                </div>
                ${workspaceMeta}
                <div class="tv-progress-wrap">
                    <div class="tv-progress"><div class="tv-progress-fill" style="width:${pct}%"></div></div>
                    <span class="tv-progress-pct">${pct}%</span>
                </div>
                ${workers ? `<div class="tv-workers">${workers}</div>` : ''}
            </div>`;
        }).join('');
    }

    // ── Queue ──
    function _renderQueue(jobs) {
        const tbody = document.getElementById('tbodyQueue');
        if (!tbody) return;

        if (!jobs || jobs.length === 0) {
            tbody.innerHTML = '<tr><td colspan="6" class="tv-empty-cell"><i class="bi bi-check-circle me-2"></i>All jobs are allocated</td></tr>';
            return;
        }

        const today = new Date(new Date().toDateString());
        tbody.innerHTML = jobs.map(j => {
            const isOverdue = j.deliveryDate && new Date(j.deliveryDate) < today;

            const prevLabel  = j.prevStep    || null;
            const currLabel  = j.currentStep || j.currentStage || null;
            const nextLabel  = j.nextStep    || null;

            const stepFlow = `<div class="tv-step-flow">
                <span class="tv-step tv-step-prev" title="Previous Step">
                    <i class="bi bi-check-circle-fill tv-step-icon"></i>
                    <span class="tv-step-label">${_esc(prevLabel || '—')}</span>
                </span>
                <i class="bi bi-chevron-double-right tv-step-arrow"></i>
                <span class="tv-step tv-step-current${!currLabel ? ' tv-step-empty' : ''}" title="Current Step">
                    <i class="bi bi-play-circle-fill tv-step-icon"></i>
                    <span class="tv-step-label">${_esc(currLabel || '—')}</span>
                </span>
                <i class="bi bi-chevron-double-right tv-step-arrow"></i>
                <span class="tv-step tv-step-next${!nextLabel ? ' tv-step-empty' : ''}" title="Next Step">
                    <i class="bi bi-arrow-right-circle tv-step-icon"></i>
                    <span class="tv-step-label">${_esc(nextLabel || '—')}</span>
                </span>
            </div>`;

            return `<tr>
                <td class="fw-bold">${_esc(j.jobNo)}</td>
                <td>${_esc(j.partyName || '—')}</td>
                <td class="tv-priority-cell tv-priority-${_esc(j.priority || 'Normal')}">${_esc(j.priority || '—')}</td>
                <td class="${isOverdue ? 'tv-delivery-overdue' : ''}">${_fmtDate(j.deliveryDate)}${isOverdue ? ' ⚠' : ''}</td>
                <td>${stepFlow}</td>
                <td>${_esc(j.statusCode || '—')}</td>
            </tr>`;
        }).join('');
    }

    function _stageFilterLabel() {
        const map = {
            all: 'All Stages',
            designing: 'Designing/DTP',
            ctp: 'CTP',
            printing: 'Printing',
            binding: 'Binding',
            finishing: 'Finishing',
            delivery: 'Delivery'
        };
        return map[_stageFilter] || 'Selected Stage';
    }

    function _applyStageFilter(jobs, stageKey) {
        const key = (stageKey || _stageFilter || 'all').toLowerCase();
        if (key === 'all') return jobs;
        return jobs.filter(j => _matchesStageFor(j, key));
    }

    function _matchesStage(job) {
        return _matchesStageFor(job, _stageFilter);
    }

    function _matchesStageFor(job, stageKey) {
        const stage = ((job?.taskStage || '') + ' ' + (job?.currentStage || '') + ' ' + (job?.statusCode || '')).toLowerCase();
        switch ((stageKey || 'all').toLowerCase()) {
            case 'designing':
                return stage.includes('design') || stage.includes('dtp') || stage.includes('artwork');
            case 'ctp':
                return stage.includes('ctp') || stage.includes('plate') || stage.includes('pre-press') || stage.includes('prepress');
            case 'printing':
                return stage.includes('print');
            case 'binding':
                return stage.includes('bind');
            case 'finishing':
                return stage.includes('finish') || stage.includes('laminat') || stage.includes('fold') || stage.includes('trim') || stage.includes('post-press') || stage.includes('postpress');
            case 'delivery':
                return stage.includes('delivery') || stage.includes('dispatch') || stage.includes('challan') || stage.includes('gate pass') || stage.includes('load');
            default:
                return true;
        }
    }

    // ── Machines ──
    function _renderMachines(machines) {
        const grid = document.getElementById('gridMachines');
        if (!grid) return;

        if (!machines || machines.length === 0) {
            grid.innerHTML = '<div class="tv-empty-state"><i class="bi bi-cpu"></i><span>No machines found</span></div>';
            return;
        }

        grid.innerHTML = machines.map(m => {
            const statusIcon = m.status === 'RUNNING' ? 'bi-play-circle-fill'
                             : m.status === 'BREAKDOWN' ? 'bi-exclamation-triangle-fill'
                             : 'bi-pause-circle';
            const statusLabel = m.status === 'RUNNING' ? 'Running'
                              : m.status === 'BREAKDOWN' ? 'Breakdown'
                              : 'Idle';

            return `<div class="tv-machine-card status-${_esc(m.status)}">
                <div class="tv-job-row">
                    <span class="tv-machine-name">${_esc(m.machineName)}</span>
                    <span class="tv-machine-status st-${_esc(m.status)}">
                        <i class="bi ${statusIcon}"></i> ${statusLabel}
                    </span>
                </div>
                <span class="tv-machine-code">${_esc(m.machineCode)} · ${_esc(m.machineCategory || '—')}</span>
                ${m.status === 'RUNNING' ? `<div class="tv-machine-info"><strong>${_esc(m.currentJob || '')}</strong> — ${_esc(m.currentProduct || '—')} <span class="text-muted">(${m.jobCount} job${m.jobCount !== 1 ? 's' : ''})</span></div>` : ''}
                ${m.status === 'BREAKDOWN' ? `<div class="tv-machine-info" style="color:#fca5a5"><i class="bi bi-exclamation-circle me-1"></i>${_esc(m.breakdownFault || 'Unknown')} · ${_esc(m.breakdownSeverity || '')}</div>` : ''}
                ${m.maxSpeedPerHour ? `<div class="tv-machine-info"><i class="bi bi-speedometer2 me-1"></i>${_num(m.maxSpeedPerHour)}/hr</div>` : ''}
            </div>`;
        }).join('');
    }

    // ── Workforce ──
    function _renderWorkforce(workers) {
        const tbody = document.getElementById('tbodyWorkforce');
        if (!tbody) return;

        if (!workers || workers.length === 0) {
            tbody.innerHTML = '<tr><td colspan="6" class="tv-empty-cell"><i class="bi bi-people me-2"></i>No workers assigned</td></tr>';
            return;
        }

        tbody.innerHTML = workers.map(w => {
            const processChips = (w.processes || []).map(p => `<span class="tv-wf-machine">${_esc(p.processName || '—')}</span>`).join('');
            const startTimeChips = (w.processes || []).map(p => `<span class="tv-wf-machine">${_fmtDateTime(p.startedOn)}</span>`).join('');
            const totalTime = _calcTotalProcessTime(w.processes, w.workStartTime);
            const leaveBadge = w.isOnLeave
                ? `<span class="tv-wf-leave-badge"><i class="bi bi-calendar-x me-1"></i>${_esc(w.leaveLabel || 'On Leave')}</span>`
                : '';
            const rowClass = w.isOnLeave ? ' class="tv-wf-on-leave"' : '';

            return `<tr${rowClass}>
                <td class="fw-bold">${_esc(w.employeeName || '—')} ${leaveBadge}</td>
                <td>${_esc(w.employeeCode || '—')}</td>
                <td>${_esc(w.roleCode || '—')}</td>
                <td><div class="tv-wf-machines">${processChips || '—'}</div></td>
                <td><div class="tv-wf-machines">${startTimeChips || _fmtDateTime(w.workStartTime)}</div></td>
                <td><span class="tv-wf-total-time">${_esc(totalTime)}</span></td>
            </tr>`;
        }).join('');
    }

    function _calcTotalProcessTime(processes, fallbackStartTime) {
        const now = new Date();
        const starts = (processes || [])
            .map(p => p?.startedOn)
            .filter(Boolean)
            .map(v => new Date(v))
            .filter(d => !isNaN(d.getTime()) && d <= now);

        if (starts.length === 0 && fallbackStartTime) {
            const d = new Date(fallbackStartTime);
            if (!isNaN(d.getTime()) && d <= now) starts.push(d);
        }

        if (starts.length === 0) return '00m';

        const totalMinutes = starts.reduce((sum, dt) => sum + Math.max(0, Math.floor((now - dt) / 60000)), 0);
        const days = Math.floor(totalMinutes / 1440);
        const hours = Math.floor((totalMinutes % 1440) / 60);
        const mins = totalMinutes % 60;

        if (days > 0) return `${days}d ${hours}h ${mins}m`;
        if (hours > 0) return `${hours}h ${mins}m`;
        return `${mins}m`;
    }

    // ── AI Insights Generator ──
    function _generateAiInsights(data) {
        const insights = [];
        const s = data.stats;

        // Utilization
        if (s.totalMachines > 0) {
            const util = Math.round((s.running / s.totalMachines) * 100);
            insights.push(`🏭 Machine utilization: ${util}% (${s.running} of ${s.totalMachines} running)`);
        }

        // Breakdown alert
        if (s.breakdown > 0) {
            insights.push(`⚠️ ${s.breakdown} machine${s.breakdown > 1 ? 's' : ''} in breakdown — immediate attention required`);
        }

        // Overdue
        if (s.overdueJobs > 0) {
            insights.push(`🔴 ${s.overdueJobs} overdue job${s.overdueJobs > 1 ? 's' : ''} past delivery date — escalation recommended`);
        }

        // Queue pressure
        if (s.queuedJobs > s.activeJobs && s.queuedJobs > 5) {
            insights.push(`📈 Queue pressure: ${s.queuedJobs} jobs waiting vs ${s.activeJobs} active — consider adding shifts`);
        }

        // Idle machines
        if (s.idle > 0 && s.queuedJobs > 0) {
            insights.push(`💡 ${s.idle} idle machine${s.idle > 1 ? 's' : ''} available with ${s.queuedJobs} jobs in queue — allocation opportunity`);
        }

        // Worker to job ratio
        if (s.totalWorkers > 0 && s.activeJobs > 0) {
            const ratio = (s.totalWorkers / s.activeJobs).toFixed(1);
            insights.push(`👷 Worker ratio: ${ratio} workers per active job`);
        }

        // Urgent/Critical jobs in running
        if (data.runningJobs) {
            const urgent = data.runningJobs.filter(j => j.priority === 'Urgent' || j.priority === 'Critical');
            if (urgent.length > 0) {
                insights.push(`🚨 ${urgent.length} urgent/critical job${urgent.length > 1 ? 's' : ''} currently in production`);
            }
        }

        // AI priority score highlights
        if (data.queueJobs) {
            const highAi = data.queueJobs.filter(j => j.aiPriorityScore && j.aiPriorityScore >= 70);
            if (highAi.length > 0) {
                insights.push(`🧠 ${highAi.length} high-priority AI-scored job${highAi.length > 1 ? 's' : ''} waiting in queue`);
            }
        }

        // Healthy status
        if (s.breakdown === 0 && s.overdueJobs === 0) {
            insights.push('✅ All systems healthy — no breakdowns or overdue jobs');
        }

        // Render ticker
        const ticker = document.getElementById('tvAiTicker');
        if (ticker && insights.length > 0) {
            const msg = insights.join('     ·     ');
            ticker.innerHTML = `<span class="tv-ai-msg">${_esc(msg)}</span>`;
        }
    }

    // ── Speech Engine ──
    function _initSpeech() {
        // Bind mute toggle
        var btn = document.getElementById('tvSpeechToggle');
        if (btn) {
            btn.addEventListener('click', _toggleSpeech);
        }
        // Restore preference
        try {
            var saved = localStorage.getItem('tv_speech_enabled');
            if (saved === 'false') {
                _speechEnabled = false;
                _updateSpeechUI();
            }
        } catch (e) { /* ignore */ }
    }

    function _toggleSpeech() {
        _speechEnabled = !_speechEnabled;
        _updateSpeechUI();
        try { localStorage.setItem('tv_speech_enabled', _speechEnabled); } catch (e) { /* ignore */ }
        if (!_speechEnabled) {
            window.speechSynthesis && window.speechSynthesis.cancel();
            _speechQueue = [];
            _isSpeaking = false;
        } else {
            _speak('Speech announcements enabled.');
        }
    }

    function _updateSpeechUI() {
        var btn = document.getElementById('tvSpeechToggle');
        var icon = document.getElementById('tvSpeechIcon');
        if (btn) btn.classList.toggle('muted', !_speechEnabled);
        if (icon) {
            icon.className = _speechEnabled ? 'bi bi-volume-up' : 'bi bi-volume-mute';
        }
    }

    function _speak(text) {
        if (!_speechEnabled || !window.speechSynthesis) return;
        _speechQueue.push(text);
        if (!_isSpeaking) _processQueue();
    }

    function _processQueue() {
        if (_speechQueue.length === 0) {
            _isSpeaking = false;
            _setSpeakingIndicator(false);
            return;
        }
        _isSpeaking = true;
        _setSpeakingIndicator(true);
        var text = _speechQueue.shift();
        var utter = new SpeechSynthesisUtterance(text);
        utter.lang = 'en-IN';
        utter.rate = 0.95;
        utter.pitch = 1.0;
        utter.volume = 1.0;
        // Pick a good voice if available
        var voices = window.speechSynthesis.getVoices();
        var preferred = voices.find(v => v.lang === 'en-IN' && v.name.toLowerCase().includes('female'))
                     || voices.find(v => v.lang === 'en-IN')
                     || voices.find(v => v.lang.startsWith('en') && v.name.toLowerCase().includes('female'))
                     || voices.find(v => v.lang.startsWith('en'));
        if (preferred) utter.voice = preferred;
        utter.onend = function() { setTimeout(_processQueue, 400); };
        utter.onerror = function() { setTimeout(_processQueue, 200); };
        window.speechSynthesis.speak(utter);
    }

    function _setSpeakingIndicator(active) {
        var ind = document.getElementById('tvSpeechIndicator');
        if (ind) ind.classList.toggle('speaking', active);
    }

    // ── Toast Notifications ──
    function _showToast(message, category, iconClass, severity) {
        var container = document.getElementById('tvSpeechToasts');
        if (!container) return;
        var toast = document.createElement('div');
        toast.className = 'tv-speech-toast sev-' + (severity || 'info');
        toast.innerHTML = '<i class="bi ' + _esc(iconClass || 'bi-bell') + ' tv-speech-toast-icon"></i>'
            + '<div class="tv-speech-toast-body">'
            + '<div class="tv-speech-toast-title">' + _esc(category || 'UPDATE') + '</div>'
            + '<div class="tv-speech-toast-msg">' + _esc(message) + '</div>'
            + '</div>';
        container.appendChild(toast);
        // Auto remove
        setTimeout(function() {
            toast.classList.add('toast-out');
            setTimeout(function() { toast.remove(); }, 500);
        }, TOAST_DURATION);
        // Limit visible toasts
        while (container.children.length > 5) {
            container.removeChild(container.firstChild);
        }
    }

    // ── Helpers ──
    function _setText(id, val) {
        const el = document.getElementById(id);
        if (el) el.textContent = val != null ? val : '–';
    }

    function _esc(s) {
        if (s == null) return '';
        const d = document.createElement('div');
        d.textContent = String(s);
        return d.innerHTML;
    }

    function _num(n) {
        if (n == null) return '—';
        return Number(n).toLocaleString('en-IN');
    }

    function _fmtDate(d) {
        if (!d) return '—';
        try {
            const dt = new Date(d);
            return dt.toLocaleDateString('en-IN', { day: '2-digit', month: 'short' });
        } catch { return '—'; }
    }

    function _fmtDateTime(d) {
        if (!d) return '—';
        try {
            const dt = new Date(d);
            return dt.toLocaleString('en-IN', { day: '2-digit', month: 'short', hour: '2-digit', minute: '2-digit', hour12: true });
        } catch { return '—'; }
    }

    // ── Public ──
    return { init };
})();

$(HelpdeskTV.init);
