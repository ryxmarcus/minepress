// ===== MinePress Enquiry Module — JS =====

const ENQ_API = '/api/enquiry';
let enquiryItems = [];
let selectedCustomer = null;
let _enqCompanyStateCode = null;
let _enqCustomerStateCode = null;
let _enqGstType = 'INTRA'; // INTRA = CGST+SGST, INTER = IGST
let sgtPct = 0; // backward-compat alias guard

async function loadEnqCompanyInfo() {
    try {
        const data = await $.get(`${ENQ_API}/company-info`);
        if (data && data.stateCode) {
            _enqCompanyStateCode = data.stateCode;
        }
    } catch (err) {
        console.warn('Failed to load company info for GST:', err);
    }
}

function buildRecommendedMachinesForEnquiry(snapshot, item) {
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
}

function updateEnqGstType() {
    if (_enqCompanyStateCode && _enqCustomerStateCode) {
        _enqGstType = _enqCompanyStateCode === _enqCustomerStateCode ? 'INTRA' : 'INTER';
    } else {
        _enqGstType = 'INTRA';
    }

    const badge = $('#enqGstBadge');
    const text = $('#enqGstTypeText');
    if (_enqGstType === 'INTER') {
        badge.removeClass('d-none');
        text.text('IGST (Inter-State)');
        $('#enqGstTypeBadge').removeClass('bg-teal-lt').addClass('bg-azure-lt');
    } else {
        badge.removeClass('d-none');
        text.text('CGST + SGST (Intra-State)');
        $('#enqGstTypeBadge').removeClass('bg-azure-lt').addClass('bg-teal-lt');
    }
    if (typeof AddItemModal !== 'undefined') AddItemModal.updatePreview();
}

// ══════════════════════════════════════════════
//  INIT
// ══════════════════════════════════════════════
$(document).ready(async function () {
    await loadEnqCompanyInfo();

    CustomerSearch.init({
        apiBase: ENQ_API,
        onSelect: function (cust) {
            selectedCustomer = cust;
            if (cust.gstno && cust.gstno.length >= 2) {
                _enqCustomerStateCode = cust.gstno.substring(0, 2);
            } else {
                _enqCustomerStateCode = null;
            }
            updateEnqGstType();
            $('#txtContactPerson').val(cust.name || '');
            $('#txtContactEmail').val(cust.email || '');
            $('#txtContactMobile').val(cust.mobile || '');
            $('#enqQuickCustomer').html(`<i class="bi bi-person-fill me-1 text-primary"></i>${escapeHtml(cust.name)}`);
            updateSaveButtonState();
        },
        onClear: function () {
            selectedCustomer = null;
            _enqCustomerStateCode = null;
            updateEnqGstType();
            $('#enqQuickCustomer').html('<i class="bi bi-person me-1"></i>No customer selected');
            updateSaveButtonState();
        }
    });
    HsnSacSelector.init();
    AddItemModal.init({
        title: 'Add Item to Enquiry',
        avatarClass: 'bg-primary-lt text-primary',
        confirmStyle: 'background:var(--tblr-primary);color:#fff;',
        gstTypeGetter: function () { return _enqGstType; },
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
        onConfirm: function (data) { confirmAddItem(data); }
    });

    $('#btnSaveEnquiry').on('click', saveEnquiry);

    // Sync quick info panel
    $('#ddlPriority').on('change', function () {
        const p = $(this).find('option:selected').text() || 'Normal';
        $('#enqQuickPriority').html(`<i class="bi bi-flag-fill me-1"></i>Priority: ${escapeHtml(p)}`);
    });
    $('#txtExpectedDelivery').on('change', function () {
        const v = $(this).val();
        $('#enqQuickDelivery').html(v
            ? `<i class="bi bi-calendar-event-fill me-1 text-primary"></i>Delivery: ${v}`
            : '<i class="bi bi-calendar-event me-1"></i>Delivery: Not set');
    });

    });

// CustomerSearch is now loaded from /js/customer-search.js (shared component)
// HsnSacSelector is now loaded from /js/hsn-sac-selector.js (shared component)

// ══════════════════════════════════════════════
//  ADD ITEM
// ══════════════════════════════════════════════
function confirmAddItem(data) {
    const name        = data.name;
    const qty         = data.qty;
    const description = data.description;
    const grossAmount = data.grossAmount;
    const discPct     = data.discPct || 0;
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
    const snapshot   = collectRateCalcSnapshot();

    let cgstPct = 0, sgstPct = 0, igstPct = 0;
    if (_enqGstType === 'INTER') {
        igstPct = gstRate;
        sgtPct = 0;
    } else {
        cgstPct = gstRate / 2;
        sgstPct = gstRate / 2;
        sgtPct = sgstPct;
    }

    const item = {
        seq: enquiryItems.length + 1,
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
        netTotal: 0,
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
        configData: buildConfigDataForEnquiry(snapshot, {
            productName: name,
            quantity: qty,
            noOfPages: parseInt($('#txtTotalPages').val()) || 0,
            trimWidthMm: parseFloat($('#txtTrimWidth').val()) || 0,
            trimHeightMm: parseFloat($('#txtTrimHeight').val()) || 0
        }, calcResult, partsData),
        recommendedMachines: buildRecommendedMachinesForEnquiry(snapshot, {
            quantity: qty,
            printingMethod: $('#txtPrintingMode').val() || '',
            trimWidthMm: parseFloat($('#txtTrimWidth').val()) || 0,
            trimHeightMm: parseFloat($('#txtTrimHeight').val()) || 0
        })
    };

    item.totalTaxAmount = item.cgstAmount + item.sgstAmount + item.igstAmount;
    item.netTotal       = item.taxableValue + item.totalTaxAmount;

    enquiryItems.push(item);
    renderItemsSummary();
    updateSaveButtonState();

    AddItemModal.close();

    const drawerEl = document.getElementById('drawerRateCalc');
    if (drawerEl && window.bootstrap && bootstrap.Offcanvas) {
        const drawer = bootstrap.Offcanvas.getInstance(drawerEl) || bootstrap.Offcanvas.getOrCreateInstance(drawerEl);
        drawer.hide();
    }

    Swal2.success(`Item "${name}" added successfully!`);

    window._lastCalcResult    = null;
    window._lastCalcPartsData = null;
    $('#btnReset').trigger('click');
}

function collectRateCalcSnapshot() {
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
        plateName: $('#ddlPlate option:selected').text() || '',
        sidesText: $('#ddlSides option:selected').text() || '',
        designingNames: ($('#ddlDesigning option:selected').map(function () { return $(this).text(); }).get() || []),
        bindingNames: ($('#ddlBinding option:selected').map(function () { return $(this).text(); }).get() || []),
        finishingNames: ($('#ddlFinishings option:selected').map(function () { return $(this).text(); }).get() || []),
        partDetails: typeof collectPartDetails === 'function' ? collectPartDetails() : []
    };
}

function buildConfigDataForEnquiry(snapshot, item, calcResult, partsData) {
    const flags = (calcResult && calcResult.jobTypeFlags) || {};
    const workflowStages = [];
    if (flags.isDesignRequired || flags.isDtpRequired) workflowStages.push('Design/DTP');
    if (flags.isCtpRequired) workflowStages.push('CTP/Plates');
    if (flags.isPrintingRequired) workflowStages.push('Printing');
    if (flags.isBindingRequired) workflowStages.push('Binding');
    if (flags.isFinishingRequired) workflowStages.push('Finishing');

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
        designDtp: (flags.isDesignRequired || flags.isDtpRequired) ? { designItems: snapshot.designingNames || [] } : null,
        ctpPlates: flags.isCtpRequired ? { plateName: snapshot.plateName || '' } : null,
        printing: flags.isPrintingRequired ? { machineName: snapshot.machineName || '' } : null
    }));

    return JSON.stringify({
        jobType: { value: snapshot.jobTypeName || '', workflowStages },
        productType: snapshot.productTypeName || '',
        productSize: snapshot.productSizeName || '',
        trimWidth: item.trimWidthMm || snapshot.trimWidthMm || 0,
        trimHeight: item.trimHeightMm || snapshot.trimHeightMm || 0,
        quantity: item.quantity || snapshot.quantity || 0,
        sides: snapshot.sidesText || '',
        productParts,
        binding: flags.isBindingRequired ? { bindingTypes: snapshot.bindingNames || [] } : null,
        finishing: flags.isFinishingRequired ? { finishingTypes: snapshot.finishingNames || [] } : null
    });
}

function removeItem(idx) {
    enquiryItems.splice(idx, 1);
    enquiryItems.forEach((item, i) => item.seq = i + 1);
    renderItemsSummary();
    updateSaveButtonState();
}

function renderItemsSummary() {
    const container = $('#enquiryItemsList');
    if (enquiryItems.length === 0) {
        container.html(`<div class="empty py-4">
            <div class="empty-icon"><i class="bi bi-calculator fs-1 text-secondary opacity-50"></i></div>
            <p class="empty-title">No items added yet</p>
            <p class="empty-subtitle text-secondary">Click <strong>"Open Rate Calculator"</strong> to estimate and add items</p>
        </div>`);
        updateTotals();
        return;
    }

    let html = '';
    enquiryItems.forEach((item, idx) => {
        const gstLabel = _enqGstType === 'INTER'
            ? 'IGST ' + (item.igstPercent || 0) + '%'
            : 'GST ' + (item.gstRate || 0) + '% (CGST+SGST)';

        html += `<div class="enq-item-chip">
            <button type="button" class="btn-close" onclick="removeItem(${idx})" title="Remove"></button>
            <div class="d-flex justify-content-between align-items-start">
                <div>
                    <div class="enq-item-name">
                        <span class="badge bg-primary-lt me-1">#${item.seq}</span>
                        ${escapeHtml(item.productName)}
                    </div>
                    <div class="enq-item-meta mt-1">
                        ${item.jobTypeName ? '<span class="badge bg-blue-lt me-1"><i class="bi bi-printer me-1"></i>' + escapeHtml(item.jobTypeName) + '</span>' : ''}
                        ${item.productTypeName ? '<span class="badge bg-purple-lt me-1"><i class="bi bi-box me-1"></i>' + escapeHtml(item.productTypeName) + '</span>' : ''}
                        ${item.productSizeName ? '<span class="badge bg-teal-lt me-1"><i class="bi bi-rulers me-1"></i>' + escapeHtml(item.productSizeName) + '</span>' : ''}
                    </div>
                    <div class="enq-item-meta mt-1">
                        ${item.quantity ? 'Qty: ' + item.quantity : ''}
                        ${item.noOfPages ? ' · ' + item.noOfPages + ' pages' : ''}
                    </div>
                    ${item.productDescription ? '<div class="enq-item-meta mt-1"><i class="bi bi-chat-text me-1"></i>' + escapeHtml(item.productDescription) + '</div>' : ''}
                    ${item.calcRefNo ? '<div class="enq-item-meta mt-1"><a href="/RateCalculator/Details?id=' + (item.rateCalculatorId || '') + '" class="badge bg-cyan-lt text-decoration-none"><i class="bi bi-link-45deg me-1"></i>' + escapeHtml(item.calcRefNo) + '</a></div>' : ''}
                    <div class="enq-item-meta mt-1">
                        <span class="badge bg-teal-lt">${gstLabel}</span>
                        ${item.discountPercent > 0 ? ' <span class="badge bg-orange-lt">Disc: ' + item.discountPercent + '%</span>' : ''}
                    </div>
                </div>
                <div class="text-end">
                    <div class="enq-item-price">${fmt(item.netTotal)}</div>
                    <div class="enq-item-meta">Unit: ${fmt(item.costPerUnit)}</div>
                </div>
            </div>
        </div>`;
    });
    container.html(html);
    updateTotals();
}

function updateTotals() {
    let subtotal = 0, totalTax = 0, grandTotal = 0;
    enquiryItems.forEach(item => {
        subtotal += item.grossAmount || 0;
        totalTax += item.totalTaxAmount || 0;
        grandTotal += item.netTotal || 0;
    });
    $('#enqSubtotal').text(fmt(subtotal));
    $('#enqTotalTax').text(fmt(totalTax));
    $('#enqGrandTotal').text(fmt(grandTotal));
    $('#enqItemCount').text(enquiryItems.length);
    $('#enqSummaryItemCount').text(enquiryItems.length);
}

// ══════════════════════════════════════════════
//  SAVE: Rate Calc → then Enquiry
// ══════════════════════════════════════════════
async function saveEnquiry() {
    if (!selectedCustomer) { Swal2.warning('Please select a customer.'); return; }
    if (enquiryItems.length === 0) { Swal2.warning('Please add at least one item.'); return; }

    const btn = $('#btnSaveEnquiry');
    btn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm me-2"></span>Saving...');
    const progress = window.SaveProgress || null;
    const useOverlay = !!progress;
    if (useOverlay) {
        progress.start({
            title: 'Saving Enquiry',
            subtitle: 'We are creating linked records and preparing your enquiry.',
            steps: [
                { title: 'Persist item rate calculations', detail: 'Saving calculator snapshot for each item' },
                { title: 'Build enquiry payload', detail: 'Compiling totals and line item metadata' },
                { title: 'Finalize enquiry', detail: 'Creating enquiry and generating reference number' }
            ],
            message: 'Starting save process...'
        });
    } else {
        Swal2.showLoading('Saving Enquiry...');
    }

    try {
        // Step 1: Save each item's rate calculation to hyb_job_rate_calculator
        if (useOverlay) progress.setStep(0, `Saving rate calculations for ${enquiryItems.length} item(s)...`);
        for (let i = 0; i < enquiryItems.length; i++) {
            const item = enquiryItems[i];
            const snap = item.rateCalcSnapshot || {};

            const rateCalcPayload = {
                partyId: selectedCustomer.partyId,
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
                netTotal: item.netTotal,
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
                url: `${ENQ_API}/saveratecalc`,
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(rateCalcPayload)
            });

            // Store the saved rate calc ID on the item
            item.rateCalculatorId = rcResult.rateCalcId;
            item.calcRefNo = rcResult.calcRefNo;
            if (useOverlay) progress.setMessage(`Saved item ${i + 1} of ${enquiryItems.length}.`);
        }

        // Step 2: Save enquiry with linked rate calculator IDs
        if (useOverlay) progress.setStep(1, 'Preparing enquiry request...');
        const payload = {
            partyId: selectedCustomer.partyId,
            contactPerson: $('#txtContactPerson').val()?.trim() || '',
            contactMobile: $('#txtContactMobile').val()?.trim() || '',
            contactEmail: $('#txtContactEmail').val()?.trim() || '',
            enquirySource: $('#ddlEnquirySource').val() || '',
            expectedDeliveryDate: $('#txtExpectedDelivery').val() || null,
            priority: $('#ddlPriority').val() || 'NORMAL',
            remarks: $('#txtRemarks').val()?.trim() || '',
            items: enquiryItems.map(item => ({
                itemSequence: item.seq,
                rateCalculatorId: item.rateCalculatorId || null,
                calcRefNo: item.calcRefNo || null,
                productName: item.productName,
                productDescription: item.productDescription,
                productTypeName: item.productTypeName,
                jobTypeName: item.jobTypeName,
                productSizeName: item.productSizeName,
                quantity: item.quantity,
                noOfPages: item.noOfPages,
                trimWidthMm: item.trimWidthMm,
                trimHeightMm: item.trimHeightMm,
                printingMethod: item.printingMethod,
                specificationsJson: item.specificationsJson,
                status: 'DRAFT'
            }))
        };

        const res = await $.ajax({
            url: `${ENQ_API}/save`,
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload)
        });

        if (useOverlay) {
            progress.setStep(2, 'Finalizing enquiry...');
            progress.complete(`Enquiry ${res.enquiryNo} created successfully.`);
            setTimeout(() => progress.close(), 250);
        } else {
            Swal2.hideLoading();
        }
        await Swal2.saveSuccess('Enquiry Saved!', `Enquiry <strong>${res.enquiryNo}</strong> created successfully.`, '/Enquiry');

    } catch (err) {
        if (useOverlay) {
            progress.error('Save failed. Please review the error and retry.');
            setTimeout(() => progress.close(), 600);
        } else {
            Swal2.hideLoading();
        }
        const msg = err.responseJSON?.message || err.responseText || 'Unknown error';
        Swal2.error('Failed to save: ' + msg);
    } finally {
        btn.prop('disabled', false).html('<i class="bi bi-check-lg me-2"></i>Save Enquiry');
    }
}

function updateSaveButtonState() {
    const canSave = selectedCustomer && enquiryItems.length > 0;
    $('#btnSaveEnquiry').prop('disabled', !canSave);
}

// ══════════════════════════════════════════════
//  HELPERS
// ══════════════════════════════════════════════
function showEnqAlert(message, type) {
    const iconMap = { success: 'success', danger: 'error', warning: 'warning', info: 'info' };
    Swal2.toast(message, iconMap[type] || 'info');
}

function fmt(n) {
    if (n == null || isNaN(n)) return '₹0.00';
    return '₹' + Number(n).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

function escapeHtml(value) {
    return (value || '')
        .replaceAll('&', '&amp;')
        .replaceAll('<', '&lt;')
        .replaceAll('>', '&gt;')
        .replaceAll('"', '&quot;')
        .replaceAll("'", '&#39;');
}
