// ===== MinePress Shared: HSN/SAC Tax Code Selector (Advanced) =====
// Usage: HsnSacSelector.init({ apiBase, onSelect, onClear })
// Tabs: ALL | HSN (Goods) | SAC (Services)
// No auto-load on focus — results only appear when user types.

const HsnSacSelector = {
    _timer: null,
    _selected: null,
    _activeType: 'ALL',
    _allResults: [],
    _apiBase: '/api/enquiry',
    _onSelect: null,
    _onClear: null,

    init(options) {
        const self = this;
        if (options && options.apiBase) this._apiBase = options.apiBase;
        if (options && options.onSelect) this._onSelect = options.onSelect;
        if (options && options.onClear) this._onClear = options.onClear;

        // Tab switching — filter cached results without re-fetching
        $(document).on('click', '#hsnSacWidget .hsn-type-tab', function () {
            self._activeType = $(this).data('type');
            $('#hsnSacWidget .hsn-type-tab').removeClass('active');
            $(this).addClass('active');
            if (self._allResults.length > 0) {
                self._renderFiltered();
            }
        });

        // Search input — only fire when user types (≥ 1 char); hide on empty
        $('#hsnSearchInput').on('input', function () {
            const q = $(this).val().trim();
            clearTimeout(self._timer);
            if (q.length === 0) {
                self._allResults = [];
                $('#hsnDropdown').addClass('d-none');
                return;
            }
            self._timer = setTimeout(() => self._fetch(q), 300);
        });

        $('#hsnClearBtn').on('click', () => self.clear());

        // Close dropdown on outside click
        $(document).on('click', function (e) {
            if (!$(e.target).closest('#hsnSacWidget').length) {
                $('#hsnDropdown').addClass('d-none');
            }
        });
    },

    _fetch(q) {
        $('#hsnSearchSpinner').removeClass('d-none');
        $('#hsnTypeHint').addClass('d-none');
        $.get(`${this._apiBase}/hsnsaccodes`, { q }, (data) => {
            $('#hsnSearchSpinner').addClass('d-none');
            this._allResults = data || [];
            this._renderFiltered();
        }).fail(() => {
            $('#hsnSearchSpinner').addClass('d-none');
        });
    },

    _renderFiltered() {
        const filtered = this._activeType === 'ALL'
            ? this._allResults
            : this._allResults.filter(h => (h.codeType || 'HSN').toUpperCase() === this._activeType);

        const container = $('#hsnResults').empty();
        $('#hsnTypeHint').addClass('d-none');

        if (filtered.length === 0) {
            $('#hsnNoResult').removeClass('d-none');
            container.addClass('d-none');
        } else {
            $('#hsnNoResult').addClass('d-none');
            container.removeClass('d-none');

            filtered.forEach(h => {
                const rate = h.defaultGstRate != null ? h.defaultGstRate + '%' : '—';
                const typeLabel = (h.codeType || 'HSN').toUpperCase();
                const typeClass = typeLabel === 'SAC' ? 'sac' : 'hsn';
                container.append(`
                    <div class="hsn-result-item" data-id="${h.id}">
                        <span class="hsn-type-badge ${typeClass}">${HsnSacSelector._esc(typeLabel)}</span>
                        <span class="hsn-result-code">${HsnSacSelector._esc(h.code)}</span>
                        <span class="hsn-result-desc">${HsnSacSelector._esc(h.description)}</span>
                        <span class="hsn-result-rate">${HsnSacSelector._esc(rate)}</span>
                    </div>`);
            });

            container.find('.hsn-result-item').on('click', function () {
                const id = $(this).data('id');
                const item = filtered.find(h => h.id === id);
                if (item) HsnSacSelector.select(item);
            });
        }

        $('#hsnDropdown').removeClass('d-none');
    },

    select(item) {
        this._selected = item;
        const rate = item.defaultGstRate != null ? item.defaultGstRate : 0;
        const typeLabel = (item.codeType || 'HSN').toUpperCase();
        const typeClass = typeLabel === 'SAC' ? 'sac' : 'hsn';

        $('#hsnSelTypeBadge').text(typeLabel).removeClass('hsn sac').addClass(typeClass);
        $('#hsnSelCode').text(item.code);
        $('#hsnSelDesc').text(item.description);
        $('#hsnSelRate').text(rate + '% GST');
        $('#hsnSelectedId').val(item.id);
        $('#hsnSelectedRate').val(rate);

        $('#hsnSelectedBadge').removeClass('d-none');
        $('#hsnSearchInput').val('').prop('disabled', true);
        $('#hsnDropdown').addClass('d-none');

        $(document).trigger('hsnSacSelected', [item]);
        if (this._onSelect) this._onSelect(item);
    },

    clear() {
        this._selected = null;
        this._allResults = [];
        $('#hsnSelectedBadge').addClass('d-none');
        $('#hsnSearchInput').val('').prop('disabled', false);
        $('#hsnSelectedId').val('');
        $('#hsnSelectedRate').val('0');
        $('#hsnDropdown').addClass('d-none');
        $(document).trigger('hsnSacCleared');
        if (this._onClear) this._onClear();
    },

    getSelected() {
        return this._selected;
    },

    _esc(v) {
        return (v || '')
            .replaceAll('&', '&amp;')
            .replaceAll('<', '&lt;')
            .replaceAll('>', '&gt;')
            .replaceAll('"', '&quot;')
            .replaceAll("'", '&#39;');
    }
};

