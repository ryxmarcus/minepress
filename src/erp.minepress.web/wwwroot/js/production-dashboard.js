/**
 * Production Dashboard — Real-time stats, charts, activity feeds
 * IIFE module for the Production Command Center
 */
const ProdDashboard = (function () {
    'use strict';

    let refreshTimer = null;
    const REFRESH_INTERVAL = 60000; // 1 minute

    // ── Init ───────────────────────────────────────────────────
    function init() {
        loadDashboard();
        // Auto-refresh every minute
        refreshTimer = setInterval(loadDashboard, REFRESH_INTERVAL);
    }

    // ── Load Dashboard Data ────────────────────────────────────
    async function loadDashboard() {
        try {
            const data = await $.get('/api/production/dashboard-stats');
            renderKPIs(data);
            renderDonutChart(data.machines);
            renderBreakdownBars(data.breakdowns.byCategory);
            renderSeverityPanel(data.breakdowns.bySeverity);
            renderMaintenanceOverview(data.maintenance);
            renderBreakdownStats(data.breakdowns);
            renderRecentBreakdowns(data.breakdowns.recent);
            renderRecentAllocations(data.recentAllocations);
            renderUpcomingDeliveries(data.upcomingDeliveries);
            pulseIndicator();
        } catch (e) {
            console.error('Dashboard load failed', e);
        }
    }

    // ── KPI Cards ──────────────────────────────────────────────
    function renderKPIs(data) {
        const m = data.machines;
        const j = data.jobs;
        const b = data.breakdowns;

        animateCounter('dbTotalMachines', m.total);
        animateCounter('dbActiveJobs', j.totalActive);
        animateCounter('dbOpenBreakdowns', b.open);
        $('#dbEfficiency').text(data.efficiencyPercent);

        $('#dbRunning').text(m.running);
        $('#dbIdle').text(m.idle);
        $('#dbDown').text(m.breakdown + m.maintenance);

        $('#dbUrgentJobs').text(j.urgent);
        $('#dbUnallocated').text(j.unallocated);

        $('#dbAvgDowntime').text(b.avgDowntimeMinutes);
        $('#dbResolvedMonth').text(b.resolvedThisMonth);

        $('#dbManpower').text(data.manpower.totalMapped);
        $('#dbTodayAlloc').text(j.todayAllocations);

        // KPI bar segments
        var total = m.total || 1;
        $('#dbBarRunning').css('width', (m.running / total * 100) + '%');
        $('#dbBarBreakdown').css('width', (m.breakdown / total * 100) + '%');
        $('#dbBarMaintenance').css('width', (m.maintenance / total * 100) + '%');
    }

    // ── Donut Chart ────────────────────────────────────────────
    function renderDonutChart(machines) {
        const total = machines.total || 1;
        const circumference = 2 * Math.PI * 52; // ~326.73
        const segments = [
            { id: 'dbDonutRunning', value: machines.running, label: 'dbDonutRunningLabel' },
            { id: 'dbDonutBreakdown', value: machines.breakdown, label: 'dbDonutBreakdownLabel' },
            { id: 'dbDonutMaintenance', value: machines.maintenance, label: 'dbDonutMaintenanceLabel' },
            { id: 'dbDonutIdle', value: machines.idle, label: 'dbDonutIdleLabel' }
        ];

        let offset = -circumference / 4; // start at 12 o'clock

        segments.forEach(seg => {
            const pct = seg.value / total;
            const dashLen = pct * circumference;
            const el = document.getElementById(seg.id);
            if (el) {
                el.setAttribute('stroke-dasharray', `${dashLen} ${circumference - dashLen}`);
                el.setAttribute('stroke-dashoffset', `${-offset}`);
            }
            offset += dashLen;
            $(`#${seg.label}`).text(seg.value);
        });

        $('#dbDonutTotal').text(machines.total);
    }

    // ── Breakdown Category Bars ────────────────────────────────
    function renderBreakdownBars(byCategory) {
        const $el = $('#dbBreakdownBars');
        $el.empty();

        if (!byCategory || byCategory.length === 0) {
            $el.html('<div class="prod-empty"><i class="bi bi-check-circle"></i>No breakdowns recorded</div>');
            return;
        }

        const maxVal = Math.max(...byCategory.map(c => c.count));
        const colors = {
            'Mechanical': 'var(--tblr-danger)',
            'Electrical': 'var(--tblr-warning)',
            'Software': 'var(--tblr-info)',
            'Operator Error': 'var(--tblr-orange)'
        };

        byCategory.forEach(item => {
            const pct = maxVal > 0 ? (item.count / maxVal * 100) : 0;
            const color = colors[item.category] || 'var(--tblr-primary)';
            $el.append(`
                <div class="prod-hbar-item">
                    <div class="prod-hbar-label">${escHtml(item.category)}</div>
                    <div class="prod-hbar-track">
                        <div class="prod-hbar-fill" style="width:${pct}%;background:${color}"></div>
                    </div>
                    <div class="prod-hbar-value">${item.count}</div>
                </div>`);
        });
    }

    // ── Severity Panel ─────────────────────────────────────────
    function renderSeverityPanel(bySeverity) {
        const $el = $('#dbSeverityPanel');
        $el.empty();

        if (!bySeverity || bySeverity.length === 0) {
            $el.html('<div class="d-flex align-items-center gap-2 py-2"><i class="bi bi-shield-check text-success"></i><span class="text-muted small">No active breakdowns</span></div>');
            return;
        }

        const sevConfig = {
            'Critical': { bg: 'bg-danger', icon: 'bi-exclamation-octagon-fill', cls: 'text-danger' },
            'High': { bg: 'bg-orange', icon: 'bi-exclamation-triangle-fill', cls: 'text-orange' },
            'Medium': { bg: 'bg-warning', icon: 'bi-exclamation-circle-fill', cls: 'text-warning' },
            'Low': { bg: 'bg-info', icon: 'bi-info-circle-fill', cls: 'text-info' }
        };

        const ordered = ['Critical', 'High', 'Medium', 'Low'];
        ordered.forEach(sev => {
            const item = bySeverity.find(s => s.severity === sev);
            const count = item ? item.count : 0;
            if (count === 0) return;
            const cfg = sevConfig[sev] || { bg: 'bg-muted', icon: 'bi-circle', cls: 'text-muted' };
            $el.append(`
                <div class="d-flex align-items-center gap-2 py-2 ${count > 0 ? '' : 'opacity-50'}">
                    <div class="prod-severity-dot ${cfg.bg}"></div>
                    <div class="flex-fill">
                        <div class="d-flex align-items-center justify-content-between">
                            <span class="fw-medium small">${sev}</span>
                            <span class="fw-bold ${cfg.cls}">${count}</span>
                        </div>
                        <div class="prod-severity-bar-track">
                            <div class="prod-severity-bar-fill ${cfg.bg}" style="width:${Math.min(count * 20, 100)}%"></div>
                        </div>
                    </div>
                </div>`);
        });
    }

    // ── Maintenance Overview ───────────────────────────────────
    function renderMaintenanceOverview(maint) {
        $('#dbMaintDue').text(maint.pendingDue);
        $('#dbMaintCompleted').text(maint.completedThisMonth);
        $('#dbMaintCost').text('₹' + formatNumber(maint.totalCost));
    }

    // ── Breakdown Stats Footer ─────────────────────────────────
    function renderBreakdownStats(breakdowns) {
        $('#dbRepairCost').text('₹' + formatNumber(breakdowns.totalRepairCost));
        $('#dbAvgDowntime2').text(breakdowns.avgDowntimeMinutes + ' min');
        $('#dbResolvedCount').text(breakdowns.resolvedThisMonth);
    }

    // ── Recent Breakdowns Feed ─────────────────────────────────
    function renderRecentBreakdowns(items) {
        const $el = $('#dbRecentBreakdowns');
        $el.empty();

        if (!items || items.length === 0) {
            $el.html('<div class="prod-empty p-3"><i class="bi bi-check-circle"></i>No recent breakdowns</div>');
            return;
        }

        items.forEach(b => {
            const sevBadge = getSeverityBadge(b.severityLevel);
            const statusBadge = getStatusBadge(b.breakdownStatus);
            const timeAgo = b.createdOn ? getTimeAgo(new Date(b.createdOn)) : '';
            $el.append(`
                <div class="list-group-item list-group-item-action py-2 px-3">
                    <div class="d-flex align-items-center gap-2">
                        <div class="prod-feed-icon bg-danger-lt"><i class="bi bi-lightning-charge text-danger"></i></div>
                        <div class="flex-fill" style="min-width:0">
                            <div class="d-flex align-items-center gap-1">
                                <span class="fw-semibold small text-truncate">${escHtml(b.machineName)}</span>
                                ${sevBadge}
                            </div>
                            <div class="text-muted" style="font-size:.7rem">
                                ${escHtml(b.faultCategory || '—')}
                                ${b.downtimeMinutes ? ' · ' + b.downtimeMinutes + ' min' : ''}
                            </div>
                        </div>
                        <div class="text-end" style="min-width:60px">
                            ${statusBadge}
                            <div class="text-muted" style="font-size:.62rem">${timeAgo}</div>
                        </div>
                    </div>
                </div>`);
        });
    }

    // ── Recent Allocations Feed ────────────────────────────────
    function renderRecentAllocations(items) {
        const $el = $('#dbRecentAllocations');
        $el.empty();

        if (!items || items.length === 0) {
            $el.html('<div class="prod-empty p-3"><i class="bi bi-inbox"></i>No recent allocations</div>');
            return;
        }

        items.forEach(a => {
            const timeAgo = a.createdOn ? getTimeAgo(new Date(a.createdOn)) : '';
            $el.append(`
                <div class="list-group-item list-group-item-action py-2 px-3">
                    <div class="d-flex align-items-center gap-2">
                        <div class="prod-feed-icon bg-primary-lt"><i class="bi bi-kanban text-primary"></i></div>
                        <div class="flex-fill" style="min-width:0">
                            <div class="fw-semibold small">${escHtml(a.jobNo)}</div>
                            <div class="text-muted" style="font-size:.7rem"><i class="bi bi-gear me-1"></i>${escHtml(a.machineName || '—')}</div>
                        </div>
                        <div class="text-end" style="min-width:60px">
                            <span class="badge bg-green-lt text-green" style="font-size:.6rem">${escHtml(a.allocationStatus || 'ALLOCATED')}</span>
                            <div class="text-muted" style="font-size:.62rem">${timeAgo}</div>
                        </div>
                    </div>
                </div>`);
        });
    }

    // ── Upcoming Deliveries Feed ───────────────────────────────
    function renderUpcomingDeliveries(items) {
        const $el = $('#dbUpcomingDeliveries');
        $el.empty();

        if (!items || items.length === 0) {
            $el.html('<div class="prod-empty p-3"><i class="bi bi-calendar-check"></i>No upcoming deliveries</div>');
            return;
        }

        items.forEach(d => {
            const dDate = d.deliveryDate ? new Date(d.deliveryDate).toLocaleDateString('en-IN', { day: '2-digit', month: 'short' }) : '—';
            const daysLeft = d.deliveryDate ? Math.ceil((new Date(d.deliveryDate) - new Date()) / 86400000) : null;
            const urgencyBadge = daysLeft !== null && daysLeft <= 1
                ? '<span class="badge bg-danger-lt text-danger ms-1" style="font-size:.55rem">TODAY</span>'
                : daysLeft !== null && daysLeft <= 2
                    ? '<span class="badge bg-warning-lt text-warning ms-1" style="font-size:.55rem">SOON</span>'
                    : '';
            const priBadge = d.priority === 'Urgent' || d.priority === 'Critical'
                ? `<span class="badge bg-danger-lt text-danger ms-1" style="font-size:.55rem">${escHtml(d.priority)}</span>`
                : '';
            $el.append(`
                <div class="list-group-item list-group-item-action py-2 px-3">
                    <div class="d-flex align-items-center gap-2">
                        <div class="prod-feed-icon bg-cyan-lt"><i class="bi bi-box-seam text-cyan"></i></div>
                        <div class="flex-fill" style="min-width:0">
                            <div class="d-flex align-items-center gap-1">
                                <span class="fw-semibold small">${escHtml(d.jobNo)}</span>
                                ${priBadge}${urgencyBadge}
                            </div>
                            <div class="text-muted text-truncate" style="font-size:.7rem">${escHtml(d.productName || '—')} · ${escHtml(d.partyName || '—')}</div>
                        </div>
                        <div class="text-end" style="min-width:55px">
                            <div class="fw-semibold small text-cyan">${dDate}</div>
                            ${daysLeft !== null ? `<div class="text-muted" style="font-size:.62rem">${daysLeft}d left</div>` : ''}
                        </div>
                    </div>
                </div>`);
        });
    }

    // ── Helpers ─────────────────────────────────────────────────
    function getSeverityBadge(severity) {
        if (!severity) return '';
        const cfg = {
            'Critical': 'bg-danger-lt text-danger',
            'High': 'bg-orange-lt text-orange',
            'Medium': 'bg-warning-lt text-warning',
            'Low': 'bg-info-lt text-info'
        };
        return `<span class="badge ${cfg[severity] || 'bg-muted-lt text-muted'}" style="font-size:.55rem">${escHtml(severity)}</span>`;
    }

    function getStatusBadge(status) {
        if (!status) return '';
        const cfg = {
            'Open': 'bg-danger-lt text-danger',
            'Assigned': 'bg-blue-lt text-blue',
            'In Progress': 'bg-warning-lt text-warning',
            'Resolved': 'bg-success-lt text-success',
            'Closed': 'bg-muted-lt text-muted'
        };
        return `<span class="badge ${cfg[status] || 'bg-muted-lt text-muted'}" style="font-size:.55rem">${escHtml(status)}</span>`;
    }

    function getTimeAgo(date) {
        const now = new Date();
        const diffMs = now - date;
        const diffMin = Math.floor(diffMs / 60000);
        if (diffMin < 1) return 'Just now';
        if (diffMin < 60) return diffMin + 'm ago';
        const diffHr = Math.floor(diffMin / 60);
        if (diffHr < 24) return diffHr + 'h ago';
        const diffDay = Math.floor(diffHr / 24);
        return diffDay + 'd ago';
    }

    function animateCounter(elId, target) {
        const el = document.getElementById(elId);
        if (!el) return;
        const current = parseInt(el.textContent) || 0;
        if (current === target) return;
        const step = target > current ? 1 : -1;
        const dur = Math.min(500, Math.abs(target - current) * 30);
        const steps = Math.abs(target - current);
        if (steps === 0) return;
        const interval = dur / steps;
        let val = current;
        const timer = setInterval(() => {
            val += step;
            el.textContent = val;
            if (val === target) clearInterval(timer);
        }, interval);
    }

    function formatNumber(num) {
        if (num == null) return '0';
        return Number(num).toLocaleString('en-IN', { maximumFractionDigits: 0 });
    }

    function escHtml(str) {
        if (!str) return '';
        const div = document.createElement('div');
        div.textContent = str;
        return div.innerHTML;
    }

    function pulseIndicator() {
        const dot = document.querySelector('.prod-live-dot');
        if (dot) {
            dot.classList.remove('prod-live-pulse');
            void dot.offsetWidth;
            dot.classList.add('prod-live-pulse');
        }
    }

    function refresh() {
        loadDashboard();
    }

    // ── Public API ─────────────────────────────────────────────
    return {
        init,
        refresh
    };
})();
