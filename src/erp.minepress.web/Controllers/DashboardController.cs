using erp.minepress.infrastructure.ErrorLogging;
using erp.minepress.persistence.Context;
using erp.minepress.notification.Interfaces;
using erp.minepress.web.Helpers;
using erp.minepress.web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace erp.minepress.web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IUserActivityService _activity;
    private readonly INotificationService _notifier;
    private readonly ILogger<DashboardController> _logger;
    private readonly ISystemErrorLogger _systemErrorLogger;

    public DashboardController(ApplicationDbContext db, IUserActivityService activity, INotificationService notifier, ILogger<DashboardController> logger, ISystemErrorLogger systemErrorLogger)
    {
        _db = db;
        _activity = activity;
        _notifier = notifier;
        _logger = logger;
        _systemErrorLogger = systemErrorLogger;
    }

    private async Task AuditExceptionAsync(Exception ex, string additionalData, string severity = "Error")
        => await _systemErrorLogger.LogAsync(ex, HttpContext, severity, additionalData);

    /// <summary>
    /// Returns department info + dashboard type for the logged-in user.
    /// </summary>
    [HttpGet("info")]
    public async Task<IActionResult> GetDashboardInfo()
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized();

        var dept = await _db.MstDepartments
            .AsNoTracking()
            .Where(d => d.DeptId == user.DepartmentId)
            .Select(d => new { d.DeptCode, d.DeptName, d.IsProduction, d.ParentDeptCode })
            .FirstOrDefaultAsync();

        var deptCode = dept?.DeptCode ?? "GEN";
        var dashboardType = ResolveDashboardType(deptCode);

        return Ok(new
        {
            user.UserId,
            user.Name,
            user.UserCode,
            DeptCode = deptCode,
            DeptName = dept?.DeptName ?? "General",
            IsProduction = dept?.IsProduction ?? false,
            DashboardType = dashboardType,
            user.IsSystemAdmin,
            user.IsApprovalUser,
            LoginAt = user.LoginAt.ToString("dd-MMM-yyyy HH:mm")
        });
    }

    /// <summary>
    /// Summary stats: tasks assigned, pending approvals, today's activities, unread notifications, attendance status.
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized();

        var today = DateTime.Today;
        var todayUtc = DateTime.UtcNow.Date;

        // Tasks assigned to this user (jobs where AssignedTo = userId and not completed)
        var tasksAssigned = await _db.TrnJobs
            .CountAsync(j => j.AssignedTo == user.UserId
                && j.StatusCode != "COMPLETED" && j.StatusCode != "CLOSED");

        // Pending approvals — enquiries/quotations in draft/pending created by or assigned to user
        var pendingApprovals = 0;
        if (user.IsApprovalUser || user.IsSystemAdmin)
        {
            var pendingEnq = await _db.TrnEnquiries
                .CountAsync(e => e.Status == "PENDING" || e.Status == "SUBMITTED");
            var pendingQuot = await _db.TrnQuotations
                .CountAsync(q => q.Status == "PENDING" || q.Status == "SUBMITTED");
            pendingApprovals = pendingEnq + pendingQuot;
        }

        // Today's activities count
        var todayActivities = await _db.TrnUserActivityLogs
            .CountAsync(a => a.UserId == user.UserId && a.ActivityOn.Date == today);

        // Unread notifications
        var unreadNotifications = await _db.TrnUserNotifications
            .CountAsync(n => n.UserId == user.UserId
                && (n.IsRead == null || n.IsRead == false)
                && (n.IsDismissed == null || n.IsDismissed == false));

        // Alerts (high-priority or action-required notifications)
        var alertCount = await _db.TrnUserNotifications
            .CountAsync(n => n.UserId == user.UserId
                && (n.IsDismissed == null || n.IsDismissed == false)
                && (n.Priority == "HIGH" || n.Priority == "URGENT" || n.ActionRequired == true));

        // Today's attendance (login log)
        var loginToday = await _db.UserLoginLogs
            .Where(l => l.Userid == user.UserId && l.Loginat.HasValue && l.Loginat.Value.Date == todayUtc)
            .OrderBy(l => l.Loginat)
            .Select(l => new { l.Loginat, l.Logoutat })
            .FirstOrDefaultAsync();

        // Active jobs count
        var activeJobs = await _db.TrnJobs
            .CountAsync(j => j.StatusCode != "COMPLETED" && j.StatusCode != "CLOSED" && j.StatusCode != "CANCELLED");

        // Today's enquiries
        var todayEnquiries = await _db.TrnEnquiries
            .CountAsync(e => e.CreatedOn.HasValue && e.CreatedOn.Value.Date == today);

        return Ok(new
        {
            TasksAssigned = tasksAssigned,
            PendingApprovals = pendingApprovals,
            TodayActivities = todayActivities,
            UnreadNotifications = unreadNotifications,
            AlertCount = alertCount,
            ActiveJobs = activeJobs,
            TodayEnquiries = todayEnquiries,
            Attendance = loginToday != null ? new
            {
                CheckedIn = true,
                LoginTime = loginToday.Loginat?.ToString("HH:mm"),
                LogoutTime = loginToday.Logoutat?.ToString("HH:mm"),
                Status = loginToday.Logoutat.HasValue ? "Completed" : "Active"
            } : new
            {
                CheckedIn = false,
                LoginTime = (string?)null,
                LogoutTime = (string?)null,
                Status = "Not Logged In"
            }
        });
    }

    /// <summary>
    /// Recent activities for the current user (last 20).
    /// </summary>
    [HttpGet("activities")]
    public async Task<IActionResult> GetRecentActivities()
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized();

        var activities = await _db.TrnUserActivityLogs
            .Where(a => a.UserId == user.UserId)
            .OrderByDescending(a => a.ActivityOn)
            .Take(20)
            .Select(a => new
            {
                a.ActivityLogId,
                a.Module,
                a.ActivityType,
                a.Title,
                a.Description,
                a.Severity,
                ActivityOn = a.ActivityOn.ToString("dd-MMM HH:mm"),
                a.EntityType,
                a.EntityCode
            })
            .ToListAsync();

        return Ok(activities);
    }

    /// <summary>
    /// Department-specific KPIs based on department code.
    /// </summary>
    [HttpGet("kpis")]
    public async Task<IActionResult> GetDepartmentKpis()
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized();

        var dept = await _db.MstDepartments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.DeptId == user.DepartmentId);

        var deptCode = dept?.DeptCode ?? "GEN";
        var kpis = new List<object>();

        switch (deptCode)
        {
            case "SAL":
            case "CST":
            case "EST":
                // Sales KPIs
                var totalEnq = await _db.TrnEnquiries.CountAsync();
                var openEnq = await _db.TrnEnquiries.CountAsync(e => e.Status != "CLOSED" && e.Status != "CANCELLED");
                var totalQuot = await _db.TrnQuotations.CountAsync();
                var approvedQuot = await _db.TrnQuotations.CountAsync(q => q.Status == "APPROVED");
                kpis.Add(new { Label = "Total Enquiries", Value = totalEnq, Icon = "bi-clipboard-data", Color = "primary" });
                kpis.Add(new { Label = "Open Enquiries", Value = openEnq, Icon = "bi-hourglass-split", Color = "warning" });
                kpis.Add(new { Label = "Total Quotations", Value = totalQuot, Icon = "bi-file-earmark-text", Color = "info" });
                kpis.Add(new { Label = "Approved Quotations", Value = approvedQuot, Icon = "bi-check-circle", Color = "success" });
                break;

            case "PRE":
            case "PRT":
            case "FINP":
            case "PKG":
            case "DSP":
                // Production KPIs
                var activeJobsProd = await _db.TrnJobs.CountAsync(j => j.StatusCode != "COMPLETED" && j.StatusCode != "CLOSED" && j.StatusCode != "CANCELLED");
                var completedToday = await _db.TrnJobs.CountAsync(j => j.CompletedOn.HasValue && j.CompletedOn.Value.Date == DateTime.Today);
                var urgentJobs = await _db.TrnJobs.CountAsync(j => j.Priority == "URGENT" && j.StatusCode != "COMPLETED" && j.StatusCode != "CLOSED");
                var totalMachines = await _db.MstMachines.CountAsync(m => m.IsActive == true);
                kpis.Add(new { Label = "Active Jobs", Value = activeJobsProd, Icon = "bi-gear-wide-connected", Color = "primary" });
                kpis.Add(new { Label = "Completed Today", Value = completedToday, Icon = "bi-check2-all", Color = "success" });
                kpis.Add(new { Label = "Urgent Jobs", Value = urgentJobs, Icon = "bi-exclamation-triangle", Color = "danger" });
                kpis.Add(new { Label = "Active Machines", Value = totalMachines, Icon = "bi-cpu", Color = "info" });
                break;

            case "FIN":
                // Finance KPIs
                var totalInvoices = await _db.TrnSalesInvoices.CountAsync();
                var pendingPayments = await _db.TrnPayments.CountAsync(p => p.Status == "PENDING");
                var totalReceipts = await _db.TrnReceipts.CountAsync();
                var pendingChallans = await _db.TrnChallans.CountAsync(c => c.Status == "PENDING");
                kpis.Add(new { Label = "Total Invoices", Value = totalInvoices, Icon = "bi-receipt", Color = "primary" });
                kpis.Add(new { Label = "Pending Payments", Value = pendingPayments, Icon = "bi-cash-stack", Color = "warning" });
                kpis.Add(new { Label = "Total Receipts", Value = totalReceipts, Icon = "bi-wallet2", Color = "success" });
                kpis.Add(new { Label = "Pending Challans", Value = pendingChallans, Icon = "bi-truck", Color = "info" });
                break;

            case "INV":
            case "PUR":
                // Inventory & Purchase KPIs
                var totalItems = await _db.VwMstItems.CountAsync(i => i.IsActive == true);
                var totalSuppliers = await _db.MstSuppliers.CountAsync(s => s.IsActive == true);
                var pendingGatePasses = await _db.TrnGatePasses.CountAsync(g => g.Status == "PENDING");
                var outsourceItems = await _db.TrnJobOutsources.CountAsync(o => o.Status == "PENDING");
                kpis.Add(new { Label = "Active Items", Value = totalItems, Icon = "bi-box-seam", Color = "primary" });
                kpis.Add(new { Label = "Active Suppliers", Value = totalSuppliers, Icon = "bi-shop", Color = "info" });
                kpis.Add(new { Label = "Pending Gate Pass", Value = pendingGatePasses, Icon = "bi-door-open", Color = "warning" });
                kpis.Add(new { Label = "Pending Outsource", Value = outsourceItems, Icon = "bi-arrow-left-right", Color = "danger" });
                break;

            case "QMS":
                // Quality KPIs
                var jobsInQc = await _db.TrnJobs.CountAsync(j => j.CurrentStage == "QC" || j.CurrentStage == "QUALITY");
                var totalJobsActive = await _db.TrnJobs.CountAsync(j => j.StatusCode != "COMPLETED" && j.StatusCode != "CLOSED" && j.StatusCode != "CANCELLED");
                var completedJobs = await _db.TrnJobs.CountAsync(j => j.StatusCode == "COMPLETED");
                kpis.Add(new { Label = "Jobs in QC", Value = jobsInQc, Icon = "bi-shield-check", Color = "warning" });
                kpis.Add(new { Label = "Active Jobs", Value = totalJobsActive, Icon = "bi-gear", Color = "primary" });
                kpis.Add(new { Label = "Completed Jobs", Value = completedJobs, Icon = "bi-patch-check", Color = "success" });
                kpis.Add(new { Label = "Pass Rate", Value = "—", Icon = "bi-graph-up", Color = "info" });
                break;

            case "HR":
                // HR KPIs
                var totalEmployees = await _db.MstEmployees.CountAsync(e => e.IsActive == true);
                var totalUsers = await _db.MstUsers.CountAsync(u => u.Isactive == true);
                var todayLogins = await _db.UserLoginLogs.CountAsync(l => l.Loginat.HasValue && l.Loginat.Value.Date == DateTime.UtcNow.Date);
                var lockedUsers = await _db.MstUsers.CountAsync(u => u.Islocked == true);
                kpis.Add(new { Label = "Active Employees", Value = totalEmployees, Icon = "bi-people", Color = "primary" });
                kpis.Add(new { Label = "Active Users", Value = totalUsers, Icon = "bi-person-check", Color = "success" });
                kpis.Add(new { Label = "Today's Logins", Value = todayLogins, Icon = "bi-box-arrow-in-right", Color = "info" });
                kpis.Add(new { Label = "Locked Users", Value = lockedUsers, Icon = "bi-lock", Color = "danger" });
                break;

            case "SEC":
                // Security KPIs
                var todayGatePasses = await _db.TrnGatePasses.CountAsync(g => g.CreatedOn.HasValue && g.CreatedOn.Value.Date == DateTime.Today);
                var pendingGP = await _db.TrnGatePasses.CountAsync(g => g.Status == "PENDING");
                var todayDispatches = await _db.TrnChallans.CountAsync(c => c.ChallanDate == DateOnly.FromDateTime(DateTime.Today));
                kpis.Add(new { Label = "Today Gate Passes", Value = todayGatePasses, Icon = "bi-door-open", Color = "primary" });
                kpis.Add(new { Label = "Pending Approval", Value = pendingGP, Icon = "bi-hourglass", Color = "warning" });
                kpis.Add(new { Label = "Today Dispatches", Value = todayDispatches, Icon = "bi-truck", Color = "info" });
                kpis.Add(new { Label = "Security Status", Value = "Active", Icon = "bi-shield-lock", Color = "success" });
                break;

            case "MNT":
                // Maintenance KPIs
                var totalMachinesMnt = await _db.MstMachines.CountAsync();
                var activeMachines = await _db.MstMachines.CountAsync(m => m.IsActive == true);
                var pendingMaint = await _db.MstMachineMaintenances.CountAsync();
                kpis.Add(new { Label = "Total Machines", Value = totalMachinesMnt, Icon = "bi-gear-wide", Color = "primary" });
                kpis.Add(new { Label = "Active Machines", Value = activeMachines, Icon = "bi-check-circle", Color = "success" });
                kpis.Add(new { Label = "Maintenance Records", Value = pendingMaint, Icon = "bi-wrench-adjustable", Color = "warning" });
                kpis.Add(new { Label = "Uptime", Value = "—", Icon = "bi-graph-up-arrow", Color = "info" });
                break;

            default:
                // MGT / ADM / IT / General — executive overview
                var totalJobsAll = await _db.TrnJobs.CountAsync();
                var activeJobsAll = await _db.TrnJobs.CountAsync(j => j.StatusCode != "COMPLETED" && j.StatusCode != "CLOSED" && j.StatusCode != "CANCELLED");
                var totalEnqAll = await _db.TrnEnquiries.CountAsync();
                var totalCustomers = await _db.MstCustomers.CountAsync(c => c.IsActive == true);
                kpis.Add(new { Label = "Total Jobs", Value = totalJobsAll, Icon = "bi-layers", Color = "primary" });
                kpis.Add(new { Label = "Active Jobs", Value = activeJobsAll, Icon = "bi-gear-wide-connected", Color = "warning" });
                kpis.Add(new { Label = "Total Enquiries", Value = totalEnqAll, Icon = "bi-clipboard-data", Color = "info" });
                kpis.Add(new { Label = "Active Customers", Value = totalCustomers, Icon = "bi-people", Color = "success" });
                break;
        }

        return Ok(kpis);
    }

    /// <summary>
    /// Action-required items: approvals + urgent notifications.
    /// </summary>
    [HttpGet("actions")]
    public async Task<IActionResult> GetPendingActions()
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized();

        var actions = await _db.TrnUserNotifications
            .Where(n => n.UserId == user.UserId
                && n.ActionRequired == true
                && (n.IsDismissed == null || n.IsDismissed == false))
            .OrderByDescending(n => n.CreatedOn)
            .Take(10)
            .Select(n => new
            {
                n.UserNotificationId,
                n.Title,
                n.Message,
                n.Icon,
                n.Color,
                n.ActionUrl,
                n.ActionLabel,
                n.Priority,
                CreatedOn = n.CreatedOn.HasValue ? n.CreatedOn.Value.ToString("dd-MMM HH:mm") : ""
            })
            .ToListAsync();

        return Ok(actions);
    }

    /// <summary>
    /// AI-generated / smart alerts for the agentic notification panel.
    /// Combines AI agent activities and AI notification logs.
    /// </summary>
    [HttpGet("ai-alerts")]
    public async Task<IActionResult> GetAiAlerts()
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized();

        // AI agent activities (agentic smart alerts)
        var agentAlerts = await _db.TrnAiAgentActivities
            .OrderByDescending(a => a.CreatedOn)
            .Take(5)
            .Select(a => new
            {
                Id = a.ActivityId,
                Title = a.AgentName + " — " + a.AgentAction,
                Message = a.OutputJson,
                a.Module,
                Confidence = a.ConfidenceScore,
                CreatedOn = a.CreatedOn.HasValue ? a.CreatedOn.Value.ToString("dd-MMM HH:mm") : "",
                Source = "AGENT"
            })
            .ToListAsync();

        // AI notification logs
        var aiLogs = await _db.TrnAiNotificationLogs
            .OrderByDescending(a => a.CreatedOn)
            .Take(5)
            .Select(a => new
            {
                Id = a.AiLogId,
                Title = a.AiAction,
                Message = a.AiResponse,
                Module = (string?)null,
                Confidence = a.AiConfidence,
                CreatedOn = a.CreatedOn.HasValue ? a.CreatedOn.Value.ToString("dd-MMM HH:mm") : "",
                Source = "AI_LOG"
            })
            .ToListAsync();

        var combined = agentAlerts.Concat(aiLogs)
            .OrderByDescending(x => x.CreatedOn)
            .Take(8)
            .ToList();

        return Ok(combined);
    }

    /// <summary>
    /// HRMS summary for the logged-in user — leaves, loan, advance, attendance, shift, etc.
    /// </summary>
    [HttpGet("hrms")]
    public async Task<IActionResult> GetMyHrms()
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var currentYear = DateTime.Today.Year.ToString();

        // Find employee by code or user mapping
        var employee = await _db.MstEmployees
            .AsNoTracking()
            .Where(e => e.EmpCode == user.EmployeeCode && e.IsActive == true)
            .Select(e => new { e.EmployeeId, e.EmpCode, e.FirstName, e.LastName, e.DateOfJoining })
            .FirstOrDefaultAsync();

        var empId = employee?.EmployeeId ?? 0;

        // Leave balance
        var leaveBalances = await _db.HrLeaveBalances
            .AsNoTracking()
            .Where(lb => lb.EmployeeId == empId)
            .Join(_db.HrLeaveTypes, lb => lb.LeaveTypeId, lt => lt.LeaveTypeId,
                (lb, lt) => new { LeaveTypeName = lt.LeaveName, lb.OpeningBalance, lb.Availed, lb.ClosingBalance })
            .ToListAsync();

        // Pending leave requests
        var pendingLeaves = await _db.HrLeaveRequests
            .CountAsync(lr => lr.EmployeeId == empId && (lr.Status == "PENDING" || lr.Status == "SUBMITTED"));

        // Active loans
        var activeLoans = await _db.HrLoans
            .AsNoTracking()
            .Where(l => l.EmployeeId == empId && l.Status == "ACTIVE")
            .Select(l => new { l.LoanNo, l.LoanType, l.LoanAmount, l.OutstandingAmount, l.EmiAmount })
            .ToListAsync();

        // Salary advance
        var pendingAdvances = await _db.HrSalaryAdvances
            .AsNoTracking()
            .Where(a => a.EmployeeId == empId && (a.Status == "PENDING" || a.Status == "ACTIVE"))
            .Select(a => new { a.AdvanceNo, a.AdvanceAmount, a.BalanceAmount, a.Status })
            .ToListAsync();

        // Today's attendance
        var todayAttendance = await _db.HybEmployeeAttendances
            .AsNoTracking()
            .Where(a => a.EmployeeId == empId && a.AttendanceDate == today)
            .Select(a => new { a.CheckIn, a.CheckOut, a.TotalHours, a.Status, a.OvertimeHours })
            .FirstOrDefaultAsync();

        // Current shift
        var currentShift = await _db.HrShiftRosters
            .AsNoTracking()
            .Where(sr => sr.EmployeeId == empId && sr.IsActive && sr.EffectiveFrom <= today
                && (sr.EffectiveTo == null || sr.EffectiveTo >= today))
            .Join(_db.MstShiftTypes, sr => sr.ShiftTypeId, st => st.ShiftTypeId,
                (sr, st) => new { st.ShiftName, st.ShiftStartTime, st.ShiftEndTime, sr.WeekOffDays })
            .FirstOrDefaultAsync();

        // Overtime this month
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var overtimeHours = await _db.HrOvertimes
            .Where(o => o.EmployeeId == empId && o.OtDate >= monthStart && o.OtDate <= today && o.Status == "APPROVED")
            .SumAsync(o => o.OtHours ?? 0);

        // Medical claims this year
        var medicalClaims = await _db.HrMedicalClaims
            .Where(m => m.EmployeeId == empId && m.ClaimDate.Year == today.Year)
            .SumAsync(m => m.ApprovedAmount ?? 0);

        // Reimbursement claims this year
        var reimbursementClaims = await _db.HrReimbursements
            .AsNoTracking()
            .Where(r => r.EmployeeId == empId && r.ClaimDate.Year == today.Year)
            .Select(r => new { r.ClaimAmount, r.ApprovedAmount, r.Status })
            .ToListAsync();

        var reimbClaimedTotal = reimbursementClaims.Sum(r => r.ClaimAmount);
        var reimbApprovedTotal = reimbursementClaims.Sum(r => r.ApprovedAmount ?? 0m);
        var reimbPendingCount = reimbursementClaims.Count(r => r.Status == "PENDING" || r.Status == "SUBMITTED");

        // Upcoming holidays
        var upcomingHolidays = await _db.HrHolidays
            .AsNoTracking()
            .Where(h => h.HolidayDate >= today && h.IsActive)
            .OrderBy(h => h.HolidayDate)
            .Take(3)
            .Select(h => new { h.HolidayName, HolidayDate = h.HolidayDate.ToString("dd-MMM"), h.HolidayType })
            .ToListAsync();

        // Recent incentives
        var recentIncentives = await _db.HrIncentives
            .AsNoTracking()
            .Where(i => i.EmployeeId == empId && i.Status == "APPROVED")
            .OrderByDescending(i => i.IncentiveDate)
            .Take(2)
            .Select(i => new { i.IncentiveType, i.IncentiveAmount, IncentiveDate = i.IncentiveDate.ToString("dd-MMM") })
            .ToListAsync();

        return Ok(new
        {
            Employee = employee != null ? new
            {
                employee.EmpCode,
                Name = $"{employee.FirstName} {employee.LastName}".Trim(),
                JoinDate = employee.DateOfJoining?.ToString("dd-MMM-yyyy")
            } : null,
            LeaveBalances = leaveBalances,
            PendingLeaves = pendingLeaves,
            ActiveLoans = activeLoans,
            PendingAdvances = pendingAdvances,
            TodayAttendance = todayAttendance,
            CurrentShift = currentShift,
            OvertimeHoursThisMonth = overtimeHours,
            MedicalClaimsThisYear = medicalClaims,
            ReimbursementClaimsThisYear = reimbClaimedTotal,
            ReimbursementApprovedThisYear = reimbApprovedTotal,
            PendingReimbursements = reimbPendingCount,
            UpcomingHolidays = upcomingHolidays,
            RecentIncentives = recentIncentives
        });
    }

    /// <summary>
    /// HRMS overview for Top Management / Administration — across all employees.
    /// </summary>
    [HttpGet("hrms-overview")]
    public async Task<IActionResult> GetHrmsOverview()
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized();

        // Only MGT, ADM, or system admin can access
        var dept = await _db.MstDepartments
            .AsNoTracking()
            .Where(d => d.DeptId == user.DepartmentId)
            .Select(d => d.DeptCode)
            .FirstOrDefaultAsync();

        if (dept != "MGT" && dept != "ADM" && !user.IsSystemAdmin)
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "Access denied." });

        var today = DateOnly.FromDateTime(DateTime.Today);
        var totalEmployees = await _db.MstEmployees.CountAsync(e => e.IsActive == true);

        // Today present (have attendance record with checkin)
        var presentToday = await _db.HybEmployeeAttendances
            .CountAsync(a => a.AttendanceDate == today && a.CheckIn != null);

        // Absent today = total - present
        var absentToday = totalEmployees - presentToday;

        // On leave today
        var onLeaveToday = await _db.HrLeaveRequests
            .CountAsync(lr => lr.Status == "APPROVED" && lr.FromDate <= today && lr.ToDate >= today);

        // Pending leave requests across all
        var pendingLeaveRequests = await _db.HrLeaveRequests
            .CountAsync(lr => lr.Status == "PENDING" || lr.Status == "SUBMITTED");

        // Active loans count
        var activeLoansCount = await _db.HrLoans
            .CountAsync(l => l.Status == "ACTIVE");

        // Total outstanding loan amount
        var totalLoanOutstanding = await _db.HrLoans
            .Where(l => l.Status == "ACTIVE")
            .SumAsync(l => l.OutstandingAmount ?? 0);

        // Pending salary advances
        var pendingAdvances = await _db.HrSalaryAdvances
            .CountAsync(a => a.Status == "PENDING");

        // Pending overtime approvals
        var pendingOvertimes = await _db.HrOvertimes
            .CountAsync(o => o.Status == "PENDING");

        // Pending medical claims
        var pendingMedicals = await _db.HrMedicalClaims
            .CountAsync(m => m.Status == "PENDING" || m.Status == "SUBMITTED");

        // Pending resignations
        var pendingResignations = await _db.HrResignations
            .CountAsync(r => r.Status == "PENDING" || r.Status == "SUBMITTED");

        // Pending transfers
        var pendingTransfers = await _db.HrTransfers
            .CountAsync(t => t.Status == "PENDING");

        // Pending travel expenses
        var pendingTravels = await _db.HrTravelExpenses
            .CountAsync(t => t.Status == "PENDING" || t.Status == "SUBMITTED");

        // Pending reimbursements
        var pendingReimbursements = await _db.HrReimbursements
            .CountAsync(r => r.Status == "PENDING" || r.Status == "SUBMITTED");

        // Upcoming holidays
        var nextHoliday = await _db.HrHolidays
            .AsNoTracking()
            .Where(h => h.HolidayDate >= today && h.IsActive)
            .OrderBy(h => h.HolidayDate)
            .Select(h => new { h.HolidayName, HolidayDate = h.HolidayDate.ToString("dd-MMM") })
            .FirstOrDefaultAsync();

        return Ok(new
        {
            TotalEmployees = totalEmployees,
            PresentToday = presentToday,
            AbsentToday = absentToday,
            OnLeaveToday = onLeaveToday,
            PendingLeaveRequests = pendingLeaveRequests,
            ActiveLoans = activeLoansCount,
            TotalLoanOutstanding = totalLoanOutstanding,
            PendingAdvances = pendingAdvances,
            PendingOvertimes = pendingOvertimes,
            PendingMedicalClaims = pendingMedicals,
            PendingResignations = pendingResignations,
            PendingTransfers = pendingTransfers,
            PendingTravelExpenses = pendingTravels,
            PendingReimbursements = pendingReimbursements,
            NextHoliday = nextHoliday
        });
    }

    /// <summary>
    /// AI-powered smart suggestions based on user's HRMS and work data.
    /// </summary>
    [HttpGet("ai-suggestions")]
    public async Task<IActionResult> GetAiSuggestions()
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var suggestions = new List<object>();

        var employee = await _db.MstEmployees
            .AsNoTracking()
            .Where(e => e.EmpCode == user.EmployeeCode && e.IsActive == true)
            .Select(e => new { e.EmployeeId, e.DateOfJoining })
            .FirstOrDefaultAsync();

        var empId = employee?.EmployeeId ?? 0;

        // Check leave balance running low
        var lowLeave = await _db.HrLeaveBalances
            .AsNoTracking()
            .Where(lb => lb.EmployeeId == empId && lb.ClosingBalance <= 2 && lb.ClosingBalance > 0)
            .Join(_db.HrLeaveTypes, lb => lb.LeaveTypeId, lt => lt.LeaveTypeId,
                (lb, lt) => new { LeaveTypeName = lt.LeaveName, lb.ClosingBalance })
            .FirstOrDefaultAsync();

        if (lowLeave != null)
        {
            suggestions.Add(new
            {
                Icon = "bi-calendar-x",
                Color = "warning",
                Title = "Low Leave Balance",
                Message = $"Your {lowLeave.LeaveTypeName} balance is only {lowLeave.ClosingBalance} days. Plan accordingly.",
                Category = "LEAVE",
                Priority = "MEDIUM"
            });
        }

        // Loan EMI reminder
        var activeLoan = await _db.HrLoans
            .AsNoTracking()
            .Where(l => l.EmployeeId == empId && l.Status == "ACTIVE")
            .Select(l => new { l.LoanType, l.EmiAmount, l.OutstandingAmount })
            .FirstOrDefaultAsync();

        if (activeLoan != null)
        {
            suggestions.Add(new
            {
                Icon = "bi-bank",
                Color = "info",
                Title = "Loan EMI Active",
                Message = $"{activeLoan.LoanType} loan: EMI ₹{activeLoan.EmiAmount:N0} | Outstanding ₹{activeLoan.OutstandingAmount:N0}",
                Category = "LOAN",
                Priority = "LOW"
            });
        }

        // Pending advance recovery
        var pendingAdv = await _db.HrSalaryAdvances
            .AsNoTracking()
            .Where(a => a.EmployeeId == empId && a.Status == "ACTIVE" && a.BalanceAmount > 0)
            .Select(a => new { a.BalanceAmount, a.MonthlyDeduction })
            .FirstOrDefaultAsync();

        if (pendingAdv != null)
        {
            suggestions.Add(new
            {
                Icon = "bi-cash-stack",
                Color = "azure",
                Title = "Advance Recovery",
                Message = $"Salary advance balance ₹{pendingAdv.BalanceAmount:N0} (Monthly deduction ₹{pendingAdv.MonthlyDeduction:N0})",
                Category = "ADVANCE",
                Priority = "LOW"
            });
        }

        // Upcoming holiday alert
        var nextHoliday = await _db.HrHolidays
            .AsNoTracking()
            .Where(h => h.HolidayDate >= today && h.HolidayDate <= today.AddDays(7) && h.IsActive)
            .OrderBy(h => h.HolidayDate)
            .Select(h => new { h.HolidayName, h.HolidayDate })
            .FirstOrDefaultAsync();

        if (nextHoliday != null)
        {
            var daysAway = nextHoliday.HolidayDate.DayNumber - today.DayNumber;
            suggestions.Add(new
            {
                Icon = "bi-calendar-event",
                Color = "success",
                Title = "Upcoming Holiday",
                Message = $"{nextHoliday.HolidayName} on {nextHoliday.HolidayDate:dd-MMM} ({(daysAway == 0 ? "Today!" : daysAway == 1 ? "Tomorrow" : $"in {daysAway} days")})",
                Category = "HOLIDAY",
                Priority = daysAway <= 1 ? "HIGH" : "LOW"
            });
        }

        // Overtime suggestion
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var otHours = await _db.HrOvertimes
            .Where(o => o.EmployeeId == empId && o.OtDate >= monthStart && o.Status == "APPROVED")
            .SumAsync(o => o.OtHours ?? 0);

        if (otHours > 20)
        {
            suggestions.Add(new
            {
                Icon = "bi-clock-fill",
                Color = "warning",
                Title = "High Overtime",
                Message = $"You've logged {otHours:N1}hrs overtime this month. Consider work-life balance.",
                Category = "OVERTIME",
                Priority = "MEDIUM"
            });
        }

        // Attendance streak check
        var recentAbsent = await _db.HybEmployeeAttendances
            .CountAsync(a => a.EmployeeId == empId
                && a.AttendanceDate >= today.AddDays(-7) && a.AttendanceDate < today
                && a.Status == "ABSENT");

        if (recentAbsent == 0)
        {
            suggestions.Add(new
            {
                Icon = "bi-trophy",
                Color = "success",
                Title = "Perfect Attendance",
                Message = "Great job! You've had perfect attendance this week. Keep it up!",
                Category = "ATTENDANCE",
                Priority = "LOW"
            });
        }

        // Medical claim reminder if eligible
        var hasMedical = await _db.HrMedicalClaims
            .AnyAsync(m => m.EmployeeId == empId && m.ClaimDate.Year == today.Year);

        if (!hasMedical && today.Month >= 6)
        {
            suggestions.Add(new
            {
                Icon = "bi-heart-pulse",
                Color = "danger",
                Title = "Medical Benefits",
                Message = "You haven't claimed medical benefits this year. Check your eligibility.",
                Category = "MEDICAL",
                Priority = "LOW"
            });
        }

        // Pending reimbursement claims
        var pendingReimb = await _db.HrReimbursements
            .CountAsync(r => r.EmployeeId == empId && (r.Status == "PENDING" || r.Status == "SUBMITTED"));

        if (pendingReimb > 0)
        {
            suggestions.Add(new
            {
                Icon = "bi-receipt-cutoff",
                Color = "info",
                Title = "Pending Reimbursements",
                Message = $"You have {pendingReimb} reimbursement claim(s) awaiting approval.",
                Category = "REIMBURSEMENT",
                Priority = "MEDIUM"
            });
        }

        // Pending tasks reminder
        var pendingTasks = await _db.TrnJobs
            .CountAsync(j => j.AssignedTo == user.UserId
                && j.StatusCode != "COMPLETED" && j.StatusCode != "CLOSED");

        if (pendingTasks > 5)
        {
            suggestions.Add(new
            {
                Icon = "bi-list-task",
                Color = "primary",
                Title = "Task Backlog",
                Message = $"You have {pendingTasks} pending tasks. Prioritize urgent ones first.",
                Category = "TASKS",
                Priority = "HIGH"
            });
        }

        return Ok(suggestions.OrderByDescending(s => ((dynamic)s).Priority == "HIGH")
            .ThenByDescending(s => ((dynamic)s).Priority == "MEDIUM")
            .Take(6)
            .ToList());
    }

    // ════════════════════════════════════════════
    //  PUNCH IN / PUNCH OUT  —  Attendance Widget
    // ════════════════════════════════════════════

    /// <summary>
    /// Returns today's punch status for the logged-in employee.
    /// </summary>
    [HttpGet("punch-status")]
    public async Task<IActionResult> GetPunchStatus()
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var empId = await ResolveEmployeeId(user.EmployeeCode);
        if (empId == 0) return Ok(new { HasEmployee = false });

        var att = await _db.HybEmployeeAttendances
            .AsNoTracking()
            .Where(a => a.EmployeeId == empId && a.AttendanceDate == today)
            .Select(a => new
            {
                a.AttendanceId,
                CheckIn = a.CheckIn.HasValue ? a.CheckIn.Value.ToString("HH:mm:ss") : (string?)null,
                CheckOut = a.CheckOut.HasValue ? a.CheckOut.Value.ToString("HH:mm:ss") : (string?)null,
                CheckInRaw = a.CheckIn,
                CheckOutRaw = a.CheckOut,
                a.TotalHours,
                a.Status,
                a.BreakMinutes,
                a.Remarks
            })
            .FirstOrDefaultAsync();

        // Determine state: NOT_PUNCHED, PUNCHED_IN, PUNCHED_OUT
        string punchState;
        if (att == null)
            punchState = "NOT_PUNCHED";
        else if (att.CheckOut == null)
            punchState = "PUNCHED_IN";
        else
            punchState = "PUNCHED_OUT";

        // Calculate work duration so far (in seconds)
        double workSeconds = 0;
        if (att?.CheckInRaw != null)
        {
            var endTime = att.CheckOutRaw ?? DateTime.Now;
            workSeconds = (endTime - att.CheckInRaw.Value).TotalSeconds;
            if (workSeconds < 0) workSeconds = 0;
        }

        // Current shift info
        var shift = await _db.HrShiftRosters
            .AsNoTracking()
            .Where(sr => sr.EmployeeId == empId && sr.IsActive && sr.EffectiveFrom <= today
                && (sr.EffectiveTo == null || sr.EffectiveTo >= today))
            .Join(_db.MstShiftTypes, sr => sr.ShiftTypeId, st => st.ShiftTypeId,
                (sr, st) => new { st.ShiftName, st.ShiftStartTime, st.ShiftEndTime })
            .FirstOrDefaultAsync();

        return Ok(new
        {
            HasEmployee = true,
            PunchState = punchState,
            CheckIn = att?.CheckInRaw?.ToString("yyyy-MM-ddTHH:mm:ss"),
            CheckOut = att?.CheckOutRaw?.ToString("yyyy-MM-ddTHH:mm:ss"),
            CheckInDisplay = att?.CheckInRaw?.ToString("hh:mm tt"),
            CheckOutDisplay = att?.CheckOutRaw?.ToString("hh:mm tt"),
            TotalHours = att?.TotalHours,
            WorkSeconds = workSeconds,
            Status = att?.Status ?? "NOT_PUNCHED",
            BreakMinutes = att?.BreakMinutes ?? 0,
            Remarks = att?.Remarks,
            Shift = shift != null ? new { shift.ShiftName, shift.ShiftStartTime, shift.ShiftEndTime } : null,
            ServerTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")
        });
    }

    /// <summary>
    /// Punch In — creates today's attendance record with CheckIn timestamp.
    /// </summary>
    [HttpPost("punch-in")]
    public async Task<IActionResult> PunchIn()
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var now = DateTime.Now;
        var empId = await ResolveEmployeeId(user.EmployeeCode);
        if (empId == 0) return BadRequest(new { message = "Employee record not found." });

        // Check if already punched in today
        var existing = await _db.HybEmployeeAttendances
            .FirstOrDefaultAsync(a => a.EmployeeId == empId && a.AttendanceDate == today);

        if (existing != null)
        {
            if (existing.CheckIn != null)
                return BadRequest(new { message = "Already punched in today." });

            existing.CheckIn = now;
            existing.Status = "PRESENT";
            existing.ModifiedOn = now;
        }
        else
        {
            // Resolve department
            var deptId = await _db.MstEmployees
                .Where(e => e.EmployeeId == empId)
                .Select(e => e.DeptId)
                .FirstOrDefaultAsync();

            // Resolve shift
            var shiftId = await _db.HrShiftRosters
                .Where(sr => sr.EmployeeId == empId && sr.IsActive && sr.EffectiveFrom <= today
                    && (sr.EffectiveTo == null || sr.EffectiveTo >= today))
                .Select(sr => sr.ShiftTypeId)
                .FirstOrDefaultAsync();

            var attendance = new persistence.Models.HybEmployeeAttendance
            {
                EmployeeId = empId,
                DepartmentId = deptId,
                ShiftTypeId = shiftId > 0 ? shiftId : null,
                AttendanceDate = today,
                CheckIn = now,
                Status = "PRESENT",
                BreakMinutes = 0,
                AttendanceData = System.Text.Json.JsonSerializer.Serialize(new
                {
                    punchSource = "DASHBOARD",
                    userAgent = Request.Headers.UserAgent.ToString(),
                    ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                }),
                CreatedOn = now,
                ModifiedOn = now
            };
            _db.HybEmployeeAttendances.Add(attendance);
        }

        await _db.SaveChangesAsync();

        // Log activity & notification for punch-in
        var actEntry = ActivityLogEntry.FromUser(user, "HRMS", "PUNCH_IN", "Punched In");
        actEntry.SubModule = "ATTENDANCE";
        actEntry.Description = $"Punched in at {now:hh:mm tt} on {today:dd-MMM-yyyy}";
        actEntry.EntityType = "ATTENDANCE";
        actEntry.ActivityCategory = "ATTENDANCE";
        await _activity.LogActivityAsync(actEntry);

        await _activity.LogNotificationAsync(new UserNotificationEntry
        {
            UserId = user.UserId,
            Title = "Punched In",
            Message = $"You punched in at {now:hh:mm tt}. Have a productive day!",
            Icon = "bi-box-arrow-in-right",
            Color = "success",
            Module = "HRMS",
            EventType = "PUNCH_IN",
            Priority = "LOW"
        });

        // Send email notification for punch-in
        if (!string.IsNullOrEmpty(user.EmailId))
        {
            _ = _notifier.SendEmailAsync(
                user.EmailId,
                "Attendance: Punched In",
                $"<h3>Punch In Confirmation</h3>"
                + $"<p>Hi <b>{user.Name}</b>,</p>"
                + $"<p>You have successfully punched in at <b>{now:hh:mm tt}</b> on <b>{today:dd-MMM-yyyy}</b>.</p>"
                + $"<p>Have a productive day!</p>"
                + $"<hr/><small style='color:#888;'>MinePress ERP — Automated Attendance Notification</small>");
        }

        return Ok(new
        {
            message = "Punched in successfully!",
            checkIn = now.ToString("HH:mm:ss"),
            serverTime = now.ToString("yyyy-MM-ddTHH:mm:ss")
        });
    }

    /// <summary>
    /// Punch Out — updates today's attendance record with CheckOut timestamp.
    /// </summary>
    [HttpPost("punch-out")]
    public async Task<IActionResult> PunchOut()
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var now = DateTime.Now;
        var empId = await ResolveEmployeeId(user.EmployeeCode);
        if (empId == 0) return BadRequest(new { message = "Employee record not found." });

        var att = await _db.HybEmployeeAttendances
            .FirstOrDefaultAsync(a => a.EmployeeId == empId && a.AttendanceDate == today);

        if (att == null || att.CheckIn == null)
            return BadRequest(new { message = "You must punch in first." });

        if (att.CheckOut != null)
            return BadRequest(new { message = "Already punched out today." });

        att.CheckOut = now;
        att.ModifiedOn = now;

        // Calculate total hours for display (DB column is generated, but we still track it)
        var totalHrs = (now - att.CheckIn.Value).TotalHours - ((att.BreakMinutes ?? 0) / 60.0);
        if (totalHrs < 0) totalHrs = 0;

        await _db.SaveChangesAsync();

        // Log activity & notification for punch-out
        var actEntry = ActivityLogEntry.FromUser(user, "HRMS", "PUNCH_OUT", "Punched Out");
        actEntry.SubModule = "ATTENDANCE";
        actEntry.Description = $"Punched out at {now:hh:mm tt}. Total hours: {Math.Round(totalHrs, 2)}";
        actEntry.EntityType = "ATTENDANCE";
        actEntry.ActivityCategory = "ATTENDANCE";
        await _activity.LogActivityAsync(actEntry);

        await _activity.LogNotificationAsync(new UserNotificationEntry
        {
            UserId = user.UserId,
            Title = "Punched Out",
            Message = $"You punched out at {now:hh:mm tt}. Total work: {Math.Round(totalHrs, 2)} hrs. Good job!",
            Icon = "bi-box-arrow-right",
            Color = "info",
            Module = "HRMS",
            EventType = "PUNCH_OUT",
            Priority = "LOW"
        });

        // Send email notification for punch-out
        if (!string.IsNullOrEmpty(user.EmailId))
        {
            _ = _notifier.SendEmailAsync(
                user.EmailId,
                "Attendance: Punched Out",
                $"<h3>Punch Out Confirmation</h3>"
                + $"<p>Hi <b>{user.Name}</b>,</p>"
                + $"<p>You have successfully punched out at <b>{now:hh:mm tt}</b> on <b>{today:dd-MMM-yyyy}</b>.</p>"
                + $"<p>Total work hours: <b>{Math.Round(totalHrs, 2)} hrs</b></p>"
                + $"<p>Great job today!</p>"
                + $"<hr/><small style='color:#888;'>MinePress ERP — Automated Attendance Notification</small>");
        }

        return Ok(new
        {
            message = "Punched out successfully!",
            checkOut = now.ToString("HH:mm:ss"),
            totalHours = Math.Round(totalHrs, 2),
            serverTime = now.ToString("yyyy-MM-ddTHH:mm:ss")
        });
    }

    /// <summary>
    /// AI-powered attendance insights: work patterns, streaks, averages, predictions.
    /// </summary>
    [HttpGet("punch-ai-insights")]
    public async Task<IActionResult> GetPunchAiInsights()
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var empId = await ResolveEmployeeId(user.EmployeeCode);
        if (empId == 0) return Ok(new { insights = Array.Empty<object>() });

        var last30Days = today.AddDays(-30);
        var recentAttendance = await _db.HybEmployeeAttendances
            .AsNoTracking()
            .Where(a => a.EmployeeId == empId && a.AttendanceDate >= last30Days && a.AttendanceDate <= today)
            .OrderByDescending(a => a.AttendanceDate)
            .Select(a => new { a.AttendanceDate, a.CheckIn, a.CheckOut, a.TotalHours, a.Status })
            .ToListAsync();

        var insights = new List<object>();

        // 1. Attendance streak
        var presentDays = recentAttendance.Count(a => a.Status == "PRESENT");
        var streak = 0;
        foreach (var a in recentAttendance.OrderByDescending(x => x.AttendanceDate))
        {
            if (a.Status == "PRESENT") streak++;
            else break;
        }
        insights.Add(new
        {
            Icon = "bi-fire",
            Color = streak >= 20 ? "success" : streak >= 10 ? "warning" : "info",
            Title = $"{streak}-Day Streak",
            Message = streak >= 20 ? "Outstanding! You're on fire! 🔥" : streak >= 10 ? "Great consistency! Keep going!" : "Building your streak, keep showing up!",
            Type = "STREAK"
        });

        // 2. Average check-in time
        var checkIns = recentAttendance.Where(a => a.CheckIn != null).Select(a => a.CheckIn!.Value.TimeOfDay).ToList();
        if (checkIns.Count > 0)
        {
            var avgTicks = (long)checkIns.Average(t => t.Ticks);
            var avgCheckIn = new TimeSpan(avgTicks);
            var avgStr = $"{avgCheckIn.Hours:D2}:{avgCheckIn.Minutes:D2}";
            insights.Add(new
            {
                Icon = "bi-sunrise",
                Color = avgCheckIn.Hours < 9 ? "success" : avgCheckIn.Hours < 10 ? "warning" : "danger",
                Title = $"Avg Check-In: {avgStr}",
                Message = avgCheckIn.Hours < 9 ? "You're an early bird! Excellent punctuality." : avgCheckIn.Hours < 10 ? "On time mostly. Aim for a bit earlier!" : "Consider adjusting your routine for earlier starts.",
                Type = "AVG_CHECKIN"
            });
        }

        // 3. Average working hours
        var workHours = recentAttendance.Where(a => a.TotalHours != null && a.TotalHours > 0).Select(a => (double)a.TotalHours!.Value).ToList();
        if (workHours.Count > 0)
        {
            var avgHours = workHours.Average();
            insights.Add(new
            {
                Icon = "bi-graph-up",
                Color = avgHours >= 8 ? "success" : avgHours >= 6 ? "warning" : "danger",
                Title = $"Avg Hours: {avgHours:F1}h/day",
                Message = avgHours >= 8 ? "Healthy work hours. Excellent balance!" : avgHours >= 6 ? "Slightly below target. Consider optimizing your schedule." : "Work hours are low. Check if there are obstacles.",
                Type = "AVG_HOURS"
            });
        }

        // 4. Attendance rate
        if (presentDays > 0)
        {
            var totalWorkDays = recentAttendance.Count;
            var rate = totalWorkDays > 0 ? (presentDays * 100.0 / totalWorkDays) : 0;
            insights.Add(new
            {
                Icon = "bi-pie-chart",
                Color = rate >= 95 ? "success" : rate >= 80 ? "warning" : "danger",
                Title = $"Attendance: {rate:F0}%",
                Message = $"{presentDays} of {totalWorkDays} days present in last 30 days.",
                Type = "ATTENDANCE_RATE"
            });
        }

        // 5. Late arrival detection
        var lateDays = checkIns.Count(t => t.Hours >= 10);
        if (lateDays > 3)
        {
            insights.Add(new
            {
                Icon = "bi-alarm",
                Color = "warning",
                Title = $"Late Arrivals: {lateDays}",
                Message = $"You arrived after 10 AM on {lateDays} days this month. Try to arrive earlier.",
                Type = "LATE_ALERT"
            });
        }

        // 6. Overtime prediction
        if (workHours.Count > 5 && workHours.Average() > 9)
        {
            insights.Add(new
            {
                Icon = "bi-clock-history",
                Color = "purple",
                Title = "Overtime Trend",
                Message = "Your avg working hours exceed 9h. Take breaks and maintain work-life balance.",
                Type = "OT_PREDICTION"
            });
        }

        return Ok(new { insights });
    }

    // ════════════════════════════════════════════
    //  ATTENDANCE TRACKER  —  MGT / ADM / SysAdmin
    // ════════════════════════════════════════════

    /// <summary>
    /// Returns today's not-punched and on-leave employee lists for management view.
    /// </summary>
    [HttpGet("attendance-tracker")]
    public async Task<IActionResult> GetAttendanceTracker()
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized();

        // Only MGT, ADM, or system admin
        var deptCode = await _db.MstDepartments
            .AsNoTracking()
            .Where(d => d.DeptId == user.DepartmentId)
            .Select(d => d.DeptCode)
            .FirstOrDefaultAsync();

        if (deptCode != "MGT" && deptCode != "ADM" && !user.IsSystemAdmin)
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "Access denied." });

        var today = DateOnly.FromDateTime(DateTime.Today);

        // All active employees
        var allEmployees = await _db.MstEmployees
            .AsNoTracking()
            .Where(e => e.IsActive == true)
            .Select(e => new
            {
                e.EmployeeId,
                e.EmpCode,
                Name = (e.FirstName ?? "") + " " + (e.LastName ?? ""),
                DeptName = e.Dept != null ? e.Dept.DeptName : "—",
                DeptCode = e.Dept != null ? e.Dept.DeptCode : "",
                Designation = e.Designation != null ? e.Designation.DesignationName : "—"
            })
            .ToListAsync();

        // Employees who punched in today
        var punchedInIds = await _db.HybEmployeeAttendances
            .AsNoTracking()
            .Where(a => a.AttendanceDate == today && a.CheckIn != null)
            .Select(a => a.EmployeeId)
            .ToListAsync();

        // Employees on approved leave today
        var onLeaveEmpIds = await _db.HrLeaveRequests
            .AsNoTracking()
            .Where(lr => lr.Status == "APPROVED" && lr.FromDate <= today && lr.ToDate >= today)
            .Select(lr => lr.EmployeeId)
            .Distinct()
            .ToListAsync();

        var punchedSet = new HashSet<long>(punchedInIds);
        var leaveSet = new HashSet<long>(onLeaveEmpIds);

        // Not punched = active employees who haven't punched in and are not on leave
        var notPunched = allEmployees
            .Where(e => !punchedSet.Contains(e.EmployeeId) && !leaveSet.Contains(e.EmployeeId))
            .Select(e => new { e.EmpCode, e.Name, e.DeptName, e.DeptCode, e.Designation })
            .OrderBy(e => e.DeptName).ThenBy(e => e.Name)
            .ToList();

        // On leave
        var onLeave = allEmployees
            .Where(e => leaveSet.Contains(e.EmployeeId))
            .Select(e => new { e.EmpCode, e.Name, e.DeptName, e.DeptCode, e.Designation })
            .OrderBy(e => e.DeptName).ThenBy(e => e.Name)
            .ToList();

        // Punched in employees with times
        var punchedIn = await _db.HybEmployeeAttendances
            .AsNoTracking()
            .Where(a => a.AttendanceDate == today && a.CheckIn != null)
            .Join(_db.MstEmployees, a => a.EmployeeId, e => e.EmployeeId,
                (a, e) => new
                {
                    e.EmpCode,
                    Name = (e.FirstName ?? "") + " " + (e.LastName ?? ""),
                    CheckIn = a.CheckIn!.Value.ToString("hh:mm tt"),
                    CheckOut = a.CheckOut.HasValue ? a.CheckOut.Value.ToString("hh:mm tt") : (string?)null,
                    a.Status
                })
            .OrderBy(x => x.Name)
            .ToListAsync();

        return Ok(new
        {
            TotalEmployees = allEmployees.Count,
            PresentCount = punchedInIds.Count,
            NotPunchedCount = notPunched.Count,
            OnLeaveCount = onLeave.Count,
            NotPunched = notPunched,
            OnLeave = onLeave,
            PunchedIn = punchedIn
        });
    }

    private async Task<long> ResolveEmployeeId(string? empCode)
    {
        if (string.IsNullOrEmpty(empCode)) return 0;
        return await _db.MstEmployees
            .AsNoTracking()
            .Where(e => e.EmpCode == empCode && e.IsActive == true)
            .Select(e => e.EmployeeId)
            .FirstOrDefaultAsync();
    }

    private static string ResolveDashboardType(string deptCode) => deptCode switch
    {
        "MGT" => "executive",
        "ADM" or "IT" => "admin",
        "HR" => "hr",
        "FIN" => "finance",
        "SAL" or "CST" or "EST" => "sales",
        "PRE" or "PRT" or "FINP" or "PKG" or "DSP" => "production",
        "INV" or "PUR" => "inventory",
        "QMS" => "quality",
        "MNT" => "maintenance",
        "SEC" => "security",
        _ => "executive"
    };
}
