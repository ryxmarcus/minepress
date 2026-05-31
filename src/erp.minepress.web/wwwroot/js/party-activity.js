/**
 * MinePress Party Portal — Activity Feed JS
 */
const PartyActivity = (() => {
    const API = '/api/PartyPortal';
    let _page = 1;
    const _pageSize = 15;
    let _totalItems = 0;
    let _currentFilter = '';

    // Activity type → icon + CSS class mapping
    const typeMap = {
        ENQUIRY:       { icon: 'bi-envelope-paper',       css: 'pp-at-enquiry' },
        QUOTATION:     { icon: 'bi-file-earmark-ruled',   css: 'pp-at-quotation' },
        JOB:           { icon: 'bi-gear-wide-connected',  css: 'pp-at-job' },
        CHALLAN:       { icon: 'bi-truck',                css: 'pp-at-challan' },
        INVOICE:       { icon: 'bi-receipt',              css: 'pp-at-invoice' },
        RECEIPT:       { icon: 'bi-cash-stack',           css: 'pp-at-receipt' },
        PAYMENT:       { icon: 'bi-credit-card',          css: 'pp-at-payment' },
        PURCHASE:      { icon: 'bi-cart3',                css: 'pp-at-purchase' },
        OUTSOURCE:     { icon: 'bi-box-arrow-up-right',   css: 'pp-at-outsource' },
        PRODUCTION:    { icon: 'bi-cpu',                  css: 'pp-at-production' },
        APPROVAL:      { icon: 'bi-check2-circle',        css: 'pp-at-approval' },
        DOCUMENT:      { icon: 'bi-file-earmark',         css: 'pp-at-document' },
        COMMUNICATION: { icon: 'bi-chat-dots',            css: 'pp-at-communication' },
        VISIT:         { icon: 'bi-geo-alt',              css: 'pp-at-visit' },
        COMPLAINT:     { icon: 'bi-exclamation-triangle',  css: 'pp-at-complaint' }
    };

    function getTypeInfo(type) {
        return typeMap[type] || { icon: 'bi-circle', css: 'pp-at-document' };
    }

    function getStatusBadge(status) {
        if (!status) return '';
        const colors = {
            'Pending': 'bg-yellow-lt text-yellow',
            'Approved': 'bg-green-lt text-green',
            'Rejected': 'bg-red-lt text-red',
            'Completed': 'bg-blue-lt text-blue',
            'Cancelled': 'bg-secondary-lt text-secondary',
            'Draft': 'bg-secondary-lt text-secondary'
        };
        const cls = colors[status] || 'bg-secondary-lt text-secondary';
        return `<span class="badge ${cls}">${status}</span>`;
    }

    // ── Init (called from dashboard page) ──
    function init() {
        loadBell();
        loadTimeline();
        bindEvents();
    }

    function bindEvents() {
        const filter = document.getElementById('pp-activity-filter');
        if (filter) {
            filter.addEventListener('change', () => {
                _currentFilter = filter.value;
                _page = 1;
                loadTimeline();
            });
        }

        const loadMore = document.getElementById('pp-activity-load-more');
        if (loadMore) {
            loadMore.addEventListener('click', () => {
                _page++;
                loadTimeline(true);
            });
        }
    }

    // ── Bell Dropdown ──
    async function loadBell() {
        const list = document.getElementById('pp-bell-list');
        const badge = document.getElementById('pp-bell-badge');
        if (!list) return;

        try {
            const res = await fetch(`${API}/activities/recent?count=10`);
            if (!res.ok) return;
            const data = await res.json();

            // Badge
            if (badge && data.totalCount > 0) {
                badge.textContent = data.totalCount > 99 ? '99+' : data.totalCount;
                badge.classList.remove('d-none');
            }

            // Items
            if (!data.items || data.items.length === 0) {
                list.innerHTML = `
                    <div class="pp-bell-empty">
                        <i class="bi bi-bell-slash"></i>
                        <div>No recent activity</div>
                    </div>`;
                return;
            }

            let html = '';
            data.items.forEach(item => {
                const ti = getTypeInfo(item.activityType);
                html += `
                    <div class="pp-bell-item">
                        <div class="pp-bell-item-icon ${ti.css}">
                            <i class="bi ${ti.icon}"></i>
                        </div>
                        <div class="pp-bell-item-body">
                            <div class="pp-bell-item-title">${escHtml(item.activityTitle || item.activityCode)}</div>
                            <div class="pp-bell-item-desc">${escHtml(item.activityDescription || '')}</div>
                            <div class="pp-bell-item-time">
                                <i class="bi bi-clock me-1"></i>${item.createdOn || ''}
                                ${item.status ? ` · ${item.status}` : ''}
                            </div>
                        </div>
                    </div>`;
            });
            list.innerHTML = html;
        } catch (err) {
            console.error('Bell load error:', err);
        }
    }

    // ── Dashboard Timeline ──
    async function loadTimeline(append) {
        const container = document.getElementById('pp-activity-timeline');
        const moreWrap = document.getElementById('pp-activity-more-wrap');
        const countEl = document.getElementById('pp-activity-count');
        if (!container) return;

        if (!append) {
            container.innerHTML = `
                <div class="text-center text-secondary py-4">
                    <div class="spinner-border spinner-border-sm mb-2"></div>
                    <div class="small">Loading activity...</div>
                </div>`;
        }

        try {
            let url = `${API}/activities?page=${_page}&pageSize=${_pageSize}`;
            if (_currentFilter) url += `&type=${_currentFilter}`;

            const res = await fetch(url);
            if (!res.ok) return;
            const data = await res.json();
            _totalItems = data.total;

            if (countEl) {
                countEl.textContent = `${_totalItems} activit${_totalItems === 1 ? 'y' : 'ies'}`;
            }

            if (!data.items || data.items.length === 0) {
                if (!append) {
                    container.innerHTML = `
                        <div class="pp-activity-empty">
                            <i class="bi bi-activity"></i>
                            <div class="fw-semibold mb-1">No activity yet</div>
                            <div class="small">Activities will appear here as transactions happen.</div>
                        </div>`;
                }
                if (moreWrap) moreWrap.classList.add('d-none');
                return;
            }

            let html = '';
            data.items.forEach(item => {
                const ti = getTypeInfo(item.activityType);
                const amountStr = item.amount != null
                    ? `<span><i class="bi bi-currency-rupee"></i>${Number(item.amount).toLocaleString('en-IN')}</span>`
                    : '';
                const docStr = item.documentNo
                    ? `<span><i class="bi bi-hash"></i>${escHtml(item.documentNo)}</span>`
                    : '';

                html += `
                    <div class="pp-activity-item">
                        <div class="pp-activity-icon ${ti.css}">
                            <i class="bi ${ti.icon}"></i>
                        </div>
                        <div class="pp-activity-body">
                            <div class="pp-activity-header">
                                <div class="pp-activity-title">${escHtml(item.activityTitle || item.activityCode)}</div>
                                <div class="pp-activity-badges">
                                    <span class="badge ${ti.css}" style="font-weight:500">${item.activityType}</span>
                                    ${getStatusBadge(item.status)}
                                </div>
                            </div>
                            ${item.activityDescription ? `<div class="pp-activity-desc">${escHtml(item.activityDescription)}</div>` : ''}
                            <div class="pp-activity-meta">
                                <span><i class="bi bi-clock"></i>${item.createdOn || ''}</span>
                                ${docStr}
                                ${amountStr}
                                ${item.createdBy ? `<span><i class="bi bi-person"></i>${escHtml(item.createdBy)}</span>` : ''}
                            </div>
                        </div>
                    </div>`;
            });

            if (append) {
                container.insertAdjacentHTML('beforeend', html);
            } else {
                container.innerHTML = html;
            }

            // Show/hide load-more
            const loaded = _page * _pageSize;
            if (moreWrap) {
                if (loaded < _totalItems) moreWrap.classList.remove('d-none');
                else moreWrap.classList.add('d-none');
            }
        } catch (err) {
            console.error('Timeline load error:', err);
            if (!append) {
                container.innerHTML = `
                    <div class="pp-activity-empty">
                        <i class="bi bi-exclamation-circle"></i>
                        <div>Failed to load activity</div>
                    </div>`;
            }
        }
    }

    function escHtml(str) {
        if (!str) return '';
        const d = document.createElement('div');
        d.textContent = str;
        return d.innerHTML;
    }

    return {
        init,
        loadBell,
        loadTimeline
    };
})();
