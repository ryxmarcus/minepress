// ===== MinePress Rate Calculator — Dynamic Form + AJAX =====

const API = '/api/ratecalculator';
let jobTypes = [];
let selectedJobType = null;
let appConfig = null; // costing_rules + job_type_dynamic_fields
let allMachines = [];  // cached machines for AI auto-selection
let allPlates = [];    // cached plates for AI auto-selection
let allInks = [];      // cached inks for AI auto-selection
let allProductTypes = []; // cached product types for job type filtering
let lastCalcResult = null; // cached last calculation response for send estimation
let lastAiRecommendation = null;
let smartRecommendTimer = null;
let isAutoRecommendEnabled = true;
let isPartDistributionSyncing = false;

// Job codes that are Printing-Only: no product parts, only machine is required
function isPrintOnlyCode(code) {
    const PRINT_ONLY_CODES = ['PRINT_OFFSET', 'PRINT_DIGITAL', 'PRINT_SCREEN', 'PRINT_FLEX', 'PRINT_UV'];
    return PRINT_ONLY_CODES.includes((code || '').toUpperCase());
}

// ── Bootstrap ──────────────────────────────────────────────
$(document).ready(function () {
    mountRateCalculatorOverlaysToBody();
    initSelect2();
    loadConfig();
    loadJobTypes();
    loadProductTypes();
    loadProductSizes();

    $('#ddlJobType').on('change', function () { onJobTypeChanged(); updateProgressStepper(); });
    $('#ddlProductType').on('change', function () { onProductTypeChanged(); updateProgressStepper(); });
    $('#ddlProductSize').on('change', function () { onProductSizeChanged(); updateProgressStepper(); });
    $('#ddlMachine').on('change', function () { onMachineChanged(); updateProgressStepper(); });
    $('#btnCalculate').on('click', calculate);
    $('#btnSmartRecommend').on('click', smartRecommend);
    $('#chkAutoSmartRecommend').on('change', function () {
        isAutoRecommendEnabled = $(this).is(':checked');
        if (!isAutoRecommendEnabled && smartRecommendTimer) {
            clearTimeout(smartRecommendTimer);
            smartRecommendTimer = null;
        }
        if (isAutoRecommendEnabled) {
            scheduleSmartRecommend();
        }
    });

function mountRateCalculatorOverlaysToBody() {
    const ids = [
        '#modalBreakdown',
        '#modalBom',
        '#modalRules',
        '#modalDistribution',
        '#loadingOverlay',
        '#validationAlertOverlay'
    ];

    ids.forEach(id => {
        const el = $(id);
        if (el.length > 0 && !el.parent().is('body')) {
            el.appendTo('body');
        }
    });
}
    $('#btnReset').on('click', resetForm);
    $('#btnAutoDistributeParts').on('click', autoDistributePartPages);
    $('#txtTotalPages').on('input', onTotalPagesChanged);
    $('#txtQuantity, #txtTotalPages, #txtTrimWidth, #txtTrimHeight').on('input', function () { validateForm(); updateProgressStepper(); });
    $('#txtAreaWidth, #txtAreaHeight').on('input', updateAreaCalc);
    $('#txtLabourHours, #txtLabourRate, #txtOutsourceCost').on('input', validateForm);

    // Keyboard shortcut: Ctrl+Enter to calculate
    $(document).on('keydown', function (e) {
        if ((e.ctrlKey || e.metaKey) && e.key === 'Enter') {
            e.preventDefault();
            $('#btnCalculate').trigger('click');
        }
    });

    // Send Estimation handlers
    $('.rc-send-toggle-pill').on('click', function () {
        $('.rc-send-toggle-pill').removeClass('active');
        $(this).addClass('active');
        $(this).find('input[type=radio]').prop('checked', true);
    });
    $('#btnReviewEstimation').on('click', reviewEstimation);
    $('#btnSendWhatsApp').on('click', sendViaWhatsApp);
    $('#btnSendEmail').on('click', sendViaEmail);
    $('#btnPrintEstimation').on('click', printEstimation);
    $('#btnModalSendWhatsApp').on('click', sendViaWhatsApp);
    $('#btnModalSendEmail').on('click', sendViaEmail);
    $('#btnModalPrint').on('click', printEstimation);

    $(document).on('input', '.part-pages', syncTotalPagesFromParts);
    $(document).on('change', '.part-enabled', function () {
        const row = $(this).closest('.part-row');
        const pagesInput = row.find('.part-pages');
        const colorsSelect = row.find('.part-colors');
        const paperSelect = row.find('.part-paper');

        if ($(this).is(':checked')) {
            // Restore default pages from data attribute (min 1)
            const defaultPages = parseInt(row.data('default-pages')) || 1;
            pagesInput.val(defaultPages).prop('disabled', false);
            colorsSelect.prop('disabled', false);
            if (paperSelect.length) paperSelect.prop('disabled', false);
            row.removeClass('part-disabled');
        } else {
            // Reset pages to zero and disable inputs
            pagesInput.val(0).prop('disabled', true);
            colorsSelect.prop('disabled', true);
            if (paperSelect.length) paperSelect.prop('disabled', true);
            row.addClass('part-disabled');
        }

        syncTotalPagesFromParts();
    });
    $(document).on('change', '.part-colors', function () {
        // Re-run AI auto-selection when part colors change
        const opt = $('#ddlJobType option:selected');
        const mode = opt.data('mode') || '';
        if (opt.data('printing')) {
            autoSelectMachine(mode);
            autoSelectInks();
        }
        if (opt.data('ctp')) {
            autoSelectPlate();
        }
        scheduleSmartRecommend();
    });

    // Auto Smart Recommend triggers
    $('#ddlJobType, #ddlProductType, #ddlProductSize, #ddlSides').on('change', scheduleSmartRecommend);
    $('#txtQuantity, #txtTotalPages, #txtTrimWidth, #txtTrimHeight').on('input', scheduleSmartRecommend);
    $(document).on('input', '.part-pages', scheduleSmartRecommend);
    $(document).on('change', '.part-paper', function () {
        $(this).data('user-selected', true);
    });
    $(document).on('change', '.part-enabled, .part-paper', scheduleSmartRecommend);

    // Print-only field event handlers
    $('#ddlPrintSize').on('change', function () {
        const opt = $(this).find('option:selected');
        const w = opt.data('w');
        const h = opt.data('h');
        if (w && h) {
            $('#txtTrimWidth').val(w);
            $('#txtTrimHeight').val(h);
        }
        validateForm();
        scheduleSmartRecommend();
    });

    $('#ddlPrintColors').on('change', function () {
        const jobCode = ($('#ddlJobType option:selected').data('code') || '').toUpperCase();
        if (isPrintOnlyCode(jobCode)) {
            const mode = $('#txtPrintingMode').val();
            autoSelectMachine(mode);
            autoSelectInks();
            validateForm();
            scheduleSmartRecommend();
        }
    });

    $('#ddlPrintSide').on('change', function () {
        const jobCode = ($('#ddlJobType option:selected').data('code') || '').toUpperCase();
        if (isPrintOnlyCode(jobCode)) {
            const mode = $('#txtPrintingMode').val();
            autoSelectMachine(mode);
            validateForm();
            scheduleSmartRecommend();
        }
    });

    $('#txtPlatesReceived').on('input', function () {
        validateForm();
        scheduleSmartRecommend();
    });
});

function onTotalPagesChanged() {
    if (isPartDistributionSyncing) return;
    distributeTotalPagesSmart(false);
    validateForm();
}

function scheduleSmartRecommend() {
    if (!isAutoRecommendEnabled) return;
    if (smartRecommendTimer) clearTimeout(smartRecommendTimer);
    smartRecommendTimer = setTimeout(function () {
        smartRecommend({ silent: true, auto: true });
    }, 650);
}

function smartRecommend(options) {
    const opts = options || {};
    const jobTypeId = parseInt($('#ddlJobType').val()) || 0;
    if (jobTypeId <= 0) {
        if (!opts.silent) alert('Please select Job Type before smart recommendation.');
        return;
    }

    if (opts.auto) {
        const qty = parseInt($('#txtQuantity').val()) || 0;
        const w = parseFloat($('#txtTrimWidth').val()) || 0;
        const h = parseFloat($('#txtTrimHeight').val()) || 0;
        if (qty <= 0 || w <= 0 || h <= 0) return;
    }

    if ($('#btnSmartRecommend').prop('disabled')) return;

    const request = {
        jobTypeId: jobTypeId,
        productTypeId: toIntOrNull($('#ddlProductType').val()),
        productSizeId: toIntOrNull($('#ddlProductSize').val()),
        quantity: parseInt($('#txtQuantity').val()) || 0,
        totalPages: parseInt($('#txtTotalPages').val()) || 2,
        trimWidthMm: parseFloat($('#txtTrimWidth').val()) || 0,
        trimHeightMm: parseFloat($('#txtTrimHeight').val()) || 0,
        printingMode: $('#txtPrintingMode').val(),
        printingSides: isPrintOnlyCode($('#ddlJobType option:selected').data('code') || '')
            ? (parseInt($('#ddlPrintSide').val()) || 1)
            : (parseInt($('#ddlSides').val()) || 1),
        partDetails: collectPartDetails()
    };

    const btn = $('#btnSmartRecommend');
    btn.prop('disabled', true);

    $.ajax({
        url: `${API}/ai-recommend`,
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(request),
        success: function (res) {
            applyAiRecommendation(res);
        },
        error: function (xhr) {
            if (!opts.silent) {
                alert('Smart recommendation failed: ' + (xhr.responseText || 'Unknown error'));
            }
        },
        complete: function () {
            btn.prop('disabled', false);
        }
    });
}

function applyAiRecommendation(res) {
    lastAiRecommendation = res || null;

    if (res.machineId) {
        $('#ddlMachine').val(res.machineId.toString()).trigger('change.select2');
        $('#machineAiInfo').removeClass('d-none');
    }

    if (res.plateId) {
        $('#ddlPlate').val(res.plateId.toString()).trigger('change.select2');
        $('#plateAiInfo').removeClass('d-none');
    }

    if (Array.isArray(res.inkCodes) && res.inkCodes.length > 0) {
        $('#ddlInks').val(res.inkCodes);
    }

    if (Array.isArray(res.bindingIds) && res.bindingIds.length > 0) {
        $('#ddlBinding').val(res.bindingIds.map(x => x.toString())).trigger('change.select2');
    }

    if (Array.isArray(res.finishingIds) && res.finishingIds.length > 0) {
        $('#ddlFinishings').val(res.finishingIds.map(x => x.toString())).trigger('change.select2');
    }

    if (res.globalPaperId) {
        $('#ddlPaper').val(res.globalPaperId.toString()).trigger('change.select2');
    }

    if (Array.isArray(res.partPapers) && res.partPapers.length > 0) {
        res.partPapers.forEach(p => {
            const row = $(`#partsList .part-row[data-part-id='${p.productPartId}']`);
            const dd = row.find('.part-paper');
            if (dd.length && p.paperId && !dd.data('user-selected')) {
                dd.val(p.paperId.toString()).trigger('change.select2');
            }
        });
    }

    updateAiInfoPanel();
    validateForm();
}

function loadConfig() {
    $.get(`${API}/config`, function (data) {
        appConfig = data;
    });
}

function updateAreaCalc() {
    const w = parseFloat($('#txtAreaWidth').val()) || 0;
    const h = parseFloat($('#txtAreaHeight').val()) || 0;
    $('#txtTotalArea').val(w > 0 && h > 0 ? `${(w * h).toFixed(2)} sq.ft` : '');
    validateForm();
}

function initSelect2() {
    $('.select2').each(function () {
        $(this).select2({
            theme: 'bootstrap-5',
            width: '100%',
            allowClear: true,
            placeholder: $(this).find('option:first').text() || 'Select'
        });
    });
}

// ── Data Loaders ───────────────────────────────────────────
function loadJobTypes() {
    $.get(`${API}/jobtypes`, function (data) {
        jobTypes = data;
        const dd = $('#ddlJobType');
        dd.find('option:not(:first)').remove();
        data.forEach(jt => {
            dd.append(`<option value="${jt.jobtypeid}" 
                data-code="${jt.jobtypecode}"
                data-mode="${jt.printingmode || ''}"
                data-design="${jt.isdesignrequired}"
                data-dtp="${jt.isdtprequired}"
                data-ctp="${jt.isctprequired}"
                data-printing="${jt.isprintingrequired}"
                data-binding="${jt.isbindingrequired}"
                data-finishing="${jt.isfinishingrequired}"
                data-single="${jt.issingleprocess}"
                data-full="${jt.isfullprocess}"
                data-custmat="${jt.iscustomermaterial}"
                data-inhousemat="${jt.isinhousematerial}"
                data-outsource="${jt.isoutsourcejob}"
                >${jt.jobtypename}</option>`);
        });
        dd.trigger('change.select2');
    });
}

function loadProductTypes() {
    $.get(`${API}/producttypes`, function (data) {
        allProductTypes = data;
        populateProductTypeDropdown(data);
    });
}

function populateProductTypeDropdown(data) {
    const dd = $('#ddlProductType');
    dd.find('option:not(:first)').remove();
    data.forEach(pt => {
        dd.append(`<option value="${pt.printproducttypeid}" data-name="${pt.productname}">${pt.productname} (${pt.category || ''})</option>`);
    });
    dd.trigger('change.select2');
}

// Filter product types based on selected job type code
function filterProductTypesByJobCode(jobCode) {
    const filterMap = {
        'DESIGN_ONLY': 'isdesignrequired',
        'DTP_ONLY': 'isdtprequired',
        'CTP_ONLY': 'isctprequired',
        'BINDING_ONLY': 'isbindingrequired',
        'FINISH_ONLY': 'isfinishingrequired',
        'LAMINATION': 'isfinishingrequired'
    };

    const filterField = filterMap[jobCode] || null;
    if (!filterField) {
        // No filter — show all product types
        populateProductTypeDropdown(allProductTypes);
        return;
    }

    const filtered = allProductTypes.filter(pt => pt[filterField] === true);
    populateProductTypeDropdown(filtered.length > 0 ? filtered : allProductTypes);
}

function loadProductSizes() {
    $.get(`${API}/productsizes`, function (data) {
        const dd = $('#ddlProductSize');
        dd.find('option:not(:first)').remove();
        const ddPrint = $('#ddlPrintSize');
        ddPrint.find('option:not(:first)').remove();
        data.forEach(ps => {
            const optHtml = `<option value="${ps.productsizeid}" data-w="${ps.widthmm}" data-h="${ps.heightmm}">${ps.sizename} (${ps.widthmm}×${ps.heightmm} mm)</option>`;
            dd.append(optHtml);
            ddPrint.append(optHtml);
        });
        dd.trigger('change.select2');
        ddPrint.trigger('change.select2');
    });
}

function loadDropdown(url, dd, textFn, valFn) {
    $.get(url, function (data) {
        if (dd.prop('multiple')) {
            dd.empty();
        } else {
            dd.find('option:not(:first)').remove();
        }

        data.forEach(item => {
            dd.append(`<option value="${valFn(item)}">${textFn(item)}</option>`);
        });
        dd.trigger('change.select2');
    });
}

// ── Event Handlers ─────────────────────────────────────────
function onJobTypeChanged() {
    const opt = $('#ddlJobType option:selected');
    const id = opt.val();

    if (!id) {
        selectedJobType = null;
        hideAllSections();
        $('#jobTypeFlags').addClass('d-none');
        $('#workflowBar').addClass('d-none');
        $('#dynamicFieldsInfo').addClass('d-none');
        $('#aiSelectionInfo').addClass('d-none');
        $('#aiSelectionInfoText').html('');
        lastAiRecommendation = null;
        $('#plateAiInfo').addClass('d-none');
        $('#costEstimationBlock').hide();
        $('#sendEstimationBlock').hide();
        $('#txtPrintingMode').val('');
        // Restore all product types when job type is cleared
        populateProductTypeDropdown(allProductTypes);
        return;
    }

    selectedJobType = jobTypes.find(j => j.jobtypeid == id);
    const jobCode = opt.data('code') || '';
    const mode = opt.data('mode') || '';
    $('#txtPrintingMode').val(mode || 'N/A');

    // Filter product types based on job type code
    filterProductTypesByJobCode(jobCode);

    // ── Full reset of all controls on job type change ──
    // Product details
    $('#ddlProductType').val('').trigger('change.select2');
    $('#ddlProductSize').val('').trigger('change.select2');
    $('#txtTrimWidth').val('');
    $('#txtTrimHeight').val('');
    // Quantity should remain user/default controlled (do not auto-reset)
    $('#txtTotalPages').val(0);
    $('#partsList').html('');
    $('#partsContainer').hide();
    $('#chkCustomerMaterial').prop('checked', false);

    // Designing, Binding, Finishing
    $('#ddlDesigning').val(null).trigger('change.select2');
    $('#ddlBinding').val(null).trigger('change.select2');
    $('#ddlFinishings').val(null).trigger('change.select2');

    // Auto-selected fields
    $('#ddlPlate').val('');
    $('#ddlMachine').val('');
    $('#ddlInks').val(null);
    $('#ddlPaper').val('');
    allMachines = [];
    allPlates = [];
    allInks = [];

    // Print-Only fields reset
    $('#ddlPrintSize').val('').trigger('change.select2');
    $('#ddlPrintSide').val('1');
    $('#ddlPrintColors').val('4');
    $('#txtPlatesReceived').val('0');
    $('#secPrintOnlyFields').hide();

    // Outsource / Labour / Area
    $('#txtOutsourceCost').val('');
    $('#txtVendorDesc').val('');
    $('#txtLabourType').val('');
    $('#txtLabourHours').val('');
    $('#txtLabourRate').val('');
    $('#txtAreaWidth').val('');
    $('#txtAreaHeight').val('');
    $('#txtTotalArea').val('');

    // Hide AI info and cost estimation
    $('#aiSelectionInfo').addClass('d-none');
    $('#aiSelectionInfoText').html('');
    lastAiRecommendation = null;
    $('#plateAiInfo').addClass('d-none');
    $('#machineAiInfo').addClass('d-none');
    $('#costEstimationBlock').hide();
    $('#sendEstimationBlock').hide();

    showFlagBadges(opt);

    // Get dynamic fields config for this job type
    const dynConfig = appConfig?.job_type_dynamic_fields?.[jobCode] || null;
    const category = dynConfig?.category || '';
    const isOutsource = opt.data('outsource') === true;
    const isFull = opt.data('full') === true;

    // Show/hide sections based on flags AND dynamic config
    const hasDesign = opt.data('design') === true || opt.data('dtp') === true;
    const hasCtp = opt.data('ctp') === true;
    const hasBinding = opt.data('binding') === true;
    const hasFinishing = opt.data('finishing') === true;

    toggleSection('#secDesign', hasDesign || hasCtp);
    toggleSection('#secDesignCol', hasDesign);
    toggleSection('#secPlate', hasCtp);
    toggleSection('#secPrinting', opt.data('printing') === true);
    toggleSection('#secBinding', hasBinding || hasFinishing);
    toggleSection('#secBindingCol', hasBinding);
    toggleSection('#secFinishing', hasFinishing);
    toggleSection('#secOutsource', isOutsource);
    toggleSection('#secLabour', category === 'LABOUR');
    toggleSection('#secArea', mode === 'FLEX' || (dynConfig?.rules?.area_based === true));

    // Load dropdowns for relevant sections
    if (opt.data('design') || opt.data('dtp')) {
        loadDesignings();
    }
    if (opt.data('ctp')) {
        autoSelectPlate();
    }
    if (opt.data('printing')) {
        autoSelectMachine(mode);
        autoSelectInks();
    }
    if (opt.data('binding')) {
        loadBindings();
    }
    if (opt.data('finishing')) {
        loadFinishings();
    }

    $('#chkCustomerMaterial').prop('checked', !!opt.data('custmat'));

    // Hide parts container for job types that don't need product parts
    const noPartsCategories = ['SERVICE', 'PREPRESS', 'POST', 'OUTSOURCE', 'LABOUR'];
    if (noPartsCategories.includes(category) || isPrintOnlyCode(jobCode)) {
        $('#partsContainer').hide();
        $('#partsList').html('');
    }

    // Show print-only extra parameters section
    if (isPrintOnlyCode(jobCode)) {
        $('#secPrintOnlyFields').show();
        // Re-trigger ink auto-selection with correct color count
        autoSelectInks();
    } else {
        $('#secPrintOnlyFields').hide();
    }

    // Render workflow steps
    renderWorkflow(dynConfig);

    // Show required fields info
    renderDynamicFieldsInfo(dynConfig);

    validateForm();
}

function renderWorkflow(dynConfig) {
    $('#workflowBar').addClass('d-none');
}

function renderDynamicFieldsInfo(dynConfig) {
    $('#dynamicFieldsInfo').addClass('d-none');
}

function loadDesignings() {
    const ptName = getSelectedProductTypeName();
    const url = ptName
        ? `${API}/designings?productType=${encodeURIComponent(ptName)}`
        : `${API}/designings`;
    $.get(url, function (data) {
        const dd = $('#ddlDesigning');
        dd.empty();
        data.forEach(d => {
            dd.append(`<option value="${d.designingId}">${d.designName} (${d.designCategory || ''}) — ₹${d.baseCost || 0}</option>`);
        });
        // AI: auto-select cheapest designing service
        if (data.length > 0) {
            const best = data.reduce((a, b) => (a.baseCost || 0) <= (b.baseCost || 0) ? a : b);
            dd.val([best.designingId.toString()]);
            dd.trigger('change.select2');
        }
        updateAiInfoPanel();
        validateForm();
    });
}

function loadPapers() {
    const ptName = getSelectedProductTypeName();
    const url = ptName
        ? `${API}/papers?productType=${encodeURIComponent(ptName)}`
        : `${API}/papers`;
    loadDropdown(url, $('#ddlPaper'),
        p => `${p.paperName} (${p.gsm} GSM) — ₹${p.costPerSheet || p.costPerKg || 0}`,
        p => p.paperId);
}

function loadBindings() {
    const ptName = getSelectedProductTypeName();
    const url = ptName
        ? `${API}/bindings?productType=${encodeURIComponent(ptName)}`
        : `${API}/bindings`;
    $.get(url, function (data) {
        const dd = $('#ddlBinding');
        dd.empty();
        data.forEach(b => {
            dd.append(`<option value="${b.bindingId}">${b.bindingName} (${b.bindingType || ''}) — ₹${b.costPerBook || 0}/book</option>`);
        });
        // AI: auto-select binding based on page count rules
        if (data.length > 0) {
            const totalPages = parseInt($('#txtTotalPages').val()) || 0;
            let best = null;
            if (totalPages > 48) {
                // High page count: prefer Perfect binding
                best = data.find(b => (b.bindingType || '').toLowerCase().includes('perfect'));
            }
            if (!best && totalPages <= 48 && totalPages > 0) {
                // Low page count: prefer Saddle Stitch
                best = data.find(b => (b.bindingType || '').toLowerCase().includes('saddle'));
            }
            if (!best) {
                // Fallback: cheapest binding
                best = data.reduce((a, b) => (a.costPerBook || 0) <= (b.costPerBook || 0) ? a : b);
            }
            dd.val([best.bindingId.toString()]);
            dd.trigger('change.select2');
        }
        updateAiInfoPanel();
        validateForm();
    });
}

function loadFinishings() {
    const ptName = getSelectedProductTypeName();
    const url = ptName
        ? `${API}/finishings?productType=${encodeURIComponent(ptName)}`
        : `${API}/finishings`;
    $.get(url, function (data) {
        const dd = $('#ddlFinishings');
        dd.empty();
        data.forEach(f => {
            dd.append(`<option value="${f.finishingId}">${f.finishingName} (${f.finishingCategory || ''})</option>`);
        });
        // AI: auto-select first finishing option (most common)
        if (data.length > 0) {
            dd.val([data[0].finishingId.toString()]);
            dd.trigger('change.select2');
        }
        updateAiInfoPanel();
        validateForm();
    });
}

function getSelectedProductTypeName() {
    return $('#ddlProductType option:selected').data('name') || '';
}

function loadPartPapers() {
    const ptName = getSelectedProductTypeName();
    const url = ptName
        ? `${API}/papers?productType=${encodeURIComponent(ptName)}`
        : `${API}/papers`;
    $.get(url, function (papers) {
        // AI: also set global paper dropdown as fallback (cheapest non-cover paper)
        const ddGlobal = $('#ddlPaper');
        ddGlobal.find('option:not(:first)').remove();
        const nonCoverPapers = papers.filter(p => (p.supportedUsage || '').toLowerCase() !== 'cover');
        nonCoverPapers.forEach(p => {
            ddGlobal.append(`<option value="${p.paperId}">${p.paperName} (${p.gsm} GSM)</option>`);
        });
        if (nonCoverPapers.length > 0) {
            const bestGlobal = nonCoverPapers.reduce((a, b) =>
                (a.costPerSheet || a.costPerKg || Infinity) <= (b.costPerSheet || b.costPerKg || Infinity) ? a : b
            );
            ddGlobal.val(bestGlobal.paperId);
        }

        // Separate papers into cover and inner groups for dropdown
        const coverPapers = papers.filter(p => (p.supportedUsage || '').toLowerCase() === 'cover');
        const innerPapers = papers.filter(p => (p.supportedUsage || '').toLowerCase() !== 'cover');

        // Per-part paper: populate ALL papers grouped by type, AI auto-select best matching paper
        $('#partsList .part-paper').each(function () {
            const dd = $(this);
            const row = dd.closest('.part-row');
            const partName = (row.data('part-name') || '').toString().toLowerCase();
            const partCode = (row.data('part-code') || '').toString().toLowerCase();
            const isCover = partName.includes('cover') || partCode.includes('cover');

            // Destroy existing Select2 before modifying options
            if (dd.data('select2')) {
                dd.select2('destroy');
            }

            // Clear and rebuild with grouped options - show ALL papers
            dd.empty();
            dd.append('<option value="">-- Select Paper --</option>');

            // Add recommended group first (based on part type)
            const recommendedPapers = isCover ? coverPapers : innerPapers;
            const otherPapers = isCover ? innerPapers : coverPapers;
            const recommendedLabel = isCover ? '★ Recommended (Cover)' : '★ Recommended (Inner)';
            const otherLabel = isCover ? 'Other (Inner Papers)' : 'Other (Cover Papers)';

            if (recommendedPapers.length > 0) {
                const optGroup1 = $('<optgroup>').attr('label', recommendedLabel);
                recommendedPapers.forEach(p => {
                    optGroup1.append(`<option value="${p.paperId}" data-recommended="true">${p.paperName} (${p.gsm} GSM)</option>`);
                });
                dd.append(optGroup1);
            }

            if (otherPapers.length > 0) {
                const optGroup2 = $('<optgroup>').attr('label', otherLabel);
                otherPapers.forEach(p => {
                    optGroup2.append(`<option value="${p.paperId}">${p.paperName} (${p.gsm} GSM)</option>`);
                });
                dd.append(optGroup2);
            }

            // AI: auto-select cheapest paper from recommended group
            if (recommendedPapers.length > 0) {
                const best = recommendedPapers.reduce((a, b) =>
                    (a.costPerSheet || a.costPerKg || Infinity) <= (b.costPerSheet || b.costPerKg || Infinity) ? a : b
                );
                dd.val(best.paperId);
            }

            // Initialize Select2 with custom template for recommended indicator
            dd.select2({
                theme: 'bootstrap-5',
                width: '100%',
                allowClear: true,
                placeholder: '-- Select Paper --',
                templateResult: formatPaperOption,
                templateSelection: formatPaperSelection
            });
            dd.trigger('change.select2');
        });

        updateAiInfoPanel();
    });
}

// Format paper option in dropdown with recommended indicator
function formatPaperOption(option) {
    if (!option.id) return option.text;
    const isRecommended = $(option.element).data('recommended');
    if (isRecommended) {
        return $('<span><i class="bi bi-star-fill text-warning me-1" style="font-size:0.75rem;"></i>' + option.text + '</span>');
    }
    return option.text;
}

// Format selected paper display
function formatPaperSelection(option) {
    if (!option.id) return option.text;
    const isRecommended = $(option.element).data('recommended');
    if (isRecommended) {
        return $('<span><i class="bi bi-star-fill text-warning me-1" style="font-size:0.7rem;"></i>' + option.text + '</span>');
    }
    return option.text;
}

function onProductTypeChanged() {
    const ptId = $('#ddlProductType').val();

    // Reset trim dimensions and quantity on product type change
    $('#txtTrimWidth').val('');
    $('#txtTrimHeight').val('');
    // Quantity should remain user/default controlled (do not auto-reset)
    $('#txtTotalPages').val(0);
    $('#ddlProductSize').val('').trigger('change.select2');

    // Reload dropdowns filtered by product type (if their sections are visible)
    const opt = $('#ddlJobType option:selected');
    if (opt.data('design') === true || opt.data('dtp') === true) {
        loadDesignings();
    }
    if (opt.data('printing') === true) {
        // Paper is loaded per-part in loadPartPapers()
    }
    if (opt.data('binding') === true) {
        loadBindings();
    }
    if (opt.data('finishing') === true) {
        loadFinishings();
    }

    if (!ptId) {
        $('#partsContainer').hide();
        $('#partsList').html('');
        return;
    }

    // Skip loading parts for job types that don't need them
    const jobCode = $('#ddlJobType option:selected').data('code') || '';
    const dynConfig = appConfig?.job_type_dynamic_fields?.[jobCode] || null;
    const jobCategory = dynConfig?.category || '';
    const noPartsCategories = ['SERVICE', 'PREPRESS', 'POST', 'OUTSOURCE', 'LABOUR'];
    if (noPartsCategories.includes(jobCategory) || isPrintOnlyCode(jobCode)) {
        $('#partsContainer').hide();
        $('#partsList').html('');
        return;
    }

    $.get(`${API}/productparts/${ptId}`, function (data) {
        if (data.length > 0) {
            const anyNeedsPaper = data.some(p => p.requirespaper === true);
            const paperColClass = anyNeedsPaper ? ' has-paper' : '';
            let html = `<div class="parts-header d-none d-md-flex${paperColClass}">
                <div class="parts-hcol parts-hcol-toggle"></div>
                <div class="parts-hcol parts-hcol-name">Part</div>
                <div class="parts-hcol parts-hcol-pages">Pages</div>
                <div class="parts-hcol parts-hcol-colors">Colors</div>
                ${anyNeedsPaper ? '<div class="parts-hcol parts-hcol-paper">Paper</div>' : ''}
            </div>`;

            data.forEach((p, idx) => {
                const parsedDefaultPages = parseInt(p.defaultpages);
                const defaultPages = Number.isFinite(parsedDefaultPages) && parsedDefaultPages > 0 ? parsedDefaultPages : 0;
                // Always enable all parts by default - let user decide to uncheck
                const isInitiallyEnabled = true;
                const partIcon = (p.partname || '').toLowerCase().includes('cover') ? 'bi-journal-richtext' : 'bi-file-earmark-text';
                html += `<div class="part-card part-row${paperColClass}" data-part-id="${p.productpartid}" data-part-name="${escapeHtml(p.partname || '')}" data-part-code="${escapeHtml(p.partcode || '')}" data-requires-paper="${!!p.requirespaper}" data-default-pages="${defaultPages}">
                    <div class="part-card-inner">
                        <div class="part-col part-col-toggle">
                            <label class="form-check form-switch mb-0">
                                <input class="form-check-input part-enabled" type="checkbox" ${isInitiallyEnabled ? 'checked' : ''} id="part_${p.productpartid}">
                            </label>
                        </div>
                        <div class="part-col part-col-name">
                            <label class="part-name-label" for="part_${p.productpartid}">
                                <i class="bi ${partIcon} part-icon"></i>
                                <span>${p.partname}</span>
                            </label>
                        </div>
                        <div class="part-col part-col-pages">
                            <span class="part-field-label d-md-none">Pages</span>
                            <input type="number" class="form-control form-control-sm part-pages" min="1" step="1" value="${defaultPages || 1}" ${isInitiallyEnabled ? '' : 'disabled'}>
                        </div>
                        <div class="part-col part-col-colors">
                            <span class="part-field-label d-md-none">Colors</span>
                            <select class="form-select form-select-sm part-colors" ${isInitiallyEnabled ? '' : 'disabled'}>
                                <option value="1">1C</option>
                                <option value="2">2C</option>
                                <option value="4" ${idx === 0 ? 'selected' : ''}>4C</option>
                                <option value="5">4+1</option>
                                <option value="6">4+2</option>
                            </select>
                        </div>
                        ${anyNeedsPaper ? `<div class="part-col part-col-paper">
                            <span class="part-field-label d-md-none">Paper</span>
                            ${p.requirespaper ? `<select class="form-select form-select-sm part-paper" ${isInitiallyEnabled ? '' : 'disabled'}><option value="">-- Paper --</option></select>` : '<span class="text-muted small fst-italic">N/A</span>'}
                        </div>` : ''}
                    </div>
                </div>`;
            });

            html += ``;
            $('#partsList').html(html);
            $('#partsContainer').show();
            if (anyNeedsPaper) loadPartPapers();
            $('#partsList .part-row').each(function () {
                const row = $(this);
                if (!row.find('.part-enabled').is(':checked')) {
                    row.addClass('part-disabled');
                }
            });
            // Sync total pages from actual part defaults (AI: uses DB defaultpages)
            syncTotalPagesFromParts();
        } else {
            $('#partsContainer').hide();
            $('#partsList').html('');
        }
    });
}

function onProductSizeChanged() {
    const opt = $('#ddlProductSize option:selected');
    const w = opt.data('w');
    const h = opt.data('h');
    if (w && h) {
        $('#txtTrimWidth').val(w);
        $('#txtTrimHeight').val(h);
    }
}

// ── Helpers ────────────────────────────────────────────────
function showFlagBadges(opt) {
    const flags = [
        { key: 'design', label: 'Design', icon: 'bi-palette' },
        { key: 'dtp', label: 'DTP', icon: 'bi-file-earmark-text' },
        { key: 'ctp', label: 'CTP/Plates', icon: 'bi-grid-3x3-gap' },
        { key: 'printing', label: 'Printing', icon: 'bi-printer' },
        { key: 'binding', label: 'Binding', icon: 'bi-book' },
        { key: 'finishing', label: 'Finishing', icon: 'bi-stars' }
    ];

    let html = '';
    flags.forEach(f => {
        const on = opt.data(f.key) === true;
        html += `<span class="flag-badge ${on ? 'flag-on' : 'flag-off'}">
            <i class="bi ${f.icon} me-1"></i>${f.label}
        </span>`;
    });
    $('#flagBadges').html(html);
    $('#jobTypeFlags').removeClass('d-none');
}

function toggleSection(selector, show) {
    if (show) {
        $(selector).slideDown(300, function () {
            $(this).addClass('rc-section-visible');
        });
    } else {
        $(selector).removeClass('rc-section-visible').slideUp(200);
    }
}

function hideAllSections() {
    $('#secDesign, #secDesignCol, #secPlate, #secPrinting, #secBinding, #secBindingCol, #secFinishing, #secOutsource, #secLabour, #secArea').hide();
    $('#secPrintOnlyFields').hide();
}

function mapModeToCategory(mode) {
    if (!mode) return '';
    const map = {
        'OFFSET': 'OFFSET',
        'DIGITAL': 'DIGITAL',
        'SCREEN': 'SCREEN',
        'FLEX': 'FLEX',
        'UV': 'OFFSET'
    };
    return map[mode.toUpperCase()] || '';
}

function autoDistributePartPages() {
    distributeTotalPagesSmart(true);
}

function distributeTotalPagesSmart(isManual) {
    const activeRows = $('#partsList .part-row').filter(function () {
        return $(this).find('.part-enabled').is(':checked');
    });

    if (activeRows.length === 0) return;

    let totalPages = parseInt($('#txtTotalPages').val()) || 0;
    if (totalPages <= 0) return;

    // Allow any total pages - user decides (no forced even values)

    const rows = activeRows.get();
    const minEach = 1; // Minimum 1 page per part
    const minimumRequired = rows.length * minEach;
    if (totalPages < minimumRequired) {
        totalPages = minimumRequired;
        $('#txtTotalPages').val(totalPages);
    }

    const currentWeights = rows.map(r => {
        const current = parseInt($(r).find('.part-pages').val()) || 0;
        const fallback = parseInt($(r).data('default-pages')) || minEach;
        return Math.max(minEach, current > 0 ? current : fallback);
    });

    const weightSum = currentWeights.reduce((s, v) => s + v, 0) || rows.length * minEach;
    const assigned = new Array(rows.length).fill(minEach);
    let remaining = totalPages - (rows.length * minEach);

    if (remaining > 0) {
        // Distribute remaining pages proportionally based on weights
        const fractional = [];
        let allocatedPages = 0;

        for (let i = 0; i < rows.length; i++) {
            const exact = (currentWeights[i] / weightSum) * remaining;
            const whole = Math.floor(exact);
            assigned[i] += whole;
            allocatedPages += whole;
            fractional.push({ i, frac: exact - whole });
        }

        // Distribute leftover pages one by one to parts with highest fractional portions
        let leftPages = remaining - allocatedPages;
        fractional.sort((a, b) => b.frac - a.frac);
        let ptr = 0;
        while (leftPages > 0) {
            const idx = fractional[ptr % fractional.length].i;
            assigned[idx] += 1;
            leftPages--;
            ptr++;
        }
    }

    isPartDistributionSyncing = true;
    rows.forEach((row, idx) => {
        $(row).find('.part-pages').val(assigned[idx]);
    });
    isPartDistributionSyncing = false;

    if (isManual) {
        lastAiRecommendation = {
            ...(lastAiRecommendation || {}),
            insights: [
                `Smart page distribution applied across ${rows.length} part(s) for total ${totalPages} pages.`,
                'Pages distributed proportionally based on existing/default pages.'
            ],
            warnings: []
        };
        updateAiInfoPanel();
    }
}

function syncTotalPagesFromParts() {
    if (isPartDistributionSyncing) return;
    let sum = 0;
    $('#partsList .part-row').each(function () {
        const row = $(this);
        if (row.find('.part-enabled').is(':checked')) {
            let pages = parseInt(row.find('.part-pages').val()) || 0;
            // Allow any page number - user decides, no forced even values
            if (pages > 0) {
                sum += pages;
            }
        }
    });
    $('#txtTotalPages').val(sum > 0 ? sum : 0);
    validateForm();
}

function collectPartDetails() {
    return $('#partsList .part-row').map(function () {
        const row = $(this);
        if (!row.find('.part-enabled').is(':checked')) {
            return null;
        }

        const noOfPages = parseInt(row.find('.part-pages').val()) || 0;
        const partPaper = row.find('.part-paper');
        const partPaperId = partPaper.length ? toLongOrNull(partPaper.val()) : null;
        const globalPaperId = toLongOrNull($('#ddlPaper').val());
        const resolvedPaperId = partPaper.length ? partPaperId : globalPaperId;
        const resolvedPaperName = resolvedPaperId
            ? ((partPaper.length
                ? partPaper.find('option:selected').text()
                : $('#ddlPaper option:selected').text()) || '').trim()
            : null;

        return {
            productPartId: parseInt(row.data('part-id')) || 0,
            partName: row.data('part-name') || '',
            noOfPages: noOfPages,
            colors: parseInt(row.find('.part-colors').val()) || 4,
            paperId: resolvedPaperId,
            paperName: resolvedPaperName
        };
    }).get().filter(p => p && p.noOfPages > 0);
}

function validateForm() {
    validateAllFields(false);
}

// Build validation rules based on current job type flags and visible sections
function getValidationRules() {
    const rules = [];
    const opt = $('#ddlJobType option:selected');
    const jobCode = opt.data('code') || '';
    const dynConfig = appConfig?.job_type_dynamic_fields?.[jobCode] || null;
    const category = dynConfig?.category || '';
    const isPrintingRequired = opt.data('printing') === true;

    // Always required: Job Type
    rules.push({ id: 'ddlJobType', label: 'Job Type', type: 'select' });

    // Always required: Quantity
    rules.push({ id: 'txtQuantity', label: 'Quantity', type: 'number', min: 1 });

    // Product type required for FULL and PRINT_ONLY categories
    const productCategories = ['FULL', 'PRINT_ONLY'];
    if (productCategories.includes(category) && !isPrintOnlyCode(jobCode)) {
        rules.push({ id: 'ddlProductType', label: 'Product Type', type: 'select' });
    }

    // Trim dimensions required when printing section is visible
    if (opt.data('printing') === true) {
        rules.push({ id: 'txtTrimWidth', label: 'Trim Width', type: 'number', min: 1 });
        rules.push({ id: 'txtTrimHeight', label: 'Trim Height', type: 'number', min: 1 });
    }

    const partsRows = $('#partsList .part-row');
    const hasPartsUi = $('#partsContainer').is(':visible') && partsRows.length > 0;
    const activePartsCount = hasPartsUi
        ? partsRows.filter(function () { return $(this).find('.part-enabled').is(':checked'); }).length
        : 0;

    // Parts validations only when at least one part is active.
    if (hasPartsUi && activePartsCount > 0) {
        rules.push({ id: '_parts', label: 'Product Parts (at least one part with pages > 0)', type: 'parts' });
        rules.push({ id: '_partPageEven', label: 'Each active part must have even pages (minimum 2)', type: 'partPageEven' });
        rules.push({ id: '_pagesConsistency', label: 'Total pages must match sum of active part pages', type: 'pagesConsistency' });
    } else if (isPrintingRequired && !isPrintOnlyCode(jobCode)) {
        // Require Total Pages for printing jobs (not applicable for Print-Only job types)
        rules.push({ id: 'txtTotalPages', label: 'Total Pages', type: 'number', min: 2, even: true });
    }

    // Paper required per part when parts have paper dropdowns
    if (hasPartsUi && activePartsCount > 0) {
        rules.push({ id: '_partPapers', label: 'Paper for each active part', type: 'partPapers' });
    }

    // Plate: not mandatory — AI auto-selected value is used if not manually changed

    // Machine: required for Print-Only job types; otherwise AI auto-selected (not mandatory)
    if (isPrintOnlyCode(jobCode) && $('#secPrinting').is(':visible')) {
        rules.push({ id: 'ddlMachine', label: 'Machine', type: 'select' });
        rules.push({ id: 'ddlPrintSize', label: 'Print Size', type: 'select' });
        rules.push({ id: 'ddlPrintColors', label: 'Number of Colors', type: 'select' });
    }

    // Design section: at least one designing service (skip if no options available)
    if ($('#secDesign').is(':visible') && $('#ddlDesigning option').length > 0) {
        rules.push({ id: 'ddlDesigning', label: 'Designing Service', type: 'multiselect' });
    }

    // Binding section: at least one binding type (skip if no options available)
    if ($('#secBinding').is(':visible') && $('#ddlBinding option').length > 0) {
        rules.push({ id: 'ddlBinding', label: 'Binding Type', type: 'multiselect' });
    }

    // Finishing: not mandatory

    // Outsource section
    if ($('#secOutsource').is(':visible')) {
        rules.push({ id: 'txtOutsourceCost', label: 'Outsource Cost', type: 'number', min: 1 });
    }

    // Labour section
    if ($('#secLabour').is(':visible')) {
        rules.push({ id: 'txtLabourHours', label: 'Labour Hours', type: 'number', min: 0.5 });
        rules.push({ id: 'txtLabourRate', label: 'Labour Rate', type: 'number', min: 1 });
    }

    // Area section (Flex / Large Format)
    if ($('#secArea').is(':visible')) {
        rules.push({ id: 'txtAreaWidth', label: 'Area Width', type: 'number', min: 0.1 });
        rules.push({ id: 'txtAreaHeight', label: 'Area Height', type: 'number', min: 0.1 });
    }

    return rules;
}

// Validate all fields, return array of error messages. If showVisual=true, highlight invalid fields.
function validateAllFields(showVisual) {
    const rules = getValidationRules();
    const errors = [];

    // Clear previous visual errors
    if (showVisual) {
        $('.is-invalid').removeClass('is-invalid');
        $('.validation-error-text').remove();
    }

    rules.forEach(rule => {
        let isValid = true;

        if (rule.type === 'parts') {
            // Check at least one enabled part has pages > 0
            const activeParts = $('#partsList .part-row').filter(function () {
                return $(this).find('.part-enabled').is(':checked') && (parseInt($(this).find('.part-pages').val()) || 0) > 0;
            });
            isValid = activeParts.length > 0;
            if (!isValid && showVisual) {
                $('#partsList').addClass('is-invalid');
            }
        } else if (rule.type === 'partPapers') {
            // Check each active part that has a paper dropdown has a paper selected
            let allPapersSelected = true;
            $('#partsList .part-row').each(function () {
                const row = $(this);
                if (row.find('.part-enabled').is(':checked')) {
                    const paperDD = row.find('.part-paper');
                    if (paperDD.length && !paperDD.val()) {
                        allPapersSelected = false;
                        if (showVisual) paperDD.addClass('is-invalid');
                    }
                }
            });
            isValid = allPapersSelected;
        } else if (rule.type === 'partPageEven') {
            isValid = true;
            $('#partsList .part-row').each(function () {
                const row = $(this);
                if (!row.find('.part-enabled').is(':checked')) return;
                const val = parseInt(row.find('.part-pages').val()) || 0;
                if (val < 2 || val % 2 !== 0) {
                    isValid = false;
                    if (showVisual) row.find('.part-pages').addClass('is-invalid');
                }
            });
        } else if (rule.type === 'pagesConsistency') {
            let partSum = 0;
            $('#partsList .part-row').each(function () {
                const row = $(this);
                if (row.find('.part-enabled').is(':checked')) {
                    partSum += parseInt(row.find('.part-pages').val()) || 0;
                }
            });
            const totalPages = parseInt($('#txtTotalPages').val()) || 0;
            isValid = partSum === totalPages && totalPages > 0;
            if (!isValid && showVisual) {
                markInvalid('txtTotalPages');
                $('#partsList').addClass('is-invalid');
            }
        } else if (rule.type === 'select') {
            const val = $(`#${rule.id}`).val();
            isValid = !!val;
            if (!isValid && showVisual) {
                markInvalid(rule.id);
            }
        } else if (rule.type === 'multiselect') {
            const val = $(`#${rule.id}`).val();
            isValid = val && val.length > 0;
            if (!isValid && showVisual) {
                markInvalid(rule.id);
            }
        } else if (rule.type === 'number') {
            const val = parseFloat($(`#${rule.id}`).val());
            isValid = !isNaN(val) && val >= (rule.min || 0);
            if (isValid && rule.even === true) {
                isValid = Number.isInteger(val) && val % 2 === 0;
            }
            if (!isValid && showVisual) {
                markInvalid(rule.id);
            }
        }

        if (!isValid) {
            errors.push(rule.label);
        }
    });

    return errors;
}

// Mark a field as invalid with red border
function markInvalid(fieldId) {
    const el = $(`#${fieldId}`);
    // For Select2 dropdowns, mark the Select2 container
    if (el.hasClass('select2') || el.data('select2')) {
        el.next('.select2-container').find('.select2-selection').addClass('is-invalid');
    } else {
        el.addClass('is-invalid');
    }
}

// ── AI Auto-Selection Logic ────────────────────────────────

// Auto-select best plate: picks first available plate (cheapest)
function autoSelectPlate() {
    $.get(`${API}/plates`, function (data) {
        allPlates = data;
        const dd = $('#ddlPlate');
        dd.empty().append('<option value=""></option>');
        data.forEach(p => {
            dd.append(`<option value="${p.plateId}">${p.plateName}</option>`);
        });
        // AI: pick cheapest plate
        if (data.length > 0) {
            const best = data.reduce((a, b) =>
                ((a.plateCost || 0) + (a.processingCost || 0)) <= ((b.plateCost || 0) + (b.processingCost || 0)) ? a : b
            );
            dd.val(best.plateId);
            dd.trigger('change.select2');
            $('#plateAiInfo').removeClass('d-none');
            updateAiInfoPanel();
        }
    });
}

// Auto-select best machine based on printing mode, sheet size, color capability, and cost
function autoSelectMachine(mode) {
    const category = mapModeToCategory(mode);
    $.get(`${API}/machines?category=${encodeURIComponent(category)}`, function (data) {
        allMachines = data;
        const dd = $('#ddlMachine');
        dd.empty().append('<option value=""></option>');
        data.forEach(m => {
            dd.append(`<option value="${m.machineId}">${m.machineName}</option>`);
        });

        if (data.length === 0) { updateAiInfoPanel(); return; }

        const trimW = parseFloat($('#txtTrimWidth').val()) || 210;
        const trimH = parseFloat($('#txtTrimHeight').val()) || 297;
        const maxColors = getMaxColorsFromParts();

        // AI scoring: higher is better
        const scored = data.map(m => {
            let score = 0;
            const mw = m.maxSheetWidthMm || 0;
            const ml = m.maxSheetLengthMm || 0;

            // Must fit sheet size — strong penalty if too small
            const fits = (mw >= trimW && ml >= trimH) || (ml >= trimW && mw >= trimH);
            if (fits) score += 50;

            // Color capability — must support required colors
            if ((m.maxColors || 0) >= maxColors) score += 30;

            // Prefer machines that match closely (not oversized)
            if (fits && mw > 0 && ml > 0) {
                const sheetArea = mw * ml;
                const trimArea = trimW * trimH;
                const utilization = trimArea / sheetArea;
                score += Math.round(utilization * 20); // up to 20 pts for good utilization
            }

            // Lower cost is better — normalize (cheaper = higher score)
            const maxCost = Math.max(...data.map(x => x.hourlyRunningCost || 1));
            const costRatio = 1 - ((m.hourlyRunningCost || 0) / (maxCost || 1));
            score += Math.round(costRatio * 15);

            // Prefer double-side capable if printing both sides
            const sides = isPrintOnlyCode($('#ddlJobType option:selected').data('code') || '')
                ? (parseInt($('#ddlPrintSide').val()) || 1)
                : (parseInt($('#ddlSides').val()) || 1);
            if (sides === 2 && (m.printingSide || '').toString().includes('2')) score += 10;

            return { machine: m, score };
        });

        scored.sort((a, b) => b.score - a.score);
        const best = scored[0].machine;
        dd.val(best.machineId);
        dd.trigger('change.select2');
        $('#machineAiInfo').removeClass('d-none');
        updateAiInfoPanel();
    });
}

// Handle manual machine change — hides AI badge
function onMachineChanged() {
    // If user manually changes machine, hide the AI auto-selected badge
    $('#machineAiInfo').addClass('d-none');
    updateAiInfoPanel();
    validateForm();
}

// Auto-select inks based on printing mode and max colors from product parts
function autoSelectInks() {
    const mode = ($('#txtPrintingMode').val() || '').toUpperCase();
    const modePrefixMap = {
        'OFFSET': 'INK_OFF_',
        'DIGITAL': 'INK_DIG_',
        'SCREEN': 'INK_SCR_',
        'FLEX': 'INK_FLEXO_',
        'UV': 'INK_UV_'
    };
    const modePrefix = modePrefixMap[mode] || '';

    $.get(`${API}/inks`, function (data) {
        allInks = data;
        const dd = $('#ddlInks');
        dd.empty();
        data.forEach(i => {
            dd.append(`<option value="${i.inkCode}">${i.inkName}</option>`);
        });

        const maxColors = getMaxColorsFromParts();

        // Filter inks by mode prefix (if available), else use all
        let modeInks = modePrefix
            ? data.filter(i => (i.inkCode || '').toUpperCase().startsWith(modePrefix))
            : data;

        // If no mode-specific inks found, fall back to all inks
        if (modeInks.length === 0) modeInks = data;

        // CMYK color groups — pick ONE ink per color group
        const cmykGroups = [
            { name: 'cyan', match: ['cyan'] },
            { name: 'magenta', match: ['magenta'] },
            { name: 'yellow', match: ['yellow'] },
            { name: 'black', match: ['black', 'key'] }
        ];

        const selected = [];
        const usedInks = new Set();

        // For each CMYK color, find the first matching ink from mode-filtered list
        const colorsNeeded = Math.min(maxColors, 4);
        const colorOrder = colorsNeeded === 1
            ? [cmykGroups[3]] // 1C = Black only
            : cmykGroups.slice(0, colorsNeeded);

        colorOrder.forEach(group => {
            const ink = modeInks.find(i => {
                const colorLower = (i.colorName || '').toLowerCase();
                return group.match.some(m => colorLower.includes(m)) && !usedInks.has(i.inkCode);
            });
            if (ink) {
                selected.push(ink.inkCode);
                usedInks.add(ink.inkCode);
            }
        });

        // If more than 4 colors requested (spot colors), add remaining non-CMYK inks
        if (maxColors > 4) {
            const spotsNeeded = maxColors - selected.length;
            const spotInks = modeInks.filter(i => !usedInks.has(i.inkCode));
            spotInks.slice(0, spotsNeeded).forEach(i => {
                selected.push(i.inkCode);
            });
        }

        dd.val(selected);
        updateAiInfoPanel();
    });
}

// Get max color count from product parts (or dedicated print-only dropdown)
function getMaxColorsFromParts() {
    const jobCode = ($('#ddlJobType option:selected').data('code') || '').toUpperCase();
    if (isPrintOnlyCode(jobCode)) {
        return parseInt($('#ddlPrintColors').val()) || 4;
    }
    let maxColors = 4;
    $('#partsList .part-row').each(function () {
        if ($(this).find('.part-enabled').is(':checked')) {
            const c = parseInt($(this).find('.part-colors').val()) || 0;
            if (c > maxColors) maxColors = c;
        }
    });
    return maxColors;
}

// Update the AI selection info panel
function updateAiInfoPanel() {
    const insights = Array.isArray(lastAiRecommendation?.insights) ? lastAiRecommendation.insights : [];
    const warnings = Array.isArray(lastAiRecommendation?.warnings) ? lastAiRecommendation.warnings : [];

    if (insights.length === 0 && warnings.length === 0) {
        $('#aiSelectionInfo').addClass('d-none');
        $('#aiSelectionInfoText').html('');
        return;
    }

    let html = '';
    if (insights.length > 0) {
        html += `<div class="fw-semibold mb-1">AI Recommendations Applied</div><ul class="mb-1 ps-3">${insights.map(i => `<li>${escapeHtml(i)}</li>`).join('')}</ul>`;
    }
    if (warnings.length > 0) {
        html += `<div class="text-warning-emphasis"><strong>Warnings:</strong><ul class="mb-0 ps-3">${warnings.map(w => `<li>${escapeHtml(w)}</li>`).join('')}</ul></div>`;
    }

    $('#aiSelectionInfoText').html(html);
    $('#aiSelectionInfo').removeClass('d-none');
}

// ── Progress Stepper ───────────────────────────────────────
function updateProgressStepper() {
    const steps = $('.rc-progress-step');
    const lines = $('.rc-progress-line');

    // Step 1: Job Config — Job Type selected
    const step1Done = !!$('#ddlJobType').val();

    // Step 2: Product — Quantity entered
    const step2Done = step1Done && (parseInt($('#txtQuantity').val()) || 0) > 0;

    // Step 3: Design — design/plate section visible and has selection, OR section not required
    const designVisible = $('#secDesign').is(':visible');
    const step3Done = step2Done && (!designVisible || ($('#ddlDesigning').val() && $('#ddlDesigning').val().length > 0) || ($('#ddlPlate').val()));

    // Step 4: Print — machine selected or printing not required
    const printVisible = $('#secPrinting').is(':visible');
    const step4Done = step3Done && (!printVisible || !!$('#ddlMachine').val());

    // Step 5: Calculate — results visible
    const step5Done = $('#costEstimationBlock').is(':visible');

    const states = [step1Done, step2Done, step3Done, step4Done, step5Done];
    let activeIdx = 0;
    for (let i = 0; i < states.length; i++) {
        if (states[i]) activeIdx = i + 1;
    }

    steps.each(function (i) {
        const $s = $(this);
        $s.removeClass('active completed');
        if (i < activeIdx) {
            $s.addClass('completed');
        } else if (i === activeIdx) {
            $s.addClass('active');
        }
    });

    lines.each(function (i) {
        $(this).toggleClass('filled', i < activeIdx);
    });
}

// ── Animated Count-Up ──────────────────────────────────────
function animateCountUp(el, targetVal, duration) {
    const $el = $(el);
    const startVal = 0;
    const startTime = performance.now();

    function update(now) {
        const elapsed = now - startTime;
        const progress = Math.min(elapsed / duration, 1);
        // Ease out cubic
        const eased = 1 - Math.pow(1 - progress, 3);
        const current = startVal + (targetVal - startVal) * eased;
        $el.text(fmt(current));
        if (progress < 1) {
            requestAnimationFrame(update);
        } else {
            $el.text(fmt(targetVal));
        }
    }
    requestAnimationFrame(update);
}

// ── Staged Loading Text ────────────────────────────────────
let loadingInterval = null;
function startStagedLoading() {
    const stages = [
        { text: 'Analyzing parameters...', sub: 'Reading job configuration' },
        { text: 'Computing material costs...', sub: 'Paper, ink, plates' },
        { text: 'Calculating machine time...', sub: 'Optimizing production run' },
        { text: 'Applying costing rules...', sub: 'Markup, wastage, finishing' },
        { text: 'Generating estimate...', sub: 'Almost ready' }
    ];
    let idx = 0;
    $('.rc-loading-text').text(stages[0].text);
    $('.rc-loading-sub').text(stages[0].sub);
    loadingInterval = setInterval(function () {
        idx++;
        if (idx < stages.length) {
            $('.rc-loading-text').text(stages[idx].text);
            $('.rc-loading-sub').text(stages[idx].sub);
        }
    }, 600);
}

function stopStagedLoading() {
    if (loadingInterval) {
        clearInterval(loadingInterval);
        loadingInterval = null;
    }
}

// ── Calculate ──────────────────────────────────────────────
function calculate() {
    // Run full validation with visual indicators
    const errors = validateAllFields(true);
    if (errors.length > 0) {
        showValidationAlert(errors);
        return;
    }

    const partDetails = collectPartDetails();
    const opt = $('#ddlJobType option:selected');
    const jobCode = (opt.data('code') || '').toUpperCase();
    const maxColors = getMaxColorsFromParts();
    const request = {
        jobTypeId: parseInt($('#ddlJobType').val()),
        productTypeId: toIntOrNull($('#ddlProductType').val()),
        productSizeId: toIntOrNull($('#ddlProductSize').val()),
        quantity: parseInt($('#txtQuantity').val()) || 0,
        totalPages: parseInt($('#txtTotalPages').val()) || 2,
        trimWidthMm: parseFloat($('#txtTrimWidth').val()) || 0,
        trimHeightMm: parseFloat($('#txtTrimHeight').val()) || 0,
        printingMode: $('#txtPrintingMode').val(),
        colors: maxColors,
        printingSides: isPrintOnlyCode(jobCode)
            ? (parseInt($('#ddlPrintSide').val()) || 1)
            : (parseInt($('#ddlSides').val()) || 1),
        platesReceived: isPrintOnlyCode(jobCode)
            ? (parseInt($('#txtPlatesReceived').val()) || 0)
            : 0,
        paperId: toLongOrNull($('#ddlPaper').val()),
        machineId: toLongOrNull($('#ddlMachine').val()),
        plateId: toLongOrNull($('#ddlPlate').val()),
        inkCodes: $('#ddlInks').val() || [],
        finishingIds: ($('#ddlFinishings').val() || []).map(Number),
        bindingIds: ($('#ddlBinding').val() || []).map(Number),
        designingIds: ($('#ddlDesigning').val() || []).map(Number),
        partDetails: partDetails,
        isCustomerMaterial: $('#chkCustomerMaterial').is(':checked'),
        outsourceCost: parseFloat($('#txtOutsourceCost').val()) || null,
        labourHours: parseFloat($('#txtLabourHours').val()) || null,
        labourRate: parseFloat($('#txtLabourRate').val()) || null,
        areaWidthFt: parseFloat($('#txtAreaWidth').val()) || null,
        areaHeightFt: parseFloat($('#txtAreaHeight').val()) || null
    };

    $('#loadingOverlay').fadeIn(200);
    $('#btnCalculate').prop('disabled', true).addClass('rc-calculating');
    startStagedLoading();

    $.ajax({
        url: `${API}/calculate`,
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(request),
        success: function (res) {
            showResults(res);
        },
        error: function (xhr) {
            alert('Calculation failed: ' + (xhr.responseText || 'Unknown error'));
        },
        complete: function () {
            stopStagedLoading();
            $('#loadingOverlay').fadeOut(300);
            $('#btnCalculate').prop('disabled', false).removeClass('rc-calculating');
        }
    });
}

function toIntOrNull(v) { return v ? parseInt(v) : null; }
function toLongOrNull(v) { return v ? parseInt(v) : null; }

// ── Display Results ────────────────────────────────────────
function showResults(res) {
    // Cache result for send estimation
    lastCalcResult = res;

    // Animate summary values with count-up
    animateCountUp('#resGrandTotal', res.grandTotal || 0, 800);
    animateCountUp('#resTax', res.taxAmount || 0, 600);
    animateCountUp('#resNetTotal', res.netTotal || 0, 900);
    animateCountUp('#resCostPerUnit', res.costPerUnit || 0, 700);

    // Populate Cost Breakdown modal (dual view: grid + cards)
    let tableHtml = '';
    let cardHtml = '';
    const breakdownRows = res.breakdown || [];
    breakdownRows.forEach((row, idx) => {
        const isTotal = row.category === 'Total' || row.category === 'Tax' || row.category === 'Grand Total';
        // Grid row (desktop)
        tableHtml += `<div class="rc-grid-row ${isTotal ? 'rc-grid-row-total' : 'rc-grid-row-data'}">
            <div class="rc-grid-col" style="width:40px;">${row.icon || ''}</div>
            <div class="rc-grid-col flex-fill fw-medium">${row.name}</div>
            <div class="rc-grid-col" style="width:120px;"><span class="badge bg-secondary-lt">${row.category}</span></div>
            <div class="rc-grid-col flex-fill text-muted small">${row.detail || ''}</div>
            <div class="rc-grid-col text-end fw-semibold" style="width:140px;">${fmt(row.amount)}</div>
        </div>`;
        // Card (mobile)
        const cardClass = isTotal ? 'rc-card rc-card-total' : 'rc-card';
        cardHtml += `<div class="${cardClass}">
            <div class="row g-2 align-items-center">
                <div class="col-auto rc-card-icon">${row.icon || ''}</div>
                <div class="col">
                    <div class="rc-card-name">${row.name}</div>
                    <div class="rc-card-detail">${row.detail || ''}</div>
                </div>
                <div class="col-auto">
                    <span class="badge ${isTotal ? 'bg-dark-lt' : 'bg-secondary-lt'} rounded-pill">${row.category}</span>
                </div>
                <div class="col-12 col-sm-auto text-sm-end">
                    <span class="rc-card-amount">${fmt(row.amount)}</span>
                </div>
            </div>
        </div>`;
    });
    $('#breakdownBodyTable').html(tableHtml);
    $('#breakdownBodyCards').html(cardHtml);
    $('#breakdownCount').text(`${breakdownRows.length} item${breakdownRows.length !== 1 ? 's' : ''}`);
    $('#breakdownEmpty').toggleClass('d-none', breakdownRows.length > 0);

    // Populate BOM modal (dual view: grid + cards)
    let bomTableHtml = '';
    let bomCardHtml = '';
    const bomRows = res.bom || [];
    bomRows.forEach((row, idx) => {
        const qtyFmt = Number(row.quantity || 0).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
        // Grid row (desktop)
        bomTableHtml += `<div class="rc-grid-row rc-grid-row-data">
            <div class="rc-grid-col" style="width:120px;"><span class="badge bg-light text-dark">${row.category || ''}</span></div>
            <div class="rc-grid-col flex-fill fw-medium">${row.item || ''}</div>
            <div class="rc-grid-col flex-fill text-muted small">${row.specification || ''}</div>
            <div class="rc-grid-col text-end" style="width:100px;">${qtyFmt}</div>
            <div class="rc-grid-col" style="width:70px;">${row.unit || ''}</div>
            <div class="rc-grid-col text-end" style="width:110px;">${fmt(row.rate)}</div>
            <div class="rc-grid-col text-end fw-semibold" style="width:120px;">${fmt(row.amount)}</div>
        </div>`;
        // Card (mobile)
        bomCardHtml += `<div class="rc-card">
            <div class="d-flex justify-content-between align-items-start mb-1">
                <span class="badge bg-light text-dark">${row.category || ''}</span>
                <span class="rc-card-amount">${fmt(row.amount)}</span>
            </div>
            <div class="rc-card-name">${row.item || ''}</div>
            <div class="rc-card-detail">${row.specification || ''}</div>
            <div class="d-flex gap-3 mt-1 small text-muted">
                <span>Qty: ${qtyFmt}</span>
                <span>${row.unit || ''}</span>
                <span>Rate: ${fmt(row.rate)}</span>
            </div>
        </div>`;
    });
    $('#bomBodyTable').html(bomTableHtml);
    $('#bomBodyCards').html(bomCardHtml);
    $('#bomCount').text(`${bomRows.length} item${bomRows.length !== 1 ? 's' : ''}`);
    $('#bomEmpty').toggleClass('d-none', bomRows.length > 0);

    // Populate Cost Distribution modal
    const costs = [
        { label: 'Paper', value: res.paperCost, color: '#0d6efd' },
        { label: 'Plates', value: res.plateCost, color: '#6610f2' },
        { label: 'Ink', value: res.inkCost, color: '#d63384' },
        { label: 'Machine', value: res.machineCost, color: '#fd7e14' },
        { label: 'Finishing', value: res.finishingCost, color: '#20c997' },
        { label: 'Binding', value: res.bindingCost, color: '#0dcaf0' },
        { label: 'Designing', value: res.designingCost, color: '#ffc107' },
        { label: 'Packing', value: res.packingCost, color: '#6c757d' }
    ].filter(c => c.value > 0);

    const totalCost = costs.reduce((s, c) => s + c.value, 0);
    const maxVal = Math.max(...costs.map(c => c.value), 1);
    let barsHtml = '';
    costs.forEach(c => {
        const pct = Math.max((c.value / maxVal * 100), 5);
        const sharePct = totalCost > 0 ? ((c.value / totalCost) * 100).toFixed(1) : '0.0';
        barsHtml += `<div class="rc-bar-row">
            <div class="rc-bar-label">${c.label}</div>
            <div class="rc-bar-track">
                <div class="rc-bar-fill" style="width:${pct}%;background:${c.color};">
                    <span class="rc-bar-value">${fmt(c.value)}</span>
                </div>
            </div>
            <div class="rc-bar-pct">${sharePct}%</div>
        </div>`;
    });
    $('#costBars').html(barsHtml);
    $('#distCount').text(`${costs.length} categor${costs.length !== 1 ? 'ies' : 'y'}`);
    $('#distEmpty').toggleClass('d-none', costs.length > 0);

    // Populate Applied Costing Rules modal (dual view: grid + cards)
    let rulesTableHtml = '';
    let rulesCardHtml = '';
    const rulesRows = res.appliedRules || [];
    rulesRows.forEach((rule, idx) => {
        // Grid row (desktop)
        rulesTableHtml += `<div class="rc-grid-row rc-grid-row-data">
            <div class="rc-grid-col" style="width:160px;"><span class="badge bg-warning-lt">${rule.rule || ''}</span></div>
            <div class="rc-grid-col flex-fill small">${rule.detail || ''}</div>
            <div class="rc-grid-col flex-fill small text-muted">${rule.impact || ''}</div>
        </div>`;
        // Card (mobile)
        rulesCardHtml += `<div class="rc-card">
            <div class="mb-1"><span class="badge bg-warning-lt">${rule.rule || ''}</span></div>
            <div class="rc-card-name small">${rule.detail || ''}</div>
            <div class="rc-card-detail">${rule.impact || ''}</div>
        </div>`;
    });
    $('#rulesBodyTable').html(rulesTableHtml);
    $('#rulesBodyCards').html(rulesCardHtml);
    $('#rulesCount').text(`${rulesRows.length} rule${rulesRows.length !== 1 ? 's' : ''}`);
    $('#rulesEmpty').toggleClass('d-none', rulesRows.length > 0);

    // Show Cost Estimation block with celebration effect
    const $block = $('#costEstimationBlock');
    $block.slideDown(400, function () {
        // Add success celebration
        $block.find('.rc-result-card-v2').addClass('rc-result-celebrate');
        setTimeout(function () {
            $block.find('.rc-result-card-v2').removeClass('rc-result-celebrate');
        }, 1200);

        // Stagger metric card animations
        $block.find('.rc-metric-card').each(function (i) {
            const $card = $(this);
            $card.addClass('rc-metric-enter');
            setTimeout(function () {
                $card.removeClass('rc-metric-enter').addClass('rc-metric-entered');
            }, 150 * i + 300);
        });

        // Update stepper to completed state
        updateProgressStepper();

        // Show Send Estimation block (Index page only)
        $('#sendEstimationBlock').slideDown(400);
    });

    // Scroll to Cost Estimation block
    $('html, body').animate({ scrollTop: $block.offset().top - 20 }, 500);
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

// ── Validation Alert ───────────────────────────────────────
function showValidationAlert(errors) {
    // Remove any previous validation alert
    $('#validationAlertOverlay').remove();

    let listHtml = errors.map(e => `<li>${escapeHtml(e)}</li>`).join('');
    const html = `
        <div id="validationAlertOverlay" class="validation-alert-overlay">
            <div class="validation-alert-box">
                <div class="validation-alert-header">
                    <i class="bi bi-exclamation-triangle-fill me-2"></i>Validation Failed
                </div>
                <div class="validation-alert-body">
                    <p class="mb-2">Please fill in the following required fields:</p>
                    <ul class="validation-error-list">${listHtml}</ul>
                </div>
                <div class="validation-alert-footer">
                    <button class="btn btn-primary px-4" onclick="$('#validationAlertOverlay').fadeOut(200, function(){ $(this).remove(); })">OK</button>
                </div>
            </div>
        </div>`;
    $('body').append(html);
    $('#validationAlertOverlay').fadeIn(200);
}

// Clear validation error on field input/change
$(document).on('input change', '.is-invalid, .form-control, .form-select', function () {
    $(this).removeClass('is-invalid');
    // Also clear Select2 invalid state
    $(this).next('.select2-container').find('.select2-selection').removeClass('is-invalid');
});

// ── Reset ──────────────────────────────────────────────────
function resetForm() {
    // Clear validation errors
    $('.is-invalid').removeClass('is-invalid');
    $('.select2-selection.is-invalid').removeClass('is-invalid');
    $('#validationAlertOverlay').remove();

    $('#ddlJobType').val('').trigger('change');
    $('#ddlProductType').val('').trigger('change');
    $('#ddlProductSize').val('').trigger('change');
    $('#ddlDesigning').val(null).trigger('change');
    $('#ddlBinding').val(null).trigger('change');
    $('#ddlInks').val(null);
    $('#ddlFinishings').val(null).trigger('change');
    $('#ddlPlate').val('');
    $('#ddlMachine').val('');
    $('#txtQuantity').val(500);
    $('#txtTotalPages').val(0);
    $('#txtTrimWidth').val('');
    $('#txtTrimHeight').val('');
    $('#ddlSides').val(2);
    $('#chkCustomerMaterial').prop('checked', false);
    $('#txtPrintingMode').val('');
    $('#txtOutsourceCost').val('');
    $('#txtVendorDesc').val('');
    $('#txtLabourType').val('');
    $('#txtLabourHours').val('');
    $('#txtLabourRate').val('');
    $('#txtAreaWidth').val('');
    $('#txtAreaHeight').val('');
    $('#txtTotalArea').val('');
    // Print-only fields
    $('#ddlPrintSize').val('').trigger('change.select2');
    $('#ddlPrintSide').val('1');
    $('#ddlPrintColors').val('4');
    $('#txtPlatesReceived').val('0');
    $('#secPrintOnlyFields').hide();
    $('#partsList').html('');
    $('#partsContainer').hide();
    hideAllSections();
    $('#jobTypeFlags').addClass('d-none');
    $('#workflowBar').addClass('d-none');
    $('#dynamicFieldsInfo').addClass('d-none');
    $('#aiSelectionInfo').addClass('d-none');
    $('#aiSelectionInfoText').html('');
    $('#chkAutoSmartRecommend').prop('checked', true);
    isAutoRecommendEnabled = true;
    $('#plateAiInfo').addClass('d-none');
    $('#machineAiInfo').addClass('d-none');
    $('#costEstimationBlock').hide();
    $('#sendEstimationBlock').hide();
    allMachines = [];
    allPlates = [];
    allInks = [];
    lastCalcResult = null;
    lastAiRecommendation = null;
    $('#txtSendCustName').val('');
    $('#txtSendCustPhone').val('');
    $('#txtSendCustEmail').val('');
    $('.rc-send-toggle-pill').removeClass('active').first().addClass('active');
    $('input[name=sendContentType][value=summary]').prop('checked', true);

    // Restore all product types (remove job type filter)
    populateProductTypeDropdown(allProductTypes);

    // Reset progress stepper
    updateProgressStepper();

    // Remove metric entrance classes
    $('.rc-metric-entered, .rc-metric-enter').removeClass('rc-metric-entered rc-metric-enter');
}

// ═══════════════════════════════════════════════════════════
// SEND ESTIMATION / QUICK REQUISITION
// ═══════════════════════════════════════════════════════════

function generateEstRefNo() {
    const now = new Date();
    const pad = (n) => String(n).padStart(2, '0');
    return 'EST-' + now.getFullYear() + pad(now.getMonth() + 1) + pad(now.getDate()) + '-' +
        pad(now.getHours()) + pad(now.getMinutes()) + pad(now.getSeconds());
}

function getJobSummaryInfo() {
    return {
        jobType: $('#ddlJobType option:selected').text().trim() || '—',
        productType: $('#ddlProductType option:selected').text().trim() || '—',
        quantity: $('#txtQuantity').val() || '—',
        size: ($('#txtTrimWidth').val() || '—') + ' × ' + ($('#txtTrimHeight').val() || '—') + ' mm'
    };
}

function buildConfigData(snapshot, partDetails) {
    var flags = (lastCalcResult && lastCalcResult.jobTypeFlags) || {};
    var stages = [];
    if (flags.isDesignRequired || flags.isDtpRequired) stages.push('Design/DTP');
    if (flags.isCtpRequired) stages.push('CTP/Plates');
    if (flags.isPrintingRequired) stages.push('Printing');
    if (flags.isBindingRequired) stages.push('Binding');
    if (flags.isFinishingRequired) stages.push('Finishing');

    var jobTypeName = $('#ddlJobType option:selected').text() || '';
    var productTypeName = $('#ddlProductType option:selected').text() || '';
    var productSizeName = $('#ddlProductSize option:selected').text() || '';
    var sidesText = $('#ddlSides option:selected').text() || '';
    var machineName = $('#ddlMachine option:selected').text() || '';
    var plateName = $('#ddlPlate option:selected').text() || '';

    var designItems = [];
    $('#ddlDesigning option:selected').each(function () { designItems.push($(this).text()); });
    var bindingTypes = [];
    $('#ddlBinding option:selected').each(function () { bindingTypes.push($(this).text()); });
    var finishingTypes = [];
    $('#ddlFinishings option:selected').each(function () { finishingTypes.push($(this).text()); });

    var productParts = (partDetails || []).map(function (p) {
        return {
            partName: p.partName || '',
            specification: {
                pages: p.noOfPages || null,
                color: p.colors || null,
                paper: p.paperName || null
            },
            designDtp: (flags.isDesignRequired || flags.isDtpRequired) ? { designItems: designItems } : null,
            ctpPlates: flags.isCtpRequired ? { plateName: plateName } : null,
            printing: flags.isPrintingRequired ? { machineName: machineName } : null
        };
    });

    return {
        jobType: { value: jobTypeName.trim(), workflowStages: stages },
        productType: productTypeName.trim(),
        productSize: productSizeName.trim(),
        trimWidth: snapshot.trimWidthMm,
        trimHeight: snapshot.trimHeightMm,
        quantity: snapshot.quantity,
        sides: sidesText.trim(),
        productParts: productParts,
        binding: flags.isBindingRequired ? { bindingTypes: bindingTypes } : null,
        finishing: flags.isFinishingRequired ? { finishingTypes: finishingTypes } : null
    };
}

function buildRecommendedMachinesData(snapshot) {
    const machineId = snapshot.machineId || toLongOrNull($('#ddlMachine').val());
    const machineName = ($('#ddlMachine option:selected').text() || '').trim();
    if (!machineId || !machineName) return null;

    return [{
        machineId: machineId,
        machineName: machineName,
        printingMode: snapshot.printingMode || null,
        trimWidthMm: snapshot.trimWidthMm || null,
        trimHeightMm: snapshot.trimHeightMm || null,
        quantity: snapshot.quantity || null
    }];
}

function getCalcInputForSave() {
    const partDetails = collectPartDetails();
    const snapshot = {
        jobTypeId: parseInt($('#ddlJobType').val()) || null,
        productTypeId: toIntOrNull($('#ddlProductType').val()),
        productSizeId: toIntOrNull($('#ddlProductSize').val()),
        quantity: parseInt($('#txtQuantity').val()) || 0,
        totalPages: parseInt($('#txtTotalPages').val()) || 2,
        trimWidthMm: parseFloat($('#txtTrimWidth').val()) || 0,
        trimHeightMm: parseFloat($('#txtTrimHeight').val()) || 0,
        printingMode: $('#txtPrintingMode').val() || null,
        colors: parseInt($('#ddlSides').val()) || 1,
        paperId: toLongOrNull($('#ddlPaper').val()),
        machineId: toLongOrNull($('#ddlMachine').val()),
        plateId: toLongOrNull($('#ddlPlate').val()),
        inkCodes: $('#ddlInks').val() || [],
        finishingIds: ($('#ddlFinishings').val() || []).map(Number),
        bindingIds: ($('#ddlBinding').val() || []).map(Number),
        designingIds: ($('#ddlDesigning').val() || []).map(Number),
        partDetails: partDetails,
        isCustomerMaterial: $('#chkCustomerMaterial').is(':checked')
    };
    var configObj = buildConfigData(snapshot, partDetails);
    var recommendedMachinesObj = buildRecommendedMachinesData(snapshot);
    return {
        jobTypeId: snapshot.jobTypeId,
        productTypeId: snapshot.productTypeId,
        productSizeId: snapshot.productSizeId,
        partyId: null,
        totalPages: snapshot.totalPages,
        trimWidthMm: snapshot.trimWidthMm,
        trimHeightMm: snapshot.trimHeightMm,
        printingMode: snapshot.printingMode,
        isCustomerMaterial: snapshot.isCustomerMaterial,
        partsData: partDetails.length > 0 ? JSON.stringify(partDetails) : null,
        costBreakdown: lastCalcResult && lastCalcResult.breakdown ? JSON.stringify(lastCalcResult.breakdown) : null,
        bomData: lastCalcResult && lastCalcResult.bom ? JSON.stringify(lastCalcResult.bom) : null,
        recommendedMachines: recommendedMachinesObj ? JSON.stringify(recommendedMachinesObj) : null,
        calcInputSnapshot: JSON.stringify(snapshot),
        configData: JSON.stringify(configObj)
    };
}

function validateSendFields(channel) {
    const name = $('#txtSendCustName').val().trim();
    const phone = $('#txtSendCustPhone').val().trim();
    const email = $('#txtSendCustEmail').val().trim();
    const errors = [];
    if (!name) errors.push('Customer Name is required');
    if (channel === 'whatsapp' || channel === 'review') {
        if (!phone) errors.push('Phone No is required for WhatsApp');
    }
    if (channel === 'email') {
        if (!email) errors.push('Email ID is required');
        if (email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) errors.push('Invalid Email format');
    }
    if (!lastCalcResult) errors.push('Please calculate the cost first');
    return errors;
}

function isDetailedContent() {
    return $('input[name=sendContentType]:checked').val() === 'detailed';
}

function buildEstimationPreviewHtml(refNo) {
    const info = getJobSummaryInfo();
    const res = lastCalcResult;
    const name = escapeHtml($('#txtSendCustName').val().trim());
    const phone = escapeHtml($('#txtSendCustPhone').val().trim());
    const email = escapeHtml($('#txtSendCustEmail').val().trim());
    const now = new Date();
    const dateStr = now.toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' });
    const detailed = isDetailedContent();

    let breakdownHtml = '';
    if (detailed && res.breakdown && res.breakdown.length > 0) {
        breakdownHtml = `<div class="rc-est-table-section">
            <h6>Cost Breakdown</h6>
            <table class="rc-est-table">
                <thead><tr><th>Component</th><th>Detail</th><th class="text-end">Amount (₹)</th></tr></thead>
                <tbody>${res.breakdown.map(r => `<tr>
                    <td class="${(r.category === 'Total' || r.category === 'Tax' || r.category === 'Grand Total') ? 'fw-bold' : ''}">${escapeHtml(r.name)}</td>
                    <td>${escapeHtml(r.detail || '')}</td>
                    <td class="text-end fw-bold">${fmt(r.amount)}</td>
                </tr>`).join('')}</tbody>
            </table>
        </div>`;
    }

    return `<div class="rc-est-preview">
        <div class="rc-est-header">
            <div>
                <div class="rc-est-company-name">MinePress</div>
                <div class="rc-est-company-tagline">Printing & Publishing Solutions</div>
            </div>
            <div>
                <div class="rc-est-doc-title">ESTIMATION</div>
                <div class="rc-est-meta">
                    <span>Ref: ${escapeHtml(refNo)}</span>
                    <span>Date: ${dateStr}</span>
                </div>
            </div>
        </div>
        <div class="rc-est-customer-bar">
            <div class="rc-est-cust-item"><i class="bi bi-person-fill me-1 text-muted"></i><strong>${name}</strong></div>
            ${phone ? `<div class="rc-est-cust-item"><i class="bi bi-phone-fill me-1 text-muted"></i>${phone}</div>` : ''}
            ${email ? `<div class="rc-est-cust-item"><i class="bi bi-envelope-fill me-1 text-muted"></i>${email}</div>` : ''}
        </div>
        <div class="rc-est-job-bar">
            <div class="rc-est-job-item"><strong>Job:</strong> ${escapeHtml(info.jobType)}</div>
            <div class="rc-est-job-item"><strong>Product:</strong> ${escapeHtml(info.productType)}</div>
            <div class="rc-est-job-item"><strong>Qty:</strong> ${escapeHtml(info.quantity)}</div>
            <div class="rc-est-job-item"><strong>Size:</strong> ${escapeHtml(info.size)}</div>
        </div>
        ${breakdownHtml}
        <div class="rc-est-summary-section">
            <table class="rc-est-summary-tbl">
                <tr><td>Grand Total</td><td class="text-end">${fmt(res.grandTotal)}</td></tr>
                <tr><td>GST (18%)</td><td class="text-end">${fmt(res.taxAmount)}</td></tr>
                <tr class="rc-est-net-row"><td>Net Total</td><td class="text-end">${fmt(res.netTotal)}</td></tr>
                <tr><td>Cost Per Unit</td><td class="text-end">${fmt(res.costPerUnit)}</td></tr>
            </table>
        </div>
        <div class="rc-est-footer">
            <ol>
                <li>This is a system-generated estimation and may vary based on final specifications.</li>
                <li>Prices are valid for 15 days from the date of estimation.</li>
                <li>GST @18% is applicable as shown above.</li>
                <li>Delivery timeline will be confirmed upon order confirmation.</li>
            </ol>
        </div>
    </div>`;
}

function populatePrintableEstimation(refNo) {
    const info = getJobSummaryInfo();
    const res = lastCalcResult;
    const now = new Date();
    const dateStr = now.toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' });
    const detailed = isDetailedContent();

    $('#printRefNo').text(refNo);
    $('#printDate').text(dateStr);
    $('#printCustName').text($('#txtSendCustName').val().trim());
    $('#printCustPhone').text($('#txtSendCustPhone').val().trim());
    $('#printCustEmail').text($('#txtSendCustEmail').val().trim());
    $('#printJobType').text(info.jobType);
    $('#printProductType').text(info.productType);
    $('#printQuantity').text(info.quantity);
    $('#printSize').text(info.size);
    $('#printGrandTotal').text(fmt(res.grandTotal));
    $('#printTax').text(fmt(res.taxAmount));
    $('#printNetTotal').text(fmt(res.netTotal));
    $('#printCostPerUnit').text(fmt(res.costPerUnit));

    if (detailed && res.breakdown && res.breakdown.length > 0) {
        let rows = '';
        res.breakdown.forEach(r => {
            rows += `<tr><td>${escapeHtml(r.name)}</td><td>${escapeHtml(r.detail || '')}</td><td class="text-end">${fmt(r.amount)}</td></tr>`;
        });
        $('#printBreakdownBody').html(rows);
        $('#printBreakdownSection').show();
    } else {
        $('#printBreakdownSection').hide();
    }
}

function reviewEstimation() {
    const errors = validateSendFields('review');
    if (errors.length > 0) {
        Swal.fire({ icon: 'warning', title: 'Missing Information', html: errors.map(e => `<div>• ${e}</div>`).join(''), confirmButtonColor: '#25d366' });
        return;
    }
    const refNo = generateEstRefNo();
    const html = buildEstimationPreviewHtml(refNo);
    $('#estimationPreviewContent').html(html);
    $('#estRefNo').text(refNo);
    $('#modalEstimationPreview').modal('show');
}

function buildWhatsAppMessage() {
    const info = getJobSummaryInfo();
    const res = lastCalcResult;
    const name = $('#txtSendCustName').val().trim();
    const detailed = isDetailedContent();
    const refNo = generateEstRefNo();
    let msg = `📋 *MinePress — Printing Estimation*\n`;
    msg += `Ref: ${refNo}\n\n`;
    msg += `Dear ${name},\n\n`;
    msg += `Here is your estimation:\n`;
    msg += `━━━━━━━━━━━━━━━━\n`;
    msg += `*Job:* ${info.jobType}\n`;
    msg += `*Product:* ${info.productType}\n`;
    msg += `*Qty:* ${info.quantity}\n`;
    msg += `*Size:* ${info.size}\n`;
    msg += `━━━━━━━━━━━━━━━━\n\n`;

    if (detailed && res.breakdown && res.breakdown.length > 0) {
        msg += `*Cost Breakdown:*\n`;
        res.breakdown.forEach(r => {
            if (r.amount > 0) msg += `• ${r.name}: ${fmt(r.amount)}\n`;
        });
        msg += `\n`;
    }

    msg += `💰 *Grand Total:* ${fmt(res.grandTotal)}\n`;
    msg += `📊 *GST (18%):* ${fmt(res.taxAmount)}\n`;
    msg += `✅ *Net Total:* ${fmt(res.netTotal)}\n`;
    msg += `🏷️ *Cost/Unit:* ${fmt(res.costPerUnit)}\n\n`;
    msg += `_Valid for 15 days. GST @18% applicable._\n`;
    msg += `_MinePress — Printing & Publishing Solutions_`;
    return { msg, refNo };
}

function sendViaWhatsApp() {
    const errors = validateSendFields('whatsapp');
    if (errors.length > 0) {
        Swal.fire({ icon: 'warning', title: 'Missing Information', html: errors.map(e => `<div>• ${e}</div>`).join(''), confirmButtonColor: '#25d366' });
        return;
    }
    let phone = $('#txtSendCustPhone').val().trim().replace(/[^0-9]/g, '');
    if (phone.length === 10) phone = '91' + phone;
    const { msg, refNo } = buildWhatsAppMessage();
    const url = `https://wa.me/${phone}?text=${encodeURIComponent(msg)}`;
    window.open(url, '_blank');
    logSendActivity('whatsapp', refNo);
}

function sendViaEmail() {
    const errors = validateSendFields('email');
    if (errors.length > 0) {
        Swal.fire({ icon: 'warning', title: 'Missing Information', html: errors.map(e => `<div>• ${e}</div>`).join(''), confirmButtonColor: '#0d6efd' });
        return;
    }
    const refNo = generateEstRefNo();
    const info = getJobSummaryInfo();
    const calcInput = getCalcInputForSave();
    const payload = {
        customerName: $('#txtSendCustName').val().trim(),
        customerPhone: $('#txtSendCustPhone').val().trim(),
        customerEmail: $('#txtSendCustEmail').val().trim(),
        refNo: refNo,
        jobType: info.jobType,
        productType: info.productType,
        quantity: info.quantity,
        size: info.size,
        includeBreakdown: isDetailedContent(),
        grandTotal: lastCalcResult.grandTotal,
        taxAmount: lastCalcResult.taxAmount,
        netTotal: lastCalcResult.netTotal,
        costPerUnit: lastCalcResult.costPerUnit,
        breakdown: isDetailedContent() ? (lastCalcResult.breakdown || []) : [],
        ...calcInput
    };

    Swal.fire({
        title: 'Sending Email...',
        html: '<div class="d-flex align-items-center gap-2"><div class="spinner-border spinner-border-sm text-primary"></div> Sending estimation to <strong>' + escapeHtml(payload.customerEmail) + '</strong></div>',
        allowOutsideClick: false,
        showConfirmButton: false,
        didOpen: () => { Swal.showLoading(); }
    });

    $.ajax({
        url: `${API}/send-estimation`,
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(payload),
        success: function () {
            Swal.fire({
                icon: 'success',
                title: 'Email Sent!',
                html: `Estimation <strong>${refNo}</strong> sent successfully to <strong>${escapeHtml(payload.customerEmail)}</strong>.<br/><small class="text-muted">Sales team & management have been notified.</small>`,
                confirmButtonColor: '#25d366'
            });
        },
        error: function (xhr) {
            Swal.fire({
                icon: 'error',
                title: 'Send Failed',
                text: xhr.responseText || 'Could not send email. Please try again.',
                confirmButtonColor: '#dc3545'
            });
        }
    });
}

function printEstimation() {
    const errors = validateSendFields('print');
    if (errors.length > 0) {
        Swal.fire({ icon: 'warning', title: 'Missing Information', html: errors.map(e => `<div>• ${e}</div>`).join(''), confirmButtonColor: '#6366f1' });
        return;
    }
    const refNo = generateEstRefNo();
    populatePrintableEstimation(refNo);
    $('#printableEstimation').removeClass('d-none');
    setTimeout(function () {
        window.print();
        setTimeout(function () {
            $('#printableEstimation').addClass('d-none');
        }, 500);
    }, 200);
    logSendActivity('print', refNo);
}

function logSendActivity(channel, refNo) {
    const info = getJobSummaryInfo();
    const calcInput = getCalcInputForSave();
    const payload = {
        channel: channel,
        refNo: refNo,
        customerName: $('#txtSendCustName').val().trim(),
        customerPhone: $('#txtSendCustPhone').val().trim(),
        customerEmail: $('#txtSendCustEmail').val().trim(),
        jobType: info.jobType,
        productType: info.productType,
        quantity: info.quantity,
        netTotal: lastCalcResult ? lastCalcResult.netTotal : 0,
        grandTotal: lastCalcResult ? lastCalcResult.grandTotal : 0,
        taxAmount: lastCalcResult ? lastCalcResult.taxAmount : 0,
        costPerUnit: lastCalcResult ? lastCalcResult.costPerUnit : 0,
        ...calcInput
    };
    $.ajax({
        url: `${API}/log-estimation-activity`,
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(payload)
    });
}
