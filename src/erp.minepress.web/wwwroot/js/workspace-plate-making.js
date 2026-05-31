const PlateMaking = (() => {
    let _taskId = 0;
    let _task = null;
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
        const rows = document.querySelectorAll('#pmPlateMakingTableBody .pm-plate-row');
        let totalColors = 0, totalRequired = 0, totalMade = 0;

        rows.forEach(row => {
            const colors   = parseInt(row.querySelector('.pm-colors')?.value   || 0) || 0;
            const required = parseInt(row.querySelector('.pm-required')?.value || 0) || 0;
            const made     = parseInt(row.querySelector('.pm-made')?.value     || 0) || 0;

            if (row.dataset.completed !== 'true') {
                const pending = Math.max(0, required - made);
                const pendingEl = row.querySelector('.pm-pending');
                if (pendingEl) {
                    pendingEl.textContent = pending;
                    pendingEl.className = `badge pm-pending fw-bold ${pending > 0 ? 'bg-orange-lt text-orange' : 'bg-green-lt text-green'}`;
                }
            }

            totalColors   += colors;
            totalRequired += required;
            totalMade     += made;
        });

        const totalPending = Math.max(0, totalRequired - totalMade);

        const act  = $('pmTotalActivities');
        const col  = $('pmTotalColors');
        const req  = $('pmTotalRequired');
        const made = $('pmTotalMade');
        const pend = $('pmTotalPending');
        if (act)  act.textContent  = rows.length;
        if (col)  col.textContent  = totalColors;
        if (req)  req.textContent  = totalRequired;
        if (made) made.textContent = totalMade;
        if (pend) pend.textContent = totalPending;
    }

    function bindTableInputs(container) {
        container = container || document.getElementById('pmPlateMakingTableBody');
        if (!container) return;
        container.querySelectorAll('.pm-required, .pm-made, .pm-colors').forEach(input => {
            if (!input.dataset.bound) {
                input.dataset.bound = '1';
                input.addEventListener('input', updateTotals);
            }
        });
    }

    // ── Individual row complete ───────────────────────────────────────────

    async function completeIndividualTask(btn) {
        const taskStatus = (_task?.taskStatus || '').toUpperCase();
        if (taskStatus !== 'IN_PROGRESS') {
            try { Swal.fire({ icon: 'warning', title: 'Task Not Started', text: 'Please start the task before completing activities.' }); } catch { alert('Please start the task before completing activities.'); }
            return;
        }

        const row = btn.closest('.pm-plate-row');
        if (!row) return;

        const required = parseInt(row.querySelector('.pm-required')?.value || 0) || 0;
        const made     = parseInt(row.querySelector('.pm-made')?.value     || 0) || 0;

        if (required <= 0) {
            try { Swal.fire({ icon: 'warning', title: 'Required Missing', text: 'Plates Required must be greater than 0 to complete this task.' }); } catch { alert('Plates Required must be greater than 0.'); }
            return;
        }
        if (made < required) {
            try { Swal.fire({ icon: 'warning', title: 'Plates Pending', text: `${required - made} plate(s) still pending. Set Plates Made equal to Required first.` }); } catch { alert(`${required - made} plate(s) still pending.`); }
            return;
        }

        const rowData = collectSingleRow(row, true);
        if (!rowData) {
            try { Swal.fire({ icon: 'warning', title: 'Missing Activity', text: 'Activity name cannot be empty.' }); } catch { }
            return;
        }

        // Lock row immediately
        row.dataset.completed = 'true';
        row.classList.add('table-success');
        row.querySelectorAll('.pm-required, .pm-made, .pm-colors, .pm-plate-type').forEach(el => el.disabled = true);

        const pendingEl = row.querySelector('.pm-pending');
        if (pendingEl) {
            pendingEl.textContent = '0';
            pendingEl.className = 'badge pm-pending fw-bold bg-green-lt text-green';
        }

        btn.innerHTML = '<i class="bi bi-arrow-repeat me-1"></i>Saving...';
        btn.disabled = true;
        updateTotals();

        try {
            const notes = $('pmNotes')?.value || '';
            const result = await upsertRows([rowData], notes);

            const saved = result?.entries?.find(e =>
                (e.activityName || '').toLowerCase() === rowData.activity.toLowerCase()
            );
            if (saved?.plateMakingId) {
                row.dataset.plateMakingId = saved.plateMakingId;
            }

            btn.innerHTML = '<i class="bi bi-check-circle-fill me-1"></i>Done';
            btn.classList.remove('btn-outline-success');
            btn.classList.add('btn-success');
        } catch (err) {
            btn.innerHTML = '<i class="bi bi-check-circle-fill me-1"></i>Done (offline)';
            btn.classList.add('btn-warning');
            try { Swal.fire({ icon: 'warning', title: 'Save Warning', text: `Row marked complete locally but DB save failed: ${err?.message || 'Unknown error'}` }); } catch { }
        }
    }

    function bindCompleteButtons(container) {
        container = container || document;
        container.querySelectorAll('.pm-complete-task').forEach(btn => {
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

        const row = btn.closest('.pm-plate-row');
        if (!row) return;

        const activityEl = row.querySelector('.pm-activity-name');
        const activity = activityEl?.tagName === 'INPUT'
            ? (activityEl.value || '').trim()
            : (activityEl?.textContent || '').trim();

        if (!activity) {
            try { Swal.fire({ icon: 'warning', title: 'Missing Activity', text: 'Please enter an activity name before updating.' }); } catch { alert('Enter an activity name.'); }
            return;
        }

        const required = parseInt(row.querySelector('.pm-required')?.value || 0) || 0;
        const made     = parseInt(row.querySelector('.pm-made')?.value     || 0) || 0;

        if (required <= 0) {
            try { Swal.fire({ icon: 'warning', title: 'Plates Required', text: 'Plates Required must be greater than 0 before updating.' }); } catch { alert('Enter Plates Required > 0.'); }
            return;
        }
        if (made > required) {
            try { Swal.fire({ icon: 'warning', title: 'Invalid Plates', text: 'Plates Made cannot exceed Plates Required.' }); } catch { }
            return;
        }

        const rowData = collectSingleRow(row, row.dataset.completed === 'true');
        if (!rowData) return;

        const origHtml = btn.innerHTML;
        btn.innerHTML = '<span class="spinner-border spinner-border-sm"></span>';
        btn.disabled  = true;

        try {
            const notes  = $('pmNotes')?.value || '';
            const result = await upsertRows([rowData], notes);

            const saved = result?.entries?.find(e =>
                (e.activityName || '').toLowerCase() === rowData.activity.toLowerCase()
            );
            if (saved?.plateMakingId && !row.dataset.plateMakingId) {
                row.dataset.plateMakingId = saved.plateMakingId;
            }

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

    function bindUpdateButtons(container) {
        container = container || document;
        container.querySelectorAll('.pm-update-task').forEach(btn => {
            if (!btn.dataset.bound) {
                btn.dataset.bound = '1';
                btn.addEventListener('click', () => updateSingleRow(btn));
            }
        });
    }

    // ── Add custom row ────────────────────────────────────────────────────

    function addCustomRow() {
        const tbody = $('pmPlateMakingTableBody');
        if (!tbody) return;
        const noRow = $('pmNoConfigRow');
        if (noRow) noRow.remove();

        const idx = _customRowIndex++;
        const tr = document.createElement('tr');
        tr.className = 'pm-plate-row';
        tr.dataset.row = idx;
        tr.innerHTML = `
            <td><input type="checkbox" class="form-check-input pm-row-check" value="${idx}"></td>
            <td>
                <div class="d-flex align-items-center gap-2">
                    <span class="avatar avatar-xs bg-blue-lt text-blue"><i class="bi bi-layers-fill"></i></span>
                    <input type="text" class="form-control form-control-sm pm-activity-name"
                           placeholder="Plate activity name" style="min-width:140px;">
                </div>
            </td>
            <td>
                <input type="text" class="form-control form-control-sm pm-part-name-input" placeholder="Part name" style="min-width:80px;">
            </td>
            <td><input type="text" class="form-control form-control-sm pm-plate-type" placeholder="CTP / Conv." style="min-width:80px;"></td>
            <td><input type="number" class="form-control form-control-sm pm-colors" value="4" min="0"></td>
            <td><input type="number" class="form-control form-control-sm pm-required" value="0" min="0"></td>
            <td><input type="number" class="form-control form-control-sm pm-made" value="0" min="0"></td>
            <td class="text-end"><span class="badge bg-orange-lt text-orange pm-pending fw-bold">0</span></td>
            <td style="width:180px;">
                <div class="d-flex gap-1 flex-wrap">
                    <button type="button" class="btn btn-sm btn-outline-primary pm-update-task" data-row="${idx}" title="Update this row">
                        <i class="bi bi-arrow-repeat me-1"></i>Update
                    </button>
                    <button type="button" class="btn btn-sm btn-outline-success pm-complete-task" data-row="${idx}">
                        <i class="bi bi-check-circle me-1"></i>Complete
                    </button>
                </div>
            </td>`;

        tbody.appendChild(tr);
        bindTableInputs(tr);
        bindCompleteButtons(tr);
        bindUpdateButtons(tr);
        updateTotals();
        tr.querySelector('.pm-activity-name')?.focus();
    }

    // ── Render Task Summary ───────────────────────────────────────────────

    function renderSummary(data) {
        const t = data.task || {};
        const el = $('pmTaskSummary');
        if (!el) return;
        el.innerHTML = `
            <div class="pw-info-list">
                <div class="pw-info-row">
                    <i class="bi bi-layers"></i>
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

    // ── Collect all plate rows for save ───────────────────────────────────

    function collectPlateRows() {
        const rows = [];
        document.querySelectorAll('#pmPlateMakingTableBody .pm-plate-row').forEach(row => {
            const activityEl = row.querySelector('.pm-activity-name');
            const activity   = activityEl?.tagName === 'INPUT' ? (activityEl.value || '').trim() : (activityEl?.textContent || '').trim();
            if (!activity) return;

            const partNameEl  = row.querySelector('.pm-part-name-input');
            const partNameSpan = row.querySelector('.pm-part-name');
            const partName    = partNameEl ? (partNameEl.value || '').trim() : (partNameSpan?.dataset.part || partNameSpan?.textContent || '').trim();
            const plateType   = (row.querySelector('.pm-plate-type')?.value || '').trim();
            const colors      = parseInt(row.querySelector('.pm-colors')?.value   || 0) || 0;
            const required    = parseInt(row.querySelector('.pm-required')?.value || 0) || 0;
            const made        = parseInt(row.querySelector('.pm-made')?.value     || 0) || 0;
            const pending     = Math.max(0, required - made);
            const plateMakingId = row.dataset.plateMakingId ? parseInt(row.dataset.plateMakingId) : null;
            const isCompleted   = row.dataset.completed === 'true';
            rows.push({ plateMakingId, activity, partName, plateType, numberOfColors: colors, numberOfPlates: required, platesMade: made, platesPending: pending, isCompleted });
        });
        return rows;
    }

    function collectCheckedRows(forceIsCompleted = null) {
        const rows = [];
        document.querySelectorAll('#pmPlateMakingTableBody .pm-plate-row').forEach(row => {
            const chk = row.querySelector('.pm-row-check');
            if (!chk || !chk.checked) return;

            const activityEl = row.querySelector('.pm-activity-name');
            const activity   = activityEl?.tagName === 'INPUT' ? (activityEl.value || '').trim() : (activityEl?.textContent || '').trim();
            if (!activity) return;

            const partNameEl   = row.querySelector('.pm-part-name-input');
            const partNameSpan = row.querySelector('.pm-part-name');
            const partName     = partNameEl ? (partNameEl.value || '').trim() : (partNameSpan?.dataset.part || partNameSpan?.textContent || '').trim();
            const plateType    = (row.querySelector('.pm-plate-type')?.value || '').trim();
            const colors       = parseInt(row.querySelector('.pm-colors')?.value   || 0) || 0;
            const required     = parseInt(row.querySelector('.pm-required')?.value || 0) || 0;
            const made         = parseInt(row.querySelector('.pm-made')?.value     || 0) || 0;
            const pending      = Math.max(0, required - made);
            const plateMakingId = row.dataset.plateMakingId ? parseInt(row.dataset.plateMakingId) : null;
            const isCompleted   = forceIsCompleted !== null ? forceIsCompleted : (row.dataset.completed === 'true');
            rows.push({ plateMakingId, activity, partName, plateType, numberOfColors: colors, numberOfPlates: required, platesMade: made, platesPending: pending, isCompleted, _row: row });
        });
        return rows;
    }

    function collectSingleRow(row, isCompleted = false) {
        const activityEl = row.querySelector('.pm-activity-name');
        const activity   = activityEl?.tagName === 'INPUT' ? (activityEl.value || '').trim() : (activityEl?.textContent || '').trim();
        if (!activity) return null;

        const partNameEl   = row.querySelector('.pm-part-name-input');
        const partNameSpan = row.querySelector('.pm-part-name');
        const partName     = partNameEl ? (partNameEl.value || '').trim() : (partNameSpan?.dataset.part || partNameSpan?.textContent || '').trim();
        const plateType    = (row.querySelector('.pm-plate-type')?.value || '').trim();
        const colors       = parseInt(row.querySelector('.pm-colors')?.value   || 0) || 0;
        const required     = parseInt(row.querySelector('.pm-required')?.value || 0) || 0;
        const made         = parseInt(row.querySelector('.pm-made')?.value     || 0) || 0;
        const pending      = Math.max(0, required - made);
        const plateMakingId = row.dataset.plateMakingId ? parseInt(row.dataset.plateMakingId) : null;
        return { plateMakingId, activity, partName, plateType, numberOfColors: colors, numberOfPlates: required, platesMade: made, platesPending: pending, isCompleted };
    }

    // ── Upsert rows ───────────────────────────────────────────────────────

    async function upsertRows(rows, notes = '') {
        if (!rows || rows.length === 0) return;
        const payload = {
            notes,
            rows: rows.map(r => ({
                plateMakingId:  r.plateMakingId || null,
                activity:       r.activity,
                partName:       r.partName || null,
                plateType:      r.plateType || null,
                numberOfColors: r.numberOfColors,
                numberOfPlates: r.numberOfPlates,
                platesMade:     r.platesMade,
                platesPending:  r.platesPending,
                isCompleted:    r.isCompleted
            }))
        };
        const result = await fetchJson(`/api/workspace/plate-making/${_taskId}/upsert`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });
        stampPlateMakingIds(result?.entries);
        return result;
    }

    function stampPlateMakingIds(entries) {
        if (!entries || !entries.length) return;
        const tbody = document.getElementById('pmPlateMakingTableBody');
        if (!tbody) return;
        entries.forEach(entry => {
            tbody.querySelectorAll('.pm-plate-row').forEach(row => {
                if (row.dataset.plateMakingId) return;
                const activityEl = row.querySelector('.pm-activity-name');
                const name = activityEl?.tagName === 'INPUT'
                    ? (activityEl.value || '').trim()
                    : (activityEl?.textContent || '').trim();
                if (name.toLowerCase() === (entry.activityName || '').toLowerCase()) {
                    row.dataset.plateMakingId = entry.plateMakingId;
                }
            });
        });
    }

    // ── Issued plates from store ──────────────────────────────────────────

    async function loadIssuedPlates() {
        const body = $('pmIssuedPlatesBody');
        if (!body) return;

        try {
            const data = await fetchJson(`/api/workspace/plate-making/${_taskId}/store-check`);
            const items = data?.items || [];

            if (items.length === 0) {
                body.innerHTML = `
                    <div class="text-center py-3 text-secondary" style="font-size:.82rem;">
                        <i class="bi bi-inbox me-1"></i>No plates issued from store for this job.
                    </div>`;
                return;
            }

            const totalQty = data.totalIssued ?? items.reduce((s, i) => s + (i.issuedQty || 0), 0);

            let html = `<div class="pm-issued-plates-list">`;

            // Group by forPart
            const byPart = {};
            items.forEach(it => {
                const part = it.forPart || 'General';
                if (!byPart[part]) byPart[part] = [];
                byPart[part].push(it);
            });

            Object.entries(byPart).forEach(([part, rows]) => {
                html += `<div class="pm-issued-part-group">`;
                html += `<div class="pm-issued-part-label"><i class="bi bi-box-seam me-1"></i>${esc(part)}</div>`;
                rows.forEach(it => {
                    html += `
                        <div class="pm-issued-plate-row">
                            <div class="pm-issued-plate-name">${esc(it.materialName)}</div>
                            <div class="pm-issued-plate-meta">
                                ${it.materialCode ? `<span class="text-secondary">${esc(it.materialCode)}</span>` : ''}
                                ${it.specification ? `<span class="text-secondary">· ${esc(it.specification)}</span>` : ''}
                            </div>
                            <div class="d-flex justify-content-between align-items-center mt-1">
                                <span class="badge bg-teal-lt text-teal fw-semibold">${it.issuedQty} ${esc(it.uom || 'Pcs')}</span>
                                <span class="text-secondary" style="font-size:.72rem;">${esc(it.issueNo)} · ${esc(it.issueDate)}</span>
                            </div>
                        </div>`;
                });
                html += `</div>`;
            });

            html += `</div>`;
            html += `
                <div class="px-3 py-2 border-top d-flex justify-content-between align-items-center" style="font-size:.82rem;">
                    <span class="text-secondary"><i class="bi bi-stack me-1"></i>${items.length} item(s)</span>
                    <span class="fw-bold text-teal">${totalQty} plates total</span>
                </div>`;

            body.innerHTML = html;
        } catch {
            body.innerHTML = `<div class="text-center py-2 text-secondary" style="font-size:.82rem;"><i class="bi bi-exclamation-circle me-1"></i>Could not load store data.</div>`;
        }
    }

    function issuedPlatesSummaryHtml(data) {
        const items = data?.items || [];
        if (items.length === 0)
            return '<div class="text-secondary" style="font-size:.82rem;"><i class="bi bi-inbox me-1"></i>No plates issued from store for this job.</div>';

        const totalQty = data.totalIssued ?? items.reduce((s, i) => s + (i.issuedQty || 0), 0);
        let rows = items.map(it =>
            `<tr><td>${esc(it.forPart || '—')}</td><td>${esc(it.materialName)}</td><td class="text-end fw-semibold">${it.issuedQty} ${esc(it.uom || 'Pcs')}</td></tr>`
        ).join('');
        return `
            <div class="mb-1" style="font-size:.8rem;font-weight:600;"><i class="bi bi-boxes me-1"></i>Issued Plates from Store</div>
            <table class="table table-sm table-bordered mb-1" style="font-size:.78rem;">
                <thead class="table-light"><tr><th>Part</th><th>Material</th><th class="text-end">Qty</th></tr></thead>
                <tbody>${rows}</tbody>
                <tfoot><tr><td colspan="2" class="text-end fw-semibold">Total</td><td class="text-end fw-bold text-teal">${totalQty}</td></tr></tfoot>
            </table>`;
    }

    // ── Load task detail ──────────────────────────────────────────────────

    async function load() {
        try {
            const data = await fetchJson(`/api/workspace/task/${_taskId}/process-detail`);
            _task = data.task;
            renderSummary(data);
        } catch (err) {
            try { Swal.fire({ icon: 'error', title: 'Load Failed', text: err?.message || 'Unable to load plate making details.' }); } catch { }
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

        const allRows = collectPlateRows();

        // Fetch issued plates from store to show in the start confirmation
        let storeCheckHtml = '';
        try {
            const storeData = await fetchJson(`/api/workspace/plate-making/${_taskId}/store-check`);
            storeCheckHtml = issuedPlatesSummaryHtml(storeData);
        } catch {
            storeCheckHtml = '<div class="text-secondary" style="font-size:.82rem;"><i class="bi bi-exclamation-circle me-1"></i>Could not load store data.</div>';
        }

        try {
            const res = await Swal.fire({
                title: 'Start plate making step?',
                html: `
                    <p class="mb-2">Start <strong>${esc(_task?.processName || 'Plate Making')}</strong>?<br>
                    <small class="text-muted">${allRows.length} activity row(s) will be saved.</small></p>
                    <div class="text-start border rounded p-2 bg-light">${storeCheckHtml}</div>`,
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Yes, start',
                cancelButtonText: 'Cancel'
            });
            if (!res.isConfirmed) return;

            const btn = $('pmStartBtn');
            setButtonLoading(btn, true, 'Starting...');
            try {
                const notes = $('pmNotes')?.value || '';
                await upsertRows(allRows, notes);
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
        const notes    = $('pmNotes')?.value || '';
        const plateRows = collectPlateRows();

        const btn = $('pmSaveBtn');
        setButtonLoading(btn, true, 'Saving...');
        try {
            await upsertRows(plateRows, notes);
            Swal.fire({ icon: 'success', title: 'Saved', timer: 1000, showConfirmButton: false });
        } catch (err) {
            try { Swal.fire({ icon: 'error', title: 'Save Failed', text: err?.message || 'Unable to save progress.' }); } catch { alert('Save failed.'); }
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

        const incompleteRows = document.querySelectorAll('#pmPlateMakingTableBody .pm-plate-row:not([data-completed="true"])');
        if (incompleteRows.length > 0) {
            Swal.fire({
                icon: 'warning',
                title: 'Tasks Incomplete',
                text: `${incompleteRows.length} task(s) are not yet completed. Use the Complete button on each row to mark them done.`
            });
            return;
        }

        const plateRows     = collectPlateRows();
        const totalRequired = plateRows.reduce((s, r) => s + r.numberOfPlates, 0);
        const totalMade     = plateRows.reduce((s, r) => s + r.platesMade,     0);
        const totalPending  = Math.max(0, totalRequired - totalMade);

        if (totalPending > 0) {
            Swal.fire({
                icon: 'warning',
                title: 'Plates Pending',
                text: `${totalPending} plate(s) still pending. All plates must be made before finishing.`
            });
            return;
        }

        const checkedRows = collectCheckedRows(true);
        if (checkedRows.length === 0) {
            try { Swal.fire({ icon: 'warning', title: 'No Selection', text: 'Please check at least one activity row before completing.' }); } catch { alert('Select at least one row.'); }
            return;
        }

        const notes = $('pmNotes')?.value || '';

        try {
            const res = await Swal.fire({
                title: 'Complete plate making step?',
                html: `All plate making activities will be marked as done.<br><span class="text-muted" style="font-size:.85rem;">${checkedRows.length} checked row(s) will be saved as completed.</span>`,
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Yes, complete',
                cancelButtonText: 'Cancel',
                confirmButtonColor: '#10b981'
            });
            if (!res.isConfirmed) return;

            const btn = $('pmCompleteBtn');
            setButtonLoading(btn, true, 'Completing...');
            try {
                await upsertRows(checkedRows, notes);
                await fetchJson(`/api/workspace/task/${_taskId}/complete`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ remarks: notes })
                });
                Swal.fire({ icon: 'success', title: 'Step Completed!', text: 'Plate making marked as complete.', timer: 1800, showConfirmButton: false });
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

        bindTableInputs();
        bindCompleteButtons();
        bindUpdateButtons();
        updateTotals();

        $('pmRefreshBtn')?.addEventListener('click', () => load());
        $('pmRefreshPlatesBtn')?.addEventListener('click', loadIssuedPlates);
        $('pmStartBtn')?.addEventListener('click',    startStep);
        $('pmSaveBtn')?.addEventListener('click',     saveProgress);
        $('pmCompleteBtn')?.addEventListener('click', completeStep);

        load();
        loadIssuedPlates();
    }

    return { init, addCustomRow };
})();
