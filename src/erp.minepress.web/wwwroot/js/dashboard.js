/**
 * MinePress ERP — Dashboard JS
 * Loads data from DashboardController API endpoints
 * Auto-refreshes every 60 seconds
 */
const Dashboard = (() => {
    const API = '/api/dashboard';
    let refreshTimer = null;

    // ── Helpers ──
    function $id(id) { return document.getElementById(id); }

    function animateCounter(el, value) {
        if (!el) return;
        el.textContent = value;
        el.classList.remove('dash-counter-animate');
        void el.offsetWidth; // reflow
        el.classList.add('dash-counter-animate');
    }

    function escapeHtml(str) {
        if (!str) return '';
        const div = document.createElement('div');
        div.textContent = str;
        return div.innerHTML;
    }

    function severityColor(sev) {
        const map = { INFO: 'info', WARNING: 'warning', ERROR: 'danger', CRITICAL: 'danger', SUCCESS: 'success' };
        return map[(sev || '').toUpperCase()] || 'secondary';
    }

    function priorityBadge(priority) {
        const map = { URGENT: 'danger', HIGH: 'warning', NORMAL: 'info', LOW: 'secondary' };
        const color = map[(priority || '').toUpperCase()] || 'secondary';
        return `<span class="badge bg-${color}-lt">${escapeHtml(priority || 'Normal')}</span>`;
    }

    // ── API Fetchers ──
    async function fetchJson(endpoint) {
        try {
            const res = await fetch(`${API}/${endpoint}`);
            if (!res.ok) return null;
            return await res.json();
        } catch {
            return null;
        }
    }

    // ── Load Stats ──
    async function loadStats() {
        const data = await fetchJson('stats');
        if (!data) return;

        animateCounter($id('statTasks'), data.tasksAssigned ?? 0);
        animateCounter($id('statApprovals'), data.pendingApprovals ?? 0);
        animateCounter($id('statNotifications'), data.unreadNotifications ?? 0);
        animateCounter($id('statAlerts'), data.alertCount ?? 0);

        // Attendance
        const att = data.attendance;
        if (att && att.checkedIn) {
            animateCounter($id('statAttendance'), att.status || 'Active');
            if ($id('attendLogin')) $id('attendLogin').textContent = att.loginTime || '—';
            if ($id('attendLogout')) $id('attendLogout').textContent = att.logoutTime || '—';
        } else {
            animateCounter($id('statAttendance'), 'Absent');
            if ($id('attendLogin')) $id('attendLogin').textContent = '—';
            if ($id('attendLogout')) $id('attendLogout').textContent = '—';
        }

        if ($id('attendActivities')) $id('attendActivities').textContent = data.todayActivities ?? 0;
        if ($id('infoActiveJobs')) $id('infoActiveJobs').textContent = data.activeJobs ?? 0;
        if ($id('infoTodayEnq')) $id('infoTodayEnq').textContent = data.todayEnquiries ?? 0;
    }

    // ── Load KPIs ──
    async function loadKpis() {
        const container = $id('kpiContainer');
        if (!container) return;

        const data = await fetchJson('kpis');
        if (!data || !data.length) {
            container.innerHTML = '<div class="col-12 dash-empty"><i class="bi bi-bar-chart"></i>No KPIs available</div>';
            return;
        }

        container.innerHTML = data.map(kpi => `
            <div class="col-6 col-md-3">
                <div class="dash-kpi-widget">
                    <span class="avatar avatar-md bg-${escapeHtml(kpi.color || 'primary')}-lt mb-2">
                        <i class="bi ${escapeHtml(kpi.icon || 'bi-circle')}"></i>
                    </span>
                    <div class="dash-kpi-value text-${escapeHtml(kpi.color || 'primary')}">${escapeHtml(String(kpi.value ?? '—'))}</div>
                    <div class="dash-kpi-label">${escapeHtml(kpi.label || '')}</div>
                </div>
            </div>
        `).join('');
    }

    // ── Load Activities ──
    async function loadActivities() {
        const list = $id('activityList');
        if (!list) return;

        const data = await fetchJson('activities');
        if (!data || !data.length) {
            list.innerHTML = '<div class="list-group-item dash-empty"><i class="bi bi-clock-history"></i>No recent activities</div>';
            return;
        }

        list.innerHTML = data.map(a => `
            <div class="list-group-item py-2 dash-activity-severity-${escapeHtml(a.severity || '')}">
                <div class="d-flex justify-content-between align-items-start">
                    <div>
                        <div class="fw-semibold small">${escapeHtml(a.title || a.activityType || 'Activity')}</div>
                        <div class="text-secondary" style="font-size:.75rem;">
                            <span class="badge bg-${severityColor(a.severity)}-lt me-1">${escapeHtml(a.module || '')}</span>
                            ${escapeHtml(a.description || '')}
                        </div>
                    </div>
                    <div class="text-secondary text-nowrap small">${escapeHtml(a.activityOn || '')}</div>
                </div>
            </div>
        `).join('');
    }

    // ── Load Actions ──
    async function loadActions() {
        const list = $id('actionList');
        const badge = $id('actionCount');
        if (!list) return;

        const data = await fetchJson('actions');
        if (!data || !data.length) {
            list.innerHTML = '<div class="list-group-item dash-empty"><i class="bi bi-check-circle"></i>No pending actions</div>';
            if (badge) badge.textContent = '0';
            return;
        }

        if (badge) badge.textContent = data.length;

        list.innerHTML = data.map(a => `
            <div class="list-group-item py-2">
                <div class="d-flex justify-content-between align-items-start">
                    <div>
                        <div class="d-flex align-items-center gap-2">
                            <i class="bi ${escapeHtml(a.icon || 'bi-lightning-charge')} text-${escapeHtml(a.color || 'warning')}"></i>
                            <span class="fw-semibold small">${escapeHtml(a.title || 'Action Required')}</span>
                            ${priorityBadge(a.priority)}
                        </div>
                        <div class="text-secondary" style="font-size:.75rem;">${escapeHtml(a.message || '')}</div>
                    </div>
                    <div class="text-end">
                        ${a.actionUrl ? `<a href="${escapeHtml(a.actionUrl)}" class="btn btn-sm btn-outline-primary">${escapeHtml(a.actionLabel || 'Go')}</a>` : ''}
                        <div class="text-secondary text-nowrap" style="font-size:.7rem;">${escapeHtml(a.createdOn || '')}</div>
                    </div>
                </div>
            </div>
        `).join('');
    }

    // ── Load HRMS My Status ──
    async function loadHrms() {
        const section = $id('hrmsMyStatusSection');
        if (!section) return;

        const data = await fetchJson('hrms');
        if (!data) return;

        // Leave balance total
        const totalLeave = (data.leaveBalances || []).reduce((s, l) => s + (l.closingBalance ?? 0), 0);
        animateCounter($id('hrmsLeaveBalance'), totalLeave);
        animateCounter($id('hrmsPendingLeaves'), data.pendingLeaves ?? 0);

        // Loans
        const loanCount = (data.activeLoans || []).length;
        animateCounter($id('hrmsActiveLoan'), loanCount);

        // Advance
        const advTotal = (data.pendingAdvances || []).reduce((s, a) => s + (a.balanceAmount ?? 0), 0);
        animateCounter($id('hrmsAdvance'), advTotal > 0 ? `₹${advTotal.toLocaleString('en-IN')}` : '0');

        // Overtime
        animateCounter($id('hrmsOvertime'), `${(data.overtimeHoursThisMonth ?? 0).toFixed(1)}h`);

        // Medical
        animateCounter($id('hrmsMedical'), `₹${(data.medicalClaimsThisYear ?? 0).toLocaleString('en-IN')}`);

        // Reimbursements
        animateCounter($id('hrmsReimbursement'), `₹${(data.reimbursementClaimsThisYear ?? 0).toLocaleString('en-IN')}`);

        // Shift info
        const shiftEl = $id('hrmsShiftInfo');
        if (shiftEl && data.currentShift) {
            shiftEl.innerHTML = `<span class="fw-semibold">${escapeHtml(data.currentShift.shiftName)}</span>
                <span class="text-secondary small">${escapeHtml(data.currentShift.shiftStartTime || '')} – ${escapeHtml(data.currentShift.shiftEndTime || '')}</span>`;
        } else if (shiftEl) {
            shiftEl.innerHTML = '<span class="text-secondary">No shift assigned</span>';
        }

        // Holidays
        const holEl = $id('hrmsHolidays');
        if (holEl && data.upcomingHolidays && data.upcomingHolidays.length) {
            holEl.innerHTML = data.upcomingHolidays.map(h =>
                `<div class="d-flex justify-content-between small"><span>${escapeHtml(h.holidayName)}</span><span class="text-secondary">${escapeHtml(h.holidayDate)}</span></div>`
            ).join('');
        } else if (holEl) {
            holEl.innerHTML = '<span class="text-secondary small">No upcoming holidays</span>';
        }

        // Incentives
        const incEl = $id('hrmsIncentives');
        if (incEl && data.recentIncentives && data.recentIncentives.length) {
            incEl.innerHTML = data.recentIncentives.map(i =>
                `<div class="d-flex justify-content-between small"><span>${escapeHtml(i.incentiveType)}</span><span class="text-success fw-semibold">₹${(i.incentiveAmount ?? 0).toLocaleString('en-IN')}</span></div>`
            ).join('');
        } else if (incEl) {
            incEl.innerHTML = '<span class="text-secondary small">No recent incentives</span>';
        }
    }

    // ── Load HRMS Overview (MGT/ADM) ──
    async function loadHrmsOverview() {
        const section = $id('hrmsOverviewSection');
        if (!section) return;

        const data = await fetchJson('hrms-overview');
        if (!data) return;

        animateCounter($id('ovTotalEmp'), data.totalEmployees ?? 0);
        animateCounter($id('ovPresent'), data.presentToday ?? 0);
        animateCounter($id('ovAbsent'), data.absentToday ?? 0);
        animateCounter($id('ovOnLeave'), data.onLeaveToday ?? 0);
        animateCounter($id('ovPendingLeave'), data.pendingLeaveRequests ?? 0);
        animateCounter($id('ovActiveLoans'), data.activeLoans ?? 0);
        animateCounter($id('ovPendingAdv'), data.pendingAdvances ?? 0);
        animateCounter($id('ovPendingOT'), data.pendingOvertimes ?? 0);
        animateCounter($id('ovPendingMed'), data.pendingMedicalClaims ?? 0);
        animateCounter($id('ovPendingResign'), data.pendingResignations ?? 0);
        animateCounter($id('ovPendingTransfer'), data.pendingTransfers ?? 0);
        animateCounter($id('ovPendingReim'), data.pendingReimbursements ?? 0);
    }

    // ── Load AI Smart Suggestions ──
    async function loadAiSuggestions() {
        const list = $id('aiSuggestionList');
        const thinking = $id('aiThinking');
        if (!list) return;

        if (thinking) thinking.style.display = 'flex';

        const data = await fetchJson('ai-suggestions');

        if (thinking) thinking.style.display = 'none';

        if (!data || !data.length) {
            list.innerHTML = '<div class="list-group-item dash-empty"><i class="bi bi-stars"></i>No suggestions right now. Everything looks great!</div>';
            return;
        }

        const categoryIcon = (cat) => {
            const icons = { LEAVE: 'bi-calendar-x', LOAN: 'bi-bank', ADVANCE: 'bi-cash-stack', HOLIDAY: 'bi-calendar-event', OVERTIME: 'bi-clock-fill', ATTENDANCE: 'bi-trophy', MEDICAL: 'bi-heart-pulse', TASKS: 'bi-list-task' };
            return icons[(cat || '').toUpperCase()] || 'bi-lightbulb';
        };

        list.innerHTML = data.map(s => `
            <div class="list-group-item py-2 dash-ai-priority-${escapeHtml((s.priority || '').toUpperCase())}">
                <div class="d-flex align-items-start gap-2">
                    <span class="dash-ai-suggestion-icon bg-${escapeHtml(s.color || 'purple')}-lt text-${escapeHtml(s.color || 'purple')} mt-1">
                        <i class="bi ${escapeHtml(s.icon || categoryIcon(s.category))}"></i>
                    </span>
                    <div class="flex-fill">
                        <div class="d-flex align-items-center gap-2">
                            <span class="fw-semibold small">${escapeHtml(s.title || 'Suggestion')}</span>
                            ${priorityBadge(s.priority)}
                        </div>
                        <div class="text-secondary" style="font-size:.75rem;">${escapeHtml(s.message || '')}</div>
                        ${s.category ? `<span class="badge bg-purple-lt mt-1" style="font-size:.6rem;">${escapeHtml(s.category)}</span>` : ''}
                    </div>
                </div>
            </div>
        `).join('');
    }

    // ── Punch In/Out — State ──
    let punchState = 'NOT_PUNCHED';
    let punchCheckInTime = null;
    let punchWorkSeconds = 0;
    let punchClockInterval = null;
    let punchTimerInterval = null;

    // ── Punch: Live Clock ──
    function updatePunchClock() {
        const clockEl = $id('punchLiveClock');
        const dateEl = $id('punchLiveDate');
        if (!clockEl) return;
        const now = new Date();
        clockEl.textContent = now.toLocaleTimeString('en-IN', { hour: '2-digit', minute: '2-digit', second: '2-digit', hour12: false });
        if (dateEl) {
            dateEl.textContent = now.toLocaleDateString('en-IN', { weekday: 'long', day: 'numeric', month: 'short', year: 'numeric' });
        }
    }

    // ── Punch: Work Timer ──
    function updateWorkTimer() {
        if (punchState !== 'PUNCHED_IN') return;
        punchWorkSeconds++;
        renderWorkDuration(punchWorkSeconds);
    }

    function renderWorkDuration(totalSecs) {
        const hrs = Math.floor(totalSecs / 3600);
        const mins = Math.floor((totalSecs % 3600) / 60);
        const el = $id('punchWorkHours');
        if (el) el.textContent = `${hrs}h ${mins}m`;

        // Progress bar (8h = 100%)
        const pct = Math.min(100, (totalSecs / 28800) * 100);
        const bar = $id('punchProgressBar');
        const pctEl = $id('punchProgressPct');
        const section = $id('punchProgressSection');
        if (bar) {
            bar.style.width = pct + '%';
            bar.className = 'progress-bar punch-progress-bar' +
                (pct >= 100 ? ' bg-success' : pct >= 75 ? ' bg-primary' : pct >= 50 ? ' bg-info' : ' bg-warning');
        }
        if (pctEl) pctEl.textContent = Math.round(pct) + '%';
        if (section) section.style.display = '';
    }

    // ── Load Punch Status ──
    async function loadPunchStatus() {
        const section = $id('punchWidgetSection');
        if (!section) return;

        const data = await fetchJson('punch-status');
        if (!data || !data.hasEmployee) {
            section.style.display = 'none';
            return;
        }

        punchState = data.punchState || 'NOT_PUNCHED';
        punchWorkSeconds = data.workSeconds || 0;

        const btn = $id('punchActionBtn');
        const btnText = $id('punchBtnText');
        const badge = $id('punchStatusBadge');
        const inTimeEl = $id('punchInTime');
        const outTimeEl = $id('punchOutTime');
        const shiftEl = $id('punchShiftInfo');
        const progressSection = $id('punchProgressSection');

        // Check-in / Check-out times (use pre-formatted display strings from API)
        if (inTimeEl) inTimeEl.textContent = data.checkInDisplay || '--:--';
        if (outTimeEl) outTimeEl.textContent = data.checkOutDisplay || '--:--';

        // Shift
        if (shiftEl && data.shift) {
            shiftEl.textContent = data.shift.shiftName || '—';
        } else if (shiftEl) {
            shiftEl.textContent = 'General';
        }

        // State-based UI
        if (btn) btn.disabled = false;
        if (punchState === 'NOT_PUNCHED') {
            if (btn) { btn.className = 'punch-btn punch-btn-in'; }
            if (btnText) btnText.textContent = 'Punch In';
            if (badge) badge.innerHTML = '<span class="badge bg-warning-lt"><i class="bi bi-circle me-1"></i>Not Punched In</span>';
            if (progressSection) progressSection.style.display = 'none';
            renderWorkDuration(0);
        } else if (punchState === 'PUNCHED_IN') {
            if (btn) { btn.className = 'punch-btn punch-btn-out'; }
            if (btnText) btnText.textContent = 'Punch Out';
            if (badge) badge.innerHTML = '<span class="badge bg-success-lt"><i class="bi bi-check-circle me-1"></i>Checked In — Working</span>';
            punchCheckInTime = data.checkIn ? new Date(data.checkIn) : null;
            renderWorkDuration(punchWorkSeconds);
            // Start timer
            if (!punchTimerInterval) punchTimerInterval = setInterval(updateWorkTimer, 1000);
        } else {
            // PUNCHED_OUT
            if (btn) { btn.className = 'punch-btn punch-btn-done'; btn.disabled = true; }
            if (btnText) btnText.textContent = 'Day Complete';
            if (badge) badge.innerHTML = '<span class="badge bg-primary-lt"><i class="bi bi-check-all me-1"></i>Day Completed</span>';
            renderWorkDuration(punchWorkSeconds);
            if (punchTimerInterval) { clearInterval(punchTimerInterval); punchTimerInterval = null; }
        }
    }

    // ── Punch Action ──
    async function doPunch() {
        const btn = $id('punchActionBtn');
        if (btn) btn.disabled = true;

        const endpoint = punchState === 'NOT_PUNCHED' ? 'punch-in' : 'punch-out';
        const actionLabel = punchState === 'NOT_PUNCHED' ? 'Punch In' : 'Punch Out';

        try {
            const res = await fetch(`${API}/${endpoint}`, { method: 'POST', headers: { 'Content-Type': 'application/json' } });
            const data = await res.json();

            if (res.ok) {
                if (typeof Swal !== 'undefined') {
                    Swal.fire({
                        icon: 'success',
                        title: actionLabel + ' Successful',
                        text: data.message,
                        timer: 2000,
                        showConfirmButton: false,
                        toast: true,
                        position: 'top-end'
                    });
                }
                await loadPunchStatus();
                loadPunchAiInsights();
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

    // ── Load Punch AI Insights ──
    async function loadPunchAiInsights() {
        const list = $id('punchAiInsightsList');
        if (!list) return;

        const data = await fetchJson('punch-ai-insights');
        if (!data || !data.insights || !data.insights.length) {
            list.innerHTML = '<div class="list-group-item text-center py-3 text-muted small"><i class="bi bi-stars d-block" style="font-size:1.5rem;opacity:.3;"></i>Keep punching in to unlock AI insights!</div>';
            return;
        }

        list.innerHTML = data.insights.map(i => `
            <div class="list-group-item py-2 punch-ai-insight-item">
                <div class="d-flex align-items-start gap-2">
                    <span class="punch-ai-insight-icon bg-${escapeHtml(i.color || 'primary')}-lt text-${escapeHtml(i.color || 'primary')}">
                        <i class="bi ${escapeHtml(i.icon || 'bi-lightbulb')}"></i>
                    </span>
                    <div class="flex-fill">
                        <div class="fw-semibold small">${escapeHtml(i.title || '')}</div>
                        <div class="text-secondary" style="font-size:.72rem;">${escapeHtml(i.message || '')}</div>
                    </div>
                </div>
            </div>
        `).join('');
    }

    // ── Load Attendance Tracker (MGT/ADM/SysAdmin) ──
    async function loadAttendanceTracker() {
        const section = $id('attendanceTrackerSection');
        if (!section) return;

        const data = await fetchJson('attendance-tracker');
        if (!data) return;

        // Update badges
        const presentBadge = $id('trackerPresentBadge');
        const absentBadge = $id('trackerAbsentBadge');
        const leaveBadge = $id('trackerLeaveBadge');
        if (presentBadge) presentBadge.textContent = `${data.presentCount ?? 0} Present`;
        if (absentBadge) absentBadge.textContent = `${data.notPunchedCount ?? 0} Not Punched`;
        if (leaveBadge) leaveBadge.textContent = `${data.onLeaveCount ?? 0} On Leave`;

        // Update counts
        const npCount = $id('trackerNotPunchedCount');
        const olCount = $id('trackerOnLeaveCount');
        if (npCount) npCount.textContent = data.notPunchedCount ?? 0;
        if (olCount) olCount.textContent = data.onLeaveCount ?? 0;

        // Not Punched list
        const npList = $id('trackerNotPunchedList');
        if (npList) {
            if (data.notPunched && data.notPunched.length) {
                npList.innerHTML = data.notPunched.map(e => `
                    <div class="dash-tracker-item">
                        <div class="d-flex align-items-center gap-2">
                            <span class="avatar avatar-xs bg-danger-lt">${escapeHtml((e.name || '?').charAt(0))}</span>
                            <div class="flex-fill">
                                <div class="fw-semibold small">${escapeHtml(e.name || '—')}</div>
                                <div class="text-secondary" style="font-size:.7rem;">${escapeHtml(e.empCode || '')} · ${escapeHtml(e.designation || '')}</div>
                            </div>
                            <span class="badge bg-secondary-lt" style="font-size:.65rem;">${escapeHtml(e.deptCode || '')}</span>
                        </div>
                    </div>
                `).join('');
            } else {
                npList.innerHTML = '<div class="text-center text-muted py-3 small"><i class="bi bi-check-circle text-success d-block" style="font-size:1.2rem;"></i>All employees punched in!</div>';
            }
        }

        // On Leave list
        const olList = $id('trackerOnLeaveList');
        if (olList) {
            if (data.onLeave && data.onLeave.length) {
                olList.innerHTML = data.onLeave.map(e => `
                    <div class="dash-tracker-item">
                        <div class="d-flex align-items-center gap-2">
                            <span class="avatar avatar-xs bg-warning-lt">${escapeHtml((e.name || '?').charAt(0))}</span>
                            <div class="flex-fill">
                                <div class="fw-semibold small">${escapeHtml(e.name || '—')}</div>
                                <div class="text-secondary" style="font-size:.7rem;">${escapeHtml(e.empCode || '')} · ${escapeHtml(e.designation || '')}</div>
                            </div>
                            <span class="badge bg-secondary-lt" style="font-size:.65rem;">${escapeHtml(e.deptCode || '')}</span>
                        </div>
                    </div>
                `).join('');
            } else {
                olList.innerHTML = '<div class="text-center text-muted py-3 small"><i class="bi bi-calendar-check text-success d-block" style="font-size:1.2rem;"></i>No one on leave today</div>';
            }
        }
    }

    // ── Hero Time ──
    function updateHeroTime() {
        const el = $id('heroTime');
        if (!el) return;
        const now = new Date();
        const opts = { weekday: 'long', day: 'numeric', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit' };
        el.textContent = now.toLocaleDateString('en-IN', opts);
    }

    // ── Refresh All ──
    async function refreshAll() {
        document.body.classList.add('dash-refreshing');
        await Promise.all([
            loadStats(),
            loadKpis(),
            loadActivities(),
            loadActions(),
            loadHrms(),
            loadHrmsOverview(),
            loadAiSuggestions(),
            loadPunchStatus(),
            loadPunchAiInsights(),
            loadAttendanceTracker()
        ]);
        updateHeroTime();
        document.body.classList.remove('dash-refreshing');
    }

    // ── Init ──
    function init() {
        updateHeroTime();
        updatePunchClock();
        refreshAll();
        // Auto-refresh every 60 seconds
        refreshTimer = setInterval(refreshAll, 60000);
        // Update clock every minute
        setInterval(updateHeroTime, 60000);
        // Live punch clock every second
        punchClockInterval = setInterval(updatePunchClock, 1000);
    }

    // ── Public API ──
    return {
        init,
        refresh: refreshAll,
        refreshAi: loadAiSuggestions,
        doPunch,
        refreshPunch: loadPunchStatus,
        refreshTracker: loadAttendanceTracker
    };
})();

// Auto-initialize when DOM is ready
document.addEventListener('DOMContentLoaded', Dashboard.init);
