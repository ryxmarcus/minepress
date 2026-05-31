/* ===== MinePress — Shared HSN/SAC Inline Picker =====
   Usage:
     HsnSacPicker.attach(inputElement, {
         onSelect: function(item, inputEl) { ... },
         onClear:  function(inputEl) { ... },
         apiUrl:   '/api/enquiry/hsnsaccodes'  // optional override
     });
   Each input gets its own independent dropdown instance.
===== */

window.HsnSacPicker = (function () {
    'use strict';

    const DEFAULT_API = '/api/enquiry/hsnsaccodes';
    const DEBOUNCE_MS = 300;
    const instances = new WeakMap();

    function _esc(v) {
        if (!v) return '';
        const d = document.createElement('div');
        d.appendChild(document.createTextNode(v));
        return d.innerHTML;
    }

    function createDropdown(input) {
        const wrap = document.createElement('div');
        wrap.className = 'hsp-wrap';
        input.parentNode.insertBefore(wrap, input);
        wrap.appendChild(input);

        input.classList.add('hsp-input');
        input.setAttribute('autocomplete', 'off');

        // Clear button
        const clearBtn = document.createElement('button');
        clearBtn.type = 'button';
        clearBtn.className = 'hsp-clear';
        clearBtn.tabIndex = -1;
        clearBtn.innerHTML = '<i class="bi bi-x"></i>';
        wrap.appendChild(clearBtn);

        // Dropdown container
        const dd = document.createElement('div');
        dd.className = 'hsp-dropdown';
        wrap.appendChild(dd);

        return { wrap, clearBtn, dd };
    }

    function attach(input, options) {
        if (!(input instanceof HTMLElement)) return;
        if (instances.has(input)) return; // already attached

        const opts = Object.assign({ apiUrl: DEFAULT_API, onSelect: null, onClear: null }, options || {});
        const { wrap, clearBtn, dd } = createDropdown(input);

        let timer = null;
        let results = [];
        let activeIdx = -1;
        let selectedItem = null;

        function doSearch() {
            const q = (input.value || '').trim();
            dd.innerHTML = '<div class="hsp-loading"><i class="bi bi-arrow-repeat"></i> Searching...</div>';
            dd.classList.add('show');

            $.get(opts.apiUrl, { q }, function (data) {
                results = data || [];
                activeIdx = -1;
                renderResults();
            }).fail(function () {
                dd.innerHTML = '<div class="hsp-empty"><i class="bi bi-exclamation-triangle"></i>Failed to load</div>';
            });
        }

        function renderResults() {
            if (results.length === 0) {
                dd.innerHTML = '<div class="hsp-empty"><i class="bi bi-receipt"></i>No matching HSN/SAC codes</div>';
                dd.classList.add('show');
                return;
            }

            let html = '';
            results.forEach(function (h, i) {
                const rate = h.defaultGstRate != null ? h.defaultGstRate + '%' : '—';
                const typeClass = (h.codeType || '').toUpperCase() === 'SAC' ? 'sac' : 'hsn';
                const typeLabel = (h.codeType || 'HSN').toUpperCase();
                const activeClass = i === activeIdx ? ' hsp-active' : '';
                html += '<div class="hsp-item' + activeClass + '" data-idx="' + i + '">'
                    + '<span class="hsp-item-code">' + _esc(h.code) + '</span>'
                    + '<span class="hsp-item-type ' + typeClass + '">' + typeLabel + '</span>'
                    + '<span class="hsp-item-desc">' + _esc(h.description) + '</span>'
                    + '<span class="hsp-item-rate">' + rate + '</span>'
                    + '</div>';
            });
            dd.innerHTML = html;
            dd.classList.add('show');

            // Click handlers on items
            $(dd).find('.hsp-item').on('click', function () {
                var idx = parseInt($(this).data('idx'));
                if (results[idx]) selectItem(results[idx]);
            });
        }

        function selectItem(item) {
            selectedItem = item;
            input.value = item.code;
            input.classList.add('hsp-has-value');
            wrap.setAttribute('data-hsp-desc', item.description + ' (' + (item.defaultGstRate || 0) + '% GST)');
            dd.classList.remove('show');
            results = [];
            activeIdx = -1;

            if (typeof opts.onSelect === 'function') {
                opts.onSelect(item, input);
            }
        }

        function clearSelection() {
            selectedItem = null;
            input.value = '';
            input.classList.remove('hsp-has-value');
            wrap.removeAttribute('data-hsp-desc');
            dd.classList.remove('show');
            results = [];
            activeIdx = -1;
            input.focus();

            if (typeof opts.onClear === 'function') {
                opts.onClear(input);
            }
        }

        // ── Events ──

        $(input).on('input', function () {
            clearTimeout(timer);
            // If user types, remove selected state
            if (selectedItem && input.value !== selectedItem.code) {
                selectedItem = null;
                input.classList.remove('hsp-has-value');
                wrap.removeAttribute('data-hsp-desc');
            }
            timer = setTimeout(doSearch, DEBOUNCE_MS);
        });

        $(input).on('focus', function () {
            if (!selectedItem) {
                clearTimeout(timer);
                timer = setTimeout(doSearch, 100);
            }
        });

        $(input).on('keydown', function (e) {
            if (!dd.classList.contains('show') || results.length === 0) return;

            if (e.key === 'ArrowDown') {
                e.preventDefault();
                activeIdx = Math.min(activeIdx + 1, results.length - 1);
                renderResults();
            } else if (e.key === 'ArrowUp') {
                e.preventDefault();
                activeIdx = Math.max(activeIdx - 1, 0);
                renderResults();
            } else if (e.key === 'Enter') {
                e.preventDefault();
                if (activeIdx >= 0 && results[activeIdx]) {
                    selectItem(results[activeIdx]);
                }
            } else if (e.key === 'Escape') {
                dd.classList.remove('show');
                activeIdx = -1;
            }
        });

        $(clearBtn).on('click', function (e) {
            e.preventDefault();
            e.stopPropagation();
            clearSelection();
        });

        // Close dropdown on outside click
        $(document).on('mousedown', function (e) {
            if (!wrap.contains(e.target)) {
                dd.classList.remove('show');
                activeIdx = -1;
            }
        });

        // Store instance reference
        const instance = {
            getSelected: function () { return selectedItem; },
            clear: clearSelection,
            setCode: function (code, desc, gstRate) {
                input.value = code;
                input.classList.add('hsp-has-value');
                selectedItem = { code: code, description: desc || '', defaultGstRate: gstRate || 0 };
                wrap.setAttribute('data-hsp-desc', (desc || '') + ' (' + (gstRate || 0) + '% GST)');
            },
            destroy: function () {
                clearTimeout(timer);
                $(input).off('input focus keydown');
                $(clearBtn).off('click');
                dd.remove();
                clearBtn.remove();
                input.classList.remove('hsp-input', 'hsp-has-value');
                wrap.parentNode.insertBefore(input, wrap);
                wrap.remove();
                instances.delete(input);
            }
        };

        instances.set(input, instance);
        return instance;
    }

    function getInstance(input) {
        return instances.get(input) || null;
    }

    return { attach: attach, getInstance: getInstance };
})();
