/* ===== MinePress — Shared Job Picker Module ===== */
/* Usage: JobPicker.open({ onSelect: fn(job) })                                    */
/* job = { jobId, jobNo, productName, quantity, statusCode, rateCalcId,            */
/*         priority, progressPercent, jobDate, deliveryDate, customerName }         */

window.JobPicker = (function () {
    'use strict';

    const API = '/api/store/jobs/search';
    const ModalClass = (typeof minepress !== 'undefined' && minepress.Modal) || (typeof bootstrap !== 'undefined' && bootstrap.Modal);
    let modal = null;
    let currentCallback = null;
    let currentPage = 1, pageSize = 25;
    let sortField = 'jobNo', sortDir = 'desc';
    let searchTimer = null;

    function init() {
        if (modal) return;
        var el = document.getElementById('jobPickerModal');
        if (!el || !ModalClass) return;
        modal = new ModalClass(el);

        // Search input
        $('#jpSearchInput').on('input', function () {
            clearTimeout(searchTimer);
            searchTimer = setTimeout(function () { currentPage = 1; loadJobs(); }, 300);
        });

        // Status filter
        $('#jpStatusFilter').on('change', function () { currentPage = 1; loadJobs(); });

        // Page size
        $('#jpPageSize').on('change', function () {
            pageSize = parseInt($(this).val());
            currentPage = 1;
            loadJobs();
        });

        // Sorting
        $(document).on('click', '.jp-sortable', function () {
            var field = $(this).data('sort');
            if (sortField === field) {
                sortDir = sortDir === 'asc' ? 'desc' : 'asc';
            } else {
                sortField = field;
                sortDir = 'asc';
            }
            $('.jp-sortable .jp-sort-icon').attr('class', 'bi bi-arrow-down-up jp-sort-icon');
            $(this).find('.jp-sort-icon').attr('class',
                sortDir === 'asc' ? 'bi bi-arrow-up jp-sort-icon text-primary' : 'bi bi-arrow-down jp-sort-icon text-primary');
            loadJobs();
        });

        // Select job click
        $(document).on('click', '.jp-select-btn', function () {
            var data = $(this).data('job');
            if (currentCallback && typeof currentCallback === 'function') {
                currentCallback(data);
            }
            $(this).closest('tr').addClass('jp-row-selected');
            setTimeout(function () { if (modal) modal.hide(); }, 350);
        });

        // Row click to select
        $(document).on('click', '.jp-job-row td:not(:last-child)', function () {
            $(this).closest('tr').find('.jp-select-btn').trigger('click');
        });

        // Keyboard: Esc to close
        $('#jobPickerModal').on('keydown', function (e) {
            if (e.key === 'Escape') modal.hide();
        });

        // Focus search on open
        $('#jobPickerModal').on('shown.bs.modal', function () {
            $('#jpSearchInput').trigger('focus');
        });

        // Clear on close
        $('#jobPickerModal').on('hidden.bs.modal', function () {
            currentCallback = null;
        });
    }

    function open(options) {
        options = options || {};
        currentCallback = options.onSelect || null;
        currentPage = 1;
        sortField = 'jobNo';
        sortDir = 'desc';
        pageSize = parseInt($('#jpPageSize').val()) || 25;
        $('#jpSearchInput').val('');
        $('#jpStatusFilter').val('');
        $('.jp-sortable .jp-sort-icon').attr('class', 'bi bi-arrow-down-up jp-sort-icon');

        init();
        loadJobs();
        modal.show();
    }

    function close() {
        if (modal) modal.hide();
    }

    function loadJobs() {
        var q = $('#jpSearchInput').val();
        var status = $('#jpStatusFilter').val();

        $('#jpSearchSpinner').removeClass('d-none');
        $('#jpTableBody').html('<tr><td colspan="8" class="text-center text-muted py-5"><div class="spinner-border spinner-border-sm me-2"></div>Loading...</td></tr>');

        $.getJSON(API, {
            q: q || '',
            status: status || '',
            page: currentPage,
            pageSize: pageSize
        })
        .done(function (data) {
            $('#jpSearchSpinner').addClass('d-none');
            var items = data.items || data;
            var total = data.totalCount || items.length;
            // Client-side sort
            items = sortItems(items);
            renderTable(items, total);
            renderPaging(total);
        })
        .fail(function () {
            $('#jpSearchSpinner').addClass('d-none');
            $('#jpTableBody').html('<tr><td colspan="8" class="text-center text-danger py-4"><i class="bi bi-exclamation-circle me-1"></i>Failed to load jobs.</td></tr>');
            $('#jpResultCount').text('');
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

    function getStatusBadge(status) {
        var map = {
            'OPEN':        'bg-blue-lt',
            'IN_PROGRESS': 'bg-cyan-lt',
            'COMPLETED':   'bg-green-lt',
            'DELIVERED':   'bg-teal-lt',
            'CLOSED':      'bg-secondary-lt',
            'CANCELLED':   'bg-red-lt',
            'ON_HOLD':     'bg-yellow-lt'
        };
        var cls = map[(status || '').toUpperCase()] || 'bg-secondary-lt';
        return '<span class="jp-status-badge badge ' + cls + '">' + esc(status || '—') + '</span>';
    }

    function getProgressBar(pct) {
        var p = parseInt(pct) || 0;
        var color = p >= 80 ? '#2fb344' : p >= 40 ? '#f76707' : '#206bc4';
        return '<div class="jp-progress" title="' + p + '%">' +
               '<div class="jp-progress-bar" style="width:' + p + '%;background:' + color + ';"></div>' +
               '</div>' +
               '<small class="text-muted" style="font-size:.7rem;">' + p + '%</small>';
    }

    function renderTable(items, total) {
        if (!items || !items.length) {
            $('#jpTableBody').html(
                '<tr><td colspan="8" class="text-center py-5">' +
                '<div class="jp-empty-state">' +
                '<i class="bi bi-briefcase"></i>' +
                '<div>No jobs found</div>' +
                '<small class="text-muted">Try a different search term or filter</small>' +
                '</div></td></tr>');
            $('#jpResultCount').text('0 jobs');
            return;
        }

        $('#jpResultCount').text(total.toLocaleString() + ' job(s)');

        var html = '';
        items.forEach(function (j) {
            var jobJson = JSON.stringify(j).replace(/"/g, '&quot;');
            html += '<tr class="jp-job-row">';
            html += '<td><span class="badge bg-purple-lt fw-semibold">' + esc(j.jobNo) + '</span></td>';
            html += '<td>';
            html += '<div class="fw-semibold">' + esc(j.productName || '—') + '</div>';
            if (j.priority) {
                var prCls = (j.priority || '').toUpperCase() === 'HIGH' ? 'bg-danger-lt' :
                            (j.priority || '').toUpperCase() === 'URGENT' ? 'bg-red-lt' :
                            (j.priority || '').toUpperCase() === 'MEDIUM' ? 'bg-warning-lt' : 'bg-secondary-lt';
                html += '<span class="jp-priority-badge badge ' + prCls + '">' + esc(j.priority) + '</span>';
            }
            html += '</td>';
            html += '<td class="small">';
            if (j.customerName) {
                html += '<i class="bi bi-person me-1 text-muted"></i>' + esc(j.customerName);
            } else {
                html += '<span class="text-muted">—</span>';
            }
            html += '</td>';
            html += '<td class="text-center fw-semibold">' + (j.quantity || 0).toLocaleString() + '</td>';
            html += '<td class="text-center">' + getStatusBadge(j.statusCode) + '</td>';
            html += '<td class="text-center">' + getProgressBar(j.progressPercent) + '</td>';
            html += '<td class="small text-muted">' + esc(j.jobDate || '') + '</td>';
            html += '<td class="text-center">';
            html += '<button class="btn btn-sm btn-purple jp-select-btn" data-job="' + jobJson + '" title="Select Job">';
            html += '<i class="bi bi-check-lg me-1"></i>Select';
            html += '</button>';
            html += '</td>';
            html += '</tr>';
        });

        $('#jpTableBody').html(html);
    }

    function renderPaging(total) {
        var totalPages = Math.max(1, Math.ceil(total / pageSize));
        if (currentPage > totalPages) currentPage = totalPages;

        var start = total === 0 ? 0 : (currentPage - 1) * pageSize + 1;
        var end = Math.min(currentPage * pageSize, total);
        $('#jpPagingInfo').text(total > 0 ? start + '–' + end + ' of ' + total.toLocaleString() : '');

        if (totalPages <= 1) {
            $('#jpPaginationNav').html('');
            return;
        }

        var html = '';
        // Prev
        html += '<li class="page-item ' + (currentPage <= 1 ? 'disabled' : '') + '">';
        html += '<a class="page-link" href="javascript:void(0)" data-jppage="' + (currentPage - 1) + '">&laquo;</a></li>';

        var startPage = Math.max(1, currentPage - 3);
        var endPage = Math.min(totalPages, currentPage + 3);

        if (startPage > 1) {
            html += '<li class="page-item"><a class="page-link" href="javascript:void(0)" data-jppage="1">1</a></li>';
            if (startPage > 2) html += '<li class="page-item disabled"><span class="page-link">&hellip;</span></li>';
        }

        for (var i = startPage; i <= endPage; i++) {
            html += '<li class="page-item ' + (i === currentPage ? 'active' : '') + '">';
            html += '<a class="page-link" href="javascript:void(0)" data-jppage="' + i + '">' + i + '</a></li>';
        }

        if (endPage < totalPages) {
            if (endPage < totalPages - 1) html += '<li class="page-item disabled"><span class="page-link">&hellip;</span></li>';
            html += '<li class="page-item"><a class="page-link" href="javascript:void(0)" data-jppage="' + totalPages + '">' + totalPages + '</a></li>';
        }

        // Next
        html += '<li class="page-item ' + (currentPage >= totalPages ? 'disabled' : '') + '">';
        html += '<a class="page-link" href="javascript:void(0)" data-jppage="' + (currentPage + 1) + '">&raquo;</a></li>';

        var $nav = $('#jpPaginationNav');
        $nav.html(html);
        $nav.off('click', '.page-link').on('click', '.page-link', function (e) {
            e.preventDefault();
            var $li = $(this).closest('.page-item');
            if ($li.hasClass('disabled') || $li.hasClass('active')) return;
            var p = parseInt($(this).data('jppage'));
            if (p >= 1 && p <= totalPages) {
                currentPage = p;
                loadJobs();
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
