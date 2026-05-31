/**
 * Workspace Print Work — Printing Process Data Entry
 * Handles the per-part printing table, machine select, sheets tracking,
 * start/save/complete actions, and task summary rendering.
 */
const PrintWork = (() => {
    'use strict';

    let _taskId = 0;
    let _task = null;
    let _flow = null;
    let _machines = [];     // [{machineId, machineName}]
    let _rows = [];         // current table rows (loaded from server or built from parts)
    let _isPrintOnly = false;
    let _printSizes = [];   // [{productsizeid, sizename, widthmm, heightmm}]

    const PRINT_ONLY_CODES = ['PRINT_OFFSET', 'PRINT_DIGITAL', 'PRINT_SCREEN', 'PRINT_FLEX', 'PRINT_UV'];

    const $ = id => document.getElementById(id);

    // ── Helpers ────────────────────────────────────────────────
    function esc(v) {
        return (v || '').toString()
            .replaceAll('&', '&amp;')
            .replaceAll('<', '&lt;')
            .replaceAll('>', '&gt;')
            .replaceAll('"', '&quot;')
            .replaceAll("'", '&#39;');
    }

    function getCsrfToken() {
        const el = document.querySelector('input[name="__RequestVerificationToken"]');
        return el ? el.value : '';
    }

    async function fetchJson(url, opts = {}) {
        opts.headers = opts.headers || {};
        const method = (opts.method || 'GET').toUpperCase();
        if (method !== 'GET') {
            const token = getCsrfToken();
            if (token) opts.headers['RequestVerificationToken'] = token;
        }
        const r = await fetch(url, opts);
        if (!r.ok) {
            const raw = await r.text();
            let msg = '';
            try { msg = JSON.parse(raw)?.message || raw; } catch { msg = raw; }
            throw new Error(msg || `HTTP ${r.status}`);
        }
        if (r.status === 204) return null;
        return r.json();
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
            btn.disabled = true;
            btn.innerHTML = `<span class="spinner-border spinner-border-sm me-1"></span>${esc(label || 'Loading...')}`;
        } else {
            btn.disabled = false;
            btn.innerHTML = btn.dataset.origHtml || btn.innerHTML;
        }
    }

    // ── Option builders ────────────────────────────────────────
    function methodOptions(selected) {
        const methods = [
            { val: 'OFFSET',  label: 'Offset Printing' },
            { val: 'DIGITAL', label: 'Digital Printing' },
            { val: 'SCREEN',  label: 'Screen Printing' }
        ];
        let opts = '<option value="">— Select Method —</option>';
        methods.forEach(m => {
            const sel = (selected || '').toUpperCase() === m.val ? ' selected' : '';
            opts += `<option value="${m.val}"${sel}>${m.label}</option>`;
        });
        return opts;
    }

    function colorsOptions(selected) {
        let opts = '<option value="">— Select —</option>';
        for (let i = 1; i <= 10; i++) {
            const sel = parseInt(selected) === i ? ' selected' : '';
            opts += `<option value="${i}"${sel}>${i} Color${i > 1 ? 's' : ''}</option>`;
        }
        return opts;
    }

    function platesOptions(selected) {
        let opts = '<option value="">— Select —</option>';
        for (let i = 1; i <= 24; i++) {
            const sel = parseInt(selected) === i ? ' selected' : '';
            opts += `<option value="${i}"${sel}>${i} Plate${i > 1 ? 's' : ''}</option>`;
        }
        return opts;
    }

    // ── Print-Only option builders ─────────────────────────────
    function printSizeOptions(selected) {
        let opts = '<option value="">— Select Size —</option>';
        _printSizes.forEach(ps => {
            const sel = (selected && ps.productsizeid == selected) ? ' selected' : '';
            opts += `<option value="${ps.productsizeid}"${sel}>${esc(ps.sizename)} (${ps.widthmm}×${ps.heightmm} mm)</option>`;
        });
        return opts;
    }

    function printSideOptions(selected) {
        const sides = [{ val: '1', label: 'Single Side' }, { val: '2', label: 'Double Side' }];
        let opts = '';
        sides.forEach(s => {
            const sel = (selected || '1') === s.val ? ' selected' : '';
            opts += `<option value="${s.val}"${sel}>${s.label}</option>`;
        });
        return opts;
    }

    function printColorsOptions(selected) {
        let opts = '<option value="">— Select —</option>';
        for (let i = 1; i <= 6; i++) {
            const labels = { 1: '1 Color', 2: '2 Colors', 3: '3 Colors', 4: '4 Colors (CMYK)', 5: '5 Colors', 6: '6 Colors' };
            const sel = parseInt(selected) === i ? ' selected' : '';
            opts += `<option value="${i}"${sel}>${labels[i]}</option>`;
        }
        return opts;
    }

    function machineOptions(selectedId) {
        let opts = '<option value="">— Select Machine —</option>';
        _machines.forEach(m => {
            const sel = (selectedId && m.machineId == selectedId) ? ' selected' : '';
            opts += `<option value="${m.machineId}"${sel}>${esc(m.machineName)}</option>`;
        });
        return opts;
    }

    // ── updateTableHeaders: no-op (vertical cards have no thead) ─
    function updateTableHeaders() { /* vertical card layout — no header swap needed */ }

    // ── Init Select2 on card selects ──────────────────────────
    function initRowSelects(container) {
        if (!window.jQuery) return;
        const jQ = window.jQuery;
        const selectors = _isPrintOnly
            ? '.prw-printsize-select, .prw-machine-select, .prw-printside-select, .prw-printcolors-select'
            : '.prw-method-select, .prw-machine-select, .prw-colors-select, .prw-plates-select';
        container.querySelectorAll(selectors).forEach(el => {
            if (jQ(el).data('select2')) return;
            jQ(el).select2({ width: '100%', dropdownParent: jQ('body'), minimumResultsForSearch: 3 });
        });
    }

    // ── Build a single vertical part card ──────────────────────
    function buildPartCard(row, index) {
        const id = row.printWorkId || `new-${index}`;
        const checked = row.isSelected ? 'checked' : '';
        const required = row.totalSheetsRequired || 0;
        const printed  = row.totalSheetsPrinted  || 0;
        const balance  = required - printed;
        const balClass = balance < 0 ? 'text-danger' : balance === 0 && required > 0 ? 'text-success' : '';

        const startBtnHtml = row.isStarted
            ? `<button class="btn btn-sm btn-success prw-row-start-btn" data-row-id="${id}" data-started="true" disabled>
                   <i class="bi bi-play-circle-fill me-1"></i>Started
               </button>`
            : `<button class="btn btn-sm btn-outline-primary prw-row-start-btn" data-row-id="${id}" data-started="false">
                   <i class="bi bi-play-circle me-1"></i>Start
               </button>`;

        const fieldsHtml = _isPrintOnly ? `
            <div class="prw-field-row">
                <label>Print Size</label>
                <select class="form-select form-select-sm prw-printsize-select">${printSizeOptions(row.printSizeId)}</select>
            </div>
            <div class="prw-field-row">
                <label>Machine</label>
                <select class="form-select form-select-sm prw-machine-select">${machineOptions(row.machineId)}</select>
            </div>
            <div class="prw-field-row">
                <label>Printing Side</label>
                <select class="form-select form-select-sm prw-printside-select">${printSideOptions(row.printSide)}</select>
            </div>
            <div class="prw-field-row">
                <label>No. of Colors</label>
                <select class="form-select form-select-sm prw-printcolors-select">${printColorsOptions(row.numberOfColors)}</select>
            </div>
            <div class="prw-field-row">
                <label>Plates Received</label>
                <input type="number" class="form-control form-control-sm text-center prw-plates-received" value="${row.platesReceived || 0}" min="0" placeholder="0">
            </div>` : `
            <div class="prw-field-row">
                <label>Printing Method</label>
                <select class="form-select form-select-sm prw-method-select">${methodOptions(row.printingMethod)}</select>
            </div>
            <div class="prw-field-row">
                <label>Machine</label>
                <select class="form-select form-select-sm prw-machine-select">${machineOptions(row.machineId)}</select>
            </div>
            <div class="prw-field-row">
                <label>No. of Colors</label>
                <select class="form-select form-select-sm prw-colors-select">${colorsOptions(row.numberOfColors)}</select>
            </div>
            <div class="prw-field-row">
                <label>No. of Plates</label>
                <select class="form-select form-select-sm prw-plates-select">${platesOptions(row.numberOfPlates)}</select>
            </div>
            <div class="prw-field-row">
                <label>SL No From</label>
                <input type="number" class="form-control form-control-sm text-center prw-sl-from" value="${row.slNoFrom || ''}" min="1" placeholder="From">
            </div>
            <div class="prw-field-row">
                <label>SL No To</label>
                <input type="number" class="form-control form-control-sm text-center prw-sl-to" value="${row.slNoTo || ''}" min="1" placeholder="To">
            </div>`;

        return `
        <div class="prw-part-panel" data-row-id="${id}" data-index="${index}">
            <div class="prw-part-panel-header">
                <div class="d-flex align-items-center gap-2 flex-grow-1 min-w-0">
                    <input type="checkbox" class="form-check-input flex-shrink-0 prw-row-check" ${checked}>
                    <span class="prw-part-name fw-semibold text-truncate">${esc(row.partName || '—')}</span>
                </div>
                <div class="d-flex gap-2 align-items-center flex-shrink-0">
                    ${startBtnHtml}
                    <button class="btn btn-sm btn-outline-danger prw-row-delete-btn" data-action="delete-row" title="Remove part">
                        <i class="bi bi-trash"></i>
                    </button>
                </div>
            </div>
            <div class="prw-part-fields">
                ${fieldsHtml}
                <div class="prw-field-row prw-field-sheets">
                    <label>Total Sheets Required</label>
                    <input type="number" class="form-control form-control-sm text-center prw-required" value="${required}" min="0">
                </div>
                <div class="prw-field-row prw-field-sheets">
                    <label>Total Sheets Printed</label>
                    <input type="number" class="form-control form-control-sm text-center prw-printed" value="${printed}" min="0">
                </div>
                <div class="prw-field-row prw-field-balance">
                    <label>Balance</label>
                    <div class="prw-balance-display ${balClass}" data-balance="${balance}">${balance.toLocaleString('en-IN')}</div>
                </div>
            </div>
        </div>`;
    }

    // ── Navigation state ───────────────────────────────────────
    let _currentPartIndex = 0;

    function showPart(index) {
        const panels = document.querySelectorAll('.prw-part-panel');
        if (!panels.length) return;
        _currentPartIndex = Math.max(0, Math.min(index, panels.length - 1));
        panels.forEach((p, i) => p.classList.toggle('d-none', i !== _currentPartIndex));
        // Update tab pills
        document.querySelectorAll('.prw-nav-tab').forEach((t, i) => {
            t.classList.toggle('active', i === _currentPartIndex);
        });
        // Update prev/next state
        const first = document.getElementById('prwNavFirst');
        const prev  = document.getElementById('prwNavPrev');
        const next  = document.getElementById('prwNavNext');
        const last  = document.getElementById('prwNavLast');
        if (first) first.disabled = _currentPartIndex === 0;
        if (prev)  prev.disabled  = _currentPartIndex === 0;
        if (next)  next.disabled  = _currentPartIndex >= panels.length - 1;
        if (last)  last.disabled  = _currentPartIndex >= panels.length - 1;
    }

    function buildNavTabs(count) {
        const strip  = document.getElementById('prwNavStrip');
        const tabsEl = document.getElementById('prwNavTabs');
        if (!strip || !tabsEl) return;
        if (count <= 1) { strip.classList.add('d-none'); return; }
        strip.classList.remove('d-none');
        tabsEl.innerHTML = Array.from({ length: count }, (_, i) =>
            `<button class="btn btn-sm prw-nav-tab ${i === 0 ? 'btn-primary active' : 'btn-outline-secondary'}" data-part="${i}">Part ${i + 1}</button>`
        ).join('');
        tabsEl.querySelectorAll('.prw-nav-tab').forEach(btn => {
            btn.addEventListener('click', () => showPart(parseInt(btn.dataset.part)));
        });
    }

    // ── Render all part cards ──────────────────────────────────
    function renderCards(rows) {
        const container = document.getElementById('prwPartsContainer');
        const loading   = document.getElementById('prwLoadingMsg');
        if (!container) return;
        if (loading) loading.remove();

        if (!rows || rows.length === 0) {
            container.innerHTML = `<div class="text-center py-5 text-secondary">
                <i class="bi bi-inbox fs-2 d-block mb-2"></i>
                No parts found. Use <b>Add Part</b> to add a custom part.
            </div>`;
            buildNavTabs(0);
            updateTotals();
            return;
        }

        container.innerHTML = rows.map((r, i) => buildPartCard(r, i)).join('');
        buildNavTabs(rows.length);
        showPart(0);
        initRowSelects(container);
        bindRowEvents(container);
        updateTotals();
    }

    // keep alias for callers that use renderTable
    function renderTable(rows) { renderCards(rows); }

    // ── Bind input events within a card container ──────────────
    function bindRowEvents(container) {
        container.querySelectorAll('.prw-required, .prw-printed').forEach(inp => {
            inp.addEventListener('input', () => {
                const panel = inp.closest('.prw-part-panel');
                const req = parseInt(panel.querySelector('.prw-required').value) || 0;
                const prt = parseInt(panel.querySelector('.prw-printed').value) || 0;
                const bal = req - prt;
                const balEl = panel.querySelector('.prw-balance-display');
                if (balEl) {
                    balEl.textContent = bal.toLocaleString('en-IN');
                    balEl.dataset.balance = bal;
                    balEl.className = 'prw-balance-display' + (bal < 0 ? ' text-danger' : bal === 0 && req > 0 ? ' text-success' : '');
                }
                updateTotals();
                updateProgressBar();
            });
        });

        // Delete button
        container.querySelectorAll('[data-action="delete-row"]').forEach(btn => {
            btn.addEventListener('click', () => {
                const panel = btn.closest('.prw-part-panel');
                const idx = parseInt(panel.dataset.index);
                panel.remove();
                // Rebuild nav tabs
                const remaining = document.querySelectorAll('.prw-part-panel');
                buildNavTabs(remaining.length);
                showPart(Math.min(idx, remaining.length - 1));
                updateTotals();
                updateProgressBar();
            });
        });

        // Per-panel Start button
        container.querySelectorAll('.prw-row-start-btn').forEach(btn => {
            if (btn.dataset.started === 'true') return;
            btn.addEventListener('click', () => startPartRow(btn));
        });

        // Totals update on select change
        container.querySelectorAll('.prw-plates-select, .prw-printcolors-select, .prw-plates-received, .prw-printside-select, .prw-colors-select').forEach(sel => {
            sel.addEventListener('change', updateTotals);
        });

        // Select-all
        const selectAll = $('prwSelectAll');
        if (selectAll) {
            // remove old listener by cloning
            const fresh = selectAll.cloneNode(true);
            selectAll.replaceWith(fresh);
            fresh.addEventListener('change', () => {
                document.querySelectorAll('.prw-row-check').forEach(cb => { cb.checked = fresh.checked; });
            });
        }
    }

    // ── Totals bar update ──────────────────────────────────────
    function updateTotals() {
        let totalPlates = 0, totalColors = 0, totalRequired = 0, totalPrinted = 0, totalBalance = 0;

        document.querySelectorAll('.prw-part-panel[data-row-id]').forEach(panel => {
            if (_isPrintOnly) {
                totalPlates  += parseInt(panel.querySelector('.prw-plates-received')?.value || 0);
                totalColors  += parseInt(panel.querySelector('.prw-printcolors-select')?.value || 0);
            } else {
                totalPlates  += parseInt(panel.querySelector('.prw-plates-select')?.value || 0);
            }
            const req = parseInt(panel.querySelector('.prw-required')?.value || 0);
            const prt = parseInt(panel.querySelector('.prw-printed')?.value  || 0);
            totalRequired += req;
            totalPrinted  += prt;
            totalBalance  += (req - prt);
        });

        const fmt = n => n.toLocaleString('en-IN');
        if ($('prwTotalColors'))   $('prwTotalColors').textContent   = _isPrintOnly ? fmt(totalColors) : '—';
        if ($('prwTotalPlates'))   $('prwTotalPlates').textContent   = fmt(totalPlates);
        if ($('prwTotalRequired')) $('prwTotalRequired').textContent = fmt(totalRequired);
        if ($('prwTotalPrinted'))  $('prwTotalPrinted').textContent  = fmt(totalPrinted);
        if ($('prwTotalBalance'))  $('prwTotalBalance').textContent  = fmt(totalBalance);

        // Sidebar summary
        if ($('prwSummaryRequired')) $('prwSummaryRequired').textContent = fmt(totalRequired);
        if ($('prwSummaryPrinted'))  $('prwSummaryPrinted').textContent  = fmt(totalPrinted);
        if ($('prwSummaryBalance'))  $('prwSummaryBalance').textContent  = fmt(totalBalance);

        updateProgressBar(totalRequired, totalPrinted);
        renderPrintSummary();
    }

    function updateProgressBar(req, prt) {
        if (req === undefined) {
            req = 0; prt = 0;
            document.querySelectorAll('.prw-part-panel[data-row-id]').forEach(panel => {
                req += parseInt(panel.querySelector('.prw-required')?.value || 0);
                prt += parseInt(panel.querySelector('.prw-printed')?.value  || 0);
            });
        }
        const pct = req > 0 ? Math.min(100, Math.round((prt / req) * 100)) : 0;
        if ($('prwProgressBar')) $('prwProgressBar').style.width = pct + '%';
        if ($('prwProgressPct')) $('prwProgressPct').textContent = pct + '%';
    }

    // ── Collect current data from all part cards ───────────────
    function collectRows() {
        const rows = [];
        document.querySelectorAll('.prw-part-panel[data-row-id]').forEach((panel, i) => {
            const rowId = panel.dataset.rowId;
            const nameEl = panel.querySelector('.prw-part-name-input');
            const partName = nameEl ? nameEl.value.trim() : (panel.querySelector('.prw-part-name')?.textContent.trim() || '—');
            const methodSelect  = panel.querySelector('.prw-method-select');
            const machineSelect = panel.querySelector('.prw-machine-select');

            if (_isPrintOnly) {
                rows.push({
                    printWorkId:         rowId?.startsWith('new-') ? null : parseInt(rowId),
                    partName,
                    partSequence:        i + 1,
                    printSizeId:         parseInt(panel.querySelector('.prw-printsize-select')?.value  || 0) || null,
                    printSide:           panel.querySelector('.prw-printside-select')?.value || '1',
                    numberOfColors:      parseInt(panel.querySelector('.prw-printcolors-select')?.value || 0) || 0,
                    platesReceived:      parseInt(panel.querySelector('.prw-plates-received')?.value  || 0) || 0,
                    machineId:           parseInt(machineSelect?.value || 0) || null,
                    totalSheetsRequired: parseInt(panel.querySelector('.prw-required')?.value || 0) || 0,
                    totalSheetsPrinted:  parseInt(panel.querySelector('.prw-printed')?.value  || 0) || 0,
                    isSelected:          panel.querySelector('.prw-row-check')?.checked || false,
                });
            } else {
                rows.push({
                    printWorkId:         rowId?.startsWith('new-') ? null : parseInt(rowId),
                    partName,
                    partSequence:        i + 1,
                    printingMethod:      methodSelect?.value || null,
                    machineId:           parseInt(machineSelect?.value || 0) || null,
                    numberOfColors:      parseInt(panel.querySelector('.prw-colors-select')?.value  || 0) || 0,
                    numberOfPlates:      parseInt(panel.querySelector('.prw-plates-select')?.value  || 0) || 0,
                    slNoFrom:            parseInt(panel.querySelector('.prw-sl-from')?.value || 0) || null,
                    slNoTo:              parseInt(panel.querySelector('.prw-sl-to')?.value   || 0) || null,
                    totalSheetsRequired: parseInt(panel.querySelector('.prw-required')?.value || 0) || 0,
                    totalSheetsPrinted:  parseInt(panel.querySelector('.prw-printed')?.value  || 0) || 0,
                    isSelected:          panel.querySelector('.prw-row-check')?.checked || false,
                });
            }
        });
        return rows;
    }

    // ── Task Summary ───────────────────────────────────────────
    function renderSummary(data) {
        const t = data.task || {};
        const el = $('prwTaskSummary');
        if (!el) return;
        el.innerHTML = `
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

    function renderAllocation(data) {
        const wrap = $('prwAllocationWrap');
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
        if ($('prwJobNo')) $('prwJobNo').textContent = `Job: ${t.jobNo || '—'}`;
        if ($('prwPartyName')) $('prwPartyName').textContent = `Customer: ${t.partyName || '—'}`;
        if ($('prwProcessName')) $('prwProcessName').textContent = `Process: ${t.processName || '—'}`;
        if ($('prwPriority')) $('prwPriority').textContent = `Priority: ${t.priority || 'Normal'}`;
    }

    // ── Part-wise execution sidebar ────────────────────────────
    function renderPartsWrap(rows) {
        const el = $('prwPartsWrap');
        if (!el) return;
        if (!rows || rows.length === 0) {
            el.innerHTML = '<div class="text-secondary small">No parts loaded.</div>';
            return;
        }
        el.innerHTML = rows.map(r => `
            <div class="d-flex align-items-center justify-content-between py-1 border-bottom">
                <span class="small fw-medium">${esc(r.partName)}</span>
                ${r.isStarted
                    ? '<span class="badge bg-success-lt text-success"><i class="bi bi-play-circle me-1"></i>Started</span>'
                    : '<span class="badge bg-secondary-lt text-secondary">Pending</span>'}
            </div>`).join('');
    }

    // ── Per-row start action ───────────────────────────────────
    async function startPartRow(btn) {
        const rowId = btn.dataset.rowId;
        const panel = btn.closest('.prw-part-panel');
        const partName = panel.querySelector('.prw-part-name')?.textContent.trim()
                      || panel.querySelector('.prw-part-name-input')?.value.trim()
                      || 'this part';

        const confirm = await Swal.fire({
            title: `Start printing for "${partName}"?`,
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: 'Yes, Start',
            cancelButtonText: 'Cancel'
        });
        if (!confirm.isConfirmed) return;

        Swal.fire({ title: 'Please wait...', allowOutsideClick: false, didOpen: () => Swal.showLoading() });
        try {
            if (rowId && !rowId.startsWith('new-')) {
                await fetchJson(`/api/workspace/print-work/${rowId}/start-part`, { method: 'POST', headers: { 'Content-Type': 'application/json' } });
            }
            // Update UI immediately
            btn.outerHTML = `<button class="btn btn-sm btn-success py-0 px-2 prw-row-start-btn" data-row-id="${rowId}" data-started="true" title="Started" disabled>
                <i class="bi bi-play-circle-fill me-1"></i>Started
            </button>`;
            Swal.fire({ icon: 'success', title: 'Started', timer: 1000, showConfirmButton: false });
        } catch (e) {
            Swal.close();
            Swal.fire({ icon: 'error', title: 'Failed', text: e.message });
        }
    }

    // ── Main Load ──────────────────────────────────────────────
    async function load() {
        try {
            const [data, flow, machineRes, printRows, allocation] = await Promise.all([
                fetchJson(`/api/workspace/task/${_taskId}/process-detail`),
                fetchJson(`/api/workspace/process-flow/${_taskId}`).catch(() => null),
                fetchJson(`/api/workspace/print-machines`).catch(() => []),
                fetchJson(`/api/workspace/print-work/${_taskId}`).catch(() => []),
                fetchJson(`/api/workspace/task/${_taskId}/allocation`).catch(() => null)
            ]);

            _task = data.task;
            _flow = flow;
            _machines = machineRes || [];

            // Detect print-only job type
            const jobTypeCode = (data?.job?.jobTypeCode || '').toUpperCase();
            _isPrintOnly = PRINT_ONLY_CODES.includes(jobTypeCode) || (data?.job?.isSingleProcess === true);

            // Load print sizes when needed
            if (_isPrintOnly && _printSizes.length === 0) {
                _printSizes = await fetchJson('/api/rate-calculator/productsizes').catch(() => []);
            }

            updateTableHeaders();
            renderSummary(data);
            renderQuickInfo(data);
            renderAllocation(allocation);

            // Build rows: use saved entries if available, otherwise build from job parts
            let rows = Array.isArray(printRows) && printRows.length > 0
                ? printRows
                : buildRowsFromJobParts(data);

            _rows = rows;
            renderTable(rows);
            renderPartsWrap(rows);
            updateProgressBar();

        } catch (err) {
            Swal.fire({ icon: 'error', title: 'Load Failed', text: err?.message || 'Unable to load print work details.' });
        }
    }

    // ── Build default rows from job config parts ───────────────
    function buildRowsFromJobParts(data) {
        const parts = data?.job?.productParts || [];
        if (_isPrintOnly) {
            // Print-only jobs are single-process; one row with job's product name
            const productName = data?.job?.productName || 'Print Job';
            return [{ partName: productName, totalSheetsRequired: data?.job?.quantity || 0, totalSheetsPrinted: 0, numberOfColors: 4, platesReceived: 0, printSide: '1' }];
        }
        if (parts.length === 0) {
            // Fallback: default printing parts
            return [
                { partName: 'Cover', totalSheetsRequired: 0, totalSheetsPrinted: 0, numberOfColors: 4, numberOfPlates: 4 },
                { partName: 'Inside B/W', totalSheetsRequired: 0, totalSheetsPrinted: 0, numberOfColors: 1, numberOfPlates: 1 },
                { partName: 'Inside Color', totalSheetsRequired: 0, totalSheetsPrinted: 0, numberOfColors: 4, numberOfPlates: 4 },
                { partName: 'Special Pages', totalSheetsRequired: 0, totalSheetsPrinted: 0, numberOfColors: 4, numberOfPlates: 4 }
            ];
        }
        return parts.map(p => ({
            partName: p.partName || p.name || '—',
            totalSheetsRequired: p.quantity || 0,
            totalSheetsPrinted: 0,
            numberOfColors: p.colors || p.numberOfColors || 0,
            numberOfPlates: p.plates || p.numberOfPlates || 0
        }));
    }

    // ── Start Step (task-level) ────────────────────────────────
    async function startStep() {
        if (!_task) return;
        const taskStatus = (_task.taskStatus || '').toUpperCase();
        if (['IN_PROGRESS', 'RUNNING', 'STARTED'].includes(taskStatus)) {
            Swal.fire({ icon: 'info', title: 'Already Started', text: 'This step is already started.' });
            return;
        }

        const res = await Swal.fire({
            title: 'Start Printing Step?',
            text: 'This will mark the printing task as In Progress.',
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: 'Yes, Start',
            cancelButtonText: 'Cancel'
        });
        if (!res.isConfirmed) return;

        const btn = $('prwStartBtn');
        setButtonLoading(btn, true, 'Starting...');
        Swal.fire({ title: 'Please wait...', allowOutsideClick: false, didOpen: () => Swal.showLoading() });
        try {
            await fetchJson(`/api/workspace/task/${_taskId}/start`, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: '{}' });
            await load();
            Swal.fire({ icon: 'success', title: 'Started', timer: 1200, showConfirmButton: false });
        } catch (err) {
            Swal.close();
            Swal.fire({ icon: 'error', title: 'Start Failed', text: err?.message || 'Unable to start task.' });
        } finally {
            setButtonLoading(btn, false);
        }
    }

    // ── Row-level validation ────────────────────────────────────
    function validateRows(rows) {
        const selected = rows.filter(r => r.isSelected);
        if (selected.length === 0) {
            Swal.fire({ icon: 'warning', title: 'No Parts Selected', text: 'Please select at least one part row before saving.' });
            return false;
        }
        for (const r of selected) {
            if (_isPrintOnly) {
                if (!r.printSizeId) {
                    Swal.fire({ icon: 'warning', title: 'Print Size Required', text: `Please select a print size for part "${r.partName}".` });
                    return false;
                }
                if (!r.machineId) {
                    Swal.fire({ icon: 'warning', title: 'Machine Required', text: `Please select a machine for part "${r.partName}".` });
                    return false;
                }
                if (!r.numberOfColors || r.numberOfColors < 1) {
                    Swal.fire({ icon: 'warning', title: 'Colors Required', text: `Please select number of colors for part "${r.partName}".` });
                    return false;
                }
            } else {
                if (!r.printingMethod) {
                    Swal.fire({ icon: 'warning', title: 'Printing Method Required', text: `Please select a printing method for part "${r.partName}".` });
                    return false;
                }
                if (!r.machineId) {
                    Swal.fire({ icon: 'warning', title: 'Machine Required', text: `Please select a machine for part "${r.partName}".` });
                    return false;
                }
                if (!r.numberOfColors || r.numberOfColors < 1) {
                    Swal.fire({ icon: 'warning', title: 'Colors Required', text: `Please select number of colors for part "${r.partName}".` });
                    return false;
                }
                if (!r.numberOfPlates || r.numberOfPlates < 1) {
                    Swal.fire({ icon: 'warning', title: 'Plates Required', text: `Please select number of plates for part "${r.partName}".` });
                    return false;
                }
            }
            if (!r.totalSheetsRequired || r.totalSheetsRequired < 1) {
                Swal.fire({ icon: 'warning', title: 'Sheets Required', text: `Please enter total sheets required for part "${r.partName}".` });
                return false;
            }
        }
        return true;
    }

    // ── Save Progress ──────────────────────────────────────────
    async function saveProgress() {
        const rows = collectRows();
        if (!validateRows(rows)) return;
        const notes = $('prwNotes')?.value || '';

        const payload = { workspaceTaskId: _taskId, notes, entries: rows };

        const btn = $('prwSaveBtn');
        setButtonLoading(btn, true, 'Saving...');
        Swal.fire({ title: 'Please wait...', allowOutsideClick: false, didOpen: () => Swal.showLoading() });
        try {
            await fetchJson(`/api/workspace/print-work/${_taskId}/save`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });
            Swal.fire({ icon: 'success', title: 'Saved', timer: 1200, showConfirmButton: false });
            await load();
        } catch (err) {
            Swal.close();
            Swal.fire({ icon: 'error', title: 'Save Failed', text: err?.message || 'Unable to save.' });
        } finally {
            setButtonLoading(btn, false);
        }
    }

    // ── Complete Step ──────────────────────────────────────────
    async function completeStep() {
        const taskStatus = (_task?.taskStatus || '').toUpperCase();
        if (taskStatus !== 'IN_PROGRESS') {
            Swal.fire({ icon: 'warning', title: 'Not Started', text: 'Please start the printing step before marking it complete.' });
            return;
        }

        const rows = collectRows();
        if (!validateRows(rows)) return;
        const anyPrinted = rows.some(r => (r.totalSheetsPrinted || 0) > 0);
        if (!anyPrinted) {
            Swal.fire({ icon: 'warning', title: 'No Sheets Printed', text: 'Please enter at least one printed sheet count before completing.' });
            return;
        }

        const notes = $('prwNotes')?.value || '';
        const confirm = await Swal.fire({
            title: 'Complete Printing Step?',
            text: 'This will mark the printing task as Completed and trigger the next workflow step.',
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: 'Yes, Complete',
            cancelButtonText: 'Cancel'
        });
        if (!confirm.isConfirmed) return;

        const btn = $('prwCompleteBtn');
        setButtonLoading(btn, true, 'Completing...');
        Swal.fire({ title: 'Please wait...', allowOutsideClick: false, didOpen: () => Swal.showLoading() });
        try {
            // Save latest data first
            await fetchJson(`/api/workspace/print-work/${_taskId}/save`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ workspaceTaskId: _taskId, notes, entries: rows })
            });
            // Then complete the task
            await fetchJson(`/api/workspace/task/${_taskId}/complete`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ remarks: notes })
            });
            Swal.fire({ icon: 'success', title: 'Step Completed!', timer: 1500, showConfirmButton: false })
                .then(() => window.location.href = '/Workspace/MyTasks');
        } catch (err) {
            Swal.close();
            Swal.fire({ icon: 'error', title: 'Complete Failed', text: err?.message || 'Unable to complete task.' });
        } finally {
            setButtonLoading(btn, false);
        }
    }

    // ── Add custom blank part card ─────────────────────────────
    function addCustomRow() {
        const container = document.getElementById('prwPartsContainer');
        if (!container) return;

        // Remove empty state placeholder if present
        const emptyMsg = container.querySelector('.text-center.py-5');
        if (emptyMsg) container.innerHTML = '';

        const existing = document.querySelectorAll('.prw-part-panel[data-row-id]');
        const idx   = existing.length;
        const rowId = `new-${Date.now()}`;

        const blankRow = _isPrintOnly
            ? { partName: '', printSizeId: null, printSide: '1', numberOfColors: 4, platesReceived: 0, machineId: null, totalSheetsRequired: 0, totalSheetsPrinted: 0, isSelected: true, isStarted: false }
            : { partName: '', printingMethod: null, machineId: null, numberOfColors: 0, numberOfPlates: 0, slNoFrom: null, slNoTo: null, totalSheetsRequired: 0, totalSheetsPrinted: 0, isSelected: true, isStarted: false };

        // Build card HTML — use editable part name input
        const cardHtml = buildPartCard({ ...blankRow, printWorkId: rowId }, idx)
            .replace(
                `<span class="prw-part-name fw-semibold text-truncate">${esc(blankRow.partName || '—')}</span>`,
                `<input type="text" class="form-control form-control-sm prw-part-name-input flex-grow-1" placeholder="Enter part name…" style="max-width:200px;">`
            );

        container.insertAdjacentHTML('beforeend', cardHtml);

        const allPanels = document.querySelectorAll('.prw-part-panel[data-row-id]');
        buildNavTabs(allPanels.length);
        showPart(allPanels.length - 1);

        const newPanel = container.querySelector(`.prw-part-panel[data-row-id="${rowId}"]`);
        if (newPanel) {
            initRowSelects(newPanel);
            bindRowEvents(newPanel);
            newPanel.querySelector('.prw-part-name-input')?.focus();
        }
        updateTotals();
    }

    // ── Init ───────────────────────────────────────────────────
    // ── Print Final Summary (AI Estimate) ────────────────────
    function renderPrintSummary() {
        const el = $('prwPrintSummary');
        if (!el) return;
        // Skip — summary is already rendered server-side from config_data
        if (el.dataset.hasConfig === 'true') return;

        let netSheets = 0, totalColors = 0, totalImpressions = 0;
        let panelCount = 0;

        document.querySelectorAll('.prw-part-panel[data-row-id]').forEach(panel => {
            const req    = parseInt(panel.querySelector('.prw-required')?.value    || 0);
            const sides  = _isPrintOnly
                ? parseInt(panel.querySelector('.prw-printside-select')?.value    || 1)
                : 1;
            const colors = _isPrintOnly
                ? parseInt(panel.querySelector('.prw-printcolors-select')?.value  || 0)
                : parseInt(panel.querySelector('.prw-colors-select')?.value       || 0);

            netSheets        += req;
            totalColors      += colors;
            totalImpressions += req * sides;
            panelCount++;
        });

        if (netSheets === 0) {
            el.innerHTML = `<div class="prw-summary-empty"><i class="bi bi-calculator me-2"></i>Enter sheet counts to see estimates.</div>`;
            return;
        }

        const WASTAGE_PCT   = 10;
        const wastageSheets = Math.ceil(netSheets * WASTAGE_PCT / 100);
        const totalSheets   = netSheets + wastageSheets;
        const scaleFactor   = netSheets > 0 ? totalSheets / netSheets : 1;
        const totalImpFinal = Math.round(totalImpressions * scaleFactor);
        const plates        = totalColors; // 1 plate per color per form

        // Ink estimate: 1.5–2.0 gm per sheet per color (offset standard)
        const inkMin = Math.round(totalSheets * Math.max(1, totalColors) * 1.5);
        const inkMax = Math.round(totalSheets * Math.max(1, totalColors) * 2.0);
        const inkDisplay = inkMin >= 1000
            ? `~${(inkMin / 1000).toFixed(1)}\u2013${(inkMax / 1000).toFixed(1)} kg`
            : `~${inkMin}\u2013${inkMax} gm`;

        const colorLabel = totalColors === 1 ? '1 color' : `${totalColors} colors`;
        const rows = [
            { icon: 'bi-file-earmark-text',  label: 'Net Sheets',              value: netSheets.toLocaleString('en-IN'),     cls: '' },
            { icon: 'bi-percent',            label: `Wastage (${WASTAGE_PCT}%)`,value: wastageSheets.toLocaleString('en-IN'), cls: 'prw-summary-wastage' },
            { icon: 'bi-layers-fill',        label: 'Total Sheets',            value: totalSheets.toLocaleString('en-IN'),   cls: 'prw-summary-total' },
            { icon: 'bi-printer-fill',       label: 'Total Impressions',       value: totalImpFinal.toLocaleString('en-IN'), cls: '' },
            { icon: 'bi-grid-3x3-gap-fill',  label: `Plates (${colorLabel})`,  value: plates > 0 ? plates.toLocaleString('en-IN') : '\u2014', cls: '' },
            { icon: 'bi-droplet-fill',       label: 'Ink Required',            value: inkDisplay,                            cls: 'prw-summary-ink' },
        ];

        el.innerHTML = `
            <div class="prw-summary-ai-badge"><i class="bi bi-cpu-fill me-1"></i>AI Estimate</div>
            <div class="prw-summary-table">
                ${rows.map(r => `
                <div class="prw-summary-row${r.cls ? ' ' + r.cls : ''}">
                    <div class="prw-summary-key"><i class="bi ${r.icon} me-2"></i>${r.label}</div>
                    <div class="prw-summary-val">${r.value}</div>
                </div>`).join('')}
            </div>
            <div class="prw-summary-note"><i class="bi bi-info-circle me-1"></i>Based on 10% press wastage. Ink is an offset estimate.</div>
        `;
    }

    function init(taskId) {
        _taskId = taskId;

        $('prwStartBtn')?.addEventListener('click', startStep);
        $('prwSaveBtn')?.addEventListener('click', saveProgress);
        $('prwCompleteBtn')?.addEventListener('click', completeStep);
        $('prwRefreshBtn')?.addEventListener('click', load);
        $('prwAddRowBtn')?.addEventListener('click', addCustomRow);

        // Navigation buttons
        $('prwNavFirst')?.addEventListener('click', () => showPart(0));
        $('prwNavPrev')?.addEventListener('click',  () => showPart(_currentPartIndex - 1));
        $('prwNavNext')?.addEventListener('click',  () => showPart(_currentPartIndex + 1));
        $('prwNavLast')?.addEventListener('click',  () => {
            const count = document.querySelectorAll('.prw-part-panel').length;
            showPart(count - 1);
        });

        load();
    }

    return { init };
})();
