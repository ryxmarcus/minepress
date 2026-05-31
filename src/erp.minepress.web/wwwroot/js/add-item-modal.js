// ===== MinePress Shared: Add Item Modal =====
// Shared across Enquiry, Quotation, Job Create pages.
//
// Usage:
//   AddItemModal.init({
//     title,           // e.g. "Add Item to Job"
//     avatarClass,     // e.g. "bg-purple-lt text-purple"
//     confirmStyle,    // e.g. "background:#6f42c1;color:#fff;"
//     gstTypeGetter,   // function() → 'INTRA' | 'INTER'
//     onPrepare,       // function() → { grossAmount, qty, productName, description } | false
//     onConfirm        // function({ name, qty, description, grossAmount, discPct, hsnItem })
//   })

const AddItemModal = {
    _opts: {},

    init(opts) {
        const self = this;
        this._opts = opts || {};

        const el = document.getElementById('aimModal');
        if (!el) return;

        // Apply theming
        if (opts.title) $('#aimModalTitle').text(opts.title);
        if (opts.avatarClass) $('#aimModalAvatar').attr('class', 'avatar avatar-sm ' + opts.avatarClass);
        if (opts.confirmStyle) $('#aimConfirmBtn').attr('style', opts.confirmStyle);

        // Intercept show.bs.modal — validate calc result and pre-fill fields
        el.addEventListener('show.bs.modal', function (e) {
            if (!opts.onPrepare) return;
            const data = opts.onPrepare();
            if (data === false) {
                e.preventDefault();
                return;
            }
            self._fill(data || {});
        });

        // Confirm button
        $('#aimConfirmBtn').off('click.aim').on('click.aim', function () { self._confirm(); });

        // Live preview whenever inputs change
        $('#aimGrossAmount, #aimDiscountPct, #aimItemQty')
            .off('input.aim').on('input.aim', function () { self.updatePreview(); });

        // HSN/SAC selection events → refresh preview
        $(document).off('hsnSacSelected.aim').on('hsnSacSelected.aim', function () { self.updatePreview(); });
        $(document).off('hsnSacCleared.aim').on('hsnSacCleared.aim', function () { self.updatePreview(); });

        // Clear validation state when user starts typing name
        $('#aimItemName').off('input.aim').on('input.aim', function () {
            $(this).removeClass('is-invalid');
        });

        // Smart Name / Description button
        $('#aimSuggestBtn').off('click.aim').on('click.aim', function () {
            const btn = $(this);
            const origHtml = btn.html();
            btn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm"></span>');
            setTimeout(function () {
                const info = self._suggestProductInfo();
                $('#aimItemName').val(info.name).removeClass('is-invalid');
                if (info.description) $('#aimItemDesc').val(info.description);
                btn.prop('disabled', false).html(origHtml);
            }, 320);
        });
    },

    // Pre-fill all modal fields from rate-calculator data
    _fill(data) {
        $('#aimItemName').val(data.productName || '').removeClass('is-invalid');
        $('#aimItemQty').val(data.qty || 1);
        $('#aimItemDesc').val(data.description || '');
        $('#aimGrossAmount').val((data.grossAmount || 0).toFixed(2));
        $('#aimDiscountPct').val(0);
        if (typeof HsnSacSelector !== 'undefined') HsnSacSelector.clear();
        this._showCalcSummary();
        this.updatePreview();
    },

    // Recalculate and render the GST breakdown preview
    updatePreview() {
        const gross    = parseFloat($('#aimGrossAmount').val()) || 0;
        const discPct  = parseFloat($('#aimDiscountPct').val()) || 0;
        const discAmt  = gross * (discPct / 100);
        const taxable  = gross - discAmt;

        const hsnItem = (typeof HsnSacSelector !== 'undefined') ? HsnSacSelector.getSelected() : null;
        const gstRate = hsnItem ? (hsnItem.defaultGstRate || 0) : 0;

        const gstType = this._opts.gstTypeGetter ? this._opts.gstTypeGetter() : 'INTRA';
        let cgst = 0, sgst = 0, igst = 0;

        if (gstType === 'INTER') {
            igst = taxable * (gstRate / 100);
            $('#aimCgstWrap, #aimSgstWrap').addClass('d-none');
            $('#aimIgstWrap').removeClass('d-none');
            $('#aimGstTypeText').text('IGST (Inter-State)');
        } else {
            cgst = taxable * ((gstRate / 2) / 100);
            sgst = cgst;
            $('#aimCgstWrap, #aimSgstWrap').removeClass('d-none');
            $('#aimIgstWrap').addClass('d-none');
            $('#aimGstTypeText').text('CGST + SGST (Intra-State)');
        }

        const net = taxable + cgst + sgst + igst;
        $('#aimTaxable').text(this._fmt(taxable));
        $('#aimCgst').text(this._fmt(cgst));
        $('#aimSgst').text(this._fmt(sgst));
        $('#aimIgst').text(this._fmt(igst));
        $('#aimNetTotal').text(this._fmt(net));
    },

    // Validate and dispatch to module's onConfirm callback
    _confirm() {
        const name = ($('#aimItemName').val() || '').trim();
        if (!name) {
            $('#aimItemName').addClass('is-invalid').focus();
            return;
        }

        const data = {
            name:        name,
            qty:         Math.max(1, parseInt($('#aimItemQty').val()) || 1),
            description: ($('#aimItemDesc').val() || '').trim(),
            grossAmount: parseFloat($('#aimGrossAmount').val()) || 0,
            discPct:     parseFloat($('#aimDiscountPct').val()) || 0,
            hsnItem:     (typeof HsnSacSelector !== 'undefined') ? HsnSacSelector.getSelected() : null
        };

        if (this._opts.onConfirm) this._opts.onConfirm(data);
    },

    // Close the modal programmatically
    close() {
        const el = document.getElementById('aimModal');
        if (el) {
            const m = bootstrap.Modal.getInstance(el);
            if (m) m.hide();
        }
    },

    // Populate the Rate Calculator Estimate banner from the shared result elements
    _showCalcSummary() {
        const parse = id => parseFloat(($('#' + id).text() || '').replace(/[\u20b9,]/g, '')) || 0;
        const gross   = parse('resGrandTotal');
        const tax     = parse('resTax');
        const net     = parse('resNetTotal');
        const perUnit = parse('resCostPerUnit');
        if (gross <= 0) { $('#aimCalcBanner').hide(); return; }
        $('#aimEstGross').text(this._fmt(gross));
        $('#aimEstTax').text(this._fmt(tax));
        $('#aimEstNet').text(this._fmt(net || (gross + tax)));
        $('#aimEstUnit').text(this._fmt(perUnit));
        $('#aimCalcBanner').show();
    },

    // Build a smart product name and description from rate-calculator input fields
    _suggestProductInfo() {
        const trim    = s => (s || '').trim();
        const jobType = trim($('#ddlJobType option:selected').text());
        const prodType = trim($('#ddlProductType option:selected').data('name') || '');
        const sizeText = trim($('#ddlProductSize option:selected').text());
        const pages    = parseInt($('#txtTotalPages').val()) || 0;
        const qty      = parseInt($('#txtQuantity').val()) || 0;
        const method   = trim($('#txtPrintingMode').val());
        const trimW    = parseFloat($('#txtTrimWidth').val()) || 0;
        const trimH    = parseFloat($('#txtTrimHeight').val()) || 0;

        const sizeOk = sizeText && !/^(select|choose|--|custom|size)$/i.test(sizeText);
        const sizeDisplay = sizeOk ? sizeText : (trimW > 0 && trimH > 0 ? trimW + '×' + trimH + 'mm' : '');

        // --- Name: primary noun · size · pages ---
        const nameParts = [];
        const primary = prodType || jobType;
        if (primary) nameParts.push(primary);
        if (sizeDisplay) nameParts.push(sizeDisplay);
        if (pages > 0) nameParts.push(pages + ' Pg');

        // --- Description: full spec line ---
        const descParts = [];
        if (jobType) descParts.push(jobType);
        if (prodType && prodType !== jobType) descParts.push('Type: ' + prodType);
        if (sizeDisplay) descParts.push('Size: ' + sizeDisplay);
        if (pages > 0) descParts.push('Pages: ' + pages);
        if (method) descParts.push(method);
        if (qty > 0) descParts.push('Qty: ' + qty.toLocaleString('en-IN'));

        return {
            name:        nameParts.join(' · ') || 'Custom Item',
            description: descParts.join(' | ')
        };
    },

    _fmt(n) {
        return '₹' + (n || 0).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }
};
