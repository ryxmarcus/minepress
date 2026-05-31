// ===== MinePress Quotation Create Module — JS =====

const QT_CREATE_API = '/api/quotation';
const ENQ_API_BASE = '/api/enquiry';

const QtCreate = {
    _items: [],
    _selectedCustomer: null,
    _enquiryId: null,
    _companyStateCode: null,
    _customerStateCode: null,
    _gstType: 'INTRA', // INTRA = CGST+SGST, INTER = IGST

    // ══════════════════════════════════════════════
    //  INIT
    // ══════════════════════════════════════════════

    async init(fromEnquiryId) {
        const self = this;

        // Load company info for GST state comparison
        await this.loadCompanyInfo();

        // Initialize customer search widget
        CustomerSearch.init({
            apiBase: ENQ_API_BASE,
            onSelect: function (cust) {
                self._selectedCustomer = cust;
                self.onCustomerSelected(cust);
            },
            onClear: function () {
                self._selectedCustomer = null;
                self._customerStateCode = null;
                self.updateGstType();
                $('#qtQuickCustomer').html('<i class="bi bi-person me-1"></i>No customer selected');
                $('#qtQuickGst').html('<i class="bi bi-shield me-1"></i>GST: Auto-detect');
                self.updateSaveButtonState();
            }
        });

        // Init HSN/SAC Selector
        HsnSacSelector.init();

        // Init shared Add Item Modal
        AddItemModal.init({
            title: 'Add Item to Quotation',
            avatarClass: 'bg-green-lt text-green',
            confirmStyle: 'background:var(--tblr-success);color:#fff;',
            gstTypeGetter: function () { return self._gstType; },
            onPrepare: function () {
                const grandTotal = parseFloat($('#resGrandTotal').text().replace(/[₹,]/g, '')) || 0;
                if (grandTotal <= 0) {
                    Swal2.warning('Please calculate rate first before adding an item.');
                    return false;
                }
                return {
                    grossAmount: grandTotal,
                    qty: parseInt($('#txtQuantity').val()) || 0,
                    productName: $('#ddlProductType option:selected').data('name') || $('#ddlJobType option:selected').text() || '',
                    description: ''
                };
            },
            onConfirm: function (data) { self.confirmAddItem(data); }
        });

        // Event bindings
        $('#btnSaveQuotation').on('click', function () { self.saveQuotation(); });

        // Sync quick info panel
        $('#txtValidTill').on('change', function () {
            const v = $(this).val();
            $('#qtQuickValidTill').html(v
                ? '<i class="bi bi-calendar-event-fill me-1 text-green"></i>Valid till: ' + v
                : '<i class="bi bi-calendar-event me-1"></i>Valid till: Not set');
        });

        // Set default valid till to 30 days from now
        const d = new Date();
        d.setDate(d.getDate() + 30);
        $('#txtValidTill').val(d.toISOString().split('T')[0]).trigger('change');

        // Load from enquiry if applicable
        if (fromEnquiryId) {
            this._enquiryId = fromEnquiryId;
            await this.loadFromEnquiry(fromEnquiryId);
            $('#qtQuickSource').html('<i class="bi bi-clipboard-data-fill me-1 text-primary"></i>Source: From Enquiry');
        }
    },

    // ══════════════════════════════════════════════
    //  COMPANY & GST
    // ══════════════════════════════════════════════

    async loadCompanyInfo() {
        try {
            const data = await $.get(`${QT_CREATE_API}/company-info`);
            if (data && data.stateCode) {
                this._companyStateCode = data.stateCode;
            }
        } catch (err) {
            console.warn('Failed to load company info for GST:', err);
        }
    },

    onCustomerSelected(cust) {
        // Extract state code from GSTIN (first 2 digits)
        if (cust.gstno && cust.gstno.length >= 2) {
            this._customerStateCode = cust.gstno.substring(0, 2);
        } else {
            this._customerStateCode = null;
        }
        this.updateGstType();

        // Update quick info
        $('#qtQuickCustomer').html('<i class="bi bi-person-fill me-1 text-green"></i>' + this.esc(cust.name));
        const gstText = cust.gstno ? 'GSTIN: ' + this.esc(cust.gstno) : 'No GSTIN';
        $('#qtQuickGst').html('<i class="bi bi-shield-fill-check me-1 text-teal"></i>' + gstText);

        // Recalculate all items with new GST type
        if (this._items.length > 0) {
            this._items.forEach((_, idx) => this.recalcItem(idx));
            this.renderItemsSummary();
            this.updateTotals();
        }

        this.updateSaveButtonState();
    },

    updateGstType() {
        if (this._companyStateCode && this._customerStateCode) {
            this._gstType = this._companyStateCode === this._customerStateCode ? 'INTRA' : 'INTER';
        } else {
            this._gstType = 'INTRA'; // Default to intra-state
        }

        // Update badge in header
        const badge = $('#qtGstBadge');
        const text = $('#qtGstTypeText');
        if (this._gstType === 'INTER') {
            badge.removeClass('d-none');
            text.text('IGST (Inter-State)');
            $('#qtGstTypeBadge').removeClass('bg-teal-lt').addClass('bg-azure-lt');
            $('#qtCgstRow, #qtSgstRow').hide();
            $('#qtIgstRow').show();
        } else {
            badge.removeClass('d-none');
            text.text('CGST + SGST (Intra-State)');
            $('#qtGstTypeBadge').removeClass('bg-azure-lt').addClass('bg-teal-lt');
            $('#qtCgstRow, #qtSgstRow').show();
            $('#qtIgstRow').hide();
        }
        if (typeof AddItemModal !== 'undefined') AddItemModal.updatePreview();
    },

    // ══════════════════════════════════════════════
    //  LOAD FROM ENQUIRY
    // ══════════════════════════════════════════════

    async loadFromEnquiry(enquiryId) {
        try {
            const data = await $.get(`${QT_CREATE_API}/from-enquiry/${enquiryId}`);
            $('#qtCreateTitle').text('New Quotation from ' + data.enquiryNo);
            $('#qtCreateSubtitle').text('Converting enquiry ' + data.enquiryNo + ' to quotation');

            // Set customer via the customer search widget
            if (data.partyId) {
                CustomerSearch.selectById(data.partyId);
            }

            // Populate items from enquiry
            if (data.items && data.items.length > 0) {
                data.items.forEach((item, idx) => {
                    const unitRate = item.costPerUnit || 0;
                    const qty = item.quantity || 0;
                    const gross = unitRate * qty;
                    const gstRate = 18; // Default GST rate

                    this._items.push({
                        seq: idx + 1,
                        enquiryItemId: item.enquiryItemId,
                        rateCalculatorId: item.rateCalculatorId,
                        calcRefNo: item.calcRefNo,
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
                        unitRate: unitRate,
                        grossAmount: gross,
                        discountPercent: 0,
                        discountAmount: 0,
                        taxableValue: gross,
                        gstRate: gstRate,
                        cgstPercent: 0,
                        cgstAmount: 0,
                        sgstPercent: 0,
                        sgstAmount: 0,
                        igstPercent: 0,
                        igstAmount: 0,
                        totalTaxAmount: 0,
                        netAmount: 0,
                        hsnSacCodeId: null,
                        rateCalcSnapshot: null
                    });
                });

                // Recalculate with correct GST type
                this._items.forEach((_, idx) => this.recalcItem(idx));
                this.renderItemsSummary();
                this.updateTotals();
                this.updateSaveButtonState();
            }
        } catch (err) {
            Swal2.error('Failed to load enquiry data: ' + (err.responseJSON?.message || err.statusText));
        }
    },

    // ══════════════════════════════════════════════
    //  ADD ITEM (from Rate Calculator)
    // ══════════════════════════════════════════════

    confirmAddItem(data) {
        const name        = data.name;
        const qty         = data.qty;
        const description = data.description;
        const grossAmount = data.grossAmount;
        const discPct     = data.discPct;
        const hsnItem     = data.hsnItem;

        if (qty <= 0) {
            Swal2.warning('Quantity must be at least 1.');
            return;
        }

        const costPerUnit  = parseFloat($('#resCostPerUnit').text().replace(/[₹,]/g, '')) || (qty > 0 ? grossAmount / qty : 0);
        const discAmt      = grossAmount * (discPct / 100);
        const taxable      = grossAmount - discAmt;
        const hsnSacCodeId = hsnItem ? hsnItem.id : null;
        const gstRate      = hsnItem ? (hsnItem.defaultGstRate || 0) : 0;

        const calcResult = window._lastCalcResult || {};
        const partsData  = window._lastCalcPartsData || [];
        const snapshot   = this.collectRateCalcSnapshot();

        let cgstPct = 0, sgstPct = 0, igstPct = 0;
        if (this._gstType === 'INTER') {
            igstPct = gstRate;
        } else {
            cgstPct = gstRate / 2;
            sgstPct = gstRate / 2;
        }

        const item = {
            seq: this._items.length + 1,
            enquiryItemId: null,
            rateCalculatorId: null,
            calcRefNo: null,
            productName: name,
            productDescription: description,
            productTypeName: $('#ddlProductType option:selected').data('name') || '',
            jobTypeName: $('#ddlJobType option:selected').text() || '',
            productSizeName: $('#ddlProductSize option:selected').text() || '',
            quantity: qty,
            noOfPages: parseInt($('#txtTotalPages').val()) || 0,
            trimWidthMm: parseFloat($('#txtTrimWidth').val()) || 0,
            trimHeightMm: parseFloat($('#txtTrimHeight').val()) || 0,
            printingMethod: $('#txtPrintingMode').val() || '',
            unitRate: costPerUnit,
            grossAmount: grossAmount,
            discountPercent: discPct,
            discountAmount: discAmt,
            taxableValue: taxable,
            gstRate: gstRate,
            cgstPercent: cgstPct,
            cgstAmount: taxable * (cgstPct / 100),
            sgstPercent: sgstPct,
            sgstAmount: taxable * (sgstPct / 100),
            igstPercent: igstPct,
            igstAmount: taxable * (igstPct / 100),
            totalTaxAmount: 0,
            netAmount: 0,
            hsnSacCodeId: hsnSacCodeId,
            costPerUnit: costPerUnit,
            rateCalcGrandTotal: parseFloat($('#resGrandTotal').text().replace(/[₹,]/g, '')) || grossAmount,
            rateCalcSnapshot: snapshot,
            specificationsJson: JSON.stringify(snapshot),
            partsData: JSON.stringify(partsData),
            costBreakdown: JSON.stringify(calcResult.breakdown || []),
            bomData: JSON.stringify(calcResult.bom || []),
            aiInsights: JSON.stringify(calcResult.appliedRules || []),
            calcInputSnapshot: JSON.stringify(snapshot),
            configData: this.buildConfigData(snapshot, {
                productName: name,
                quantity: qty,
                noOfPages: parseInt($('#txtTotalPages').val()) || 0,
                trimWidthMm: parseFloat($('#txtTrimWidth').val()) || 0,
                trimHeightMm: parseFloat($('#txtTrimHeight').val()) || 0
            }, calcResult, partsData),
            recommendedMachines: this.buildRecommendedMachines(snapshot, {
                quantity: qty,
                printingMethod: $('#txtPrintingMode').val() || '',
                trimWidthMm: parseFloat($('#txtTrimWidth').val()) || 0,
                trimHeightMm: parseFloat($('#txtTrimHeight').val()) || 0
            })
        };

        item.totalTaxAmount = item.cgstAmount + item.sgstAmount + item.igstAmount;
        item.netAmount      = item.taxableValue + item.totalTaxAmount;

        this._items.push(item);
        this.renderItemsSummary();
        this.updateTotals();
        this.updateSaveButtonState();

        AddItemModal.close();

        const drawerEl = document.getElementById('drawerQtRateCalc');
        if (drawerEl && window.bootstrap && bootstrap.Offcanvas) {
            const drawer = bootstrap.Offcanvas.getInstance(drawerEl) || bootstrap.Offcanvas.getOrCreateInstance(drawerEl);
            drawer.hide();
        }

        Swal2.success('Item "' + name + '" added successfully!');

        window._lastCalcResult    = null;
        window._lastCalcPartsData = null;
        $('#btnReset').trigger('click');
    },

    collectRateCalcSnapshot() {
        return {
            jobTypeId: parseInt($('#ddlJobType').val()) || null,
            productTypeId: parseInt($('#ddlProductType').val()) || null,
            productSizeId: parseInt($('#ddlProductSize').val()) || null,
            machineId: parseInt($('#ddlMachine').val()) || null,
            plateId: parseInt($('#ddlPlate').val()) || null,
            printingSides: parseInt($('#ddlSides').val()) || 1,
            quantity: parseInt($('#txtQuantity').val()) || 0,
            totalPages: parseInt($('#txtTotalPages').val()) || 0,
            trimWidthMm: parseFloat($('#txtTrimWidth').val()) || 0,
            trimHeightMm: parseFloat($('#txtTrimHeight').val()) || 0,
            printingMode: $('#txtPrintingMode').val() || '',
            isCustomerMaterial: $('#chkCustomerMaterial').is(':checked'),
            paperId: parseInt($('#ddlPaper').val()) || null,
            inkCodes: $('#ddlInks').val() || [],
            finishingIds: ($('#ddlFinishings').val() || []).map(Number),
            bindingIds: ($('#ddlBinding').val() || []).map(Number),
            designingIds: ($('#ddlDesigning').val() || []).map(Number),
            jobTypeName: $('#ddlJobType option:selected').text() || '',
            productTypeName: $('#ddlProductType option:selected').data('name') || '',
            productSizeName: $('#ddlProductSize option:selected').text() || '',
            machineName: $('#ddlMachine option:selected').text() || '',
            partDetails: typeof collectPartDetails === 'function' ? collectPartDetails() : []
        };
    },

    buildConfigData(snapshot, item, calcResult, partsData) {
        const flags = (calcResult && calcResult.jobTypeFlags) || {};
        const workflowStages = [];
        if (flags.isDesignRequired || flags.isDtpRequired) workflowStages.push('Design/DTP');
        if (flags.isCtpRequired) workflowStages.push('CTP/Plates');
        if (flags.isPrintingRequired) workflowStages.push('Printing');
        if (flags.isBindingRequired) workflowStages.push('Binding');
        if (flags.isFinishingRequired) workflowStages.push('Finishing');

        const plateName = $('#ddlPlate option:selected').text() || '';
        const sidesText = $('#ddlSides option:selected').text() || '';
        const designingNames = ($('#ddlDesigning option:selected').map(function () { return $(this).text(); }).get() || []);
        const bindingNames = ($('#ddlBinding option:selected').map(function () { return $(this).text(); }).get() || []);
        const finishingNames = ($('#ddlFinishings option:selected').map(function () { return $(this).text(); }).get() || []);

        const rawParts = (partsData && partsData.length > 0)
            ? partsData
            : [{
                partName: item.productName || '',
                noOfPages: item.noOfPages || null,
                colors: snapshot.printingSides || null,
                paperName: ''
            }];

        const productParts = rawParts.map(p => ({
            partName: p.partName || '',
            specification: {
                pages: p.noOfPages || null,
                color: p.colors || null,
                paper: p.paperName || null
            },
            designDtp: (flags.isDesignRequired || flags.isDtpRequired) ? { designItems: designingNames } : null,
            ctpPlates: flags.isCtpRequired ? { plateName: plateName } : null,
            printing: flags.isPrintingRequired ? { machineName: snapshot.machineName || '' } : null
        }));

        return JSON.stringify({
            jobType: { value: snapshot.jobTypeName || '', workflowStages },
            productType: snapshot.productTypeName || '',
            productSize: snapshot.productSizeName || '',
            trimWidth: item.trimWidthMm || snapshot.trimWidthMm || 0,
            trimHeight: item.trimHeightMm || snapshot.trimHeightMm || 0,
            quantity: item.quantity || snapshot.quantity || 0,
            sides: sidesText || '',
            productParts,
            binding: flags.isBindingRequired ? { bindingTypes: bindingNames } : null,
            finishing: flags.isFinishingRequired ? { finishingTypes: finishingNames } : null
        });
    },

    buildRecommendedMachines(snapshot, item) {
        const machineId = snapshot.machineId || null;
        const machineName = (snapshot.machineName || '').trim();
        if (!machineId || !machineName) return null;

        return JSON.stringify([{
            machineId: machineId,
            machineName: machineName,
            printingMode: item.printingMethod || snapshot.printingMode || null,
            trimWidthMm: item.trimWidthMm || snapshot.trimWidthMm || 0,
            trimHeightMm: item.trimHeightMm || snapshot.trimHeightMm || 0,
            quantity: item.quantity || snapshot.quantity || 0
        }]);
    },

    // ══════════════════════════════════════════════
    //  ITEM CALCULATIONS
    // ══════════════════════════════════════════════

    recalcItem(idx) {
        const item = this._items[idx];
        item.grossAmount = item.quantity * item.unitRate;
        item.discountAmount = item.grossAmount * (item.discountPercent / 100);
        item.taxableValue = item.grossAmount - item.discountAmount;

        // Apply GST based on type
        if (this._gstType === 'INTER') {
            item.cgstPercent = 0;
            item.cgstAmount = 0;
            item.sgstPercent = 0;
            item.sgstAmount = 0;
            item.igstPercent = item.gstRate || 0;
            item.igstAmount = item.taxableValue * (item.igstPercent / 100);
        } else {
            const halfRate = (item.gstRate || 0) / 2;
            item.cgstPercent = halfRate;
            item.cgstAmount = item.taxableValue * (halfRate / 100);
            item.sgstPercent = halfRate;
            item.sgstAmount = item.taxableValue * (halfRate / 100);
            item.igstPercent = 0;
            item.igstAmount = 0;
        }

        item.totalTaxAmount = item.cgstAmount + item.sgstAmount + item.igstAmount;
        item.netAmount = item.taxableValue + item.totalTaxAmount;
    },

    removeItem(idx) {
        this._items.splice(idx, 1);
        this._items.forEach((item, i) => item.seq = i + 1);
        this.renderItemsSummary();
        this.updateTotals();
        this.updateSaveButtonState();
    },

    // ══════════════════════════════════════════════
    //  RENDER
    // ══════════════════════════════════════════════

    renderItemsSummary() {
        const container = $('#qtItemsList');
        if (this._items.length === 0) {
            container.html('<div class="empty py-4">' +
                '<div class="empty-icon"><i class="bi bi-calculator fs-1 text-secondary opacity-50"></i></div>' +
                '<p class="empty-title">No items added yet</p>' +
                '<p class="empty-subtitle text-secondary">Click <strong>"Open Rate Calculator"</strong> to estimate and add items</p>' +
                '</div>');
            return;
        }

        let html = '';
        const self = this;
        this._items.forEach(function (item, idx) {
            const gstLabel = self._gstType === 'INTER'
                ? 'IGST ' + (item.igstPercent || 0) + '%'
                : 'GST ' + (item.gstRate || 0) + '% (CGST+SGST)';

            html += '<div class="qt-item-chip">' +
                '<button type="button" class="btn-close" onclick="QtCreate.removeItem(' + idx + ')" title="Remove"></button>' +
                '<div class="d-flex justify-content-between align-items-start">' +
                '<div>' +
                '<div class="qt-item-name">' +
                '<span class="badge bg-green-lt me-1">#' + item.seq + '</span>' +
                self.esc(item.productName) +
                '</div>' +
                '<div class="qt-item-meta mt-1">' +
                (item.jobTypeName ? '<span class="badge bg-blue-lt me-1"><i class="bi bi-printer me-1"></i>' + self.esc(item.jobTypeName) + '</span>' : '') +
                (item.productTypeName ? '<span class="badge bg-purple-lt me-1"><i class="bi bi-box me-1"></i>' + self.esc(item.productTypeName) + '</span>' : '') +
                (item.productSizeName ? '<span class="badge bg-teal-lt me-1"><i class="bi bi-rulers me-1"></i>' + self.esc(item.productSizeName) + '</span>' : '') +
                (item.printingMethod ? '<span class="badge bg-orange-lt me-1"><i class="bi bi-palette me-1"></i>' + self.esc(item.printingMethod) + '</span>' : '') +
                '</div>' +
                '<div class="qt-item-meta mt-1">' +
                (item.quantity ? 'Qty: ' + item.quantity : '') +
                (item.noOfPages ? ' · ' + item.noOfPages + ' pages' : '') +
                ((item.trimWidthMm && item.trimHeightMm) ? ' · ' + item.trimWidthMm + '×' + item.trimHeightMm + 'mm' : '') +
                '</div>' +
                (item.productDescription ? '<div class="qt-item-meta mt-1"><i class="bi bi-chat-text me-1"></i>' + self.esc(item.productDescription) + '</div>' : '') +
                (item.calcRefNo ? '<div class="qt-item-meta mt-1"><a href="/RateCalculator/Details?id=' + (item.rateCalculatorId || '') + '" class="badge bg-cyan-lt text-decoration-none"><i class="bi bi-link-45deg me-1"></i>' + self.esc(item.calcRefNo) + '</a></div>' : '') +
                '<div class="qt-item-meta mt-1">' +
                '<span class="badge bg-teal-lt">' + gstLabel + '</span>' +
                (item.discountPercent > 0 ? ' <span class="badge bg-orange-lt">Disc: ' + item.discountPercent + '%</span>' : '') +
                '</div>' +
                '</div>' +
                '<div class="text-end">' +
                '<div class="qt-item-price">' + self.fmt(item.netAmount) + '</div>' +
                '<div class="qt-item-meta">Unit: ' + self.fmt(item.unitRate) + '</div>' +
                '</div>' +
                '</div>' +
                '</div>';
        });
        container.html(html);
    },

    updateTotals() {
        let subtotal = 0, discount = 0, taxable = 0, cgst = 0, sgst = 0, igst = 0, totalTax = 0, net = 0;
        this._items.forEach(function (item) {
            subtotal += item.grossAmount || 0;
            discount += item.discountAmount || 0;
            taxable += item.taxableValue || 0;
            cgst += item.cgstAmount || 0;
            sgst += item.sgstAmount || 0;
            igst += item.igstAmount || 0;
            totalTax += item.totalTaxAmount || 0;
            net += item.netAmount || 0;
        });

        $('#qtSubtotal').text(this.fmt(subtotal));
        $('#qtDiscount').text('-' + this.fmt(discount));
        $('#qtTaxable').text(this.fmt(taxable));
        $('#qtCgst').text(this.fmt(cgst));
        $('#qtSgst').text(this.fmt(sgst));
        $('#qtIgst').text(this.fmt(igst));
        $('#qtGrandTotal').text(this.fmt(net));
        $('#qtItemCount').text(this._items.length);
        $('#qtSummaryItemCount').text(this._items.length);
    },

    updateSaveButtonState() {
        const canSave = this._selectedCustomer && this._items.length > 0;
        $('#btnSaveQuotation').prop('disabled', !canSave);
    },

    // ══════════════════════════════════════════════
    //  SAVE: Rate Calc → then Quotation
    // ══════════════════════════════════════════════

    async saveQuotation() {
        if (!this._selectedCustomer) { Swal2.warning('Please select a customer.'); return; }
        if (this._items.length === 0) { Swal2.warning('Please add at least one item.'); return; }

        const btn = $('#btnSaveQuotation');
        btn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm me-2"></span>Saving...');
        const progress = window.SaveProgress || null;
        const useOverlay = !!progress;
        if (useOverlay) {
            progress.start({
                title: 'Saving Quotation',
                subtitle: 'We are persisting rates, calculating totals, and creating quotation.',
                steps: [
                    { title: 'Persist item rate calculations', detail: 'Saving calculator records for required items' },
                    { title: 'Calculate quotation totals', detail: 'Preparing amount, discount, tax, and net totals' },
                    { title: 'Create quotation', detail: 'Saving quotation header and line items' },
                    { title: 'Send customer email', detail: 'Dispatching quotation copy to customer inbox' }
                ],
                message: 'Starting save process...'
            });
        } else {
            Swal2.showLoading('Saving Quotation...');
        }

        try {
            // Step 1: Save each item's rate calculation to hyb_job_rate_calculator
            if (useOverlay) progress.setStep(0, `Saving rate calculations for ${this._items.length} item(s)...`);
            for (let i = 0; i < this._items.length; i++) {
                const item = this._items[i];
                // Skip if already has a rate calc ID (from enquiry conversion)
                if (item.rateCalculatorId) continue;
                if (!item.rateCalcSnapshot) continue;

                const snap = item.rateCalcSnapshot;
                const rateCalcPayload = {
                    partyId: this._selectedCustomer.partyId,
                    jobTypeId: snap.jobTypeId,
                    productTypeId: snap.productTypeId,
                    productSizeId: snap.productSizeId,
                    quantity: item.quantity,
                    totalPages: item.noOfPages || 0,
                    trimWidthMm: item.trimWidthMm,
                    trimHeightMm: item.trimHeightMm,
                    printingMode: item.printingMethod,
                    isCustomerMaterial: snap.isCustomerMaterial || false,
                    grandTotal: item.rateCalcGrandTotal || item.grossAmount,
                    taxAmount: item.totalTaxAmount,
                    netTotal: item.netAmount,
                    costPerUnit: item.costPerUnit || item.unitRate,
                    partsData: item.partsData || null,
                    costBreakdown: item.costBreakdown || null,
                    bomData: item.bomData || null,
                    aiInsights: item.aiInsights || null,
                    calcInputSnapshot: item.calcInputSnapshot || item.specificationsJson,
                    configData: item.configData || null,
                    recommendedMachines: item.recommendedMachines || null
                };

                const rcResult = await $.ajax({
                    url: ENQ_API_BASE + '/saveratecalc',
                    method: 'POST',
                    contentType: 'application/json',
                    data: JSON.stringify(rateCalcPayload)
                });

                item.rateCalculatorId = rcResult.rateCalcId;
                item.calcRefNo = rcResult.calcRefNo;
                if (useOverlay) progress.setMessage(`Rate calc saved for item ${i + 1} of ${this._items.length}.`);
            }

            // Step 2: Calculate totals
            if (useOverlay) progress.setStep(1, 'Computing quotation totals...');
            let totalAmount = 0, discountAmount = 0, taxableAmount = 0, taxAmount = 0, netAmount = 0;
            this._items.forEach(function (item) {
                totalAmount += item.grossAmount || 0;
                discountAmount += item.discountAmount || 0;
                taxableAmount += item.taxableValue || 0;
                taxAmount += item.totalTaxAmount || 0;
                netAmount += item.netAmount || 0;
            });

            // Step 3: Save quotation
            const payload = {
                partyId: this._selectedCustomer.partyId,
                enquiryId: this._enquiryId,
                partyRefNo: $('#txtPartyRefNo').val() || null,
                validTill: $('#txtValidTill').val() || null,
                termsConditions: $('#txtTerms').val() || null,
                remarks: $('#txtRemarks').val() || null,
                totalAmount: totalAmount,
                discountAmount: discountAmount,
                taxableAmount: taxableAmount,
                taxAmount: taxAmount,
                netAmount: netAmount,
                items: this._items.map(function (item, idx) {
                    return {
                        enquiryItemId: item.enquiryItemId || null,
                        itemSequence: idx + 1,
                        productName: item.productName,
                        productDescription: item.productDescription,
                        productTypeName: item.productTypeName || '',
                        jobTypeName: item.jobTypeName || '',
                        productSizeName: item.productSizeName || '',
                        noOfPages: item.noOfPages || 0,
                        trimWidthMm: item.trimWidthMm || 0,
                        trimHeightMm: item.trimHeightMm || 0,
                        printingMethod: item.printingMethod || '',
                        quantity: item.quantity,
                        unitRate: item.unitRate,
                        grossAmount: item.grossAmount,
                        discountPercent: item.discountPercent,
                        discountAmount: item.discountAmount,
                        taxableValue: item.taxableValue,
                        cgstPercent: item.cgstPercent,
                        cgstAmount: item.cgstAmount,
                        sgstPercent: item.sgstPercent,
                        sgstAmount: item.sgstAmount,
                        igstPercent: item.igstPercent || 0,
                        igstAmount: item.igstAmount || 0,
                        totalTaxAmount: item.totalTaxAmount,
                        netAmount: item.netAmount,
                        rateCalculatorId: item.rateCalculatorId,
                        calcRefNo: item.calcRefNo,
                        remarks: ''
                    };
                })
            };

            if (useOverlay) progress.setStep(2, 'Creating quotation record...');
            const result = await $.ajax({
                url: QT_CREATE_API + '/save',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(payload)
            });

            if (!useOverlay) {
                Swal2.hideLoading();
            }

            // Auto-send email to customer (best-effort)
            try {
                if (useOverlay) progress.setStep(3, 'Sending quotation email to customer...');
                await $.ajax({ url: QT_CREATE_API + '/send-email/' + result.quotationId, method: 'POST' });
            } catch (_) { /* email send is best-effort */ }

            if (useOverlay) {
                progress.complete('Quotation created successfully.');
                setTimeout(function () { progress.close(); }, 250);
            }

            await Swal.fire({
                icon: 'success',
                title: 'Quotation Created & Emailed!',
                html: '<strong>' + result.quotationNo + '</strong> saved and emailed to the customer.',
                confirmButtonText: 'View Quotation',
                confirmButtonColor: '#198754',
                showCancelButton: true,
                cancelButtonText: 'Back to List'
            }).then(function (res) {
                if (res.isConfirmed) {
                    window.location.href = '/Quotation/Details?id=' + result.quotationId;
                } else {
                    window.location.href = '/Quotation';
                }
            });

        } catch (err) {
            if (useOverlay) {
                progress.error('Save failed. Please review the error and retry.');
                setTimeout(function () { progress.close(); }, 600);
            } else {
                Swal2.hideLoading();
            }
            const msg = err.responseJSON?.message || err.responseText || 'Unknown error';
            Swal2.error('Failed to save: ' + msg);
        } finally {
            btn.prop('disabled', false).html('<i class="bi bi-check-lg me-2"></i>Save Quotation');
        }
    },

    // ══════════════════════════════════════════════
    //  HELPERS
    // ══════════════════════════════════════════════

    fmt(n) {
        if (n == null || isNaN(n)) return '₹0.00';
        return '₹' + Number(n).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    },

    esc(value) {
        return (value || '')
            .replaceAll('&', '&amp;')
            .replaceAll('<', '&lt;')
            .replaceAll('>', '&gt;')
            .replaceAll('"', '&quot;')
            .replaceAll("'", '&#39;');
    }
};
