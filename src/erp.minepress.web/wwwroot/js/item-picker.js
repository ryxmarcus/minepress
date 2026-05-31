/* ===== MinePress — Shared Item Picker Module ===== */
/* Usage: ItemPicker.open({ onSelect: fn(item), group: 'PAPER' }) */

window.ItemPicker = (function () {
    'use strict';

    const API = '/api/store/items/search';
    const ModalClass = (typeof minepress !== 'undefined' && minepress.Modal) || (typeof bootstrap !== 'undefined' && bootstrap.Modal);
    let modal = null;
    let currentCallback = null;
    let currentPage = 1, pageSize = 25;
    let sortField = 'itemName', sortDir = 'asc';
    let searchTimer = null;
    let groupsLoaded = false;

    function init() {
        if (modal) return;
        const el = document.getElementById('itemPickerModal');
        if (!el || !ModalClass) return;
        modal = new ModalClass(el);

        // Search
        $('#ipSearchInput').on('input', function () {
            clearTimeout(searchTimer);
            const q = $(this).val();
            searchTimer = setTimeout(() => { currentPage = 1; loadItems(); }, 300);
        });

        // Group filter
        $('#ipGroupFilter').on('change', function () { currentPage = 1; loadItems(); });

        // Page size
        $('#ipPageSize').on('change', function () {
            pageSize = parseInt($(this).val());
            currentPage = 1;
            loadItems();
        });

        // Sorting
        $(document).on('click', '.ip-sortable', function () {
            const field = $(this).data('sort');
            if (sortField === field) {
                sortDir = sortDir === 'asc' ? 'desc' : 'asc';
            } else {
                sortField = field;
                sortDir = 'asc';
            }
            $('.ip-sortable .ip-sort-icon').attr('class', 'bi bi-arrow-down-up ip-sort-icon');
            $(this).find('.ip-sort-icon').attr('class',
                sortDir === 'asc' ? 'bi bi-arrow-up ip-sort-icon text-primary' : 'bi bi-arrow-down ip-sort-icon text-primary');
            loadItems();
        });

        // Add item click
        $(document).on('click', '.ip-add-btn', function () {
            const data = $(this).data('item');
            if (currentCallback && typeof currentCallback === 'function') {
                currentCallback(data);
            }
            // Flash row then close modal
            $(this).closest('tr').addClass('ip-row-added');
            setTimeout(() => { if (modal) modal.hide(); }, 350);
        });

        // Keyboard: Esc to close
        $('#itemPickerModal').on('keydown', function (e) {
            if (e.key === 'Escape') modal.hide();
        });

        // Focus search on open
        $('#itemPickerModal').on('shown.bs.modal', function () {
            $('#ipSearchInput').trigger('focus');
        });

        // Clear on close
        $('#itemPickerModal').on('hidden.bs.modal', function () {
            currentCallback = null;
        });
    }

    function open(options) {
        options = options || {};
        currentCallback = options.onSelect || null;
        currentPage = 1;
        sortField = 'itemName';
        sortDir = 'asc';
        pageSize = parseInt($('#ipPageSize').val()) || 25;
        $('#ipSearchInput').val('');
        $('.ip-sortable .ip-sort-icon').attr('class', 'bi bi-arrow-down-up ip-sort-icon');

        init();

        // Pre-select group if provided
        if (options.group) {
            loadGroups(function () {
                $('#ipGroupFilter').val(options.group);
                loadItems();
            });
        } else {
            loadGroups();
            loadItems();
        }

        modal.show();
    }

    function close() {
        if (modal) modal.hide();
    }

    function loadGroups(callback) {
        if (groupsLoaded) {
            if (callback) callback();
            return;
        }
        // Groups come with first data load — we'll populate from the API response
        if (callback) callback();
    }

    function populateGroups(groups) {
        if (groupsLoaded) return;
        const $sel = $('#ipGroupFilter');
        const currentVal = $sel.val();
        $sel.find('option:not(:first)').remove();
        (groups || []).forEach(g => {
            $sel.append(`<option value="${esc(g)}">${esc(g)}</option>`);
        });
        if (currentVal) $sel.val(currentVal);
        groupsLoaded = true;
    }

    function loadItems() {
        const group = $('#ipGroupFilter').val();
        const q = $('#ipSearchInput').val();

        $('#ipSearchSpinner').removeClass('d-none');
        $('#ipTableBody').html('<tr><td colspan="8" class="text-center text-muted py-5"><div class="spinner-border spinner-border-sm me-2"></div>Loading...</td></tr>');

        $.getJSON(API, {
            group: group || '',
            q: q || '',
            page: currentPage,
            pageSize: pageSize,
            sortField: sortField,
            sortDir: sortDir
        })
        .done(function (data) {
            $('#ipSearchSpinner').addClass('d-none');
            populateGroups(data.groups);
            renderTable(data.items, data.totalCount);
            renderPaging(data.totalCount);
        })
        .fail(function () {
            $('#ipSearchSpinner').addClass('d-none');
            $('#ipTableBody').html('<tr><td colspan="8" class="text-center text-danger py-4"><i class="bi bi-exclamation-circle me-1"></i>Failed to load items.</td></tr>');
            $('#ipResultCount').text('');
        });
    }

    function renderTable(items, total) {
        if (!items || !items.length) {
            $('#ipTableBody').html(`
                <tr><td colspan="8" class="text-center py-5">
                    <div class="ip-empty-state">
                        <i class="bi bi-search"></i>
                        <div>No items found</div>
                        <small class="text-muted">Try a different search term or filter</small>
                    </div>
                </td></tr>`);
            $('#ipResultCount').text('0 items');
            return;
        }

        $('#ipResultCount').text(`${total.toLocaleString()} item(s)`);

        let html = '';
        items.forEach(item => {
            const stockClass = item.currentStock <= 0 ? 'text-danger' :
                               item.currentStock <= item.reorderLevel ? 'text-warning' : 'text-success';
            const itemJson = JSON.stringify(item).replace(/"/g, '&quot;');

            html += `<tr class="ip-item-row">
                <td><span class="badge bg-azure-lt fw-normal">${esc(item.itemCode)}</span></td>
                <td>
                    <div class="fw-semibold">${esc(item.itemName)}</div>
                    ${item.itemCategory ? `<small class="text-muted">${esc(item.itemCategory)}</small>` : ''}
                </td>
                <td><span class="badge bg-secondary-lt">${esc(item.itemGroup)}</span></td>
                <td class="text-center">${esc(item.uom)}</td>
                <td class="text-end fw-semibold ${stockClass}">${item.currentStock.toLocaleString('en-IN')}</td>
                <td class="text-end">${item.purchaseRate.toLocaleString('en-IN', { minimumFractionDigits: 2 })}</td>
                <td class="small text-muted">${esc(item.hsnCode)}</td>
                <td class="text-center">
                    <button class="btn btn-sm btn-primary ip-add-btn" data-item="${itemJson}" title="Add Item">
                        <i class="bi bi-plus-lg me-1"></i>Add
                    </button>
                </td>
            </tr>`;
        });

        $('#ipTableBody').html(html);
    }

    function renderPaging(total) {
        const totalPages = Math.max(1, Math.ceil(total / pageSize));
        if (currentPage > totalPages) currentPage = totalPages;

        const start = total === 0 ? 0 : (currentPage - 1) * pageSize + 1;
        const end = Math.min(currentPage * pageSize, total);
        $('#ipPagingInfo').text(total > 0 ? `${start}–${end} of ${total.toLocaleString()}` : '');

        if (totalPages <= 1) {
            $('#ipPaginationNav').html('');
            return;
        }

        let html = '';
        // Prev
        html += `<li class="page-item ${currentPage <= 1 ? 'disabled' : ''}">
            <a class="page-link" href="javascript:void(0)" data-ippage="${currentPage - 1}">&laquo;</a></li>`;

        const startPage = Math.max(1, currentPage - 3);
        const endPage = Math.min(totalPages, currentPage + 3);

        if (startPage > 1) {
            html += `<li class="page-item"><a class="page-link" href="javascript:void(0)" data-ippage="1">1</a></li>`;
            if (startPage > 2) html += `<li class="page-item disabled"><span class="page-link">&hellip;</span></li>`;
        }

        for (let i = startPage; i <= endPage; i++) {
            html += `<li class="page-item ${i === currentPage ? 'active' : ''}">
                <a class="page-link" href="javascript:void(0)" data-ippage="${i}">${i}</a></li>`;
        }

        if (endPage < totalPages) {
            if (endPage < totalPages - 1) html += `<li class="page-item disabled"><span class="page-link">&hellip;</span></li>`;
            html += `<li class="page-item"><a class="page-link" href="javascript:void(0)" data-ippage="${totalPages}">${totalPages}</a></li>`;
        }

        // Next
        html += `<li class="page-item ${currentPage >= totalPages ? 'disabled' : ''}">
            <a class="page-link" href="javascript:void(0)" data-ippage="${currentPage + 1}">&raquo;</a></li>`;

        const $nav = $('#ipPaginationNav');
        $nav.html(html);
        $nav.off('click', '.page-link').on('click', '.page-link', function (e) {
            e.preventDefault();
            const $li = $(this).closest('.page-item');
            if ($li.hasClass('disabled') || $li.hasClass('active')) return;
            const p = parseInt($(this).data('ippage'));
            if (p >= 1 && p <= totalPages) {
                currentPage = p;
                loadItems();
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
