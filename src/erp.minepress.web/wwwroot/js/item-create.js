/* ═══════════════════════════════════════════════════════════════
   ITEM CREATE — Wizard Client Module
   MinePress ERP — AI-Powered Item Creation
   ═══════════════════════════════════════════════════════════════ */

const IcApp = (() => {
    const API = '/api/itemmanagement';
    let _step = 1;
    const TOTAL = 4;
    let _selectedGroup = null;

    // ── Code prefix rules ──
    const CODE_PREFIXES = {
        CHEMICAL: 'CH_',
        INK:      'INK_',
        PLATE:    'PLT_',
        PAPER:    'PAP_',
        OTHER:    'OTH_'
    };
    const ALL_PREFIXES = Object.values(CODE_PREFIXES);

    // ── Helpers ──
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
    function esc(s) { const d = document.createElement('div'); d.textContent = s ?? ''; return d.innerHTML; }
    function $(id) { return document.getElementById(id); }
    function val(id) { return $(id)?.value?.trim() ?? ''; }

    // ══════════════════════════════════════
    // CODE PREFIX HELPERS
    // ══════════════════════════════════════
    function getRequiredPrefix() {
        return _selectedGroup ? (CODE_PREFIXES[_selectedGroup] || '') : '';
    }

    function updateCodeHint() {
        const hint = $('icCodeHint');
        const badge = $('icCodePrefixBadge');
        const rules = $('icCodePrefixRules');
        const input = $('icItemCode');
        const prefix = getRequiredPrefix();

        if (!prefix) {
            if (hint) hint.innerHTML = '<i class="bi bi-info-circle me-1"></i>Select an item group to see the required prefix.';
            if (badge) badge.innerHTML = '<i class="bi bi-upc-scan"></i>';
            if (rules) rules.classList.add('d-none');
            if (input) input.placeholder = 'Select a group first';
            return;
        }

        if (rules) rules.classList.remove('d-none');
        if (badge) badge.innerHTML = `<span class="ic-prefix-tag ${_selectedGroup.toLowerCase()}">${prefix}</span>`;
        if (input) input.placeholder = `${prefix}001`;

        const code = input ? input.value.toUpperCase() : '';
        const isValid = code.startsWith(prefix) && code.length > prefix.length;
        const isEmpty = !code || code === prefix;

        if (isEmpty) {
            if (hint) hint.innerHTML = `<i class="bi bi-info-circle me-1 text-primary"></i>Code must start with <strong>${prefix}</strong> — e.g. <code>${prefix}001</code>`;
        } else if (isValid) {
            if (hint) hint.innerHTML = `<i class="bi bi-check-circle-fill me-1 text-success"></i>Valid code — starts with required prefix <strong>${prefix}</strong>`;
        } else {
            if (hint) hint.innerHTML = `<i class="bi bi-exclamation-triangle-fill me-1 text-danger"></i>Code must start with <strong>${prefix}</strong> for <strong>${_selectedGroup}</strong> items`;
        }
    }

    function applyCodePrefix() {
        const prefix = getRequiredPrefix();
        const input = $('icItemCode');
        if (!input || !prefix) { updateCodeHint(); return; }

        let current = input.value.toUpperCase();

        // Strip any other known prefix from the current value
        let suffix = current;
        for (const p of ALL_PREFIXES) {
            if (current.startsWith(p)) {
                suffix = current.slice(p.length);
                break;
            }
        }

        input.value = prefix + suffix;
        updateCodeHint();
    }

    function enforceCodePrefix(e) {
        const prefix = getRequiredPrefix();
        if (!prefix) {
            // Still uppercase
            const input = e.target;
            const pos = input.selectionStart;
            input.value = input.value.toUpperCase();
            input.setSelectionRange(pos, pos);
            return;
        }

        const input = e.target;
        const pos = input.selectionStart;
        let val = input.value.toUpperCase();

        if (!val.startsWith(prefix)) {
            // If user deleted into the prefix, restore it
            let suffix = val;
            for (const p of ALL_PREFIXES) {
                if (val.startsWith(p)) { suffix = val.slice(p.length); break; }
            }
            // Strip any partial prefix characters at the start
            for (const p of ALL_PREFIXES) {
                for (let len = p.length - 1; len >= 1; len--) {
                    if (val.startsWith(p.slice(0, len)) && !val.startsWith(p)) {
                        suffix = val.slice(len);
                        break;
                    }
                }
            }
            val = prefix + suffix;
            input.value = val;
            // Place cursor after prefix if it fell inside it
            const newPos = Math.max(pos, prefix.length);
            input.setSelectionRange(newPos, newPos);
        } else {
            input.value = val;
            input.setSelectionRange(pos, pos);
        }

        updateCodeHint();
    }

    // ══════════════════════════════════════
    // INIT
    // ══════════════════════════════════════
    function init() {
        // Group card selection
        document.querySelectorAll('.ic-group-card input[name="itemGroupRadio"]').forEach(radio => {
            radio.addEventListener('change', () => {
                _selectedGroup = radio.value;
                updateGroupAiHints();
                applyCodePrefix();
            });
        });

        // Item code — enforce prefix & uppercase on every input
        const codeInput = $('icItemCode');
        if (codeInput) {
            codeInput.addEventListener('input', enforceCodePrefix);
            codeInput.addEventListener('keydown', e => {
                // Prevent deleting into prefix area
                const prefix = getRequiredPrefix();
                if (!prefix) return;
                const start = e.target.selectionStart;
                const end = e.target.selectionEnd;
                if ((e.key === 'Backspace' && start <= prefix.length && start === end) ||
                    (e.key === 'Delete' && start < prefix.length && start === end)) {
                    e.preventDefault();
                }
            });
        }

        loadFilters();
        updateNav();
    }

    // ══════════════════════════════════════
    // FILTERS — Category & UOM Select2
    // ══════════════════════════════════════
    async function loadFilters() {
        const jq = window.jQuery;
        if (!jq) return;

        // Initialize Select2 on both selects immediately (with no options yet)
        const s2opts = (placeholder) => ({
            theme: 'bootstrap-5',
            width: '100%',
            tags: true,
            allowClear: true,
            placeholder: placeholder,
            createTag: function (params) {
                const term = (params.term || '').trim();
                if (!term) return null;
                return { id: term, text: term, newTag: true };
            }
        });

        jq('#icItemCategory').select2(s2opts('Search or enter a category…'));
        jq('#icUom').select2(s2opts('Search or enter a unit (Kg, Ltr, Nos…)'));

        try {
            const data = await fetchJson(`${API}/filters`);

            // Populate Category
            const catSel = jq('#icItemCategory');
            catSel.find('option:not(:first)').remove();
            (data.categories || []).forEach(c => {
                catSel.append(new Option(c, c, false, false));
            });
            catSel.trigger('change');

            // Populate UOM
            const uomSel = jq('#icUom');
            uomSel.find('option:not(:first)').remove();
            (data.uoms || []).forEach(u => {
                uomSel.append(new Option(u, u, false, false));
            });
            uomSel.trigger('change');
        } catch {
            // Non-fatal — user can still type new values via tags
        }
    }

    // ══════════════════════════════════════
    // STEP NAVIGATION
    // ══════════════════════════════════════
    function goStep(n) {
        if (n < 1 || n > TOTAL) return;
        if (n > _step) {
            for (let s = _step; s < n; s++) {
                if (!validateStep(s)) return;
            }
        }
        _step = n;
        renderStep();
    }

    function nextStep() {
        if (_step >= TOTAL) return;
        if (!validateStep(_step)) return;
        _step++;
        if (_step === 3) showGroupFields();
        if (_step === 4) { buildReview(); runAiChecks(); }
        renderStep();
    }

    function prevStep() {
        if (_step <= 1) return;
        _step--;
        if (_step === 3) showGroupFields();
        renderStep();
    }

    function renderStep() {
        // Panels
        document.querySelectorAll('.ic-step-panel').forEach(p => p.classList.remove('active'));
        const panel = $(`icStep${_step}`);
        if (panel) panel.classList.add('active');

        // Step indicators
        document.querySelectorAll('.ic-step').forEach(s => {
            const sn = parseInt(s.dataset.step);
            s.classList.remove('active', 'completed');
            if (sn === _step) s.classList.add('active');
            else if (sn < _step) s.classList.add('completed');
        });

        // Lines
        document.querySelectorAll('.ic-step-line').forEach((line, i) => {
            if (i + 1 < _step) line.classList.add('completed');
            else line.classList.remove('completed');
        });

        updateNav();
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }

    function updateNav() {
        const prev = $('icBtnPrev');
        const next = $('icBtnNext');
        const create = $('icBtnCreate');
        if (prev) prev.style.display = _step > 1 ? '' : 'none';
        if (next) next.style.display = _step < TOTAL ? '' : 'none';
        if (create) create.style.display = _step === TOTAL ? '' : 'none';
    }

    // ══════════════════════════════════════
    // VALIDATION
    // ══════════════════════════════════════
    function validateStep(step) {
        if (step === 1) {
            if (!_selectedGroup) {
                Swal.fire({ icon: 'warning', title: 'Select Group', text: 'Please select an item group to proceed.' });
                return false;
            }
        }
        if (step === 2) {
            const required = [
                { id: 'icItemCode', label: 'Item Code' },
                { id: 'icItemName', label: 'Item Name' }
            ];
            for (const f of required) {
                if (!val(f.id)) {
                    Swal.fire({ icon: 'warning', title: 'Required', text: `${f.label} is required.` });
                    $(f.id)?.focus();
                    return false;
                }
            }

            // Enforce prefix rule
            const prefix = getRequiredPrefix();
            const code = val('icItemCode').toUpperCase();
            if (prefix && !code.startsWith(prefix)) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Invalid Item Code',
                    html: `Item code for <strong>${_selectedGroup}</strong> must start with <strong>${prefix}</strong>.<br>
                           <span class="text-muted" style="font-size:.9rem;">Example: <code>${prefix}001</code></span>`
                });
                $('icItemCode')?.focus();
                return false;
            }
            if (prefix && code === prefix) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Incomplete Item Code',
                    html: `Please enter a code after the prefix <strong>${prefix}</strong>.<br>
                           <span class="text-muted" style="font-size:.9rem;">Example: <code>${prefix}001</code></span>`
                });
                $('icItemCode')?.focus();
                return false;
            }
        }
        return true;
    }

    // ══════════════════════════════════════
    // GROUP-SPECIFIC FIELDS
    // ══════════════════════════════════════
    function showGroupFields() {
        // Hide all
        document.querySelectorAll('.ic-group-fields').forEach(f => f.classList.add('d-none'));
        // Show matching
        const id = `fields${_selectedGroup.charAt(0) + _selectedGroup.slice(1).toLowerCase()}`;
        const el = $(id);
        if (el) el.classList.remove('d-none');
        else $('fieldsNone')?.classList.remove('d-none');
    }

    // ══════════════════════════════════════
    // AI HINTS
    // ══════════════════════════════════════
    function updateGroupAiHints() {
        const hint = $('icAiHint');
        const badges = $('icAiBadges');
        if (!hint) return;

        const tips = {
            'CHEMICAL': { hint: 'Chemical selected — safety & shelf-life fields will appear in Step 3.', badges: ['<span class="badge bg-pink-lt text-pink">Hazmat tracking</span>', '<span class="badge bg-cyan-lt text-cyan">Shelf-life</span>'] },
            'INK': { hint: 'Ink selected — Pantone matching & coverage calculations available.', badges: ['<span class="badge bg-purple-lt text-purple">Pantone</span>', '<span class="badge bg-azure-lt text-azure">Coverage calc</span>'] },
            'PAPER': { hint: 'Paper selected — GSM, grain direction & sheet dimensions will be tracked.', badges: ['<span class="badge bg-blue-lt text-blue">GSM tracking</span>', '<span class="badge bg-teal-lt text-teal">Dimensions</span>'] },
            'PLATE': { hint: 'Plate selected — impression capacity & processing cost fields available.', badges: ['<span class="badge bg-yellow-lt text-yellow">Impressions</span>', '<span class="badge bg-orange-lt text-orange">Processing</span>'] },
            'OTHER': { hint: 'General item selected — basic specification fields will be shown.', badges: ['<span class="badge bg-secondary-lt text-secondary">General</span>'] }
        };

        const info = tips[_selectedGroup] || { hint: 'Select a group to continue.', badges: [] };
        hint.textContent = info.hint;
        if (badges) badges.innerHTML = info.badges.join('');
    }

    // ══════════════════════════════════════
    // REVIEW (Step 4)
    // ══════════════════════════════════════
    function buildReview() {
        const grid = $('icReviewGrid');
        const badge = $('icReviewGroupBadge');
        if (!grid) return;

        // Group badge
        const groupIcons = {
            CHEMICAL: 'bi-droplet-half', INK: 'bi-palette', PAPER: 'bi-file-earmark',
            PLATE: 'bi-disc', OTHER: 'bi-three-dots'
        };
        if (badge) {
            badge.innerHTML = `<i class="bi ${groupIcons[_selectedGroup] || 'bi-box'}"></i> ${_selectedGroup}`;
        }

        // Collect common fields
        const items = [
            { label: 'Item Code', value: val('icItemCode') },
            { label: 'Item Name', value: val('icItemName') },
            { label: 'Category', value: val('icItemCategory') || '—' },
            { label: 'UOM', value: val('icUom') || '—' },
            { label: 'Purchase Rate', value: val('icPurchaseRate') ? '₹' + val('icPurchaseRate') : '—' },
            { label: 'Reorder Level', value: val('icReorderLevel') || '—' },
            { label: 'Current Stock', value: val('icCurrentStock') || '—' },
            { label: 'HSN Code', value: val('icHsnCode') || '—' },
            { label: 'GST Rate', value: val('icGstRate') ? val('icGstRate') + '%' : '—' }
        ];

        // Group-specific fields
        if (_selectedGroup === 'CHEMICAL') {
            items.push({ label: 'Chemical Type', value: val('icChemicalType') || '—' });
            items.push({ label: 'Process Stage', value: val('icProcessStage') || '—' });
            items.push({ label: 'Manufacturer', value: val('icChemManufacturer') || '—' });
            items.push({ label: 'Brand', value: val('icChemBrand') || '—' });
            items.push({ label: 'Dilution Ratio', value: val('icDilutionRatio') || '—' });
            items.push({ label: 'Shelf Life', value: val('icChemShelfLife') ? val('icChemShelfLife') + ' months' : '—' });
            const haz = document.querySelector('input[name="hazardous"]:checked');
            items.push({ label: 'Hazardous', value: haz?.value === 'true' ? '⚠ Yes' : 'No' });
        } else if (_selectedGroup === 'INK') {
            items.push({ label: 'Ink Type', value: val('icInkType') || '—' });
            items.push({ label: 'Color Name', value: val('icColorName') || '—' });
            items.push({ label: 'Pantone Code', value: val('icPantoneCode') || '—' });
            items.push({ label: 'Manufacturer', value: val('icInkManufacturer') || '—' });
            items.push({ label: 'Coverage', value: val('icCoverage') ? val('icCoverage') + ' sq.m/Kg' : '—' });
            items.push({ label: 'Wastage', value: val('icInkWastage') ? val('icInkWastage') + '%' : '—' });
        } else if (_selectedGroup === 'PAPER') {
            items.push({ label: 'Paper Type', value: val('icPaperType') || '—' });
            items.push({ label: 'GSM', value: val('icGsm') ? val('icGsm') + ' g/m²' : '—' });
            items.push({ label: 'Finish', value: val('icPaperFinish') || '—' });
            items.push({ label: 'Grain Direction', value: val('icGrainDir') || '—' });
            items.push({ label: 'Sheet Size', value: (val('icSheetLength') && val('icSheetWidth')) ? val('icSheetLength') + ' × ' + val('icSheetWidth') + ' mm' : '—' });
            items.push({ label: 'Supplier', value: val('icPaperSupplier') || '—' });
            items.push({ label: 'Brand', value: val('icPaperBrand') || '—' });
        } else if (_selectedGroup === 'PLATE') {
            items.push({ label: 'Plate Type', value: val('icPlateType') || '—' });
            items.push({ label: 'Thickness', value: val('icThickness') ? val('icThickness') + ' mm' : '—' });
            items.push({ label: 'Max Impressions', value: val('icMaxImpressions') || '—' });
            items.push({ label: 'Plate Size', value: (val('icPlateLength') && val('icPlateWidth')) ? val('icPlateLength') + ' × ' + val('icPlateWidth') + ' mm' : '—' });
            items.push({ label: 'Processing Cost', value: val('icProcessingCost') ? '₹' + val('icProcessingCost') : '—' });
        } else if (_selectedGroup === 'OTHER') {
            items.push({ label: 'Item Type', value: val('icOtherItemType') || '—' });
            items.push({ label: 'Description', value: val('icOtherDesc') || '—' });
            items.push({ label: 'Supplier', value: val('icOtherSupplier') || '—' });
            items.push({ label: 'Brand', value: val('icOtherBrand') || '—' });
        }

        grid.innerHTML = items.map(i => `
            <div class="ic-review-item">
                <div class="review-label">${esc(i.label)}</div>
                <div class="review-value ${i.value === '—' ? 'empty' : ''}">${esc(i.value)}</div>
            </div>
        `).join('');
    }

    function runAiChecks() {
        const checks = $('icAiChecks');
        if (!checks) return;

        const results = [];
        const code = val('icItemCode');
        const name = val('icItemName');
        const rate = parseFloat(val('icPurchaseRate')) || 0;
        const reorder = parseFloat(val('icReorderLevel')) || 0;
        const stock = parseFloat(val('icCurrentStock')) || 0;
        const hsn = val('icHsnCode');
        const gst = val('icGstRate');

        // Code naming convention — prefix validation
        const prefix = getRequiredPrefix();
        if (prefix && code.startsWith(prefix) && code.length > prefix.length) {
            results.push({ cls: 'pass', icon: 'bi-check-circle-fill', text: `Code "${code}" correctly uses required prefix <strong>${prefix}</strong> for ${_selectedGroup}` });
        } else if (prefix) {
            results.push({ cls: 'warn', icon: 'bi-exclamation-triangle-fill', text: `Code must start with <strong>${prefix}</strong> for ${_selectedGroup} items (e.g. ${prefix}001)` });
        } else if (code) {
            results.push({ cls: 'pass', icon: 'bi-check-circle-fill', text: `Code "${code}" provided` });
        }

        // Rate check
        if (rate > 0) {
            results.push({ cls: 'pass', icon: 'bi-check-circle-fill', text: `Purchase rate ₹${rate} configured` });
        } else {
            results.push({ cls: 'warn', icon: 'bi-exclamation-triangle-fill', text: 'No purchase rate — costing reports will be incomplete' });
        }

        // HSN check
        if (hsn) {
            results.push({ cls: 'pass', icon: 'bi-check-circle-fill', text: `HSN code ${hsn} provided for GST compliance` });
        } else {
            results.push({ cls: 'warn', icon: 'bi-exclamation-triangle-fill', text: 'No HSN code — required for GST invoicing' });
        }

        // GST check
        if (gst) {
            results.push({ cls: 'pass', icon: 'bi-check-circle-fill', text: `GST rate ${gst}% configured` });
        } else {
            results.push({ cls: 'warn', icon: 'bi-exclamation-triangle-fill', text: 'No GST rate — will default to 0% on invoices' });
        }

        // Stock check
        if (reorder > 0) {
            results.push({ cls: 'pass', icon: 'bi-check-circle-fill', text: `Reorder level set at ${reorder} — low stock alerts active` });
        } else {
            results.push({ cls: 'info', icon: 'bi-info-circle-fill', text: 'No reorder level — low stock alerts will be disabled' });
        }

        if (stock > 0 && reorder > 0 && stock < reorder) {
            results.push({ cls: 'warn', icon: 'bi-exclamation-diamond-fill', text: `Opening stock (${stock}) is below reorder level (${reorder})` });
        }

        // Chemical-specific
        if (_selectedGroup === 'CHEMICAL') {
            const haz = document.querySelector('input[name="hazardous"]:checked');
            if (haz?.value === 'true') {
                results.push({ cls: 'warn', icon: 'bi-shield-exclamation', text: 'Hazardous material — MSDS documentation recommended' });
            }
            if (!val('icChemShelfLife')) {
                results.push({ cls: 'info', icon: 'bi-clock', text: 'No shelf life set — consider adding for expiry tracking' });
            }
        }

        // Store notification
        results.push({ cls: 'info', icon: 'bi-envelope-fill', text: 'Store department will be notified about this new item' });
        results.push({ cls: 'pass', icon: 'bi-journal-check', text: 'Item creation will be logged in the activity trail' });

        checks.innerHTML = results.map(r => `
            <div class="ic-ai-check ${r.cls}">
                <i class="bi ${r.icon}"></i>
                <span>${r.text}</span>
            </div>
        `).join('');
    }

    // ══════════════════════════════════════
    // CREATE ITEM
    // ══════════════════════════════════════
    async function createItem() {
        // Final validation
        for (let s = 1; s <= 3; s++) {
            if (!validateStep(s)) {
                goStep(s);
                return;
            }
        }

        const payload = {
            itemGroup: _selectedGroup,
            itemCode: val('icItemCode'),
            itemName: val('icItemName'),
            itemCategory: val('icItemCategory') || null,
            uom: val('icUom') || null,
            purchaseRate: parseFloat(val('icPurchaseRate')) || null,
            reorderLevel: parseFloat(val('icReorderLevel')) || null,
            currentStock: parseFloat(val('icCurrentStock')) || null,
            hsnCode: val('icHsnCode') || null,
            gstRate: parseFloat(val('icGstRate')) || null,
            remarks: val('icRemarks') || null
        };

        // Group-specific
        if (_selectedGroup === 'CHEMICAL') {
            payload.chemicalType = val('icChemicalType') || null;
            payload.processStage = val('icProcessStage') || null;
            payload.chemManufacturer = val('icChemManufacturer') || null;
            payload.chemBrand = val('icChemBrand') || null;
            payload.dilutionRatio = val('icDilutionRatio') || null;
            payload.chemShelfLife = parseInt(val('icChemShelfLife')) || null;
            const haz = document.querySelector('input[name="hazardous"]:checked');
            payload.hazardous = haz?.value === 'true' ? true : (haz?.value === 'false' ? false : null);
        } else if (_selectedGroup === 'INK') {
            payload.inkType = val('icInkType') || null;
            payload.colorName = val('icColorName') || null;
            payload.pantoneCode = val('icPantoneCode') || null;
            payload.inkManufacturer = val('icInkManufacturer') || null;
            payload.coverage = parseFloat(val('icCoverage')) || null;
            payload.inkWastage = parseFloat(val('icInkWastage')) || null;
        } else if (_selectedGroup === 'PAPER') {
            payload.paperType = val('icPaperType') || null;
            payload.gsm = parseInt(val('icGsm')) || null;
            payload.paperFinish = val('icPaperFinish') || null;
            payload.grainDir = val('icGrainDir') || null;
            payload.sheetLength = parseInt(val('icSheetLength')) || null;
            payload.sheetWidth = parseInt(val('icSheetWidth')) || null;
            payload.paperSupplier = val('icPaperSupplier') || null;
            payload.paperBrand = val('icPaperBrand') || null;
        } else if (_selectedGroup === 'PLATE') {
            payload.plateType = val('icPlateType') || null;
            payload.thickness = parseFloat(val('icThickness')) || null;
            payload.maxImpressions = parseInt(val('icMaxImpressions')) || null;
            payload.plateLength = parseInt(val('icPlateLength')) || null;
            payload.plateWidth = parseInt(val('icPlateWidth')) || null;
            payload.processingCost = parseFloat(val('icProcessingCost')) || null;
        } else if (_selectedGroup === 'OTHER') {
            payload.otherItemType = val('icOtherItemType') || null;
            payload.otherDesc = val('icOtherDesc') || null;
            payload.otherSupplier = val('icOtherSupplier') || null;
            payload.otherBrand = val('icOtherBrand') || null;
        }

        const btn = $('icBtnCreate');
        if (btn) { btn.disabled = true; btn.innerHTML = '<i class="bi bi-hourglass-split me-1"></i>Creating…'; }

        try {
            const res = await postJson(`${API}/items`, payload);

            await Swal.fire({
                icon: 'success',
                title: 'Item Created Successfully!',
                html: `
                    <div class="text-start" style="font-size:.9rem;">
                        <div class="mb-2"><strong>${esc(payload.itemName)}</strong> has been added to inventory.</div>
                        <div class="p-3 rounded" style="background:#f1f5f9;">
                            <div class="mb-1"><i class="bi bi-box-seam me-1"></i><strong>Group:</strong> ${esc(_selectedGroup)}</div>
                            <div class="mb-1"><i class="bi bi-hash me-1"></i><strong>Code:</strong> ${esc(payload.itemCode)}</div>
                            ${payload.hsnCode ? `<div class="mb-1"><i class="bi bi-receipt me-1"></i><strong>HSN:</strong> ${esc(payload.hsnCode)}</div>` : ''}
                            <div class="mb-1"><i class="bi bi-envelope-check me-1 text-success"></i>Store department notified</div>
                            <div><i class="bi bi-journal-check me-1 text-info"></i>Activity logged</div>
                        </div>
                    </div>
                `,
                confirmButtonText: 'Go to Item Management',
                allowOutsideClick: false
            });

            window.location.href = '/Maintenance/ItemManagement';
        } catch (e) {
            Swal.fire({ icon: 'error', title: 'Error', text: e.message });
        } finally {
            if (btn) { btn.disabled = false; btn.innerHTML = '<i class="bi bi-box-seam me-1"></i>Create Item & Notify Store'; }
        }
    }

    // ── Public API ──
    return {
        init,
        goStep,
        nextStep,
        prevStep,
        createItem
    };
})();

document.addEventListener('DOMContentLoaded', () => IcApp.init());
