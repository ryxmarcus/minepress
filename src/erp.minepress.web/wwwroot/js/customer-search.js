// ===== MinePress — Shared Customer Search Component =====
// Usage: CustomerSearch.init({ apiBase, onSelect, onClear })
// Depends on: jQuery, _CustomerSearchPartial.cshtml

const CustomerSearch = {
    _timer: null,
    _page: 1,
    _total: 0,
    _pageSize: 10,
    _selected: null,
    _apiBase: '/api/enquiry',
    _onSelect: null,
    _onClear: null,

    init(options) {
        options = options || {};
        if (options.apiBase) this._apiBase = options.apiBase;
        if (options.onSelect) this._onSelect = options.onSelect;
        if (options.onClear) this._onClear = options.onClear;

        const self = this;
        $('#csSearchInput').on('input', function () {
            clearTimeout(self._timer);
            self._timer = setTimeout(() => { self._page = 1; self.search(); }, 300);
        });
        $('#csSearchInput').on('focus', function () {
            if ($('#csSelectedBadge').hasClass('d-none')) {
                self._page = 1;
                self.search();
            }
        });
        $('#csClearBtn').on('click', () => self.clear());

        $(document).on('click', function (e) {
            if (!$(e.target).closest('#customerSearchWidget').length) {
                $('#csDropdown').addClass('d-none');
            }
        });

        $('#csLoadMoreBtn').on('click', function () {
            self._page++;
            self.search(true);
        });
    },

    search(append) {
        const q = ($('#csSearchInput').val() || '').trim();
        $('#csSearchSpinner').removeClass('d-none');

        $.get(`${this._apiBase}/customers/search`, { q, page: this._page, pageSize: this._pageSize }, (data) => {
            $('#csSearchSpinner').addClass('d-none');
            this._total = data.total;

            const container = $('#csResults');
            if (!append) container.empty();

            if (data.items.length === 0 && !append) {
                $('#csNoResult').removeClass('d-none');
                $('#csResults').addClass('d-none');
            } else {
                $('#csNoResult').addClass('d-none');
                $('#csResults').removeClass('d-none');

                data.items.forEach(c => {
                    const initials = (c.name || '?').substring(0, 2).toUpperCase();
                    const sub = [c.code, c.gstno ? 'GST: ' + c.gstno : '', c.mobile].filter(Boolean).join(' · ');
                    container.append(`
                        <div class="cs-result-item" data-id="${c.partyId}">
                            <span class="cs-avatar">${this._esc(initials)}</span>
                            <div class="flex-fill">
                                <div class="cs-result-name">${this._esc(c.name)}</div>
                                <div class="cs-result-sub">${this._esc(sub)}</div>
                            </div>
                        </div>`);
                });

                container.find('.cs-result-item').off('click').on('click', function () {
                    const id = $(this).data('id');
                    const cust = data.items.find(c => c.partyId === id);
                    if (cust) CustomerSearch.select(cust);
                });
            }

            const loaded = container.children().length;
            if (loaded < this._total) {
                $('#csLoadMore').removeClass('d-none');
            } else {
                $('#csLoadMore').addClass('d-none');
            }

            $('#csDropdown').removeClass('d-none');
        }).fail(() => {
            $('#csSearchSpinner').addClass('d-none');
        });
    },

    select(cust) {
        this._selected = cust;
        const initials = (cust.name || '?').substring(0, 2).toUpperCase();

        $('#csSelAvatar').text(initials);
        $('#csSelName').text(cust.name);
        $('#csSelCode').text(cust.code || '');
        $('#csSelGst').text(cust.gstno ? 'GST: ' + cust.gstno : '');
        $('#csSelEmail').text(cust.email || '-');
        $('#csSelMobile').text(cust.mobile || '-');
        $('#csPartyId').val(cust.partyId);

        $('#csSelectedBadge').removeClass('d-none');
        $('#csSearchInput').val('').prop('disabled', true);
        $('#csDropdown').addClass('d-none');

        if (this._onSelect) this._onSelect(cust);
    },

    selectById(id) {
        if (!id) return;
        $.get(`${this._apiBase}/customers/${id}`, (cust) => {
            if (cust) this.select(cust);
        });
    },

    getSelectedId() {
        return this._selected ? this._selected.partyId : (parseInt($('#csPartyId').val()) || 0);
    },

    getSelected() {
        return this._selected;
    },

    clear() {
        this._selected = null;
        $('#csSelectedBadge').addClass('d-none');
        $('#csSearchInput').val('').prop('disabled', false).focus();
        $('#csPartyId').val('');

        if (this._onClear) this._onClear();
    },

    _esc(v) {
        return (v || '').replaceAll('&', '&amp;').replaceAll('<', '&lt;').replaceAll('>', '&gt;').replaceAll('"', '&quot;').replaceAll("'", '&#39;');
    }
};
