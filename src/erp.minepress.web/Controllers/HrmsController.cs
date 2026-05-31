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
public class HrmsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IUserActivityService _activityService;
    private readonly INotificationService _notifier;
    private readonly ILogger<HrmsController> _logger;
    private readonly ISystemErrorLogger _systemErrorLogger;

    public HrmsController(ApplicationDbContext db, IUserActivityService activityService, INotificationService notifier, ILogger<HrmsController> logger, ISystemErrorLogger systemErrorLogger)
    {
        _db = db;
        _activityService = activityService;
        _notifier = notifier;
        _logger = logger;
        _systemErrorLogger = systemErrorLogger;
    }

    private async Task AuditExceptionAsync(Exception ex, string additionalData, string severity = "Error")
        => await _systemErrorLogger.LogAsync(ex, HttpContext, severity, additionalData);

    private UserSessionData? CurrentUser => HttpContext.Session.GetCurrentUser();
    private bool IsMgtOrAdmin(string? deptCode) => deptCode == "MGT" || deptCode == "ADM";

    // ════════════════════════════════════════════
    //  LEAVES
    // ════════════════════════════════════════════

    [HttpGet("leaves")]
    public async Task<IActionResult> GetLeaves([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var user = CurrentUser;
        if (user == null) return Unauthorized();

        var empId = await GetEmployeeId(user);
        var query = _db.HrLeaveRequests.AsNoTracking().Where(l => l.EmployeeId == empId);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(l => l.Status == status);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(l => l.FromDate)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Join(_db.HrLeaveTypes, lr => lr.LeaveTypeId, lt => lt.LeaveTypeId,
                (lr, lt) => new
                {
                    lr.LeaveId,
                    lr.LeaveNo,
                    LeaveType = lt.LeaveName,
                    FromDate = lr.FromDate.ToString("dd-MMM-yyyy"),
                    ToDate = lr.ToDate.ToString("dd-MMM-yyyy"),
                    lr.TotalDays,
                    lr.HalfDay,
                    lr.Status,
                    lr.Reason,
                    lr.ApprovedBy
                })
            .ToListAsync();

        return Ok(new { items, total, page, pageSize });
    }

    [HttpGet("leaves/balances")]
    public async Task<IActionResult> GetLeaveBalances()
    {
        var user = CurrentUser;
        if (user == null) return Unauthorized();

        var empId = await GetEmployeeId(user);
        var balances = await _db.HrLeaveBalances.AsNoTracking()
            .Where(lb => lb.EmployeeId == empId)
            .Join(_db.HrLeaveTypes, lb => lb.LeaveTypeId, lt => lt.LeaveTypeId,
                (lb, lt) => new
                {
                    lb.BalanceId,
                    LeaveType = lt.LeaveName,
                    lb.FinYear,
                    lb.OpeningBalance,
                    lb.Accrued,
                    lb.Availed,
                    lb.ClosingBalance,
                    Balance = lb.ClosingBalance ?? 0,
                    Entitled = (lb.OpeningBalance ?? 0) + (lb.Accrued ?? 0),
                    Used = lb.Availed ?? 0
                })
            .ToListAsync();

        return Ok(balances);
    }

    [HttpPost("leaves/apply")]
    public async Task<IActionResult> ApplyLeave([FromBody] LeaveApplyRequest req)
    {
        var user = CurrentUser;
        if (user == null) return Unauthorized();

        var empId = await GetEmployeeId(user);
        if (empId == 0) return BadRequest(new { message = "Employee not found" });

        // ── Auto-create leave balance if not found ──
        var now = DateTime.Now;
        var fyStart = now.Month >= 4 ? now.Year : now.Year - 1;
        var finYear = $"{fyStart}-{(fyStart + 1) % 100:D2}";

        var existingBalance = await _db.HrLeaveBalances
            .FirstOrDefaultAsync(lb => lb.EmployeeId == empId
                && lb.LeaveTypeId == req.LeaveTypeId
                && lb.FinYear == finYear);

        if (existingBalance == null)
        {
            var leaveType = await _db.HrLeaveTypes
                .FirstOrDefaultAsync(lt => lt.LeaveTypeId == req.LeaveTypeId && lt.IsActive);

            if (leaveType == null)
                return BadRequest(new { message = "Invalid leave type." });

            var openingBalance = leaveType.MaxDaysPerYear ?? 0;

            _db.HrLeaveBalances.Add(new persistence.Models.HrLeaveBalance
            {
                EmployeeId = empId,
                LeaveTypeId = req.LeaveTypeId,
                FinYear = finYear,
                OpeningBalance = openingBalance,
                Accrued = 0,
                Availed = 0,
                Encashed = 0,
                Lapsed = 0,
                CarryForward = 0
            });
            await _db.SaveChangesAsync();
        }

        var leave = new persistence.Models.HrLeaveRequest
        {
            EmployeeId = empId,
            LeaveNo = await GenerateHrmsNo("LV"),
            LeaveTypeId = req.LeaveTypeId,
            FromDate = DateOnly.Parse(req.FromDate),
            ToDate = DateOnly.Parse(req.ToDate),
            TotalDays = req.TotalDays,
            HalfDay = req.HalfDay,
            Reason = req.Reason,
            Status = "PENDING"
        };

        _db.HrLeaveRequests.Add(leave);
        await _db.SaveChangesAsync();

        await LogActivity(user, "HRMS", "LEAVE_APPLY", $"Leave applied: {req.FromDate} to {req.ToDate}",
            "HrLeaveRequest", leave.LeaveId);

        await _activityService.LogNotificationAsync(new UserNotificationEntry
        {
            UserId = user.UserId,
            Title = "Leave Applied",
            Message = $"Your leave request from {req.FromDate} to {req.ToDate} has been submitted.",
            Icon = "bi-calendar-x",
            Color = "info",
            Module = "HRMS",
            EventType = "LEAVE_APPLY",
            Priority = "NORMAL"
        });

        if (!string.IsNullOrEmpty(user.EmailId))
        {
            _ = _notifier.SendEmailAsync(
                user.EmailId,
                "HRMS: Leave Application Submitted",
                $"<h3>Leave Application Confirmation</h3>"
                + $"<p>Hi <b>{user.Name}</b>,</p>"
                + $"<p>Your leave request from <b>{req.FromDate}</b> to <b>{req.ToDate}</b> ({req.TotalDays} day(s)) has been submitted for approval.</p>"
                + $"<p>Reason: {req.Reason ?? "N/A"}</p>"
                + $"<hr/><small style='color:#888;'>MinePress ERP — Automated HRMS Notification</small>");
        }

        _ = NotifyDepartmentAsync("HR",
            "HRMS Alert: Leave Application",
            $"<h3>Leave Application — HR Notification</h3>"
            + $"<p>Employee <b>{user.Name}</b> ({user.EmployeeCode}) has applied for leave.</p>"
            + $"<p>Period: <b>{req.FromDate}</b> to <b>{req.ToDate}</b> ({req.TotalDays} day(s))</p>"
            + $"<p>Reason: {req.Reason ?? "N/A"}</p>"
            + $"<hr/><small style='color:#888;'>MinePress ERP — Automated HRMS Notification (HR Dept)</small>");

        _ = NotifyDepartmentAsync("IT",
            "ERP Alert: Leave Application",
            $"<h3>Leave Application — IT &amp; ERP Notification</h3>"
            + $"<p>Employee <b>{user.Name}</b> ({user.EmployeeCode}) has applied for leave.</p>"
            + $"<p>Period: <b>{req.FromDate}</b> to <b>{req.ToDate}</b> ({req.TotalDays} day(s))</p>"
            + $"<hr/><small style='color:#888;'>MinePress ERP — Automated Notification (IT &amp; ERP Support)</small>");

        return Ok(new { message = "Leave applied successfully", leaveId = leave.LeaveId });
    }

    // ════════════════════════════════════════════
    //  HOLIDAYS
    // ════════════════════════════════════════════

    [HttpGet("holidays")]
    public async Task<IActionResult> GetHolidays([FromQuery] string? year)
    {
        var user = CurrentUser;
        if (user == null) return Unauthorized();

        var query = _db.HrHolidays.AsNoTracking().Where(h => h.IsActive);
        if (!string.IsNullOrEmpty(year))
            query = query.Where(h => h.FinYear == year);

        var items = await query
            .OrderBy(h => h.HolidayDate)
            .Select(h => new
            {
                h.HolidayId,
                h.HolidayName,
                HolidayDate = h.HolidayDate.ToString("dd-MMM-yyyy"),
                h.HolidayType,
                h.FinYear,
                h.IsOptional
            })
            .ToListAsync();

        return Ok(items);
    }

    // ════════════════════════════════════════════
    //  LOANS
    // ════════════════════════════════════════════

    [HttpGet("loans")]
    public async Task<IActionResult> GetLoans()
    {
        var user = CurrentUser;
        if (user == null) return Unauthorized();

        var empId = await GetEmployeeId(user);
        var items = await _db.HrLoans.AsNoTracking()
            .Where(l => l.EmployeeId == empId)
            .OrderByDescending(l => l.LoanDate)
            .Select(l => new
            {
                l.LoanId,
                l.LoanNo,
                l.LoanType,
                LoanDate = l.LoanDate.ToString("dd-MMM-yyyy"),
                l.LoanAmount,
                l.InterestRate,
                l.TenureMonths,
                l.EmiAmount,
                l.OutstandingAmount,
                l.Status
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost("loans/apply")]
    public async Task<IActionResult> ApplyLoan([FromBody] LoanApplyRequest req)
    {
        var user = CurrentUser;
        if (user == null) return Unauthorized();

        var empId = await GetEmployeeId(user);
        if (empId == 0) return BadRequest(new { message = "Employee not found" });

        var loan = new persistence.Models.HrLoan
        {
            EmployeeId = empId,
            LoanNo = await GenerateHrmsNo("LN"),
            LoanType = req.LoanType,
            LoanDate = DateOnly.Parse(req.LoanDate),
            LoanAmount = req.LoanAmount,
            InterestRate = req.InterestRate,
            TenureMonths = req.TenureMonths,
            EmiAmount = req.EmiAmount,
            OutstandingAmount = req.LoanAmount,
            Status = "PENDING"
        };

        _db.HrLoans.Add(loan);
        await _db.SaveChangesAsync();

        await LogActivity(user, "HRMS", "LOAN_APPLY", $"Loan applied: {req.LoanType} ₹{req.LoanAmount:N0}",
            "HrLoan", loan.LoanId);

        await _activityService.LogNotificationAsync(new UserNotificationEntry
        {
            UserId = user.UserId,
            Title = "Loan Application Submitted",
            Message = $"Your {req.LoanType} loan of ₹{req.LoanAmount:N0} has been submitted for approval.",
            Icon = "bi-bank",
            Color = "info",
            Module = "HRMS",
            EventType = "LOAN_APPLY",
            Priority = "NORMAL"
        });

        if (!string.IsNullOrEmpty(user.EmailId))
        {
            _ = _notifier.SendEmailAsync(
                user.EmailId,
                "HRMS: Loan Application Submitted",
                $"<h3>Loan Application Confirmation</h3>"
                + $"<p>Hi <b>{user.Name}</b>,</p>"
                + $"<p>Your <b>{req.LoanType}</b> loan of <b>₹{req.LoanAmount:N0}</b> has been submitted for approval.</p>"
                + $"<p>Tenure: {req.TenureMonths} months | EMI: ₹{req.EmiAmount:N0}</p>"
                + $"<hr/><small style='color:#888;'>MinePress ERP — Automated HRMS Notification</small>");
        }

        _ = NotifyDepartmentAsync("HR",
            "HRMS Alert: Loan Application",
            $"<h3>Loan Application — HR Notification</h3>"
            + $"<p>Employee <b>{user.Name}</b> ({user.EmployeeCode}) has applied for a <b>{req.LoanType}</b> loan.</p>"
            + $"<p>Amount: <b>₹{req.LoanAmount:N0}</b> | Tenure: {req.TenureMonths} months | EMI: ₹{req.EmiAmount:N0}</p>"
            + $"<hr/><small style='color:#888;'>MinePress ERP — Automated HRMS Notification (HR Dept)</small>");

        _ = NotifyDepartmentAsync("FIN",
            "Finance Alert: Loan Application",
            $"<h3>Loan Application — Finance Notification</h3>"
            + $"<p>Employee <b>{user.Name}</b> ({user.EmployeeCode}) has applied for a <b>{req.LoanType}</b> loan.</p>"
            + $"<p>Amount: <b>₹{req.LoanAmount:N0}</b> | Tenure: {req.TenureMonths} months | EMI: ₹{req.EmiAmount:N0}</p>"
            + $"<hr/><small style='color:#888;'>MinePress ERP — Automated HRMS Notification (Accounts &amp; Finance)</small>");

        _ = NotifyDepartmentAsync("IT",
            "ERP Alert: Loan Application",
            $"<h3>Loan Application — IT &amp; ERP Notification</h3>"
            + $"<p>Employee <b>{user.Name}</b> ({user.EmployeeCode}) has applied for a <b>{req.LoanType}</b> loan.</p>"
            + $"<p>Amount: <b>₹{req.LoanAmount:N0}</b> | Tenure: {req.TenureMonths} months | EMI: ₹{req.EmiAmount:N0}</p>"
            + $"<hr/><small style='color:#888;'>MinePress ERP — Automated Notification (IT &amp; ERP Support)</small>");

        return Ok(new { message = "Loan application submitted", loanId = loan.LoanId });
    }

    // ════════════════════════════════════════════
    //  SALARY ADVANCE
    // ════════════════════════════════════════════

    [HttpGet("advances")]
    public async Task<IActionResult> GetAdvances()
    {
        var user = CurrentUser;
        if (user == null) return Unauthorized();

        var empId = await GetEmployeeId(user);
        var items = await _db.HrSalaryAdvances.AsNoTracking()
            .Where(a => a.EmployeeId == empId)
            .OrderByDescending(a => a.AdvanceDate)
            .Select(a => new
            {
                a.AdvanceId,
                a.AdvanceNo,
                AdvanceDate = a.AdvanceDate.ToString("dd-MMM-yyyy"),
                a.AdvanceAmount,
                a.RepaymentMonths,
                a.MonthlyDeduction,
                a.BalanceAmount,
                a.Status
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost("advances/apply")]
    public async Task<IActionResult> ApplyAdvance([FromBody] AdvanceApplyRequest req)
    {
        var user = CurrentUser;
        if (user == null) return Unauthorized();

        var empId = await GetEmployeeId(user);
        if (empId == 0) return BadRequest(new { message = "Employee not found" });

        var advance = new persistence.Models.HrSalaryAdvance
        {
            EmployeeId = empId,
            AdvanceNo = await GenerateHrmsNo("ADV"),
            AdvanceDate = DateOnly.Parse(req.AdvanceDate),
            AdvanceAmount = req.AdvanceAmount,
            RepaymentMonths = req.RepaymentMonths,
            MonthlyDeduction = req.RepaymentMonths > 0 ? req.AdvanceAmount / req.RepaymentMonths : req.AdvanceAmount,
            BalanceAmount = req.AdvanceAmount,
            Status = "PENDING"
        };

        _db.HrSalaryAdvances.Add(advance);
        await _db.SaveChangesAsync();

        await LogActivity(user, "HRMS", "ADVANCE_APPLY", $"Salary advance applied: ₹{req.AdvanceAmount:N0}",
            "HrSalaryAdvance", advance.AdvanceId);

        await _activityService.LogNotificationAsync(new UserNotificationEntry
        {
            UserId = user.UserId,
            Title = "Salary Advance Submitted",
            Message = $"Your salary advance of ₹{req.AdvanceAmount:N0} has been submitted.",
            Icon = "bi-cash-stack",
            Color = "info",
            Module = "HRMS",
            EventType = "ADVANCE_APPLY",
            Priority = "NORMAL"
        });

        if (!string.IsNullOrEmpty(user.EmailId))
        {
            _ = _notifier.SendEmailAsync(
                user.EmailId,
                "HRMS: Salary Advance Submitted",
                $"<h3>Salary Advance Confirmation</h3>"
                + $"<p>Hi <b>{user.Name}</b>,</p>"
                + $"<p>Your salary advance of <b>₹{req.AdvanceAmount:N0}</b> has been submitted for approval.</p>"
                + $"<p>Repayment: {req.RepaymentMonths} months</p>"
                + $"<hr/><small style='color:#888;'>MinePress ERP — Automated HRMS Notification</small>");
        }

        _ = NotifyDepartmentAsync("HR",
            "HRMS Alert: Salary Advance",
            $"<h3>Salary Advance — HR Notification</h3>"
            + $"<p>Employee <b>{user.Name}</b> ({user.EmployeeCode}) has applied for a salary advance.</p>"
            + $"<p>Amount: <b>₹{req.AdvanceAmount:N0}</b> | Repayment: {req.RepaymentMonths} months</p>"
            + $"<hr/><small style='color:#888;'>MinePress ERP — Automated HRMS Notification (HR Dept)</small>");

        _ = NotifyDepartmentAsync("FIN",
            "Finance Alert: Salary Advance",
            $"<h3>Salary Advance — Finance Notification</h3>"
            + $"<p>Employee <b>{user.Name}</b> ({user.EmployeeCode}) has applied for a salary advance.</p>"
            + $"<p>Amount: <b>₹{req.AdvanceAmount:N0}</b> | Repayment: {req.RepaymentMonths} months</p>"
            + $"<hr/><small style='color:#888;'>MinePress ERP — Automated HRMS Notification (Accounts &amp; Finance)</small>");

        _ = NotifyDepartmentAsync("IT",
            "ERP Alert: Salary Advance",
            $"<h3>Salary Advance — IT &amp; ERP Notification</h3>"
            + $"<p>Employee <b>{user.Name}</b> ({user.EmployeeCode}) has applied for a salary advance.</p>"
            + $"<p>Amount: <b>₹{req.AdvanceAmount:N0}</b> | Repayment: {req.RepaymentMonths} months</p>"
            + $"<hr/><small style='color:#888;'>MinePress ERP — Automated Notification (IT &amp; ERP Support)</small>");

        return Ok(new { message = "Advance application submitted", advanceId = advance.AdvanceId });
    }

    // ════════════════════════════════════════════
    //  MEDICAL CLAIMS
    // ════════════════════════════════════════════

    [HttpGet("medical")]
    public async Task<IActionResult> GetMedicalClaims()
    {
        var user = CurrentUser;
        if (user == null) return Unauthorized();

        var empId = await GetEmployeeId(user);
        var items = await _db.HrMedicalClaims.AsNoTracking()
            .Where(m => m.EmployeeId == empId)
            .OrderByDescending(m => m.ClaimDate)
            .Select(m => new
            {
                m.MedicalClaimId,
                m.ClaimNo,
                ClaimDate = m.ClaimDate.ToString("dd-MMM-yyyy"),
                m.PatientName,
                m.Relation,
                m.HospitalName,
                m.ClaimAmount,
                m.ApprovedAmount,
                m.Status
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost("medical/submit")]
    public async Task<IActionResult> SubmitMedicalClaim([FromBody] MedicalClaimRequest req)
    {
        var user = CurrentUser;
        if (user == null) return Unauthorized();

        var empId = await GetEmployeeId(user);
        if (empId == 0) return BadRequest(new { message = "Employee not found" });

        var claim = new persistence.Models.HrMedicalClaim
        {
            EmployeeId = empId,
            ClaimNo = await GenerateHrmsNo("MC"),
            ClaimDate = DateOnly.Parse(req.ClaimDate),
            PatientName = req.PatientName,
            Relation = req.Relation,
            HospitalName = req.HospitalName,
            ClaimAmount = req.ClaimAmount,
            Status = "PENDING"
        };

        _db.HrMedicalClaims.Add(claim);
        await _db.SaveChangesAsync();

        await LogActivity(user, "HRMS", "MEDICAL_CLAIM", $"Medical claim: ₹{req.ClaimAmount:N0} for {req.PatientName}",
            "HrMedicalClaim", claim.MedicalClaimId);

        await _activityService.LogNotificationAsync(new UserNotificationEntry
        {
            UserId = user.UserId,
            Title = "Medical Claim Submitted",
            Message = $"Medical claim of ₹{req.ClaimAmount:N0} for {req.PatientName} submitted.",
            Icon = "bi-heart-pulse",
            Color = "info",
            Module = "HRMS",
            EventType = "MEDICAL_CLAIM",
            Priority = "NORMAL"
        });

        if (!string.IsNullOrEmpty(user.EmailId))
        {
            _ = _notifier.SendEmailAsync(
                user.EmailId,
                "HRMS: Medical Claim Submitted",
                $"<h3>Medical Claim Confirmation</h3>"
                + $"<p>Hi <b>{user.Name}</b>,</p>"
                + $"<p>Your medical claim of <b>₹{req.ClaimAmount:N0}</b> for <b>{req.PatientName}</b> ({req.Relation ?? "Self"}) has been submitted.</p>"
                + $"<p>Hospital: {req.HospitalName ?? "N/A"}</p>"
                + $"<hr/><small style='color:#888;'>MinePress ERP — Automated HRMS Notification</small>");
        }

        _ = NotifyDepartmentAsync("HR",
            "HRMS Alert: Medical Claim",
            $"<h3>Medical Claim — HR Notification</h3>"
            + $"<p>Employee <b>{user.Name}</b> ({user.EmployeeCode}) has submitted a medical claim.</p>"
            + $"<p>Amount: <b>₹{req.ClaimAmount:N0}</b> | Patient: {req.PatientName} ({req.Relation ?? "Self"})</p>"
            + $"<p>Hospital: {req.HospitalName ?? "N/A"}</p>"
            + $"<hr/><small style='color:#888;'>MinePress ERP — Automated HRMS Notification (HR Dept)</small>");

        _ = NotifyDepartmentAsync("FIN",
            "Finance Alert: Medical Claim",
            $"<h3>Medical Claim — Finance Notification</h3>"
            + $"<p>Employee <b>{user.Name}</b> ({user.EmployeeCode}) has submitted a medical claim.</p>"
            + $"<p>Amount: <b>₹{req.ClaimAmount:N0}</b> | Patient: {req.PatientName} ({req.Relation ?? "Self"})</p>"
            + $"<p>Hospital: {req.HospitalName ?? "N/A"}</p>"
            + $"<hr/><small style='color:#888;'>MinePress ERP — Automated HRMS Notification (Accounts &amp; Finance)</small>");

        _ = NotifyDepartmentAsync("IT",
            "ERP Alert: Medical Claim",
            $"<h3>Medical Claim — IT &amp; ERP Notification</h3>"
            + $"<p>Employee <b>{user.Name}</b> ({user.EmployeeCode}) has submitted a medical claim.</p>"
            + $"<p>Amount: <b>₹{req.ClaimAmount:N0}</b> | Patient: {req.PatientName} ({req.Relation ?? "Self"})</p>"
            + $"<hr/><small style='color:#888;'>MinePress ERP — Automated Notification (IT &amp; ERP Support)</small>");

        return Ok(new { message = "Medical claim submitted", claimId = claim.MedicalClaimId });
    }

    // ════════════════════════════════════════════
    //  OVERTIME
    // ════════════════════════════════════════════

    [HttpGet("overtime")]
    public async Task<IActionResult> GetOvertime()
    {
        var user = CurrentUser;
        if (user == null) return Unauthorized();

        var empId = await GetEmployeeId(user);
        var items = await _db.HrOvertimes.AsNoTracking()
            .Where(o => o.EmployeeId == empId)
            .OrderByDescending(o => o.OtDate)
            .Select(o => new
            {
                o.OtId,
                o.OtNo,
                OtDate = o.OtDate.ToString("dd-MMM-yyyy"),
                FromTime = o.FromTime.HasValue ? o.FromTime.Value.ToString(@"hh\:mm") : "",
                ToTime = o.ToTime.HasValue ? o.ToTime.Value.ToString(@"hh\:mm") : "",
                o.OtHours,
                o.OtRatePerHour,
                o.OtAmount,
                o.Status
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost("overtime/submit")]
    public async Task<IActionResult> SubmitOvertime([FromBody] OvertimeRequest req)
    {
        var user = CurrentUser;
        if (user == null) return Unauthorized();

        var empId = await GetEmployeeId(user);
        if (empId == 0) return BadRequest(new { message = "Employee not found" });

        var ot = new persistence.Models.HrOvertime
        {
            EmployeeId = empId,
            OtNo = await GenerateHrmsNo("OT"),
            OtDate = DateOnly.Parse(req.OtDate),
            FromTime = TimeOnly.Parse(req.FromTime),
            ToTime = TimeOnly.Parse(req.ToTime),
            OtHours = req.OtHours,
            OtRatePerHour = req.OtRatePerHour,
            OtAmount = req.OtHours * req.OtRatePerHour,
            Status = "PENDING"
        };

        _db.HrOvertimes.Add(ot);
        await _db.SaveChangesAsync();

        await LogActivity(user, "HRMS", "OT_SUBMIT", $"Overtime submitted: {req.OtHours}hrs on {req.OtDate}",
            "HrOvertime", ot.OtId);

        await _activityService.LogNotificationAsync(new UserNotificationEntry
        {
            UserId = user.UserId,
            Title = "Overtime Submitted",
            Message = $"Overtime of {req.OtHours} hrs on {req.OtDate} submitted for approval.",
            Icon = "bi-clock-history",
            Color = "info",
            Module = "HRMS",
            EventType = "OT_SUBMIT",
            Priority = "NORMAL"
        });

        if (!string.IsNullOrEmpty(user.EmailId))
        {
            _ = _notifier.SendEmailAsync(
                user.EmailId,
                "HRMS: Overtime Submitted",
                $"<h3>Overtime Submission Confirmation</h3>"
                + $"<p>Hi <b>{user.Name}</b>,</p>"
                + $"<p>Your overtime of <b>{req.OtHours} hrs</b> on <b>{req.OtDate}</b> has been submitted for approval.</p>"
                + $"<p>Rate: ₹{req.OtRatePerHour:N0}/hr | Amount: ₹{req.OtHours * req.OtRatePerHour:N0}</p>"
                + $"<hr/><small style='color:#888;'>MinePress ERP — Automated HRMS Notification</small>");
        }

        _ = NotifyDepartmentAsync("HR",
            "HRMS Alert: Overtime Submission",
            $"<h3>Overtime Submission — HR Notification</h3>"
            + $"<p>Employee <b>{user.Name}</b> ({user.EmployeeCode}) has submitted overtime.</p>"
            + $"<p>Date: <b>{req.OtDate}</b> | Hours: <b>{req.OtHours} hrs</b> | Amount: ₹{req.OtHours * req.OtRatePerHour:N0}</p>"
            + $"<hr/><small style='color:#888;'>MinePress ERP — Automated HRMS Notification (HR Dept)</small>");

        _ = NotifyDepartmentAsync("FIN",
            "Finance Alert: Overtime Submission",
            $"<h3>Overtime Submission — Finance Notification</h3>"
            + $"<p>Employee <b>{user.Name}</b> ({user.EmployeeCode}) has submitted overtime.</p>"
            + $"<p>Date: <b>{req.OtDate}</b> | Hours: <b>{req.OtHours} hrs</b> | Amount: ₹{req.OtHours * req.OtRatePerHour:N0}</p>"
            + $"<hr/><small style='color:#888;'>MinePress ERP — Automated HRMS Notification (Accounts &amp; Finance)</small>");

        _ = NotifyDepartmentAsync("IT",
            "ERP Alert: Overtime Submission",
            $"<h3>Overtime Submission — IT &amp; ERP Notification</h3>"
            + $"<p>Employee <b>{user.Name}</b> ({user.EmployeeCode}) has submitted overtime.</p>"
            + $"<p>Date: <b>{req.OtDate}</b> | Hours: <b>{req.OtHours} hrs</b> | Amount: ₹{req.OtHours * req.OtRatePerHour:N0}</p>"
            + $"<hr/><small style='color:#888;'>MinePress ERP — Automated Notification (IT &amp; ERP Support)</small>");

        return Ok(new { message = "Overtime submitted", otId = ot.OtId });
    }

    // ════════════════════════════════════════════
    //  RESIGNATION
    // ════════════════════════════════════════════

    [HttpGet("resignations")]
    public async Task<IActionResult> GetResignations()
    {
        var user = CurrentUser;
        if (user == null) return Unauthorized();

        var empId = await GetEmployeeId(user);
        var items = await _db.HrResignations.AsNoTracking()
            .Where(r => r.EmployeeId == empId)
            .OrderByDescending(r => r.ResignationDate)
            .Select(r => new
            {
                r.ResignationId,
                r.ResignationNo,
                ResignationDate = r.ResignationDate.ToString("dd-MMM-yyyy"),
                r.ResignationReason,
                LastWorkingDay = r.LastWorkingDay.HasValue ? r.LastWorkingDay.Value.ToString("dd-MMM-yyyy") : "",
                r.NoticePeriodDays,
                r.Status
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost("resignations/submit")]
    public async Task<IActionResult> SubmitResignation([FromBody] ResignationRequest req)
    {
        var user = CurrentUser;
        if (user == null) return Unauthorized();

        var empId = await GetEmployeeId(user);
        if (empId == 0) return BadRequest(new { message = "Employee not found" });

        var resign = new persistence.Models.HrResignation
        {
            EmployeeId = empId,
            ResignationNo = await GenerateHrmsNo("RSG"),
            ResignationDate = DateOnly.Parse(req.ResignationDate),
            ResignationReason = req.Reason,
            LastWorkingDay = string.IsNullOrEmpty(req.LastWorkingDay) ? null : DateOnly.Parse(req.LastWorkingDay),
            NoticePeriodDays = req.NoticePeriodDays,
            Status = "PENDING"
        };

        _db.HrResignations.Add(resign);
        await _db.SaveChangesAsync();

        await LogActivity(user, "HRMS", "RESIGN_SUBMIT", $"Resignation submitted",
            "HrResignation", resign.ResignationId, "WARNING");

        await _activityService.LogNotificationAsync(new UserNotificationEntry
        {
            UserId = user.UserId,
            Title = "Resignation Submitted",
            Message = "Your resignation has been submitted for review.",
            Icon = "bi-box-arrow-right",
            Color = "warning",
            Module = "HRMS",
            EventType = "RESIGNATION",
            Priority = "HIGH"
        });

        if (!string.IsNullOrEmpty(user.EmailId))
        {
            _ = _notifier.SendEmailAsync(
                user.EmailId,
                "HRMS: Resignation Submitted",
                $"<h3>Resignation Acknowledgement</h3>"
                + $"<p>Hi <b>{user.Name}</b>,</p>"
                + $"<p>Your resignation submitted on <b>{req.ResignationDate}</b> has been received and is under review.</p>"
                + $"<p>Notice Period: {req.NoticePeriodDays} days</p>"
                + (string.IsNullOrEmpty(req.LastWorkingDay) ? "" : $"<p>Last Working Day: <b>{req.LastWorkingDay}</b></p>")
                + $"<hr/><small style='color:#888;'>MinePress ERP — Automated HRMS Notification</small>");
        }

        _ = NotifyDepartmentAsync("HR",
            "HRMS Alert: Resignation Submitted",
            $"<h3>Resignation — HR Notification</h3>"
            + $"<p>Employee <b>{user.Name}</b> ({user.EmployeeCode}) has submitted a resignation.</p>"
            + $"<p>Date: <b>{req.ResignationDate}</b> | Notice Period: {req.NoticePeriodDays} days</p>"
            + (string.IsNullOrEmpty(req.LastWorkingDay) ? "" : $"<p>Last Working Day: <b>{req.LastWorkingDay}</b></p>")
            + $"<hr/><small style='color:#888;'>MinePress ERP — Automated HRMS Notification (HR Dept)</small>");

        _ = NotifyDepartmentAsync("IT",
            "ERP Alert: Resignation Submitted",
            $"<h3>Resignation — IT &amp; ERP Notification</h3>"
            + $"<p>Employee <b>{user.Name}</b> ({user.EmployeeCode}) has submitted a resignation.</p>"
            + $"<p>Date: <b>{req.ResignationDate}</b> | Notice Period: {req.NoticePeriodDays} days</p>"
            + $"<hr/><small style='color:#888;'>MinePress ERP — Automated Notification (IT &amp; ERP Support)</small>");

        return Ok(new { message = "Resignation submitted", resignationId = resign.ResignationId });
    }

    // ════════════════════════════════════════════
    //  SHIFT ROSTER
    // ════════════════════════════════════════════

    [HttpGet("shifts")]
    public async Task<IActionResult> GetShifts()
    {
        var user = CurrentUser;
        if (user == null) return Unauthorized();

        var empId = await GetEmployeeId(user);
        var items = await _db.HrShiftRosters.AsNoTracking()
            .Where(sr => sr.EmployeeId == empId)
            .OrderByDescending(sr => sr.EffectiveFrom)
            .Join(_db.MstShiftTypes, sr => sr.ShiftTypeId, st => st.ShiftTypeId,
                (sr, st) => new
                {
                    sr.RosterId,
                    st.ShiftName,
                    st.ShiftCode,
                    ShiftStart = st.ShiftStartTime,
                    ShiftEnd = st.ShiftEndTime,
                    EffectiveFrom = sr.EffectiveFrom.ToString("dd-MMM-yyyy"),
                    EffectiveTo = sr.EffectiveTo.HasValue ? sr.EffectiveTo.Value.ToString("dd-MMM-yyyy") : "Ongoing",
                    sr.WeekOffDays,
                    sr.IsActive
                })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("shift-types")]
    public async Task<IActionResult> GetShiftTypes()
    {
        var user = CurrentUser;
        if (user == null) return Unauthorized();

        var items = await _db.MstShiftTypes.AsNoTracking()
            .Select(st => new { st.ShiftTypeId, st.ShiftCode, st.ShiftName, st.ShiftStartTime, st.ShiftEndTime })
            .ToListAsync();

        return Ok(items);
    }

    // ════════════════════════════════════════════
    //  TRANSFER
    // ════════════════════════════════════════════

    [HttpGet("transfers")]
    public async Task<IActionResult> GetTransfers()
    {
        var user = CurrentUser;
        if (user == null) return Unauthorized();

        var empId = await GetEmployeeId(user);
        var items = await _db.HrTransfers.AsNoTracking()
            .Where(t => t.EmployeeId == empId)
            .OrderByDescending(t => t.TransferDate)
            .Select(t => new
            {
                t.TransferId,
                t.TransferNo,
                TransferDate = t.TransferDate.ToString("dd-MMM-yyyy"),
                t.FromDeptId,
                t.ToDeptId,
                t.TransferReason,
                EffectiveDate = t.EffectiveDate.ToString("dd-MMM-yyyy"),
                t.Status
            })
            .ToListAsync();

        return Ok(items);
    }

    // ════════════════════════════════════════════
    //  TRAVEL EXPENSES
    // ════════════════════════════════════════════

    [HttpGet("travel")]
    public async Task<IActionResult> GetTravelExpenses()
    {
        var user = CurrentUser;
        if (user == null) return Unauthorized();

        var empId = await GetEmployeeId(user);
        var items = await _db.HrTravelExpenses.AsNoTracking()
            .Where(t => t.EmployeeId == empId)
            .OrderByDescending(t => t.TravelDate)
            .Select(t => new
            {
                t.TravelId,
                t.TravelNo,
                t.Purpose,
                t.FromLocation,
                t.ToLocation,
                TravelDate = t.TravelDate.ToString("dd-MMM-yyyy"),
                ReturnDate = t.ReturnDate.HasValue ? t.ReturnDate.Value.ToString("dd-MMM-yyyy") : "",
                t.ClaimAmount,
                t.ApprovedAmount,
                t.Status
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost("travel/submit")]
    public async Task<IActionResult> SubmitTravelExpense([FromBody] TravelExpenseRequest req)
    {
        var user = CurrentUser;
        if (user == null) return Unauthorized();

        var empId = await GetEmployeeId(user);
        if (empId == 0) return BadRequest(new { message = "Employee not found" });

        var travel = new persistence.Models.HrTravelExpense
        {
            EmployeeId = empId,
            TravelNo = await GenerateHrmsNo("TRV"),
            Purpose = req.Purpose,
            FromLocation = req.FromLocation,
            ToLocation = req.ToLocation,
            TravelDate = DateOnly.Parse(req.TravelDate),
            ReturnDate = string.IsNullOrEmpty(req.ReturnDate) ? null : DateOnly.Parse(req.ReturnDate),
            ClaimAmount = req.ClaimAmount,
            Status = "PENDING"
        };

        _db.HrTravelExpenses.Add(travel);
        await _db.SaveChangesAsync();

        await LogActivity(user, "HRMS", "TRAVEL_SUBMIT", $"Travel expense: {req.FromLocation} → {req.ToLocation} ₹{req.ClaimAmount:N0}",
            "HrTravelExpense", travel.TravelId);

        await _activityService.LogNotificationAsync(new UserNotificationEntry
        {
            UserId = user.UserId,
            Title = "Travel Expense Submitted",
            Message = $"Travel expense of ₹{req.ClaimAmount:N0} ({req.FromLocation} → {req.ToLocation}) submitted.",
            Icon = "bi-airplane",
            Color = "info",
            Module = "HRMS",
            EventType = "TRAVEL_SUBMIT",
            Priority = "NORMAL"
        });

        if (!string.IsNullOrEmpty(user.EmailId))
        {
            _ = _notifier.SendEmailAsync(
                user.EmailId,
                "HRMS: Travel Expense Submitted",
                $"<h3>Travel Expense Confirmation</h3>"
                + $"<p>Hi <b>{user.Name}</b>,</p>"
                + $"<p>Your travel expense from <b>{req.FromLocation}</b> to <b>{req.ToLocation}</b> on <b>{req.TravelDate}</b> has been submitted.</p>"
                + $"<p>Claim Amount: <b>₹{req.ClaimAmount:N0}</b></p>"
                + $"<p>Purpose: {req.Purpose}</p>"
                + $"<hr/><small style='color:#888;'>MinePress ERP — Automated HRMS Notification</small>");
        }

        _ = NotifyDepartmentAsync("HR",
            "HRMS Alert: Travel Expense",
            $"<h3>Travel Expense — HR Notification</h3>"
            + $"<p>Employee <b>{user.Name}</b> ({user.EmployeeCode}) has submitted a travel expense.</p>"
            + $"<p>Route: <b>{req.FromLocation}</b> → <b>{req.ToLocation}</b> | Date: {req.TravelDate}</p>"
            + $"<p>Claim Amount: <b>₹{req.ClaimAmount:N0}</b> | Purpose: {req.Purpose}</p>"
            + $"<hr/><small style='color:#888;'>MinePress ERP — Automated HRMS Notification (HR Dept)</small>");

        _ = NotifyDepartmentAsync("FIN",
            "Finance Alert: Travel Expense",
            $"<h3>Travel Expense — Finance Notification</h3>"
            + $"<p>Employee <b>{user.Name}</b> ({user.EmployeeCode}) has submitted a travel expense.</p>"
            + $"<p>Route: <b>{req.FromLocation}</b> → <b>{req.ToLocation}</b> | Date: {req.TravelDate}</p>"
            + $"<p>Claim Amount: <b>₹{req.ClaimAmount:N0}</b> | Purpose: {req.Purpose}</p>"
            + $"<hr/><small style='color:#888;'>MinePress ERP — Automated HRMS Notification (Accounts &amp; Finance)</small>");

        _ = NotifyDepartmentAsync("IT",
            "ERP Alert: Travel Expense",
            $"<h3>Travel Expense — IT &amp; ERP Notification</h3>"
            + $"<p>Employee <b>{user.Name}</b> ({user.EmployeeCode}) has submitted a travel expense.</p>"
            + $"<p>Route: <b>{req.FromLocation}</b> → <b>{req.ToLocation}</b> | Date: {req.TravelDate}</p>"
            + $"<p>Claim Amount: <b>₹{req.ClaimAmount:N0}</b></p>"
            + $"<hr/><small style='color:#888;'>MinePress ERP — Automated Notification (IT &amp; ERP Support)</small>");

        return Ok(new { message = "Travel expense submitted", travelId = travel.TravelId });
    }

    // ════════════════════════════════════════════
    //  ATTENDANCE
    // ════════════════════════════════════════════

    [HttpGet("attendance")]
    public async Task<IActionResult> GetAttendance([FromQuery] string? month)
    {
        var user = CurrentUser;
        if (user == null) return Unauthorized();

        var empId = await GetEmployeeId(user);
        var now = DateOnly.FromDateTime(DateTime.Today);
        DateOnly start, end;

        if (!string.IsNullOrEmpty(month) && DateOnly.TryParse(month + "-01", out var monthStart))
        {
            start = monthStart;
            end = monthStart.AddMonths(1).AddDays(-1);
        }
        else
        {
            start = new DateOnly(now.Year, now.Month, 1);
            end = new DateOnly(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
        }

        // Cap end at today so future dates don't appear
        var effectiveEnd = end > now ? now : end;

        var records = await _db.HybEmployeeAttendances.AsNoTracking()
            .Where(a => a.EmployeeId == empId && a.AttendanceDate >= start && a.AttendanceDate <= effectiveEnd)
            .ToListAsync();

        // Build full calendar: one row per day from start → effectiveEnd
        var lookup = records.ToDictionary(a => a.AttendanceDate);
        var items = new List<object>();

        for (var date = start; date <= effectiveEnd; date = date.AddDays(1))
        {
            var dayOfWeek = date.DayOfWeek;
            var isSunday = dayOfWeek == DayOfWeek.Sunday;

            if (lookup.TryGetValue(date, out var rec))
            {
                items.Add(new
                {
                    AttendanceDate = date.ToString("dd-MMM-yyyy"),
                    DayName = date.ToString("ddd"),
                    IsSunday = isSunday,
                    CheckIn = rec.CheckIn.HasValue ? rec.CheckIn.Value.ToString(@"hh\:mm tt") : "",
                    CheckOut = rec.CheckOut.HasValue ? rec.CheckOut.Value.ToString(@"hh\:mm tt") : "",
                    TotalHours = rec.TotalHours,
                    OvertimeHours = rec.OvertimeHours,
                    Status = rec.Status
                });
            }
            else
            {
                items.Add(new
                {
                    AttendanceDate = date.ToString("dd-MMM-yyyy"),
                    DayName = date.ToString("ddd"),
                    IsSunday = isSunday,
                    CheckIn = "",
                    CheckOut = "",
                    TotalHours = (decimal?)null,
                    OvertimeHours = (decimal?)null,
                    Status = isSunday ? "SUNDAY" : "NO_RECORD"
                });
            }
        }

        // Return in ascending order (1st → end of month)
        return Ok(new { items, total = items.Count });
    }

    [HttpGet("attendance/summary")]
    public async Task<IActionResult> GetAttendanceSummary([FromQuery] string? month)
    {
        var user = CurrentUser;
        if (user == null) return Unauthorized();

        var empId = await GetEmployeeId(user);
        var now = DateOnly.FromDateTime(DateTime.Today);
        DateOnly start, end;

        if (!string.IsNullOrEmpty(month) && DateOnly.TryParse(month + "-01", out var ms))
        {
            start = ms;
            end = ms.AddMonths(1).AddDays(-1);
        }
        else
        {
            start = new DateOnly(now.Year, now.Month, 1);
            end = new DateOnly(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
        }

        // For current month, cap end to today so future days aren't counted
        var effectiveEnd = end > now ? now : end;

        var records = await _db.HybEmployeeAttendances.AsNoTracking()
            .Where(a => a.EmployeeId == empId && a.AttendanceDate >= start && a.AttendanceDate <= effectiveEnd)
            .ToListAsync();

        // Total calendar days from 1st to effectiveEnd
        var totalDays = (effectiveEnd.ToDateTime(TimeOnly.MinValue) - start.ToDateTime(TimeOnly.MinValue)).Days + 1;

        return Ok(new
        {
            TotalDays = totalDays,
            Present = records.Count(r => r.Status == "PRESENT" || r.Status == "ACTIVE"),
            Absent = records.Count(r => r.Status == "ABSENT"),
            HalfDay = records.Count(r => r.Status == "HALFDAY"),
            Late = records.Count(r => r.Status == "LATE"),
            TotalHours = records.Sum(r => r.TotalHours ?? 0),
            OvertimeHours = records.Sum(r => r.OvertimeHours ?? 0),
            StartDate = start.ToString("dd-MMM-yyyy"),
            EndDate = effectiveEnd.ToString("dd-MMM-yyyy")
        });
    }

    // ════════════════════════════════════════════
    //  INCENTIVES
    // ════════════════════════════════════════════

    [HttpGet("incentives")]
    public async Task<IActionResult> GetIncentives()
    {
        var user = CurrentUser;
        if (user == null) return Unauthorized();

        var empId = await GetEmployeeId(user);
        var items = await _db.HrIncentives.AsNoTracking()
            .Where(i => i.EmployeeId == empId)
            .OrderByDescending(i => i.IncentiveDate)
            .Select(i => new
            {
                i.IncentiveId,
                i.IncentiveNo,
                i.IncentiveType,
                i.ReferencePeriod,
                IncentiveDate = i.IncentiveDate.ToString("dd-MMM-yyyy"),
                i.IncentiveAmount,
                i.Status
            })
            .ToListAsync();

        return Ok(items);
    }

    // ════════════════════════════════════════════
    //  LEAVE TYPES (for dropdowns)
    // ════════════════════════════════════════════

    [HttpGet("leave-types")]
    public async Task<IActionResult> GetLeaveTypes()
    {
        var user = CurrentUser;
        if (user == null) return Unauthorized();

        var items = await _db.HrLeaveTypes.AsNoTracking()
            .Select(lt => new { lt.LeaveTypeId, LeaveTypeName = lt.LeaveName })
            .ToListAsync();

        return Ok(items);
    }

    // ════════════════════════════════════════════
    //  EMPLOYEE LOOKUP
    // ════════════════════════════════════════════

    [HttpGet("employees/search")]
    public async Task<IActionResult> SearchEmployees([FromQuery] string? q)
    {
        var user = CurrentUser;
        if (user == null) return Unauthorized();

        var query = _db.MstEmployees.AsNoTracking()
            .Where(e => e.IsActive == true);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(e =>
                e.EmpCode.ToLower().Contains(term) ||
                (e.FirstName != null && e.FirstName.ToLower().Contains(term)) ||
                (e.LastName != null && e.LastName.ToLower().Contains(term)));
        }

        var items = await query
            .OrderBy(e => e.FirstName)
            .Take(30)
            .Select(e => new
            {
                e.EmployeeId,
                e.EmpCode,
                Name = (e.FirstName ?? "") + (e.LastName != null ? " " + e.LastName : "")
            })
            .ToListAsync();

        return Ok(items);
    }

    // ════════════════════════════════════════════
    //  REIMBURSEMENTS
    // ════════════════════════════════════════════

    [HttpGet("reimbursements")]
    public async Task<IActionResult> GetReimbursements()
    {
        var user = CurrentUser;
        if (user == null) return Unauthorized();

        var empId = await GetEmployeeId(user);
        var items = await _db.HrReimbursements.AsNoTracking()
            .Where(r => r.EmployeeId == empId)
            .OrderByDescending(r => r.ClaimDate)
            .Select(r => new
            {
                r.ReimbursementId,
                r.ReimbursementNo,
                r.ReimbursementType,
                ClaimDate = r.ClaimDate.ToString("dd-MMM-yyyy"),
                r.ClaimAmount,
                r.ApprovedAmount,
                r.PaidAmount,
                r.Description,
                r.Status
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost("reimbursements/submit")]
    public async Task<IActionResult> SubmitReimbursement([FromBody] ReimbursementRequest req)
    {
        var user = CurrentUser;
        if (user == null) return Unauthorized();

        long empId;
        if (req.EmployeeId.HasValue && req.EmployeeId.Value > 0)
        {
            var exists = await _db.MstEmployees.AsNoTracking()
                .AnyAsync(e => e.EmployeeId == req.EmployeeId.Value && e.IsActive == true);
            if (!exists) return BadRequest(new { message = "Selected employee not found or inactive." });
            empId = req.EmployeeId.Value;
        }
        else
        {
            empId = await GetEmployeeId(user);
        }
        if (empId == 0) return BadRequest(new { message = "Employee not found. Please select an employee." });

        var reim = new persistence.Models.HrReimbursement
        {
            EmployeeId = empId,
            ReimbursementNo = await GenerateHrmsNo("RMB"),
            ReimbursementType = req.ReimbursementType,
            ClaimDate = DateOnly.Parse(req.ClaimDate),
            ClaimAmount = req.ClaimAmount,
            Description = req.Description,
            Status = "PENDING",
            CreatedBy = user.UserId,
            CreatedOn = DateTime.Now
        };

        _db.HrReimbursements.Add(reim);
        await _db.SaveChangesAsync();

        await LogActivity(user, "HRMS", "REIMBURSEMENT_SUBMIT", $"Reimbursement submitted: {req.ReimbursementType} ₹{req.ClaimAmount:N0}",
            "HrReimbursement", reim.ReimbursementId);

        await _activityService.LogNotificationAsync(new UserNotificationEntry
        {
            UserId = user.UserId,
            Title = "Reimbursement Submitted",
            Message = $"Your {req.ReimbursementType} reimbursement of ₹{req.ClaimAmount:N0} has been submitted.",
            Icon = "bi-receipt",
            Color = "info",
            Module = "HRMS",
            EventType = "REIMBURSEMENT_SUBMIT",
            Priority = "NORMAL"
        });

        if (!string.IsNullOrEmpty(user.EmailId))
        {
            _ = _notifier.SendEmailAsync(
                user.EmailId,
                "HRMS: Reimbursement Submitted",
                $"<h3>Reimbursement Confirmation</h3>"
                + $"<p>Hi <b>{user.Name}</b>,</p>"
                + $"<p>Your <b>{req.ReimbursementType}</b> reimbursement of <b>₹{req.ClaimAmount:N0}</b> has been submitted for approval.</p>"
                + $"<p>Description: {req.Description ?? "N/A"}</p>"
                + $"<hr/><small style='color:#888;'>MinePress ERP — Automated HRMS Notification</small>");
        }

        _ = NotifyDepartmentAsync("HR",
            "HRMS Alert: Reimbursement Claim",
            $"<h3>Reimbursement Claim — HR Notification</h3>"
            + $"<p>Employee <b>{user.Name}</b> ({user.EmployeeCode}) has submitted a reimbursement claim.</p>"
            + $"<p>Type: <b>{req.ReimbursementType}</b> | Amount: <b>₹{req.ClaimAmount:N0}</b></p>"
            + $"<p>Description: {req.Description ?? "N/A"}</p>"
            + $"<hr/><small style='color:#888;'>MinePress ERP — Automated HRMS Notification (HR Dept)</small>");

        _ = NotifyDepartmentAsync("FIN",
            "Finance Alert: Reimbursement Claim",
            $"<h3>Reimbursement Claim — Finance Notification</h3>"
            + $"<p>Employee <b>{user.Name}</b> ({user.EmployeeCode}) has submitted a reimbursement claim.</p>"
            + $"<p>Type: <b>{req.ReimbursementType}</b> | Amount: <b>₹{req.ClaimAmount:N0}</b></p>"
            + $"<p>Description: {req.Description ?? "N/A"}</p>"
            + $"<hr/><small style='color:#888;'>MinePress ERP — Automated HRMS Notification (Accounts &amp; Finance)</small>");

        _ = NotifyDepartmentAsync("IT",
            "ERP Alert: Reimbursement Claim",
            $"<h3>Reimbursement Claim — IT &amp; ERP Notification</h3>"
            + $"<p>Employee <b>{user.Name}</b> ({user.EmployeeCode}) has submitted a reimbursement claim.</p>"
            + $"<p>Type: <b>{req.ReimbursementType}</b> | Amount: <b>₹{req.ClaimAmount:N0}</b></p>"
            + $"<hr/><small style='color:#888;'>MinePress ERP — Automated Notification (IT &amp; ERP Support)</small>");

        return Ok(new { message = "Reimbursement submitted", reimbursementId = reim.ReimbursementId });
    }

    // ════════════════════════════════════════════
    //  HELPERS
    // ════════════════════════════════════════════

    private async Task<long> GetEmployeeId(UserSessionData user)
    {
        if (string.IsNullOrEmpty(user.EmployeeCode)) return 0;
        var emp = await _db.MstEmployees.AsNoTracking()
            .Where(e => e.EmpCode == user.EmployeeCode && e.IsActive == true)
            .Select(e => e.EmployeeId)
            .FirstOrDefaultAsync();
        return emp;
    }

    private async Task<string> GenerateHrmsNo(string prefix)
    {
        var today = DateTime.Now;
        var datePart = today.ToString("yyyyMMdd");
        var random = today.ToString("HHmmss");
        return await Task.FromResult($"{prefix}-{datePart}-{random}");
    }

    private async Task NotifyDepartmentAsync(string deptCode, string subject, string htmlBody)
    {
        try
        {
            var emails = await _db.MstUsers.AsNoTracking()
                .Join(_db.MstDepartments,
                    u => u.Departmentid,
                    d => d.DeptId,
                    (u, d) => new { u.Emailid, u.Isactive, d.DeptCode })
                .Where(x => x.DeptCode == deptCode && x.Isactive == true && !string.IsNullOrEmpty(x.Emailid))
                .Select(x => x.Emailid!)
                .Distinct()
                .ToListAsync();

            foreach (var email in emails)
            {
                _ = _notifier.SendEmailAsync(email, subject, htmlBody);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to notify {DeptCode} department: {Subject}", deptCode, subject);
        }
    }

    private async Task LogActivity(UserSessionData user, string module, string activityType, string title,
        string? entityType = null, long? entityId = null, string? severity = "INFO")
    {
        try
        {
            var entry = ActivityLogEntry.FromUser(user, module, activityType, title);
            entry.SubModule = "HRMS";
            entry.EntityType = entityType;
            entry.EntityId = entityId;
            entry.Severity = severity;
            await _activityService.LogActivityAsync(entry);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log HRMS activity: {Title}", title);
        }
    }
}

// ════════════════════════════════════════════
//  REQUEST DTOs
// ════════════════════════════════════════════

public class LeaveApplyRequest
{
    public int LeaveTypeId { get; set; }
    public string FromDate { get; set; } = string.Empty;
    public string ToDate { get; set; } = string.Empty;
    public decimal TotalDays { get; set; }
    public bool HalfDay { get; set; }
    public string? Reason { get; set; }
}

public class LoanApplyRequest
{
    public string LoanType { get; set; } = string.Empty;
    public string LoanDate { get; set; } = string.Empty;
    public decimal LoanAmount { get; set; }
    public decimal InterestRate { get; set; }
    public int TenureMonths { get; set; }
    public decimal EmiAmount { get; set; }
}

public class AdvanceApplyRequest
{
    public string AdvanceDate { get; set; } = string.Empty;
    public decimal AdvanceAmount { get; set; }
    public int RepaymentMonths { get; set; }
}

public class MedicalClaimRequest
{
    public string ClaimDate { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string? Relation { get; set; }
    public string? HospitalName { get; set; }
    public decimal ClaimAmount { get; set; }
}

public class OvertimeRequest
{
    public string OtDate { get; set; } = string.Empty;
    public string FromTime { get; set; } = string.Empty;
    public string ToTime { get; set; } = string.Empty;
    public decimal OtHours { get; set; }
    public decimal OtRatePerHour { get; set; }
}

public class ResignationRequest
{
    public string ResignationDate { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string? LastWorkingDay { get; set; }
    public int NoticePeriodDays { get; set; }
}

public class TravelExpenseRequest
{
    public string Purpose { get; set; } = string.Empty;
    public string FromLocation { get; set; } = string.Empty;
    public string ToLocation { get; set; } = string.Empty;
    public string TravelDate { get; set; } = string.Empty;
    public string? ReturnDate { get; set; }
    public decimal ClaimAmount { get; set; }
}

public class ReimbursementRequest
{
    public long? EmployeeId { get; set; }
    public string ReimbursementType { get; set; } = string.Empty;
    public string ClaimDate { get; set; } = string.Empty;
    public decimal ClaimAmount { get; set; }
    public string? Description { get; set; }
}
