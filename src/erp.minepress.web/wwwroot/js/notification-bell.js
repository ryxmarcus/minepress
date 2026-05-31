/**
 * MinePress ERP — Notification Bell System
 * Handles fetching, rendering, filtering, mark-read, dismiss, and polling.
 */
const NotificationBell = (() => {
    'use strict';

    const API = {
        list: '/api/notification/list',
        unreadCount: '/api/notification/unread-count',
        markRead: (id) => `/api/notification/mark-read/${id}`,
        markAllRead: '/api/notification/mark-all-read',
        dismiss: (id) => `/api/notification/dismiss/${id}`
    };

    const POLL_INTERVAL = 30000; // 30 seconds
    let _pollTimer = null;
    let _notifications = [];
    let _currentFilter = 'all'; // all | unread | action

    // ── Color Mapping ──
    const colorMap = {
        primary: 'bg-primary-lt',
        success: 'bg-success-lt',
        warning: 'bg-warning-lt',
        danger: 'bg-danger-lt',
        info: 'bg-info-lt',
        secondary: 'bg-secondary-lt',
        red: 'bg-danger-lt',
        green: 'bg-success-lt',
        blue: 'bg-primary-lt',
        orange: 'bg-warning-lt',
        azure: 'bg-info-lt'
    };

    const defaultIcons = {
        ENQUIRY: 'bi bi-clipboard-data',
        RATE_CALC: 'bi bi-calculator',
        AUTH: 'bi bi-shield-lock',
        JOB: 'bi bi-briefcase',
        QUOTATION: 'bi bi-file-earmark-text',
        SYSTEM: 'bi bi-gear'
    };

    // ── Initialize ──
    function init() {
        _bindEvents();
        loadNotifications();
        loadUnreadCount();
        _startPolling();
    }

    // ── Fetch Notifications ──
    async function loadNotifications() {
        const listEl = document.getElementById('notifList');
        if (!listEl) return;

        listEl.innerHTML = `
            <div class="notif-loading">
                <div class="spinner-border text-primary" role="status"></div>
                <div class="text-muted small mt-2">Loading...</div>
            </div>`;

        try {
            const res = await fetch(API.list);
            if (!res.ok) throw new Error('Failed to fetch');
            _notifications = await res.json();
            _renderList();
            _updateFilterCounts();
        } catch {
            listEl.innerHTML = `
                <div class="notif-empty">
                    <div class="notif-empty-icon"><i class="bi bi-wifi-off"></i></div>
                    <div class="notif-empty-text">Failed to load notifications</div>
                    <div class="notif-empty-sub">Please try again later</div>
                </div>`;
        }
    }

    // ── Fetch Unread Count (lightweight) ──
    async function loadUnreadCount() {
        try {
            const res = await fetch(API.unreadCount);
            if (!res.ok) return;
            const data = await res.json();
            _updateBadge(data.count);
        } catch { /* silent */ }
    }

    // ── Render List ──
    function _renderList() {
        const listEl = document.getElementById('notifList');
        if (!listEl) return;

        const filtered = _getFiltered();

        if (filtered.length === 0) {
            const msgs = {
                all: { icon: 'bi-bell-slash', text: 'No notifications yet', sub: 'You\'re all caught up!' },
                unread: { icon: 'bi-check-circle', text: 'All read!', sub: 'No unread notifications' },
                action: { icon: 'bi-check2-all', text: 'No actions pending', sub: 'Nothing requires your attention' }
            };
            const m = msgs[_currentFilter] || msgs.all;
            listEl.innerHTML = `
                <div class="notif-empty">
                    <div class="notif-empty-icon"><i class="bi ${m.icon}"></i></div>
                    <div class="notif-empty-text">${m.text}</div>
                    <div class="notif-empty-sub">${m.sub}</div>
                </div>`;
            return;
        }

        listEl.innerHTML = filtered.map(n => _renderItem(n)).join('');
    }

    function _renderItem(n) {
        const isUnread = !n.isRead;
        const icon = n.icon || defaultIcons[n.module] || 'bi bi-bell';
        const colorClass = colorMap[n.color] || colorMap[n.module?.toLowerCase()] || 'bg-primary-lt';
        const priorityClass = n.priority === 'HIGH' ? 'notif-priority-high' : n.priority === 'URGENT' ? 'notif-priority-urgent' : '';
        const timeAgo = _timeAgo(n.createdOn);

        return `
        <div class="notif-item ${isUnread ? 'notif-unread' : ''} ${priorityClass}"
             data-id="${n.userNotificationId}" data-url="${n.referenceUrl || ''}">
            <div class="notif-icon ${colorClass}">
                <i class="${icon}"></i>
            </div>
            <div class="notif-body">
                <div class="notif-title">${_escHtml(n.title)}</div>
                <div class="notif-message">${_escHtml(n.message)}</div>
                <div class="notif-meta">
                    ${n.module ? `<span class="notif-module-tag">${n.module}</span>` : ''}
                    <span><i class="bi bi-clock me-1"></i>${timeAgo}</span>
                    ${n.aiGenerated ? '<span title="AI Generated"><i class="bi bi-stars text-warning"></i></span>' : ''}
                </div>
                ${n.actionRequired && n.actionUrl ? `
                    <a href="${n.actionUrl}" class="notif-action-badge" onclick="event.stopPropagation();">
                        <i class="bi bi-lightning-charge-fill"></i> ${_escHtml(n.actionLabel || 'Take Action')}
                    </a>` : ''}
            </div>
            <div class="notif-actions">
                ${isUnread ? `<button class="notif-action-btn" title="Mark as read" onclick="NotificationBell.markRead(${n.userNotificationId}, event)"><i class="bi bi-check2"></i></button>` : ''}
                <button class="notif-action-btn dismiss-btn" title="Dismiss" onclick="NotificationBell.dismiss(${n.userNotificationId}, event)"><i class="bi bi-x-lg"></i></button>
            </div>
        </div>`;
    }

    // ── Filter ──
    function _getFiltered() {
        switch (_currentFilter) {
            case 'unread': return _notifications.filter(n => !n.isRead);
            case 'action': return _notifications.filter(n => n.actionRequired);
            default: return _notifications;
        }
    }

    function _updateFilterCounts() {
        const allCount = _notifications.length;
        const unreadCount = _notifications.filter(n => !n.isRead).length;
        const actionCount = _notifications.filter(n => n.actionRequired).length;

        _setText('notifCountAll', allCount);
        _setText('notifCountUnread', unreadCount);
        _setText('notifCountAction', actionCount);
        _updateBadge(unreadCount);
    }

    function setFilter(filter) {
        _currentFilter = filter;
        document.querySelectorAll('.notif-filter-tab').forEach(t => {
            t.classList.toggle('active', t.dataset.filter === filter);
        });
        _renderList();
    }

    // ── Actions ──
    async function markRead(id, event) {
        if (event) { event.stopPropagation(); event.preventDefault(); }
        try {
            await fetch(API.markRead(id), { method: 'POST' });
            const n = _notifications.find(x => x.userNotificationId === id);
            if (n) n.isRead = true;
            _renderList();
            _updateFilterCounts();
        } catch { /* silent */ }
    }

    async function markAllRead() {
        try {
            await fetch(API.markAllRead, { method: 'POST' });
            _notifications.forEach(n => n.isRead = true);
            _renderList();
            _updateFilterCounts();
        } catch { /* silent */ }
    }

    async function dismiss(id, event) {
        if (event) { event.stopPropagation(); event.preventDefault(); }
        try {
            await fetch(API.dismiss(id), { method: 'POST' });
            _notifications = _notifications.filter(x => x.userNotificationId !== id);
            _renderList();
            _updateFilterCounts();
        } catch { /* silent */ }
    }

    // ── Click Handler (navigate) ──
    function _bindEvents() {
        // Item click → mark read + navigate
        document.addEventListener('click', (e) => {
            const item = e.target.closest('.notif-item');
            if (!item) return;
            if (e.target.closest('.notif-action-btn') || e.target.closest('.notif-action-badge')) return;

            const id = parseInt(item.dataset.id);
            const url = item.dataset.url;

            if (id) markRead(id);
            if (url) window.location.href = url;
        });

        // Filter tab clicks
        document.querySelectorAll('.notif-filter-tab').forEach(tab => {
            tab.addEventListener('click', () => setFilter(tab.dataset.filter));
        });

        // Refresh when dropdown opens
        const bellDropdown = document.getElementById('notifBellDropdown');
        if (bellDropdown) {
            bellDropdown.addEventListener('show.bs.dropdown', () => loadNotifications());
        }
    }

    // ── Polling ──
    function _startPolling() {
        _pollTimer = setInterval(() => loadUnreadCount(), POLL_INTERVAL);
    }

    // ── Helpers ──
    function _updateBadge(count) {
        const badge = document.getElementById('notifBadge');
        if (!badge) return;
        badge.textContent = count > 99 ? '99+' : (count > 0 ? count : '');
        badge.dataset.count = count;
        badge.style.display = count > 0 ? '' : 'none';

        // Ring animation
        const bell = document.getElementById('notifBellIcon');
        if (bell && count > 0) {
            bell.classList.add('notif-bell-ring');
            setTimeout(() => bell.classList.remove('notif-bell-ring'), 700);
        }
    }

    function _setText(id, val) {
        const el = document.getElementById(id);
        if (el) el.textContent = val;
    }

    function _timeAgo(isoStr) {
        if (!isoStr) return '';
        const date = new Date(isoStr);
        const now = new Date();
        const diffMs = now - date;
        const diffMin = Math.floor(diffMs / 60000);
        const diffHr = Math.floor(diffMs / 3600000);
        const diffDay = Math.floor(diffMs / 86400000);

        if (diffMin < 1) return 'Just now';
        if (diffMin < 60) return `${diffMin}m ago`;
        if (diffHr < 24) return `${diffHr}h ago`;
        if (diffDay < 7) return `${diffDay}d ago`;
        return date.toLocaleDateString('en-IN', { day: '2-digit', month: 'short' });
    }

    function _escHtml(str) {
        if (!str) return '';
        const div = document.createElement('div');
        div.textContent = str;
        return div.innerHTML;
    }

    // ── Public API ──
    return {
        init,
        loadNotifications,
        loadUnreadCount,
        markRead,
        markAllRead,
        dismiss,
        setFilter
    };
})();

// Auto-init when DOM ready
document.addEventListener('DOMContentLoaded', () => NotificationBell.init());
