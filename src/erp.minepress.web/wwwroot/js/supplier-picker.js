/* ===== MinePress — Shared Supplier Picker Module ===== */
/* Usage: SupplierPicker.open({ onSelect: fn(supplier) })          */
/* supplier = { id, name, code, address1, email, mobile, gstno, city } */

window.SupplierPicker = (function () {
    'use strict';

    const API = '/api/store/suppliers/search';
    const ModalClass = (typeof minepress !== 'undefined' && minepress.Modal) || (typeof bootstrap !== 'undefined' && bootstrap.Modal);
    let modal = null;
    let currentCallback = null;
    let currentPage = 1, pageSize = 25;
    let sortField = 'name', sortDir = 'asc';
    let searchTimer = null;

    function init() {
        if (modal) return;
        const el = document.getElementById('supplierPickerModal');
        if (!el || !ModalClass) return;
        modal = new ModalClass(el);

        // Search input
        $('#spSearchInput').on('input', function () {
            clearTimeout(searchTimer);
            searchTimer = setTimeout(() => { currentPage = 1; loadSuppliers(); }, 300);
        });

        // Page size
        $('#spPageSize').on('change', function () {
            pageSize = parseInt($(this).val());
            currentPage = 1;
            loadSuppliers();
        });

        // Sorting
        $(document).on('click', '.sp-sortable', function () {
            const field = $(this).data('sort');
            if (sortField === field) {
                sortDir = sortDir === 'asc' ? 'desc' : 'asc';
            } else {
                sortField = field;
                sortDir = 'asc';
            }
            $('.sp-sortable .sp-sort-icon').attr('class', 'bi bi-arrow-down-up sp-sort-icon');
            $(this).find('.sp-sort-icon').attr('class',
                sortDir === 'asc' ? 'bi bi-arrow-up sp-sort-icon text-primary' : 'bi bi-arrow-down sp-sort-icon text-primary');
            loadSuppliers();
        });

        // Select supplier click
        $(document).on('click', '.sp-select-btn', function () {
            const data = $(this).data('supplier');
            if (currentCallback && typeof currentCallback === 'function') {
                currentCallback(data);
            }
            $(this).closest('tr').addClass('sp-row-selected');
            setTimeout(() => { if (modal) modal.hide(); }, 350);
        });

        // Row click to select
        $(document).on('click', '.sp-supplier-row td:not(:last-child)', function () {
            $(this).closest('tr').find('.sp-select-btn').trigger('click');
        });

        // Keyboard: Esc to close
        $('#supplierPickerModal').on('keydown', function (e) {
            if (e.key === 'Escape') modal.hide();
        });

        // Focus search on open
        $('#supplierPickerModal').on('shown.bs.modal', function () {
            $('#spSearchInput').trigger('focus');
        });

        // Clear on close
        $('#supplierPickerModal').on('hidden.bs.modal', function () {
            currentCallback = null;
        });
    }

    function open(options) {
        options = options || {};
        currentCallback = options.onSelect || null;
        currentPage = 1;
        sortField = 'name';
        sortDir = 'asc';
        pageSize = parseInt($('#spPageSize').val()) || 25;
        $('#spSearchInput').val('');
        $('.sp-sortable .sp-sort-icon').attr('class', 'bi bi-arrow-down-up sp-sort-icon');

        init();
        loadSuppliers();
        modal.show();
    }

    function close() {
        if (modal) modal.hide();
    }

    function loadSuppliers() {
        const q = $('#spSearchInput').val();

        $('#spSearchSpinner').removeClass('d-none');
        $('#spTableBody').html('<tr><td colspan="6" class="text-center text-muted py-5"><div class="spinner-border spinner-border-sm me-2"></div>Loading...</td></tr>');

        $.getJSON(API, {
            q: q || '',
            page: currentPage,
            pageSize: pageSize
        })
        .done(function (data) {
            $('#spSearchSpinner').addClass('d-none');
            var items = data.items || data;
            var total = data.totalCount || items.length;
            // Client-side sort
            items = sortItems(items);
            renderTable(items, total);
            renderPaging(total);
        })
        .fail(function () {
            $('#spSearchSpinner').addClass('d-none');
            $('#spTableBody').html('<tr><td colspan="6" class="text-center text-danger py-4"><i class="bi bi-exclamation-circle me-1"></i>Failed to load suppliers.</td></tr>');
            $('#spResultCount').text('');
        });
    }

    function sortItems(items) {
        if (!items || !items.length) return items;
        return items.slice().sort(function (a, b) {
            var av = (a[sortField] || '').toString().toLowerCase();
            var bv = (b[sortField] || '').toString().toLowerCase();
            if (av < bv) return sortDir === 'asc' ? -1 : 1;
            if (av > bv) return sortDir === 'asc' ? 1 : -1;
            return 0;
        });
    }

    function renderTable(items, total) {
        if (!items || !items.length) {
            $('#spTableBody').html(
                '<tr><td colspan="6" class="text-center py-5">' +
                '<div class="sp-empty-state">' +
                '<i class="bi bi-building-slash"></i>' +
                '<div>No suppliers found</div>' +
                '<small class="text-muted">Try a different search term</small>' +
                '</div></td></tr>');
            $('#spResultCount').text('0 suppliers');
            return;
        }

        $('#spResultCount').text(total.toLocaleString() + ' supplier(s)');

        var html = '';
        items.forEach(function (s) {
            var supplierJson = JSON.stringify(s).replace(/"/g, '&quot;');
            html += '<tr class="sp-supplier-row">';
            html += '<td><span class="badge bg-teal-lt fw-normal">' + esc(s.code || '—') + '</span></td>';
            html += '<td>';
            html += '<div class="fw-semibold">' + esc(s.name) + '</div>';
            if (s.email) html += '<small class="text-muted"><i class="bi bi-envelope me-1"></i>' + esc(s.email) + '</small>';
            html += '</td>';
            html += '<td>';
            if (s.gstno) {
                html += '<span class="sp-info-badge bg-indigo-lt">' + esc(s.gstno) + '</span>';
            } else {
                html += '<span class="text-muted small">—</span>';
            }
            html += '</td>';
            html += '<td>';
            if (s.mobile) {
                html += '<i class="bi bi-phone me-1 text-muted"></i>' + esc(s.mobile.toString());
            } else {
                html += '<span class="text-muted small">—</span>';
            }
            html += '</td>';
            html += '<td>';
            if (s.city) {
                html += '<i class="bi bi-geo-alt me-1 text-muted"></i>' + esc(s.city);
            } else {
                html += '<span class="text-muted small">—</span>';
            }
            html += '</td>';
            html += '<td class="text-center">';
            html += '<button class="btn btn-sm btn-success sp-select-btn" data-supplier="' + supplierJson + '" title="Select Supplier">';
            html += '<i class="bi bi-check-lg me-1"></i>Select';
            html += '</button>';
            html += '</td>';
            html += '</tr>';
        });

        $('#spTableBody').html(html);
    }

    function renderPaging(total) {
        var totalPages = Math.max(1, Math.ceil(total / pageSize));
        if (currentPage > totalPages) currentPage = totalPages;

        var start = total === 0 ? 0 : (currentPage - 1) * pageSize + 1;
        var end = Math.min(currentPage * pageSize, total);
        $('#spPagingInfo').text(total > 0 ? start + '–' + end + ' of ' + total.toLocaleString() : '');

        if (totalPages <= 1) {
            $('#spPaginationNav').html('');
            return;
        }

        var html = '';
        // Prev
        html += '<li class="page-item ' + (currentPage <= 1 ? 'disabled' : '') + '">';
        html += '<a class="page-link" href="javascript:void(0)" data-sppage="' + (currentPage - 1) + '">&laquo;</a></li>';

        var startPage = Math.max(1, currentPage - 3);
        var endPage = Math.min(totalPages, currentPage + 3);

        if (startPage > 1) {
            html += '<li class="page-item"><a class="page-link" href="javascript:void(0)" data-sppage="1">1</a></li>';
            if (startPage > 2) html += '<li class="page-item disabled"><span class="page-link">&hellip;</span></li>';
        }

        for (var i = startPage; i <= endPage; i++) {
            html += '<li class="page-item ' + (i === currentPage ? 'active' : '') + '">';
            html += '<a class="page-link" href="javascript:void(0)" data-sppage="' + i + '">' + i + '</a></li>';
        }

        if (endPage < totalPages) {
            if (endPage < totalPages - 1) html += '<li class="page-item disabled"><span class="page-link">&hellip;</span></li>';
            html += '<li class="page-item"><a class="page-link" href="javascript:void(0)" data-sppage="' + totalPages + '">' + totalPages + '</a></li>';
        }

        // Next
        html += '<li class="page-item ' + (currentPage >= totalPages ? 'disabled' : '') + '">';
        html += '<a class="page-link" href="javascript:void(0)" data-sppage="' + (currentPage + 1) + '">&raquo;</a></li>';

        var $nav = $('#spPaginationNav');
        $nav.html(html);
        $nav.off('click', '.page-link').on('click', '.page-link', function (e) {
            e.preventDefault();
            var $li = $(this).closest('.page-item');
            if ($li.hasClass('disabled') || $li.hasClass('active')) return;
            var p = parseInt($(this).data('sppage'));
            if (p >= 1 && p <= totalPages) {
                currentPage = p;
                loadSuppliers();
            }
        });
    }

    function esc(v) {
        return (v || '').toString()
            .replaceAll('&', '&amp;').replaceAll('<', '&lt;')
            .replaceAll('>', '&gt;').replaceAll('"', '&quot;')
            .replaceAll("'", '&#39;');
    }

    return { open: open, close: close };
})();
