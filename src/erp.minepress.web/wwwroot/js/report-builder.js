/**
 * MinePress ERP — Smart Report Builder Engine
 */
const RB = (() => {
    // ── State ──
    let tables = [];
    let columns = [];           // { name, displayName, dataType, isNumeric, isDate, isBoolean }
    let selectedColumns = [];   // column names selected by user
    let filters = [];           // { id, columnName, operator, filterValue, filterValue2, logicOperator }
    let groupBys = [];          // column names
    let orderBys = [];          // { column, dir }
    let aggregates = [];        // { column, fn }
    let savedReports = [];
    let currentReportId = 0;
    let joinedTables = [];      // { table, joinType, fkColumn, pkColumn }
    let relationships = { outgoing: [], incoming: [] };
    let reportType = 'detail';  // 'detail' | 'summary'
    let joinedColumns = {};     // { tableName: [columns] }

    let resultData = [];
    let resultColumns = [];
    let currentPage = 1;
    let totalPages = 1;
    let totalCount = 0;
    let nextFilterId = 1;
    let activeCategory = null;  // for category chip filtering

    const API = '/api/report';

    // ── Operator sets per data type ──
    const OPERATORS = {
        numeric: [
            { value: 'eq', label: '= Equal' },
            { value: 'neq', label: '≠ Not Equal' },
            { value: 'gt', label: '> Greater' },
            { value: 'gte', label: '≥ Greater or Equal' },
            { value: 'lt', label: '< Less' },
            { value: 'lte', label: '≤ Less or Equal' },
            { value: 'between', label: '↔ Between' },
            { value: 'isnull', label: '∅ Is Null' },
            { value: 'isnotnull', label: '✓ Not Null' }
        ],
        date: [
            { value: 'eq', label: '= Equal' },
            { value: 'gt', label: '> After' },
            { value: 'gte', label: '≥ On or After' },
            { value: 'lt', label: '< Before' },
            { value: 'lte', label: '≤ On or Before' },
            { value: 'between', label: '↔ Between' },
            { value: 'isnull', label: '∅ Is Null' },
            { value: 'isnotnull', label: '✓ Not Null' }
        ],
        boolean: [
            { value: 'eq', label: '= Equal' },
            { value: 'isnull', label: '∅ Is Null' },
            { value: 'isnotnull', label: '✓ Not Null' }
        ],
        text: [
            { value: 'eq', label: '= Equal' },
            { value: 'neq', label: '≠ Not Equal' },
            { value: 'contains', label: '⊃ Contains' },
            { value: 'startswith', label: 'A… Starts With' },
            { value: 'endswith', label: '…Z Ends With' },
            { value: 'isnull', label: '∅ Is Null' },
            { value: 'isnotnull', label: '✓ Not Null' }
        ]
    };

    // ── Init ──
    function init() {
        loadTables();
        loadSavedReports();
        initSmartTableSearch();
        $('#rbColumnSearch').on('input', filterColumnList);
        $('#rbSearchSaved').on('input', filterSavedList);
        $('#rbPageSize').on('change', () => { currentPage = 1; executeReport(); });
        $('#rbExportFormat').on('change', updateExportRecommendation);
    }

    function switchTab(tabId) {
        const tab = document.getElementById(tabId);
        if (tab) new bootstrap.Tab(tab).show();
    }

    // ══════════════════════════════════════════════════════════════════════
    //  Smart Table Search
    // ══════════════════════════════════════════════════════════════════════

    function initSmartTableSearch() {
        const $search = $('#rbTableSearch');
        const $dropdown = $('#rbTableDropdown');

        $search.on('input', function () {
            const q = this.value.trim().toLowerCase();
            if (q.length === 0 && !activeCategory) {
                $dropdown.hide();
                return;
            }
            renderTableDropdown(q);
            $dropdown.show();
        });

        $search.on('focus', function () {
            if (tables.length > 0) {
                renderTableDropdown(this.value.trim().toLowerCase());
                $dropdown.show();
            }
        });

        $(document).on('click', function (e) {
            if (!$(e.target).closest('.rb-smart-search').length) {
                $dropdown.hide();
            }
        });
    }

    function renderTableDropdown(query) {
        let filtered = tables;

        if (activeCategory) {
            filtered = filtered.filter(t => t.category === activeCategory);
        }
        if (query) {
            filtered = filtered.filter(t =>
                t.name.toLowerCase().includes(query) ||
                t.friendlyName.toLowerCase().includes(query) ||
                t.category.toLowerCase().includes(query)
            );
        }

        if (filtered.length === 0) {
            $('#rbTableDropdown').html('<div class="rb-dropdown-empty"><i class="bi bi-search"></i> No tables found</div>');
            return;
        }

        // Group by category
        const groups = {};
        filtered.forEach(t => {
            if (!groups[t.category]) groups[t.category] = [];
            groups[t.category].push(t);
        });

        let html = '';
        Object.keys(groups).sort().forEach(cat => {
            html += `<div class="rb-dropdown-cat">${cat}</div>`;
            groups[cat].forEach(t => {
                const icon = t.type === 'view' ? 'bi-eye' : 'bi-table';
                const typeBadge = t.type === 'view' ? '<span class="badge bg-info-lt" style="font-size:.55rem;">VIEW</span>' : '';
                const isSelected = $('#rbTableSelect').val() === t.name;
                html += `
                    <div class="rb-dropdown-item ${isSelected ? 'active' : ''}" onclick="RB.selectTable('${t.name}')">
                        <i class="bi ${icon} rb-dropdown-icon"></i>
                        <div class="flex-grow-1 min-width-0">
                            <div class="rb-dropdown-name text-truncate">${highlightMatch(t.friendlyName, query)}</div>
                            <div class="rb-dropdown-sub">${t.name} ${typeBadge}</div>
                        </div>
                        <i class="bi bi-chevron-right rb-dropdown-arrow"></i>
                    </div>`;
            });
        });
        $('#rbTableDropdown').html(html);
    }

    function highlightMatch(text, query) {
        if (!query) return escHtml(text);
        const idx = text.toLowerCase().indexOf(query.toLowerCase());
        if (idx < 0) return escHtml(text);
        return escHtml(text.substring(0, idx)) +
            '<mark>' + escHtml(text.substring(idx, idx + query.length)) + '</mark>' +
            escHtml(text.substring(idx + query.length));
    }

    function renderCategoryChips() {
        const cats = [...new Set(tables.map(t => t.category))].sort();
        const catIcons = {
            'Masters': 'bi-database', 'Transactions': 'bi-arrow-left-right',
            'HR & Payroll': 'bi-people', 'Reports': 'bi-bar-chart-line',
            'Views': 'bi-eye', 'System': 'bi-gear', 'Activities': 'bi-activity',
            'Hybrid': 'bi-intersect', 'Other': 'bi-grid'
        };
        const catCounts = {};
        tables.forEach(t => { catCounts[t.category] = (catCounts[t.category] || 0) + 1; });

        let html = `<button class="rb-chip ${!activeCategory ? 'active' : ''}" onclick="RB.filterByCategory(null)">All <span class="rb-chip-count">${tables.length}</span></button>`;
        cats.forEach(cat => {
            const icon = catIcons[cat] || 'bi-folder';
            html += `<button class="rb-chip ${activeCategory === cat ? 'active' : ''}" onclick="RB.filterByCategory('${cat}')">
                <i class="bi ${icon}"></i>${cat} <span class="rb-chip-count">${catCounts[cat]}</span>
            </button>`;
        });
        $('#rbCategoryChips').html(html);
    }

    function filterByCategory(cat) {
        activeCategory = cat;
        renderCategoryChips();
        const q = $('#rbTableSearch').val().trim().toLowerCase();
        renderTableDropdown(q);
        $('#rbTableDropdown').show();
    }

    function selectTable(tableName) {
        $('#rbTableSelect').val(tableName);
        $('#rbTableDropdown').hide();
        $('#rbTableSearch').val('');
        onTableChange();
    }

    function clearTable() {
        $('#rbTableSelect').val('');
        $('#rbTableSearch').val('');
        $('#rbTablePreview').hide();
        columns = [];
        selectedColumns = [];
        joinedTables = [];
        relationships = { outgoing: [], incoming: [] };
        joinedColumns = {};
        renderColumnList();
        renderJoins();
        $('#rbTableInfo').text('');
    }

    function showTablePreview(tableName) {
        const info = tables.find(t => t.name === tableName);
        if (!info) { $('#rbTablePreview').hide(); return; }

        const icon = info.type === 'view' ? 'bi-eye' : 'bi-table';
        const catBadgeColor = {
            'Masters': 'bg-primary-lt', 'Transactions': 'bg-success-lt',
            'HR & Payroll': 'bg-warning-lt', 'Reports': 'bg-info-lt',
            'Views': 'bg-cyan-lt', 'System': 'bg-secondary-lt',
            'Activities': 'bg-orange-lt', 'Hybrid': 'bg-purple-lt', 'Other': 'bg-secondary-lt'
        };

        $('#rbPreviewIcon').html(`<i class="bi ${icon}"></i>`);
        $('#rbPreviewName').text(info.friendlyName);
        let meta = `<span class="badge ${catBadgeColor[info.category] || 'bg-secondary-lt'}" style="font-size:.55rem;">${info.category}</span>`;
        meta += ` <span class="text-muted" style="font-size:.65rem;">• ${columns.length} columns</span>`;
        if (info.type === 'view') meta += ` <span class="badge bg-info-lt" style="font-size:.55rem;">VIEW</span>`;
        $('#rbPreviewMeta').html(meta);

        // Relationship badges
        let badges = '';
        if (relationships.outgoing.length > 0) {
            badges += `<span class="badge bg-primary-lt" style="font-size:.55rem;" title="Foreign keys to other tables"><i class="bi bi-box-arrow-up-right me-1"></i>${relationships.outgoing.length} FK out</span> `;
        }
        if (relationships.incoming.length > 0) {
            badges += `<span class="badge bg-success-lt" style="font-size:.55rem;" title="Tables referencing this table"><i class="bi bi-box-arrow-in-down-left me-1"></i>${relationships.incoming.length} FK in</span> `;
        }
        $('#rbPreviewBadges').html(badges);
        $('#rbTablePreview').show();
    }

    // ══════════════════════════════════════════════════════════════════════
    //  Data Loading
    // ══════════════════════════════════════════════════════════════════════

    async function loadTables() {
        try {
            tables = await $.get(`${API}/tables`);
            renderCategoryChips();
            $('#rbStatTables').text(tables.length);
        } catch (e) {
            console.error('Failed to load tables', e);
        }
    }

    async function onTableChange() {
        const tableName = $('#rbTableSelect').val();
        if (!tableName) {
            clearTable();
            return;
        }

        try {
            columns = await $.get(`${API}/tables/${tableName}/columns`);
            selectedColumns = [];
            filters = [];
            groupBys = [];
            orderBys = [];
            aggregates = [];
            joinedTables = [];
            joinedColumns = {};
            renderColumnList();
            renderFilters();
            renderGroupBys();
            renderOrderBys();
            renderAggregates();

            // Load FK relationships
            try {
                relationships = await $.get(`${API}/tables/${tableName}/relationships`);
            } catch { relationships = { outgoing: [], incoming: [] }; }
            renderJoins();
            showTablePreview(tableName);
        } catch (e) {
            console.error('Failed to load columns', e);
            Swal.fire('Error', 'Failed to load columns for this table.', 'error');
        }
    }

    async function loadSavedReports() {
        try {
            savedReports = await $.get(`${API}/saved`);
            renderSavedList();
            updateStats();
        } catch (e) {
            console.error('Failed to load saved reports', e);
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    //  Column List
    // ══════════════════════════════════════════════════════════════════════

    function renderColumnList() {
        const search = ($('#rbColumnSearch').val() || '').toLowerCase();

        // Combine primary columns + joined table columns
        let allCols = columns.map(c => ({ ...c, qualified: c.name, sourceTable: null }));
        Object.entries(joinedColumns).forEach(([tbl, cols]) => {
            cols.forEach(c => {
                allCols.push({ ...c, qualified: `${tbl}.${c.name}`, sourceTable: tbl, displayName: `${humanize(tbl)} › ${c.displayName}` });
            });
        });

        const filtered = allCols.filter(c =>
            c.qualified.toLowerCase().includes(search) || c.displayName.toLowerCase().includes(search)
        );

        if (filtered.length === 0) {
            $('#rbColumnList').html('<div class="text-muted small text-center py-3">No columns found</div>');
            return;
        }

        let html = '';
        let currentSource = null;
        filtered.forEach(col => {
            const source = col.sourceTable || 'primary';
            if (source !== currentSource) {
                currentSource = source;
                const label = col.sourceTable ? humanize(col.sourceTable) : 'Primary Table';
                html += `<div class="rb-column-group-header">${label}</div>`;
            }

            const sel = selectedColumns.includes(col.qualified) ? 'selected' : '';
            const typeClass = col.isNumeric ? 'numeric' : col.isDate ? 'date' : col.isBoolean ? 'bool' : (col.dataType.includes('char') || col.dataType === 'text') ? 'text' : 'other';
            const typeLabel = col.isNumeric ? 'NUM' : col.isDate ? 'DATE' : col.isBoolean ? 'BOOL' : (col.dataType.includes('char') || col.dataType === 'text') ? 'TXT' : col.dataType.substring(0, 4).toUpperCase();

            html += `
                <div class="rb-column-item ${sel}" data-col="${col.qualified}" onclick="RB.toggleColumn('${col.qualified}')" oncontextmenu="RB.columnContextMenu(event, '${col.qualified}')">
                    <i class="bi bi-grip-vertical rb-drag-handle"></i>
                    <input type="checkbox" class="form-check-input" style="margin:0; width:14px; height:14px;"
                           ${sel ? 'checked' : ''} onclick="event.stopPropagation(); RB.toggleColumn('${col.qualified}')">
                    <span class="rb-col-type ${typeClass}">${typeLabel}</span>
                    <span class="text-truncate" title="${col.qualified}">${col.displayName}</span>
                </div>`;
        });
        $('#rbColumnList').html(html);
    }

    function toggleColumn(colName) {
        const idx = selectedColumns.indexOf(colName);
        if (idx >= 0) selectedColumns.splice(idx, 1);
        else selectedColumns.push(colName);
        renderColumnList();
    }

    function selectAllColumns() {
        selectedColumns = columns.map(c => c.name);
        renderColumnList();
    }

    function deselectAllColumns() {
        selectedColumns = [];
        renderColumnList();
    }

    function filterColumnList() {
        renderColumnList();
    }

    // ── Column Context Menu (right-click → quick filter) ──
    function columnContextMenu(event, colName) {
        event.preventDefault();
        event.stopPropagation();

        // Remove existing context menus
        $('.rb-context-menu').remove();

        const col = findColInfo(colName);
        if (!col) return;

        const typeLabel = col.isNumeric ? 'Numeric' : col.isDate ? 'Date' : col.isBoolean ? 'Boolean' : 'Text';

        let html = `<div class="rb-context-menu" style="top:${event.clientY}px; left:${event.clientX}px;">
            <div class="rb-ctx-header">${col.displayName} <span class="badge bg-secondary-lt" style="font-size:.55rem;">${typeLabel}</span></div>
            <div class="rb-ctx-item" onclick="RB.addQuickFilter('${colName}', 'eq')"><i class="bi bi-funnel me-2"></i>Filter (=)</div>
            ${col.isNumeric ? '<div class="rb-ctx-item" onclick="RB.addQuickFilter(\'' + colName + '\', \'between\')"><i class="bi bi-arrows-expand me-2"></i>Filter (Between)</div>' : ''}
            ${!col.isBoolean ? '<div class="rb-ctx-item" onclick="RB.addQuickFilter(\'' + colName + '\', \'contains\')"><i class="bi bi-search me-2"></i>Filter (Contains)</div>' : ''}
            <div class="rb-ctx-divider"></div>
            <div class="rb-ctx-item" onclick="RB.addQuickSort('${colName}', 'ASC')"><i class="bi bi-sort-alpha-down me-2"></i>Sort Ascending</div>
            <div class="rb-ctx-item" onclick="RB.addQuickSort('${colName}', 'DESC')"><i class="bi bi-sort-alpha-up me-2"></i>Sort Descending</div>
            <div class="rb-ctx-divider"></div>
            <div class="rb-ctx-item" onclick="RB.addQuickGroupBy('${colName}')"><i class="bi bi-collection me-2"></i>Group By</div>
        </div>`;

        $('body').append(html);
        setTimeout(() => {
            $(document).one('click', () => $('.rb-context-menu').remove());
        }, 10);
    }

    function addQuickFilter(colName, operator) {
        $('.rb-context-menu').remove();
        const col = findColInfo(colName);
        if (!col) return;
        filters.push({ id: nextFilterId++, columnName: colName, operator, filterValue: '', filterValue2: '', logicOperator: 'AND' });
        renderFilters();
    }

    function addQuickSort(colName, dir) {
        $('.rb-context-menu').remove();
        const existing = orderBys.find(o => o.column === colName);
        if (existing) { existing.dir = dir; }
        else { orderBys.push({ column: colName, dir }); }
        renderOrderBys();
    }

    function addQuickGroupBy(colName) {
        $('.rb-context-menu').remove();
        if (!groupBys.includes(colName)) {
            groupBys.push(colName);
            renderGroupBys();
            $('#rbAggregatesSection').show();
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    //  Joined Tables
    // ══════════════════════════════════════════════════════════════════════

    function addJoin() {
        const allRels = [...relationships.outgoing, ...relationships.incoming.map(r => ({
            fkColumn: r.referencingColumn,
            referencedTable: r.referencingTable,
            referencedTableName: r.referencingTableName,
            referencedColumn: r.pkColumn
        }))];

        const available = allRels.filter(r => !joinedTables.some(j => j.table === r.referencedTable));
        if (available.length === 0) { Swal.fire('Info', 'No more related tables available to join.', 'info'); return; }

        const rel = available[0];
        joinedTables.push({ table: rel.referencedTable, joinType: 'LEFT', fkColumn: rel.fkColumn, pkColumn: rel.referencedColumn });
        loadJoinedColumns(rel.referencedTable);
        renderJoins();
    }

    function removeJoin(idx) {
        const removed = joinedTables.splice(idx, 1)[0];
        if (removed) {
            delete joinedColumns[removed.table];
            selectedColumns = selectedColumns.filter(c => !c.startsWith(removed.table + '.'));
        }
        renderJoins();
        renderColumnList();
    }

    function updateJoinType(idx, value) {
        if (joinedTables[idx]) joinedTables[idx].joinType = value;
    }

    async function loadJoinedColumns(tableName) {
        try {
            const cols = await $.get(`${API}/tables/${tableName}/columns`);
            joinedColumns[tableName] = cols;
            renderColumnList();
        } catch (e) {
            console.error('Failed to load joined columns', e);
        }
    }

    function renderJoins() {
        if (joinedTables.length === 0) {
            $('#rbJoinList').html('<div class="text-muted small text-center py-2">No tables joined</div>');
            return;
        }

        let html = '';
        joinedTables.forEach((jt, idx) => {
            html += `
                <div class="rb-join-item">
                    <select class="form-select" onchange="RB.updateJoinType(${idx},this.value)" style="width:62px;">
                        <option value="LEFT" ${jt.joinType === 'LEFT' ? 'selected' : ''}>LEFT</option>
                        <option value="INNER" ${jt.joinType === 'INNER' ? 'selected' : ''}>INNER</option>
                        <option value="RIGHT" ${jt.joinType === 'RIGHT' ? 'selected' : ''}>RIGHT</option>
                    </select>
                    <span class="text-truncate flex-grow-1 fw-medium" title="${jt.table}">${humanize(jt.table)}</span>
                    <span class="badge bg-secondary-lt" style="font-size:.6rem;">${jt.fkColumn} → ${jt.pkColumn}</span>
                    <button class="btn-remove" onclick="RB.removeJoin(${idx})"><i class="bi bi-x-circle"></i></button>
                </div>`;
        });
        $('#rbJoinList').html(html);
    }

    // ══════════════════════════════════════════════════════════════════════
    //  Report Type
    // ══════════════════════════════════════════════════════════════════════

    function setReportType(type) {
        reportType = type;
        if (type === 'detail') {
            $('#rbTypeDetail').addClass('active');
            $('#rbTypeSummary').removeClass('active');
            $('#rbAggregatesSection').toggle(groupBys.length > 0);
        } else {
            $('#rbTypeSummary').addClass('active');
            $('#rbTypeDetail').removeClass('active');
            $('#rbAggregatesSection').show();
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    //  Smart Filters
    // ══════════════════════════════════════════════════════════════════════

    function getColumnType(colName) {
        const col = findColInfo(colName);
        if (!col) return 'text';
        if (col.isBoolean) return 'boolean';
        if (col.isNumeric) return 'numeric';
        if (col.isDate) return 'date';
        return 'text';
    }

    function getOperatorsForType(type) {
        return OPERATORS[type] || OPERATORS.text;
    }

    function addSmartFilter() {
        if (columns.length === 0) { Swal.fire('Info', 'Select a table first.', 'info'); return; }
        const col = columns[0];
        const type = getColumnType(col.name);
        const defaultOp = type === 'text' ? 'contains' : 'eq';
        filters.push({ id: nextFilterId++, columnName: col.name, operator: defaultOp, filterValue: '', filterValue2: '', logicOperator: 'AND' });
        renderFilters();
    }

    function addFilter() { addSmartFilter(); }

    function removeFilter(id) {
        filters = filters.filter(f => f.id !== id);
        renderFilters();
    }

    function clearAllFilters() {
        filters = [];
        renderFilters();
    }

    function renderFilters() {
        // Update badge count
        if (filters.length > 0) {
            $('#rbFilterCount').text(filters.length).show();
            $('#rbClearFiltersBtn').show();
        } else {
            $('#rbFilterCount').hide();
            $('#rbClearFiltersBtn').hide();
        }

        if (filters.length === 0) {
            $('#rbFilterList').html(`<div class="rb-filter-empty text-muted small text-center py-2">
                <i class="bi bi-funnel" style="font-size:1rem; opacity:.4;"></i>
                <div class="mt-1">No filters — click <strong>+Add</strong> or right-click a column</div>
            </div>`);
            return;
        }

        // Build all columns options (primary + joined)
        let allCols = columns.map(c => ({ ...c, qualified: c.name }));
        Object.entries(joinedColumns).forEach(([tbl, cols]) => {
            cols.forEach(c => {
                allCols.push({ ...c, qualified: `${tbl}.${c.name}`, displayName: `${humanize(tbl)} › ${c.displayName}` });
            });
        });

        let html = '';
        filters.forEach((f, idx) => {
            const colType = getColumnType(f.columnName);
            const operators = getOperatorsForType(colType);
            const isNullOp = f.operator === 'isnull' || f.operator === 'isnotnull';
            const isBetween = f.operator === 'between';
            const typeClass = colType === 'numeric' ? 'numeric' : colType === 'date' ? 'date' : colType === 'boolean' ? 'bool' : 'text';

            // Column options with type indicators
            const colOpts = allCols.map(c => {
                const ct = c.isNumeric ? 'NUM' : c.isDate ? 'DATE' : c.isBoolean ? 'BOOL' : 'TXT';
                return `<option value="${c.qualified}" ${c.qualified === f.columnName ? 'selected' : ''}>${c.displayName} [${ct}]</option>`;
            }).join('');

            // Operator options filtered by type
            const opOpts = operators.map(op =>
                `<option value="${op.value}" ${op.value === f.operator ? 'selected' : ''}>${op.label}</option>`
            ).join('');

            // Value input based on type
            let valueHtml = '';
            if (!isNullOp) {
                if (colType === 'boolean') {
                    valueHtml = `<div class="rb-filter-bool-toggle">
                        <button class="btn btn-sm ${f.filterValue === 'true' ? 'btn-success' : 'btn-outline-secondary'}" onclick="RB.updateFilter(${f.id},'val','true')">
                            <i class="bi bi-check-circle"></i> Yes
                        </button>
                        <button class="btn btn-sm ${f.filterValue === 'false' ? 'btn-danger' : 'btn-outline-secondary'}" onclick="RB.updateFilter(${f.id},'val','false')">
                            <i class="bi bi-x-circle"></i> No
                        </button>
                    </div>`;
                } else if (colType === 'date') {
                    valueHtml = `<input type="date" class="form-control rb-filter-val" value="${escAttr(f.filterValue || '')}" onchange="RB.updateFilter(${f.id},'val',this.value)">`;
                    if (isBetween) {
                        valueHtml += `<span class="rb-filter-between-sep">to</span>
                            <input type="date" class="form-control rb-filter-val" value="${escAttr(f.filterValue2 || '')}" onchange="RB.updateFilter(${f.id},'val2',this.value)">`;
                    }
                } else if (colType === 'numeric') {
                    valueHtml = `<input type="number" class="form-control rb-filter-val" value="${escAttr(f.filterValue || '')}" onchange="RB.updateFilter(${f.id},'val',this.value)" placeholder="Value" step="any">`;
                    if (isBetween) {
                        valueHtml += `<span class="rb-filter-between-sep">to</span>
                            <input type="number" class="form-control rb-filter-val" value="${escAttr(f.filterValue2 || '')}" onchange="RB.updateFilter(${f.id},'val2',this.value)" placeholder="Max" step="any">`;
                    }
                } else {
                    valueHtml = `<input type="text" class="form-control rb-filter-val" value="${escAttr(f.filterValue || '')}" onchange="RB.updateFilter(${f.id},'val',this.value)" placeholder="Value">`;
                    if (isBetween) {
                        valueHtml += `<span class="rb-filter-between-sep">to</span>
                            <input type="text" class="form-control rb-filter-val" value="${escAttr(f.filterValue2 || '')}" onchange="RB.updateFilter(${f.id},'val2',this.value)" placeholder="Max">`;
                    }
                }
            }

            html += `
                <div class="rb-smart-filter" data-fid="${f.id}">
                    <div class="rb-filter-row-top">
                        ${idx > 0 ? `<select class="form-select rb-filter-logic" onchange="RB.updateFilter(${f.id},'logic',this.value)">
                            <option value="AND" ${f.logicOperator === 'AND' ? 'selected' : ''}>AND</option>
                            <option value="OR" ${f.logicOperator === 'OR' ? 'selected' : ''}>OR</option>
                        </select>` : '<span class="rb-filter-logic-placeholder">WHERE</span>'}
                        <span class="rb-filter-type-badge ${typeClass}">${colType === 'numeric' ? '123' : colType === 'date' ? '📅' : colType === 'boolean' ? '⊘' : 'Aa'}</span>
                        <select class="form-select rb-filter-col" onchange="RB.onFilterColumnChange(${f.id},this.value)">${colOpts}</select>
                        <button class="btn-remove" onclick="RB.removeFilter(${f.id})" title="Remove"><i class="bi bi-x-circle"></i></button>
                    </div>
                    <div class="rb-filter-row-bottom">
                        <select class="form-select rb-filter-op" onchange="RB.onFilterOperatorChange(${f.id},this.value)">${opOpts}</select>
                        ${valueHtml}
                    </div>
                    ${isNullOp ? '<div class="rb-filter-null-hint">No value needed</div>' : ''}
                </div>`;
        });
        $('#rbFilterList').html(html);
    }

    function onFilterColumnChange(id, value) {
        const f = filters.find(x => x.id === id);
        if (!f) return;
        const oldType = getColumnType(f.columnName);
        f.columnName = value;
        const newType = getColumnType(value);

        // Reset operator and value when column type changes
        if (oldType !== newType) {
            const ops = getOperatorsForType(newType);
            f.operator = newType === 'text' ? 'contains' : 'eq';
            f.filterValue = '';
            f.filterValue2 = '';
        }
        renderFilters();
    }

    function onFilterOperatorChange(id, value) {
        const f = filters.find(x => x.id === id);
        if (!f) return;
        f.operator = value;
        renderFilters(); // Re-render to show/hide value inputs
    }

    function updateFilter(id, field, value) {
        const f = filters.find(x => x.id === id);
        if (!f) return;
        if (field === 'col') f.columnName = value;
        else if (field === 'op') f.operator = value;
        else if (field === 'val') f.filterValue = value;
        else if (field === 'val2') f.filterValue2 = value;
        else if (field === 'logic') f.logicOperator = value;
    }

    // ══════════════════════════════════════════════════════════════════════
    //  Group By
    // ══════════════════════════════════════════════════════════════════════

    function addGroupBy() {
        if (columns.length === 0) { Swal.fire('Info', 'Select a table first.', 'info'); return; }
        const avail = columns.filter(c => !groupBys.includes(c.name));
        if (avail.length === 0) return;
        groupBys.push(avail[0].name);
        renderGroupBys();
        $('#rbAggregatesSection').show();
    }

    function removeGroupBy(idx) {
        groupBys.splice(idx, 1);
        renderGroupBys();
        if (groupBys.length === 0) {
            $('#rbAggregatesSection').hide();
            aggregates = [];
            renderAggregates();
        }
    }

    function renderGroupBys() {
        if (groupBys.length === 0) {
            $('#rbGroupByList').html('<div class="text-muted small text-center py-2">No grouping</div>');
            return;
        }

        const colOpts = columns.map(c => `<option value="${c.name}">${c.displayName}</option>`).join('');
        let html = '';
        groupBys.forEach((g, idx) => {
            html += `
                <div class="rb-group-item">
                    <i class="bi bi-grip-vertical" style="opacity:.3; font-size:.7rem;"></i>
                    <select class="form-select flex-grow-1" onchange="RB.updateGroupBy(${idx},this.value)">${colOpts.replace(`value="${g}"`, `value="${g}" selected`)}</select>
                    <button class="btn-remove" onclick="RB.removeGroupBy(${idx})"><i class="bi bi-x-circle"></i></button>
                </div>`;
        });
        $('#rbGroupByList').html(html);
    }

    function updateGroupBy(idx, value) {
        groupBys[idx] = value;
    }

    // ══════════════════════════════════════════════════════════════════════
    //  Order By
    // ══════════════════════════════════════════════════════════════════════

    function addOrderBy() {
        if (columns.length === 0) { Swal.fire('Info', 'Select a table first.', 'info'); return; }
        orderBys.push({ column: columns[0].name, dir: 'ASC' });
        renderOrderBys();
    }

    function removeOrderBy(idx) {
        orderBys.splice(idx, 1);
        renderOrderBys();
    }

    function renderOrderBys() {
        if (orderBys.length === 0) {
            $('#rbOrderByList').html('<div class="text-muted small text-center py-2">Default order</div>');
            return;
        }

        const colOpts = columns.map(c => `<option value="${c.name}">${c.displayName}</option>`).join('');
        let html = '';
        orderBys.forEach((o, idx) => {
            html += `
                <div class="rb-order-item">
                    <select class="form-select flex-grow-1" onchange="RB.updateOrderBy(${idx},'col',this.value)">${colOpts.replace(`value="${o.column}"`, `value="${o.column}" selected`)}</select>
                    <select class="form-select" onchange="RB.updateOrderBy(${idx},'dir',this.value)" style="width:65px;">
                        <option value="ASC" ${o.dir === 'ASC' ? 'selected' : ''}>ASC</option>
                        <option value="DESC" ${o.dir === 'DESC' ? 'selected' : ''}>DESC</option>
                    </select>
                    <button class="btn-remove" onclick="RB.removeOrderBy(${idx})"><i class="bi bi-x-circle"></i></button>
                </div>`;
        });
        $('#rbOrderByList').html(html);
    }

    function updateOrderBy(idx, field, value) {
        if (field === 'col') orderBys[idx].column = value;
        else if (field === 'dir') orderBys[idx].dir = value;
    }

    // ══════════════════════════════════════════════════════════════════════
    //  Aggregates
    // ══════════════════════════════════════════════════════════════════════

    function addAggregate() {
        const numCols = columns.filter(c => c.isNumeric);
        if (numCols.length === 0) { Swal.fire('Info', 'No numeric columns available.', 'info'); return; }
        aggregates.push({ column: numCols[0].name, fn: 'SUM' });
        renderAggregates();
    }

    function removeAggregate(idx) {
        aggregates.splice(idx, 1);
        renderAggregates();
    }

    function renderAggregates() {
        if (aggregates.length === 0) {
            $('#rbAggregateList').html('<div class="text-muted small text-center py-2">No aggregates</div>');
            return;
        }

        const numCols = columns.filter(c => c.isNumeric);
        const colOpts = numCols.map(c => `<option value="${c.name}">${c.displayName}</option>`).join('');
        const fnOpts = '<option value="SUM">SUM</option><option value="AVG">AVG</option><option value="COUNT">COUNT</option><option value="MIN">MIN</option><option value="MAX">MAX</option>';

        let html = '';
        aggregates.forEach((a, idx) => {
            html += `
                <div class="rb-agg-item">
                    <select class="form-select" onchange="RB.updateAggregate(${idx},'fn',this.value)" style="width:70px;">${fnOpts.replace(`value="${a.fn}"`, `value="${a.fn}" selected`)}</select>
                    <select class="form-select flex-grow-1" onchange="RB.updateAggregate(${idx},'col',this.value)">${colOpts.replace(`value="${a.column}"`, `value="${a.column}" selected`)}</select>
                    <button class="btn-remove" onclick="RB.removeAggregate(${idx})"><i class="bi bi-x-circle"></i></button>
                </div>`;
        });
        $('#rbAggregateList').html(html);
    }

    function updateAggregate(idx, field, value) {
        if (field === 'col') aggregates[idx].column = value;
        else if (field === 'fn') aggregates[idx].fn = value;
    }

    // ══════════════════════════════════════════════════════════════════════
    //  Execute Report
    // ══════════════════════════════════════════════════════════════════════

    async function executeReport() {
        const sourceTable = $('#rbTableSelect').val();
        if (!sourceTable) { Swal.fire('Info', 'Select a data source first.', 'info'); return; }
        if (selectedColumns.length === 0 && reportType !== 'summary') { Swal.fire('Info', 'Select at least one column.', 'info'); return; }

        showLoading(true);

        const dto = {
            sourceTable,
            columns: selectedColumns,
            filters: filters.map(f => ({
                columnName: f.columnName,
                operator: f.operator,
                filterValue: f.filterValue || null,
                filterValue2: f.filterValue2 || null,
                logicOperator: f.logicOperator
            })),
            orderByColumns: orderBys,
            groupByColumns: groupBys,
            aggregates: aggregates.map(a => ({ column: a.column, function: a.fn })),
            joinedTables: joinedTables.length > 0 ? joinedTables : null,
            reportType,
            showTotals: $('#rbShowTotals').is(':checked'),
            showGrandTotal: $('#rbShowGrandTotal').is(':checked'),
            page: currentPage,
            pageSize: parseInt($('#rbPageSize').val()) || 25
        };

        try {
            const result = await $.ajax({ url: `${API}/execute`, type: 'POST', contentType: 'application/json', data: JSON.stringify(dto) });

            resultData = result.data;
            resultColumns = result.columnNames;
            totalCount = result.totalCount;
            currentPage = result.page;
            totalPages = result.totalPages;

            renderTable(result.totals);
            renderPagination();
            showLoading(false);

            $('#rbResultsEmpty').hide();
            $('#rbTableContainer').show();
            $('#rbPagination').toggle(result.reportType === 'detail');
            $('#rbRowCountBadge').text(`${totalCount.toLocaleString()} rows`);
            $('#rbPageBadge').text(result.reportType === 'summary' ? 'Summary' : `Page ${currentPage}/${totalPages}`);
            $('#rbStatRows').text(totalCount.toLocaleString());

            // Switch to Results tab
            switchTab('rbTabResults');

            // Execution metrics
            if (result.executionTimeMs !== undefined && result.executionTimeMs !== null) {
                $('#rbExecTimeVal').text(result.executionTimeMs);
                $('#rbExecTimeBadge').show();
            } else {
                $('#rbExecTimeBadge').hide();
            }
            if (result.queryPlanId) {
                $('#rbQueryPlanVal').text(result.queryPlanId);
                $('#rbQueryPlanBadge').show();
            } else {
                $('#rbQueryPlanBadge').hide();
            }

        } catch (e) {
            showLoading(false);
            console.error('Execute failed', e);
            Swal.fire('Error', e.responseJSON?.message || 'Failed to execute report.', 'error');
        }
    }

    function renderTable(totals) {
        if (resultData.length === 0) {
            $('#rbTableHead').html('<tr><th class="text-center py-3">No data found</th></tr>');
            $('#rbTableBody').html('');
            $('#rbTableFoot').hide();
            return;
        }

        // Header
        let thead = '<tr>';
        thead += '<th style="width:40px;">#</th>';
        resultColumns.forEach(col => {
            const colInfo = columns.find(c => c.name === col) || findJoinedColInfo(col);
            const display = colInfo ? colInfo.displayName : humanize(col);
            thead += `<th onclick="RB.sortByColumn('${col}')" title="${col}">
                ${display}
                <i class="bi bi-arrow-down-up sort-icon"></i>
            </th>`;
        });
        thead += '</tr>';
        $('#rbTableHead').html(thead);

        // Body
        const pageSize = parseInt($('#rbPageSize').val()) || 25;
        const startRow = (currentPage - 1) * pageSize;
        let tbody = '';
        resultData.forEach((row, idx) => {
            tbody += '<tr>';
            tbody += `<td class="text-muted">${startRow + idx + 1}</td>`;
            resultColumns.forEach(col => {
                const val = row[col];
                const colInfo = columns.find(c => c.name === col) || findJoinedColInfo(col);
                tbody += `<td>${formatValue(val, colInfo)}</td>`;
            });
            tbody += '</tr>';
        });
        $('#rbTableBody').html(tbody);

        // Totals footer
        if (totals && Object.keys(totals).length > 0) {
            let tfoot = '<tr class="rb-totals-row">';
            tfoot += '<td class="fw-bold">Σ</td>';
            resultColumns.forEach(col => {
                const sumKey = `${col}_sum`;
                if (totals[sumKey] !== undefined) {
                    const sum = Number(totals[sumKey]);
                    tfoot += `<td class="fw-bold text-end" title="Sum: ${sum.toLocaleString(undefined, {maximumFractionDigits:2})}">${sum.toLocaleString(undefined, {maximumFractionDigits:2})}</td>`;
                } else {
                    tfoot += '<td></td>';
                }
            });
            tfoot += '</tr>';
            $('#rbTableFoot').html(tfoot).show();
        } else {
            $('#rbTableFoot').hide();
        }
    }

    function findJoinedColInfo(colName) {
        for (const [tbl, cols] of Object.entries(joinedColumns)) {
            const alias = colName.replace(`${tbl}_`, '');
            const found = cols.find(c => c.name === alias || `${tbl}_${c.name}` === colName);
            if (found) return found;
        }
        return null;
    }

    function findColInfo(colName) {
        // Check primary columns
        const primary = columns.find(c => c.name === colName);
        if (primary) return primary;
        // Check joined columns
        if (colName.includes('.')) {
            const parts = colName.split('.', 2);
            if (joinedColumns[parts[0]]) {
                return joinedColumns[parts[0]].find(c => c.name === parts[1]);
            }
        }
        return findJoinedColInfo(colName);
    }

    function formatValue(val, colInfo) {
        if (val === null || val === undefined) return '<span class="rb-null">null</span>';
        if (colInfo?.isBoolean || val === true || val === false) {
            return val === true ? '<span class="rb-bool-true"><i class="bi bi-check-circle-fill"></i> Yes</span>'
                : '<span class="rb-bool-false"><i class="bi bi-circle"></i> No</span>';
        }
        if (colInfo?.isDate && typeof val === 'string' && val.includes('T')) {
            try {
                const d = new Date(val);
                return d.toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' }) + ' ' +
                       d.toLocaleTimeString('en-GB', { hour: '2-digit', minute: '2-digit' });
            } catch { return escHtml(String(val)); }
        }
        if (colInfo?.isNumeric && typeof val === 'number') {
            return val.toLocaleString(undefined, { maximumFractionDigits: 4 });
        }
        const s = String(val);
        return s.length > 80 ? escHtml(s.substring(0, 80)) + '…' : escHtml(s);
    }

    function sortByColumn(col) {
        const existing = orderBys.find(o => o.column === col);
        if (existing) {
            existing.dir = existing.dir === 'ASC' ? 'DESC' : 'ASC';
        } else {
            orderBys = [{ column: col, dir: 'ASC' }];
        }
        renderOrderBys();
        currentPage = 1;
        executeReport();
    }

    // ── Pagination ──
    function renderPagination() {
        const pageSize = parseInt($('#rbPageSize').val()) || 25;
        const from = totalCount === 0 ? 0 : (currentPage - 1) * pageSize + 1;
        const to = Math.min(currentPage * pageSize, totalCount);
        $('#rbPaginationInfo').text(`Showing ${from}–${to} of ${totalCount.toLocaleString()}`);
        $('#rbPageDisplay').text(`${currentPage} / ${totalPages}`);
        $('#rbBtnFirst, #rbBtnPrev').prop('disabled', currentPage <= 1);
        $('#rbBtnLast, #rbBtnNext').prop('disabled', currentPage >= totalPages);
    }

    function goToPage(p) {
        if (p === -1) p = totalPages;
        if (p < 1 || p > totalPages) return;
        currentPage = p;
        executeReport();
    }

    function nextPage() { goToPage(currentPage + 1); }
    function prevPage() { goToPage(currentPage - 1); }

    // ══════════════════════════════════════════════════════════════════════
    //  Save / Load
    // ══════════════════════════════════════════════════════════════════════

    async function saveReport() {
        const code = $('#rbReportCode').val().trim();
        const name = $('#rbReportName').val().trim();
        const sourceTable = $('#rbTableSelect').val();

        if (!code) { Swal.fire('Required', 'Enter a report code.', 'warning'); return; }
        if (!name) { Swal.fire('Required', 'Enter a report name.', 'warning'); return; }
        if (!sourceTable) { Swal.fire('Required', 'Select a data source.', 'warning'); return; }

        const dto = {
            reportId: currentReportId,
            reportCode: code,
            reportName: name,
            description: $('#rbDescription').val() || null,
            sourceTable,
            isShared: $('#rbIsShared').is(':checked'),
            pageSize: parseInt($('#rbPageSize').val()) || 25,
            reportType,
            showTotals: $('#rbShowTotals').is(':checked'),
            showGrandTotal: $('#rbShowGrandTotal').is(':checked'),
            groupByColumns: groupBys.length > 0 ? groupBys : null,
            orderByColumns: orderBys.length > 0 ? orderBys : null,
            joinedTables: joinedTables.length > 0 ? joinedTables : null,
            chartType: null,
            chartConfig: null,
            columns: selectedColumns.map((c, i) => ({
                columnName: c,
                displayName: columns.find(x => x.name === c)?.displayName || c,
                isVisible: true,
                aggregateFunction: null,
                formatString: null,
                columnWidth: null
            })),
            filters: filters.map(f => ({
                columnName: f.columnName,
                operator: f.operator,
                filterValue: f.filterValue || null,
                filterValue2: f.filterValue2 || null,
                logicOperator: f.logicOperator
            }))
        };

        try {
            const result = await $.ajax({ url: `${API}/saved`, type: 'POST', contentType: 'application/json', data: JSON.stringify(dto) });
            currentReportId = result.id;
            $('#rbReportId').val(result.id);
            Swal.fire({ icon: 'success', title: 'Saved!', text: result.message, timer: 1500, showConfirmButton: false });
            loadSavedReports();
        } catch (e) {
            Swal.fire('Error', e.responseJSON?.message || 'Failed to save report.', 'error');
        }
    }

    async function openReport(id) {
        try {
            const r = await $.get(`${API}/saved/${id}`);
            currentReportId = r.reportId;
            $('#rbReportId').val(r.reportId);
            $('#rbReportCode').val(r.reportCode);
            $('#rbReportName').val(r.reportName);
            $('#rbDescription').val(r.description || '');
            $('#rbIsShared').prop('checked', r.isShared);
            $('#rbPageSize').val(r.pageSize || 25);

            // Restore report type
            reportType = r.reportType || 'detail';
            setReportType(reportType);
            $('#rbShowTotals').prop('checked', r.showTotals || false);
            $('#rbShowGrandTotal').prop('checked', r.showGrandTotal || false);

            // Set table
            $('#rbTableSelect').val(r.sourceTable);
            columns = await $.get(`${API}/tables/${r.sourceTable}/columns`);

            // Load relationships
            try {
                relationships = await $.get(`${API}/tables/${r.sourceTable}/relationships`);
            } catch { relationships = { outgoing: [], incoming: [] }; }

            showTablePreview(r.sourceTable);

            // Restore joined tables
            joinedTables = [];
            joinedColumns = {};
            if (r.joinedTables) {
                try {
                    const jts = typeof r.joinedTables === 'string' ? JSON.parse(r.joinedTables) : r.joinedTables;
                    if (Array.isArray(jts)) {
                        joinedTables = jts.map(j => ({
                            table: j.table || j.Table,
                            joinType: j.joinType || j.JoinType || 'LEFT',
                            fkColumn: j.fkColumn || j.FkColumn,
                            pkColumn: j.pkColumn || j.PkColumn
                        }));
                        for (const jt of joinedTables) {
                            try {
                                const cols = await $.get(`${API}/tables/${jt.table}/columns`);
                                joinedColumns[jt.table] = cols;
                            } catch { }
                        }
                    }
                } catch { }
            }
            renderJoins();

            // Restore selections
            selectedColumns = (r.columns || []).map(c => c.columnName);
            filters = (r.filters || []).map((f, i) => ({ id: nextFilterId++, columnName: f.columnName, operator: f.operator, filterValue: f.filterValue || '', filterValue2: f.filterValue2 || '', logicOperator: f.logicOperator || 'AND' }));

            // Restore group by and order by from JSON
            groupBys = [];
            orderBys = [];
            if (r.groupByColumns) {
                try { groupBys = typeof r.groupByColumns === 'string' ? JSON.parse(r.groupByColumns) : r.groupByColumns; } catch { }
            }
            if (r.orderByColumns) {
                try {
                    const parsed = typeof r.orderByColumns === 'string' ? JSON.parse(r.orderByColumns) : r.orderByColumns;
                    orderBys = Array.isArray(parsed) ? parsed.map(o => ({
                        column: o.column || o.Column,
                        dir: o.dir || o.Dir || 'ASC'
                    })) : [];
                } catch { }
            }

            aggregates = [];
            renderColumnList();
            renderFilters();
            renderGroupBys();
            renderOrderBys();
            renderAggregates();
            if (groupBys.length > 0 || reportType === 'summary') $('#rbAggregatesSection').show();

            showDesigner();
            executeReport();

        } catch (e) {
            Swal.fire('Error', e.responseJSON?.message || 'Failed to load report.', 'error');
        }
    }

    async function deleteReport(id) {
        const result = await Swal.fire({ title: 'Delete Report?', text: 'This cannot be undone.', icon: 'warning', showCancelButton: true, confirmButtonText: 'Delete', confirmButtonColor: '#dc3545' });
        if (!result.isConfirmed) return;

        try {
            await $.ajax({ url: `${API}/saved/${id}`, type: 'DELETE' });
            Swal.fire({ icon: 'success', title: 'Deleted!', timer: 1200, showConfirmButton: false });
            loadSavedReports();
        } catch (e) {
            Swal.fire('Error', 'Failed to delete.', 'error');
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    //  AI Summary
    // ══════════════════════════════════════════════════════════════════════

    async function showAiSummary() {
        const sourceTable = $('#rbTableSelect').val();
        if (!sourceTable) { Swal.fire('Info', 'Select a data source first.', 'info'); return; }

        const modal = new bootstrap.Modal(document.getElementById('rbAiModal'));
        modal.show();
        $('#rbAiBody').html('<div class="text-center py-5"><div class="spinner-border text-primary"></div><div class="mt-2 text-muted">Analyzing your data...</div></div>');

        try {
            const dto = { sourceTable, columns: selectedColumns.length > 0 ? selectedColumns : null };
            const data = await $.ajax({ url: `${API}/ai-summary`, type: 'POST', contentType: 'application/json', data: JSON.stringify(dto) });

            let html = '';

            // Overview
            html += `<div class="rb-ai-card">
                <h6><i class="bi bi-info-circle me-1 text-primary"></i>Overview</h6>
                <div class="rb-ai-stat-grid">
                    <div class="rb-ai-stat"><div class="value">${data.totalRows.toLocaleString()}</div><div class="label">Total Rows</div></div>
                    <div class="rb-ai-stat"><div class="value">${data.selectedColumns.length}</div><div class="label">Columns</div></div>
                    <div class="rb-ai-stat"><div class="value">${data.tableName}</div><div class="label">Source</div></div>
                </div>
            </div>`;

            // Numeric stats
            if (data.numericStats && data.numericStats.length > 0) {
                html += `<div class="rb-ai-card">
                    <h6><i class="bi bi-calculator me-1 text-success"></i>Numeric Analysis</h6>
                    <div class="table-responsive"><table class="table table-sm table-bordered mb-0" style="font-size:.78rem;">
                    <thead><tr><th>Column</th><th class="text-end">Count</th><th class="text-end">Sum</th><th class="text-end">Avg</th><th class="text-end">Min</th><th class="text-end">Max</th></tr></thead><tbody>`;
                data.numericStats.forEach(s => {
                    html += `<tr><td class="fw-bold">${s.column}</td><td class="text-end">${Number(s.count).toLocaleString()}</td><td class="text-end">${Number(s.sum).toLocaleString(undefined, {maximumFractionDigits:2})}</td><td class="text-end">${Number(s.avg).toLocaleString(undefined, {maximumFractionDigits:2})}</td><td class="text-end">${Number(s.min).toLocaleString(undefined, {maximumFractionDigits:2})}</td><td class="text-end">${Number(s.max).toLocaleString(undefined, {maximumFractionDigits:2})}</td></tr>`;
                });
                html += '</tbody></table></div></div>';
            }

            // Date range
            if (data.dateRange) {
                html += `<div class="rb-ai-card">
                    <h6><i class="bi bi-calendar-range me-1 text-warning"></i>Date Range</h6>
                    <div class="d-flex gap-3">
                        <div><span class="text-muted small">Column:</span> <strong>${data.dateRange.column}</strong></div>
                        <div><span class="text-muted small">From:</span> <strong>${formatDateDisplay(data.dateRange.from)}</strong></div>
                        <div><span class="text-muted small">To:</span> <strong>${formatDateDisplay(data.dateRange.to)}</strong></div>
                    </div>
                </div>`;
            }

            // Top values
            if (data.topValues) {
                html += `<div class="rb-ai-card">
                    <h6><i class="bi bi-bar-chart me-1 text-info"></i>Top Values — ${data.topValues.column}</h6>
                    <div class="d-flex flex-wrap gap-2">`;
                data.topValues.values.forEach(v => {
                    html += `<span class="badge bg-primary-lt">${escHtml(v.value || '(empty)')} <strong>(${v.count})</strong></span>`;
                });
                html += '</div></div>';
            }

            html += `<div class="text-muted small text-end mt-2"><i class="bi bi-clock me-1"></i>Generated at ${data.generatedAt}</div>`;

            $('#rbAiBody').html(html);
        } catch (e) {
            $('#rbAiBody').html('<div class="text-center text-danger py-3"><i class="bi bi-exclamation-circle me-1"></i>Failed to generate summary.</div>');
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    //  Export
    // ══════════════════════════════════════════════════════════════════════

    function ensureReportReady() {
        const sourceTable = $('#rbTableSelect').val();
        if (!sourceTable) {
            Swal.fire('Info', 'Select a data source and run report first.', 'info');
            return false;
        }
        if (!resultData || resultData.length === 0 || !resultColumns || resultColumns.length === 0) {
            Swal.fire('Info', 'Run report first to prepare export or print.', 'info');
            return false;
        }
        return true;
    }

    function getCurrentUserName() {
        return $('#rbCurrentUserName').val() || 'Current User';
    }

    function getExportContext() {
        return {
            reportCode: ($('#rbReportCode').val() || '').trim(),
            reportName: ($('#rbReportName').val() || '').trim() || 'Ad-hoc Report',
            userName: getCurrentUserName(),
            generatedAt: new Date(),
            pageLabel: reportType === 'summary' ? 'Summary' : `Page ${currentPage}/${totalPages}`
        };
    }

    function getFilterDescriptions() {
        return filters.map((f, idx) => {
            const col = findColInfo(f.columnName);
            const colLabel = col?.displayName || humanize(f.columnName.replace('.', '_'));
            const logic = idx === 0 ? 'WHERE' : (f.logicOperator || 'AND').toUpperCase();
            const op = (f.operator || '').toUpperCase();
            const val1 = f.filterValue || '—';
            const val2 = f.filterValue2 || '—';
            const rhs = op === 'BETWEEN' ? `${val1} and ${val2}` : (op === 'ISNULL' || op === 'ISNOTNULL' ? '' : val1);
            return `${logic} ${colLabel} ${op}${rhs ? ` ${rhs}` : ''}`;
        });
    }

    function getSmartExportSuggestion() {
        const rowCount = totalCount || resultData.length;
        const colCount = resultColumns.length;
        const hasFilters = filters.length > 0;
        if (rowCount > 5000 || colCount > 15) {
            return {
                format: 'csv',
                text: `Smart suggestion: CSV is best for ${rowCount.toLocaleString()} rows and ${colCount} columns (fast and analysis-friendly).`
            };
        }
        if (hasFilters || reportType === 'summary') {
            return {
                format: 'pdf',
                text: 'Smart suggestion: PDF is ideal for sharing filtered/summary insights with readable layout.'
            };
        }
        return {
            format: 'print',
            text: 'Smart suggestion: Print Preview is ideal for quick review and instant hard-copy output.'
        };
    }

    function updateExportRecommendation() {
        const selectedFormat = $('#rbExportFormat').val();
        const recommendation = getSmartExportSuggestion();
        const modeText = selectedFormat ? `Selected: ${selectedFormat.toUpperCase()}. ` : '';
        $('#rbExportRecommendation').text(`${modeText}${recommendation.text}`);
    }

    function openExportDialog() {
        if (!ensureReportReady()) return;
        const recommendation = getSmartExportSuggestion();
        $('#rbExportFormat').val(recommendation.format || 'csv');
        updateExportRecommendation();
        const modal = bootstrap.Modal.getOrCreateInstance(document.getElementById('rbExportModal'));
        modal.show();
    }

    async function confirmExportSelection() {
        if (!ensureReportReady()) return;

        const format = $('#rbExportFormat').val() || 'csv';
        const includeHeader = $('#rbIncludeHeader').is(':checked');
        const includeFooter = $('#rbIncludeFooter').is(':checked');
        const includeFilters = $('#rbIncludeFilters').is(':checked');

        const modal = bootstrap.Modal.getOrCreateInstance(document.getElementById('rbExportModal'));
        modal.hide();

        if (format === 'csv') {
            await exportCsv({ includeHeader, includeFooter, includeFilters });
            return;
        }

        const asPdf = format === 'pdf';
        openPrintPreview({ includeHeader, includeFooter, includeFilters, asPdf });
    }

    async function exportCsv(options = {}) {
        const sourceTable = $('#rbTableSelect').val();
        if (!sourceTable || selectedColumns.length === 0) {
            Swal.fire('Info', 'Run a report first.', 'info');
            return;
        }

        const ctx = getExportContext();
        const dto = {
            sourceTable,
            columns: selectedColumns,
            filters: filters.map(f => ({ columnName: f.columnName, operator: f.operator, filterValue: f.filterValue || null, filterValue2: f.filterValue2 || null, logicOperator: f.logicOperator })),
            orderByColumns: orderBys,
            joinedTables: joinedTables.length > 0 ? joinedTables : null,
            page: 1,
            pageSize: 10000,
            reportName: ctx.reportName,
            reportCode: ctx.reportCode,
            includeHeader: options.includeHeader === true,
            includeFooter: options.includeFooter === true,
            includeFilters: options.includeFilters === true,
            filterDescriptions: options.includeFilters === true ? getFilterDescriptions() : []
        };

        try {
            const response = await fetch(`${API}/export/csv`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(dto)
            });

            if (!response.ok) {
                throw new Error('CSV export request failed.');
            }

            const blob = await response.blob();
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `${ctx.reportName.replace(/\s+/g, '_').toLowerCase()}_${new Date().toISOString().slice(0, 10)}.csv`;
            a.click();
            window.URL.revokeObjectURL(url);
        } catch (e) {
            Swal.fire('Error', 'Export failed.', 'error');
        }
    }

    function buildPrintDocumentHtml(options = {}) {
        const ctx = getExportContext();
        const includeHeader = options.includeHeader !== false;
        const includeFooter = options.includeFooter !== false;
        const includeFilters = options.includeFilters !== false;
        const filterDescriptions = includeFilters ? getFilterDescriptions() : [];

        const headers = resultColumns.map(c => {
            const info = columns.find(x => x.name === c) || findJoinedColInfo(c);
            return `<th>${escHtml(info?.displayName || humanize(c))}</th>`;
        }).join('');

        const rows = resultData.map(row => {
            const cells = resultColumns.map(c => {
                const info = columns.find(x => x.name === c) || findJoinedColInfo(c);
                const raw = row[c];
                let display = '';
                if (raw === null || raw === undefined) display = 'null';
                else if (info?.isDate) display = formatDateDisplay(raw);
                else if (info?.isNumeric && typeof raw === 'number') display = raw.toLocaleString(undefined, { maximumFractionDigits: 4 });
                else display = String(raw);
                return `<td>${escHtml(display)}</td>`;
            }).join('');
            return `<tr>${cells}</tr>`;
        }).join('');

        const filterBlock = includeFilters ? `
            <section class="rb-export-meta-section">
                <h4>Applied Filters</h4>
                ${filterDescriptions.length === 0
                ? '<div class="rb-export-muted">No filters applied.</div>'
                : `<ul>${filterDescriptions.map(f => `<li>${escHtml(f)}</li>`).join('')}</ul>`}
            </section>` : '';

        const headerBlock = includeHeader ? `
            <header class="rb-export-header">
                <div>
                    <h2>${escHtml(ctx.reportName)}</h2>
                    <div class="rb-export-muted">${escHtml(ctx.reportCode || 'No report code')}</div>
                </div>
                <div class="rb-export-meta-grid">
                    <div><span>User</span><strong>${escHtml(ctx.userName)}</strong></div>
                    <div><span>Generated</span><strong>${escHtml(ctx.generatedAt.toLocaleString())}</strong></div>
                    <div><span>Rows</span><strong>${(totalCount || resultData.length).toLocaleString()}</strong></div>
                    <div><span>View</span><strong>${escHtml(ctx.pageLabel)}</strong></div>
                </div>
            </header>` : '';

        const footerBlock = includeFooter ? `
            <footer class="rb-export-footer">
                <div>Report: ${escHtml(ctx.reportName)}</div>
                <div>Generated by ${escHtml(ctx.userName)}</div>
            </footer>` : '';

        return `<!doctype html>
<html>
<head>
    <meta charset="utf-8" />
    <title>${escHtml(ctx.reportName)} - Export</title>
    <style>
        body { font-family: Segoe UI, Arial, sans-serif; margin: 24px; color:#1f2937; }
        .rb-export-header { display:flex; justify-content:space-between; gap:20px; margin-bottom:16px; border-bottom:1px solid #d1d5db; padding-bottom:10px; }
        .rb-export-header h2 { margin:0; font-size:20px; }
        .rb-export-meta-grid { display:grid; grid-template-columns:repeat(2,minmax(120px,1fr)); gap:6px 12px; font-size:12px; }
        .rb-export-meta-grid span { color:#6b7280; display:block; }
        .rb-export-meta-grid strong { font-size:12px; }
        .rb-export-meta-section { margin:12px 0; font-size:12px; }
        .rb-export-meta-section h4 { margin:0 0 6px; font-size:13px; }
        .rb-export-meta-section ul { margin:0; padding-left:18px; }
        .rb-export-muted { color:#6b7280; }
        table { width:100%; border-collapse:collapse; margin-top:10px; }
        th, td { border:1px solid #d1d5db; padding:6px 8px; font-size:12px; vertical-align:top; }
        th { background:#f3f4f6; text-align:left; }
        .rb-export-footer { margin-top:14px; border-top:1px solid #d1d5db; padding-top:8px; display:flex; justify-content:space-between; font-size:11px; color:#4b5563; }
    </style>
</head>
<body>
    ${headerBlock}
    ${filterBlock}
    <table>
        <thead><tr>${headers}</tr></thead>
        <tbody>${rows}</tbody>
    </table>
    ${footerBlock}
</body>
</html>`;
    }

    function openPrintPreview(options = {}) {
        const html = buildPrintDocumentHtml(options);
        const printWin = window.open('', '_blank');
        if (!printWin) {
            Swal.fire('Popup blocked', 'Please allow popups to print/export report.', 'warning');
            return;
        }

        printWin.document.open();
        printWin.document.write(html);
        printWin.document.close();

        setTimeout(() => {
            printWin.focus();
            printWin.print();
        }, 250);

        if (options.asPdf) {
            Swal.fire('PDF Export', 'In the print dialog, choose "Save as PDF" to download the PDF report.', 'info');
        }
    }

    function printReport() {
        openExportDialog();
    }

    // ══════════════════════════════════════════════════════════════════════
    //  Saved Reports List
    // ══════════════════════════════════════════════════════════════════════

    function renderSavedList() {
        const search = ($('#rbSearchSaved').val() || '').toLowerCase();
        const filtered = savedReports.filter(r =>
            r.reportName.toLowerCase().includes(search) ||
            r.reportCode.toLowerCase().includes(search) ||
            (r.description || '').toLowerCase().includes(search)
        );

        if (filtered.length === 0) {
            $('#rbSavedGrid').html(`
                <div class="col-12 text-center py-5">
                    <i class="bi bi-inbox" style="font-size:2.5rem; opacity:.3;"></i>
                    <div class="text-muted mt-2">No reports found</div>
                    <button class="btn btn-primary btn-sm mt-3" onclick="RB.newReport()">
                        <i class="bi bi-plus-lg me-1"></i>Create Your First Report
                    </button>
                </div>`);
            return;
        }

        let html = '';
        filtered.forEach(r => {
            html += `
                <div class="col-sm-6 col-lg-4 col-xl-3">
                    <div class="card rb-report-card" onclick="RB.openReport(${r.reportId})">
                        <div class="card-body">
                            <div class="d-flex align-items-center gap-2 mb-2">
                                <div class="rb-card-icon"><i class="bi bi-file-earmark-bar-graph"></i></div>
                                <div>
                                    <div class="rb-card-title">${escHtml(r.reportName)}</div>
                                    <div class="rb-card-code">${escHtml(r.reportCode)}</div>
                                </div>
                            </div>
                            ${r.description ? `<div class="text-muted small mb-2">${escHtml(r.description).substring(0, 80)}</div>` : ''}
                            <div class="rb-card-meta">
                                <span class="rb-card-meta-item"><i class="bi bi-table"></i>${escHtml(humanize(r.sourceTable))}</span>
                                <span class="rb-card-meta-item"><i class="bi bi-columns-gap"></i>${r.columnCount} cols</span>
                                <span class="rb-card-meta-item"><i class="bi bi-funnel"></i>${r.filterCount} filters</span>
                                ${r.isShared ? '<span class="badge bg-success-lt">Shared</span>' : ''}
                            </div>
                            <div class="rb-card-actions" onclick="event.stopPropagation();">
                                <button class="btn btn-sm btn-ghost-primary flex-fill" onclick="RB.openReport(${r.reportId})">
                                    <i class="bi bi-pencil me-1"></i>Open
                                </button>
                                ${r.isOwner ? `<button class="btn btn-sm btn-ghost-danger" onclick="RB.deleteReport(${r.reportId})"><i class="bi bi-trash"></i></button>` : ''}
                            </div>
                        </div>
                    </div>
                </div>`;
        });
        $('#rbSavedGrid').html(html);
    }

    function filterSavedList() {
        renderSavedList();
    }

    function updateStats() {
        $('#rbStatSaved').text(savedReports.length);
        $('#rbStatShared').text(savedReports.filter(r => r.isShared).length);
    }

    // ══════════════════════════════════════════════════════════════════════
    //  View Toggle
    // ══════════════════════════════════════════════════════════════════════

    function showDesigner() {
        $('#rbSavedListView').hide();
        $('#rbDesignerView').show();
    }

    function showSavedList() {
        $('#rbDesignerView').hide();
        $('#rbSavedListView').show();
        loadSavedReports();
    }

    function newReport() {
        currentReportId = 0;
        selectedColumns = [];
        filters = [];
        groupBys = [];
        orderBys = [];
        aggregates = [];
        joinedTables = [];
        joinedColumns = {};
        relationships = { outgoing: [], incoming: [] };
        reportType = 'detail';
        resultData = [];
        resultColumns = [];
        currentPage = 1;
        totalPages = 1;
        totalCount = 0;
        activeCategory = null;

        $('#rbReportId').val(0);
        $('#rbReportCode').val('RPT-' + Date.now().toString(36).toUpperCase());
        $('#rbReportName').val('');
        $('#rbDescription').val('');
        $('#rbIsShared').prop('checked', false);
        $('#rbShowTotals').prop('checked', false);
        $('#rbShowGrandTotal').prop('checked', false);
        $('#rbTableSelect').val('');
        $('#rbTableSearch').val('');
        $('#rbPageSize').val('25');
        columns = [];

        setReportType('detail');
        renderCategoryChips();
        renderColumnList();
        renderFilters();
        renderGroupBys();
        renderOrderBys();
        renderAggregates();
        renderJoins();
        $('#rbAggregatesSection').hide();
        $('#rbResultsEmpty').show();
        $('#rbTableContainer').hide();
        $('#rbPagination').hide();
        $('#rbTableFoot').hide();
        $('#rbTableInfo').text('');
        $('#rbTablePreview').hide();
        $('#rbExecTimeBadge').hide();
        $('#rbQueryPlanBadge').hide();

        switchTab('rbTabReportInfo');
        showDesigner();
    }

    // ══════════════════════════════════════════════════════════════════════
    //  Helpers
    // ══════════════════════════════════════════════════════════════════════

    function showLoading(show) {
        if (show) {
            if (!$('#rbLoadingOverlay').length) {
                $('#rbResultsWrapper').append('<div class="rb-loading" id="rbLoadingOverlay"><div class="spinner-border text-primary"></div></div>');
            }
        } else {
            $('#rbLoadingOverlay').remove();
        }
    }

    function escHtml(str) {
        const d = document.createElement('div');
        d.textContent = str;
        return d.innerHTML;
    }

    function escAttr(str) {
        return String(str).replace(/&/g, '&amp;').replace(/"/g, '&quot;').replace(/'/g, '&#39;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
    }

    function humanize(name) {
        let n = name;
        ['mst_', 'trn_', 'hr_', 'hyb_', 'rpt_', 'vw_', 'sys_', 'txn_'].forEach(pfx => {
            if (n.startsWith(pfx)) { n = n.substring(pfx.length); }
        });
        return n.split('_').map(w => w.charAt(0).toUpperCase() + w.slice(1)).join(' ');
    }

    function formatDateDisplay(val) {
        if (!val) return '—';
        try {
            return new Date(val).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
        } catch { return String(val); }
    }

    // ── Collapsible Section Toggle ──
    function toggleSection(el) {
        const body = el.nextElementSibling;
        const isCollapsed = el.classList.toggle('collapsed');
        body.style.display = isCollapsed ? 'none' : '';
    }

    // ── Public API ──
    return {
        init, newReport, showSavedList, showDesigner, switchTab,
        openReport, saveReport, deleteReport,
        executeReport, exportCsv, printReport, openExportDialog, confirmExportSelection,
        showAiSummary,
        toggleColumn, selectAllColumns, deselectAllColumns,
        addFilter, addSmartFilter, removeFilter, updateFilter, clearAllFilters,
        onFilterColumnChange, onFilterOperatorChange,
        addGroupBy, removeGroupBy, updateGroupBy,
        addOrderBy, removeOrderBy, updateOrderBy,
        addAggregate, removeAggregate, updateAggregate,
        addJoin, removeJoin, updateJoinType,
        setReportType, toggleSection,
        sortByColumn,
        goToPage, nextPage, prevPage,
        selectTable, clearTable, filterByCategory,
        columnContextMenu, addQuickFilter, addQuickSort, addQuickGroupBy
    };
})();

$(document).ready(function () {
    RB.init();
});
