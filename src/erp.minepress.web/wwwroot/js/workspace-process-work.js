const WorkspaceProcessWork = (() => {
    let _taskId = 0;
    let _task = null;
    let _items = [];
    let _flow = null;
    let _detailData = null;

    const $ = (id) => document.getElementById(id);

    function getCsrfToken() {
        const el = document.querySelector('input[name="__RequestVerificationToken"]');
        return el ? el.value : '';
    }

    async function fetchJson(url, opts = {}) {
        opts.headers = opts.headers || {};
        const method = (opts.method || 'GET').toUpperCase();
        if (method !== 'GET') {
            const token = getCsrfToken();
            if (token) opts.headers.RequestVerificationToken = token;
        }


        const r = await fetch(url, opts);
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

        if (r.status === 204) return null;
        return r.json();
    }

    function esc(v) {
        return (v || '').toString()
            .replaceAll('&', '&amp;')
            .replaceAll('<', '&lt;')
            .replaceAll('>', '&gt;')
            .replaceAll('"', '&quot;')
            .replaceAll("'", '&#39;');
    }

    function badgeClass(status) {
        const s = (status || '').toUpperCase();
        if (s === 'COMPLETED' || s === 'APPROVED') return 'success';
        if (s === 'IN_PROGRESS') return 'info';
        if (s === 'REJECTED') return 'danger';
        return 'secondary';
    }

    function progressPercent(flow) {
        if (!flow || !flow.steps || !flow.steps.length) return 0;
        const done = flow.steps.filter(s => s.stepStatus === 'COMPLETED' || s.stepStatus === 'APPROVED').length;
        return Math.round((done / flow.steps.length) * 100);
    }

    function getNextFlowStep(flow) {
        if (!flow?.steps?.length || flow.currentIndex < 0 || flow.currentIndex + 1 >= flow.steps.length) return null;
        return flow.steps[flow.currentIndex + 1];
    }

    function renderSummary(data) {
        const t = data.task || {};
        $('pwTaskSummary').innerHTML = `
            <div class="pw-info-list">
                <div class="pw-info-row">
                    <i class="bi bi-diagram-3"></i>
                    <span class="pw-info-label">Process</span>
                    <span class="pw-info-value">${esc(t.processName || t.processCode || '—')}</span>
                </div>
                <div class="pw-info-row">
                    <i class="bi bi-building"></i>
                    <span class="pw-info-label">Department</span>
                    <span class="pw-info-value">${esc(t.departmentName || '—')}</span>
                </div>
                <div class="pw-info-row">
                    <i class="bi bi-flag"></i>
                    <span class="pw-info-label">Status</span>
                    <span class="badge bg-${badgeClass(t.taskStatus)}-lt">${esc(t.taskStatus || 'PENDING')}</span>
                </div>
                ${t.dueDate ? `<div class="pw-info-row">
                    <i class="bi bi-calendar"></i>
                    <span class="pw-info-label">Due Date</span>
                    <span class="pw-info-value">${esc(t.dueDate)}</span>
                </div>` : ''}
            </div>`;
    }

    function renderPipeline(flow) {
        const el = $('pwPipelineWrap');
        if (!el) return;

        if (!flow || !flow.steps || !flow.steps.length) {
            el.innerHTML = '<div class="text-secondary">Workflow pipeline not available for this task.</div>';
            return;
        }

        const pct = progressPercent(flow);
        const currentStep = flow.steps.find(s => s.isCurrent) || flow.steps[Math.max(0, flow.currentIndex || 0)] || null;
        const previousStep = flow.currentIndex > 0 ? flow.steps[flow.currentIndex - 1] : null;
        const nextStep = getNextFlowStep(flow);

        const laneCard = (title, icon, tone, step, emptyText) => `
            <div class="pw-lane-card ${tone}">
                <div class="pw-lane-title"><i class="bi ${icon} me-2"></i>${title}</div>
                ${step ? `
                    <div class="pw-lane-step">${esc(step.processName || step.eventLabel || step.processCode || 'Step')}</div>
                    <div class="pw-lane-meta">
                        ${step.assignedUserName ? `<span><i class="bi bi-person me-1"></i>${esc(step.assignedUserName)}</span>` : ''}
                        ${step.departmentName ? `<span><i class="bi bi-building me-1"></i>${esc(step.departmentName)}</span>` : ''}
                    </div>
                    <div class="pw-lane-status">${esc((step.stepStatus || 'NOT_STARTED').replaceAll('_', ' '))}</div>`
                : `<div class="pw-lane-empty">${emptyText}</div>`}
            </div>`;

        el.innerHTML = `
            <div class="pw-pipeline-shell">
                <div class="pw-pipeline-top">
                    <div>
                        <div class="pw-section-kicker">AI Workflow View</div>
                        <h4 class="mb-1">Job Workflow Pipeline</h4>
                        <div class="text-secondary small">Real-time visibility for previous, current, and next process steps.</div>
                    </div>
                    <div class="pw-pipeline-progress">
                        <div class="pw-pipeline-progress-value">${pct}%</div>
                        <div class="pw-pipeline-progress-label">${flow.completedSteps || 0}/${flow.totalSteps || flow.steps.length} steps complete</div>
                    </div>
                </div>
                <div class="pw-pipeline-bar">
                    <div class="pw-pipeline-fill" style="width:${pct}%"></div>
                </div>
                <div class="pw-pipeline-lanes">
                    ${laneCard('Previous', 'bi-check2-circle', 'done', previousStep, 'No completed previous step')}
                    ${laneCard('Current', currentStep?.taskType === 'APPROVAL' ? 'bi-shield-check' : 'bi-play-circle', 'active', currentStep, 'Current step not active yet')}
                    ${laneCard('Next', 'bi-arrow-right-circle', 'next', nextStep, 'No next step queued')}
                </div>
                <div class="pw-stage-track">
                    ${flow.steps.map((s, i) => {
            let cls = 'pending';
            let icon = 'bi-circle';
            if (s.stepStatus === 'COMPLETED' || s.stepStatus === 'APPROVED') {
                cls = 'done';
                icon = 'bi-check-circle-fill';
            } else if (s.isCurrent || s.stepStatus === 'IN_PROGRESS') {
                cls = 'active';
                icon = s.taskType === 'APPROVAL' ? 'bi-shield-check' : 'bi-play-circle-fill';
            } else if (s.stepStatus === 'REJECTED') {
                cls = 'risk';
                icon = 'bi-x-circle-fill';
            }

            return `<div class="pw-stage-node ${cls}">
                            <div class="pw-stage-dot"><i class="bi ${icon}"></i></div>
                            <div class="pw-stage-body">
                                <div class="pw-stage-name">${esc(s.processName || s.eventLabel || s.processCode || `Step ${i + 1}`)}</div>
                                <div class="pw-stage-sub">${esc((s.stepStatus || 'NOT_STARTED').replaceAll('_', ' '))}</div>
                            </div>
                        </div>`;
        }).join('')}
                </div>
            </div>`;
    }

    function buildInsights(data) {
        const t = data.task || {};
        const flow = _flow;
        const currentStep = flow?.steps?.find(s => s.isCurrent) || null;
        const nextStep = getNextFlowStep(flow);
        const selectedProcessInputs = document.querySelectorAll('#pwProcessInputs option:checked').length;
        const checked = document.querySelectorAll('.pw-check:checked').length + selectedProcessInputs;
        const totalChecks = document.querySelectorAll('.pw-check').length + document.querySelectorAll('#pwProcessInputs option').length;
        const selectedParts = document.querySelectorAll('.pw-part:checked').length;
        const items = [];

        items.push({
            tone: 'primary',
            icon: 'bi-robot',
            title: 'AI Recommendation',
            text: currentStep?.stepStatus === 'PENDING'
                ? `Start ${currentStep.processName || currentStep.eventLabel || 'this step'} and capture work notes early so the next department gets cleaner handoff data.`
                : `Keep updating notes and part selection so ${nextStep?.processName || 'the next step'} receives clear execution context.`
        });

        items.push({
            tone: t.isOverdue ? 'danger' : 'success',
            icon: t.isOverdue ? 'bi-exclamation-triangle' : 'bi-clock-history',
            title: t.isOverdue ? 'SLA Risk Detected' : 'Timeline Health',
            text: t.isOverdue
                ? `This work item is overdue. Complete the highest-impact checklist items first and add a concise delay note for traceability.`
                : `Current SLA looks controlled. Keep notes and checkpoints updated to avoid last-minute rework.`
        });

        items.push({
            tone: 'info',
            icon: 'bi-ui-checks-grid',
            title: 'Execution Coverage',
            text: totalChecks > 0
                ? `${checked}/${totalChecks} checkpoints selected and ${selectedParts}/${_items.length || 0} parts tagged for execution.`
                : 'No predefined checkpoints were found. Use work notes to capture critical verifications.'
        });

        if (nextStep) {
            items.push({
                tone: 'warning',
                icon: 'bi-signpost-split',
                title: 'Prepare Next Step',
                text: `Likely next workflow handoff is ${nextStep.processName || nextStep.eventLabel}${nextStep.departmentName ? ` for ${nextStep.departmentName}` : ''}. Record output quality, quantity, and exceptions before completion.`
            });
        }

        if (data.job?.deliveryDate) {
            items.push({
                tone: 'purple',
                icon: 'bi-truck',
                title: 'Delivery Awareness',
                text: `Job delivery target is ${data.job.deliveryDate}. Keep this process completion aligned with downstream finishing and dispatch timing.`
            });
        }

        return items.slice(0, 5);
    }

    function renderInsights(data) {
        const el = $('pwSmartInsights');
        if (!el) return;

        const insights = buildInsights(data);
        el.innerHTML = insights.map(i => `
            <div class="pw-ai-card ${i.tone}">
                <div class="pw-ai-icon"><i class="bi ${i.icon}"></i></div>
                <div class="pw-ai-body">
                    <div class="pw-ai-title">${esc(i.title)}</div>
                    <div class="pw-ai-text">${esc(i.text)}</div>
                </div>
            </div>`).join('');
    }

    async function renderChecklist(data) {
        const wrap = $('pwChecklistWrap');
        if (!wrap) return;

        const processInputs = await fetchJson(`/api/workspace/task/${_taskId}/process-input-options`).catch(() => null);
        if (processInputs?.options?.length) {
            wrap.innerHTML = `
                <div class="pw-process-input-block">
                    <div class="small text-secondary mb-2">${esc(processInputs.label || 'Process Inputs')} (Multiple Select)</div>
                    <select id="pwProcessInputs" class="form-select pw-process-input-select" multiple size="6">
                        ${processInputs.options.map(o => `<option value="${o.id}">${esc(o.name)}${o.code ? ` (${esc(o.code)})` : ''}</option>`).join('')}
                    </select>
                    <div class="mt-2 d-flex gap-2">
                        <button id="pwAddInputsToNotesBtn" class="btn btn-sm btn-outline-primary">Add selected to Notes</button>
                        <button id="pwClearProcessInputsBtn" class="btn btn-sm btn-outline-secondary">Clear Selection</button>
                        <div class="small text-secondary ms-auto">Hold Ctrl/Cmd to select multiple inputs.</div>
                    </div>
                </div>`;

            // Attach handlers for adding selected inputs into Work Notes and clearing selection
            const addBtn = wrap.querySelector('#pwAddInputsToNotesBtn');
            const clearBtn = wrap.querySelector('#pwClearProcessInputsBtn');
            const notesEl = document.getElementById('pwNotes');

            addBtn?.addEventListener('click', () => {
                const selected = Array.from(document.querySelectorAll('#pwProcessInputs option:checked'))
                    .map(o => o.textContent.trim())
                    .filter(t => t.length > 0);
                if (!selected.length) {
                    try { Swal.fire({ icon: 'warning', title: 'No selection', text: 'Please select one or more process inputs first.' }); } catch { alert('Please select one or more process inputs first.'); }
                    return;
                }

                const block = selected.map(s => `- ${s}`).join('\n');
                if (notesEl) {
                    if (!notesEl.value || notesEl.value.trim() === '') notesEl.value = block;
                    else notesEl.value = notesEl.value.trim() + '\n' + block;
                    // trigger input event for any listeners
                    notesEl.dispatchEvent(new Event('input', { bubbles: true }));
                }

                // clear selection after adding
                document.querySelectorAll('#pwProcessInputs option:checked').forEach(o => o.selected = false);
                renderInsights(_detailData || { task: _task });
            });

            clearBtn?.addEventListener('click', () => {
                document.querySelectorAll('#pwProcessInputs option:checked').forEach(o => o.selected = false);
            });

            return;
        }

        const process = (data.task?.processCode || '').toUpperCase();
        const checks = [];

        if (process === 'DES_DTP') {
            checks.push('Design content checked');
            checks.push('DTP layout finalized');
            checks.push('Fonts and dimensions verified');
        } else if (process === 'PRINT') {
            checks.push('Machine setup verified');
            checks.push('Print quality checked');
            checks.push('Output sent for next process by part');
        } else if (process.includes('QC')) {
            checks.push('QC checklist completed');
            checks.push('Non-conformance remarks recorded');
        } else {
            checks.push('Required process inputs verified');
            checks.push('Step execution notes captured');
        }

        wrap.innerHTML = checks.map((c, i) => `
            <label class="form-check mb-2 pw-check-item">
                <input class="form-check-input pw-check" type="checkbox" value="${i + 1}">
                <span class="form-check-label">${esc(c)}</span>
            </label>
        `).join('');
    }

    const PARALLEL_PROCESSES = ['DES_DTP', 'PRE_PRESS', 'POST_PRESS'];

    function itemBadgeClass(status) {
        const s = (status || '').toUpperCase();
        if (s === 'COMPLETED') return 'success';
        if (s === 'RUNNING') return 'info';
        if (s === 'CLOSED') return 'dark';
        return 'secondary';
    }

    function itemSpecLine(it) {
        if (!it?.workData) return '';
        try {
            const w = typeof it.workData === 'string' ? JSON.parse(it.workData) : it.workData;
            const spec = w?.specification;
            if (!spec) return '';

            const bits = [];
            if (spec.pages != null && spec.pages !== '') bits.push(`${esc(spec.pages)} pages`);
            if (spec.color != null && spec.color !== '') bits.push(`${esc(spec.color)} color`);
            if (spec.paper) bits.push(esc(spec.paper));
            if (!bits.length) return '';

            return `<div class="small text-secondary"><i class="bi bi-card-text me-1"></i>${bits.join(' · ')}</div>`;
        } catch {
            return '';
        }
    }

    async function renderParts(data) {
        const items = data.items || [];
        _items = items;
        const wrap = $('pwPartsWrap');
        const processCode = (_task?.processCode || '').toUpperCase();
        const isParallel = PARALLEL_PROCESSES.includes(processCode);

        if (!isParallel) {
            // Non-parallel: keep original checkbox display
            if (!items.length) {
                wrap.innerHTML = '<div class="text-secondary">No part-wise items found for this task.</div>';
                return;
            }
            wrap.innerHTML = items.map(i => `
                <label class="form-check pw-part-item mb-2">
                    <input class="form-check-input pw-part" type="checkbox" value="${i.jobItemId}">
                    <span class="form-check-label">
                        <strong>${esc(i.productName || `Part ${i.itemSequence}`)}</strong>
                        <div class="small text-secondary">${esc(i.productDescription || '')}</div>
                    </span>
                </label>
            `).join('');
            return;
        }

        // ── Parallel-eligible: fetch item tasks and render dashboard ──
        let itemTasks = [];
        try {
            itemTasks = await fetchJson(`/api/workspace/task/${_taskId}/item-tasks`);
        } catch { }

        // Auto-create if none exist yet
        if (itemTasks.length === 0 && items.length > 0) {
            try {
                await fetchJson(`/api/workspace/task/${_taskId}/create-item-tasks`, {
                    method: 'POST', headers: { 'Content-Type': 'application/json' }, body: '{}'
                });
                itemTasks = await fetchJson(`/api/workspace/task/${_taskId}/item-tasks`);
            } catch { }
        }

        if (!itemTasks.length) {
            wrap.innerHTML = '<div class="text-secondary">No item tasks available.</div>';
            return;
        }

        const total = itemTasks.length;
        const completed = itemTasks.filter(t => t.taskStatus === 'COMPLETED').length;
        const running = itemTasks.filter(t => t.taskStatus === 'RUNNING').length;
        const pct = total ? Math.round((completed / total) * 100) : 0;

        wrap.innerHTML = `
            <div class="mb-3">
                <div class="d-flex justify-content-between mb-1">
                    <small class="text-secondary">Item Progress</small>
                    <small class="fw-bold">${completed}/${total} completed</small>
                </div>
                <div class="progress" style="height:8px;">
                    <div class="progress-bar bg-success" style="width:${pct}%"></div>
                    <div class="progress-bar bg-info" style="width:${total ? Math.round((running / total) * 100) : 0}%"></div>
                </div>
            </div>
            <div class="list-group list-group-flush">
                ${itemTasks.map(it => {
            const statusClass = `pw-status-badge pw-status-${(it.taskStatus || '').toLowerCase().replace(/\s/g, '_')}`;
            return `<div class="list-group-item d-flex align-items-center justify-content-between px-0 py-2">
                        <div>
                            <strong>${esc(it.itemName)}</strong>
                            ${it.itemDescription ? `<div class="small text-secondary">${esc(it.itemDescription)}</div>` : ''}
                            ${itemSpecLine(it)}
                        </div>
                        <div class="d-flex align-items-center gap-2">
                            <span class="${statusClass}">${esc(it.taskStatus)}</span>
                            ${it.taskStatus === 'NOT_STARTED' ? `<button class="btn btn-sm btn-outline-primary pw-item-start" data-item-id="${it.taskItemId}"><i class="bi bi-play-fill"></i> Start</button>` : ''}
                            ${it.taskStatus === 'RUNNING' ? `<button class="btn btn-sm btn-outline-success pw-item-complete" data-item-id="${it.taskItemId}"><i class="bi bi-check-lg"></i> Complete</button>` : ''}
                        </div>
                    </div>`;
        }).join('')}
            </div>
        `;

        // Bind item-level action buttons
        wrap.querySelectorAll('.pw-item-start').forEach(btn => {
            btn.addEventListener('click', async () => {
                const itemId = btn.dataset.itemId;
                await fetchJson(`/api/workspace/item-task/${itemId}/start`, {
                    method: 'POST', headers: { 'Content-Type': 'application/json' }, body: '{}'
                });
                Swal.fire({ icon: 'success', title: 'Item Started', timer: 1000, showConfirmButton: false });
                await load();
            });
        });
        wrap.querySelectorAll('.pw-item-complete').forEach(btn => {
            btn.addEventListener('click', async () => {
                const itemId = btn.dataset.itemId;
                await fetchJson(`/api/workspace/item-task/${itemId}/complete`, {
                    method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ remarks: '' })
                });
                Swal.fire({ icon: 'success', title: 'Item Completed', timer: 1000, showConfirmButton: false });
                await load();
            });
        });
    }

    function renderAllocation(data) {
        const wrap = $('pwAllocationWrap');
        if (!wrap) return;
        const machines = data?.machines || [];
        if (!machines.length) {
            wrap.innerHTML = '<div class="px-3 py-2 text-secondary small"><i class="bi bi-info-circle me-1"></i>No machine allocated for this job.</div>';
            return;
        }
        wrap.innerHTML = machines.map(m => {
            const employees = m.employees || m.Employees || [];
            const empHtml = employees.length
                ? employees.map(e => `
                    <div class="pw-alloc-emp">
                        <span class="avatar avatar-xs bg-teal-lt text-teal me-2"><i class="bi bi-person-fill"></i></span>
                        <span class="pw-alloc-emp-name">${esc(e.employeeName || e.EmployeeName || '—')}</span>
                        ${(e.roleCode || e.RoleCode) ? `<span class="badge bg-blue-lt text-blue ms-auto">${esc(e.roleCode || e.RoleCode)}</span>` : ''}
                        ${(e.shiftCode || e.ShiftCode) ? `<span class="badge bg-muted-lt text-secondary ms-1">${esc(e.shiftCode || e.ShiftCode)}</span>` : ''}
                    </div>`).join('')
                : '<div class="text-secondary small px-1 py-1"><i class="bi bi-person-x me-1"></i>No workforce assigned</div>';
            return `
                <div class="pw-alloc-machine">
                    <div class="pw-alloc-machine-header">
                        <span class="avatar avatar-xs bg-orange-lt text-orange me-2"><i class="bi bi-cpu-fill"></i></span>
                        <span class="fw-semibold">${esc(m.machineName || m.MachineName || '—')}</span>
                        ${(m.machineCode || m.MachineCode) ? `<span class="text-secondary small ms-1">(${esc(m.machineCode || m.MachineCode)})</span>` : ''}
                    </div>
                    ${(m.processName || m.ProcessName) ? `<div class="pw-alloc-process-label"><i class="bi bi-gear me-1 text-secondary"></i>${esc(m.processName || m.ProcessName)}</div>` : ''}
                    <div class="pw-alloc-emp-list">${empHtml}</div>
                </div>`;
        }).join('');
    }

    function renderQuickInfo(data) {
        const t = data.task || {};
        $('pwJobNo').textContent = `Job: ${t.jobNo || '—'}`;
        $('pwPartyName').textContent = `Customer: ${t.partyName || '—'}`;
        $('pwProcessName').textContent = `Process: ${t.processName || '—'}`;
        $('pwPriority').textContent = `Priority: ${t.priority || 'Normal'}`;
    }

    async function load() {
        try {
            const [data, flow, allocation] = await Promise.all([
                fetchJson(`/api/workspace/task/${_taskId}/process-detail`),
                fetchJson(`/api/workspace/process-flow/${_taskId}`).catch(() => null),
                fetchJson(`/api/workspace/task/${_taskId}/allocation`).catch(() => null)
            ]);

            _detailData = data;
            _task = data.task;
            _flow = flow;
            renderSummary(data);
            renderQuickInfo(data);
            renderAllocation(allocation);
            await renderChecklist(data);
            renderPipeline(flow);
            await renderParts(data);
            renderInsights(data);
        } catch (err) {
            const message = err?.message || 'Unable to load process work details.';
            Swal.fire({ icon: 'error', title: 'Load Failed', text: message });
        }
    }

    async function startStep() {
        if (!_task) return;

        const currentStep = _flow?.steps?.find(s => s.isCurrent) || null;
        const stepName = currentStep?.processName || currentStep?.eventLabel || _task?.processName || 'this step';
        const stepStatus = (currentStep?.stepStatus || _task?.taskStatus || '').toUpperCase();

        // If already started, inform user
        if (['IN_PROGRESS', 'RUNNING', 'STARTED'].includes(stepStatus)) {
            try { Swal.fire({ icon: 'info', title: 'Already Started', text: 'This step is already started.' }); } catch { alert('This step is already started.'); }
            return;
        }

        // Ask confirmation before starting
        try {
            const res = await Swal.fire({
                title: `Start step?`,
                text: `Do you really want to start ${stepName}?`,
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Yes, start',
                cancelButtonText: 'Cancel'
            });

            if (!res.isConfirmed) return;

            // show progress on Start button
            const startBtn = document.getElementById('pwStartBtn');
            setButtonLoading(startBtn, true, 'Starting...');
            try {
                await fetchJson(`/api/workspace/task/${_taskId}/start`, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: '{}' });
                await load();
                Swal.fire({ icon: 'success', title: 'Started', timer: 1200, showConfirmButton: false });
            } finally {
                setButtonLoading(startBtn, false);
            }
        } catch (err) {
            Swal.fire({ icon: 'error', title: 'Start Failed', text: err?.message || 'Unable to start this task.' });
        }
    }

    async function saveProgress() {
        const notes = $('pwNotes').value || '';
        const partIds = Array.from(document.querySelectorAll('.pw-part:checked')).map(x => parseInt(x.value));
        const processInputIds = Array.from(document.querySelectorAll('#pwProcessInputs option:checked')).map(x => parseInt(x.value));
        const checks = Array.from(document.querySelectorAll('.pw-check:checked')).length + processInputIds.length;

        const payload = {
            remarks: notes,
            partIds,
            processInputIds,
            checksCompleted: checks
        };

        await fetchJson(`/api/workspace/task/${_taskId}/work-note`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        renderInsights(_detailData || { task: _task });
        Swal.fire({ icon: 'success', title: 'Saved', timer: 1000, showConfirmButton: false });
    }

    async function completeStep() {
        // Validate: task must be started (IN_PROGRESS) before completing
        const taskStatus = (_task?.taskStatus || '').toUpperCase();
        if (taskStatus !== 'IN_PROGRESS') {
            Swal.fire({
                icon: 'warning',
                title: 'Process Not Started',
                text: 'This process has not been started yet. Please start the process before marking it as complete.'
            });
            return;
        }

        const notes = $('pwNotes').value || '';
        const checked = document.querySelectorAll('.pw-check:checked').length + document.querySelectorAll('#pwProcessInputs option:checked').length;

        if (checked === 0) {
            Swal.fire({ icon: 'warning', title: 'Checklist Required', text: 'Please select at least one process checkpoint.' });
            return;
        }

        const currentStep = _flow?.steps?.find(s => s.isCurrent) || null;
        const stepName = currentStep?.processName || currentStep?.eventLabel || _task?.processName || 'this step';

        const confirm = await Swal.fire({
            title: `Complete step?`,
            text: `Do you really want to complete this step ${stepName}?`,
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: 'Yes, complete',
            cancelButtonText: 'Cancel'
        });

        if (!confirm.isConfirmed) return;

        // show progress on Complete button
        const completeBtn = document.getElementById('pwCompleteBtn');
        setButtonLoading(completeBtn, true, 'Completing...');
        try {
            await fetchJson(`/api/workspace/task/${_taskId}/complete`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ remarks: notes })
            });

            Swal.fire({ icon: 'success', title: 'Step Completed', timer: 1200, showConfirmButton: false })
                .then(() => window.location.href = '/Workspace/MyTasks');
        } catch (err) {
            Swal.fire({ icon: 'error', title: 'Complete Failed', text: err?.message || 'Unable to complete this task.' });
        } finally {
            setButtonLoading(completeBtn, false);
        }
    }

    function bind() {
        $('pwRefreshBtn')?.addEventListener('click', load);
        $('pwStartBtn')?.addEventListener('click', startStep);
        $('pwSaveBtn')?.addEventListener('click', saveProgress);
        $('pwCompleteBtn')?.addEventListener('click', completeStep);
        document.addEventListener('change', (e) => {
            if (e.target?.classList?.contains('pw-check') || e.target?.classList?.contains('pw-part')) {
                renderInsights(_detailData || { task: _task });
            }
        });
    }

    // Utility to set a button into loading state with text and spinner
    function setButtonLoading(btn, isLoading, text) {
        if (!btn) return;
        if (isLoading) {
            btn.dataset.origHtml = btn.innerHTML;
            btn.disabled = true;
            btn.innerHTML = `<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>${text || 'Please wait...'} `;
        } else {
            btn.disabled = false;
            if (btn.dataset.origHtml) {
                btn.innerHTML = btn.dataset.origHtml;
                delete btn.dataset.origHtml;
            }
        }
    }

    async function init(taskId) {
        _taskId = taskId;
        bind();
        await load();
    }

    return { init };
})();