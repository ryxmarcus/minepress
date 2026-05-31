window.SaveProgress = (function () {
    var root = null;
    var steps = [];
    var currentIndex = -1;

    function ensureDom() {
        if (root) return;

        root = document.createElement('div');
        root.className = 'mp-save-progress-overlay';
        root.innerHTML = '' +
            '<div class="mp-save-progress-card" role="status" aria-live="polite">' +
            '  <div class="mp-save-progress-head">' +
            '    <div class="mp-save-progress-icon"><i class="bi bi-stars"></i></div>' +
            '    <div>' +
            '      <div class="mp-save-progress-title" id="mpSaveProgressTitle">Saving...</div>' +
            '      <div class="mp-save-progress-subtitle" id="mpSaveProgressSubtitle">Please wait while we process your request.</div>' +
            '    </div>' +
            '  </div>' +
            '  <div class="mp-save-progress-track">' +
            '    <div class="mp-save-progress-fill" id="mpSaveProgressFill"></div>' +
            '  </div>' +
            '  <div class="mp-save-progress-steps" id="mpSaveProgressSteps"></div>' +
            '  <div class="mp-save-progress-foot" id="mpSaveProgressFoot">Initializing...</div>' +
            '</div>';

        document.body.appendChild(root);
    }

    function esc(v) {
        return (v || '').toString()
            .replaceAll('&', '&amp;')
            .replaceAll('<', '&lt;')
            .replaceAll('>', '&gt;')
            .replaceAll('"', '&quot;')
            .replaceAll("'", '&#39;');
    }

    function renderSteps() {
        var wrap = document.getElementById('mpSaveProgressSteps');
        if (!wrap) return;

        wrap.innerHTML = steps.map(function (s, i) {
            var cls = 'pending';
            var icon = 'bi-circle';

            if (i < currentIndex) {
                cls = 'done';
                icon = 'bi-check-circle-fill';
            } else if (i === currentIndex) {
                cls = 'active';
                icon = 'bi-arrow-repeat';
            }

            return '' +
                '<div class="mp-save-progress-step ' + cls + '">' +
                '  <div class="mp-save-progress-step-icon"><i class="bi ' + icon + '"></i></div>' +
                '  <div class="mp-save-progress-step-body">' +
                '    <div class="mp-save-progress-step-title">' + esc(s.title || ('Step ' + (i + 1))) + '</div>' +
                (s.detail ? '<div class="mp-save-progress-step-detail">' + esc(s.detail) + '</div>' : '') +
                '  </div>' +
                '</div>';
        }).join('');

        updatePercent();
    }

    function updatePercent() {
        var fill = document.getElementById('mpSaveProgressFill');
        if (!fill) return;

        var total = steps.length || 1;
        var doneCount = currentIndex < 0 ? 0 : currentIndex;
        var pct = Math.max(5, Math.min(100, Math.round((doneCount / total) * 100)));
        fill.style.width = pct + '%';
    }

    function setText(id, value, fallback) {
        var el = document.getElementById(id);
        if (!el) return;
        el.textContent = value || fallback || '';
    }

    return {
        start: function (opts) {
            ensureDom();
            opts = opts || {};
            steps = Array.isArray(opts.steps) ? opts.steps.slice() : [];
            currentIndex = opts.startStepIndex != null ? opts.startStepIndex : 0;

            setText('mpSaveProgressTitle', opts.title, 'Saving...');
            setText('mpSaveProgressSubtitle', opts.subtitle, 'Please wait while we process your request.');
            setText('mpSaveProgressFoot', opts.message, 'Initializing...');

            root.classList.remove('is-error');
            root.classList.add('is-open');
            renderSteps();
        },

        setStep: function (index, message) {
            if (!root) return;
            currentIndex = Math.max(0, Math.min(index, Math.max(steps.length - 1, 0)));
            if (message) {
                setText('mpSaveProgressFoot', message, 'Working...');
            }
            renderSteps();
        },

        setMessage: function (message) {
            if (!root) return;
            setText('mpSaveProgressFoot', message, 'Working...');
        },

        complete: function (message) {
            if (!root) return;
            currentIndex = steps.length;
            updatePercent();
            renderSteps();
            setText('mpSaveProgressFoot', message, 'Completed successfully.');
        },

        error: function (message) {
            if (!root) return;
            root.classList.add('is-error');
            setText('mpSaveProgressFoot', message, 'Something went wrong.');
        },

        close: function () {
            if (!root) return;
            root.classList.remove('is-open');
        }
    };
})();