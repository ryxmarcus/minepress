const DesignWork = (() => {
    let _taskId = 0;
    let _task = null;
    let _flow = null;
    let _detailData = null;
    let _customRowIndex = 10000;

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
                } catch { msg = raw; }
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

    function setButtonLoading(btn, loading, label) {
        if (!btn) return;
        if (loading) {
            btn.dataset.origHtml = btn.innerHTML;
            btn.innerHTML = `<span class="spinner-border spinner-border-sm me-1"></span>${label || 'Loading...'}`;
            btn.disabled = true;
        } else {
            if (btn.dataset.origHtml) btn.innerHTML = btn.dataset.origHtml;
            btn.disabled = false;
        }
    }

    // ── Totals & pending calculation ──────────────────────────────────────

    function updateTotals() {
        const rows = document.querySelectorAll('#dwDesignTableBody .dw-design-row');
        let totalRequired = 0, totalCompleted = 0;

        rows.forEach(row => {
            if (row.dataset.completed === 'true') {
                const req  = parseInt(row.querySelector('.dw-required')?.value  || 0) || 0;
                const comp = parseInt(row.querySelector('.dw-completed')?.value || 0) || 0;
                totalRequired  += req;
                totalCompleted += comp;
                return;
            }
            const req  = parseInt(row.querySelector('.dw-required')?.value  || 0) || 0;
            const comp = parseInt(row.querySelector('.dw-completed')?.value || 0) || 0;
            const pending = Math.max(0, req - comp);

            const pendingEl = row.querySelector('.dw-pending');
            if (pendingEl) {
                pendingEl.textContent = pending;
                pendingEl.className = `badge dw-pending fw-bold ${pending > 0 ? 'bg-orange-lt text-orange' : 'bg-green-lt text-green'}`;
            }

            totalRequired  += req;
            totalCompleted += comp;
        });

        const totalPending = Math.max(0, totalRequired - totalCompleted);
        const pct = totalRequired > 0 ? Math.round((totalCompleted / totalRequired) * 100) : 0;

        const act  = $('dwTotalActivities');
        const req  = $('dwTotalRequired');
        const comp = $('dwTotalCompleted');
        const pend = $('dwTotalPending');
        if (act)  act.textContent  = rows.length;
        if (req)  req.textContent  = totalRequired;
        if (comp) comp.textContent = totalCompleted;
        if (pend) pend.textContent = totalPending;
    }

    function bindTableInputs(container) {
        container = container || document.getElementById('dwDesignTableBody');
        if (!container) return;
        container.querySelectorAll('.dw-required, .dw-completed').forEach(input => {
            if (!input.dataset.bound) {
                input.dataset.bound = '1';
                input.addEventListener('input', updateTotals);
            }
        });
    }

    // ── Individual task complete ──────────────────────────────────────────

    async function completeIndividualTask(btn) {
        const taskStatus = (_task?.taskStatus || '').toUpperCase();
        if (taskStatus !== 'IN_PROGRESS') {
            try { Swal.fire({ icon: 'warning', title: 'Task Not Started', text: 'Please start the task before completing activities.' }); } catch { alert('Please start the task before completing activities.'); }
            return;
        }

        const row = btn.closest('.dw-design-row');
        if (!row) return;
        const req  = parseInt(row.querySelector('.dw-required')?.value  || 0) || 0;
        const comp = parseInt(row.querySelector('.dw-completed')?.value || 0) || 0;

        if (req <= 0) {
            try { Swal.fire({ icon: 'warning', title: 'Required Missing', text: 'Pages Required must be greater than 0 to complete this task.' }); } catch { alert('Pages Required must be greater than 0.'); }
            return;
        }
        if (comp < req) {
            try { Swal.fire({ icon: 'warning', title: 'Pages Pending', text: `${req - comp} page(s) still pending. Set Pages Completed equal to Required first.` }); } catch { alert(`${req - comp} page(s) still pending.`); }
            return;
        }

        // ── Collect this row as a single upsert item ──────────────────────
        const rowData = collectSingleRow(row, true);
        if (!rowData) {
            try { Swal.fire({ icon: 'warning', title: 'Missing Activity', text: 'Activity name cannot be empty.' }); } catch { }
            return;
        }

        // ── UI: lock the row immediately ──────────────────────────────────
        row.dataset.completed = 'true';
        row.classList.add('table-success');

        const reqInput  = row.querySelector('.dw-required');
        const compInput = row.querySelector('.dw-completed');
        if (reqInput)  reqInput.disabled  = true;
        if (compInput) compInput.disabled = true;

        const pendingEl = row.querySelector('.dw-pending');
        if (pendingEl) {
            pendingEl.textContent = '0';
            pendingEl.className = 'badge dw-pending fw-bold bg-green-lt text-green';
        }

        btn.innerHTML = '<i class="bi bi-arrow-repeat me-1"></i>Saving...';
        btn.disabled = true;

        updateTotals();

        // ── Upsert this row to DB (update if exists, insert if not) ───────
        try {
            const notes = $('dwNotes')?.value || '';
            const result = await upsertRows([rowData], notes);

            // Stamp the returned DesignWorkId onto this row specifically
            const saved = result?.entries?.find(e =>
                (e.activityName || '').toLowerCase() === rowData.activity.toLowerCase()
            );
            if (saved?.designWorkId) {
                row.dataset.designWorkId = saved.designWorkId;
            }

            btn.innerHTML = '<i class="bi bi-check-circle-fill me-1"></i>Done';
            btn.classList.remove('btn-outline-success');
            btn.classList.add('btn-success');
        } catch (err) {
            // UI already locked — just log, don't revert to avoid confusion
            btn.innerHTML = '<i class="bi bi-check-circle-fill me-1"></i>Done (offline)';
            btn.classList.add('btn-warning');
            try { Swal.fire({ icon: 'warning', title: 'Save Warning', text: `Row marked complete locally but DB save failed: ${err?.message || 'Unknown error'}` }); } catch { }
        }
    }

    // ── Bind individual complete buttons ──────────────────────────────────

    function bindCompleteButtons(container) {
        container = container || document;
        container.querySelectorAll('.dw-complete-task').forEach(btn => {
            if (!btn.dataset.bound) {
                btn.dataset.bound = '1';
                btn.addEventListener('click', () => completeIndividualTask(btn));
            }
        });
    }

    // ── Update single row ─────────────────────────────────────────────────

    async function updateSingleRow(btn) {
        const taskStatus = (_task?.taskStatus || '').toUpperCase();
        if (taskStatus !== 'IN_PROGRESS') {
            try { Swal.fire({ icon: 'warning', title: 'Task Not Started', text: 'Please start the task before updating activities.' }); } catch { alert('Please start the task before updating activities.'); }
            return;
        }

        const row = btn.closest('.dw-design-row');
        if (!row) return;

        const activityEl = row.querySelector('.dw-activity-name');
        const activity = activityEl?.tagName === 'INPUT'
            ? (activityEl.value || '').trim()
            : (activityEl?.textContent || '').trim();

        if (!activity) {
            try { Swal.fire({ icon: 'warning', title: 'Missing Activity', text: 'Please enter an activity name before updating.' }); } catch { alert('Enter an activity name.'); }
            return;
        }

        const required  = parseInt(row.querySelector('.dw-required')?.value  || 0) || 0;
        const completed = parseInt(row.querySelector('.dw-completed')?.value || 0) || 0;

        if (required <= 0) {
            try { Swal.fire({ icon: 'warning', title: 'Pages Required', text: 'Pages Required must be greater than 0 before updating.' }); } catch { alert('Enter Pages Required > 0.'); }
            return;
        }
        if (completed > required) {
            try { Swal.fire({ icon: 'warning', title: 'Invalid Pages', text: 'Pages Completed cannot exceed Pages Required.' }); } catch { }
            return;
        }

        const rowData = collectSingleRow(row, row.dataset.completed === 'true');
        if (!rowData) return;

        const origHtml = btn.innerHTML;
        btn.innerHTML  = '<span class="spinner-border spinner-border-sm"></span>';
        btn.disabled   = true;

        try {
            const notes  = $('dwNotes')?.value || '';
            const result = await upsertRows([rowData], notes);

            // Stamp back the DesignWorkId if this was a new row
            const saved = result?.entries?.find(e =>
                (e.activityName || '').toLowerCase() === rowData.activity.toLowerCase()
            );
            if (saved?.designWorkId && !row.dataset.designWorkId) {
                row.dataset.designWorkId = saved.designWorkId;
            }

            // Visual feedback — flash row green briefly
            row.classList.add('table-info');
            setTimeout(() => row.classList.remove('table-info'), 1200);

            updateTotals();

            try { Swal.fire({ icon: 'success', title: 'Updated', text: `"${activity}" saved successfully.`, timer: 1000, showConfirmButton: false }); } catch { }
        } catch (err) {
            try { Swal.fire({ icon: 'error', title: 'Update Failed', text: err?.message || 'Unable to save this row.' }); } catch { alert('Update failed.'); }
        } finally {
            btn.innerHTML = origHtml;
            btn.disabled  = false;
        }
    }

    // ── Bind update buttons ───────────────────────────────────────────────

    function bindUpdateButtons(container) {
        container = container || document;
        container.querySelectorAll('.dw-update-task').forEach(btn => {
            if (!btn.dataset.bound) {
                btn.dataset.bound = '1';
                btn.addEventListener('click', () => updateSingleRow(btn));
            }
        });
    }

    // ── Add custom row ────────────────────────────────────────────────────

    function addCustomRow() {
        const tbody = $('dwDesignTableBody');
        if (!tbody) return;
        const noRow = $('dwNoConfigRow');
        if (noRow) noRow.remove();

        const idx = _customRowIndex++;
        const tr = document.createElement('tr');
        tr.className = 'dw-design-row';
        tr.dataset.row = idx;
        tr.innerHTML = `
            <td><input type="checkbox" class="form-check-input dw-row-check" value="${idx}"></td>
            <td>
                <div class="d-flex align-items-center gap-2">
                    <span class="avatar avatar-xs bg-purple-lt text-purple"><i class="bi bi-pencil-fill"></i></span>
                    <input type="text" class="form-control form-control-sm dw-activity-name"
                           placeholder="Design activity name" style="min-width:140px;white-space:normal;word-break:break-word;">
                </div>
            </td>
            <td><input type="number" class="form-control form-control-sm dw-required" value="0" min="0"></td>
            <td><input type="number" class="form-control form-control-sm dw-completed" value="0" min="0"></td>
            <td class="text-end"><span class="badge bg-orange-lt text-orange dw-pending fw-bold">0</span></td>
            <td style="width:180px;">
                <div class="d-flex gap-1 flex-wrap">
                    <button type="button" class="btn btn-sm btn-outline-primary dw-update-task" data-row="${idx}" title="Update this row">
                        <i class="bi bi-arrow-repeat me-1"></i>Update
                    </button>
                    <button type="button" class="btn btn-sm btn-outline-success dw-complete-task" data-row="${idx}">
                        <i class="bi bi-check-circle me-1"></i>Complete
                    </button>
                </div>
            </td>`;

        tbody.appendChild(tr);
        bindTableInputs(tr);
        bindCompleteButtons(tr);
        bindUpdateButtons(tr);
        updateTotals();
        tr.querySelector('.dw-activity-name')?.focus();
    }

    // ── Render Task Summary ───────────────────────────────────────────────

    function renderSummary(data) {
        const t = data.task || {};
        const el = $('dwTaskSummary');
        if (!el) return;
        el.innerHTML = `
            <div class="pw-info-list">
                <div class="pw-info-row">
                    <i class="bi bi-pen"></i>
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
                ${t.jobNo ? `<div class="pw-info-row">
                    <i class="bi bi-briefcase"></i>
                    <span class="pw-info-label">Job No</span>
                    <span class="pw-info-value fw-semibold">${esc(t.jobNo)}</span>
                </div>` : ''}
            </div>`;
    }

    // ── Render Quick Info ─────────────────────────────────────────────────

    function renderQuickInfo(data) {
        const t = data.task || {};
        const setTxt = (id, txt) => { const el = $(id); if (el) el.textContent = txt; };
        setTxt('dwJobNo',       `Job: ${t.jobNo || '—'}`);
        setTxt('dwPartyName',   `Customer: ${t.partyName || '—'}`);
        setTxt('dwProcessName', `Process: ${t.processName || '—'}`);
        setTxt('dwPriority',    `Priority: ${t.priority || 'Normal'}`);
    }

    // ── Render Checklist ──────────────────────────────────────────────────

    async function renderChecklist(data) {
        const wrap = $('dwChecklistWrap');
        if (!wrap) return;

        const processInputs = await fetchJson(`/api/workspace/task/${_taskId}/process-input-options`).catch(() => null);
        if (processInputs?.options?.length) {
            wrap.innerHTML = `
                <div class="pw-process-input-block">
                    <div class="small text-secondary mb-2">${esc(processInputs.label || 'Process Inputs')} (Multiple Select)</div>
                    <select id="dwProcessInputs" class="form-select pw-process-input-select" multiple size="6">
                        ${processInputs.options.map(o => `<option value="${o.id}">${esc(o.name)}${o.code ? ` (${esc(o.code)})` : ''}</option>`).join('')}
                    </select>
                    <div class="mt-2 d-flex gap-2">
                        <button id="dwAddInputsToNotesBtn" class="btn btn-sm btn-outline-primary">Add to Notes</button>
                        <button id="dwClearProcessInputsBtn" class="btn btn-sm btn-outline-secondary">Clear</button>
                        <div class="small text-secondary ms-auto">Hold Ctrl to multi-select.</div>
                    </div>
                </div>`;

            wrap.querySelector('#dwAddInputsToNotesBtn')?.addEventListener('click', () => {
                const selected = Array.from(document.querySelectorAll('#dwProcessInputs option:checked'))
                    .map(o => o.textContent.trim()).filter(t => t.length > 0);
                if (!selected.length) {
                    try { Swal.fire({ icon: 'warning', title: 'No selection', text: 'Please select one or more inputs first.' }); } catch { alert('Please select one or more inputs first.'); }
                    return;
                }
                const block = selected.map(s => `- ${s}`).join('\n');
                const notesEl = $('dwNotes');
                if (notesEl) {
                    notesEl.value = notesEl.value.trim() ? notesEl.value.trim() + '\n' + block : block;
                    notesEl.dispatchEvent(new Event('input', { bubbles: true }));
                }
                document.querySelectorAll('#dwProcessInputs option:checked').forEach(o => o.selected = false);
            });

            wrap.querySelector('#dwClearProcessInputsBtn')?.addEventListener('click', () => {
                document.querySelectorAll('#dwProcessInputs option:checked').forEach(o => o.selected = false);
            });
            return;
        }

        // Fallback hardcoded checks
        const checks = [
            'Content / brief received from client',
            'Design references / brand guidelines verified',
            'Fonts, logos and assets available',
            'DTP layout dimensions confirmed',
            'Color profile verified (CMYK / RGB)',
            'Output quality & bleed settings checked',
            'Final file approved for next stage'
        ];

        wrap.innerHTML = checks.map((c, i) => `
            <label class="form-check mb-2 pw-check-item">
                <input class="form-check-input pw-check" type="checkbox" value="${i + 1}">
                <span class="form-check-label">${esc(c)}</span>
            </label>`).join('');
    }

    // ── Load saved design work rows from DB and stamp data-design-work-id ─

    async function loadSavedDesignRows() {
        try {
            const entries = await fetchJson(`/api/workspace/design-work/${_taskId}`);
            if (!entries || !entries.length) return;

            const tbody = $('dwDesignTableBody');
            if (!tbody) return;

            // Match saved entries to existing rows by activity name (case-insensitive)
            entries.forEach(entry => {
                const rows = tbody.querySelectorAll('.dw-design-row');
                rows.forEach(row => {
                    const activityEl = row.querySelector('.dw-activity-name');
                    const name = activityEl?.tagName === 'INPUT'
                        ? (activityEl.value || '').trim()
                        : (activityEl?.textContent || '').trim();

                    if (name.toLowerCase() === (entry.activityName || '').toLowerCase()) {
                        row.dataset.designWorkId = entry.designWorkId;
                    }
                });
            });
        } catch { /* non-blocking */ }
    }

    // ── Collect table rows for save ───────────────────────────────────────

    function collectDesignRows() {
        const rows = [];
        document.querySelectorAll('#dwDesignTableBody .dw-design-row').forEach(row => {
            const activityEl = row.querySelector('.dw-activity-name');
            const activity = activityEl?.tagName === 'INPUT' ? (activityEl.value || '').trim() : (activityEl?.textContent || '').trim();
            const required  = parseInt(row.querySelector('.dw-required')?.value  || 0) || 0;
            const completed = parseInt(row.querySelector('.dw-completed')?.value || 0) || 0;
            const pending   = Math.max(0, required - completed);
            const designWorkId = row.dataset.designWorkId ? parseInt(row.dataset.designWorkId) : null;
            const isCompleted  = row.dataset.completed === 'true';
            rows.push({ designWorkId, activity, required, completed, pending, isCompleted });
        });
        return rows;
    }

    // ── Collect only checked rows ─────────────────────────────────────────

    function collectCheckedRows(forceIsCompleted = null) {
        const rows = [];
        document.querySelectorAll('#dwDesignTableBody .dw-design-row').forEach(row => {
            const chk = row.querySelector('.dw-row-check');
            if (!chk || !chk.checked) return;

            const activityEl = row.querySelector('.dw-activity-name');
            const activity = activityEl?.tagName === 'INPUT' ? (activityEl.value || '').trim() : (activityEl?.textContent || '').trim();
            if (!activity) return;

            const required     = parseInt(row.querySelector('.dw-required')?.value  || 0) || 0;
            const completed    = parseInt(row.querySelector('.dw-completed')?.value || 0) || 0;
            const pending      = Math.max(0, required - completed);
            const designWorkId = row.dataset.designWorkId ? parseInt(row.dataset.designWorkId) : null;
            const isCompleted  = forceIsCompleted !== null ? forceIsCompleted : (row.dataset.completed === 'true');
            rows.push({ designWorkId, activity, required, completed, pending, isCompleted, _row: row });
        });
        return rows;
    }

    // ── Collect a single row (for individual complete) ────────────────────

    function collectSingleRow(row, isCompleted = false) {
        const activityEl = row.querySelector('.dw-activity-name');
        const activity = activityEl?.tagName === 'INPUT' ? (activityEl.value || '').trim() : (activityEl?.textContent || '').trim();
        if (!activity) return null;

        const required     = parseInt(row.querySelector('.dw-required')?.value  || 0) || 0;
        const completed    = parseInt(row.querySelector('.dw-completed')?.value || 0) || 0;
        const pending      = Math.max(0, required - completed);
        const designWorkId = row.dataset.designWorkId ? parseInt(row.dataset.designWorkId) : null;
        return { designWorkId, activity, required, completed, pending, isCompleted };
    }

    // ── Upsert rows to DB and stamp back design-work IDs ─────────────────

    async function upsertRows(rows, notes = '') {
        if (!rows || rows.length === 0) return;
        const payload = {
            notes,
            rows: rows.map(r => ({
                designWorkId: r.designWorkId || null,
                activity:     r.activity,
                required:     r.required,
                completed:    r.completed,
                pending:      r.pending,
                isCompleted:  r.isCompleted
            }))
        };
        const result = await fetchJson(`/api/workspace/design-work/${_taskId}/upsert`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });
        // Stamp returned IDs onto rows by activity name (case-insensitive)
        stampDesignWorkIds(result?.entries);
        return result;
    }

    // ── Stamp DesignWorkId onto TR rows from server response ──────────────

    function stampDesignWorkIds(entries) {
        if (!entries || !entries.length) return;
        const tbody = document.getElementById('dwDesignTableBody');
        if (!tbody) return;
        entries.forEach(entry => {
            tbody.querySelectorAll('.dw-design-row').forEach(row => {
                if (row.dataset.designWorkId) return; // already stamped
                const activityEl = row.querySelector('.dw-activity-name');
                const name = activityEl?.tagName === 'INPUT'
                    ? (activityEl.value || '').trim()
                    : (activityEl?.textContent || '').trim();
                if (name.toLowerCase() === (entry.activityName || '').toLowerCase()) {
                    row.dataset.designWorkId = entry.designWorkId;
                }
            });
        });
    }

    // ── Load ──────────────────────────────────────────────────────────────

    async function load() {
        try {
            const [data, flow] = await Promise.all([
                fetchJson(`/api/workspace/task/${_taskId}/process-detail`),
                fetchJson(`/api/workspace/process-flow/${_taskId}`).catch(() => null)
            ]);
            _detailData = data;
            _task = data.task;
            _flow = flow;
            renderSummary(data);
            await renderChecklist(data);
        } catch (err) {
            try { Swal.fire({ icon: 'error', title: 'Load Failed', text: err?.message || 'Unable to load design work details.' }); } catch { }
        }
    }

    // ── Start Step ────────────────────────────────────────────────────────

    async function startStep() {
        if (!_task) return;
        const stepStatus = (_task?.taskStatus || '').toUpperCase();
        if (['IN_PROGRESS', 'RUNNING', 'STARTED'].includes(stepStatus)) {
            try { Swal.fire({ icon: 'info', title: 'Already Started', text: 'This step is already in progress.' }); } catch { }
            return;
        }

        const allRows = collectDesignRows();

        try {
            const res = await Swal.fire({
                title: 'Start design step?',
                text: `Start ${_task?.processName || 'Design / DTP'}? (${allRows.length} activity row(s) will be saved.)`,
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Yes, start',
                cancelButtonText: 'Cancel'
            });
            if (!res.isConfirmed) return;

            const btn = $('dwStartBtn');
            setButtonLoading(btn, true, 'Starting...');
            try {
                const notes = $('dwNotes')?.value || '';

                // ── 1. Upsert all rows first ──────────────────────────────
                await upsertRows(allRows, notes);

                // ── 2. Start the task ─────────────────────────────────────
                await fetchJson(`/api/workspace/task/${_taskId}/start`, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: '{}' });
                await load();
                Swal.fire({ icon: 'success', title: 'Started', text: `${allRows.length} row(s) saved & step started.`, timer: 1500, showConfirmButton: false });
            } finally {
                setButtonLoading(btn, false);
            }
        } catch (err) {
            try { Swal.fire({ icon: 'error', title: 'Start Failed', text: err?.message || 'Unable to start this task.' }); } catch { }
        }
    }

    // ── Save Progress ─────────────────────────────────────────────────────

    async function saveProgress() {
        const notes = $('dwNotes')?.value || '';
        const checks = Array.from(document.querySelectorAll('.pw-check:checked')).length
                     + Array.from(document.querySelectorAll('#dwProcessInputs option:checked')).length;
        const designRows = collectDesignRows();

        const payload = {
            remarks: notes,
            partIds: [],
            processInputIds: Array.from(document.querySelectorAll('#dwProcessInputs option:checked')).map(x => parseInt(x.value)),
            checksCompleted: checks,
            designProgress: designRows.map(r => ({
                designWorkId: r.designWorkId || null,
                activity:     r.activity,
                required:     r.required,
                completed:    r.completed,
                pending:      r.pending,
                isCompleted:  r.isCompleted
            }))
        };

        const btn = $('dwSaveBtn');
        setButtonLoading(btn, true, 'Saving...');
        try {
            const saved = await fetchJson(`/api/workspace/task/${_taskId}/work-note`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });

            // Reload saved rows to get assigned DesignWorkIds
            await loadSavedDesignRows();

            Swal.fire({ icon: 'success', title: 'Saved', timer: 1000, showConfirmButton: false });
        } finally {
            setButtonLoading(btn, false);
        }
    }

    // ── Complete Step ─────────────────────────────────────────────────────

    async function completeStep() {
        const taskStatus = (_task?.taskStatus || '').toUpperCase();
        if (taskStatus !== 'IN_PROGRESS') {
            Swal.fire({
                icon: 'warning',
                title: 'Process Not Started',
                text: 'Please start the process before marking it as complete.'
            });
            return;
        }

        // Guard: all individual tasks must be completed
        const incompleteRows = document.querySelectorAll('#dwDesignTableBody .dw-design-row:not([data-completed="true"])');
        if (incompleteRows.length > 0) {
            Swal.fire({
                icon: 'warning',
                title: 'Tasks Incomplete',
                text: `${incompleteRows.length} task(s) are not yet completed. Use the Complete button on each row to mark them done.`
            });
            return;
        }

        const designRows = collectDesignRows();
        const totalRequired  = designRows.reduce((s, r) => s + r.required,  0);
        const totalCompleted = designRows.reduce((s, r) => s + r.completed, 0);
        const totalPending   = Math.max(0, totalRequired - totalCompleted);

        if (totalPending > 0) {
            Swal.fire({
                icon: 'warning',
                title: 'Pages Pending',
                text: `${totalPending} page(s) still pending. All pages must be completed before finishing.`
            });
            return;
        }

        // Checked rows to upsert as completed
        const checkedRows = collectCheckedRows(true);
        if (checkedRows.length === 0) {
            try { Swal.fire({ icon: 'warning', title: 'No Selection', text: 'Please check at least one activity row to save before completing.' }); } catch { alert('Select at least one row.'); }
            return;
        }

        const notes = $('dwNotes')?.value || '';

        try {
            const res = await Swal.fire({
                title: 'Complete design step?',
                html: `All design activities will be marked as done.<br><span class="text-muted" style="font-size:.85rem;">${checkedRows.length} checked row(s) will be saved as completed.</span>`,
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Yes, complete',
                cancelButtonText: 'Cancel',
                confirmButtonColor: '#10b981'
            });
            if (!res.isConfirmed) return;

            const btn = $('dwCompleteBtn');
            setButtonLoading(btn, true, 'Completing...');
            try {
                // ── 1. Upsert checked rows as completed ───────────────────
                await upsertRows(checkedRows, notes);

                // ── 2. Call task complete ─────────────────────────────────
                await fetchJson(`/api/workspace/task/${_taskId}/complete`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({
                        remarks: notes,
                        designProgress: designRows.map(r => ({
                            designWorkId: r.designWorkId || null,
                            activity:     r.activity,
                            required:     r.required,
                            completed:    r.completed,
                            pending:      r.pending,
                            isCompleted:  true
                        }))
                    })
                });
                Swal.fire({ icon: 'success', title: 'Step Completed!', text: 'Design work marked as complete.', timer: 1800, showConfirmButton: false });
                setTimeout(() => { window.location.href = '/Workspace/MyTasks'; }, 1900);
            } finally {
                setButtonLoading(btn, false);
            }
        } catch (err) {
            try { Swal.fire({ icon: 'error', title: 'Complete Failed', text: err?.message || 'Unable to complete this task.' }); } catch { }
        }
    }

    // ── Init ──────────────────────────────────────────────────────────────

    function init(taskId) {
        _taskId = taskId;

        // Bind existing table inputs and complete buttons
        bindTableInputs();
        bindCompleteButtons();
        bindUpdateButtons();
        updateTotals();

        // Refresh button
        $('dwRefreshBtn')?.addEventListener('click', () => load());

        // Action buttons
        $('dwStartBtn')?.addEventListener('click',    startStep);
        $('dwSaveBtn')?.addEventListener('click',     saveProgress);
        $('dwCompleteBtn')?.addEventListener('click', completeStep);

        load();
    }

    return { init };
})();
