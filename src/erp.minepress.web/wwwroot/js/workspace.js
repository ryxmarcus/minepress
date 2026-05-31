/* ═══════════════════════════════════════════════════════════════
   WORKSPACE MODULE — JavaScript
   ═══════════════════════════════════════════════════════════════ */
const Workspace = (() => {
    const API = '/api/workspace';
    let _currentTaskFilter = 'pending';
    let _currentApprovalFilter = 'pending';
    let _currentPriority = '';
    let _currentSearch = '';
    let _calDate = new Date();
    let _notifPage = 1;
    let _historyPage = 1;
    let _historyDays = 30;

    /* ──────── HELPERS ──────── */
    const $id = id => document.getElementById(id);
    const fetchJson = async (url) => {
        const r = await fetch(url);
        if (!r.ok) {
            const raw = await r.text();
            let msg = '';
            if (raw) {
                try {
                    const parsed = JSON.parse(raw);
                    msg = parsed?.message || parsed?.error || parsed?.title || '';
                } catch {
                    msg = raw;
                }
            }
            throw new Error(msg || `HTTP ${r.status}`);
        }
        return r.json();
    };
    const postJson = async (url, body = {}) => {
        const r = await fetch(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(body)
        });
        if (!r.ok) {
            const raw = await r.text();
            let msg = '';
            if (raw) {
                try {
                    const parsed = JSON.parse(raw);
                    msg = parsed?.message || parsed?.error || parsed?.title || '';
                } catch {
                    msg = raw;
                }
            }
            throw new Error(msg || `HTTP ${r.status}`);
        }
        return r.json();
    };
    const postNoBody = async (url) => {
        const r = await fetch(url, { method: 'POST' });
        if (!r.ok) {
            const raw = await r.text();
            let msg = '';
            if (raw) {
                try {
                    const parsed = JSON.parse(raw);
                    msg = parsed?.message || parsed?.error || parsed?.title || '';
                } catch {
                    msg = raw;
                }
            }
            throw new Error(msg || `HTTP ${r.status}`);
        }
        return r.json();
    };

    const timeAgo = (dateStr) => {
        if (!dateStr) return '';
        const d = new Date(dateStr);
        const now = new Date();
        const secs = Math.floor((now - d) / 1000);
        if (secs < 60) return 'just now';
        if (secs < 3600) return `${Math.floor(secs / 60)}m ago`;
        if (secs < 86400) return `${Math.floor(secs / 3600)}h ago`;
        if (secs < 604800) return `${Math.floor(secs / 86400)}d ago`;
        return d.toLocaleDateString();
    };

    const formatDate = (dateStr) => {
        if (!dateStr) return '—';
        return new Date(dateStr).toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' });
    };

    const formatDateTime = (dateStr) => {
        if (!dateStr) return '—';
        return new Date(dateStr).toLocaleString('en-IN', { day: '2-digit', month: 'short', hour: '2-digit', minute: '2-digit' });
    };

    const priorityBadge = (p) => {
        const cls = `ws-priority-${(p || 'normal').toLowerCase()}`;
        return `<span class="badge ${cls}">${p || 'NORMAL'}</span>`;
    };

    const statusBadge = (s) => {
        const cls = `ws-status-${(s || 'pending').toLowerCase()}`;
        return `<span class="badge ${cls}">${(s || 'PENDING').replace('_', ' ')}</span>`;
    };

    const taskIcon = (type) => {
        const map = {
            'TASK': 'task', 'APPROVAL': 'approval',
            'FOLLOW_UP': 'follow-up', 'REVIEW': 'review'
        };
        const iconMap = {
            'TASK': 'bi-list-task', 'APPROVAL': 'bi-shield-check',
            'FOLLOW_UP': 'bi-arrow-repeat', 'REVIEW': 'bi-eye'
        };
        return `<div class="ws-task-icon ${map[type] || 'task'}"><i class="bi ${iconMap[type] || 'bi-list-task'}"></i></div>`;
    };

    const slaBar = (slaHours, assignedOn) => {
        if (!slaHours || !assignedOn) return '';
        const start = new Date(assignedOn);
        const now = new Date();
        const elapsed = (now - start) / 3600000;
        const pct = Math.min(100, Math.round((elapsed / slaHours) * 100));
        const cls = pct < 60 ? 'green' : pct < 85 ? 'yellow' : 'red';
        return `<span class="ws-sla-bar"><span class="ws-sla-fill ${cls}" style="width:${pct}%"></span></span> <small class="text-secondary">${pct}%</small>`;
    };

    const emptyState = (icon, msg) =>
        `<div class="ws-empty"><i class="bi ${icon}"></i><div class="text-secondary">${msg}</div></div>`;

    const esc = (value) => (value || '')
        .toString()
        .replaceAll('&', '&amp;')
        .replaceAll('<', '&lt;')
        .replaceAll('>', '&gt;')
        .replaceAll('"', '&quot;')
        .replaceAll("'", '&#39;');

    function taskActionHint(t) {
        const status = (t.taskStatus || '').toUpperCase();
        const processCode = (t.processCode || '').toUpperCase();
        const processName = t.processName || t.title || 'this step';
        const ref = t.jobNo || t.partyName || '';
        const refText = ref ? ` (${ref})` : '';
        const isApproval = (t.taskType || '').toUpperCase() === 'APPROVAL' || processCode.includes('APPR');

        if (status === 'PENDING' && isApproval) {
            return `Approval pending: review ${processName}${refText} and approve or reject with remark.`;
        }

        if (status === 'PENDING') {
            return `Task pending: open ${processName}${refText}, complete required checks, then mark complete.`;
        }

        if (status === 'IN_PROGRESS' && isApproval) {
            return `Approval in progress: finalize decision for ${processName}${refText} and submit remarks.`;
        }

        if (status === 'IN_PROGRESS') {
            return `In progress: continue ${processName}${refText} and update work note before completion.`;
        }

        if (status === 'REJECTED') {
            return `Rejected step: review remarks, correct ${processName}${refText}, and resubmit.`;
        }

        if (status === 'COMPLETED' || status === 'APPROVED') {
            return `${isApproval ? 'Approval' : 'Task'} completed for ${processName}${refText}.`;
        }

        return `Open ${processName}${refText} to view the next required action.`;
    }

    /* ──────── PROCESS FLOW HELPERS ──────── */
    const _processFlowCache = {};

    async function fetchProcessFlow(taskId) {
        if (_processFlowCache[taskId]) return _processFlowCache[taskId];
        try {
            const data = await fetchJson(`${API}/process-flow/${taskId}`);
            _processFlowCache[taskId] = data;
            return data;
        } catch { return null; }
    }

    async function fetchJobProcessFlow(jobId) {
        const key = `job_${jobId}`;
        if (_processFlowCache[key]) return _processFlowCache[key];
        try {
            const data = await fetchJson(`${API}/process-flow/job/${jobId}`);
            _processFlowCache[key] = data;
            return data;
        } catch { return null; }
    }

    // ── Modern V2 Pipeline Rendering ──
    function renderPipelineV2(flow) {
        if (!flow || (!flow.previous && !flow.current && !flow.next)) return '';

        const steps = [
            {
                slot: 'Previous',
                state: flow.previous ? 'completed' : 'muted',
                icon: 'bi-check-lg',
                name: flow.previous?.title || 'No previous step',
                dept: flow.previous?.department || ''
            },
            {
                slot: 'Current',
                state: flow.current ? 'active' : 'muted',
                icon: flow.current?.taskType === 'APPROVAL' ? 'bi-shield-check' : 'bi-play-fill',
                name: flow.current?.title || 'Not active yet',
                dept: flow.current?.department || ''
            },
            {
                slot: 'Next',
                state: flow.next ? 'waiting' : 'muted',
                icon: 'bi-arrow-right',
                name: flow.next?.label || 'No next step',
                dept: flow.next?.department || ''
            }
        ];

        const progress = flow.totalSteps > 0 ? Math.round((flow.completedSteps / flow.totalSteps) * 100) : 0;

        const stepsHtml = steps.map((s, i) => `
            <div class="ws-pipeline-step ${s.state}">
                <div class="ws-pipeline-step-title">${s.slot}</div>
                <div class="ws-pipeline-circle">
                    <i class="bi ${s.icon}"></i>
                </div>
                <div class="ws-pipeline-info">
                    <div class="ws-pipeline-name">${esc(s.name)}</div>
                    ${s.dept ? `<div class="ws-pipeline-dept">${esc(s.dept)}</div>` : ''}
                </div>
            </div>
        `).join('');

        return `
            <div class="ws-pipeline-inline">
                <div class="ws-pipeline-header">
                    <span class="ws-pipeline-label"><i class="bi bi-diagram-3 me-1"></i>Workflow Progress</span>
                    ${flow.totalSteps > 0 ? `<span class="ws-pipeline-progress-text">${flow.completedSteps}/${flow.totalSteps} completed</span>` : ''}
                </div>
                <div class="ws-pipeline-track">
                    ${stepsHtml}
                </div>
            </div>
        `;
    }

    function renderProcessFlowStrip(flow) {
        if (!flow || !flow.previous && !flow.current && !flow.next) return '';
        const prevHtml = flow.previous
            ? `<div class="ws-flow-step completed">
                   <i class="bi bi-check-circle-fill"></i>
                   <div class="ws-flow-step-info">
                       <span class="ws-flow-step-label">${flow.previous.title || 'Previous Step'}</span>
                       <span class="ws-flow-step-user">${flow.previous.assignedTo || ''} ${flow.previous.department ? `(${flow.previous.department})` : ''}</span>
                   </div>
               </div>`
            : `<div class="ws-flow-step not-started"><i class="bi bi-circle"></i><div class="ws-flow-step-info"><span class="ws-flow-step-label text-secondary">—</span></div></div>`;

        const currHtml = flow.current
            ? `<div class="ws-flow-step active">
                   <i class="bi bi-${flow.current.taskType === 'APPROVAL' ? 'shield-check' : 'play-circle-fill'}"></i>
                   <div class="ws-flow-step-info">
                       <span class="ws-flow-step-label">${flow.current.title || 'Current Step'}</span>
                       <span class="ws-flow-step-user">${flow.current.assignedTo || ''} ${flow.current.department ? `(${flow.current.department})` : ''}</span>
                       ${flow.current.isOverdue ? '<span class="badge bg-danger" style="font-size:.6rem;">OVERDUE</span>' : ''}
                   </div>
               </div>`
            : `<div class="ws-flow-step not-started"><i class="bi bi-circle"></i><div class="ws-flow-step-info"><span class="ws-flow-step-label text-secondary">—</span></div></div>`;

        const nextHtml = flow.next
            ? `<div class="ws-flow-step upcoming">
                   <i class="bi bi-arrow-right-circle"></i>
                   <div class="ws-flow-step-info">
                       <span class="ws-flow-step-label">${flow.next.label || 'Next Step'}</span>
                       <span class="ws-flow-step-user">${flow.next.department || ''}</span>
                   </div>
               </div>`
            : `<div class="ws-flow-step not-started"><i class="bi bi-circle"></i><div class="ws-flow-step-info"><span class="ws-flow-step-label text-secondary">—</span></div></div>`;

        const progress = flow.totalSteps > 0 ? Math.round((flow.completedSteps / flow.totalSteps) * 100) : 0;

        return `<div class="ws-process-flow-strip">
            <div class="ws-flow-steps">
                ${prevHtml}
                <div class="ws-flow-connector"></div>
                ${currHtml}
                <div class="ws-flow-connector"></div>
                ${nextHtml}
            </div>
            ${flow.totalSteps > 0 ? `<div class="ws-flow-progress"><div class="ws-flow-progress-bar" style="width:${progress}%"></div><small class="text-secondary">${flow.completedSteps}/${flow.totalSteps} steps</small></div>` : ''}
        </div>`;
    }

    function renderFullProcessStepper(flowData) {
        if (!flowData || !flowData.steps || flowData.steps.length === 0)
            return '<div class="text-secondary text-center py-2">No process flow data available.</div>';

        const steps = flowData.steps;
        return `<div class="ws-process-stepper">
            ${steps.map((s, i) => {
                let cls = 'not-started';
                let icon = 'bi-circle';
                if (s.stepStatus === 'COMPLETED' || s.stepStatus === 'APPROVED') {
                    cls = 'completed'; icon = 'bi-check-circle-fill';
                } else if (s.isCurrent) {
                    cls = 'active'; icon = 'bi-play-circle-fill';
                } else if (s.stepStatus === 'REJECTED') {
                    cls = 'rejected'; icon = 'bi-x-circle-fill';
                } else if (s.stepStatus === 'IN_PROGRESS') {
                    cls = 'active'; icon = 'bi-arrow-repeat';
                }
                return `<div class="ws-stepper-step ${cls}">
                    <div class="ws-stepper-dot"><i class="bi ${icon}"></i></div>
                    <div class="ws-stepper-content">
                        <div class="ws-stepper-label">${s.eventLabel || 'Step ' + (i + 1)}</div>
                        <div class="ws-stepper-detail">
                            ${s.assignedUserName ? `<span><i class="bi bi-person me-1"></i>${s.assignedUserName}</span>` : ''}
                            ${s.departmentName ? `<span><i class="bi bi-building me-1"></i>${s.departmentName}</span>` : ''}
                            ${s.completedOn ? `<span><i class="bi bi-calendar-check me-1"></i>${s.completedOn}</span>` : ''}
                        </div>
                        <div class="ws-stepper-status">${statusBadge(s.stepStatus)}</div>
                    </div>
                    ${i < steps.length - 1 ? '<div class="ws-stepper-line"></div>' : ''}
                </div>`;
            }).join('')}
        </div>`;
    }

    /* ──────── INDEX PAGE ──────── */
    async function initIndex() {
        updateClock();
        setInterval(updateClock, 1000);
        await Promise.all([loadSummary(), loadAiSuggestions(), loadRecentTasks(), loadRecentApprovals()]);
    }

    function updateClock() {
        const el = $id('wsHeroTime');
        if (el) el.textContent = new Date().toLocaleTimeString('en-IN', { hour: '2-digit', minute: '2-digit' });
    }

    async function loadSummary() {
        try {
            const d = await fetchJson(`${API}/summary`);
            $id('statPending').textContent = d.pendingTasks ?? 0;
            $id('statInProgress').textContent = d.inProgressTasks ?? 0;
            $id('statCompleted').textContent = d.completedTasks ?? 0;
            $id('statOverdue').textContent = d.overdueTasks ?? 0;
            $id('statApprovals').textContent = d.pendingApprovals ?? 0;
            $id('statAssigned').textContent = d.assignedByMe ?? 0;
            $id('statTodayDue').textContent = d.todayDue ?? 0;
            $id('statTotal').textContent = d.totalActive ?? 0;
        } catch (e) {
            console.error('Failed to load summary:', e);
        }
    }

    async function loadAiSuggestions() {
        const el = $id('wsAiSuggestions');
        if (!el) return;
        try {
            const items = await fetchJson(`${API}/ai-suggestions`);
            if (!items || items.length === 0) {
                el.innerHTML = emptyState('bi-cpu', 'No suggestions at this time. Everything looks good!');
                return;
            }
            el.innerHTML = items.map(s => {
                const iconCls = s.type === 'overdue' ? 'overdue' : s.type === 'priority' ? 'priority' : s.type === 'success' ? 'success' : 'insight';
                const icon = s.type === 'overdue' ? 'bi-exclamation-triangle' : s.type === 'priority' ? 'bi-lightning-charge' : s.type === 'success' ? 'bi-check-circle' : 'bi-lightbulb';
                return `<div class="ws-ai-item">
                    <div class="ws-ai-icon ${iconCls}"><i class="bi ${icon}"></i></div>
                    <div class="ws-ai-text">
                        <div class="fw-medium">${s.title || ''}</div>
                        <div class="text-secondary">${s.message || ''}</div>
                    </div>
                    ${s.actionUrl ? `<a href="${s.actionUrl}" class="btn btn-sm btn-ghost-primary">View</a>` : ''}
                </div>`;
            }).join('');
        } catch (e) {
            el.innerHTML = emptyState('bi-cpu', 'Unable to load AI suggestions.');
        }
    }

    async function loadRecentTasks() {
        const el = $id('wsRecentTasks');
        if (!el) return;
        try {
            const items = await fetchJson(`${API}/tasks?filter=pending`);
            const tasks = (items || []).slice(0, 8);
            if (tasks.length === 0) {
                el.innerHTML = emptyState('bi-inbox', 'No pending tasks. You\'re all caught up!');
                return;
            }
            el.innerHTML = tasks.map(t => renderTaskRow(t, true)).join('');
        } catch (e) {
            el.innerHTML = emptyState('bi-exclamation-circle', 'Failed to load tasks.');
        }
    }

    async function loadRecentApprovals() {
        const el = $id('wsRecentApprovals');
        if (!el) return;
        try {
            const items = await fetchJson(`${API}/approvals?filter=pending`);
            const approvals = (items || []).slice(0, 6);
            if (approvals.length === 0) {
                el.innerHTML = emptyState('bi-shield', 'No pending approvals.');
                return;
            }
            el.innerHTML = approvals.map(a => `<div class="ws-approval-item">
                <div class="ws-task-icon approval"><i class="bi bi-shield-check"></i></div>
                <div class="ws-task-content">
                    <div class="ws-task-title">${a.title || 'Approval'}</div>
                    <div class="ws-task-meta">
                        <span>${a.jobNo || ''}</span>
                        <span>${a.partyName || ''}</span>
                        <span>${timeAgo(a.assignedOn)}</span>
                    </div>
                </div>
                <div class="ws-task-actions">
                    <button class="btn btn-sm btn-success" onclick="Workspace.approveTask(${a.taskId})" title="Approve">
                        <i class="bi bi-check-lg"></i>
                    </button>
                    <button class="btn btn-sm btn-danger" onclick="Workspace.rejectTask(${a.taskId})" title="Reject">
                        <i class="bi bi-x-lg"></i>
                    </button>
                </div>
            </div>`).join('');
        } catch (e) {
            el.innerHTML = emptyState('bi-exclamation-circle', 'Failed to load approvals.');
        }
    }

    function refresh() {
        loadSummary();
        loadAiSuggestions();
        loadRecentTasks();
        loadRecentApprovals();
    }

    /* ──────── MY TASKS PAGE ──────── */
    async function initMyTasks() {
        bindTaskFilters();
        await loadTaskCounts();
        await loadTasks();
    }

    function bindTaskFilters() {
        document.querySelectorAll('#taskFilterGroup .btn').forEach(btn => {
            btn.addEventListener('click', function () {
                document.querySelectorAll('#taskFilterGroup .btn').forEach(b => b.classList.remove('active'));
                this.classList.add('active');
                _currentTaskFilter = this.dataset.filter;
                loadTasks();
            });
        });

        const priorityEl = $id('taskPriorityFilter');
        if (priorityEl) priorityEl.addEventListener('change', () => {
            _currentPriority = priorityEl.value;
            loadTasks();
        });

        const searchEl = $id('taskSearchInput');
        if (searchEl) {
            let timer;
            searchEl.addEventListener('input', () => {
                clearTimeout(timer);
                timer = setTimeout(() => {
                    _currentSearch = searchEl.value;
                    loadTasks();
                }, 400);
            });
        }
    }

    async function loadTaskCounts() {
        try {
            const d = await fetchJson(`${API}/summary`);
            const setBadge = (id, val) => { const el = $id(id); if (el) el.textContent = val ?? 0; };
            setBadge('badgePending', d.pendingTasks);
            setBadge('badgeInProgress', d.inProgressTasks);
            setBadge('badgeCompleted', d.completedTasks);
            setBadge('badgeAssigned', d.assignedByMe);
            setBadge('badgeOverdue', d.overdueTasks);
        } catch { }
    }

    async function loadTasks() {
        const el = $id('wsTasksList');
        if (!el) return;
        el.innerHTML = '<div class="text-center py-4"><div class="spinner-border spinner-border-sm"></div></div>';
        try {
            let url = `${API}/tasks?filter=${_currentTaskFilter}`;
            if (_currentPriority) url += `&priority=${_currentPriority}`;
            if (_currentSearch) url += `&search=${encodeURIComponent(_currentSearch)}`;
            const items = await fetchJson(url);
            if (!items || items.length === 0) {
                el.innerHTML = emptyState('bi-inbox', 'No tasks found for this filter.');
                return;
            }
            el.innerHTML = items.map(t => renderTaskRow(t, false)).join('');
        } catch (e) {
            el.innerHTML = emptyState('bi-exclamation-circle', 'Failed to load tasks.');
        }
    }

    function renderTaskRow(t, compact) {
        const isOverdue = t.isOverdue || (t.dueDateIso && new Date(t.dueDateIso) < new Date() && t.taskStatus !== 'COMPLETED');
        const actionHint = taskActionHint(t);
        const taskType = (t.taskType || 'TASK').toUpperCase();
        const iconClass = taskType === 'APPROVAL' ? 'approval' : 'task';
        const iconName = taskType === 'APPROVAL' ? 'bi-shield-check' : 'bi-list-task';

        // Priority/Status V2 badges
        const priority = (t.priority || 'NORMAL').toLowerCase();
        const status = (t.taskStatus || 'PENDING').toLowerCase().replace('_', '-');
        const statusLabel = (t.taskStatus || 'PENDING').replace('_', ' ');

        // Compact mode uses simpler layout (for index page)
        if (compact) {
            return `<div class="ws-task-item${!t.isRead ? ' unread' : ''}">
                ${taskIcon(t.taskType)}
                <div class="ws-task-content">
                    <div class="ws-task-title">${esc(t.title || 'Task')} ${isOverdue ? '<i class="bi bi-exclamation-triangle text-danger ms-1"></i>' : ''}</div>
                    <div class="ws-task-meta">
                        ${t.jobNo ? `<span><i class="bi bi-briefcase me-1"></i>${t.jobNo}</span>` : ''}
                        <span>${timeAgo(t.assignedOn)}</span>
                    </div>
                </div>
                <div class="d-flex align-items-center gap-2">
                    ${priorityBadge(t.priority)}
                    ${statusBadge(t.taskStatus)}
                </div>
                <div class="ws-task-actions">
                    <button class="btn btn-sm btn-primary" onclick="Workspace.openTaskWork(${t.taskId}, '${esc(t.processCode || '')}')" title="Open"><i class="bi bi-box-arrow-up-right"></i></button>
                </div>
            </div>`;
        }

        // Full modern card V2 layout
        let actions = '';
        if (t.taskStatus === 'PENDING') {
            actions = `<button class="btn btn-primary" onclick="Workspace.openTaskWork(${t.taskId}, '${esc(t.processCode || '')}')" title="Start Work"><i class="bi bi-play-fill"></i></button>`;
        } else if (t.taskStatus === 'IN_PROGRESS') {
            actions = `<button class="btn btn-success" onclick="Workspace.openTaskWork(${t.taskId}, '${esc(t.processCode || '')}')" title="Continue Work"><i class="bi bi-arrow-up-right"></i></button>`;
        }
        actions += `<button class="btn btn-outline-secondary" onclick="Workspace.viewTask(${t.taskId})" title="View Details"><i class="bi bi-eye"></i></button>`;

        const html = `<div class="ws-task-card-v2" data-task-id="${t.taskId}">
            <div class="ws-card-header-v2">
                <div class="ws-card-icon-v2 ${iconClass}">
                    <i class="bi ${iconName}"></i>
                </div>
                <div class="ws-card-info-v2">
                    <div class="ws-card-process-name">${esc(t.title || t.processName || 'Task')}</div>
                    <div class="ws-card-job-ref">
                        ${t.jobNo ? `<span class="job-badge"><i class="bi bi-briefcase-fill me-1"></i>${esc(t.jobNo)}</span>` : ''}
                        ${t.partyName ? `<span><i class="bi bi-person me-1"></i>${esc(t.partyName)}</span>` : ''}
                        ${t.workflowName ? `<span class="step-badge">Step ${t.taskSequence || '?'} of ${t.workflowName}</span>` : ''}
                    </div>
                </div>
                <div class="ws-badge-group">
                    <span class="ws-priority-v2 ${priority}">${(t.priority || 'NORMAL').toUpperCase()}</span>
                    <span class="ws-status-v2 ${status}">${statusLabel}</span>
                </div>
                <div class="ws-card-actions-v2">
                    ${actions}
                </div>
            </div>
            <div class="ws-card-body-v2">
                <div class="ws-card-action-hint">
                    <i class="bi bi-lightbulb-fill"></i>
                    <div class="ws-card-action-hint-text">${esc(actionHint)}</div>
                </div>
                <div class="ws-card-meta-v2">
                    ${t.assignedOn ? `<span class="ws-card-meta-item"><i class="bi bi-clock"></i> Assigned ${timeAgo(t.assignedOn)}</span>` : ''}
                    ${t.dueDate ? `<span class="ws-card-meta-item"><i class="bi bi-calendar-event"></i> Due ${t.dueDate}</span>` : ''}
                    ${t.department ? `<span class="ws-card-meta-item"><i class="bi bi-building"></i> ${esc(t.department)}</span>` : ''}
                    ${isOverdue ? `<span class="ws-card-meta-item" style="color:#dc2626;"><i class="bi bi-exclamation-triangle-fill"></i> Overdue</span>` : ''}
                </div>
            </div>
            <div id="taskPipeline_${t.taskId}" class="ws-pipeline-placeholder"></div>
        </div>`;

        // Async load pipeline for tasks with jobId
        if (t.jobId) {
            setTimeout(async () => {
                const el = $id(`taskPipeline_${t.taskId}`);
                if (!el) return;
                const flow = await fetchJobProcessFlow(t.jobId);
                if (flow) {
                    el.innerHTML = renderPipelineV2(flow);
                } else {
                    el.remove();
                }
            }, 50);
        }

        return html;
    }

    function openTaskWork(taskId, processCode) {
        const code = (processCode || '').toUpperCase();
        if (code === 'DES_DTP' || code === 'PRE_DES') {
            window.location.href = `/Workspace/DesignWork?taskId=${taskId}`;
        } else if (code === 'PRE_PRESS' || code === 'PRE_CTP' || code.includes('CTP') || code.includes('PLATE')) {
            window.location.href = `/Workspace/PlateMaking?taskId=${taskId}`;
        } else if (code === 'PRINT') {
            window.location.href = `/Workspace/PrintWork?taskId=${taskId}`;
        } else {
            window.location.href = `/Workspace/ProcessWork?taskId=${taskId}`;
        }
    }

    function refreshTasks() {
        loadTaskCounts();
        loadTasks();
    }

    /* ──────── APPROVALS PAGE ──────── */
    async function initApprovals() {
        bindApprovalFilters();
        await loadApprovalCounts();
        await loadApprovalsList();
    }

    function bindApprovalFilters() {
        document.querySelectorAll('#approvalFilterGroup .btn').forEach(btn => {
            btn.addEventListener('click', function () {
                document.querySelectorAll('#approvalFilterGroup .btn').forEach(b => b.classList.remove('active'));
                this.classList.add('active');
                _currentApprovalFilter = this.dataset.filter;
                loadApprovalsList();
            });
        });

        const searchEl = $id('approvalSearchInput');
        if (searchEl) {
            let timer;
            searchEl.addEventListener('input', () => {
                clearTimeout(timer);
                timer = setTimeout(() => {
                    _currentSearch = searchEl.value;
                    loadApprovalsList();
                }, 400);
            });
        }
    }

    async function loadApprovalCounts() {
        try {
            const pending = await fetchJson(`${API}/approvals?filter=pending`);
            const approved = await fetchJson(`${API}/approvals?filter=approved`);
            const rejected = await fetchJson(`${API}/approvals?filter=rejected`);
            const set = (id, arr) => { const el = $id(id); if (el) el.textContent = (arr || []).length; };
            set('approvalBadgePending', pending);
            set('approvalBadgeApproved', approved);
            set('approvalBadgeRejected', rejected);
        } catch { }
    }

    async function loadApprovalsList() {
        const el = $id('wsApprovalsList');
        if (!el) return;
        el.innerHTML = '<div class="text-center py-4"><div class="spinner-border spinner-border-sm"></div></div>';
        try {
            let url = `${API}/approvals?filter=${_currentApprovalFilter}`;
            if (_currentSearch) url += `&search=${encodeURIComponent(_currentSearch)}`;
            const items = await fetchJson(url);
            if (!items || items.length === 0) {
                el.innerHTML = emptyState('bi-shield', 'No approvals found for this filter.');
                return;
            }
            el.innerHTML = items.map(a => {
                const isPending = a.taskStatus === 'PENDING';
                const approvalHint = taskActionHint(a);
                const priority = (a.priority || 'NORMAL').toLowerCase();
                const status = (a.taskStatus || 'PENDING').toLowerCase().replace('_', '-');
                const statusLabel = (a.taskStatus || 'PENDING').replace('_', ' ');

                // Modern V2 card for approvals
                let actions = '';
                if (isPending) {
                    actions = `
                        <button class="btn btn-success" onclick="Workspace.showApprovalModal(${a.taskId}, 'approve')" title="Approve">
                            <i class="bi bi-check-lg"></i>
                        </button>
                        <button class="btn btn-danger" onclick="Workspace.showApprovalModal(${a.taskId}, 'reject')" title="Reject">
                            <i class="bi bi-x-lg"></i>
                        </button>
                    `;
                }
                actions += `<button class="btn btn-outline-secondary" onclick="Workspace.viewTask(${a.taskId})" title="View Details"><i class="bi bi-eye"></i></button>`;

                return `<div class="ws-task-card-v2" data-task-id="${a.taskId}">
                    <div class="ws-card-header-v2">
                        <div class="ws-card-icon-v2 approval">
                            <i class="bi bi-shield-check"></i>
                        </div>
                        <div class="ws-card-info-v2">
                            <div class="ws-card-process-name">${esc(a.title || a.processName || 'Approval Required')}</div>
                            <div class="ws-card-job-ref">
                                ${a.jobNo ? `<span class="job-badge"><i class="bi bi-briefcase-fill me-1"></i>${esc(a.jobNo)}</span>` : ''}
                                ${a.partyName ? `<span><i class="bi bi-person me-1"></i>${esc(a.partyName)}</span>` : ''}
                                ${a.workflowName ? `<span class="step-badge">Step ${a.taskSequence || '?'} of ${a.workflowName}</span>` : ''}
                            </div>
                        </div>
                        <div class="ws-badge-group">
                            <span class="ws-priority-v2 ${priority}">${(a.priority || 'NORMAL').toUpperCase()}</span>
                            <span class="ws-status-v2 ${status}">${statusLabel}</span>
                        </div>
                        <div class="ws-card-actions-v2">
                            ${actions}
                        </div>
                    </div>
                    <div class="ws-card-body-v2">
                        <div class="ws-card-action-hint">
                            <i class="bi bi-lightbulb-fill"></i>
                            <div class="ws-card-action-hint-text">${esc(approvalHint)}</div>
                        </div>
                        <div class="ws-card-meta-v2">
                            ${a.assignedOn ? `<span class="ws-card-meta-item"><i class="bi bi-clock"></i> Assigned ${timeAgo(a.assignedOn)}</span>` : ''}
                            ${a.dueDate ? `<span class="ws-card-meta-item"><i class="bi bi-calendar-event"></i> Due ${a.dueDate}</span>` : ''}
                            ${a.department ? `<span class="ws-card-meta-item"><i class="bi bi-building"></i> ${esc(a.department)}</span>` : ''}
                            ${a.assignedBy ? `<span class="ws-card-meta-item"><i class="bi bi-person-check"></i> From ${esc(a.assignedBy)}</span>` : ''}
                        </div>
                    </div>
                    <div id="approvalPipeline_${a.taskId}" class="ws-pipeline-placeholder"></div>
                </div>`;
            }).join('');

            // Async load pipeline for each approval
            items.forEach(a => {
                if (a.jobId) {
                    setTimeout(async () => {
                        const pipelineEl = $id(`approvalPipeline_${a.taskId}`);
                        if (!pipelineEl) return;
                        const flow = await fetchJobProcessFlow(a.jobId);
                        if (flow) {
                            pipelineEl.innerHTML = renderPipelineV2(flow);
                        } else {
                            pipelineEl.remove();
                        }
                    }, 50);
                }
            });
        } catch (e) {
            el.innerHTML = emptyState('bi-exclamation-circle', 'Failed to load approvals.');
        }
    }

    function refreshApprovals() {
        loadApprovalCounts();
        loadApprovalsList();
    }

    /* ──────── CALENDAR PAGE ──────── */
    let _calView = 'month'; // 'month' or 'day'

    async function initCalendar() {
        // Check URL for view param
        const params = new URLSearchParams(window.location.search);
        if (params.get('view') === 'daily' || params.get('view') === 'day') {
            _calView = 'day';
        }
        await renderCalendarView();
    }

    function calSetView(view) {
        _calView = view;
        document.querySelectorAll('#calViewGroup .btn').forEach(b => b.classList.remove('active'));
        const activeBtn = document.querySelector(`#calViewGroup [data-view="${view}"]`);
        if (activeBtn) activeBtn.classList.add('active');
        renderCalendarView();
    }

    async function renderCalendarView() {
        const monthContainer = $id('calMonthContainer');
        const dayContainer = $id('calDayContainer');
        if (_calView === 'day') {
            if (monthContainer) monthContainer.style.display = 'none';
            if (dayContainer) dayContainer.style.display = '';
            await renderDayView();
        } else {
            if (monthContainer) monthContainer.style.display = '';
            if (dayContainer) dayContainer.style.display = 'none';
            await renderCalendar();
        }
    }

    async function renderCalendar() {
        const grid = $id('wsCalendarGrid');
        const title = $id('calMonthTitle');
        if (!grid) return;

        const year = _calDate.getFullYear();
        const month = _calDate.getMonth();
        if (title) title.textContent = _calDate.toLocaleDateString('en-IN', { month: 'long', year: 'numeric' });

        const firstDay = new Date(year, month, 1);
        const lastDay = new Date(year, month + 1, 0);
        const startOffset = firstDay.getDay();

        // Fetch events for the month
        const start = new Date(year, month, 1 - startOffset).toISOString();
        const end = new Date(year, month + 1, 7).toISOString();
        let events = [];
        try {
            events = await fetchJson(`${API}/calendar?start=${start}&end=${end}`);
        } catch { }

        const dayNames = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
        let html = dayNames.map(d => `<div class="ws-cal-header">${d}</div>`).join('');

        const today = new Date();
        const todayStr = `${today.getFullYear()}-${String(today.getMonth() + 1).padStart(2, '0')}-${String(today.getDate()).padStart(2, '0')}`;

        for (let i = 0; i < 42; i++) {
            const d = new Date(year, month, 1 - startOffset + i);
            const dayStr = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
            const isOther = d.getMonth() !== month;
            const isToday = dayStr === todayStr;

            const dayEvents = (events || []).filter(e => {
                const eDate = e.date ? e.date.substring(0, 10) : (e.start ? e.start.substring(0, 10) : '');
                return eDate === dayStr;
            });

            const eventsHtml = dayEvents.slice(0, 3).map(e =>
                `<div class="ws-cal-event" style="background:${e.color || '#0054a6'}" title="${e.title || ''}">${e.title || ''}</div>`
            ).join('');
            const moreHtml = dayEvents.length > 3 ? `<div class="text-secondary" style="font-size:.65rem;">+${dayEvents.length - 3} more</div>` : '';
            const countBadge = dayEvents.length > 0 ? `<span class="ws-cal-count">${dayEvents.length}</span>` : '';

            html += `<div class="ws-cal-day${isToday ? ' today' : ''}${isOther ? ' other-month' : ''}${dayEvents.length > 0 ? ' has-events' : ''}" data-date="${dayStr}" onclick="Workspace.calDrillDown('${dayStr}')">
                <div class="ws-cal-day-num">${d.getDate()} ${countBadge}</div>
                ${eventsHtml}${moreHtml}
            </div>`;

            if (i >= 34 && d.getMonth() !== month) break;
        }

        grid.innerHTML = html;
    }

    async function renderDayView() {
        const timeline = $id('wsDayTimeline');
        const titleEl = $id('calDayTitle');
        const summaryEl = $id('calDaySummary');
        if (!timeline) return;

        timeline.innerHTML = '<div class="text-center py-4"><div class="spinner-border spinner-border-sm"></div></div>';

        const dateStr = `${_calDate.getFullYear()}-${String(_calDate.getMonth() + 1).padStart(2, '0')}-${String(_calDate.getDate()).padStart(2, '0')}`;

        try {
            const data = await fetchJson(`${API}/calendar/day?date=${dateStr}`);
            const { summary, tasks } = data;

            // Title
            if (titleEl) {
                titleEl.innerHTML = `<i class="bi bi-calendar-day me-2"></i>${summary.dateFormatted}${summary.isToday ? ' <span class="badge bg-primary ms-2">Today</span>' : ''}`;
            }

            // Summary badges
            if (summaryEl) {
                summaryEl.innerHTML = `
                    <span class="badge bg-primary-lt"><i class="bi bi-list-task me-1"></i>${summary.total} Total</span>
                    <span class="badge bg-warning-lt"><i class="bi bi-hourglass-split me-1"></i>${summary.pending} Pending</span>
                    <span class="badge bg-cyan-lt"><i class="bi bi-play-circle me-1"></i>${summary.inProgress} In Progress</span>
                    <span class="badge bg-success-lt"><i class="bi bi-check-circle me-1"></i>${summary.completed} Done</span>
                    ${summary.overdue > 0 ? `<span class="badge bg-danger-lt"><i class="bi bi-exclamation-triangle me-1"></i>${summary.overdue} Overdue</span>` : ''}
                    ${summary.approvals > 0 ? `<span class="badge bg-purple-lt"><i class="bi bi-shield-check me-1"></i>${summary.approvals} Approvals</span>` : ''}`;
            }

            if (!tasks || tasks.length === 0) {
                timeline.innerHTML = emptyState('bi-calendar-x', 'No tasks scheduled for this day.');
                return;
            }

            // Build hourly timeline (6 AM to 11 PM, plus overflow)
            const START_HOUR = 6;
            const END_HOUR = 23;

            // Group tasks by hour
            const hourBuckets = {};
            const earlyTasks = [];
            tasks.forEach(t => {
                const hr = t.dueHour;
                if (hr < START_HOUR) {
                    earlyTasks.push(t);
                } else {
                    if (!hourBuckets[hr]) hourBuckets[hr] = [];
                    hourBuckets[hr].push(t);
                }
            });

            let html = '';

            // Early tasks (before 6 AM)
            if (earlyTasks.length > 0) {
                html += renderDayHourSlot('Early', 'Before 6:00 AM', earlyTasks);
            }

            // Hourly slots
            for (let h = START_HOUR; h <= END_HOUR; h++) {
                const hourLabel = h === 0 ? '12 AM' : h < 12 ? `${h} AM` : h === 12 ? '12 PM' : `${h - 12} PM`;
                const hourTasks = hourBuckets[h] || [];
                html += renderDayHourSlot(hourLabel, null, hourTasks, h);
            }

            timeline.innerHTML = html;

        } catch (e) {
            timeline.innerHTML = emptyState('bi-exclamation-circle', 'Failed to load day view.');
        }
    }

    function renderDayHourSlot(label, subLabel, tasks, hour) {
        const now = new Date();
        const isCurrentHour = hour !== undefined &&
            _calDate.toDateString() === now.toDateString() &&
            now.getHours() === hour;

        const taskCards = tasks.map(t => {
            const typeIcon = t.taskType === 'APPROVAL' ? 'bi-shield-check' : 'bi-list-task';
            const overdueTag = t.isOverdue ? '<i class="bi bi-exclamation-triangle text-danger ms-1" title="Overdue"></i>' : '';
            let actions = '';
            if (t.taskStatus === 'PENDING') {
                actions = `<button class="btn btn-sm btn-primary py-0 px-1" onclick="event.stopPropagation();Workspace.startTask(${t.taskId})" title="Start"><i class="bi bi-play-fill"></i></button>`;
            } else if (t.taskStatus === 'IN_PROGRESS') {
                actions = `<button class="btn btn-sm btn-success py-0 px-1" onclick="event.stopPropagation();Workspace.completeTask(${t.taskId})" title="Complete"><i class="bi bi-check-lg"></i></button>`;
            }
            if (t.taskType === 'APPROVAL' && t.taskStatus === 'PENDING') {
                actions = `<button class="btn btn-sm btn-success py-0 px-1" onclick="event.stopPropagation();Workspace.approveTask(${t.taskId})" title="Approve"><i class="bi bi-check-lg"></i></button>
                           <button class="btn btn-sm btn-danger py-0 px-1" onclick="event.stopPropagation();Workspace.rejectTask(${t.taskId})" title="Reject"><i class="bi bi-x-lg"></i></button>`;
            }

            return `<div class="ws-day-event" style="border-left-color:${t.color}" onclick="Workspace.viewTask(${t.taskId})">
                <div class="ws-day-event-header">
                    <span class="ws-day-event-time"><i class="bi bi-clock me-1"></i>${t.dueTime} — ${t.endTime}</span>
                    ${priorityBadge(t.priority)}
                    ${statusBadge(t.taskStatus)}
                </div>
                <div class="ws-day-event-title">
                    <i class="bi ${typeIcon} me-1"></i>${t.title || 'Task'} ${overdueTag}
                </div>
                <div class="ws-day-event-meta">
                    ${t.jobNo ? `<span><i class="bi bi-briefcase me-1"></i>${t.jobNo}</span>` : ''}
                    ${t.partyName ? `<span><i class="bi bi-person me-1"></i>${t.partyName}</span>` : ''}
                    ${t.departmentName ? `<span><i class="bi bi-building me-1"></i>${t.departmentName}</span>` : ''}
                    ${t.processName ? `<span><i class="bi bi-diagram-3 me-1"></i>${t.processName}</span>` : ''}
                    ${t.slaHours ? `<span><i class="bi bi-stopwatch me-1"></i>${t.slaHours}h SLA</span>` : ''}
                </div>
                ${actions ? `<div class="ws-day-event-actions">${actions}</div>` : ''}
            </div>`;
        }).join('');

        return `<div class="ws-day-hour${isCurrentHour ? ' current-hour' : ''}${tasks.length > 0 ? ' has-events' : ''}">
            <div class="ws-day-hour-label">
                <span class="ws-day-hour-time">${label}</span>
                ${subLabel ? `<span class="ws-day-hour-sub">${subLabel}</span>` : ''}
            </div>
            <div class="ws-day-hour-content">
                ${taskCards || '<div class="ws-day-hour-empty"></div>'}
            </div>
        </div>`;
    }

    function calDrillDown(dateStr) {
        const parts = dateStr.split('-');
        _calDate = new Date(parseInt(parts[0]), parseInt(parts[1]) - 1, parseInt(parts[2]));
        calSetView('day');
    }

    function calNav(dir) {
        if (dir === 0) {
            _calDate = new Date();
        } else if (_calView === 'day') {
            _calDate.setDate(_calDate.getDate() + dir);
        } else {
            _calDate.setMonth(_calDate.getMonth() + dir);
        }
        renderCalendarView();
    }

    /* ──────── NOTIFICATIONS PAGE ──────── */
    async function initNotifications() {
        await loadNotifications();
    }

    async function loadNotifications() {
        const el = $id('wsNotificationsList');
        if (!el) return;
        el.innerHTML = '<div class="text-center py-4"><div class="spinner-border spinner-border-sm"></div></div>';
        try {
            const data = await fetchJson(`${API}/notifications?page=${_notifPage}&pageSize=20`);
            const items = data.items || data || [];
            const total = data.total || items.length;

            const totalEl = $id('notifTotal');
            if (totalEl) totalEl.textContent = `${total} notification(s)`;

            if (items.length === 0) {
                el.innerHTML = emptyState('bi-bell-slash', 'No notifications yet.');
                return;
            }

            el.innerHTML = items.map(n => {
                const iconClass = n.icon || 'bi-bell';
                const colorClass = n.color || 'primary';
                return `<div class="ws-notif-item">
                    <div class="ws-notif-icon" style="background:var(--tblr-${colorClass}-lt, rgba(0,84,166,.1)); color:var(--tblr-${colorClass}, #0054a6);">
                        <i class="bi ${iconClass}"></i>
                    </div>
                    <div class="ws-notif-content">
                        <div class="ws-notif-title">${n.title || 'Notification'}</div>
                        <div class="text-secondary" style="font-size:.825rem;">${n.message || ''}</div>
                        <div class="ws-notif-time">${timeAgo(n.createdOn)}</div>
                    </div>
                    ${n.referenceUrl ? `<a href="${n.referenceUrl}" class="btn btn-sm btn-ghost-primary">View</a>` : ''}
                </div>`;
            }).join('');

            renderPagination('notifPagination', _notifPage, Math.ceil(total / 20), (p) => {
                _notifPage = p;
                loadNotifications();
            });
        } catch (e) {
            el.innerHTML = emptyState('bi-exclamation-circle', 'Failed to load notifications.');
        }
    }

    function refreshNotifications() { _notifPage = 1; loadNotifications(); }

    /* ──────── HISTORY PAGE ──────── */
    async function initHistory() {
        const daysEl = $id('historyDaysFilter');
        if (daysEl) daysEl.addEventListener('change', () => {
            _historyDays = parseInt(daysEl.value);
            _historyPage = 1;
            loadHistory();
        });
        await loadHistory();
    }

    async function loadHistory() {
        const el = $id('wsHistoryTimeline');
        if (!el) return;
        el.innerHTML = '<div class="text-center py-4"><div class="spinner-border spinner-border-sm"></div></div>';
        try {
            const data = await fetchJson(`${API}/history?page=${_historyPage}&pageSize=25&days=${_historyDays}`);
            const items = data.items || data || [];
            const total = data.total || items.length;

            const totalEl = $id('historyTotal');
            if (totalEl) totalEl.textContent = `${total} activity record(s)`;

            if (items.length === 0) {
                el.innerHTML = emptyState('bi-clock-history', 'No activity records found for this period.');
                return;
            }

            el.innerHTML = `<div class="ws-timeline">${items.map(h => `<div class="ws-timeline-item">
                <div class="ws-timeline-dot"></div>
                <div class="ws-timeline-title">${h.action || h.title || 'Activity'}</div>
                <div class="ws-timeline-desc">${h.description || ''}</div>
                <div class="ws-timeline-time">${formatDateTime(h.createdOn || h.timestamp)}</div>
            </div>`).join('')}</div>`;

            renderPagination('historyPagination', _historyPage, Math.ceil(total / 25), (p) => {
                _historyPage = p;
                loadHistory();
            });
        } catch (e) {
            el.innerHTML = emptyState('bi-exclamation-circle', 'Failed to load history.');
        }
    }

    function refreshHistory() { _historyPage = 1; loadHistory(); }

    function buildAutomationModalHtml(title, steps) {
        return `
            <div class="ws-auto-title">${esc(title)}</div>
            <div class="ws-auto-progress-shell">
                <div class="ws-auto-progress-fill" id="wsAutoProgressFill" style="width:0%"></div>
            </div>
            <div class="ws-auto-progress-text" id="wsAutoProgressText">Starting...</div>
            <div class="ws-auto-step-list">
                ${steps.map((s, i) => `
                    <div class="ws-auto-step" id="wsAutoStep_${i}">
                        <span class="ws-auto-step-icon"><i class="bi bi-circle"></i></span>
                        <span class="ws-auto-step-label">${esc(s.label)}</span>
                    </div>`).join('')}
            </div>`;
    }

    function setAutomationStepState(index, state, text) {
        const row = $id(`wsAutoStep_${index}`);
        if (!row) return;

        row.classList.remove('running', 'done', 'failed');
        row.classList.add(state);

        const icon = row.querySelector('.ws-auto-step-icon i');
        if (!icon) return;

        if (state === 'running') {
            icon.className = 'bi bi-arrow-repeat spin';
        } else if (state === 'done') {
            icon.className = 'bi bi-check-circle-fill';
        } else if (state === 'failed') {
            icon.className = 'bi bi-x-circle-fill';
        } else {
            icon.className = 'bi bi-circle';
        }

        if (text) {
            const label = row.querySelector('.ws-auto-step-label');
            if (label) label.textContent = text;
        }
    }

    function setAutomationProgress(done, total, text) {
        const fill = $id('wsAutoProgressFill');
        const txt = $id('wsAutoProgressText');
        const pct = total > 0 ? Math.round((done / total) * 100) : 0;
        if (fill) fill.style.width = `${pct}%`;
        if (txt) txt.textContent = text || `${pct}% completed`;
    }

    function resolveAutomationScenario(taskDetail) {
        const t = taskDetail?.task || {};
        const processCode = (t.processCode || '').toUpperCase();
        const title = (t.title || '').toUpperCase();
        const sourceTable = (t.sourceTable || '').toLowerCase();

        // Skip automation if source is already a job (manual jobs don't need conversion)
        if (sourceTable === 'trn_job') {
            return null;
        }

        const isQuotationGen = (processCode.includes('QUOT') || title.includes('QUOTATION GENERATION')) && sourceTable === 'trn_enquiry';
        if (isQuotationGen) {
            return {
                key: 'quotation-from-enquiry',
                title: 'Converting Quotation from Enquiry',
                sourceTable,
                sourceId: t.sourceId
            };
        }

        // Only trigger job creation automation if source is a quotation or enquiry (with linked quotation)
        const isJobCreate = (processCode.includes('JOB_CREATE') || processCode.includes('ENQ_JOB') || title.includes('JOB CREATION'));
        const hasValidSource = sourceTable === 'trn_quotation' || sourceTable === 'trn_enquiry';
        if (isJobCreate && hasValidSource) {
            return {
                key: 'job-from-quotation',
                title: 'Converting Job from Quotation',
                sourceTable,
                sourceId: t.sourceId
            };
        }

        return null;
    }

    function buildQuotationPayloadFromEnquiry(enquiryData) {
        const mappedItems = (enquiryData.items || []).map((item, idx) => {
            const qty = Number(item.quantity) || 0;
            const unitRate = Number(item.costPerUnit) || 0;
            const grossAmount = Number(item.grandTotal) || (unitRate * qty);
            const totalTaxAmount = Number(item.taxAmount) || 0;
            const netAmount = Number(item.netTotal) || (grossAmount + totalTaxAmount);

            return {
                enquiryItemId: item.enquiryItemId || null,
                itemSequence: idx + 1,
                productName: item.productName || '',
                productDescription: item.productDescription || '',
                productTypeName: item.productTypeName || '',
                jobTypeName: item.jobTypeName || '',
                productSizeName: item.productSizeName || '',
                noOfPages: item.noOfPages || 0,
                trimWidthMm: item.trimWidthMm || 0,
                trimHeightMm: item.trimHeightMm || 0,
                printingMethod: item.printingMethod || '',
                quantity: qty,
                unitRate,
                grossAmount,
                discountPercent: 0,
                discountAmount: 0,
                taxableValue: grossAmount,
                cgstPercent: 0,
                cgstAmount: 0,
                sgstPercent: 0,
                sgstAmount: 0,
                igstPercent: 0,
                igstAmount: 0,
                totalTaxAmount,
                netAmount,
                rateCalculatorId: item.rateCalculatorId || null,
                calcRefNo: item.calcRefNo || '',
                remarks: ''
            };
        });

        const totalAmount = mappedItems.reduce((s, x) => s + (Number(x.grossAmount) || 0), 0);
        const taxAmount = mappedItems.reduce((s, x) => s + (Number(x.totalTaxAmount) || 0), 0);
        const netAmount = mappedItems.reduce((s, x) => s + (Number(x.netAmount) || 0), 0);

        return {
            partyId: enquiryData.partyId,
            enquiryId: enquiryData.enquiryId,
            partyRefNo: enquiryData.partyRefNo || '',
            partyRefDate: enquiryData.partyRefDateIso || '',
            validTill: enquiryData.validTillIso || '',
            totalAmount,
            discountAmount: 0,
            taxableAmount: totalAmount,
            taxAmount,
            netAmount,
            termsConditions: enquiryData.termsConditions || '',
            remarks: enquiryData.remarks || '',
            items: mappedItems
        };
    }

    function buildJobPayloadFromQuotation(quotationData) {
        const items = quotationData.items || [];
        const grossAmount = items.reduce((sum, i) => sum + (Number(i.grossAmount) || 0), 0);
        const discountAmount = items.reduce((sum, i) => sum + (Number(i.discountAmount) || 0), 0);
        const taxableAmount = items.reduce((sum, i) => sum + (Number(i.taxableValue) || 0), 0);
        const taxAmount = items.reduce((sum, i) => sum + (Number(i.totalTaxAmount) || 0), 0);
        const netAmount = items.reduce((sum, i) => sum + (Number(i.netAmount) || 0), 0);
        const quantity = items.reduce((sum, i) => sum + (Number(i.quantity) || 0), 0);

        return {
            partyId: quotationData.partyId,
            enquiryId: quotationData.enquiryId || null,
            quotationId: quotationData.quotationId,
            partyRefNo: quotationData.partyRefNo || '',
            partyRefNoDate: quotationData.partyRefDateIso || '',
            deliveryDate: quotationData.deliveryDateIso || '',
            productName: items[0]?.productName || quotationData.productName || 'Converted Job',
            productDescription: items[0]?.productDescription || quotationData.remarks || '',
            quantity: quantity || 1,
            totalPages: items.reduce((sum, i) => sum + (Number(i.noOfPages) || 0), 0) || null,
            priority: quotationData.priority || 'NORMAL',
            estimatedCost: quotationData.netAmount || netAmount,
            quotedAmount: quotationData.netAmount || netAmount,
            grossAmount,
            discountAmount,
            taxableAmount,
            taxAmount,
            netAmount: quotationData.netAmount || netAmount,
            remarks: quotationData.remarks || '',
            items: items.map((item, idx) => ({
                enquiryItemId: item.enquiryItemId || null,
                quotationItemId: item.quotationItemId || null,
                itemSequence: idx + 1,
                productName: item.productName || '',
                productDescription: item.productDescription || '',
                productTypeName: item.productTypeName || '',
                jobTypeName: item.jobTypeName || '',
                productSizeName: item.productSizeName || '',
                noOfPages: item.noOfPages || 0,
                trimWidthMm: item.trimWidthMm || 0,
                trimHeightMm: item.trimHeightMm || 0,
                printingMethod: item.printingMethod || '',
                printProductTypeId: item.printProductTypeId || null,
                jobTypeId: item.jobTypeId || null,
                quantity: item.quantity || 0,
                unitRate: item.unitRate || 0,
                grossAmount: item.grossAmount || 0,
                discountPercent: item.discountPercent || 0,
                discountAmount: item.discountAmount || 0,
                taxableValue: item.taxableValue || 0,
                cgstPercent: item.cgstPercent || 0,
                cgstAmount: item.cgstAmount || 0,
                sgstPercent: item.sgstPercent || 0,
                sgstAmount: item.sgstAmount || 0,
                igstPercent: item.igstPercent || 0,
                igstAmount: item.igstAmount || 0,
                totalTaxAmount: item.totalTaxAmount || 0,
                netAmount: item.netAmount || 0,
                rateCalculatorId: item.rateCalculatorId || null,
                calcRefNo: item.calcRefNo || '',
                hsnSacCodeId: item.hsnSacCodeId || null,
                remarks: ''
            }))
        };
    }

    async function executeQuotationFromEnquiryAutomation(enquiryId) {
        const enquiryData = await fetchJson(`/api/quotation/from-enquiry/${enquiryId}`);
        const payload = buildQuotationPayloadFromEnquiry(enquiryData);

        const quotationResult = await postJson('/api/quotation/save', payload);

        try {
            await postNoBody(`/api/quotation/send-email/${quotationResult.quotationId}`);
        } catch {
            // best effort
        }

        return quotationResult;
    }

    async function executeJobFromQuotationAutomation(quotationId) {
        const quotationData = await fetchJson(`/api/job/from-quotation/${quotationId}`);
        const payload = buildJobPayloadFromQuotation(quotationData);

        const jobResult = await postJson('/api/job/save', payload);

        try {
            await postNoBody(`/api/job/send-email/${jobResult.jobId}`);
        } catch {
            // best effort
        }

        return jobResult;
    }

    async function runPostApprovalAutomation(taskId) {
        const detail = await fetchJson(`${API}/task/${taskId}/process-detail`);
        const scenario = resolveAutomationScenario(detail);
        if (!scenario) return null;

        const steps = scenario.key === 'quotation-from-enquiry'
            ? [
                { label: 'Validating Enquiry' },
                { label: 'Generating Quotation' },
                { label: 'Sending Email' },
                { label: 'Updating Timeline' },
                { label: 'Completed' }
            ]
            : [
                { label: 'Validating Quotation' },
                { label: 'Creating Job' },
                { label: 'Sending Email' },
                { label: 'Updating Timeline' },
                { label: 'Completed' }
            ];

        Swal.fire({
            title: 'Background Execution',
            html: buildAutomationModalHtml(scenario.title, steps),
            showConfirmButton: false,
            allowOutsideClick: false,
            allowEscapeKey: false,
            customClass: { popup: 'ws-auto-popup' }
        });

        await new Promise(r => setTimeout(r, 30));

        const total = steps.length;
        let done = 0;
        let result;

        try {
            setAutomationStepState(0, 'running');
            setAutomationProgress(done, total, 'Preparing conversion...');

            if (scenario.key === 'quotation-from-enquiry') {
                const enquiryData = await fetchJson(`/api/quotation/from-enquiry/${scenario.sourceId}`);
                const payload = buildQuotationPayloadFromEnquiry(enquiryData);
                setAutomationStepState(0, 'done');
                done++;
                setAutomationProgress(done, total, 'Generating quotation...');

                setAutomationStepState(1, 'running');
                result = await postJson('/api/quotation/save', payload);
                setAutomationStepState(1, 'done');
                done++;

                setAutomationProgress(done, total, 'Sending email to customer...');
                setAutomationStepState(2, 'running');
                try {
                    await postNoBody(`/api/quotation/send-email/${result.quotationId}`);
                } catch {
                    // best effort
                }
                setAutomationStepState(2, 'done');
                done++;

                setAutomationProgress(done, total, 'Syncing timelines and activity...');
                setAutomationStepState(3, 'running');
                setAutomationStepState(3, 'done');
                done++;
            } else {
                let quotationId = scenario.sourceId;
                if ((scenario.sourceTable || '').toLowerCase() === 'trn_enquiry') {
                    const q = await fetchJson(`${API}/resolve-quotation/${scenario.sourceId}`);
                    quotationId = q.quotationId;
                }

                const quotationData = await fetchJson(`/api/job/from-quotation/${quotationId}`);
                const payload = buildJobPayloadFromQuotation(quotationData);
                setAutomationStepState(0, 'done');
                done++;
                setAutomationProgress(done, total, 'Creating job...');

                setAutomationStepState(1, 'running');
                result = await postJson('/api/job/save', payload);
                setAutomationStepState(1, 'done');
                done++;

                setAutomationProgress(done, total, 'Sending email to customer...');
                setAutomationStepState(2, 'running');
                try {
                    await postNoBody(`/api/job/send-email/${result.jobId}`);
                } catch {
                    // best effort
                }
                setAutomationStepState(2, 'done');
                done++;

                setAutomationProgress(done, total, 'Syncing timelines and activity...');
                setAutomationStepState(3, 'running');
                setAutomationStepState(3, 'done');
                done++;
            }

            setAutomationStepState(4, 'running');
            setAutomationStepState(4, 'done');
            done++;

            setAutomationProgress(done, total, 'Execution completed successfully');
            await new Promise(r => setTimeout(r, 550));
            Swal.close();

            return {
                scenario: scenario.key,
                result,
                message: scenario.key === 'quotation-from-enquiry'
                    ? `Quotation ${result?.quotationNo || ''} created and customer email triggered.`
                    : `Job ${result?.jobNo || ''} created and customer email triggered.`
            };
        } catch (err) {
            const nextIndex = Math.min(done, total - 1);
            setAutomationStepState(nextIndex, 'failed');
            const errorText = err?.message || 'Execution failed.';
            setAutomationProgress(done, total, `Execution failed: ${errorText}`);
            await new Promise(r => setTimeout(r, 120));
            Swal.update({
                showConfirmButton: true,
                confirmButtonText: 'Close',
                allowOutsideClick: true,
                allowEscapeKey: true
            });
            throw new Error(errorText);
        }
    }

    /* ──────── TASK ACTIONS ──────── */
    async function startTask(id) {
        const startConfirm = await Swal.fire({
            title: 'Start Task?',
            html: 'Do you really want to mark this task as <strong>Started</strong>?',
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: '<i class="bi bi-play-fill me-1"></i>Yes, Start',
            cancelButtonText: 'Cancel',
            customClass: {
                confirmButton: 'btn btn-primary px-4 me-2',
                cancelButton: 'btn btn-secondary px-4'
            },
            buttonsStyling: false,
            reverseButtons: true
        });

        if (!startConfirm.isConfirmed) return;

        try {
            await postJson(`${API}/task/${id}/start`, { remarks: '' });
            Swal.fire({ icon: 'success', title: 'Task Started', text: 'Task has been moved to In Progress.', timer: 1500, showConfirmButton: false });
            setTimeout(() => location.reload(), 1600);
        } catch (e) {
            Swal.fire({ icon: 'error', title: 'Error', text: e?.message || 'Failed to start task.' });
        }
    }

    async function completeTask(id) {
        const completeConfirm = await Swal.fire({
            title: 'Complete Task?',
            html: 'Do you really want to mark this task as <strong>Completed</strong>?',
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: '<i class="bi bi-check-lg me-1"></i>Yes, Complete',
            cancelButtonText: 'Cancel',
            customClass: {
                confirmButton: 'btn btn-success px-4 me-2',
                cancelButton: 'btn btn-secondary px-4'
            },
            buttonsStyling: false,
            reverseButtons: true
        });

        if (!completeConfirm.isConfirmed) return;

        const remarkModal = await Swal.fire({
            title: 'Completion Remarks',
            input: 'textarea',
            inputLabel: 'Remarks (optional)',
            inputPlaceholder: 'Enter completion remarks...',
            showCancelButton: true,
            confirmButtonText: 'Submit & Complete',
            cancelButtonText: 'Cancel',
            customClass: {
                confirmButton: 'btn btn-success px-4 me-2',
                cancelButton: 'btn btn-secondary px-4'
            },
            buttonsStyling: false,
            reverseButtons: true
        });

        if (!remarkModal.isConfirmed) return;

        const remarks = remarkModal.value || '';

        try {
            await postJson(`${API}/task/${id}/complete`, { remarks });
            Swal.fire({ icon: 'success', title: 'Task Completed', timer: 1500, showConfirmButton: false });
            setTimeout(() => location.reload(), 1600);
        } catch (e) {
            Swal.fire({ icon: 'error', title: 'Error', text: 'Failed to complete task.' });
        }
    }

    async function approveTask(id) {
        try {
            await postJson(`${API}/approval/${id}/approve`, { remarks: '' });
            let automation = null;
            let automationFailed = false;
            try {
                automation = await runPostApprovalAutomation(id);
            } catch {
                automationFailed = true;
            }

            Swal.fire({
                icon: automationFailed ? 'warning' : 'success',
                title: 'Approved',
                text: automationFailed
                    ? 'Approved. Background conversion failed; please run conversion manually from details page.'
                    : (automation?.message || 'Approval completed successfully.'),
                timer: 2200,
                showConfirmButton: false
            });
            setTimeout(() => location.reload(), 2300);
        } catch (e) {
            Swal.fire({ icon: 'error', title: 'Error', text: 'Failed to approve.' });
        }
    }

    async function rejectTask(id) {
        const { value: remarks } = await Swal.fire({
            title: 'Reject Approval',
            input: 'textarea',
            inputLabel: 'Rejection Reason',
            inputPlaceholder: 'Enter reason for rejection...',
            showCancelButton: true,
            confirmButtonText: 'Reject',
            confirmButtonColor: '#d63939',
            inputValidator: v => !v && 'Please provide a reason for rejection.'
        });
        if (!remarks) return;
        try {
            await postJson(`${API}/approval/${id}/reject`, { remarks });
            Swal.fire({ icon: 'success', title: 'Rejected', timer: 1500, showConfirmButton: false });
            setTimeout(() => location.reload(), 1600);
        } catch (e) {
            Swal.fire({ icon: 'error', title: 'Error', text: 'Failed to reject.' });
        }
    }

    let _approvalActionId = null;
    let _approvalActionType = null;
    let _approvalProgressPlan = [];

    function getApprovalProgressSteps(action) {
        return action === 'approve'
            ? ['Validate request', 'Update approval status', 'Run post-approval automation', 'Finalize workflow']
            : ['Validate request', 'Update rejection status', 'Finalize workflow'];
    }

    function initApprovalProgress(action) {
        _approvalProgressPlan = getApprovalProgressSteps(action);
        const wrap = $id('approvalProgressWrap');
        const stepsEl = $id('approvalProgressSteps');
        const msgEl = $id('approvalProgressMsg');
        const fill = $id('approvalProgressFill');
        const pctEl = $id('approvalProgressPct');
        if (!wrap || !stepsEl || !msgEl || !fill || !pctEl) return;

        wrap.classList.remove('d-none');
        stepsEl.innerHTML = _approvalProgressPlan.map((s, i) =>
            `<div class="ws-approval-step pending" data-step="${i}"><i class="bi bi-circle"></i><span>${esc(s)}</span></div>`
        ).join('');
        msgEl.textContent = 'Starting...';
        fill.style.width = '4%';
        pctEl.textContent = '0%';
    }

    function setApprovalProgress(stepIndex, state, message) {
        const stepsEl = $id('approvalProgressSteps');
        const msgEl = $id('approvalProgressMsg');
        const fill = $id('approvalProgressFill');
        const pctEl = $id('approvalProgressPct');
        if (!stepsEl || !msgEl || !fill || !pctEl) return;

        const nodes = Array.from(stepsEl.querySelectorAll('.ws-approval-step'));
        if (stepIndex >= 0 && stepIndex < nodes.length) {
            const node = nodes[stepIndex];
            node.classList.remove('pending', 'running', 'done', 'failed');
            node.classList.add(state);
            const icon = node.querySelector('i');
            if (icon) {
                icon.className = state === 'done'
                    ? 'bi bi-check-circle-fill'
                    : state === 'running'
                        ? 'bi bi-arrow-repeat'
                        : state === 'failed'
                            ? 'bi bi-x-circle-fill'
                            : 'bi bi-circle';
            }
        }

        const doneCount = nodes.filter(n => n.classList.contains('done')).length;
        const total = Math.max(1, nodes.length);
        let pct = Math.round((doneCount / total) * 100);
        if (nodes.some(n => n.classList.contains('running'))) pct = Math.max(pct, 10);
        if (nodes.some(n => n.classList.contains('failed'))) pct = Math.max(pct, 75);
        fill.style.width = `${Math.min(100, Math.max(4, pct))}%`;
        pctEl.textContent = `${pct}%`;
        msgEl.textContent = message || 'Processing...';
    }

    function showApprovalModal(id, action) {
        _approvalActionId = id;
        _approvalActionType = action;
        const titleEl = $id('approvalModalTitle');
        const infoEl = $id('approvalModalInfo');
        const btnEl = $id('approvalActionBtn');
        const remarksEl = $id('approvalRemarks');
        const progressWrap = $id('approvalProgressWrap');

        if (titleEl) titleEl.textContent = action === 'approve' ? 'Approve Request' : 'Reject Request';
        if (infoEl) infoEl.innerHTML = action === 'approve'
            ? '<div class="alert alert-success"><i class="bi bi-check-circle me-2"></i>You are about to approve this request.</div>'
            : '<div class="alert alert-danger"><i class="bi bi-x-circle me-2"></i>You are about to reject this request.</div>';
        if (btnEl) {
            btnEl.className = action === 'approve' ? 'btn btn-success' : 'btn btn-danger';
            btnEl.textContent = action === 'approve' ? 'Approve' : 'Reject';
            btnEl.disabled = false;
        }
        if (remarksEl) {
            remarksEl.value = '';
            remarksEl.disabled = false;
        }
        if (progressWrap) progressWrap.classList.add('d-none');

        const modal = new bootstrap.Modal($id('approvalActionModal'));
        modal.show();
    }

    async function submitApprovalAction() {
        const remarksEl = $id('approvalRemarks');
        const actionBtn = $id('approvalActionBtn');
        const remarks = (remarksEl?.value || '').trim();
        if (_approvalActionType === 'reject' && !remarks) {
            Swal.fire({ icon: 'warning', title: 'Required', text: 'Please provide remarks for rejection.' });
            return;
        }

        initApprovalProgress(_approvalActionType || 'approve');
        setApprovalProgress(0, 'running', 'Validating request...');
        if (actionBtn) {
            actionBtn.disabled = true;
            actionBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Processing...';
        }
        if (remarksEl) remarksEl.disabled = true;

        try {
            const endpoint = _approvalActionType === 'approve' ? 'approve' : 'reject';
            setApprovalProgress(0, 'done', 'Validation complete');
            setApprovalProgress(1, 'running', _approvalActionType === 'approve' ? 'Updating approval status...' : 'Updating rejection status...');
            await postJson(`${API}/approval/${_approvalActionId}/${endpoint}`, { remarks });
            setApprovalProgress(1, 'done', 'Status updated successfully');

            let automationMessage = _approvalActionType === 'approve'
                ? 'Approval completed successfully.'
                : 'Request rejected successfully.';
            let automationFailed = false;
            if (_approvalActionType === 'approve') {
                setApprovalProgress(2, 'running', 'Running post-approval automation...');
                try {
                    const automation = await runPostApprovalAutomation(_approvalActionId);
                    automationMessage = automation?.message || '';
                    setApprovalProgress(2, 'done', 'Automation completed');
                } catch (automationError) {
                    // Close any automation popup that may still be open
                    Swal.close();
                    automationFailed = true;
                    automationMessage = automationError?.message || 'Approved, but post-approval conversion failed.';
                    setApprovalProgress(2, 'failed', `Automation failed: ${automationMessage}`);
                }

                if (!automationFailed) {
                    setApprovalProgress(3, 'running', 'Finalizing workflow...');
                    setApprovalProgress(3, 'done', 'Workflow finalized');
                }
            } else {
                setApprovalProgress(2, 'running', 'Finalizing workflow...');
                setApprovalProgress(2, 'done', 'Workflow finalized');
            }

            if (automationFailed) {
                if (actionBtn) {
                    actionBtn.disabled = true;
                    actionBtn.innerHTML = 'Submit';
                }
                if (remarksEl) remarksEl.disabled = false;
                // Error is already displayed in the approval progress steps - no need for a second popup
                return;
            }

            bootstrap.Modal.getInstance($id('approvalActionModal'))?.hide();
            Swal.fire({
                icon: 'success',
                title: _approvalActionType === 'approve' ? 'Approved' : 'Rejected',
                text: automationMessage,
                timer: 1800,
                showConfirmButton: false
            });
            setTimeout(() => location.reload(), 1900);
        } catch (e) {
            const errorText = e?.message || `Failed to ${_approvalActionType}.`;
            setApprovalProgress(1, 'failed', `Action failed: ${errorText}`);
            if (actionBtn) {
                actionBtn.disabled = false;
                actionBtn.innerHTML = _approvalActionType === 'approve' ? 'Approve' : 'Reject';
            }
            if (remarksEl) remarksEl.disabled = false;
            Swal.fire({ icon: 'error', title: 'Error', text: errorText });
        }
    }

    async function viewTask(id) {
        const body = $id('taskDetailBody');
        const footer = $id('taskDetailFooter');
        if (body) body.innerHTML = '<div class="text-center py-4"><div class="spinner-border"></div></div>';
        if (footer) footer.innerHTML = '';

        const modalEl = $id('taskDetailModal');
        if (!modalEl) { console.warn('taskDetailModal not found'); return; }
        const modal = new bootstrap.Modal(modalEl);
        modal.show();

        // Mark as read
        try { await postJson(`${API}/task/${id}/read`); } catch { }

        try {
            const tasks = await fetchJson(`${API}/tasks?filter=pending`);
            const all = [...(await fetchJson(`${API}/tasks?filter=in_progress`) || []), ...(tasks || []), ...(await fetchJson(`${API}/tasks?filter=completed`) || [])];
            const t = all.find(x => x.taskId === id);

            if (!t) {
                body.innerHTML = '<div class="text-secondary text-center">Task not found.</div>';
                return;
            }

            // Fetch process flow for the full stepper
            let processFlowHtml = '<div id="taskDetailProcessFlow"><small class="text-secondary"><i class="bi bi-arrow-repeat spin me-1"></i>Loading process flow...</small></div>';

            body.innerHTML = `
                <div class="row g-3">
                    <div class="col-12">
                        <h4>${t.title || 'Task'}</h4>
                        <p class="text-secondary">${t.description || ''}</p>
                        <div class="alert alert-primary-lt py-2 px-3 mb-0">
                            <div class="small fw-semibold mb-1"><i class="bi bi-lightbulb me-1"></i>What to do now</div>
                            <div class="small">${esc(taskActionHint(t))}</div>
                        </div>
                    </div>
                    <div class="col-12">
                        <label class="form-label text-secondary fw-semibold"><i class="bi bi-signpost-split me-1"></i>Process Flow</label>
                        ${processFlowHtml}
                    </div>
                    <div class="col-6">
                        <label class="form-label text-secondary">Status</label>
                        <div>${statusBadge(t.taskStatus)}</div>
                    </div>
                    <div class="col-6">
                        <label class="form-label text-secondary">Priority</label>
                        <div>${priorityBadge(t.priority)}</div>
                    </div>
                    <div class="col-6">
                        <label class="form-label text-secondary">Type</label>
                        <div>${t.taskType || '—'}</div>
                    </div>
                    <div class="col-6">
                        <label class="form-label text-secondary">Job No</label>
                        <div>${t.jobNo ? `<a href="/Job/Details?id=${t.jobId}">${t.jobNo}</a>` : '—'}</div>
                    </div>
                    <div class="col-6">
                        <label class="form-label text-secondary">Due Date</label>
                        <div>${t.dueDate || '—'}</div>
                    </div>
                    <div class="col-6">
                        <label class="form-label text-secondary">Assigned On</label>
                        <div>${t.assignedOn || '—'}</div>
                    </div>
                    <div class="col-6">
                        <label class="form-label text-secondary">SLA</label>
                        <div>${t.slaHours ? `${t.slaHours}h` : '—'} ${slaBar(t.slaHours, t.assignedOn)}</div>
                    </div>
                    <div class="col-6">
                        <label class="form-label text-secondary">Process</label>
                        <div>${t.processCode || '—'} / ${t.subprocessCode || '—'}</div>
                    </div>
                    <div class="col-6">
                        <label class="form-label text-secondary">Department</label>
                        <div>${t.departmentName || '—'}</div>
                    </div>
                </div>`;

            let footerHtml = '';
            if (t.actionUrl) footerHtml += `<a href="${t.actionUrl}" class="btn btn-primary"><i class="bi bi-eye me-1"></i>Open</a>`;
            if (t.taskStatus === 'PENDING') footerHtml += `<button class="btn btn-primary" onclick="Workspace.openTaskWork(${t.taskId}, '${(t.processCode||'')}');bootstrap.Modal.getInstance(document.getElementById('taskDetailModal')).hide()"><i class="bi bi-play-fill me-1"></i>Start</button>`;
            if (t.taskStatus === 'IN_PROGRESS') footerHtml += `<button class="btn btn-success" onclick="Workspace.openTaskWork(${t.taskId}, '${(t.processCode||'')}');bootstrap.Modal.getInstance(document.getElementById('taskDetailModal')).hide()"><i class="bi bi-box-arrow-up-right me-1"></i>Open Work</button>`;
            footer.innerHTML = footerHtml || '<button class="btn btn-secondary" data-bs-dismiss="modal">Close</button>';

            // Async load full process flow stepper
            const flowEl = $id('taskDetailProcessFlow');
            if (flowEl) {
                const flowData = await fetchProcessFlow(id);
                if (flowData && flowData.steps && flowData.steps.length > 0) {
                    flowEl.innerHTML = renderFullProcessStepper(flowData);
                } else {
                    flowEl.innerHTML = '<div class="text-secondary" style="font-size:.825rem;">No process flow configured for this task.</div>';
                }
            }
        } catch (e) {
            body.innerHTML = '<div class="text-secondary text-center">Failed to load task details.</div>';
        }
    }

    /* ──────── PAGINATION HELPER ──────── */
    function renderPagination(containerId, current, totalPages, onPage) {
        const el = $id(containerId);
        if (!el || totalPages <= 1) { if (el) el.innerHTML = ''; return; }
        let html = '<div class="btn-group ws-pagination">';
        html += `<button class="btn btn-sm btn-outline-primary" ${current <= 1 ? 'disabled' : ''} onclick="void(0)">‹</button>`;
        const start = Math.max(1, current - 2);
        const end = Math.min(totalPages, current + 2);
        for (let i = start; i <= end; i++) {
            html += `<button class="btn btn-sm ${i === current ? 'btn-primary' : 'btn-outline-primary'}" data-page="${i}">${i}</button>`;
        }
        html += `<button class="btn btn-sm btn-outline-primary" ${current >= totalPages ? 'disabled' : ''} onclick="void(0)">›</button>`;
        html += '</div>';
        el.innerHTML = html;
        el.querySelectorAll('[data-page]').forEach(btn => {
            btn.addEventListener('click', () => onPage(parseInt(btn.dataset.page)));
        });
        el.querySelector('button:first-child')?.addEventListener('click', () => { if (current > 1) onPage(current - 1); });
        el.querySelector('button:last-child')?.addEventListener('click', () => { if (current < totalPages) onPage(current + 1); });
    }

    /* ──────── PUBLIC API ──────── */
    return {
        initIndex,
        initMyTasks,
        initApprovals,
        initCalendar,
        initNotifications,
        initHistory,
        refresh,
        refreshTasks,
        refreshApprovals,
        refreshNotifications,
        refreshHistory,
        loadAiSuggestions,
        startTask,
        completeTask,
        approveTask,
        rejectTask,
        viewTask,
        showApprovalModal,
        submitApprovalAction,
        calNav,
        calSetView,
        calDrillDown,
        fetchProcessFlow,
        fetchJobProcessFlow,
        openTaskWork
    };
})();
