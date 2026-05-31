// ===== MinePress Rate Calculator — List Page JS =====

const RC_API = '/api/ratecalculator';
let allCalcs = [];
let currentPage = 1;
let pageSize = 25;
let sortField = 'calcRefNo';
let sortDir = 'desc';

$(document).ready(function () {
    loadCalcList();
    $('#txtSearch, #ddlStatusFilter, #ddlPrintingMode, #ddlLinkedFilter').on('input change', function () { currentPage = 1; applyFilters(); });
    $('#ddlPageSize').on('change', function () { pageSize = parseInt($(this).val()); currentPage = 1; applyFilters(); });
    $(document).on('click', '.sortable', function () {
        const field = $(this).data('sort');
        if (sortField === field) { sortDir = sortDir === 'asc' ? 'desc' : 'asc'; }
        else { sortField = field; sortDir = 'asc'; }
        $('.sortable i').attr('class', 'bi bi-arrow-down-up small text-muted');
        $(this).find('i').attr('class', sortDir === 'asc' ? 'bi bi-arrow-up small text-primary' : 'bi bi-arrow-down small text-primary');
        applyFilters();
    });
});

function loadCalcList() {
    $('#calcListBody').html('<tr><td colspan="10" class="text-center text-muted py-4"><div class="spinner-border spinner-border-sm me-2"></div>Loading calculations...</td></tr>');
    $.get(`${RC_API}/list`, function (data) {
        allCalcs = data;
        updateStats();
        applyFilters();
    }).fail(function () {
        $('#calcListBody').html('<tr><td colspan="10" class="text-center text-danger py-3"><i class="bi bi-exclamation-circle me-1"></i>Failed to load calculations.</td></tr>');
    });
}

function updateStats() {
    $('#statTotal').text(allCalcs.length);
    const linked = allCalcs.filter(c => c.enquiryId || c.quotationId || c.jobId).length;
    $('#statLinked').text(linked);

    const totalValue = allCalcs.reduce((s, c) => s + (c.netTotal || 0), 0);
    $('#statTotalValue').text(fmtShort(totalValue));

    const withQty = allCalcs.filter(c => c.quantity > 0);
    const avgCost = withQty.length > 0 ? withQty.reduce((s, c) => s + (c.costPerUnit || 0), 0) / withQty.length : 0;
    $('#statAvgCost').text(fmtShort(avgCost));
}

function applyFilters() {
    const q = ($('#txtSearch').val() || '').toLowerCase();
    const status = $('#ddlStatusFilter').val();
    const printing = $('#ddlPrintingMode').val();
    const linked = $('#ddlLinkedFilter').val();

    let filtered = allCalcs.filter(c => {
        if (q && !(c.calcRefNo || '').toLowerCase().includes(q) &&
            !(c.partyName || '').toLowerCase().includes(q) &&
            !(c.productTypeName || '').toLowerCase().includes(q) &&
            !(c.jobTypeName || '').toLowerCase().includes(q) &&
            !(c.enquiryNo || '').toLowerCase().includes(q) &&
            !(c.quotationNo || '').toLowerCase().includes(q) &&
            !(c.jobNo || '').toLowerCase().includes(q)) return false;
        if (status && (c.status || '').toUpperCase() !== status) return false;
        if (printing && (c.printingMode || '').toUpperCase() !== printing) return false;
        if (linked === 'enquiry' && !c.enquiryId) return false;
        if (linked === 'quotation' && !c.quotationId) return false;
        if (linked === 'job' && !c.jobId) return false;
        if (linked === 'standalone' && (c.enquiryId || c.quotationId || c.jobId)) return false;
        return true;
    });

    // Sort
    filtered.sort((a, b) => {
        let va = a[sortField], vb = b[sortField];
        if (va == null) va = '';
        if (vb == null) vb = '';
        if (typeof va === 'string') va = va.toLowerCase();
        if (typeof vb === 'string') vb = vb.toLowerCase();
        if (va < vb) return sortDir === 'asc' ? -1 : 1;
        if (va > vb) return sortDir === 'asc' ? 1 : -1;
        return 0;
    });

    const totalItems = filtered.length;
    const totalPages = Math.max(1, Math.ceil(totalItems / pageSize));
    if (currentPage > totalPages) currentPage = totalPages;
    const start = (currentPage - 1) * pageSize;
    const paged = filtered.slice(start, start + pageSize);

    $('#filteredCount').text(filtered.length < allCalcs.length
        ? `Showing ${filtered.length} of ${allCalcs.length}` : '');
    renderTable(paged);
    renderPaging(totalItems, totalPages);
}

function renderPaging(totalItems, totalPages) {
    if (totalItems === 0) { $('#pagingControls').hide(); return; }
    $('#pagingControls').show();
    const start = (currentPage - 1) * pageSize + 1;
    const end = Math.min(currentPage * pageSize, totalItems);
    $('#pagingInfo').text(`Showing ${start} to ${end} of ${totalItems} entries`);

    let navHtml = '';
    navHtml += `<li class="page-item ${currentPage === 1 ? 'disabled' : ''}"><a class="page-link" href="javascript:void(0)" onclick="goToPage(${currentPage - 1})">&laquo;</a></li>`;
    const maxButtons = 5;
    let startPage = Math.max(1, currentPage - Math.floor(maxButtons / 2));
    let endPage = Math.min(totalPages, startPage + maxButtons - 1);
    if (endPage - startPage < maxButtons - 1) startPage = Math.max(1, endPage - maxButtons + 1);
    for (let i = startPage; i <= endPage; i++) {
        navHtml += `<li class="page-item ${i === currentPage ? 'active' : ''}"><a class="page-link" href="javascript:void(0)" onclick="goToPage(${i})">${i}</a></li>`;
    }
    navHtml += `<li class="page-item ${currentPage === totalPages ? 'disabled' : ''}"><a class="page-link" href="javascript:void(0)" onclick="goToPage(${currentPage + 1})">&raquo;</a></li>`;
    $('#paginationNav').html(navHtml);
}

function goToPage(page) {
    currentPage = page;
    applyFilters();
}

function renderTable(data) {
    if (data.length === 0) {
        $('#calcListBody').html('');
        $('#emptyState').show();
        return;
    }
    $('#emptyState').hide();

    let html = '';
    data.forEach(c => {
        const statusClass = {
            'DRAFT': 'rc-status-draft',
            'FINAL': 'rc-status-final',
            'REVISED': 'rc-status-revised',
            'EXPIRED': 'rc-status-expired'
        }[(c.status || '').toUpperCase()] || 'rc-status-draft';

        // Linked badges
        let linkedHtml = '';
        if (c.enquiryNo) linkedHtml += `<a href="/Enquiry/Details?id=${c.enquiryId}" class="rc-link-badge bg-primary-lt text-primary"><i class="bi bi-clipboard-data me-1"></i>${esc(c.enquiryNo)}</a> `;
        if (c.quotationNo) linkedHtml += `<a href="/Quotation/Details?id=${c.quotationId}" class="rc-link-badge bg-success-lt text-success"><i class="bi bi-file-earmark-text me-1"></i>${esc(c.quotationNo)}</a> `;
        if (c.jobNo) linkedHtml += `<a href="/Job/Details?id=${c.jobId}" class="rc-link-badge bg-purple-lt text-purple"><i class="bi bi-briefcase me-1"></i>${esc(c.jobNo)}</a> `;
        if (!linkedHtml) linkedHtml = '<span class="text-muted small">—</span>';

        // Product info
        let productHtml = '';
        if (c.jobTypeName) productHtml += `<span class="badge bg-blue-lt me-1">${esc(c.jobTypeName)}</span>`;
        if (c.productTypeName) productHtml += `<span class="badge bg-purple-lt me-1">${esc(c.productTypeName)}</span>`;
        if (c.productSizeName) productHtml += `<div class="text-muted small mt-1">${esc(c.productSizeName)}${c.printingMode ? ' · ' + esc(c.printingMode) : ''}</div>`;

        html += `<tr>
            <td>
                <a href="/RateCalculator/Details?id=${c.rateCalcId}" class="fw-semibold text-primary text-decoration-none">${esc(c.calcRefNo)}</a>
                ${c.version > 1 ? `<span class="badge bg-secondary-lt ms-1">v${c.version}</span>` : ''}
            </td>
            <td>
                <div class="small">${esc(c.createdOn)}</div>
                <div class="text-muted small">${esc(c.createdBy)}</div>
            </td>
            <td>
                ${c.partyName ? `<div class="fw-semibold">${esc(c.partyName)}</div><div class="text-muted small">${esc(c.partyCode || '')}</div>` : '<span class="text-muted">—</span>'}
            </td>
            <td>${productHtml || '<span class="text-muted">—</span>'}</td>
            <td class="text-center">${c.quantity || 0}</td>
            <td class="text-end fw-semibold">${fmt(c.netTotal)}</td>
            <td class="text-end">${fmt(c.costPerUnit)}</td>
            <td>${linkedHtml}</td>
            <td><span class="rc-status-badge ${statusClass}">${(c.status || 'DRAFT').toUpperCase()}</span></td>
            <td class="text-center">
                <a href="/RateCalculator/Details?id=${c.rateCalcId}" class="btn btn-ghost-primary btn-sm" title="View Details">
                    <i class="bi bi-eye"></i>
                </a>
            </td>
        </tr>`;
    });

    $('#calcListBody').html(html);
}

// Helpers
function fmt(val) { return '₹' + (val || 0).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 }); }
function fmtShort(val) {
    if (val >= 10000000) return '₹' + (val / 10000000).toFixed(1) + 'Cr';
    if (val >= 100000) return '₹' + (val / 100000).toFixed(1) + 'L';
    if (val >= 1000) return '₹' + (val / 1000).toFixed(1) + 'K';
    return '₹' + (val || 0).toFixed(0);
}
function esc(v) { return (v || '').toString().replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;').replace(/'/g, '&#39;'); }
