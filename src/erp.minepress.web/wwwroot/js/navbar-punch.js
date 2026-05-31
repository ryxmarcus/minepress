/**
 * MinePress ERP — Navbar Punch In/Out
 * Renders a compact punch button in the top navbar on every page.
 */
const NavbarPunch = (() => {
    const API = '/api/dashboard';
    let _state = 'NOT_PUNCHED'; // NOT_PUNCHED | PUNCHED_IN | PUNCHED_OUT
    let _timerInterval = null;
    let _workSeconds = 0;
    let _checkInTime = null;

    function $(id) { return document.getElementById(id); }

    function getCsrf() {
        const el = document.querySelector('input[name="__RequestVerificationToken"]');
        return el ? el.value : '';
    }

    // ── Timer ──────────────────────────────────────────────────────────────

    function startTimer() {
        if (_timerInterval) return;
        _timerInterval = setInterval(() => {
            if (_checkInTime) {
                _workSeconds = Math.floor((Date.now() - _checkInTime.getTime()) / 1000);
            } else {
                _workSeconds++;
            }
            renderTimer();
        }, 1000);
    }

    function stopTimer() {
        if (_timerInterval) { clearInterval(_timerInterval); _timerInterval = null; }
    }

    function renderTimer() {
        const el = $('npTimer');
        if (!el) return;
        const h = Math.floor(_workSeconds / 3600);
        const m = Math.floor((_workSeconds % 3600) / 60);
        const s = _workSeconds % 60;
        el.textContent = `${String(h).padStart(2,'0')}:${String(m).padStart(2,'0')}:${String(s).padStart(2,'0')}`;
    }

    // ── Render state into navbar widget ───────────────────────────────────

    function renderState() {
        const btn     = $('npPunchBtn');
        const icon    = $('npPunchIcon');
        const label   = $('npPunchLabel');
        const badge   = $('npStatusDot');
        const timerEl = $('npTimer');

        if (!btn) return;

        if (_state === 'NOT_PUNCHED') {
            btn.className   = 'btn btn-sm btn-outline-success d-flex align-items-center gap-1';
            if (icon)  icon.className  = 'bi bi-box-arrow-in-right';
            if (label) label.textContent = 'Punch In';
            if (badge) { badge.className = 'np-dot bg-warning'; badge.title = 'Not punched in'; }
            if (timerEl) timerEl.style.display = 'none';
            stopTimer();
            btn.disabled = false;
        } else if (_state === 'PUNCHED_IN') {
            btn.className   = 'btn btn-sm btn-outline-danger d-flex align-items-center gap-1';
            if (icon)  icon.className  = 'bi bi-box-arrow-right';
            if (label) label.textContent = 'Punch Out';
            if (badge) { badge.className = 'np-dot bg-success np-dot-pulse'; badge.title = 'Checked in'; }
            if (timerEl) timerEl.style.display = '';
            renderTimer();
            startTimer();
            btn.disabled = false;
        } else {
            // PUNCHED_OUT — day complete
            btn.className   = 'btn btn-sm btn-outline-secondary d-flex align-items-center gap-1';
            if (icon)  icon.className  = 'bi bi-check-circle-fill';
            if (label) label.textContent = 'Day Done';
            if (badge) { badge.className = 'np-dot bg-primary'; badge.title = 'Day completed'; }
            if (timerEl) timerEl.style.display = '';
            stopTimer();
            btn.disabled = true;
        }
    }

    // ── Load status from API ──────────────────────────────────────────────

    async function loadStatus() {
        try {
            const res = await fetch(`${API}/punch-status`);
            const data = res.ok ? await res.json() : null;
            if (!data || !data.hasEmployee) {
                showNoEmployee();
                return;
            }

            _state       = data.punchState   || 'NOT_PUNCHED';
            _workSeconds = data.workSeconds  || 0;
            _checkInTime = data.checkIn ? new Date(data.checkIn) : null;

            show();
            renderState();
        } catch {
            showNoEmployee();
        }
    }

    function hide() {
        const wrap = $('npWrap');
        if (wrap) wrap.style.display = 'none';
    }

    function show() {
        const wrap = $('npWrap');
        if (wrap) wrap.style.display = 'flex';
    }

    function showNoEmployee() {
        show();
        const btn   = $('npPunchBtn');
        const icon  = $('npPunchIcon');
        const label = $('npPunchLabel');
        const badge = $('npStatusDot');
        if (btn)   { btn.className = 'btn btn-sm btn-outline-secondary d-flex align-items-center gap-1'; btn.disabled = true; }
        if (icon)  icon.className  = 'bi bi-exclamation-circle';
        if (label) label.textContent = 'No Record';
        if (badge) { badge.className = 'np-dot bg-danger'; badge.title = 'Employee record not found'; }
        stopTimer();
    }

    // ── Punch action ──────────────────────────────────────────────────────

    async function doPunch() {
        const btn = $('npPunchBtn');
        if (btn) btn.disabled = true;

        const endpoint    = _state === 'NOT_PUNCHED' ? 'punch-in' : 'punch-out';
        const actionLabel = _state === 'NOT_PUNCHED' ? 'Punch In'  : 'Punch Out';

        try {
            const res = await fetch(`${API}/${endpoint}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': getCsrf()
                }
            });
            const data = await res.json();

            if (res.ok) {
                if (typeof Swal !== 'undefined') {
                    Swal.fire({
                        icon: 'success',
                        title: `${actionLabel} Successful`,
                        text: data.message,
                        timer: 2000,
                        showConfirmButton: false,
                        toast: true,
                        position: 'top-end'
                    });
                }
                await loadStatus();
                // Also refresh full dashboard widget if on that page
                if (typeof Dashboard !== 'undefined' && typeof Dashboard.refresh === 'function') {
                    Dashboard.refresh();
                }
            } else {
                if (typeof Swal !== 'undefined') {
                    Swal.fire({ icon: 'error', title: 'Error', text: data.message || 'Something went wrong.' });
                }
                if (btn) btn.disabled = false;
            }
        } catch {
            if (typeof Swal !== 'undefined') {
                Swal.fire({ icon: 'error', title: 'Network Error', text: 'Could not connect to server.' });
            }
            if (btn) btn.disabled = false;
        }
    }

    // ── Init ──────────────────────────────────────────────────────────────

    function init() {
        const btn = $('npPunchBtn');
        if (!btn) return;
        if (!btn.dataset.npBound) {
            btn.dataset.npBound = '1';
            btn.addEventListener('click', doPunch);
        }
        loadStatus();
    }

    return { init, doPunch };
})();

document.addEventListener('DOMContentLoaded', () => NavbarPunch.init());
