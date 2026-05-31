// ===== MinePress Rate Calculator — Details Page JS =====

const RC_API = '/api/ratecalculator';
let calcData = null;

$(document).ready(function () {
    const params = new URLSearchParams(window.location.search);
    const id = params.get('id');
    if (id) {
        loadCalcDetail(id);
    } else {
        showError('No calculation ID provided.');
    }
});

async function loadCalcDetail(id) {
    try {
        calcData = await $.get(`${RC_API}/detail/${id}`);
        renderHeader();
        renderSummary();
        renderConfig();
        renderCostBreakdown();
        renderBom();
        renderRules();
        renderDistribution();
        renderRemarks();
        $('#detailsLoader').hide();
        $('#detailsContent').show();
    } catch (err) {
        showError(err.responseJSON?.message || 'Failed to load calculation details.');
    }
}

function showError(msg) {
    $('#detailsLoader').hide();
    $('#errorMessage').text(msg);
    $('#detailsError').show();
}

// ══════════════════════════════════════════
//  HEADER
// ══════════════════════════════════════════
function renderHeader() {
    const d = calcData;
    $('#hdCalcRefNo').text(d.calcRefNo);
    const statusCls = (d.status || 'DRAFT').toUpperCase() === 'FINAL' ? 'bg-success-lt text-success'
        : (d.status || 'DRAFT').toUpperCase() === 'SENT' ? 'bg-blue-lt text-blue'
        : 'bg-secondary-lt text-secondary';
    $('#hdStatus').html(`<span class="badge ${statusCls}">${d.status || 'DRAFT'}</span>`);
    $('#hdVersion').html(`<span class="badge bg-secondary-lt text-secondary">v${d.version || 1}</span>`);
    $('#hdDate').text(d.createdOn || '');
    $('#hdCreatedBy').text(d.createdBy || '');
}

// ══════════════════════════════════════════
//  SUMMARY CARDS
// ══════════════════════════════════════════
function renderSummary() {
    const d = calcData;
    $('#smGrandTotal').text(fmt(d.grandTotal));
    $('#smNetTotal').text(fmt(d.netTotal));
    $('#smCostPerUnit').text(fmt(d.costPerUnit));
    $('#smQuantity').text((d.quantity || 0).toLocaleString('en-IN'));
}

// ══════════════════════════════════════════
//  JOB CONFIGURATION
// ══════════════════════════════════════════
function renderConfig() {
    const d = calcData;
    $('#cfgCustomer').html(d.partyName
        ? `${esc(d.partyName)} <span class="text-muted small">(${esc(d.partyCode || '')})</span>`
        : '<span class="text-muted">—</span>');
    $('#cfgJobType').html(d.jobTypeName
        ? `<span class="badge bg-blue-lt">${esc(d.jobTypeName)}</span>`
        : '—');
    $('#cfgProductType').html(d.productTypeName
        ? `<span class="badge bg-purple-lt">${esc(d.productTypeName)}</span>`
        : '—');
    $('#cfgProductSize').text(d.productSizeName || '—');
    $('#cfgPrintingMode').html(d.printingMode
        ? `<span class="badge bg-orange-lt">${esc(d.printingMode)}</span>`
        : '—');
    $('#cfgTrimSize').text(d.trimWidthMm && d.trimHeightMm
        ? `${d.trimWidthMm} × ${d.trimHeightMm} mm`
        : '—');
    $('#cfgTotalPages').text(d.totalPages || '—');
    $('#cfgCustomerMaterial').html(d.isCustomerMaterial
        ? '<span class="badge bg-warning-lt">Yes</span>'
        : '<span class="badge bg-secondary-lt">No</span>');
    $('#cfgValidity').text(d.validityDate || '—');

    // Linked documents
    let links = '';
    if (d.enquiryNo) links += `<a href="/Enquiry/Details?id=${d.enquiryId}" class="badge bg-primary-lt text-primary text-decoration-none me-1"><i class="bi bi-clipboard-data me-1"></i>${esc(d.enquiryNo)}</a>`;
    if (d.quotationNo) links += `<a href="/Quotation/Details?id=${d.quotationId}" class="badge bg-success-lt text-success text-decoration-none me-1"><i class="bi bi-file-earmark-text me-1"></i>${esc(d.quotationNo)}</a>`;
    if (d.jobNo) links += `<a href="/Job/Details?id=${d.jobId}" class="badge bg-purple-lt text-purple text-decoration-none me-1"><i class="bi bi-briefcase me-1"></i>${esc(d.jobNo)}</a>`;
    $('#cfgLinkedDocs').html(links || '<span class="text-muted">None</span>');
}

// ══════════════════════════════════════════
//  COST BREAKDOWN TAB
// ══════════════════════════════════════════
function renderCostBreakdown() {
    const raw = calcData.costBreakdown;
    if (!raw) return;

    let items;
    try { items = typeof raw === 'string' ? JSON.parse(raw) : raw; } catch { return; }
    if (!Array.isArray(items) || items.length === 0) return;

    const categoryColors = {
        'Paper': 'bg-cyan-lt text-cyan',
        'Prepress': 'bg-orange-lt text-orange',
        'Printing': 'bg-blue-lt text-blue',
        'Postpress': 'bg-teal-lt text-teal',
        'Service': 'bg-purple-lt text-purple',
        'External': 'bg-pink-lt text-pink',
        'Tax': 'bg-warning-lt text-warning',
        'Total': 'bg-dark text-white',
        'Grand Total': 'bg-dark text-white'
    };

    let html = '';
    items.forEach(item => {
        const colorClass = categoryColors[item.category] || 'bg-secondary-lt';
        const isTotal = (item.category || '').includes('Total');
        const rowClass = isTotal ? 'fw-bold' : '';

        html += `<tr class="${rowClass}">
            <td class="text-center">${item.icon || ''}</td>
            <td>${esc(item.name || '')}</td>
            <td><span class="badge ${colorClass}">${esc(item.category || '')}</span></td>
            <td class="text-muted small">${esc(item.detail || '')}</td>
            <td class="text-end ${isTotal ? 'fs-5' : ''}">${fmt(item.amount || 0)}</td>
        </tr>`;
    });

    $('#costBreakdownBody').html(html);
}

// ══════════════════════════════════════════
//  BOM TAB
// ══════════════════════════════════════════
function renderBom() {
    const raw = calcData.bomData;
    if (!raw) return;

    let items;
    try { items = typeof raw === 'string' ? JSON.parse(raw) : raw; } catch { return; }
    if (!Array.isArray(items) || items.length === 0) return;

    const categoryColors = {
        'Paper': 'bg-cyan-lt',
        'Prepress': 'bg-orange-lt',
        'Printing': 'bg-blue-lt',
        'Postpress': 'bg-teal-lt',
        'Service': 'bg-purple-lt',
        'External': 'bg-pink-lt'
    };

    let html = '';
    items.forEach(item => {
        const catClass = categoryColors[item.category] || 'bg-secondary-lt';
        html += `<tr>
            <td><span class="badge ${catClass}">${esc(item.category || '')}</span></td>
            <td class="fw-semibold">${esc(item.item || item.material_name || '')}</td>
            <td class="text-muted small">${esc(item.specification || '')}</td>
            <td>${esc(item.for_part || item.forPart || '—')}</td>
            <td class="text-end">${fmtNum(item.quantity || 0)}</td>
            <td>${esc(item.unit || '')}</td>
            <td class="text-end">${fmt(item.rate || 0)}</td>
            <td class="text-end fw-semibold">${fmt(item.amount || 0)}</td>
        </tr>`;
    });

    $('#bomBody').html(html);
}

// ══════════════════════════════════════════
//  APPLIED RULES TAB
// ══════════════════════════════════════════
function renderRules() {
    // Applied rules from calcInputSnapshot or inline
    const raw = calcData.calcInputSnapshot;
    let rules = [];

    // Try parsing from calcInputSnapshot
    if (raw) {
        try {
            const snapshot = typeof raw === 'string' ? JSON.parse(raw) : raw;
            if (snapshot.appliedRules) rules = snapshot.appliedRules;
        } catch { }
    }

    // If no rules from snapshot, try cost breakdown to extract categories
    if (rules.length === 0) {
        // Build rules from cost breakdown data
        const bdRaw = calcData.costBreakdown;
        if (bdRaw) {
            try {
                const bd = typeof bdRaw === 'string' ? JSON.parse(bdRaw) : bdRaw;
                if (Array.isArray(bd)) {
                    const categories = {};
                    bd.forEach(item => {
                        if (item.category && !item.category.includes('Total') && item.category !== 'Tax') {
                            if (!categories[item.category]) categories[item.category] = 0;
                            categories[item.category] += (item.amount || 0);
                        }
                    });
                    Object.entries(categories).forEach(([cat, amt]) => {
                        rules.push({ rule: cat + ' Cost', detail: `Total ${cat.toLowerCase()} cost component`, impact: fmt(amt) });
                    });
                }
            } catch { }
        }

        // Add standard rules
        if (calcData.quantity > 0) {
            rules.push({ rule: 'Quantity', detail: `${calcData.quantity} units ordered`, impact: `Cost/Unit: ${fmt(calcData.costPerUnit)}` });
        }
        if (calcData.taxAmount > 0) {
            rules.push({ rule: 'GST', detail: '18% tax applied', impact: fmt(calcData.taxAmount) });
        }
        rules.push({ rule: 'Rounding', detail: 'Rounded to nearest ₹10', impact: `Net: ${fmt(calcData.netTotal)}` });
    }

    if (rules.length === 0) return;

    let html = '';
    rules.forEach(r => {
        html += `<div class="rcd-rule-card">
            <div class="d-flex justify-content-between align-items-start">
                <div>
                    <div class="fw-semibold"><i class="bi bi-check-circle text-success me-1"></i>${esc(r.rule || '')}</div>
                    <div class="text-muted small mt-1">${esc(r.detail || '')}</div>
                </div>
                <div class="text-end">
                    <span class="badge bg-primary-lt">${esc(r.impact || '')}</span>
                </div>
            </div>
        </div>`;
    });

    $('#rulesContainer').html(html);

    // AI Insights
    const aiRaw = calcData.aiInsights;
    if (aiRaw) {
        let insights;
        try { insights = typeof aiRaw === 'string' ? JSON.parse(aiRaw) : aiRaw; } catch { return; }
        if (Array.isArray(insights) && insights.length > 0) {
            let aiHtml = '';
            insights.forEach(ins => {
                aiHtml += `<div class="rcd-insight-card rcd-insight-info">
                    <div class="fw-semibold">💡 ${esc(ins.rule || '')}</div>
                    <div class="small mt-1">${esc(ins.detail || '')}</div>
                    ${ins.impact ? `<div class="small text-muted mt-1"><i class="bi bi-arrow-right me-1"></i>${esc(ins.impact)}</div>` : ''}
                </div>`;
            });
            $('#aiInsightsContainer').html(aiHtml);
            $('#aiInsightsCard').show();
        }
    }
}

// ══════════════════════════════════════════
//  DISTRIBUTION TAB
// ══════════════════════════════════════════
function renderDistribution() {
    // Cost distribution bar chart
    const bdRaw = calcData.costBreakdown;
    if (bdRaw) {
        try {
            const bd = typeof bdRaw === 'string' ? JSON.parse(bdRaw) : bdRaw;
            if (Array.isArray(bd)) {
                const categories = {};
                let total = 0;
                bd.forEach(item => {
                    if (item.category && !item.category.includes('Total') && item.category !== 'Tax' && (item.amount || 0) > 0) {
                        if (!categories[item.category]) categories[item.category] = 0;
                        categories[item.category] += (item.amount || 0);
                        total += (item.amount || 0);
                    }
                });

                if (total > 0) {
                    const colors = ['#0d6efd', '#0dcaf0', '#198754', '#fd7e14', '#6f42c1', '#d63384', '#20c997', '#6610f2'];
                    let barHtml = '<div class="rcd-dist-bar">';
                    let legendHtml = '<div class="mt-3">';
                    let colorIdx = 0;

                    Object.entries(categories).sort((a, b) => b[1] - a[1]).forEach(([cat, amt]) => {
                        const pct = ((amt / total) * 100).toFixed(1);
                        const color = colors[colorIdx % colors.length];
                        barHtml += `<div style="width:${pct}%;background:${color};" title="${esc(cat)}: ${fmt(amt)} (${pct}%)">${pct > 8 ? pct + '%' : ''}</div>`;
                        legendHtml += `<div class="d-inline-flex align-items-center me-3 mb-2">
                            <span class="avatar avatar-xs rounded-circle me-2" style="background:${color};width:12px;height:12px;min-width:12px;"></span>
                            <span class="small">${esc(cat)} <strong>${fmt(amt)}</strong> <span class="text-muted">(${pct}%)</span></span>
                        </div>`;
                        colorIdx++;
                    });

                    barHtml += '</div>';
                    legendHtml += '</div>';
                    $('#costDistributionChart').html(barHtml + legendHtml);
                }
            }
        } catch { }
    }

    // Parts breakdown
    const partsRaw = calcData.partsData;
    if (partsRaw) {
        try {
            const parts = typeof partsRaw === 'string' ? JSON.parse(partsRaw) : partsRaw;
            if (Array.isArray(parts) && parts.length > 0) {
                let partsHtml = '';
                parts.forEach((part, idx) => {
                    partsHtml += `<div class="rcd-part-card">
                        <div class="d-flex justify-content-between align-items-start">
                            <div>
                                <div class="fw-semibold">
                                    <span class="badge bg-primary-lt me-1">#${idx + 1}</span>
                                    ${esc(part.partName || part.part_name || 'Part ' + (idx + 1))}
                                </div>
                                <div class="text-muted small mt-1">
                                    ${part.pages || part.noOfPages ? `<i class="bi bi-file-earmark me-1"></i>${part.pages || part.noOfPages} pages` : ''}
                                    ${part.colors ? ` · <i class="bi bi-palette me-1"></i>${part.colors} colors` : ''}
                                    ${part.paperName || part.paper_name ? ` · <i class="bi bi-layers me-1"></i>${esc(part.paperName || part.paper_name)}` : ''}
                                </div>
                            </div>
                            <div class="text-end">
                                ${part.subTotal || part.sub_total ? `<div class="fw-bold">${fmt(part.subTotal || part.sub_total)}</div>` : ''}
                            </div>
                        </div>
                    </div>`;
                });
                $('#partsContainer').html(partsHtml);
            }
        } catch { }
    }

    // Recommended machines
    const machRaw = calcData.recommendedMachines;
    if (machRaw) {
        try {
            const machines = typeof machRaw === 'string' ? JSON.parse(machRaw) : machRaw;
            if (Array.isArray(machines) && machines.length > 0) {
                let machHtml = '<div class="row g-2">';
                machines.forEach((m, idx) => {
                    machHtml += `<div class="col-md-4">
                        <div class="rcd-machine-card">
                            <div class="fw-semibold">
                                ${idx === 0 ? '<span class="badge bg-success-lt me-1">Recommended</span>' : ''}
                                ${esc(m.machineName || m.machine_name || '')}
                            </div>
                            <div class="text-muted small mt-1">
                                ${m.machineType || m.machine_type ? esc(m.machineType || m.machine_type) : ''}
                                ${m.maxColors || m.max_colors ? ` · ${m.maxColors || m.max_colors} colors` : ''}
                            </div>
                            ${m.estimatedCost || m.estimated_cost ? `<div class="mt-2 fw-semibold text-primary">${fmt(m.estimatedCost || m.estimated_cost)}</div>` : ''}
                        </div>
                    </div>`;
                });
                machHtml += '</div>';
                $('#machinesContainer').html(machHtml);
                $('#machinesCard').show();
            }
        } catch { }
    }
}

// ══════════════════════════════════════════
//  REMARKS
// ══════════════════════════════════════════
function renderRemarks() {
    let hasRemarks = false;
    if (calcData.internalRemarks) {
        $('#txtInternalRemarks').text(calcData.internalRemarks);
        $('#internalRemarksCol').show();
        hasRemarks = true;
    }
    if (calcData.clientRemarks) {
        $('#txtClientRemarks').text(calcData.clientRemarks);
        $('#clientRemarksCol').show();
        hasRemarks = true;
    }
    if (hasRemarks) $('#remarksRow').show();
}

// ══════════════════════════════════════════
//  HELPERS
// ══════════════════════════════════════════
function fmt(val) { return '₹' + (val || 0).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 }); }
function fmtNum(val) { return (val || 0).toLocaleString('en-IN', { minimumFractionDigits: 0, maximumFractionDigits: 2 }); }
function esc(v) { return (v || '').toString().replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;').replace(/'/g, '&#39;'); }
