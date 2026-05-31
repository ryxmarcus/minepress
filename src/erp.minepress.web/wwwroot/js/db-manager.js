/**
 * MinePress ERP — Database Manager Engine (DBeaver-like CRUD)
 */
const DBM = (() => {
    // ── State ──
    let tables = [];
    let currentTable = null;
    let columns = [];
    let pkColumns = [];
    let fkMap = {};             // { colName: { refTable, refColumn, constraintName } }
    let rows = [];
    let currentPage = 1;
    let totalPages = 1;
    let totalCount = 0;
    let sortColumn = null;
    let sortDir = 'ASC';
    let editMode = null;        // 'insert' | 'edit'
    let editPkValues = null;
    let pendingDeletePk = null;
    let isReadOnly = false;
    let isProtected = false;
    let isDeleteRestricted = false;
    let isMasterTable = false;
    let immutableColumns = [];
    let fkLookupCache = {};
    let searchTimer = null;

    const API = '/api/dbmanager';

    // ══════════════════════════════════════════════════════════════════════
    //  Init
    // ══════════════════════════════════════════════════════════════════════

    function init() {
        loadTables();
        $('#dbmTableSearch').on('input', filterTableTree);
        $('#dbmDataSearch').on('input', () => {
            clearTimeout(searchTimer);
            searchTimer = setTimeout(() => { currentPage = 1; loadRows(); }, 350);
        });
        $('#dbmPageSize').on('change', () => { currentPage = 1; loadRows(); });
    }

    // ══════════════════════════════════════════════════════════════════════
    //  Table Browser
    // ══════════════════════════════════════════════════════════════════════

    async function loadTables() {
        try {
            tables = await $.get(`${API}/tables`);
            renderTableTree();
            const tableCount = tables.filter(t => t.type === 'table').length;
            const viewCount = tables.filter(t => t.type === 'view').length;
            $('#dbmStatTables').text(tableCount);
            $('#dbmStatViews').text(viewCount);
        } catch (e) {
            console.error('Failed to load tables', e);
            if (e.status === 403) {
                handleSessionExpired();
            }
        }
    }

    function renderTableTree() {
        const search = ($('#dbmTableSearch').val() || '').toLowerCase();
        const groups = {};

        tables.forEach(t => {
            if (search && !t.name.toLowerCase().includes(search) && !t.friendlyName.toLowerCase().includes(search)) return;
            if (!groups[t.category]) groups[t.category] = [];
            groups[t.category].push(t);
        });

        if (Object.keys(groups).length === 0) {
            $('#dbmTableTree').html('<div class="text-muted small text-center py-3">No tables found</div>');
            return;
        }

        let html = '';
        Object.keys(groups).sort().forEach(cat => {
            html += `<div class="dbm-tree-category" onclick="this.classList.toggle('collapsed')">
                        <i class="bi bi-chevron-down"></i>${cat}
                        <span class="ms-auto text-muted" style="font-size:.6rem;">${groups[cat].length}</span>
                     </div>`;
            html += '<div class="dbm-tree-items">';
            groups[cat].forEach(t => {
                const icon = t.type === 'view' ? 'bi-eye' : 'bi-table';
                const active = currentTable === t.name ? 'active' : '';
                html += `<div class="dbm-tree-item ${active}" data-table="${t.name}" onclick="DBM.selectTable('${t.name}')">
                            <i class="bi ${icon}"></i>
                            <span class="text-truncate">${t.friendlyName}</span>
                            <span class="dbm-col-count">${t.columnCount}</span>
                         </div>`;
            });
            html += '</div>';
        });
        $('#dbmTableTree').html(html);
    }

    function filterTableTree() { renderTableTree(); }

    // ══════════════════════════════════════════════════════════════════════
    //  Select Table
    // ══════════════════════════════════════════════════════════════════════

    async function selectTable(tableName) {
        currentTable = tableName;
        currentPage = 1;
        sortColumn = null;
        sortDir = 'ASC';
        fkLookupCache = {};
        $('#dbmDataSearch').val('');

        renderTableTree(); // highlight

        try {
            // Load columns, PK, FKs, stats in parallel
            const [cols, pkRes, fkRes, statsRes] = await Promise.all([
                $.get(`${API}/tables/${tableName}/columns`),
                $.get(`${API}/tables/${tableName}/pk`),
                $.get(`${API}/tables/${tableName}/fk`),
                $.get(`${API}/tables/${tableName}/stats`)
            ]);

            columns = cols;
            pkColumns = pkRes;
            fkMap = {};
            fkRes.forEach(fk => { fkMap[fk.fkColumn] = fk; });
            isReadOnly = statsRes.readOnly;
            isProtected = statsRes.isProtected || false;
            isDeleteRestricted = statsRes.isDeleteRestricted || false;
            isMasterTable = statsRes.isMasterTable || false;
            immutableColumns = statsRes.immutableColumns || [];

            // Update UI
            const tableInfo = tables.find(t => t.name === tableName);
            $('#dbmActiveTable').text(tableInfo ? tableInfo.friendlyName : tableName);
            $('#dbmRowCountBadge').text(`${statsRes.rowCount.toLocaleString()} rows`);

            // Protection badges
            let typeTxt = 'table';
            let typeWarn = false;
            if (statsRes.isView) { typeTxt = 'view (read-only)'; typeWarn = true; }
            else if (isProtected) { typeTxt = 'protected (read-only)'; typeWarn = true; }
            else if (isMasterTable) { typeTxt = 'master (no delete, id/code locked)'; typeWarn = true; }
            $('#dbmTypeBadge').text(typeTxt).toggleClass('bg-warning-lt', typeWarn).toggleClass('bg-primary-lt', !typeWarn);

            $('#dbmStatCols').text(columns.length);
            $('#dbmStatRows').text(statsRes.rowCount.toLocaleString());

            // Toggle insert button (hide for read-only / protected)
            $('#dbmBtnInsert').toggle(!isReadOnly);

            // Show data panel
            $('#dbmEmpty').hide();
            $('#dbmStructurePanel').hide();
            $('#dbmDataPanel').show();

            // Render structure data
            renderStructure();

            // Load data
            await loadRows();

        } catch (e) {
            console.error('Failed to select table', e);
            Swal.fire('Error', 'Failed to load table.', 'error');
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    //  Load Rows
    // ══════════════════════════════════════════════════════════════════════

    async function loadRows() {
        if (!currentTable) return;
        showLoading(true);

        const dto = {
            search: $('#dbmDataSearch').val() || null,
            sortColumn: sortColumn,
            sortDir: sortDir,
            page: currentPage,
            pageSize: parseInt($('#dbmPageSize').val()) || 50
        };

        try {
            const result = await $.ajax({
                url: `${API}/tables/${currentTable}/rows`,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(dto)
            });

            rows = result.data;
            totalCount = result.totalCount;
            currentPage = result.page;
            totalPages = result.totalPages;

            renderDataGrid();
            renderPagination();
            showLoading(false);
        } catch (e) {
            showLoading(false);
            console.error('Load rows failed', e);
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    //  Render Data Grid
    // ══════════════════════════════════════════════════════════════════════

    function renderDataGrid() {
        if (rows.length === 0) {
            $('#dbmTableHead').html('<tr><th class="text-center py-3" colspan="99">No data found</th></tr>');
            $('#dbmTableBody').html('');
            return;
        }

        // Header
        let thead = '<tr>';
        thead += '<th style="width:70px;">Actions</th>';
        columns.forEach(col => {
            const isPk = pkColumns.includes(col.name);
            const isFk = !!fkMap[col.name];
            const sorted = sortColumn === col.name;
            const cls = [isPk ? 'pk-col' : '', sorted ? 'sorted' : ''].filter(Boolean).join(' ');
            const arrow = sorted ? (sortDir === 'ASC' ? 'bi-sort-up' : 'bi-sort-down') : 'bi-arrow-down-up';
            const fkIcon = isFk ? '<i class="bi bi-link-45deg" style="color:var(--tblr-info);font-size:.6rem;" title="Foreign Key"></i> ' : '';
            thead += `<th class="${cls}" onclick="DBM.sortBy('${col.name}')" title="${col.name} (${col.dataType})">
                        ${fkIcon}${col.displayName}
                        <i class="bi ${arrow} sort-icon"></i>
                      </th>`;
        });
        thead += '</tr>';
        $('#dbmTableHead').html(thead);

        // Body
        const pageSize = parseInt($('#dbmPageSize').val()) || 50;
        const startRow = (currentPage - 1) * pageSize;
        let tbody = '';

        rows.forEach((row, idx) => {
            tbody += '<tr>';
            // Actions
            tbody += '<td class="dbm-row-actions">';
            if (!isReadOnly) {
                tbody += `<button class="btn btn-outline-primary" onclick="DBM.editRow(${idx})" title="Edit"><i class="bi bi-pencil"></i></button>`;
                if (!isDeleteRestricted) {
                    tbody += `<button class="btn btn-outline-danger" onclick="DBM.deleteRow(${idx})" title="Delete"><i class="bi bi-trash"></i></button>`;
                }
            }
            tbody += `<button class="btn btn-outline-info" onclick="DBM.viewRow(${idx})" title="View"><i class="bi bi-eye"></i></button>`;
            tbody += '</td>';

            // Data cells
            columns.forEach(col => {
                const val = row[col.name];
                const isPk = pkColumns.includes(col.name);
                const isFk = !!fkMap[col.name];
                tbody += `<td class="${col.isNumeric ? 'dbm-num' : ''}">${formatCell(val, col, isPk, isFk)}</td>`;
            });
            tbody += '</tr>';
        });
        $('#dbmTableBody').html(tbody);
    }

    function formatCell(val, col, isPk, isFk) {
        if (val === null || val === undefined) return '<span class="dbm-null">NULL</span>';
        if (col.isBoolean || val === true || val === false) {
            return val === true
                ? '<span class="dbm-bool-true"><i class="bi bi-check-circle-fill"></i> Yes</span>'
                : '<span class="dbm-bool-false"><i class="bi bi-circle"></i> No</span>';
        }
        if (col.isDate && typeof val === 'string' && val.includes('T')) {
            try {
                const d = new Date(val);
                return d.toLocaleDateString('en-GB', { day:'2-digit', month:'short', year:'numeric' }) + ' ' +
                       d.toLocaleTimeString('en-GB', { hour:'2-digit', minute:'2-digit' });
            } catch { return escHtml(String(val)); }
        }
        if (col.isNumeric && typeof val === 'number') {
            const formatted = val.toLocaleString(undefined, { maximumFractionDigits: 4 });
            return isPk ? `<span class="dbm-pk-val">${formatted}</span>` : formatted;
        }

        const s = String(val);
        const display = s.length > 60 ? escHtml(s.substring(0, 60)) + '…' : escHtml(s);
        if (isPk) return `<span class="dbm-pk-val">${display}</span>`;
        if (isFk) return `<span class="dbm-fk-val" title="FK → ${fkMap[col.name].refTable}.${fkMap[col.name].refColumn}">${display}</span>`;
        return display;
    }

    // ══════════════════════════════════════════════════════════════════════
    //  Sort
    // ══════════════════════════════════════════════════════════════════════

    function sortBy(colName) {
        if (sortColumn === colName) {
            sortDir = sortDir === 'ASC' ? 'DESC' : 'ASC';
        } else {
            sortColumn = colName;
            sortDir = 'ASC';
        }
        currentPage = 1;
        loadRows();
    }

    // ══════════════════════════════════════════════════════════════════════
    //  Pagination
    // ══════════════════════════════════════════════════════════════════════

    function renderPagination() {
        const pageSize = parseInt($('#dbmPageSize').val()) || 50;
        const from = totalCount === 0 ? 0 : (currentPage - 1) * pageSize + 1;
        const to = Math.min(currentPage * pageSize, totalCount);
        $('#dbmPageInfo').text(`Showing ${from}–${to} of ${totalCount.toLocaleString()}`);
        $('#dbmPageDisplay').text(`${currentPage} / ${totalPages}`);
        $('#dbmBtnFirst, #dbmBtnPrev').prop('disabled', currentPage <= 1);
        $('#dbmBtnLast, #dbmBtnNext').prop('disabled', currentPage >= totalPages);
    }

    function goToPage(p) {
        if (p === -1) p = totalPages;
        if (p < 1 || p > totalPages) return;
        currentPage = p;
        loadRows();
    }
    function nextPage() { goToPage(currentPage + 1); }
    function prevPage() { goToPage(currentPage - 1); }

    // ══════════════════════════════════════════════════════════════════════
    //  Structure View
    // ══════════════════════════════════════════════════════════════════════

    function renderStructure() {
        let html = '';
        columns.forEach(col => {
            const isPk = pkColumns.includes(col.name);
            const isFk = !!fkMap[col.name];
            const icon = isPk ? '<i class="bi bi-key-fill pk-icon"></i>' : isFk ? '<i class="bi bi-link-45deg fk-icon"></i>' : '';
            const typeClass = col.isNumeric ? 'num' : col.isDate ? 'date' : col.isBoolean ? 'bool'
                : (col.dataType.includes('char') || col.dataType === 'text') ? 'text' : 'other';
            const nullable = col.isNullable ? '<span class="text-success small">✓</span>' : '<span class="text-danger small">✗</span>';
            const defVal = col.hasDefault ? `<code style="font-size:.7rem;">${escHtml(col.defaultValue || 'auto')}</code>` : '—';

            html += `<tr>
                <td>${icon}</td>
                <td><strong>${escHtml(col.name)}</strong></td>
                <td><span class="dbm-col-type-badge ${typeClass}">${col.dataType}${col.maxLength ? `(${col.maxLength})` : ''}</span></td>
                <td class="text-center">${nullable}</td>
                <td>${defVal}</td>
                <td class="text-muted small">${escHtml(col.comment)}</td>
            </tr>`;
        });
        $('#dbmStructureBody').html(html);
    }

    function showStructureView() {
        $('#dbmDataPanel').hide();
        $('#dbmStructurePanel').show();
    }

    function showDataView() {
        $('#dbmStructurePanel').hide();
        $('#dbmDataPanel').show();
    }

    // ══════════════════════════════════════════════════════════════════════
    //  INSERT Modal
    // ══════════════════════════════════════════════════════════════════════

    async function showInsertModal() {
        if (!currentTable || isReadOnly) return;
        editMode = 'insert';
        editPkValues = null;
        $('#dbmRowModalTitle').html('<i class="bi bi-plus-circle text-success me-2"></i>Insert New Row');
        await renderRowForm(null);
        new minepress.Modal(document.getElementById('dbmRowModal')).show();
    }

    // ══════════════════════════════════════════════════════════════════════
    //  EDIT Row
    // ══════════════════════════════════════════════════════════════════════

    async function editRow(idx) {
        if (isReadOnly) return;
        const row = rows[idx];
        if (!row) return;

        editMode = 'edit';
        editPkValues = {};
        pkColumns.forEach(pk => { editPkValues[pk] = row[pk]; });

        $('#dbmRowModalTitle').html('<i class="bi bi-pencil-square text-primary me-2"></i>Edit Row');
        await renderRowForm(row);
        new minepress.Modal(document.getElementById('dbmRowModal')).show();
    }

    // ══════════════════════════════════════════════════════════════════════
    //  VIEW Row (read-only modal)
    // ══════════════════════════════════════════════════════════════════════

    function viewRow(idx) {
        const row = rows[idx];
        if (!row) return;

        let html = '';
        columns.forEach(col => {
            const val = row[col.name];
            const isPk = pkColumns.includes(col.name);
            const isFk = !!fkMap[col.name];
            html += `<div class="dbm-field-group">
                <div class="dbm-field-label">
                    ${isPk ? '<span class="pk-badge">PK</span>' : ''}
                    ${isFk ? '<span class="fk-badge">FK</span>' : ''}
                    ${escHtml(col.name)}
                    <span class="dbm-field-type">${col.dataType}</span>
                </div>
                <div style="padding:.2rem 0;font-size:.85rem;">${formatCell(val, col, isPk, isFk)}</div>
            </div>`;
        });

        $('#dbmRowModalTitle').html('<i class="bi bi-eye text-info me-2"></i>View Row');
        $('#dbmRowModalBody').html(html);
        $('#dbmBtnSaveRow').hide();
        new minepress.Modal(document.getElementById('dbmRowModal')).show();
    }

    // ══════════════════════════════════════════════════════════════════════
    //  Render Row Form
    // ══════════════════════════════════════════════════════════════════════

    async function renderRowForm(existingRow) {
        let html = '';

        for (const col of columns) {
            const isPk = pkColumns.includes(col.name);
            const isFk = !!fkMap[col.name];
            const val = existingRow ? existingRow[col.name] : null;
            const isAutoGen = isPk && col.hasDefault && editMode === 'insert';
            const isImmutable = isMasterTable && editMode === 'edit' && immutableColumns.includes(col.name);

            html += `<div class="dbm-field-group">
                <div class="dbm-field-label">
                    ${isPk ? '<span class="pk-badge">PK</span>' : ''}
                    ${isFk ? '<span class="fk-badge">FK</span>' : ''}
                    ${isImmutable && !isPk ? '<span class="badge bg-secondary-lt" style="font-size:.55rem;">LOCKED</span>' : ''}
                    ${escHtml(col.displayName)}
                    ${!col.isNullable && !col.hasDefault ? '<span class="required-star">*</span>' : ''}
                    <span class="dbm-field-type">${col.dataType}${col.maxLength ? `(${col.maxLength})` : ''}</span>
                </div>`;

            if ((isPk && editMode === 'edit') || isImmutable) {
                // PK or immutable id/code columns are read-only on edit
                html += `<input type="text" class="form-control" data-col="${col.name}" value="${escAttr(val)}" readonly disabled>`;
            } else if (isAutoGen) {
                html += `<input type="text" class="form-control" data-col="${col.name}" value="" placeholder="(auto-generated)" disabled>`;
            } else if (col.isBoolean) {
                const checked = val === true ? 'checked' : '';
                html += `<div class="form-check form-switch" style="padding-top:.3rem;">
                    <input class="form-check-input" type="checkbox" data-col="${col.name}" data-type="boolean" ${checked}>
                </div>`;
            } else if (isFk) {
                // FK dropdown (will be populated)
                html += `<select class="form-select dbm-fk-select" data-col="${col.name}" id="fkSelect_${col.name}">
                    <option value="">— Loading… —</option>
                </select>`;
            } else if (col.isDate) {
                const dateVal = val ? formatDateForInput(val) : '';
                const inputType = col.dataType === 'date' ? 'date' : 'datetime-local';
                html += `<input type="${inputType}" class="form-control" data-col="${col.name}" value="${dateVal}">`;
            } else if (col.isNumeric) {
                html += `<input type="number" class="form-control" data-col="${col.name}" value="${val ?? ''}" step="any">`;
            } else if (col.maxLength && col.maxLength > 500) {
                html += `<textarea class="form-control" data-col="${col.name}" rows="3">${escHtml(val ?? '')}</textarea>`;
            } else {
                html += `<input type="text" class="form-control" data-col="${col.name}" value="${escAttr(val)}"
                          ${col.maxLength ? `maxlength="${col.maxLength}"` : ''}>`;
            }

            html += '</div>';
        }

        $('#dbmRowModalBody').html(html);
        $('#dbmBtnSaveRow').show();

        // Load FK dropdowns
        for (const col of columns) {
            if (fkMap[col.name]) {
                await loadFkDropdown(col.name, existingRow ? existingRow[col.name] : null);
            }
        }
    }

    async function loadFkDropdown(colName, selectedValue) {
        const sel = $(`#fkSelect_${colName}`);
        try {
            let data;
            if (fkLookupCache[colName]) {
                data = fkLookupCache[colName];
            } else {
                data = await $.get(`${API}/tables/${currentTable}/fk-lookup/${colName}`);
                fkLookupCache[colName] = data;
            }

            let opts = '<option value="">— Select —</option>';
            (data.items || []).forEach(item => {
                const isSelected = String(item.id) === String(selectedValue) ? 'selected' : '';
                opts += `<option value="${escAttr(item.id)}" ${isSelected}>${escHtml(item.label)} (${item.id})</option>`;
            });
            sel.html(opts);
        } catch {
            sel.html('<option value="">— Failed to load —</option>');
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    //  Save Row (INSERT / UPDATE)
    // ══════════════════════════════════════════════════════════════════════

    async function saveRow() {
        const rowData = {};

        columns.forEach(col => {
            const isPk = pkColumns.includes(col.name);
            const isAutoGen = isPk && col.hasDefault && editMode === 'insert';
            if (isAutoGen) return;
            if (isPk && editMode === 'edit') return;

            const el = $(`[data-col="${col.name}"]`);
            if (el.length === 0) return;

            let val;
            if (el.data('type') === 'boolean') {
                val = el.is(':checked');
            } else {
                val = el.val();
                if (val === '' && col.isNullable) val = null;
            }
            rowData[col.name] = val;
        });

        try {
            if (editMode === 'insert') {
                await $.ajax({
                    url: `${API}/tables/${currentTable}/insert`,
                    type: 'POST',
                    contentType: 'application/json',
                    data: JSON.stringify(rowData)
                });
                Swal.fire({ icon: 'success', title: 'Inserted!', text: 'Row added successfully.', timer: 1500, showConfirmButton: false });
            } else {
                await $.ajax({
                    url: `${API}/tables/${currentTable}/update`,
                    type: 'PUT',
                    contentType: 'application/json',
                    data: JSON.stringify({ pkValues: editPkValues, rowData })
                });
                Swal.fire({ icon: 'success', title: 'Updated!', text: 'Row updated successfully.', timer: 1500, showConfirmButton: false });
            }

            minepress.Modal.getInstance(document.getElementById('dbmRowModal'))?.hide();
            loadRows();
        } catch (e) {
            if (e.status === 403) { handleSessionExpired(); return; }
            const msg = e.responseJSON?.message || 'Operation failed.';
            Swal.fire('Error', msg, 'error');
        }
    }

    function handleSessionExpired() {
        Swal.fire({
            icon: 'error',
            title: 'Session Expired',
            text: 'Your session has expired or you no longer have admin access. You will be redirected to login.',
            confirmButtonText: 'OK'
        }).then(() => { window.location.href = '/Account/Login'; });
    }

    // ══════════════════════════════════════════════════════════════════════
    //  DELETE Row
    // ══════════════════════════════════════════════════════════════════════

    function deleteRow(idx) {
        if (isReadOnly) return;
        const row = rows[idx];
        if (!row) return;

        pendingDeletePk = {};
        pkColumns.forEach(pk => { pendingDeletePk[pk] = row[pk]; });

        // Show key values in confirmation
        const keyInfo = pkColumns.map(pk => `<strong>${escHtml(pk)}</strong> = ${escHtml(String(row[pk]))}`).join(', ');
        $('#dbmDeleteBody').html(`
            <div class="mb-2">Are you sure you want to delete this row?</div>
            <div class="p-2 rounded" style="background:rgba(var(--tblr-danger-rgb),.06);font-size:.85rem;">
                ${keyInfo}
            </div>
            <div class="text-muted small mt-2"><i class="bi bi-shield-check me-1"></i>Referential integrity will be checked before deletion.</div>
        `);

        new minepress.Modal(document.getElementById('dbmDeleteModal')).show();
    }

    async function confirmDelete() {
        if (!pendingDeletePk) return;

        // Close the first modal, then double-confirm with SweetAlert
        minepress.Modal.getInstance(document.getElementById('dbmDeleteModal'))?.hide();

        const keyInfo = Object.entries(pendingDeletePk).map(([k, v]) => `${k} = ${v}`).join(', ');
        const result = await Swal.fire({
            icon: 'warning',
            title: 'Final Confirmation',
            html: `<p>You are about to <strong>permanently delete</strong> a row from <strong>${escHtml(currentTable)}</strong>.</p>
                   <div class="p-2 rounded" style="background:rgba(var(--tblr-danger-rgb),.08);font-size:.85rem;">${escHtml(keyInfo)}</div>
                   <p class="text-muted small mt-2">This action cannot be undone.</p>`,
            showCancelButton: true,
            confirmButtonColor: '#d63939',
            confirmButtonText: 'Yes, delete it',
            cancelButtonText: 'Cancel'
        });

        if (!result.isConfirmed) { pendingDeletePk = null; return; }

        try {
            await $.ajax({
                url: `${API}/tables/${currentTable}/delete`,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(pendingDeletePk)
            });

            Swal.fire({ icon: 'success', title: 'Deleted!', timer: 1200, showConfirmButton: false });
            pendingDeletePk = null;
            loadRows();
        } catch (e) {
            if (e.status === 403) { handleSessionExpired(); return; }
            const data = e.responseJSON;
            if (data?.references && data.references.length > 0) {
                let refHtml = '<ul class="dbm-ref-list">';
                data.references.forEach(r => {
                    refHtml += `<li><i class="bi bi-link-45deg"></i>${escHtml(r)}</li>`;
                });
                refHtml += '</ul>';

                Swal.fire({
                    icon: 'warning',
                    title: 'Cannot Delete',
                    html: `<div>${escHtml(data.message)}</div>${refHtml}`,
                    confirmButtonText: 'OK'
                });
            } else {
                Swal.fire('Error', data?.message || 'Delete failed.', 'error');
            }
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    //  Refresh
    // ══════════════════════════════════════════════════════════════════════

    function refreshCurrentTable() {
        if (currentTable) {
            fkLookupCache = {};
            selectTable(currentTable);
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    //  Helpers
    // ══════════════════════════════════════════════════════════════════════

    function showLoading(show) {
        if (show) {
            if (!$('#dbmLoadingOverlay').length) {
                $('#dbmGridWrapper').append('<div class="dbm-loading" id="dbmLoadingOverlay"><div class="spinner-border text-primary"></div></div>');
            }
        } else {
            $('#dbmLoadingOverlay').remove();
        }
    }

    function escHtml(str) {
        if (str == null) return '';
        const d = document.createElement('div');
        d.textContent = String(str);
        return d.innerHTML;
    }

    function escAttr(val) {
        if (val == null) return '';
        return String(val).replace(/&/g,'&amp;').replace(/"/g,'&quot;').replace(/'/g,'&#39;').replace(/</g,'&lt;').replace(/>/g,'&gt;');
    }

    function formatDateForInput(val) {
        if (!val) return '';
        try {
            const d = new Date(val);
            return d.toISOString().slice(0, 16);
        } catch { return ''; }
    }

    // ── Public API ──
    return {
        init,
        selectTable, refreshCurrentTable,
        loadRows, sortBy,
        goToPage, nextPage, prevPage,
        showInsertModal, editRow, viewRow, saveRow,
        deleteRow, confirmDelete,
        showStructureView, showDataView
    };
})();

$(document).ready(function () {
    DBM.init();
});
