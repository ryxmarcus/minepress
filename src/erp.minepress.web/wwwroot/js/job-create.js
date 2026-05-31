// ===== MinePress Job Create Module — JS =====

const JB_CREATE_API = '/api/job';
const JB_ENQ_API = '/api/enquiry';

const JbCreate = {
    _items: [],
    _selectedCustomer: null,
    _enquiryId: null,
    _quotationId: null,
    _companyStateCode: null,
    _customerStateCode: null,
    _gstType: 'INTRA', // INTRA = CGST+SGST, INTER = IGST

    // ══════════════════════════════════════════════
    //  INIT
    // ══════════════════════════════════════════════

    async init(fromEnquiryId, fromQuotationId) {
        const self = this;

        // Load company info for GST state comparison
        await this.loadCompanyInfo();

        // Initialize customer search widget
        CustomerSearch.init({
            apiBase: JB_ENQ_API,
            onSelect: function (cust) {
                self._selectedCustomer = cust;
                self.onCustomerSelected(cust);
            },
            onClear: function () {
                self._selectedCustomer = null;
                self._customerStateCode = null;
                self.updateGstType();
                $('#jbQuickCustomer').html('<i class="bi bi-person me-1"></i>No customer selected');
                $('#jbQuickGst').html('<i class="bi bi-shield me-1"></i>GST: Auto-detect');
                self.updateSaveButtonState();
            }
        });

        // Init HSN/SAC Selector
        HsnSacSelector.init();

        // Init shared Add Item Modal
        AddItemModal.init({
            title: 'Add Item to Job',
            avatarClass: 'bg-purple-lt text-purple',
            confirmStyle: 'background:#6f42c1;color:#fff;',
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
        $('#btnSaveJob').on('click', function () { self.saveJob(); });

        // Sync quick info panel
        $('#txtDeliveryDate').on('change', function () {
            const v = $(this).val();
            $('#jbQuickDelivery').html(v
                ? '<i class="bi bi-truck me-1 text-purple"></i>Delivery: ' + v
                : '<i class="bi bi-truck me-1"></i>Delivery: Not set');
        });

        // Set default delivery date to 7 days from now
        const d = new Date();
        d.setDate(d.getDate() + 7);
        $('#txtDeliveryDate').val(d.toISOString().split('T')[0]).trigger('change');

        // Load from enquiry or quotation
        if (fromEnquiryId) {
            this._enquiryId = fromEnquiryId;
            await this.loadFromEnquiry(fromEnquiryId);
            $('#jbQuickSource').html('<i class="bi bi-clipboard-data-fill me-1 text-primary"></i>Source: From Enquiry');
        } else if (fromQuotationId) {
            this._quotationId = fromQuotationId;
            await this.loadFromQuotation(fromQuotationId);
            $('#jbQuickSource').html('<i class="bi bi-file-earmark-text-fill me-1 text-success"></i>Source: From Quotation');
        }
    },

    // ══════════════════════════════════════════════
    //  COMPANY & GST
    // ══════════════════════════════════════════════

    async loadCompanyInfo() {
        try {
            const data = await $.get(`${JB_CREATE_API}/company-info`);
            if (data && data.stateCode) {
                this._companyStateCode = data.stateCode;
            }
        } catch (err) {
            console.warn('Failed to load company info for GST:', err);
        }
    },

    onCustomerSelected(cust) {
        if (cust.gstno && cust.gstno.length >= 2) {
            this._customerStateCode = cust.gstno.substring(0, 2);
        } else {
            this._customerStateCode = null;
        }
        this.updateGstType();

        $('#jbQuickCustomer').html('<i class="bi bi-person-fill me-1 text-purple"></i>' + this.esc(cust.name));
        const gstText = cust.gstno ? 'GSTIN: ' + this.esc(cust.gstno) : 'No GSTIN';
        $('#jbQuickGst').html('<i class="bi bi-shield-fill-check me-1 text-teal"></i>' + gstText);

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
            this._gstType = 'INTRA';
        }

        const badge = $('#jbGstBadge');
        const text = $('#jbGstTypeText');
        if (this._gstType === 'INTER') {
            badge.removeClass('d-none');
            text.text('IGST (Inter-State)');
            $('#jbGstTypeBadge').removeClass('bg-teal-lt').addClass('bg-azure-lt');
            $('#jbCgstRow, #jbSgstRow').hide();
            $('#jbIgstRow').show();
        } else {
            badge.removeClass('d-none');
            text.text('CGST + SGST (Intra-State)');
            $('#jbGstTypeBadge').removeClass('bg-azure-lt').addClass('bg-teal-lt');
            $('#jbCgstRow, #jbSgstRow').show();
            $('#jbIgstRow').hide();
        }
        if (typeof AddItemModal !== 'undefined') AddItemModal.updatePreview();
    },

    // ══════════════════════════════════════════════
    //  LOAD FROM ENQUIRY
    // ══════════════════════════════════════════════

    async loadFromEnquiry(enquiryId) {
        try {
            const data = await $.get(`${JB_CREATE_API}/from-enquiry/${enquiryId}`);
            $('#jbCreateTitle').text('New Job from ' + data.enquiryNo);
            $('#jbCreateSubtitle').text('Converting enquiry ' + data.enquiryNo + ' to job');

            if (data.partyId) {
                CustomerSearch.selectById(data.partyId);
            }

            if (data.items && data.items.length > 0) {
                data.items.forEach((item, idx) => {
                    const unitRate = item.costPerUnit || 0;
                    const qty = item.quantity || 1;
                    const gross = unitRate * qty;
                    const gstRate = 18;

                    this._items.push({
                        seq: idx + 1,
                        enquiryItemId: item.enquiryItemId,
                        quotationItemId: null,
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
                        printProductTypeId: item.printProductTypeId || null,
                        jobTypeId: item.jobTypeId || null,
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
    //  LOAD FROM QUOTATION
    // ══════════════════════════════════════════════

    async loadFromQuotation(quotationId) {
        try {
            const data = await $.get(`${JB_CREATE_API}/from-quotation/${quotationId}`);
            $('#jbCreateTitle').text('New Job from ' + data.quotationNo);
            $('#jbCreateSubtitle').text('Converting quotation ' + data.quotationNo + ' to job');

            if (data.partyId) {
                CustomerSearch.selectById(data.partyId);
            }

            // Pre-fill job info from quotation
            if (data.partyRefNo) $('#txtPartyRefNo').val(data.partyRefNo);

            if (data.items && data.items.length > 0) {
                data.items.forEach((item, idx) => {
                    this._items.push({
                        seq: idx + 1,
                        enquiryItemId: item.enquiryItemId || null,
                        quotationItemId: item.quotationItemId,
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
                        printProductTypeId: item.printProductTypeId || null,
                        jobTypeId: item.jobTypeId || null,
                        taxCategoryId: item.taxCategoryId || null,
                        hsnSacCode: item.hsnSacCode || null,
                        quantity: item.quantity || 1,
                        unitRate: item.unitRate || 0,
                        grossAmount: item.grossAmount || 0,
                        discountPercent: item.discountPercent || 0,
                        discountAmount: item.discountAmount || 0,
                        taxableValue: item.taxableValue || 0,
                        gstRate: item.gstRate || 0,
                        cgstPercent: item.cgstPercent || 0,
                        cgstAmount: item.cgstAmount || 0,
                        sgstPercent: item.sgstPercent || 0,
                        sgstAmount: item.sgstAmount || 0,
                        igstPercent: item.igstPercent || 0,
                        igstAmount: item.igstAmount || 0,
                        totalTaxAmount: item.totalTaxAmount || 0,
                        netAmount: item.netAmount || 0,
                        hsnSacCodeId: item.hsnSacCodeId || null,
                        rateCalcSnapshot: null
                    });
                });

                this._items.forEach((_, idx) => this.recalcItem(idx));
                this.renderItemsSummary();
                this.updateTotals();
                this.updateSaveButtonState();
            }
        } catch (err) {
            Swal2.error('Failed to load quotation data: ' + (err.responseJSON?.message || err.statusText));
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

        const costPerUnit = parseFloat($('#resCostPerUnit').text().replace(/[₹,]/g, '')) || (qty > 0 ? grossAmount / qty : 0);
        const discAmt     = grossAmount * (discPct / 100);
        const taxable     = grossAmount - discAmt;

        const hsnSacCodeId = hsnItem ? hsnItem.id : null;
        const hsnSacCode = hsnItem ? hsnItem.code : null;
        const taxCategoryId = hsnItem ? (hsnItem.taxCategoryId || null) : null;
        const gstRate      = hsnItem ? (hsnItem.defaultGstRate || 0) : 0;

        const calcResult = window._lastCalcResult || {};
        const partsData = window._lastCalcPartsData || [];
        const snapshot = this.collectRateCalcSnapshot();

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
            quotationItemId: null,
            rateCalculatorId: null,
            calcRefNo: null,
            productName: name,
            productDescription: description,
            productTypeName: $('#ddlProductType option:selected').data('name') || '',
            jobTypeName: $('#ddlJobType option:selected').text() || '',
            productSizeName: $('#ddlProductSize option:selected').text() || '',
            printProductTypeId: parseInt($('#ddlProductType').val()) || null,
            jobTypeId: parseInt($('#ddlJobType').val()) || null,
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
            hsnSacCode: hsnSacCode,
            taxCategoryId: taxCategoryId,
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
        item.netAmount = item.taxableValue + item.totalTaxAmount;

        this._items.push(item);
        this.renderItemsSummary();
        this.updateTotals();
        this.updateSaveButtonState();

        AddItemModal.close();

        const drawerEl = document.getElementById('drawerJbRateCalc');
        if (drawerEl && window.bootstrap && bootstrap.Offcanvas) {
            const drawer = bootstrap.Offcanvas.getInstance(drawerEl) || bootstrap.Offcanvas.getOrCreateInstance(drawerEl);
            drawer.hide();
        }

        Swal2.success('Item "' + name + '" added successfully!');

        window._lastCalcResult = null;
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
        const container = $('#jbItemsList');
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

            html += '<div class="jb-item-chip">' +
                '<button type="button" class="btn-close" onclick="JbCreate.removeItem(' + idx + ')" title="Remove"></button>' +
                '<div class="d-flex justify-content-between align-items-start">' +
                '<div>' +
                '<div class="jb-item-name">' +
                '<span class="badge bg-purple-lt me-1">#' + item.seq + '</span>' +
                self.esc(item.productName) +
                '</div>' +
                '<div class="jb-item-meta mt-1">' +
                (item.jobTypeName ? '<span class="badge bg-blue-lt me-1"><i class="bi bi-printer me-1"></i>' + self.esc(item.jobTypeName) + '</span>' : '') +
                (item.productTypeName ? '<span class="badge bg-purple-lt me-1"><i class="bi bi-box me-1"></i>' + self.esc(item.productTypeName) + '</span>' : '') +
                (item.productSizeName ? '<span class="badge bg-teal-lt me-1"><i class="bi bi-rulers me-1"></i>' + self.esc(item.productSizeName) + '</span>' : '') +
                (item.printingMethod ? '<span class="badge bg-orange-lt me-1"><i class="bi bi-palette me-1"></i>' + self.esc(item.printingMethod) + '</span>' : '') +
                '</div>' +
                '<div class="jb-item-meta mt-1">' +
                (item.quantity ? 'Qty: ' + item.quantity : '') +
                (item.noOfPages ? ' · ' + item.noOfPages + ' pages' : '') +
                ((item.trimWidthMm && item.trimHeightMm) ? ' · ' + item.trimWidthMm + '×' + item.trimHeightMm + 'mm' : '') +
                '</div>' +
                (item.productDescription ? '<div class="jb-item-meta mt-1"><i class="bi bi-chat-text me-1"></i>' + self.esc(item.productDescription) + '</div>' : '') +
                (item.calcRefNo ? '<div class="jb-item-meta mt-1"><a href="/RateCalculator/Details?id=' + (item.rateCalculatorId || '') + '" class="badge bg-cyan-lt text-decoration-none"><i class="bi bi-link-45deg me-1"></i>' + self.esc(item.calcRefNo) + '</a></div>' : '') +
                '<div class="jb-item-meta mt-1">' +
                '<span class="badge bg-teal-lt">' + gstLabel + '</span>' +
                (item.discountPercent > 0 ? ' <span class="badge bg-orange-lt">Disc: ' + item.discountPercent + '%</span>' : '') +
                '</div>' +
                '</div>' +
                '<div class="text-end">' +
                '<div class="jb-item-price">' + self.fmt(item.netAmount) + '</div>' +
                '<div class="jb-item-meta">Unit: ' + self.fmt(item.unitRate) + '</div>' +
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

        $('#jbSubtotal').text(this.fmt(subtotal));
        $('#jbDiscount').text('-' + this.fmt(discount));
        $('#jbTaxable').text(this.fmt(taxable));
        $('#jbCgst').text(this.fmt(cgst));
        $('#jbSgst').text(this.fmt(sgst));
        $('#jbIgst').text(this.fmt(igst));
        $('#jbGrandTotal').text(this.fmt(net));
        $('#jbItemCount').text(this._items.length);
        $('#jbSummaryItemCount').text(this._items.length);
    },

    updateSaveButtonState() {
        const canSave = this._selectedCustomer && this._items.length > 0;
        $('#btnSaveJob').prop('disabled', !canSave);
    },

    // ══════════════════════════════════════════════
    //  SAVE: Rate Calc → then Job
    // ══════════════════════════════════════════════

    async saveJob() {
        if (!this._selectedCustomer) { Swal2.warning('Please select a customer.'); return; }
        if (this._items.length === 0) { Swal2.warning('Please add at least one item.'); return; }

        const btn = $('#btnSaveJob');
        btn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm me-2"></span>Saving...');
        const progress = window.SaveProgress || null;
        const useOverlay = !!progress;
        if (useOverlay) {
            progress.start({
                title: 'Saving Job',
                subtitle: 'We are persisting rate details and creating your production job.',
                steps: [
                    { title: 'Persist item rate calculations', detail: 'Saving calculator records for required items' },
                    { title: 'Calculate job totals', detail: 'Preparing gross, discount, tax, and net totals' },
                    { title: 'Create job', detail: 'Saving job header and job items' },
                    { title: 'Send customer email', detail: 'Dispatching job confirmation email' }
                ],
                message: 'Starting save process...'
            });
        } else {
            Swal2.showLoading('Saving Job...');
        }

        try {
            // Step 1: Save each item's rate calculation to hyb_job_rate_calculator
            if (useOverlay) progress.setStep(0, `Saving rate calculations for ${this._items.length} item(s)...`);
            for (let i = 0; i < this._items.length; i++) {
                const item = this._items[i];
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
                    url: JB_ENQ_API + '/saveratecalc',
                    method: 'POST',
                    contentType: 'application/json',
                    data: JSON.stringify(rateCalcPayload)
                });

                item.rateCalculatorId = rcResult.rateCalcId;
                item.calcRefNo = rcResult.calcRefNo;
                if (useOverlay) progress.setMessage(`Rate calc saved for item ${i + 1} of ${this._items.length}.`);
            }

            // Step 2: Calculate totals
            if (useOverlay) progress.setStep(1, 'Computing job totals...');
            let totalAmount = 0, discountAmount = 0, taxableAmount = 0, taxAmount = 0, netAmount = 0;
            this._items.forEach(function (item) {
                totalAmount += item.grossAmount || 0;
                discountAmount += item.discountAmount || 0;
                taxableAmount += item.taxableValue || 0;
                taxAmount += item.totalTaxAmount || 0;
                netAmount += item.netAmount || 0;
            });

            // Step 3: Save job
            const payload = {
                partyId: this._selectedCustomer.partyId,
                enquiryId: this._enquiryId,
                quotationId: this._quotationId,
                partyRefNo: $('#txtPartyRefNo').val() || null,
                deliveryDate: $('#txtDeliveryDate').val() || null,
                remarks: $('#txtRemarks').val() || null,
                grossAmount: totalAmount,
                discountAmount: discountAmount,
                taxableAmount: taxableAmount,
                taxAmount: taxAmount,
                netAmount: netAmount,
                items: this._items.map(function (item, idx) {
                    return {
                        enquiryItemId: item.enquiryItemId || null,
                        quotationItemId: item.quotationItemId || null,
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
                        printProductTypeId: item.printProductTypeId || null,
                        jobTypeId: item.jobTypeId || null,
                        taxCategoryId: item.taxCategoryId || null,
                        hsnSacCode: item.hsnSacCode || null,
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
                        hsnSacCodeId: item.hsnSacCodeId || null,
                        remarks: ''
                    };
                })
            };

            if (useOverlay) progress.setStep(2, 'Creating job record...');
            const result = await $.ajax({
                url: JB_CREATE_API + '/save',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(payload)
            });

            if (!useOverlay) {
                Swal2.hideLoading();
            }

            // Auto-send email to customer (best-effort)
            try {
                if (useOverlay) progress.setStep(3, 'Sending job email to customer...');
                await $.ajax({ url: JB_CREATE_API + '/send-email/' + result.jobId, method: 'POST' });
            } catch (_) { /* email send is best-effort */ }

            if (useOverlay) {
                progress.complete('Job created successfully.');
                setTimeout(function () { progress.close(); }, 250);
            }

            await Swal.fire({
                icon: 'success',
                title: 'Job Created & Emailed!',
                html: '<strong>' + result.jobNo + '</strong> saved and emailed to the customer.',
                confirmButtonText: 'View Job',
                confirmButtonColor: '#6f42c1',
                showCancelButton: true,
                cancelButtonText: 'Back to List'
            }).then(function (res) {
                if (res.isConfirmed) {
                    window.location.href = '/Job/Details?id=' + result.jobId;
                } else {
                    window.location.href = '/Job';
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
            btn.prop('disabled', false).html('<i class="bi bi-check-lg me-2"></i>Save Job');
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
