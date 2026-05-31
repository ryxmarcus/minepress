/* ═══════════════════════════════════════════════════════════════
   Item Management — Client-Side Module
   ═══════════════════════════════════════════════════════════════ */
const ImApp = (function () {
    'use strict';

    const API = '/api/ItemManagement';
    let currentGroup = 'ALL';
    let currentPage = 1;
    const pageSize = 25;
    let searchTimer = null;

    // ── Helpers ──────────────────────────────────────────────────
    function esc(s) { if (!s) return ''; const d = document.createElement('div'); d.textContent = s; return d.innerHTML; }
    function fmt(v, dec) { return v != null ? Number(v).toFixed(dec ?? 2) : '—'; }
    function q(id) { return document.getElementById(id); }

    async function fetchJson(url) {
        const r = await fetch(url);
        if (!r.ok) { const e = await r.json().catch(() => ({})); throw new Error(e.message || r.statusText); }
        return r.json();
    }
    async function postJson(url, body) {
        const r = await fetch(url, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) });
        const d = await r.json().catch(() => ({}));
        if (!r.ok) throw new Error(d.message || r.statusText);
        return d;
    }
    async function putJson(url, body) {
        const r = await fetch(url, { method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) });
        const d = await r.json().catch(() => ({}));
        if (!r.ok) throw new Error(d.message || r.statusText);
        return d;
    }

    // ── Group colours / icons ───────────────────────────────────
    const groupMeta = {
        CHEMICAL: { icon: 'bi-droplet-half', css: 'group-chemical', color: '#0ea5e9' },
        INK:      { icon: 'bi-palette',      css: 'group-ink',      color: '#ec4899' },
        PAPER:    { icon: 'bi-file-earmark',  css: 'group-paper',    color: '#22c55e' },
        PLATE:    { icon: 'bi-disc',          css: 'group-plate',    color: '#f59e0b' },
        OTHER:    { icon: 'bi-three-dots',    css: 'group-other',    color: '#6366f1' }
    };

    // ── Init ────────────────────────────────────────────────────
    function init() {
        loadKpis();
        loadFilters();
        loadItems();

        // Group selector radio cards → sync hidden select + toggle fields
        document.querySelectorAll('#groupSelectorRow input[name="itemGroupRadio"]').forEach(radio => {
            radio.addEventListener('change', function () {
                q('fItemGroup').value = this.value;
                updateModalGroupTheme(this.value);
                toggleGroupFields();
            });
        });
    }

    // ── KPIs ────────────────────────────────────────────────────
    async function loadKpis() {
        try {
            const d = await fetchJson(`${API}/kpis`);
            q('kpiTotal').textContent = d.total;
            q('kpiActive').textContent = d.active;
            q('kpiInactive').textContent = d.inactive;
            q('kpiLowStock').textContent = d.lowStock;

            // group counts for tabs
            q('cntAll').textContent = d.total;
            q('cntChemical').textContent = d.groups['CHEMICAL'] || 0;
            q('cntInk').textContent = d.groups['INK'] || 0;
            q('cntPaper').textContent = d.groups['PAPER'] || 0;
            q('cntPlate').textContent = d.groups['PLATE'] || 0;
            q('cntOther').textContent = d.groups['OTHER'] || 0;

            // AI bar
            const msgs = [];
            if (d.ai.outOfStock > 0) msgs.push(`<span class="ai-badge" style="background:rgba(239,68,68,.12);color:#ef4444;"><i class="bi bi-exclamation-circle me-1"></i>${d.ai.outOfStock} out of stock</span>`);
            if (d.ai.lowStock > 0) msgs.push(`<span class="ai-badge"><i class="bi bi-exclamation-triangle me-1"></i>${d.lowStock} low stock</span>`);
            if (d.ai.noRate > 0) msgs.push(`<span class="ai-badge" style="background:rgba(99,102,241,.12);color:#6366f1;"><i class="bi bi-currency-dollar me-1"></i>${d.ai.noRate} missing rate</span>`);
            if (d.ai.noHsn > 0) msgs.push(`<span class="ai-badge" style="background:rgba(236,72,153,.12);color:#ec4899;"><i class="bi bi-tag me-1"></i>${d.ai.noHsn} missing HSN</span>`);
            if (d.ai.staleItems > 0) msgs.push(`<span class="ai-badge" style="background:rgba(245,158,11,.12);color:#f59e0b;"><i class="bi bi-clock me-1"></i>${d.ai.staleItems} stale (>6mo)</span>`);

            q('imAiText').innerHTML = msgs.length
                ? `<strong>AI Insights:</strong> ` + msgs.join(' ')
                : `<strong>AI Insights:</strong> <span class="text-success"><i class="bi bi-check-circle me-1"></i>All items healthy — no issues detected.</span>`;
        } catch (e) {
            console.error('KPI error', e);
        }
    }

    // ── Filters ─────────────────────────────────────────────────
    async function loadFilters() {
        try {
            const d = await fetchJson(`${API}/filters`);
            const catSel = q('imFilterCategory');
            d.categories.forEach(c => { const o = document.createElement('option'); o.value = c; o.textContent = c; catSel.appendChild(o); });
            const uomSel = q('imFilterUom');
            d.uoms.forEach(u => { const o = document.createElement('option'); o.value = u; o.textContent = u; uomSel.appendChild(o); });
        } catch (e) { console.error('Filter error', e); }
    }

    // ── Group Tab ───────────────────────────────────────────────
    function filterGroup(grp) {
        currentGroup = grp;
        currentPage = 1;
        document.querySelectorAll('.im-group-tab').forEach(t => t.classList.toggle('active', t.dataset.group === grp));
        loadItems();
    }

    // ── Search debounce ─────────────────────────────────────────
    function debounceSearch() {
        clearTimeout(searchTimer);
        searchTimer = setTimeout(() => { currentPage = 1; loadItems(); }, 350);
    }

    // ── Load Items ──────────────────────────────────────────────
    async function loadItems() {
        try {
            const params = new URLSearchParams();
            params.set('page', currentPage);
            params.set('size', pageSize);
            if (currentGroup !== 'ALL') params.set('group', currentGroup);

            const search = q('imSearch').value.trim();
            if (search) params.set('q', search);

            const cat = q('imFilterCategory').value;
            if (cat) params.set('category', cat);

            const status = q('imFilterStatus').value;
            if (status) params.set('status', status);

            const uom = q('imFilterUom').value;
            if (uom) params.set('uom', uom);

            const stock = q('imFilterStock').value;
            if (stock) params.set('stock', stock);

            const d = await fetchJson(`${API}/items?${params}`);
            renderItems(d.items, d.totalCount, d.page, d.size);
        } catch (e) {
            console.error('Load items error', e);
            q('imTbody').innerHTML = `<tr><td colspan="11" class="text-center text-danger py-3">${esc(e.message)}</td></tr>`;
        }
    }

    // ── Render Items Table ──────────────────────────────────────
    function renderItems(items, total, page, size) {
        const tb = q('imTbody');
        if (!items || items.length === 0) {
            tb.innerHTML = '<tr><td colspan="11" class="text-center text-muted py-4"><i class="bi bi-inbox me-2"></i>No items found</td></tr>';
            q('imPageInfo').textContent = 'Showing 0 items';
            q('imPager').innerHTML = '';
            return;
        }

        tb.innerHTML = items.map(i => {
            const gm = groupMeta[i.itemGroup] || groupMeta.OTHER;
            const stockPct = (i.reorderLevel && i.reorderLevel > 0) ? Math.min(100, ((i.currentStock || 0) / i.reorderLevel) * 100) : 100;
            const stockClass = stockPct <= 0 ? 'critical' : stockPct < 50 ? 'low' : 'ok';
            return `<tr>
                <td><span class="im-group-badge ${gm.css}"><i class="bi ${gm.icon} me-1"></i>${esc(i.itemGroup)}</span></td>
                <td class="fw-semibold">${esc(i.itemCode)}</td>
                <td>${esc(i.itemName)}</td>
                <td>${esc(i.itemCategory) || '<span class="text-muted">—</span>'}</td>
                <td>${esc(i.uom) || '—'}</td>
                <td class="text-end">${fmt(i.purchaseRate)}</td>
                <td class="text-end">
                    ${fmt(i.currentStock, 0)}
                    <div class="im-stock-bar"><div class="fill ${stockClass}" style="width:${stockPct}%"></div></div>
                </td>
                <td>${esc(i.hsnCode) || '—'}</td>
                <td>${i.gstRate != null ? fmt(i.gstRate, 1) + '%' : '—'}</td>
                <td><span class="im-status ${i.isActive ? 'active' : 'inactive'}">${i.isActive ? 'Active' : 'Inactive'}</span></td>
                <td class="text-center">
                    <div class="btn-group btn-group-sm">
                        <button class="btn btn-ghost-primary" title="View" onclick="ImApp.viewItem('${esc(i.itemGroup)}','${esc(i.itemCode)}')"><i class="bi bi-eye"></i></button>
                        <button class="btn btn-ghost-warning" title="Edit" onclick="ImApp.editItem('${esc(i.itemGroup)}','${esc(i.itemCode)}')"><i class="bi bi-pencil"></i></button>
                        <button class="btn btn-ghost-${i.isActive ? 'danger' : 'success'}" title="${i.isActive ? 'Deactivate' : 'Activate'}" onclick="ImApp.toggleItem('${esc(i.itemGroup)}','${esc(i.itemCode)}',${i.isActive})"><i class="bi bi-${i.isActive ? 'x-circle' : 'check-circle'}"></i></button>
                    </div>
                </td>
            </tr>`;
        }).join('');

        // Pagination
        const totalPages = Math.ceil(total / size);
        const start = (page - 1) * size + 1;
        const end = Math.min(page * size, total);
        q('imPageInfo').textContent = `Showing ${start}–${end} of ${total} items`;

        let pHtml = '';
        pHtml += `<li class="page-item ${page <= 1 ? 'disabled' : ''}"><a class="page-link" href="#" onclick="ImApp.goPage(${page - 1});return false;">&laquo;</a></li>`;
        const maxPages = 7;
        let startP = Math.max(1, page - 3);
        let endP = Math.min(totalPages, startP + maxPages - 1);
        if (endP - startP < maxPages - 1) startP = Math.max(1, endP - maxPages + 1);
        for (let p = startP; p <= endP; p++) {
            pHtml += `<li class="page-item ${p === page ? 'active' : ''}"><a class="page-link" href="#" onclick="ImApp.goPage(${p});return false;">${p}</a></li>`;
        }
        pHtml += `<li class="page-item ${page >= totalPages ? 'disabled' : ''}"><a class="page-link" href="#" onclick="ImApp.goPage(${page + 1});return false;">&raquo;</a></li>`;
        q('imPager').innerHTML = pHtml;
    }

    function goPage(p) { currentPage = p; loadItems(); }

    // ── View Item Detail ────────────────────────────────────────
    async function viewItem(group, code) {
        try {
            const d = await fetchJson(`${API}/items/${encodeURIComponent(group)}/${encodeURIComponent(code)}`);
            const det = d.detail;
            const gm = groupMeta[d.group] || groupMeta.OTHER;

            let html = `<div class="mb-3"><span class="im-group-badge ${gm.css}" style="font-size:.85rem;padding:.3rem .8rem;"><i class="bi ${gm.icon} me-1"></i>${esc(d.group)}</span></div>`;
            html += `<div class="im-detail-grid">`;
            for (const [key, val] of Object.entries(det)) {
                if (val === null || val === undefined) continue;
                const label = key.replace(/([A-Z])/g, ' $1').replace(/^./, c => c.toUpperCase()).trim();
                html += `<div class="im-detail-item"><label>${esc(label)}</label><div class="val">${esc(String(val))}</div></div>`;
            }
            html += `</div>`;

            // AI analysis card
            html += `<div class="im-ai-card mt-3"><div class="ai-title"><i class="bi bi-stars"></i> AI Item Analysis</div><div class="row g-2">`;
            const stock = det.currentStock ?? det.current_stock ?? 0;
            const reorder = det.reorderLevel ?? det.reorder_level ?? 0;
            const rate = det.ratePerUnit ?? det.costPerKg ?? det.costPerSheet ?? det.plateCost ?? det.rate_per_unit ?? 0;

            if (reorder > 0 && stock < reorder) {
                html += `<div class="col-md-6"><div class="p-2 rounded" style="background:rgba(239,68,68,.08);"><i class="bi bi-exclamation-triangle text-danger me-1"></i><strong>Low Stock Alert:</strong> Current stock (${stock}) is below reorder level (${reorder}).</div></div>`;
            } else if (stock > 0) {
                html += `<div class="col-md-6"><div class="p-2 rounded" style="background:rgba(34,197,94,.08);"><i class="bi bi-check-circle text-success me-1"></i><strong>Stock Healthy:</strong> Adequate stock levels.</div></div>`;
            }
            if (!rate || rate === 0) {
                html += `<div class="col-md-6"><div class="p-2 rounded" style="background:rgba(245,158,11,.08);"><i class="bi bi-currency-dollar text-warning me-1"></i><strong>Missing Rate:</strong> Purchase rate not set.</div></div>`;
            }
            html += `</div></div>`;

            q('viewItemBody').innerHTML = html;
            new bootstrap.Modal(q('modalViewItem')).show();
        } catch (e) {
            Swal.fire('Error', e.message, 'error');
        }
    }

    // ── Edit Item ───────────────────────────────────────────────
    async function editItem(group, code) {
        try {
            const d = await fetchJson(`${API}/items/${encodeURIComponent(group)}/${encodeURIComponent(code)}`);
            const det = d.detail;

            q('editMode').value = 'edit';
            q('editOrigGroup').value = d.group;
            q('editOrigCode').value = code;
            q('editItemTitle').textContent = 'Edit Item';
            q('editItemSubtitle').textContent = `Editing ${code} — ${d.group}`;

            // Common
            q('fItemGroup').value = d.group;
            q('fItemGroup').disabled = true;
            q('fItemCode').value = code;
            q('fItemCode').readOnly = true;

            // Lock group selector to current group
            document.querySelectorAll('#groupSelectorRow input[name="itemGroupRadio"]').forEach(r => {
                r.disabled = true;
                r.checked = (r.value === d.group);
            });
            document.querySelectorAll('#groupSelectorRow .im-group-option').forEach(el => {
                el.setAttribute('data-disabled', el.dataset.group !== d.group ? 'true' : 'false');
            });
            q('groupSelectorRow').style.display = '';

            // Map detail fields to common form fields based on group
            switch (d.group) {
                case 'CHEMICAL':
                    q('fItemName').value = det.chemicalName || '';
                    q('fItemCategory').value = det.chemicalCategory || '';
                    q('fPurchaseRate').value = det.ratePerUnit ?? '';
                    q('fChemicalType').value = det.chemicalType || '';
                    q('fProcessStage').value = det.processStage || '';
                    q('fChemManufacturer').value = det.manufacturer || '';
                    q('fChemBrand').value = det.brand || '';
                    q('fDilutionRatio').value = det.dilutionRatio || '';
                    q('fChemShelfLife').value = det.shelfLifeMonths ?? '';
                    q('fHazardous').value = det.hazardous != null ? String(det.hazardous) : '';
                    break;
                case 'INK':
                    q('fItemName').value = det.inkName || '';
                    q('fItemCategory').value = det.inkCategory || '';
                    q('fPurchaseRate').value = det.costPerKg ?? '';
                    q('fInkType').value = det.inkType || '';
                    q('fColorName').value = det.colorName || '';
                    q('fPantoneCode').value = det.pantoneCode || '';
                    q('fInkManufacturer').value = det.manufacturer || '';
                    q('fCoverage').value = det.coverageSqMPerKg ?? '';
                    q('fInkWastage').value = det.wastagePercent ?? '';
                    break;
                case 'PAPER':
                    q('fItemName').value = det.paperName || '';
                    q('fItemCategory').value = det.paperCategory || '';
                    q('fPurchaseRate').value = det.costPerKg ?? det.costPerSheet ?? '';
                    q('fPaperType').value = det.paperType || '';
                    q('fPaperFinish').value = det.paperFinish || '';
                    q('fGsm').value = det.gsm ?? '';
                    q('fGrainDir').value = det.grainDirection || '';
                    q('fSheetLength').value = det.sheetLengthMm ?? '';
                    q('fSheetWidth').value = det.sheetWidthMm ?? '';
                    q('fPaperSupplier').value = det.supplierName || '';
                    q('fPaperBrand').value = det.brandName || '';
                    break;
                case 'PLATE':
                    q('fItemName').value = det.plateName || '';
                    q('fItemCategory').value = det.plateType || '';
                    q('fPurchaseRate').value = det.plateCost ?? '';
                    q('fPlateType').value = det.plateType || '';
                    q('fThickness').value = det.thicknessMm ?? '';
                    q('fMaxImpressions').value = det.maxImpressions ?? '';
                    q('fPlateLength').value = det.plateLengthMm ?? '';
                    q('fPlateWidth').value = det.plateWidthMm ?? '';
                    q('fProcessingCost').value = det.processingCost ?? '';
                    break;
                case 'OTHER':
                    q('fItemName').value = det.itemName || '';
                    q('fItemCategory').value = det.itemCategory || '';
                    q('fPurchaseRate').value = det.ratePerUnit ?? '';
                    q('fOtherItemType').value = det.itemType || '';
                    q('fOtherDesc').value = det.description || '';
                    q('fOtherSupplier').value = det.supplierName || '';
                    q('fOtherBrand').value = det.brand || '';
                    break;
            }

            // Common fields
            q('fUom').value = det.uom || '';
            q('fReorderLevel').value = det.reorderLevel ?? '';
            q('fCurrentStock').value = det.currentStock ?? '';
            q('fHsnCode').value = det.hsnCode || '';
            q('fGstRate').value = det.gstRate ?? '';
            q('fRemarks').value = det.remarks || '';

            updateModalGroupTheme(d.group);
            toggleGroupFields();
            new bootstrap.Modal(q('modalEditItem')).show();
        } catch (e) {
            Swal.fire('Error', e.message, 'error');
        }
    }

    // ── Show Create Modal ───────────────────────────────────────
    function showCreateModal() {
        q('editMode').value = 'create';
        q('editOrigGroup').value = '';
        q('editOrigCode').value = '';
        q('editItemTitle').textContent = 'New Item';
        q('editItemSubtitle').textContent = 'Fill in the details to create a new item';

        // Reset form
        q('fItemGroup').value = '';
        q('fItemGroup').disabled = false;
        q('fItemCode').value = '';
        q('fItemCode').readOnly = false;

        // Enable group selector
        document.querySelectorAll('#groupSelectorRow .im-group-option').forEach(el => el.removeAttribute('data-disabled'));
        document.querySelectorAll('#groupSelectorRow input[name="itemGroupRadio"]').forEach(r => { r.checked = false; r.disabled = false; });
        q('groupSelectorRow').style.display = '';

        const fields = ['fItemName', 'fItemCategory', 'fUom', 'fPurchaseRate', 'fReorderLevel',
            'fCurrentStock', 'fHsnCode', 'fGstRate',
            'fChemicalType', 'fProcessStage', 'fChemManufacturer', 'fChemBrand', 'fDilutionRatio', 'fChemShelfLife',
            'fInkType', 'fColorName', 'fPantoneCode', 'fInkManufacturer', 'fCoverage', 'fInkWastage',
            'fPaperType', 'fPaperFinish', 'fGsm', 'fGrainDir', 'fSheetLength', 'fSheetWidth', 'fPaperSupplier', 'fPaperBrand',
            'fPlateType', 'fThickness', 'fMaxImpressions', 'fPlateLength', 'fPlateWidth', 'fProcessingCost',
            'fOtherItemType', 'fOtherDesc', 'fOtherSupplier', 'fOtherBrand'];
        fields.forEach(id => { const el = q(id); if (el) el.value = ''; });
        q('fRemarks').value = '';
        q('fHazardous').value = '';

        updateModalGroupTheme('');
        toggleGroupFields();
        new bootstrap.Modal(q('modalEditItem')).show();
    }

    // ── Update modal theme based on selected group ────────────
    function updateModalGroupTheme(grp) {
        const statusEl = q('editModalStatus');
        const iconEl = q('editModalIcon');
        const grpLower = (grp || '').toLowerCase();

        // Status bar
        statusEl.className = 'modal-status';
        if (grpLower) {
            statusEl.classList.add('status-' + grpLower);
        } else {
            statusEl.classList.add('status-create');
        }

        // Icon
        iconEl.className = 'im-modal-icon';
        const iconMap = { chemical: 'bi-droplet-half', ink: 'bi-palette', paper: 'bi-file-earmark', plate: 'bi-disc', other: 'bi-three-dots' };
        if (grpLower && iconMap[grpLower]) {
            iconEl.classList.add('icon-' + grpLower);
            iconEl.innerHTML = '<i class="bi ' + iconMap[grpLower] + '"></i>';
        } else {
            iconEl.innerHTML = '<i class="bi bi-plus-circle"></i>';
        }
    }

    // ── Toggle group-specific fields ────────────────────────────
    function toggleGroupFields() {
        const grp = q('fItemGroup').value;
        document.querySelectorAll('.im-group-fields').forEach(el => el.classList.remove('show'));
        const map = { CHEMICAL: 'fieldsChemical', INK: 'fieldsInk', PAPER: 'fieldsPaper', PLATE: 'fieldsPlate', OTHER: 'fieldsOther' };
        if (map[grp]) q(map[grp]).classList.add('show');
    }

    // ── Save Item ───────────────────────────────────────────────
    async function saveItem() {
        const mode = q('editMode').value;
        const group = q('fItemGroup').disabled ? q('editOrigGroup').value : q('fItemGroup').value;
        const code = q('fItemCode').value.trim();
        const name = q('fItemName').value.trim();

        if (!group || !code || !name) {
            Swal.fire('Validation', 'Group, Code and Name are required.', 'warning');
            return;
        }

        const dto = {
            itemGroup: group,
            itemCode: code,
            itemName: name,
            itemCategory: q('fItemCategory').value || null,
            uom: q('fUom').value || null,
            purchaseRate: q('fPurchaseRate').value ? parseFloat(q('fPurchaseRate').value) : null,
            reorderLevel: q('fReorderLevel').value ? parseFloat(q('fReorderLevel').value) : null,
            currentStock: q('fCurrentStock').value ? parseFloat(q('fCurrentStock').value) : null,
            hsnCode: q('fHsnCode').value || null,
            gstRate: q('fGstRate').value ? parseFloat(q('fGstRate').value) : null,
            remarks: q('fRemarks').value || null
        };

        // Group-specific
        switch (group) {
            case 'CHEMICAL':
                dto.chemicalType = q('fChemicalType').value || null;
                dto.processStage = q('fProcessStage').value || null;
                dto.chemManufacturer = q('fChemManufacturer').value || null;
                dto.chemBrand = q('fChemBrand').value || null;
                dto.dilutionRatio = q('fDilutionRatio').value || null;
                dto.chemShelfLife = q('fChemShelfLife').value ? parseInt(q('fChemShelfLife').value) : null;
                dto.hazardous = q('fHazardous').value ? q('fHazardous').value === 'true' : null;
                break;
            case 'INK':
                dto.inkType = q('fInkType').value || null;
                dto.colorName = q('fColorName').value || null;
                dto.pantoneCode = q('fPantoneCode').value || null;
                dto.inkManufacturer = q('fInkManufacturer').value || null;
                dto.coverage = q('fCoverage').value ? parseFloat(q('fCoverage').value) : null;
                dto.inkWastage = q('fInkWastage').value ? parseFloat(q('fInkWastage').value) : null;
                break;
            case 'PAPER':
                dto.paperType = q('fPaperType').value || null;
                dto.paperFinish = q('fPaperFinish').value || null;
                dto.gsm = q('fGsm').value ? parseInt(q('fGsm').value) : null;
                dto.grainDir = q('fGrainDir').value || null;
                dto.sheetLength = q('fSheetLength').value ? parseInt(q('fSheetLength').value) : null;
                dto.sheetWidth = q('fSheetWidth').value ? parseInt(q('fSheetWidth').value) : null;
                dto.paperSupplier = q('fPaperSupplier').value || null;
                dto.paperBrand = q('fPaperBrand').value || null;
                break;
            case 'PLATE':
                dto.plateType = q('fPlateType').value || null;
                dto.thickness = q('fThickness').value ? parseFloat(q('fThickness').value) : null;
                dto.maxImpressions = q('fMaxImpressions').value ? parseInt(q('fMaxImpressions').value) : null;
                dto.plateLength = q('fPlateLength').value ? parseInt(q('fPlateLength').value) : null;
                dto.plateWidth = q('fPlateWidth').value ? parseInt(q('fPlateWidth').value) : null;
                dto.processingCost = q('fProcessingCost').value ? parseFloat(q('fProcessingCost').value) : null;
                break;
            case 'OTHER':
                dto.otherItemType = q('fOtherItemType').value || null;
                dto.otherDesc = q('fOtherDesc').value || null;
                dto.otherSupplier = q('fOtherSupplier').value || null;
                dto.otherBrand = q('fOtherBrand').value || null;
                break;
        }

        try {
            if (mode === 'edit') {
                const origGroup = q('editOrigGroup').value;
                const origCode = q('editOrigCode').value;
                await putJson(`${API}/items/${encodeURIComponent(origGroup)}/${encodeURIComponent(origCode)}`, dto);
                Swal.fire({ icon: 'success', title: 'Updated', text: 'Item updated successfully.', timer: 1500, showConfirmButton: false });
            } else {
                await postJson(`${API}/items`, dto);
                Swal.fire({ icon: 'success', title: 'Created', text: 'Item created successfully.', timer: 1500, showConfirmButton: false });
            }
            bootstrap.Modal.getInstance(q('modalEditItem'))?.hide();
            loadKpis();
            loadItems();
        } catch (e) {
            Swal.fire('Error', e.message, 'error');
        }
    }

    // ── Toggle Active ───────────────────────────────────────────
    async function toggleItem(group, code, isActive) {
        const action = isActive ? 'deactivate' : 'activate';
        const result = await Swal.fire({
            title: `${action.charAt(0).toUpperCase() + action.slice(1)} Item?`,
            text: `Are you sure you want to ${action} item ${code}?`,
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: `Yes, ${action}`
        });
        if (!result.isConfirmed) return;

        try {
            await postJson(`${API}/items/${encodeURIComponent(group)}/${encodeURIComponent(code)}/toggle`, {});
            Swal.fire({ icon: 'success', title: 'Done', text: `Item ${action}d.`, timer: 1200, showConfirmButton: false });
            loadKpis();
            loadItems();
        } catch (e) {
            Swal.fire('Error', e.message, 'error');
        }
    }

    // ── Public API ──────────────────────────────────────────────
    return {
        init,
        filterGroup,
        debounceSearch,
        loadItems,
        viewItem,
        editItem,
        showCreateModal,
        toggleGroupFields,
        saveItem,
        toggleItem,
        goPage
    };
})();

document.addEventListener('DOMContentLoaded', ImApp.init);
