/* ===== MinePress Store Module — Shared JS Utilities ===== */

window.StoreUtils = (function () {
    'use strict';

    /**
     * Render paging controls for store list pages.
     * Expects the page to have:
     *   #pagingControls  — wrapper card (shown/hidden)
     *   #pagingInfo      — text element for "Showing X to Y of Z"
     *   #paginationNav   — <ul> for pagination buttons
     *
     * @param {number} total        Total filtered items
     * @param {number} totalPages   Total pages
     * @param {number} currentPage  Current page (1-based)
     * @param {number} pageSize     Items per page
     * @param {function} onPageChange  Callback(pageNumber) when user clicks a page
     */
    function renderPaging(total, totalPages, currentPage, pageSize, onPageChange) {
        var $controls = $('#pagingControls');
        var $info = $('#pagingInfo');
        var $nav = $('#paginationNav');

        if (total <= 0) {
            $controls.hide();
            return;
        }

        $controls.show();

        var start = (currentPage - 1) * pageSize + 1;
        var end = Math.min(currentPage * pageSize, total);
        $info.text('Showing ' + start + ' to ' + end + ' of ' + total + ' entries');

        var html = '';

        // Previous
        html += '<li class="page-item ' + (currentPage <= 1 ? 'disabled' : '') + '">';
        html += '<a class="page-link" href="javascript:void(0)" data-page="' + (currentPage - 1) + '">&laquo;</a></li>';

        // Page numbers (show max 7 pages with ellipsis)
        var startPage = Math.max(1, currentPage - 3);
        var endPage = Math.min(totalPages, currentPage + 3);

        if (startPage > 1) {
            html += '<li class="page-item"><a class="page-link" href="javascript:void(0)" data-page="1">1</a></li>';
            if (startPage > 2) {
                html += '<li class="page-item disabled"><span class="page-link">&hellip;</span></li>';
            }
        }

        for (var i = startPage; i <= endPage; i++) {
            html += '<li class="page-item ' + (i === currentPage ? 'active' : '') + '">';
            html += '<a class="page-link" href="javascript:void(0)" data-page="' + i + '">' + i + '</a></li>';
        }

        if (endPage < totalPages) {
            if (endPage < totalPages - 1) {
                html += '<li class="page-item disabled"><span class="page-link">&hellip;</span></li>';
            }
            html += '<li class="page-item"><a class="page-link" href="javascript:void(0)" data-page="' + totalPages + '">' + totalPages + '</a></li>';
        }

        // Next
        html += '<li class="page-item ' + (currentPage >= totalPages ? 'disabled' : '') + '">';
        html += '<a class="page-link" href="javascript:void(0)" data-page="' + (currentPage + 1) + '">&raquo;</a></li>';

        $nav.html(html);

        // Bind click events
        $nav.off('click', '.page-link').on('click', '.page-link', function (e) {
            e.preventDefault();
            var $li = $(this).closest('.page-item');
            if ($li.hasClass('disabled') || $li.hasClass('active')) return;
            var page = parseInt($(this).data('page'));
            if (page >= 1 && page <= totalPages && typeof onPageChange === 'function') {
                onPageChange(page);
            }
        });
    }

    return {
        renderPaging: renderPaging
    };

})();
