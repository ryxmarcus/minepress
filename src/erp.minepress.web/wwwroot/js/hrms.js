/**
 * MinePress ERP — HRMS Module JS
 * Loads data from HrmsController API endpoints
 */
const Hrms = (() => {
    const API = '/api/hrms';

    // ── Helpers ──
    function $id(id) { return document.getElementById(id); }

    function esc(str) {
        if (!str) return '';
        const d = document.createElement('div');
        d.textContent = str;
        return d.innerHTML;
    }

    function fmt(n) {
        if (n == null) return '—';
        return '₹' + Number(n).toLocaleString('en-IN', { maximumFractionDigits: 0 });
    }

    function statusBadge(status) {
        const s = (status || '').toUpperCase();
        const map = {
            'PENDING':   'hrms-badge-pending',
            'APPROVED':  'hrms-badge-approved',
            'REJECTED':  'hrms-badge-rejected',
            'ACTIVE':    'hrms-badge-active',
            'CLOSED':    'hrms-badge-closed',
            'COMPLETED': 'hrms-badge-approved',
            'CANCELLED': 'hrms-badge-rejected',
            'PRESENT':   'hrms-badge-approved',
            'ABSENT':    'hrms-badge-rejected',
            'HALFDAY':   'hrms-badge-pending',
            'LATE':      'hrms-badge-pending',
            'PAID':      'hrms-badge-approved',
            'SUNDAY':    'hrms-badge-closed',
            'HOLIDAY':   'hrms-badge-active',
            'NO_RECORD': 'hrms-badge-closed'
        };
        const labels = {
            'NO_RECORD': '—',
            'SUNDAY':    'Sunday',
            'HOLIDAY':   'Holiday'
        };
        const cls = map[s] || 'hrms-badge-closed';
        const lbl = labels[s] || status;
        return `<span class="${cls}">${esc(lbl)}</span>`;
    }

    async function get(endpoint) {
        try {
            const res = await fetch(`${API}/${endpoint}`, { credentials: 'same-origin' });
            if (res.status === 401) { window.location.href = '/Account/Login'; return null; }
            if (!res.ok) return null;
            return await res.json();
        } catch (e) {
            console.error('HRMS API error:', e);
            return null;
        }
    }

    async function post(endpoint, body) {
        try {
            const res = await fetch(`${API}/${endpoint}`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'same-origin',
                body: JSON.stringify(body)
            });
            if (res.status === 401) { window.location.href = '/Account/Login'; return null; }
            const data = await res.json();
            if (!res.ok) {
                Swal.fire({ icon: 'error', title: 'Error', text: data.message || 'Request failed' });
                return null;
            }
            return data;
        } catch (e) {
            console.error('HRMS API error:', e);
            Swal.fire({ icon: 'error', title: 'Error', text: 'Network error. Please try again.' });
            return null;
        }
    }

    function emptyRow(cols, msg) {
        return `<tr><td colspan="${cols}" class="text-center text-secondary py-4">${esc(msg || 'No records found')}</td></tr>`;
    }

    // ═══════════════════════════════════════════
    //  DASHBOARD STATS (HRMS Landing)
    // ═══════════════════════════════════════════

    async function loadDashboardStats() {
        const [leaves, loans, advances, attendance, reimbursements] = await Promise.all([
            get('leaves/balances'),
            get('loans'),
            get('advances'),
            get('attendance/summary'),
            get('reimbursements')
        ]);

        const statLeave = $id('hrmsStatLeave');
        const statLoan = $id('hrmsStatLoan');
        const statAdvance = $id('hrmsStatAdvance');
        const statAttendance = $id('hrmsStatAttendance');
        const statReimbursement = $id('hrmsStatReimbursement');

        if (statLeave && leaves) {
            const total = leaves.reduce((s, b) => s + (b.balance || 0), 0);
            statLeave.textContent = total;
        }
        if (statLoan && loans) {
            const active = loans.filter(l => (l.status || '').toUpperCase() === 'ACTIVE' || (l.status || '').toUpperCase() === 'APPROVED').length;
            statLoan.textContent = active;
        }
        if (statAdvance && advances) {
            const pending = advances.filter(a => (a.status || '').toUpperCase() === 'PENDING').length;
            statAdvance.textContent = pending;
        }
        if (statAttendance && attendance) {
            statAttendance.textContent = attendance.present || 0;
        }
        if (statReimbursement && reimbursements) {
            const pending = reimbursements.filter(r => (r.status || '').toUpperCase() === 'PENDING').length;
            statReimbursement.textContent = pending;
        }

        // HRMS landing page quick stats
        const qsPresent = $id('qsPresent');
        if (qsPresent && attendance) qsPresent.textContent = attendance.present || 0;

        const qsLeave = $id('qsLeaveBalance');
        if (qsLeave && leaves) qsLeave.textContent = leaves.reduce((s, b) => s + (b.balance || 0), 0);

        const qsOT = $id('qsOvertime');
        if (qsOT && attendance) qsOT.textContent = (attendance.overtimeHours || 0).toFixed(1);

        const qsLoan = $id('qsLoanBalance');
        if (qsLoan && loans) {
            const outstanding = loans.filter(l => (l.status || '').toUpperCase() === 'ACTIVE' || (l.status || '').toUpperCase() === 'APPROVED')
                .reduce((s, l) => s + (l.outstandingAmount || 0), 0);
            qsLoan.textContent = fmt(outstanding);
        }

        const qsReim = $id('qsReimbursement');
        if (qsReim && reimbursements) {
            qsReim.textContent = reimbursements.filter(r => (r.status || '').toUpperCase() === 'PENDING').length;
        }
    }

    // ═══════════════════════════════════════════
    //  LEAVES
    // ═══════════════════════════════════════════

    async function loadLeaves(filter) {
        const qs = filter && filter !== 'all' ? `?status=${filter}` : '';
        const data = await get(`leaves${qs}`);
        const tbody = $id('leaveTableBody');
        if (!tbody) return;
        if (!data || !data.items || data.items.length === 0) {
            tbody.innerHTML = emptyRow(8, 'No leave records found');
            return;
        }
        tbody.innerHTML = data.items.map(l => `<tr>
            <td><strong>${esc(l.leaveNo)}</strong></td>
            <td>${esc(l.leaveType)}</td>
            <td>${esc(l.fromDate)}</td>
            <td>${esc(l.toDate)}</td>
            <td>${l.totalDays}${l.halfDay ? ' (½)' : ''}</td>
            <td class="text-wrap" style="max-width:200px;">${esc(l.reason)}</td>
            <td>${esc(l.approvedBy || '—')}</td>
            <td>${statusBadge(l.status)}</td>
        </tr>`).join('');
    }

    async function loadLeaveBalances() {
        const data = await get('leaves/balances');
        const container = $id('leaveBalanceCards');
        if (!container || !data) return;
        if (data.length === 0) {
            container.innerHTML = '<div class="text-center text-secondary py-3">No balance data</div>';
            return;
        }
        container.innerHTML = data.map(b => `<div class="col-6 col-md-3">
            <div class="card hrms-stat-card">
                <div class="card-body text-center">
                    <div class="fw-bold fs-2 text-primary">${b.balance}</div>
                    <div class="text-secondary small">${esc(b.leaveType)}</div>
                    <div class="text-muted" style="font-size:.7rem;">Used: ${b.used} / ${b.entitled}</div>
                </div>
            </div>
        </div>`).join('');
    }

    async function loadLeaveBalancesCompact(containerId) {
        const data = await get('leaves/balances');
        const el = $id(containerId);
        if (!el || !data) return;
        if (data.length === 0) {
            el.innerHTML = '<span class="text-secondary">No balance data</span>';
            return;
        }
        el.innerHTML = `<div class="leave-balance-compact">${data.map(b =>
            `<span class="leave-balance-chip"><span class="chip-count">${b.balance}</span>${esc(b.leaveType)}</span>`
        ).join('')}</div>`;
    }

    async function loadLeaveTypes(selectId) {
        const data = await get('leave-types');
        const sel = $id(selectId);
        if (!sel || !data) return;
        data.forEach(lt => {
            const opt = document.createElement('option');
            opt.value = lt.leaveTypeId;
            opt.textContent = lt.leaveTypeName;
            sel.appendChild(opt);
        });
    }

    // ═══════════════════════════════════════════
    //  HOLIDAYS
    // ═══════════════════════════════════════════

    async function loadHolidays(year) {
        const qs = year ? `?year=${year}` : '';
        const data = await get(`holidays${qs}`);
        const tbody = $id('holidayTableBody');
        if (!tbody) return;
        if (!data || data.length === 0) {
            tbody.innerHTML = emptyRow(4, 'No holidays found');
            updateHolidaySidebar([]);
            return;
        }
        tbody.innerHTML = data.map((h, i) => `<tr>
            <td>${i + 1}</td>
            <td><strong>${esc(h.holidayName)}</strong></td>
            <td>${esc(h.holidayDate)}</td>
            <td>${esc(h.holidayType)}</td>
        </tr>`).join('');

        updateHolidaySidebar(data);

        const countEl = $id('holidayRemainingCount');
        if (countEl) {
            const today = new Date();
            const remaining = data.filter(h => {
                const parts = h.holidayDate.split('-');
                return new Date(parts[2] + '-' + parts[1] + '-' + parts[0]) >= today;
            }).length;
            countEl.textContent = remaining;
        }
    }

    function updateHolidaySidebar(data) {
        const container = $id('upcomingHolidays');
        if (!container) return;
        const today = new Date();
        const upcoming = data.filter(h => {
            try {
                const parts = h.holidayDate.split('-');
                const months = { Jan: 0, Feb: 1, Mar: 2, Apr: 3, May: 4, Jun: 5, Jul: 6, Aug: 7, Sep: 8, Oct: 9, Nov: 10, Dec: 11 };
                const d = new Date(parseInt(parts[2]), months[parts[1]] || 0, parseInt(parts[0]));
                return d >= today;
            } catch { return false; }
        }).slice(0, 5);

        if (upcoming.length === 0) {
            container.innerHTML = '<div class="text-secondary small">No upcoming holidays</div>';
            return;
        }
        container.innerHTML = upcoming.map(h =>
            `<div class="d-flex align-items-center gap-2 py-2 border-bottom">
                <i class="bi bi-calendar-event text-primary"></i>
                <div><strong class="small">${esc(h.holidayName)}</strong><br><span class="text-secondary" style="font-size:.75rem;">${esc(h.holidayDate)}</span></div>
            </div>`
        ).join('');
    }

    // ═══════════════════════════════════════════
    //  LOANS
    // ═══════════════════════════════════════════

    async function loadLoans() {
        const data = await get('loans');
        const tbody = $id('loanTableBody');
        if (!tbody) return;
        if (!data || data.length === 0) {
            tbody.innerHTML = emptyRow(9, 'No loan records found');
            updateLoanSummary([]);
            return;
        }
        tbody.innerHTML = data.map(l => `<tr>
            <td><strong>${esc(l.loanNo)}</strong></td>
            <td>${esc(l.loanType)}</td>
            <td>${esc(l.loanDate)}</td>
            <td>${fmt(l.loanAmount)}</td>
            <td>${l.interestRate || 0}%</td>
            <td>${l.tenureMonths || 0} months</td>
            <td>${fmt(l.emiAmount)}</td>
            <td>${fmt(l.outstandingAmount)}</td>
            <td>${statusBadge(l.status)}</td>
        </tr>`).join('');

        updateLoanSummary(data);
    }

    function updateLoanSummary(data) {
        const active = data.filter(l => (l.status || '').toUpperCase() === 'ACTIVE' || (l.status || '').toUpperCase() === 'APPROVED');
        const el1 = $id('loanActiveCount'); if (el1) el1.textContent = active.length;
        const el2 = $id('loanTotalAmount'); if (el2) el2.textContent = fmt(data.reduce((s, l) => s + (l.loanAmount || 0), 0));
        const el3 = $id('loanOutstanding'); if (el3) el3.textContent = fmt(active.reduce((s, l) => s + (l.outstandingAmount || 0), 0));
        const el4 = $id('loanMonthlyEmi'); if (el4) el4.textContent = fmt(active.reduce((s, l) => s + (l.emiAmount || 0), 0));
    }

    // ═══════════════════════════════════════════
    //  SALARY ADVANCES
    // ═══════════════════════════════════════════

    async function loadAdvances() {
        const data = await get('advances');
        const tbody = $id('advanceTableBody');
        if (!tbody) return;
        if (!data || data.length === 0) {
            tbody.innerHTML = emptyRow(7, 'No advance records found');
            updateAdvanceSummary([]);
            return;
        }
        tbody.innerHTML = data.map(a => `<tr>
            <td><strong>${esc(a.advanceNo)}</strong></td>
            <td>${esc(a.advanceDate)}</td>
            <td>${fmt(a.advanceAmount)}</td>
            <td>${a.repaymentMonths || 0}</td>
            <td>${fmt(a.monthlyDeduction)}</td>
            <td>${fmt(a.balanceAmount)}</td>
            <td>${statusBadge(a.status)}</td>
        </tr>`).join('');

        updateAdvanceSummary(data);
    }

    function updateAdvanceSummary(data) {
        const pending = data.filter(a => (a.status || '').toUpperCase() === 'PENDING');
        const approved = data.filter(a => (a.status || '').toUpperCase() === 'APPROVED' || (a.status || '').toUpperCase() === 'ACTIVE');
        const el1 = $id('advPendingCount'); if (el1) el1.textContent = pending.length;
        const el2 = $id('advApprovedCount'); if (el2) el2.textContent = approved.length;
        const el3 = $id('advTotalAmount'); if (el3) el3.textContent = fmt(data.reduce((s, a) => s + (a.advanceAmount || 0), 0));
        const el4 = $id('advBalanceAmount'); if (el4) el4.textContent = fmt(data.reduce((s, a) => s + (a.balanceAmount || 0), 0));
    }

    // ═══════════════════════════════════════════
    //  MEDICAL CLAIMS
    // ═══════════════════════════════════════════

    async function loadMedical() {
        const data = await get('medical');
        const tbody = $id('medicalTableBody');
        if (!tbody) return;
        if (!data || data.length === 0) {
            tbody.innerHTML = emptyRow(8, 'No medical claims found');
            updateMedicalSummary([]);
            return;
        }
        tbody.innerHTML = data.map(m => `<tr>
            <td><strong>${esc(m.claimNo)}</strong></td>
            <td>${esc(m.claimDate)}</td>
            <td>${esc(m.patientName)}</td>
            <td>${esc(m.relation)}</td>
            <td>${esc(m.hospitalName)}</td>
            <td>${fmt(m.claimAmount)}</td>
            <td>${m.approvedAmount != null ? fmt(m.approvedAmount) : '—'}</td>
            <td>${statusBadge(m.status)}</td>
        </tr>`).join('');

        updateMedicalSummary(data);
    }

    function updateMedicalSummary(data) {
        const el1 = $id('medTotalClaims'); if (el1) el1.textContent = data.length;
        const el2 = $id('medPendingClaims'); if (el2) el2.textContent = data.filter(m => (m.status || '').toUpperCase() === 'PENDING').length;
        const el3 = $id('medClaimedAmount'); if (el3) el3.textContent = fmt(data.reduce((s, m) => s + (m.claimAmount || 0), 0));
        const el4 = $id('medApprovedAmount'); if (el4) el4.textContent = fmt(data.reduce((s, m) => s + (m.approvedAmount || 0), 0));
    }

    // ═══════════════════════════════════════════
    //  OVERTIME
    // ═══════════════════════════════════════════

    async function loadOvertime() {
        const data = await get('overtime');
        const tbody = $id('overtimeTableBody');
        if (!tbody) return;
        if (!data || data.length === 0) {
            tbody.innerHTML = emptyRow(8, 'No overtime records found');
            updateOvertimeSummary([]);
            return;
        }
        tbody.innerHTML = data.map(o => `<tr>
            <td><strong>${esc(o.otNo)}</strong></td>
            <td>${esc(o.otDate)}</td>
            <td>${esc(o.fromTime)}</td>
            <td>${esc(o.toTime)}</td>
            <td>${o.otHours || 0}</td>
            <td>${fmt(o.otRatePerHour)}</td>
            <td>${fmt(o.otAmount)}</td>
            <td>${statusBadge(o.status)}</td>
        </tr>`).join('');

        updateOvertimeSummary(data);
    }

    function updateOvertimeSummary(data) {
        const el1 = $id('otTotalEntries'); if (el1) el1.textContent = data.length;
        const el2 = $id('otTotalHours'); if (el2) el2.textContent = data.reduce((s, o) => s + (o.otHours || 0), 0).toFixed(1);
        const el3 = $id('otTotalAmount'); if (el3) el3.textContent = fmt(data.reduce((s, o) => s + (o.otAmount || 0), 0));
        const el4 = $id('otPendingCount'); if (el4) el4.textContent = data.filter(o => (o.status || '').toUpperCase() === 'PENDING').length;
    }

    // ═══════════════════════════════════════════
    //  RESIGNATIONS
    // ═══════════════════════════════════════════

    async function loadResignations() {
        const data = await get('resignations');
        const tbody = $id('resignationTableBody');
        if (!tbody) return;
        if (!data || data.length === 0) {
            tbody.innerHTML = emptyRow(6, 'No resignation records');
            return;
        }
        tbody.innerHTML = data.map(r => `<tr>
            <td><strong>${esc(r.resignationNo)}</strong></td>
            <td>${esc(r.resignationDate)}</td>
            <td class="text-wrap" style="max-width:200px;">${esc(r.resignationReason)}</td>
            <td>${esc(r.lastWorkingDay || '—')}</td>
            <td>${r.noticePeriodDays || 0}</td>
            <td>${statusBadge(r.status)}</td>
        </tr>`).join('');
    }

    // ═══════════════════════════════════════════
    //  SHIFT ROSTER
    // ═══════════════════════════════════════════

    async function loadShifts() {
        const [roster, types] = await Promise.all([get('shifts'), get('shift-types')]);

        // Shift types overview
        const typesContainer = $id('shiftTypesContainer');
        if (typesContainer && types && types.length > 0) {
            typesContainer.innerHTML = `<div class="row g-2">${types.map(t =>
                `<div class="col-6 col-md-3">
                    <div class="border rounded p-2 text-center">
                        <div class="fw-bold">${esc(t.shiftName)}</div>
                        <div class="text-secondary small">${esc(t.shiftCode)}</div>
                        <div class="text-muted" style="font-size:.75rem;">${esc(t.shiftStartTime)} – ${esc(t.shiftEndTime)}</div>
                    </div>
                </div>`
            ).join('')}</div>`;
        } else if (typesContainer) {
            typesContainer.innerHTML = '<div class="text-secondary small">No shift types configured</div>';
        }

        // Roster table
        const tbody = $id('shiftTableBody');
        if (!tbody) return;
        if (!roster || roster.length === 0) {
            tbody.innerHTML = emptyRow(8, 'No shift assignments found');
            return;
        }
        tbody.innerHTML = roster.map(s => `<tr>
            <td><strong>${esc(s.shiftName)}</strong></td>
            <td>${esc(s.shiftCode)}</td>
            <td>${esc(s.shiftStart)}</td>
            <td>${esc(s.shiftEnd)}</td>
            <td>${esc(s.effectiveFrom)}</td>
            <td>${esc(s.effectiveTo)}</td>
            <td>${esc(s.weekOffDays || '—')}</td>
            <td>${s.isActive ? '<span class="hrms-badge-active">Active</span>' : '<span class="hrms-badge-closed">Inactive</span>'}</td>
        </tr>`).join('');
    }

    // ═══════════════════════════════════════════
    //  TRANSFERS
    // ═══════════════════════════════════════════

    async function loadTransfers() {
        const data = await get('transfers');
        const tbody = $id('transferTableBody');
        if (!tbody) return;
        if (!data || data.length === 0) {
            tbody.innerHTML = emptyRow(7, 'No transfer records found');
            return;
        }
        tbody.innerHTML = data.map(t => `<tr>
            <td><strong>${esc(t.transferNo)}</strong></td>
            <td>${esc(t.transferDate)}</td>
            <td>${t.fromDeptId || '—'}</td>
            <td>${t.toDeptId || '—'}</td>
            <td class="text-wrap" style="max-width:200px;">${esc(t.transferReason)}</td>
            <td>${esc(t.effectiveDate || '—')}</td>
            <td>${statusBadge(t.status)}</td>
        </tr>`).join('');
    }

    // ═══════════════════════════════════════════
    //  TRAVEL EXPENSES
    // ═══════════════════════════════════════════

    async function loadTravel() {
        const data = await get('travel');
        const tbody = $id('travelTableBody');
        if (!tbody) return;
        if (!data || data.length === 0) {
            tbody.innerHTML = emptyRow(9, 'No travel expense records found');
            updateTravelSummary([]);
            return;
        }
        tbody.innerHTML = data.map(t => `<tr>
            <td><strong>${esc(t.travelNo)}</strong></td>
            <td class="text-wrap" style="max-width:180px;">${esc(t.purpose)}</td>
            <td>${esc(t.fromLocation)}</td>
            <td>${esc(t.toLocation)}</td>
            <td>${esc(t.travelDate)}</td>
            <td>${esc(t.returnDate || '—')}</td>
            <td>${fmt(t.claimAmount)}</td>
            <td>${t.approvedAmount != null ? fmt(t.approvedAmount) : '—'}</td>
            <td>${statusBadge(t.status)}</td>
        </tr>`).join('');

        updateTravelSummary(data);
    }

    function updateTravelSummary(data) {
        const el1 = $id('trvTotalTrips'); if (el1) el1.textContent = data.length;
        const el2 = $id('trvPendingCount'); if (el2) el2.textContent = data.filter(t => (t.status || '').toUpperCase() === 'PENDING').length;
        const el3 = $id('trvClaimedAmount'); if (el3) el3.textContent = fmt(data.reduce((s, t) => s + (t.claimAmount || 0), 0));
        const el4 = $id('trvApprovedAmount'); if (el4) el4.textContent = fmt(data.reduce((s, t) => s + (t.approvedAmount || 0), 0));
    }

    // ═══════════════════════════════════════════
    //  ATTENDANCE
    // ═══════════════════════════════════════════

    async function loadAttendance(month) {
        const qs = month ? `?month=${month}` : '';
        const [records, summary] = await Promise.all([
            get(`attendance${qs}`),
            get(`attendance/summary${qs}`)
        ]);

        // Summary cards
        if (summary) {
            const s1 = $id('attTotalDays');      if (s1) s1.textContent = summary.totalDays || 0;
            const s2 = $id('attPresent');        if (s2) s2.textContent = summary.present || 0;
            const s3 = $id('attAbsent');         if (s3) s3.textContent = summary.absent || 0;
            const s4 = $id('attHalfDay');        if (s4) s4.textContent = (summary.halfDay || 0);
            const s5 = $id('attTotalHours');     if (s5) s5.textContent = (summary.totalHours || 0).toFixed(1);
            const s6 = $id('attOvertimeHours');  if (s6) s6.textContent = (summary.overtimeHours || 0).toFixed(1);
            // Period label
            const pl = $id('attPeriodLabel');
            if (pl && summary.startDate && summary.endDate)
                pl.textContent = `${summary.startDate} — ${summary.endDate}`;
        }

        // Table
        const tbody = $id('attendanceTableBody');
        if (!tbody) return;
        if (!records || !records.items || records.items.length === 0) {
            tbody.innerHTML = emptyRow(7, 'No attendance records found for this period');
            return;
        }

        tbody.innerHTML = records.items.map(a => {
            const isWeekend = (a.status || '').toUpperCase() === 'SUNDAY';
            const isNoRecord = (a.status || '').toUpperCase() === 'NO_RECORD';
            const rowClass = isWeekend ? 'table-secondary text-muted'
                           : isNoRecord ? 'table-warning'
                           : '';
            return `<tr class="${rowClass}">
                <td>
                    <span class="fw-semibold">${esc(a.attendanceDate)}</span>
                    <span class="badge bg-secondary-lt text-secondary ms-1" style="font-size:.65rem;">${esc(a.dayName)}</span>
                </td>
                <td>${a.checkIn ? esc(a.checkIn) : '<span class="text-muted">—</span>'}</td>
                <td>${a.checkOut ? esc(a.checkOut) : '<span class="text-muted">—</span>'}</td>
                <td>${a.totalHours != null ? a.totalHours.toFixed(1) + 'h' : '<span class="text-muted">—</span>'}</td>
                <td>${a.overtimeHours != null && a.overtimeHours > 0 ? '<span class="text-success fw-semibold">' + a.overtimeHours.toFixed(1) + 'h</span>' : '<span class="text-muted">—</span>'}</td>
                <td>${statusBadge(a.status)}</td>
            </tr>`;
        }).join('');
    }

    // ═══════════════════════════════════════════
    //  INCENTIVES
    // ═══════════════════════════════════════════

    async function loadIncentives() {
        const data = await get('incentives');
        const tbody = $id('incentiveTableBody');
        if (!tbody) return;
        if (!data || data.length === 0) {
            tbody.innerHTML = emptyRow(6, 'No incentive records found');
            updateIncentiveSummary([]);
            return;
        }
        tbody.innerHTML = data.map(i => `<tr>
            <td><strong>${esc(i.incentiveNo)}</strong></td>
            <td>${esc(i.incentiveType)}</td>
            <td>${esc(i.referencePeriod)}</td>
            <td>${esc(i.incentiveDate)}</td>
            <td>${fmt(i.incentiveAmount)}</td>
            <td>${statusBadge(i.status)}</td>
        </tr>`).join('');

        updateIncentiveSummary(data);
    }

    function updateIncentiveSummary(data) {
        const el1 = $id('incTotalCount'); if (el1) el1.textContent = data.length;
        const el2 = $id('incTotalAmount'); if (el2) el2.textContent = fmt(data.reduce((s, i) => s + (i.incentiveAmount || 0), 0));
        const el3 = $id('incLatestType'); if (el3) el3.textContent = data.length > 0 ? (data[0].incentiveType || '—') : '—';
    }

    // ═══════════════════════════════════════════
    //  REIMBURSEMENTS
    // ═══════════════════════════════════════════

    async function loadReimbursements() {
        const data = await get('reimbursements');
        const tbody = $id('reimbursementTableBody');
        if (!tbody) return;
        if (!data || data.length === 0) {
            tbody.innerHTML = emptyRow(8, 'No reimbursement records found');
            updateReimbursementSummary([]);
            return;
        }
        tbody.innerHTML = data.map(r => `<tr>
            <td><strong>${esc(r.reimbursementNo)}</strong></td>
            <td>${esc(r.reimbursementType)}</td>
            <td>${esc(r.claimDate)}</td>
            <td class="text-wrap" style="max-width:200px;">${esc(r.description)}</td>
            <td>${fmt(r.claimAmount)}</td>
            <td>${r.approvedAmount != null ? fmt(r.approvedAmount) : '—'}</td>
            <td>${r.paidAmount != null && r.paidAmount > 0 ? fmt(r.paidAmount) : '—'}</td>
            <td>${statusBadge(r.status)}</td>
        </tr>`).join('');

        updateReimbursementSummary(data);
    }

    function updateReimbursementSummary(data) {
        const el1 = $id('reimTotalClaims'); if (el1) el1.textContent = data.length;
        const el2 = $id('reimPendingCount'); if (el2) el2.textContent = data.filter(r => (r.status || '').toUpperCase() === 'PENDING').length;
        const el3 = $id('reimClaimedAmount'); if (el3) el3.textContent = fmt(data.reduce((s, r) => s + (r.claimAmount || 0), 0));
        const el4 = $id('reimApprovedAmount'); if (el4) el4.textContent = fmt(data.reduce((s, r) => s + (r.approvedAmount || 0), 0));
    }

    // ═══════════════════════════════════════════
    //  PUBLIC API
    // ═══════════════════════════════════════════

    return {
        loadDashboardStats,
        loadLeaves,
        loadLeaveBalances,
        loadLeaveBalancesCompact,
        loadLeaveTypes,
        loadHolidays,
        loadLoans,
        loadAdvances,
        loadMedical,
        loadOvertime,
        loadResignations,
        loadShifts,
        loadTransfers,
        loadTravel,
        loadAttendance,
        loadIncentives,
        loadReimbursements,
        post
    };
})();
