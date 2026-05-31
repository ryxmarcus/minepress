/**
 * MinePress ERP — Real-Time Toast Alert System
 * Beautiful modern toast notifications at bottom-right with sound alerts.
 * Polls for new workspace tasks/approvals and displays real-time alerts.
 */
const ToastAlerts = (() => {
    'use strict';

    // ═══════════════════════════════════════════════════════════════
    //  CONFIGURATION
    // ═══════════════════════════════════════════════════════════════
    const CONFIG = {
        pollInterval: 15000,       // Poll every 15 seconds
        toastDuration: 8000,       // Auto-dismiss after 8 seconds
        maxToasts: 5,              // Max visible toasts
        soundEnabled: true,        // Play notification sound
        soundVolume: 0.4,          // Sound volume (0-1)
        apiEndpoint: '/api/workspace/alerts/new'
    };

    // ═══════════════════════════════════════════════════════════════
    //  STATE
    // ═══════════════════════════════════════════════════════════════
    let _pollTimer = null;
    let _lastCheckTime = null;
    let _seenAlertIds = new Set();
    let _activeToasts = [];
    let _soundContext = null;
    let _container = null;

    // ═══════════════════════════════════════════════════════════════
    //  ICONS MAPPING
    // ═══════════════════════════════════════════════════════════════
    const ICONS = {
        TASK: 'bi-list-task',
        APPROVAL: 'bi-shield-check',
        ALERT: 'bi-exclamation-triangle-fill',
        SUCCESS: 'bi-check-circle-fill',
        INFO: 'bi-info-circle-fill',
        WARNING: 'bi-exclamation-circle-fill',
        ERROR: 'bi-x-circle-fill',
        JOB: 'bi-briefcase-fill',
        ENQUIRY: 'bi-clipboard-data-fill',
        QUOTATION: 'bi-file-earmark-text-fill',
        OVERDUE: 'bi-clock-history'
    };

    // ═══════════════════════════════════════════════════════════════
    //  INITIALIZATION
    // ═══════════════════════════════════════════════════════════════
    function init() {
        _createContainer();
        _initSound();
        _loadSeenAlerts();
        _startPolling();

        // Listen for visibility changes to pause/resume polling
        document.addEventListener('visibilitychange', _handleVisibilityChange);

        console.log('[ToastAlerts] Initialized — polling every', CONFIG.pollInterval / 1000, 'seconds');
    }

    function _createContainer() {
        if (document.getElementById('mpToastContainer')) return;

        _container = document.createElement('div');
        _container.id = 'mpToastContainer';
        _container.className = 'mp-toast-container';
        _container.setAttribute('aria-live', 'polite');
        _container.setAttribute('aria-atomic', 'true');
        document.body.appendChild(_container);
    }

    function _initSound() {
        try {
            _soundContext = new (window.AudioContext || window.webkitAudioContext)();
        } catch (e) {
            console.warn('[ToastAlerts] Web Audio API not available');
        }
    }

    function _loadSeenAlerts() {
        try {
            const stored = sessionStorage.getItem('mp_seen_alerts');
            if (stored) {
                const parsed = JSON.parse(stored);
                // Keep only alerts from last 24 hours
                const cutoff = Date.now() - (24 * 60 * 60 * 1000);
                _seenAlertIds = new Set(
                    parsed.filter(item => item.ts > cutoff).map(item => item.id)
                );
            }
        } catch (e) { /* ignore */ }
    }

    function _saveSeenAlerts() {
        try {
            const data = Array.from(_seenAlertIds).map(id => ({ id, ts: Date.now() }));
            sessionStorage.setItem('mp_seen_alerts', JSON.stringify(data.slice(-100)));
        } catch (e) { /* ignore */ }
    }

    // ═══════════════════════════════════════════════════════════════
    //  POLLING
    // ═══════════════════════════════════════════════════════════════
    function _startPolling() {
        _checkForNewAlerts(); // Initial check
        _pollTimer = setInterval(_checkForNewAlerts, CONFIG.pollInterval);
    }

    function _stopPolling() {
        if (_pollTimer) {
            clearInterval(_pollTimer);
            _pollTimer = null;
        }
    }

    function _handleVisibilityChange() {
        if (document.hidden) {
            _stopPolling();
        } else {
            _startPolling();
        }
    }

    async function _checkForNewAlerts() {
        try {
            const since = _lastCheckTime 
                ? `&since=${encodeURIComponent(_lastCheckTime)}`
                : '';
            
            const res = await fetch(`${CONFIG.apiEndpoint}?_=${Date.now()}${since}`, {
                credentials: 'same-origin',
                headers: { 'Accept': 'application/json' }
            });

            if (!res.ok) {
                if (res.status === 401) {
                    _stopPolling();
                    console.log('[ToastAlerts] Session expired, polling stopped');
                }
                return;
            }

            const data = await res.json();
            _lastCheckTime = data.checkTime || new Date().toISOString();

            if (data.alerts && data.alerts.length > 0) {
                _processNewAlerts(data.alerts);
            }
        } catch (e) {
            // Silent fail — network issues shouldn't spam console
        }
    }

    function _processNewAlerts(alerts) {
        const newAlerts = alerts.filter(a => !_seenAlertIds.has(a.id));
        
        if (newAlerts.length === 0) return;

        // Play sound once for batch
        if (CONFIG.soundEnabled && newAlerts.length > 0) {
            const hasCritical = newAlerts.some(a => a.priority === 'CRITICAL' || a.priority === 'URGENT');
            _playNotificationSound(hasCritical ? 'urgent' : 'normal');
        }

        // Show toasts (limit to avoid overwhelming)
        const toShow = newAlerts.slice(0, CONFIG.maxToasts);
        toShow.forEach((alert, index) => {
            setTimeout(() => {
                show(alert);
                _seenAlertIds.add(alert.id);
            }, index * 300); // Stagger appearance
        });

        // If more alerts than we can show, show summary
        if (newAlerts.length > CONFIG.maxToasts) {
            setTimeout(() => {
                show({
                    id: `summary-${Date.now()}`,
                    type: 'info',
                    title: `+${newAlerts.length - CONFIG.maxToasts} more notifications`,
                    message: 'Click to view all in your workspace',
                    actionUrl: '/Workspace/MyTasks'
                });
            }, CONFIG.maxToasts * 300 + 200);
        }

        _saveSeenAlerts();
    }

    // ═══════════════════════════════════════════════════════════════
    //  TOAST DISPLAY
    // ═══════════════════════════════════════════════════════════════
    function show(options) {
        const {
            id = `toast-${Date.now()}`,
            type = 'task',           // task, approval, success, error, warning, info
            priority = 'normal',     // normal, high, urgent, critical
            title = 'New Notification',
            message = '',
            icon = null,
            tag = null,              // TASK, APPROVAL, OVERDUE, etc.
            actionUrl = null,
            actionLabel = null,
            duration = CONFIG.toastDuration,
            jobNo = null,
            partyName = null
        } = options;

        // Remove oldest toast if at max
        while (_activeToasts.length >= CONFIG.maxToasts) {
            const oldest = _activeToasts.shift();
            _dismissToast(oldest.element, false);
        }

        const toast = _createToastElement({
            id, type, priority, title, message, icon, tag,
            actionUrl, actionLabel, jobNo, partyName
        });

        _container.appendChild(toast);
        _activeToasts.push({ id, element: toast });

        // Trigger animation
        requestAnimationFrame(() => {
            toast.classList.add('show');
        });

        // Auto-dismiss with progress bar
        if (duration > 0) {
            const progressBar = toast.querySelector('.mp-toast-progress-bar');
            if (progressBar) {
                progressBar.style.transition = `transform ${duration}ms linear`;
                requestAnimationFrame(() => {
                    progressBar.style.transform = 'scaleX(0)';
                });
            }

            setTimeout(() => {
                _dismissToast(toast);
            }, duration);
        }

        return toast;
    }

    function _createToastElement(opts) {
        const {
            id, type, priority, title, message, icon, tag,
            actionUrl, actionLabel, jobNo, partyName
        } = opts;

        const iconClass = icon || _getIconForType(type, tag);
        const priorityClass = priority !== 'normal' ? `priority-${priority.toLowerCase()}` : '';
        const typeClass = type ? `type-${type.toLowerCase()}` : '';

        const toast = document.createElement('div');
        toast.className = `mp-toast ${priorityClass} ${typeClass}`.trim();
        toast.dataset.toastId = id;
        toast.setAttribute('role', 'alert');

        // Build meta info
        let metaHtml = '';
        if (tag) {
            metaHtml += `<span class="mp-toast-tag ${tag.toLowerCase()}">${_escHtml(tag)}</span>`;
        }
        if (jobNo) {
            metaHtml += `<span><i class="bi bi-briefcase me-1"></i>${_escHtml(jobNo)}</span>`;
        }
        if (partyName) {
            metaHtml += `<span><i class="bi bi-person me-1"></i>${_escHtml(partyName)}</span>`;
        }
        metaHtml += `<span><i class="bi bi-clock me-1"></i>Just now</span>`;

        // Build action button
        let actionHtml = '';
        if (actionUrl) {
            actionHtml = `
                <button class="mp-toast-action" onclick="window.location.href='${_escAttr(actionUrl)}';event.stopPropagation();">
                    <i class="bi bi-arrow-right-circle"></i>
                    ${_escHtml(actionLabel || 'View')}
                </button>`;
        }

        toast.innerHTML = `
            <div class="mp-toast-icon">
                <i class="bi ${iconClass}"></i>
            </div>
            <div class="mp-toast-body">
                <div class="mp-toast-title">${_escHtml(title)}</div>
                ${message ? `<div class="mp-toast-message">${_escHtml(message)}</div>` : ''}
                ${metaHtml ? `<div class="mp-toast-meta">${metaHtml}</div>` : ''}
                ${actionHtml}
            </div>
            <button class="mp-toast-close" aria-label="Dismiss">
                <i class="bi bi-x-lg"></i>
            </button>
            <div class="mp-toast-progress">
                <div class="mp-toast-progress-bar"></div>
            </div>
        `;

        // Click handlers
        toast.addEventListener('click', (e) => {
            if (e.target.closest('.mp-toast-close')) {
                _dismissToast(toast);
                return;
            }
            if (e.target.closest('.mp-toast-action')) {
                return; // Action button handles its own click
            }
            // Click on toast body navigates to action URL
            if (actionUrl) {
                window.location.href = actionUrl;
            }
        });

        // Pause auto-dismiss on hover
        toast.addEventListener('mouseenter', () => {
            const progressBar = toast.querySelector('.mp-toast-progress-bar');
            if (progressBar) {
                progressBar.style.animationPlayState = 'paused';
                progressBar.style.transitionProperty = 'none';
            }
        });

        toast.addEventListener('mouseleave', () => {
            const progressBar = toast.querySelector('.mp-toast-progress-bar');
            if (progressBar) {
                progressBar.style.transitionProperty = 'transform';
            }
        });

        return toast;
    }

    function _dismissToast(toastEl, animate = true) {
        if (!toastEl || !toastEl.parentNode) return;

        if (animate) {
            toastEl.classList.remove('show');
            toastEl.classList.add('hide');
            setTimeout(() => {
                toastEl.remove();
            }, 400);
        } else {
            toastEl.remove();
        }

        _activeToasts = _activeToasts.filter(t => t.element !== toastEl);
    }

    function dismissAll() {
        _activeToasts.forEach(t => _dismissToast(t.element, true));
        _activeToasts = [];
    }

    function _getIconForType(type, tag) {
        if (tag && ICONS[tag.toUpperCase()]) {
            return ICONS[tag.toUpperCase()];
        }
        const typeUpper = (type || 'task').toUpperCase();
        return ICONS[typeUpper] || ICONS.INFO;
    }

    // ═══════════════════════════════════════════════════════════════
    //  SOUND
    // ═══════════════════════════════════════════════════════════════
    function _playNotificationSound(type = 'normal') {
        if (!CONFIG.soundEnabled || !_soundContext) return;

        // Resume context if suspended (browser autoplay policy)
        if (_soundContext.state === 'suspended') {
            _soundContext.resume();
        }

        try {
            const oscillator = _soundContext.createOscillator();
            const gainNode = _soundContext.createGain();

            oscillator.connect(gainNode);
            gainNode.connect(_soundContext.destination);

            gainNode.gain.setValueAtTime(CONFIG.soundVolume, _soundContext.currentTime);

            if (type === 'urgent') {
                // Two-tone urgent sound
                oscillator.frequency.setValueAtTime(880, _soundContext.currentTime);
                oscillator.frequency.setValueAtTime(1100, _soundContext.currentTime + 0.1);
                oscillator.frequency.setValueAtTime(880, _soundContext.currentTime + 0.2);
                gainNode.gain.exponentialRampToValueAtTime(0.01, _soundContext.currentTime + 0.4);
                oscillator.start(_soundContext.currentTime);
                oscillator.stop(_soundContext.currentTime + 0.4);
            } else {
                // Pleasant single chime
                oscillator.frequency.setValueAtTime(659.25, _soundContext.currentTime); // E5
                oscillator.type = 'sine';
                gainNode.gain.exponentialRampToValueAtTime(0.01, _soundContext.currentTime + 0.3);
                oscillator.start(_soundContext.currentTime);
                oscillator.stop(_soundContext.currentTime + 0.3);
            }
        } catch (e) {
            // Silent fail
        }
    }

    function setSoundEnabled(enabled) {
        CONFIG.soundEnabled = enabled;
        try {
            localStorage.setItem('mp_toast_sound', enabled ? '1' : '0');
        } catch (e) { /* ignore */ }
    }

    function isSoundEnabled() {
        try {
            const stored = localStorage.getItem('mp_toast_sound');
            if (stored !== null) {
                CONFIG.soundEnabled = stored === '1';
            }
        } catch (e) { /* ignore */ }
        return CONFIG.soundEnabled;
    }

    // ═══════════════════════════════════════════════════════════════
    //  MANUAL TRIGGER (for testing / programmatic use)
    // ═══════════════════════════════════════════════════════════════
    function showSuccess(title, message, opts = {}) {
        return show({ type: 'success', title, message, icon: ICONS.SUCCESS, ...opts });
    }

    function showError(title, message, opts = {}) {
        return show({ type: 'error', title, message, icon: ICONS.ERROR, ...opts });
    }

    function showWarning(title, message, opts = {}) {
        return show({ type: 'warning', title, message, icon: ICONS.WARNING, ...opts });
    }

    function showInfo(title, message, opts = {}) {
        return show({ type: 'info', title, message, icon: ICONS.INFO, ...opts });
    }

    function showTask(taskData) {
        return show({
            type: 'task',
            tag: 'TASK',
            title: taskData.title,
            message: taskData.description,
            priority: taskData.priority?.toLowerCase() || 'normal',
            actionUrl: taskData.actionUrl || '/Workspace/MyTasks',
            jobNo: taskData.jobNo,
            partyName: taskData.partyName,
            id: `task-${taskData.taskId || Date.now()}`
        });
    }

    function showApproval(approvalData) {
        return show({
            type: 'approval',
            tag: 'APPROVAL',
            title: approvalData.title,
            message: approvalData.description || 'Requires your approval',
            priority: approvalData.priority?.toLowerCase() || 'high',
            actionUrl: approvalData.actionUrl || '/Workspace/Approvals',
            actionLabel: 'Review',
            jobNo: approvalData.jobNo,
            partyName: approvalData.partyName,
            id: `approval-${approvalData.taskId || Date.now()}`
        });
    }

    // ═══════════════════════════════════════════════════════════════
    //  HELPERS
    // ═══════════════════════════════════════════════════════════════
    function _escHtml(str) {
        if (!str) return '';
        const div = document.createElement('div');
        div.textContent = str;
        return div.innerHTML;
    }

    function _escAttr(str) {
        if (!str) return '';
        return str.replace(/"/g, '&quot;').replace(/'/g, '&#39;');
    }

    // ═══════════════════════════════════════════════════════════════
    //  PUBLIC API
    // ═══════════════════════════════════════════════════════════════
    return {
        init,
        show,
        showSuccess,
        showError,
        showWarning,
        showInfo,
        showTask,
        showApproval,
        dismissAll,
        setSoundEnabled,
        isSoundEnabled,
        // Expose for manual testing
        testSound: () => _playNotificationSound('normal'),
        testUrgentSound: () => _playNotificationSound('urgent')
    };
})();

// Auto-init when DOM ready
document.addEventListener('DOMContentLoaded', () => {
    // Only init if user is authenticated (check for presence of user indicator)
    if (document.querySelector('.navbar') || document.querySelector('[data-user-id]')) {
        ToastAlerts.init();
    }
});
