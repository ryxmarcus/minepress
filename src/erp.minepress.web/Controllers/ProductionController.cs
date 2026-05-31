using erp.minepress.infrastructure.ErrorLogging;
using erp.minepress.persistence.Context;
using erp.minepress.persistence.Models;
using erp.minepress.domain.Enums;
using erp.minepress.notification.Interfaces;
using erp.minepress.web.Helpers;
using erp.minepress.web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace erp.minepress.web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductionController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<ProductionController> _logger;
    private readonly INotificationService _notification;
    private readonly IUserActivityService _activity;
    private readonly IHttpContextAccessor _httpCtx;
    private readonly ISystemErrorLogger _systemErrorLogger;

    public ProductionController(
        ApplicationDbContext db,
        ILogger<ProductionController> logger,
        INotificationService notification,
        IUserActivityService activity,
        IHttpContextAccessor httpCtx,
        ISystemErrorLogger systemErrorLogger)
    {
        _db = db;
        _logger = logger;
        _notification = notification;
        _activity = activity;
        _httpCtx = httpCtx;
        _systemErrorLogger = systemErrorLogger;
    }

    private async Task AuditExceptionAsync(Exception ex, string additionalData, string severity = "Error")
        => await _systemErrorLogger.LogAsync(ex, HttpContext, severity, additionalData);

    // ─── Machines ───────────────────────────────────────────────

    /// <summary>All active production machines with current running job info.</summary>
    [HttpGet("machines")]
    public async Task<IActionResult> GetMachines()
    {
        var machines = await _db.MstMachines
            .Where(m => m.IsActive == true)
            .OrderBy(m => m.AutoSelectPriority ?? 999)
            .ThenBy(m => m.MachineName)
            .Select(m => new
            {
                m.MachineId,
                m.MachineCode,
                m.MachineName,
                m.MachineCategory,
                m.MachineType,
                m.Manufacturer,
                m.ModelNo,
                m.MaxSheetLengthMm,
                m.MaxSheetWidthMm,
                m.MinSheetLengthMm,
                m.MinSheetWidthMm,
                m.MinGsm,
                m.MaxGsm,
                m.MaxColors,
                m.PrintingSide,
                m.MaxSpeedPerHour,
                m.SetupTimeMin,
                m.ChangeoverTimeMin,
                m.HourlyRunningCost,
                m.ManpowerRequired,
                m.MaintenanceCycleDays,
                m.AutoSelectPriority,
                m.IsProductionMachine,
                NextMaintenance = m.MstMachineMaintenances
                    .Where(mt => mt.IsActive == true && mt.NextDueDate != null)
                    .OrderBy(mt => mt.NextDueDate)
                    .Select(mt => new { mt.MaintenanceType, mt.NextDueDate })
                    .FirstOrDefault()
            })
            .ToListAsync();

        return Ok(machines);
    }

    // ─── Unallocated Jobs (pending / in-progress, not yet allocated) ────

    [HttpGet("unallocated-jobs")]
    public async Task<IActionResult> GetUnallocatedJobs([FromQuery] string? jobType)
    {
        // Job IDs that already have an active allocation
        var allocatedJobIds = await _db.TrnJobMachineAllocations
            .Where(a => a.IsActive == true && a.AllocationStatus == "ALLOCATED")
            .Select(a => a.JobId)
            .Distinct()
            .ToListAsync();

        // When jobType filter is specified, find matching job IDs from trn_job_item
        List<long>? jobIdsByType = null;
        if (!string.IsNullOrWhiteSpace(jobType))
        {
            jobIdsByType = await _db.TrnJobItems
                .Where(ji => ji.JobTypeName == jobType)
                .Select(ji => ji.JobId)
                .Distinct()
                .ToListAsync();
        }

        // Jobs that are open, not completed, and not already allocated
        var query = _db.TrnJobs
            .Include(j => j.Party)
            .Include(j => j.JobType)
            .Where(j => j.StatusCode != "CLOSED" && j.StatusCode != "CANCELLED"
                      && j.StatusCode != "COMPLETED"
                      && !allocatedJobIds.Contains(j.JobId));

        // Filter by job type from trn_job_item
        if (jobIdsByType != null)
            query = query.Where(j => jobIdsByType.Contains(j.JobId));

        var jobs = await query
            .OrderBy(j => j.AiPriorityScore != null ? 0 : 1)
            .ThenByDescending(j => j.AiPriorityScore)
            .ThenBy(j => j.DeliveryDate)
            .ThenByDescending(j => j.Priority)
            .Select(j => new
            {
                j.JobId,
                j.JobNo,
                j.ProductName,
                j.Quantity,
                j.Priority,
                j.DeliveryDate,
                j.StatusCode,
                j.AiPriorityScore,
                j.CurrentStage,
                j.ProgressPercent,
                PartyName = j.Party != null ? j.Party.Name : null,
                JobTypeName = j.JobType != null ? j.JobType.Jobtypename : null,
                SpecsJson = j.SpecificationsJson
            })
            .Take(100)
            .ToListAsync();

        return Ok(jobs);
    }

    // ─── AI Suggestions: match job specs → best machines ────────

    [HttpGet("ai-suggestions/{jobId:long}")]
    public async Task<IActionResult> GetAiSuggestions(long jobId)
    {
        var job = await _db.TrnJobs
            .Include(j => j.JobType)
            .FirstOrDefaultAsync(j => j.JobId == jobId);

        if (job == null) return NotFound();

        // Parse specs from JSON if available
        int? sheetLength = null, sheetWidth = null, gsm = null, colors = null;
        string? printSide = null, jobTypeName = null;

        jobTypeName = job.JobType?.Jobtypename;

        if (!string.IsNullOrEmpty(job.SpecificationsJson))
        {
            try
            {
                var specs = System.Text.Json.JsonDocument.Parse(job.SpecificationsJson);
                var root = specs.RootElement;

                if (root.TryGetProperty("sheet_length_mm", out var sl)) sheetLength = sl.GetInt32();
                else if (root.TryGetProperty("sheetLengthMm", out sl)) sheetLength = sl.GetInt32();

                if (root.TryGetProperty("sheet_width_mm", out var sw)) sheetWidth = sw.GetInt32();
                else if (root.TryGetProperty("sheetWidthMm", out sw)) sheetWidth = sw.GetInt32();

                if (root.TryGetProperty("gsm", out var g)) gsm = g.GetInt32();
                if (root.TryGetProperty("colors", out var c)) colors = c.GetInt32();
                if (root.TryGetProperty("printing_side", out var ps)) printSide = ps.GetString();
                else if (root.TryGetProperty("printingSide", out ps)) printSide = ps.GetString();
            }
            catch { /* specs parse failed — continue with nulls */ }
        }

        // Get all active machines
        var machines = await _db.MstMachines
            .Where(m => m.IsActive == true)
            .ToListAsync();

        // Get selection rules
        var rules = await _db.MstMachineSelectionRules
            .Where(r => r.IsActive == true)
            .ToListAsync();

        // Score each machine
        var suggestions = machines.Select(m =>
        {
            int score = 0;
            var reasons = new List<string>();

            // Size compatibility
            bool sizeOk = true;
            if (sheetLength.HasValue)
            {
                if (m.MinSheetLengthMm.HasValue && sheetLength < m.MinSheetLengthMm) sizeOk = false;
                if (m.MaxSheetLengthMm.HasValue && sheetLength > m.MaxSheetLengthMm) sizeOk = false;
            }
            if (sheetWidth.HasValue)
            {
                if (m.MinSheetWidthMm.HasValue && sheetWidth < m.MinSheetWidthMm) sizeOk = false;
                if (m.MaxSheetWidthMm.HasValue && sheetWidth > m.MaxSheetWidthMm) sizeOk = false;
            }
            if (sizeOk) { score += 25; reasons.Add("Sheet size compatible"); }
            else reasons.Add("⚠ Sheet size out of range");

            // GSM compatibility
            bool gsmOk = true;
            if (gsm.HasValue)
            {
                if (m.MinGsm.HasValue && gsm < m.MinGsm) gsmOk = false;
                if (m.MaxGsm.HasValue && gsm > m.MaxGsm) gsmOk = false;
            }
            if (gsmOk) { score += 20; reasons.Add("GSM compatible"); }
            else reasons.Add("⚠ GSM out of range");

            // Color compatibility
            if (colors.HasValue && m.MaxColors.HasValue)
            {
                if (colors <= m.MaxColors) { score += 15; reasons.Add($"Supports {m.MaxColors} colors"); }
                else reasons.Add($"⚠ Needs {colors} colors, machine max {m.MaxColors}");
            }
            else { score += 10; }

            // Printing side
            if (!string.IsNullOrEmpty(printSide) && !string.IsNullOrEmpty(m.PrintingSide))
            {
                if (m.PrintingSide.Contains(printSide, StringComparison.OrdinalIgnoreCase))
                { score += 10; reasons.Add("Printing side match"); }
                else reasons.Add("⚠ Printing side mismatch");
            }
            else score += 5;

            // Speed bonus — higher speed = better
            if (m.MaxSpeedPerHour.HasValue && m.MaxSpeedPerHour > 0)
            {
                score += Math.Min(15, m.MaxSpeedPerHour.Value / 500);
                reasons.Add($"Speed: {m.MaxSpeedPerHour}/hr");
            }

            // Cost efficiency — lower cost = better
            if (m.HourlyRunningCost.HasValue && m.HourlyRunningCost > 0)
            {
                score += Math.Max(0, 10 - (int)(m.HourlyRunningCost.Value / 100));
                reasons.Add($"Cost: ₹{m.HourlyRunningCost}/hr");
            }

            // Auto-select priority from master
            if (m.AutoSelectPriority.HasValue)
            {
                score += Math.Max(0, 10 - m.AutoSelectPriority.Value);
            }

            // Match selection rules
            var matchingRules = rules.Where(r =>
                (string.IsNullOrEmpty(r.JobType) || (jobTypeName ?? "").Contains(r.JobType, StringComparison.OrdinalIgnoreCase)) &&
                (!r.MinLengthMm.HasValue || !sheetLength.HasValue || sheetLength >= r.MinLengthMm) &&
                (!r.MaxLengthMm.HasValue || !sheetLength.HasValue || sheetLength <= r.MaxLengthMm) &&
                (!r.MinWidthMm.HasValue || !sheetWidth.HasValue || sheetWidth >= r.MinWidthMm) &&
                (!r.MaxWidthMm.HasValue || !sheetWidth.HasValue || sheetWidth <= r.MaxWidthMm) &&
                (!r.MinGsm.HasValue || !gsm.HasValue || gsm >= r.MinGsm) &&
                (!r.MaxGsm.HasValue || !gsm.HasValue || gsm <= r.MaxGsm)
            ).ToList();

            if (matchingRules.Count > 0)
            {
                score += 15;
                reasons.Add($"{matchingRules.Count} selection rule(s) matched");
            }

            return new
            {
                m.MachineId,
                m.MachineCode,
                m.MachineName,
                m.MachineCategory,
                m.MaxSheetLengthMm,
                m.MaxSheetWidthMm,
                m.MaxColors,
                m.MaxSpeedPerHour,
                m.HourlyRunningCost,
                Score = score,
                MaxScore = 100,
                Confidence = score >= 70 ? "High" : score >= 45 ? "Medium" : "Low",
                Reasons = reasons,
                IsCompatible = sizeOk && gsmOk
            };
        })
        .OrderByDescending(x => x.Score)
        .ToList();

        return Ok(new
        {
            jobId,
            jobNo = job.JobNo,
            productName = job.ProductName,
            specs = new { sheetLength, sheetWidth, gsm, colors, printSide },
            suggestions
        });
    }

    // ─── Machine Status Dashboard ───────────────────────────────

    [HttpGet("machine-status")]
    public async Task<IActionResult> GetMachineStatus()
    {
        var machines = await _db.MstMachines
            .Where(m => m.IsActive == true)
            .Include(m => m.MstMachineMaintenances.Where(mt => mt.IsActive == true))
            .Include(m => m.TrnMachineBreakdowns.Where(b => b.IsActive == true
                && (b.BreakdownStatus == "Open" || b.BreakdownStatus == "Assigned" || b.BreakdownStatus == "In Progress")))
            .OrderBy(m => m.AutoSelectPriority ?? 999)
            .ThenBy(m => m.MachineName)
            .ToListAsync();

        var result = machines.Select(m =>
        {
            var nextMaint = m.MstMachineMaintenances
                .Where(mt => mt.NextDueDate != null)
                .OrderBy(mt => mt.NextDueDate)
                .FirstOrDefault();

            bool maintenanceDue = nextMaint?.NextDueDate != null
                && nextMaint.NextDueDate.Value.ToDateTime(TimeOnly.MinValue) <= DateTime.Now.AddDays(3);

            var activeBreakdown = m.TrnMachineBreakdowns
                .OrderByDescending(b => b.BreakdownStartTime)
                .FirstOrDefault();

            return new
            {
                m.MachineId,
                m.MachineCode,
                m.MachineName,
                m.MachineCategory,
                m.MachineType,
                m.MaxSpeedPerHour,
                m.HourlyRunningCost,
                m.ManpowerRequired,
                m.MaxColors,
                m.PrintingSide,
                MaintenanceDue = maintenanceDue,
                NextMaintenanceDate = nextMaint?.NextDueDate,
                NextMaintenanceType = nextMaint?.MaintenanceType,
                MaintenanceRepairStatus = nextMaint?.RepairStatus,
                MaintenanceDowntimeMinutes = nextMaint?.DowntimeMinutes,
                HasActiveBreakdown = activeBreakdown != null,
                ActiveBreakdown = activeBreakdown != null ? new
                {
                    activeBreakdown.BreakdownId,
                    activeBreakdown.FaultCategory,
                    activeBreakdown.SeverityLevel,
                    activeBreakdown.BreakdownStatus,
                    activeBreakdown.BreakdownStartTime,
                    activeBreakdown.DowntimeMinutes,
                    activeBreakdown.TechnicianName,
                    activeBreakdown.FaultDescription
                } : null
            };
        });

        return Ok(result);
    }

    // ─── Maintenance CRUD ───────────────────────────────────────

    [HttpGet("maintenance")]
    public async Task<IActionResult> GetMaintenance([FromQuery] long? machineId)
    {
        var query = _db.MstMachineMaintenances
            .Include(m => m.Machine)
            .AsQueryable();

        if (machineId.HasValue)
            query = query.Where(m => m.MachineId == machineId);

        var list = await query
        .OrderByDescending(m => m.NextDueDate)
        .Select(m => new
        {
            m.MaintenanceId,
            m.MachineId,
            MachineName = m.Machine != null ? m.Machine.MachineName : null,
            MachineCode = m.Machine != null ? m.Machine.MachineCode : null,
            m.MaintenanceType,
            m.FrequencyDays,
            m.LastMaintenanceDate,
            m.NextDueDate,
            m.VendorName,
            m.EstimatedCost,
            m.Remarks,
            m.IsActive,
            m.BreakdownStartTime,
            m.BreakdownEndTime,
            m.DowntimeMinutes,
            m.RepairStatus,
            m.CompletionDate,
            IsOverdue = m.NextDueDate.HasValue &&
                m.NextDueDate.Value.ToDateTime(TimeOnly.MinValue) < DateTime.Now
        })
        .ToListAsync();

        return Ok(list);
    }

    [HttpPost("maintenance")]
    public async Task<IActionResult> CreateMaintenance([FromBody] MaintenanceDto dto)
    {
        var record = new MstMachineMaintenance
        {
            MachineId = dto.MachineId,
            MaintenanceType = dto.MaintenanceType,
            FrequencyDays = dto.FrequencyDays,
            LastMaintenanceDate = dto.LastMaintenanceDate,
            NextDueDate = dto.NextDueDate,
            VendorName = dto.VendorName,
            EstimatedCost = dto.EstimatedCost,
            Remarks = dto.Remarks,
            IsActive = true,
            BreakdownStartTime = dto.BreakdownStartTime,
            BreakdownEndTime = dto.BreakdownEndTime,
            DowntimeMinutes = dto.DowntimeMinutes,
            RepairStatus = dto.RepairStatus ?? "Pending",
            CompletionDate = dto.CompletionDate
        };

        _db.MstMachineMaintenances.Add(record);
        await _db.SaveChangesAsync();

        // Notify employees mapped to this machine about new maintenance
        try
        {
            var machine = await _db.MstMachines.FindAsync(dto.MachineId);
            var mappedEmps = await _db.MstEmployeeMachineMappings
                .Include(m => m.Employee)
                .Where(m => m.MachineId == dto.MachineId && m.IsActive == true && m.IsAuthorized == true)
                .ToListAsync();

            foreach (var mapped in mappedEmps)
            {
                var email = mapped.Employee?.Email1;
                if (string.IsNullOrWhiteSpace(email)) continue;
                try
                {
                    await _notification.SendEmailAsync(
                        email,
                        $"Maintenance Scheduled - {machine?.MachineName}",
                        $"Dear {mapped.EmployeeName},\n\nA {dto.MaintenanceType} maintenance has been scheduled for machine {machine?.MachineName} ({machine?.MachineCode}).\n\nType: {dto.MaintenanceType}\nDue Date: {dto.NextDueDate:dd-MMM-yyyy}\nVendor: {dto.VendorName ?? "N/A"}\n\nPlease ensure the machine is available for maintenance as scheduled.\n\nRegards,\nMinePress ERP");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send maintenance email to {Email}", email);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send maintenance notification emails");
        }

        return Ok(new { record.MaintenanceId, message = "Maintenance scheduled" });
    }

    [HttpPut("maintenance/{id:long}")]
    public async Task<IActionResult> UpdateMaintenance(long id, [FromBody] MaintenanceDto dto)
    {
        var record = await _db.MstMachineMaintenances.FindAsync(id);
        if (record == null) return NotFound();

        record.MachineId = dto.MachineId;
        record.MaintenanceType = dto.MaintenanceType;
        record.FrequencyDays = dto.FrequencyDays;
        record.LastMaintenanceDate = dto.LastMaintenanceDate;
        record.NextDueDate = dto.NextDueDate;
        record.VendorName = dto.VendorName;
        record.EstimatedCost = dto.EstimatedCost;
        record.Remarks = dto.Remarks;
        record.IsActive = dto.IsActive ?? true;
        record.BreakdownStartTime = dto.BreakdownStartTime;
        record.BreakdownEndTime = dto.BreakdownEndTime;
        record.DowntimeMinutes = dto.DowntimeMinutes;
        record.RepairStatus = dto.RepairStatus;
        record.CompletionDate = dto.CompletionDate;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Maintenance updated" });
    }

    [HttpPost("maintenance/{id:long}/complete")]
    public async Task<IActionResult> CompleteMaintenance(long id)
    {
        var record = await _db.MstMachineMaintenances.FindAsync(id);
        if (record == null) return NotFound();

        record.LastMaintenanceDate = DateOnly.FromDateTime(DateTime.Now);
        record.RepairStatus = "Completed";
        record.CompletionDate = DateTime.Now;
        if (record.FrequencyDays.HasValue)
            record.NextDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(record.FrequencyDays.Value));

        await _db.SaveChangesAsync();

        // Notify employees mapped to this machine about maintenance completion
        try
        {
            var machine = await _db.MstMachines.FindAsync(record.MachineId);
            var mappedEmps = await _db.MstEmployeeMachineMappings
                .Include(m => m.Employee)
                .Where(m => m.MachineId == record.MachineId && m.IsActive == true && m.IsAuthorized == true)
                .ToListAsync();

            foreach (var mapped in mappedEmps)
            {
                var email = mapped.Employee?.Email1;
                if (string.IsNullOrWhiteSpace(email)) continue;
                try
                {
                    await _notification.SendEmailAsync(
                        email,
                        $"Maintenance Completed - {machine?.MachineName}",
                        $"Dear {mapped.EmployeeName},\n\nMaintenance for machine {machine?.MachineName} ({machine?.MachineCode}) has been completed.\n\nCompletion Date: {DateTime.Now:dd-MMM-yyyy HH:mm}\nNext Due Date: {record.NextDueDate:dd-MMM-yyyy}\n\nThe machine is now available for production.\n\nRegards,\nMinePress ERP");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send maintenance completion email to {Email}", email);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send maintenance completion notification emails");
        }

        return Ok(new { message = "Maintenance completed", record.NextDueDate });
    }

    // ─── Employees (Manpower) ────────────────────────────────────

    [HttpGet("employees")]
    public async Task<IActionResult> GetEmployees([FromQuery] string? dept, [FromQuery] string? search)
    {
        var query = _db.MstEmployees
            .Include(e => e.Dept)
            .Where(e => e.IsActive == true);

        if (!string.IsNullOrWhiteSpace(dept))
            query = query.Where(e => e.Dept != null && e.Dept.DeptCode == dept);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(e =>
                (e.FirstName != null && e.FirstName.Contains(search)) ||
                (e.LastName != null && e.LastName.Contains(search)) ||
                (e.EmpCode != null && e.EmpCode.Contains(search)));

        var employees = await query
            .OrderBy(e => e.FirstName)
            .Take(50)
            .Select(e => new
            {
                e.EmployeeId,
                e.EmpCode,
                e.FirstName,
                e.LastName,
                FullName = (e.FirstName ?? "") + " " + (e.LastName ?? ""),
                DeptCode = e.Dept != null ? e.Dept.DeptCode : null,
                DeptName = e.Dept != null ? e.Dept.DeptName : null,
                e.Email1,
                e.MobileNo1
            })
            .ToListAsync();

        return Ok(employees);
    }

    // ─── Filter Data ────────────────────────────────────────────

    [HttpGet("filter-data")]
    public async Task<IActionResult> GetFilterData()
    {
        var jobTypes = await _db.MstJobTypes
            .Where(jt => jt.Isactive == true)
            .OrderBy(jt => jt.Jobtypename)
            .Select(jt => new { jt.Jobtypeid, jt.Jobtypename })
            .ToListAsync();

        var machineCategories = await _db.MstMachines
            .Where(m => m.IsActive == true && m.MachineCategory != null)
            .Select(m => m.MachineCategory)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        var productTypes = await _db.TrnJobs
            .Where(j => j.StatusCode != "CLOSED" && j.StatusCode != "CANCELLED"
                      && j.StatusCode != "COMPLETED" && j.ProductName != null)
            .Select(j => j.ProductName)
            .Distinct()
            .OrderBy(p => p)
            .Take(100)
            .ToListAsync();

        return Ok(new { jobTypes, machineCategories, productTypes });
    }

    // ─── Save All Allocations (Machine + Manpower) ──────────────

    [HttpPost("save-allocations")]
    public async Task<IActionResult> SaveAllocations([FromBody] SaveAllocationsDto dto)
    {
        try
        {
        var user = _httpCtx.HttpContext?.Session.GetCurrentUser();
        var now = DateTime.Now;
        int savedCount = 0;
        var skippedJobs = new List<string>();

        foreach (var alloc in dto.Allocations)
        {
            var job = await _db.TrnJobs.FindAsync(alloc.JobId);
            var machine = await _db.MstMachines.FindAsync(alloc.MachineId);

            if (job == null)
            {
                _logger.LogWarning("SaveAllocations: JobId {JobId} not found — skipped", alloc.JobId);
                skippedJobs.Add($"JobId {alloc.JobId} not found");
                continue;
            }
            if (machine == null)
            {
                _logger.LogWarning("SaveAllocations: MachineId {MachineId} not found — skipped", alloc.MachineId);
                skippedJobs.Add($"MachineId {alloc.MachineId} not found");
                continue;
            }

            // Check if this job is already allocated to any machine
            var existingAlloc = await _db.TrnJobMachineAllocations
                .FirstOrDefaultAsync(a => a.JobId == alloc.JobId && a.IsActive == true && a.AllocationStatus == "ALLOCATED");

            if (existingAlloc != null)
            {
                if (existingAlloc.MachineId == alloc.MachineId)
                {
                    // Same machine — skip silently
                    skippedJobs.Add($"{job.JobNo} (already on {existingAlloc.MachineName})");
                    continue;
                }
                else
                {
                    // Moving to different machine — deactivate old allocation + manpower
                    existingAlloc.IsActive = false;
                    existingAlloc.AllocationStatus = "MOVED";
                    existingAlloc.ModifiedBy = user?.UserCode;
                    existingAlloc.ModifiedOn = now;

                    var oldManpower = await _db.TrnJobMachineManpowerAllocations
                        .Where(m => m.AllocationId == existingAlloc.AllocationId && m.IsActive == true)
                        .ToListAsync();
                    foreach (var mp in oldManpower)
                    {
                        mp.IsActive = false;
                        mp.ModifiedBy = user?.UserCode;
                        mp.ModifiedOn = now;
                    }

                    _db.TrnJobTimelines.Add(new TrnJobTimeline
                    {
                        JobId = alloc.JobId,
                        EventType = "PRODUCTION",
                        EventCode = "MACHINE_MOVED",
                        EventTitle = $"Moved from {existingAlloc.MachineName} to {machine.MachineName}",
                        EventDescription = $"Job {job.JobNo} moved from {existingAlloc.MachineName} to {machine.MachineName}",
                        MachineId = alloc.MachineId,
                        CreatedBy = user?.UserId ?? 0,
                        CreatedOn = now,
                        IsActive = true
                    });
                }
            }

            var record = new TrnJobMachineAllocation
            {
                JobId = alloc.JobId,
                JobNo = job.JobNo ?? "",
                ProcessCode = "PRINTING",
                ProcessName = "Printing",
                MachineId = alloc.MachineId,
                MachineCode = machine.MachineCode,
                MachineName = machine.MachineName,
                PlannedQuantity = job.Quantity,
                AllocationStatus = "ALLOCATED",
                CreatedBy = user?.UserCode,
                CreatedOn = now,
                IsActive = true
            };

            try
            {
                _db.TrnJobMachineAllocations.Add(record);
                await _db.SaveChangesAsync();
                _logger.LogInformation("SaveAllocations: Inserted TrnJobMachineAllocation AllocationId={AllocationId} JobId={JobId} MachineId={MachineId}",
                    record.AllocationId, alloc.JobId, alloc.MachineId);
            }
            catch (Exception exAlloc)
            {
                _logger.LogError(exAlloc,
                    "SaveAllocations: FAILED inserting TrnJobMachineAllocation JobId={JobId} MachineId={MachineId} — {Message}",
                    alloc.JobId, alloc.MachineId, exAlloc.Message);
                await AuditExceptionAsync(exAlloc,
                    $"SaveAllocations > TrnJobMachineAllocation INSERT JobId={alloc.JobId} MachineId={alloc.MachineId}");
                skippedJobs.Add($"{job.JobNo} (allocation save error: {exAlloc.Message})");
                // Detach the failed entity so the context stays usable
                _db.ChangeTracker.Clear();
                continue;
            }

            savedCount++;

            // Auto-assign mapped employees from mst_employee_machine_mapping
            var mappedEmployees = await _db.MstEmployeeMachineMappings
                .Include(m => m.Employee)
                .Where(m => m.MachineId == alloc.MachineId && m.IsActive == true && m.IsAuthorized == true)
                .ToListAsync();

            foreach (var mapped in mappedEmployees)
            {
                _db.TrnJobMachineManpowerAllocations.Add(new TrnJobMachineManpowerAllocation
                {
                    AllocationId = record.AllocationId,
                    JobId = alloc.JobId,
                    JobNo = record.JobNo,
                    MachineId = alloc.MachineId,
                    EmployeeId = mapped.EmployeeId,
                    EmployeeCode = mapped.EmployeeCode,
                    EmployeeName = mapped.EmployeeName,
                    RoleCode = mapped.RoleCode ?? "Operator",
                    RoleName = mapped.RoleName ?? "Operator",
                    ShiftCode = "GENERAL",
                    AllocationStatus = "ASSIGNED",
                    CreatedBy = user?.UserCode,
                    CreatedOn = now,
                    IsActive = true
                });
            }

            // Job timeline — saved atomically with manpower
            _db.TrnJobTimelines.Add(new TrnJobTimeline
            {
                JobId = alloc.JobId,
                EventType = "PRODUCTION",
                EventCode = "MACHINE_ALLOCATED",
                EventTitle = $"Allocated to {machine.MachineName}",
                EventDescription = $"Job {job.JobNo} allocated to machine {machine.MachineName} ({machine.MachineCode}). " +
                    $"Category: {machine.MachineCategory ?? "—"}, Speed: {machine.MaxSpeedPerHour?.ToString() ?? "—"}/hr, " +
                    $"Cost: ₹{machine.HourlyRunningCost?.ToString() ?? "—"}/hr, Planned Qty: {job.Quantity.ToString("N0")}, " +
                    $"Auto-assigned {mappedEmployees.Count} employee(s).",
                MachineId = alloc.MachineId,
                CreatedBy = user?.UserId ?? 0,
                CreatedOn = now,
                IsActive = true
            });

            try
            {
                await _db.SaveChangesAsync();
                _logger.LogInformation("SaveAllocations: Saved manpower ({Count}) + timeline for AllocationId={AllocationId}",
                    mappedEmployees.Count, record.AllocationId);
            }
            catch (Exception exMp)
            {
                _logger.LogError(exMp,
                    "SaveAllocations: FAILED saving manpower/timeline for AllocationId={AllocationId} — {Message}",
                    record.AllocationId, exMp.Message);
                await AuditExceptionAsync(exMp,
                    $"SaveAllocations > Manpower/Timeline INSERT AllocationId={record.AllocationId} JobId={alloc.JobId}");
                // Allocation itself saved OK; clear pending manpower/timeline to allow next iteration
                _db.ChangeTracker.Clear();
            }
        }

        // Notify employees mapped to each machine about new job allocation
        var notifiedMachineIds = new HashSet<long>();
        foreach (var alloc in dto.Allocations)
        {
            if (!notifiedMachineIds.Add(alloc.MachineId)) continue;

            var mappedEmps = await _db.MstEmployeeMachineMappings
                .Include(m => m.Employee)
                .Where(m => m.MachineId == alloc.MachineId && m.IsActive == true && m.IsAuthorized == true)
                .ToListAsync();

            var jobEntity = await _db.TrnJobs.FindAsync(alloc.JobId);
            var machEntity = await _db.MstMachines.FindAsync(alloc.MachineId);

            foreach (var mapped in mappedEmps)
            {
                var email = mapped.Employee?.Email1;
                if (string.IsNullOrWhiteSpace(email)) continue;
                try
                {
                    await _notification.SendEmailAsync(
                        email,
                        $"New Job Allocated - {jobEntity?.JobNo}",
                        $"Dear {mapped.EmployeeName},\n\nA new job {jobEntity?.JobNo} has been allocated to your machine {machEntity?.MachineName} ({machEntity?.MachineCode}).\n\nPlease check the production board for details.\n\nRegards,\nMinePress ERP");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send job allocation email to {Email}", email);
                }
            }
        }

        // Activity log
        if (user != null)
        {
            var entry = ActivityLogEntry.FromUser(user, "PRODUCTION", "ALLOCATE", $"Saved {savedCount} machine allocation(s)");
            entry.SubModule = "JOB_ALLOCATION";
            entry.EntityType = "TrnJobMachineAllocation";
            entry.EntityId = user.UserId;
            entry.Description = $"Allocated {savedCount} job(s) to machines";
            await _activity.LogActivityAsync(entry);
        }

        return Ok(new
        {
            message = savedCount > 0
                ? $"{savedCount} allocation(s) saved"
                    + (skippedJobs.Count > 0 ? $". Skipped: {string.Join(", ", skippedJobs)}" : "")
                : skippedJobs.Count > 0
                    ? $"All jobs already allocated: {string.Join(", ", skippedJobs)}"
                    : "No allocations to save",
            savedCount,
            skippedCount = skippedJobs.Count,
            skippedJobs
        });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SaveAllocations failed");
            await AuditExceptionAsync(ex, "ProductionController.SaveAllocations");
            return StatusCode(500, new { message = "An error occurred while saving allocations. Please try again." });
        }
    }

    // ─── Deallocate Job from Machine ─────────────────────────────

    [HttpPost("deallocate-job")]
    public async Task<IActionResult> DeallocateJob([FromBody] DeallocateJobDto dto)
    {
        var user = _httpCtx.HttpContext?.Session.GetCurrentUser();
        var now = DateTime.Now;

        var alloc = await _db.TrnJobMachineAllocations
            .Include(a => a.TrnJobMachineManpowerAllocations)
            .FirstOrDefaultAsync(a => a.JobId == dto.JobId && a.MachineId == dto.MachineId && a.IsActive == true);

        if (alloc == null)
            return Ok(new { message = "No active allocation found", deleted = false });

        // Deactivate manpower allocations
        foreach (var mp in alloc.TrnJobMachineManpowerAllocations.Where(m => m.IsActive == true))
        {
            mp.IsActive = false;
            mp.ModifiedBy = user?.UserCode;
            mp.ModifiedOn = now;
        }

        // Deactivate the allocation itself
        alloc.IsActive = false;
        alloc.AllocationStatus = "DEALLOCATED";
        alloc.ModifiedBy = user?.UserCode;
        alloc.ModifiedOn = now;

        // Job timeline
        _db.TrnJobTimelines.Add(new TrnJobTimeline
        {
            JobId = dto.JobId,
            EventType = "PRODUCTION",
            EventCode = "MACHINE_DEALLOCATED",
            EventTitle = $"Deallocated from {alloc.MachineName}",
            EventDescription = $"Job {alloc.JobNo} deallocated from machine {alloc.MachineName} ({alloc.MachineCode})",
            MachineId = dto.MachineId,
            CreatedBy = user?.UserId ?? 0,
            CreatedOn = now,
            IsActive = true
        });

        await _db.SaveChangesAsync();

        // Notify removed employees via email
        var removedEmployees = alloc.TrnJobMachineManpowerAllocations
            .Where(m => m.EmployeeId > 0)
            .Select(m => m.EmployeeId)
            .Distinct()
            .ToList();

        foreach (var empId in removedEmployees)
        {
            var emp = await _db.MstEmployees.FindAsync(empId);
            if (emp == null || string.IsNullOrWhiteSpace(emp.Email1)) continue;
            try
            {
                await _notification.SendEmailAsync(
                    emp.Email1,
                    $"Job Deallocated - {alloc.JobNo}",
                    $"Dear {emp.FirstName},\n\nJob {alloc.JobNo} has been deallocated from machine {alloc.MachineName} ({alloc.MachineCode}). Your manpower assignment for this job has been removed.\n\nRegards,\nMinePress ERP");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send deallocation email to {Email}", emp.Email1);
            }
        }

        // Activity log
        if (user != null)
        {
            var entry = ActivityLogEntry.FromUser(user, "PRODUCTION", "DEALLOCATE", $"Deallocated job {alloc.JobNo} from {alloc.MachineName}");
            entry.SubModule = "JOB_ALLOCATION";
            entry.EntityType = "TrnJobMachineAllocation";
            entry.EntityId = alloc.AllocationId;
            entry.Description = $"Deallocated job {alloc.JobNo} from machine {alloc.MachineName}";
            await _activity.LogActivityAsync(entry);
        }

        return Ok(new { message = $"Job {alloc.JobNo} deallocated from {alloc.MachineName}", deleted = true });
    }

    // ─── Save Manpower Allocation ────────────────────────────────

    [HttpPost("save-manpower")]
    public async Task<IActionResult> SaveManpowerAllocation([FromBody] SaveManpowerDto dto)
    {
        try
        {
        var user = _httpCtx.HttpContext?.Session.GetCurrentUser();
        var now = DateTime.Now;

        // Find or create the parent machine allocation
        var parentAlloc = await _db.TrnJobMachineAllocations
            .FirstOrDefaultAsync(a => a.JobId == dto.JobId && a.MachineId == dto.MachineId && a.IsActive == true);

        if (parentAlloc == null)
        {
            var job = await _db.TrnJobs.FindAsync(dto.JobId);
            var machine = await _db.MstMachines.FindAsync(dto.MachineId);
            if (job == null || machine == null)
                return BadRequest(new { message = "Invalid job or machine" });

            parentAlloc = new TrnJobMachineAllocation
            {
                JobId = dto.JobId,
                JobNo = job.JobNo ?? "",
                ProcessCode = "PRINTING",
                ProcessName = "Printing",
                MachineId = dto.MachineId,
                MachineCode = machine.MachineCode,
                MachineName = machine.MachineName,
                PlannedQuantity = job.Quantity,
                AllocationStatus = "ALLOCATED",
                CreatedBy = user?.UserCode,
                CreatedOn = now,
                IsActive = true
            };
            _db.TrnJobMachineAllocations.Add(parentAlloc);
            await _db.SaveChangesAsync();
        }

        // Deactivate existing manpower for this allocation
        var existing = await _db.TrnJobMachineManpowerAllocations
            .Where(m => m.AllocationId == parentAlloc.AllocationId && m.IsActive == true)
            .ToListAsync();
        foreach (var e in existing) e.IsActive = false;

        // Resolve machine details for mapping
        var machineEntity = await _db.MstMachines.FindAsync(dto.MachineId);

        // Add new manpower + upsert employee-machine mapping
        int count = 0;
        int newMappings = 0;
        foreach (var emp in dto.Employees)
        {
            var employee = await _db.MstEmployees.FindAsync(emp.EmployeeId);
            if (employee == null) continue;

            var empFullName = (employee.FirstName ?? "") + " " + (employee.LastName ?? "");

            _db.TrnJobMachineManpowerAllocations.Add(new TrnJobMachineManpowerAllocation
            {
                AllocationId = parentAlloc.AllocationId,
                JobId = dto.JobId,
                JobNo = parentAlloc.JobNo,
                MachineId = dto.MachineId,
                EmployeeId = emp.EmployeeId,
                EmployeeCode = employee.EmpCode,
                EmployeeName = empFullName,
                RoleCode = emp.RoleCode ?? "Operator",
                RoleName = emp.RoleCode ?? "Operator",
                ShiftCode = emp.ShiftCode ?? "GENERAL",
                AllocationStatus = "ASSIGNED",
                CreatedBy = user?.UserCode,
                CreatedOn = now,
                IsActive = true
            });
            count++;

            // ── Upsert mst_employee_machine_mapping ──
            var existingMapping = await _db.MstEmployeeMachineMappings
                .FirstOrDefaultAsync(m => m.EmployeeId == emp.EmployeeId
                    && m.MachineId == dto.MachineId && m.IsActive == true);

            if (existingMapping != null)
            {
                // Update role if changed
                existingMapping.RoleCode = emp.RoleCode ?? existingMapping.RoleCode;
                existingMapping.RoleName = emp.RoleCode ?? existingMapping.RoleName;
                existingMapping.ModifiedBy = user?.UserCode;
                existingMapping.ModifiedOn = now;
            }
            else
            {
                // Insert new mapping
                _db.MstEmployeeMachineMappings.Add(new MstEmployeeMachineMapping
                {
                    EmployeeId = emp.EmployeeId,
                    EmployeeCode = employee.EmpCode,
                    EmployeeName = empFullName,
                    MachineId = dto.MachineId,
                    MachineCode = machineEntity?.MachineCode,
                    MachineName = machineEntity?.MachineName,
                    RoleCode = emp.RoleCode ?? "Operator",
                    RoleName = emp.RoleCode ?? "Operator",
                    SkillLevel = "Beginner",
                    IsPrimaryMachine = false,
                    IsAuthorized = true,
                    CreatedBy = user?.UserCode,
                    CreatedOn = now,
                    IsActive = true
                });
                newMappings++;
            }

            // Send notification email to allocated employee
            if (!string.IsNullOrWhiteSpace(employee.Email1))
            {
                try
                {
                    await _notification.SendEmailAsync(
                        employee.Email1,
                        $"Machine Allocation - {parentAlloc.JobNo}",
                        $"Dear {employee.FirstName},\n\nYou have been assigned to machine {parentAlloc.MachineName} for job {parentAlloc.JobNo} as {emp.RoleCode ?? "Operator"} ({emp.ShiftCode ?? "GENERAL"} shift).\n\nRegards,\nMinePress ERP");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send allocation email to {Email}", employee.Email1);
                }
            }
        }

        await _db.SaveChangesAsync();

        // Job timeline
        _db.TrnJobTimelines.Add(new TrnJobTimeline
        {
            JobId = dto.JobId,
            EventType = "PRODUCTION",
            EventCode = "MANPOWER_ASSIGNED",
            EventTitle = $"{count} employee(s) assigned to {parentAlloc.MachineName}",
            EventDescription = $"Manpower allocation for job {parentAlloc.JobNo} on machine {parentAlloc.MachineName}" +
                (newMappings > 0 ? $". {newMappings} new machine mapping(s) created." : ""),
            MachineId = dto.MachineId,
            CreatedBy = user?.UserId ?? 0,
            CreatedOn = now,
            IsActive = true
        });
        await _db.SaveChangesAsync();

        // Activity log
        if (user != null)
        {
            var entry = ActivityLogEntry.FromUser(user, "PRODUCTION", "MANPOWER_ASSIGN", $"Assigned {count} employee(s) to machine");
            entry.SubModule = "JOB_ALLOCATION";
            entry.EntityType = "TrnJobMachineManpowerAllocation";
            entry.EntityId = parentAlloc.AllocationId;
            entry.Description = $"Assigned {count} employee(s) to machine {parentAlloc.MachineName} for job {parentAlloc.JobNo}" +
                (newMappings > 0 ? $". Created {newMappings} new employee-machine mapping(s)." : "");
            await _activity.LogActivityAsync(entry);
        }

        return Ok(new { message = $"{count} employee(s) assigned", count, newMappings, allocationId = parentAlloc.AllocationId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SaveManpowerAllocation failed");
            await AuditExceptionAsync(ex, "ProductionController.SaveManpowerAllocation");
            return StatusCode(500, new { message = "An error occurred while saving manpower allocation. Please try again." });
        }
    }

    // ─── Get All Machine Allocations (jobs per machine) ────────────

    [HttpGet("machine-allocations")]
    public async Task<IActionResult> GetMachineAllocations()
    {
        var allocations = await _db.TrnJobMachineAllocations
            .Where(a => a.IsActive == true && a.AllocationStatus == "ALLOCATED")
            .Include(a => a.TrnJobMachineManpowerAllocations.Where(mp => mp.IsActive == true))
            .OrderByDescending(a => a.CreatedOn)
            .Select(a => new
            {
                a.AllocationId,
                a.JobId,
                a.JobNo,
                a.MachineId,
                a.MachineCode,
                a.MachineName,
                a.ProcessCode,
                a.ProcessName,
                a.PlannedQuantity,
                a.CompletedQuantity,
                a.AllocationStatus,
                a.PlannedStartTime,
                a.PlannedEndTime,
                a.CreatedOn,
                Employees = a.TrnJobMachineManpowerAllocations.Select(mp => new
                {
                    mp.EmployeeId,
                    mp.EmployeeCode,
                    mp.EmployeeName,
                    mp.RoleCode,
                    mp.ShiftCode
                })
            })
            .ToListAsync();

        return Ok(allocations);
    }

    // ─── Get Manpower for a Machine-Job ─────────────────────────

    [HttpGet("manpower")]
    public async Task<IActionResult> GetManpower([FromQuery] long machineId, [FromQuery] long jobId)
    {
        var list = await _db.TrnJobMachineManpowerAllocations
            .Where(m => m.MachineId == machineId && m.JobId == jobId && m.IsActive == true)
            .Select(m => new
            {
                m.ManpowerAllocationId,
                m.EmployeeId,
                m.EmployeeCode,
                m.EmployeeName,
                m.RoleCode,
                m.ShiftCode,
                m.AllocationStatus
            })
            .ToListAsync();

        return Ok(list);
    }

    // ─── Get Employees Mapped to a Machine ──────────────────────

    [HttpGet("machine-employees")]
    public async Task<IActionResult> GetMachineEmployees([FromQuery] long machineId)
    {
        var mappings = await _db.MstEmployeeMachineMappings
            .Where(m => m.MachineId == machineId && m.IsActive == true && m.IsAuthorized == true)
            .OrderByDescending(m => m.IsPrimaryMachine)
            .ThenBy(m => m.EmployeeName)
            .Select(m => new
            {
                m.MappingId,
                m.EmployeeId,
                m.EmployeeCode,
                m.EmployeeName,
                m.RoleCode,
                m.RoleName,
                m.SkillLevel,
                m.IsPrimaryMachine,
                m.IsAuthorized,
                m.ExperienceYears
            })
            .ToListAsync();

        return Ok(mappings);
    }

    // ─── Remove Manpower (from machine mapping + job allocation) ─

    [HttpPost("remove-manpower")]
    public async Task<IActionResult> RemoveManpower([FromBody] RemoveManpowerDto dto)
    {
        var user = _httpCtx.HttpContext?.Session.GetCurrentUser();
        var now = DateTime.Now;

        var employee = await _db.MstEmployees.FindAsync(dto.EmployeeId);
        if (employee == null)
            return BadRequest(new { message = "Employee not found" });

        var machine = await _db.MstMachines.FindAsync(dto.MachineId);
        var empFullName = (employee.FirstName ?? "") + " " + (employee.LastName ?? "");

        // 1. Deactivate from mst_employee_machine_mapping
        var mapping = await _db.MstEmployeeMachineMappings
            .FirstOrDefaultAsync(m => m.EmployeeId == dto.EmployeeId
                && m.MachineId == dto.MachineId && m.IsActive == true);

        if (mapping != null)
        {
            mapping.IsActive = false;
            mapping.ModifiedBy = user?.UserCode;
            mapping.ModifiedOn = now;
        }

        // 2. Deactivate from trn_job_machine_manpower_allocation (all active entries for this employee on this machine)
        var manpowerAllocs = await _db.TrnJobMachineManpowerAllocations
            .Where(m => m.EmployeeId == dto.EmployeeId
                && m.MachineId == dto.MachineId && m.IsActive == true)
            .ToListAsync();

        foreach (var mp in manpowerAllocs)
        {
            mp.IsActive = false;
            mp.AllocationStatus = "REMOVED";
            mp.ModifiedBy = user?.UserCode;
            mp.ModifiedOn = now;
        }

        // 3. Job timeline entries for each affected job
        var affectedJobIds = manpowerAllocs.Select(m => m.JobId).Distinct().ToList();
        foreach (var jobId in affectedJobIds)
        {
            var jobNo = manpowerAllocs.FirstOrDefault(m => m.JobId == jobId)?.JobNo;
            _db.TrnJobTimelines.Add(new TrnJobTimeline
            {
                JobId = jobId,
                EventType = "PRODUCTION",
                EventCode = "MANPOWER_REMOVED",
                EventTitle = $"{empFullName} removed from {machine?.MachineName ?? "machine"}",
                EventDescription = $"Employee {empFullName} ({employee.EmpCode}) removed from machine {machine?.MachineName} for job {jobNo}",
                MachineId = dto.MachineId,
                CreatedBy = user?.UserId ?? 0,
                CreatedOn = now,
                IsActive = true
            });
        }

        await _db.SaveChangesAsync();

        // 4. Send email notification to removed employee
        if (!string.IsNullOrWhiteSpace(employee.Email1))
        {
            try
            {
                await _notification.SendEmailAsync(
                    employee.Email1,
                    $"Machine Assignment Removed - {machine?.MachineName}",
                    $"Dear {employee.FirstName},\n\nYou have been removed from machine {machine?.MachineName} ({machine?.MachineCode}).\n\nRegards,\nMinePress ERP");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send manpower removal email to {Email}", employee.Email1);
            }
        }

        // 5. Activity log
        if (user != null)
        {
            var entry = ActivityLogEntry.FromUser(user, "PRODUCTION", "MANPOWER_REMOVE",
                $"Removed {empFullName} from machine {machine?.MachineName}");
            entry.SubModule = "JOB_ALLOCATION";
            entry.EntityType = "MstEmployeeMachineMapping";
            entry.EntityId = mapping?.MappingId ?? 0;
            entry.Description = $"Removed {empFullName} ({employee.EmpCode}) from machine {machine?.MachineName}. " +
                $"Affected {manpowerAllocs.Count} job allocation(s).";
            await _activity.LogActivityAsync(entry);
        }

        return Ok(new
        {
            message = $"{empFullName} removed from {machine?.MachineName ?? "machine"}",
            removedMappings = mapping != null ? 1 : 0,
            removedAllocations = manpowerAllocs.Count,
            affectedJobs = affectedJobIds.Count
        });
    }

    // ─── Check Manpower Duplicate Across All Machines ─────────────

    [HttpGet("check-manpower-duplicate")]
    public async Task<IActionResult> CheckManpowerDuplicate([FromQuery] long employeeId, [FromQuery] long machineId)
    {
        // Check if this employee is mapped to the SAME machine already
        var sameMapping = await _db.MstEmployeeMachineMappings
            .FirstOrDefaultAsync(m => m.EmployeeId == employeeId
                && m.MachineId == machineId && m.IsActive == true);

        if (sameMapping != null)
        {
            return Ok(new
            {
                isDuplicate = true,
                sameMachine = true,
                machineName = sameMapping.MachineName,
                roleName = sameMapping.RoleName,
                message = $"{sameMapping.EmployeeName} is already assigned to {sameMapping.MachineName} as {sameMapping.RoleName}"
            });
        }

        // Check if this employee is mapped to ANY other machine
        var otherMapping = await _db.MstEmployeeMachineMappings
            .FirstOrDefaultAsync(m => m.EmployeeId == employeeId
                && m.MachineId != machineId && m.IsActive == true);

        if (otherMapping != null)
        {
            return Ok(new
            {
                isDuplicate = true,
                sameMachine = false,
                machineName = otherMapping.MachineName,
                roleName = otherMapping.RoleName,
                message = $"{otherMapping.EmployeeName} is already assigned to {otherMapping.MachineName} as {otherMapping.RoleName}"
            });
        }

        return Ok(new { isDuplicate = false });
    }

    // ─── Move Manpower Between Machines ─────────────────────────

    [HttpPost("move-manpower")]
    public async Task<IActionResult> MoveManpower([FromBody] MoveManpowerDto dto)
    {
        var user = _httpCtx.HttpContext?.Session.GetCurrentUser();
        var now = DateTime.Now;

        if (dto.FromMachineId == dto.ToMachineId)
            return BadRequest(new { message = "Source and target machines are the same" });

        var employee = await _db.MstEmployees.FindAsync(dto.EmployeeId);
        if (employee == null)
            return BadRequest(new { message = "Employee not found" });

        var fromMachine = await _db.MstMachines.FindAsync(dto.FromMachineId);
        var toMachine = await _db.MstMachines.FindAsync(dto.ToMachineId);
        if (toMachine == null)
            return BadRequest(new { message = "Target machine not found" });

        // Check if target machine is under maintenance
        var maintDue = await _db.MstMachineMaintenances
            .Where(mt => mt.MachineId == dto.ToMachineId && mt.IsActive == true && mt.NextDueDate != null)
            .OrderBy(mt => mt.NextDueDate)
            .FirstOrDefaultAsync();

        if (maintDue?.NextDueDate != null
            && maintDue.NextDueDate.Value.ToDateTime(TimeOnly.MinValue) <= DateTime.Now.AddDays(3))
        {
            return BadRequest(new
            {
                message = $"{toMachine.MachineName} is under maintenance and cannot accept manpower assignments",
                maintenanceDue = true
            });
        }

        var empFullName = (employee.FirstName ?? "") + " " + (employee.LastName ?? "");

        // 1. Deactivate mapping on source machine
        var oldMapping = await _db.MstEmployeeMachineMappings
            .FirstOrDefaultAsync(m => m.EmployeeId == dto.EmployeeId
                && m.MachineId == dto.FromMachineId && m.IsActive == true);

        if (oldMapping != null)
        {
            oldMapping.IsActive = false;
            oldMapping.ModifiedBy = user?.UserCode;
            oldMapping.ModifiedOn = now;
        }

        // 2. Deactivate manpower allocations on source machine
        var oldManpower = await _db.TrnJobMachineManpowerAllocations
            .Where(m => m.EmployeeId == dto.EmployeeId
                && m.MachineId == dto.FromMachineId && m.IsActive == true)
            .ToListAsync();

        foreach (var mp in oldManpower)
        {
            mp.IsActive = false;
            mp.AllocationStatus = "MOVED";
            mp.ModifiedBy = user?.UserCode;
            mp.ModifiedOn = now;
        }

        // 3. Create new mapping on target machine (or reactivate)
        var existingTargetMapping = await _db.MstEmployeeMachineMappings
            .FirstOrDefaultAsync(m => m.EmployeeId == dto.EmployeeId
                && m.MachineId == dto.ToMachineId);

        if (existingTargetMapping != null)
        {
            existingTargetMapping.IsActive = true;
            existingTargetMapping.IsAuthorized = true;
            existingTargetMapping.RoleCode = oldMapping?.RoleCode ?? "Operator";
            existingTargetMapping.RoleName = oldMapping?.RoleName ?? "Operator";
            existingTargetMapping.ModifiedBy = user?.UserCode;
            existingTargetMapping.ModifiedOn = now;
        }
        else
        {
            _db.MstEmployeeMachineMappings.Add(new MstEmployeeMachineMapping
            {
                EmployeeId = dto.EmployeeId,
                EmployeeCode = employee.EmpCode,
                EmployeeName = empFullName,
                MachineId = dto.ToMachineId,
                MachineCode = toMachine.MachineCode,
                MachineName = toMachine.MachineName,
                RoleCode = oldMapping?.RoleCode ?? "Operator",
                RoleName = oldMapping?.RoleName ?? "Operator",
                SkillLevel = oldMapping?.SkillLevel ?? "Beginner",
                IsPrimaryMachine = false,
                IsAuthorized = true,
                CreatedBy = user?.UserCode,
                CreatedOn = now,
                IsActive = true
            });
        }

        // 4. Auto-assign to active jobs on target machine
        var targetAllocations = await _db.TrnJobMachineAllocations
            .Where(a => a.MachineId == dto.ToMachineId && a.IsActive == true && a.AllocationStatus == "ALLOCATED")
            .ToListAsync();

        int jobsAssigned = 0;
        foreach (var alloc in targetAllocations)
        {
            _db.TrnJobMachineManpowerAllocations.Add(new TrnJobMachineManpowerAllocation
            {
                AllocationId = alloc.AllocationId,
                JobId = alloc.JobId,
                JobNo = alloc.JobNo,
                MachineId = dto.ToMachineId,
                EmployeeId = dto.EmployeeId,
                EmployeeCode = employee.EmpCode,
                EmployeeName = empFullName,
                RoleCode = oldMapping?.RoleCode ?? "Operator",
                RoleName = oldMapping?.RoleName ?? "Operator",
                ShiftCode = "GENERAL",
                AllocationStatus = "ASSIGNED",
                CreatedBy = user?.UserCode,
                CreatedOn = now,
                IsActive = true
            });
            jobsAssigned++;
        }

        // 5. Timeline entries
        var affectedJobIds = oldManpower.Select(m => m.JobId).Distinct().ToList();
        foreach (var jobId in affectedJobIds)
        {
            var jobNo = oldManpower.FirstOrDefault(m => m.JobId == jobId)?.JobNo;
            _db.TrnJobTimelines.Add(new TrnJobTimeline
            {
                JobId = jobId,
                EventType = "PRODUCTION",
                EventCode = "MANPOWER_MOVED",
                EventTitle = $"{empFullName} moved from {fromMachine?.MachineName ?? "—"} to {toMachine.MachineName}",
                EventDescription = $"Employee {empFullName} ({employee.EmpCode}) moved from {fromMachine?.MachineName} to {toMachine.MachineName} for job {jobNo}",
                MachineId = dto.ToMachineId,
                CreatedBy = user?.UserId ?? 0,
                CreatedOn = now,
                IsActive = true
            });
        }

        await _db.SaveChangesAsync();

        // 6. Email notification to employee
        if (!string.IsNullOrWhiteSpace(employee.Email1))
        {
            try
            {
                await _notification.SendEmailAsync(
                    employee.Email1,
                    $"Machine Assignment Changed - {fromMachine?.MachineName} → {toMachine.MachineName}",
                    $"Dear {employee.FirstName},\n\nYour machine assignment has been changed.\n\n" +
                    $"Previous: {fromMachine?.MachineName} ({fromMachine?.MachineCode})\n" +
                    $"New: {toMachine.MachineName} ({toMachine.MachineCode})\n\n" +
                    $"You have been auto-assigned to {jobsAssigned} active job(s) on {toMachine.MachineName}.\n\n" +
                    $"Regards,\nMinePress ERP");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send manpower move email to {Email}", employee.Email1);
            }
        }

        // 7. Activity log
        if (user != null)
        {
            var entry = ActivityLogEntry.FromUser(user, "PRODUCTION", "MANPOWER_MOVE",
                $"Moved {empFullName} from {fromMachine?.MachineName} to {toMachine.MachineName}");
            entry.SubModule = "JOB_ALLOCATION";
            entry.EntityType = "MstEmployeeMachineMapping";
            entry.EntityId = dto.EmployeeId;
            entry.Description = $"Moved {empFullName} ({employee.EmpCode}) from {fromMachine?.MachineName} to {toMachine.MachineName}. " +
                $"Removed from {oldManpower.Count} job(s), auto-assigned to {jobsAssigned} job(s) on target machine.";
            await _activity.LogActivityAsync(entry);
        }

        return Ok(new
        {
            message = $"{empFullName} moved from {fromMachine?.MachineName ?? "—"} to {toMachine.MachineName}",
            removedAllocations = oldManpower.Count,
            newJobsAssigned = jobsAssigned,
            affectedJobs = affectedJobIds.Count
        });
    }

    // ─── Machine Breakdown CRUD ─────────────────────────────────

    [HttpGet("breakdowns")]
    public async Task<IActionResult> GetBreakdowns([FromQuery] long? machineId, [FromQuery] string? status)
    {
        var query = _db.TrnMachineBreakdowns
            .Include(b => b.Machine)
            .Where(b => b.IsActive == true)
            .AsQueryable();

        if (machineId.HasValue)
            query = query.Where(b => b.MachineId == machineId);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(b => b.BreakdownStatus == status);

        var list = await query
            .OrderByDescending(b => b.BreakdownStartTime)
            .Select(b => new
            {
                b.BreakdownId,
                b.MachineId,
                MachineName = b.Machine.MachineName,
                MachineCode = b.Machine.MachineCode,
                b.FaultCode,
                b.FaultDescription,
                b.FaultCategory,
                b.SeverityLevel,
                b.BreakdownStartTime,
                b.BreakdownEndTime,
                b.DowntimeMinutes,
                b.BreakdownStatus,
                b.ReportedBy,
                b.TechnicianId,
                b.TechnicianName,
                b.RootCause,
                b.CorrectiveAction,
                b.PreventiveAction,
                b.SparePartsUsed,
                b.RepairCost,
                b.ResolvedDate,
                b.Remarks,
                b.CreatedOn,
                b.CreatedBy
            })
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("breakdowns/{id:long}")]
    public async Task<IActionResult> GetBreakdown(long id)
    {
        var b = await _db.TrnMachineBreakdowns
            .Include(x => x.Machine)
            .FirstOrDefaultAsync(x => x.BreakdownId == id && x.IsActive == true);

        if (b == null) return NotFound();

        return Ok(new
        {
            b.BreakdownId,
            b.MachineId,
            MachineName = b.Machine.MachineName,
            MachineCode = b.Machine.MachineCode,
            b.FaultCode,
            b.FaultDescription,
            b.FaultCategory,
            b.SeverityLevel,
            b.BreakdownStartTime,
            b.BreakdownEndTime,
            b.DowntimeMinutes,
            b.BreakdownStatus,
            b.ReportedBy,
            b.TechnicianId,
            b.TechnicianName,
            b.RootCause,
            b.CorrectiveAction,
            b.PreventiveAction,
            b.SparePartsUsed,
            b.RepairCost,
            b.ResolvedDate,
            b.Remarks,
            b.CreatedOn,
            b.CreatedBy
        });
    }

    [HttpPost("breakdowns")]
    public async Task<IActionResult> CreateBreakdown([FromBody] BreakdownDto dto)
    {
        var user = _httpCtx.HttpContext?.Session.GetCurrentUser();

        var machine = await _db.MstMachines.FindAsync(dto.MachineId);
        if (machine == null) return BadRequest(new { message = "Machine not found" });

        var record = new TrnMachineBreakdown
        {
            MachineId = dto.MachineId,
            FaultCode = dto.FaultCode,
            FaultDescription = dto.FaultDescription,
            FaultCategory = dto.FaultCategory,
            SeverityLevel = dto.SeverityLevel,
            BreakdownStartTime = dto.BreakdownStartTime,
            BreakdownEndTime = dto.BreakdownEndTime,
            DowntimeMinutes = dto.DowntimeMinutes,
            BreakdownStatus = dto.BreakdownStatus ?? "Open",
            ReportedBy = dto.ReportedBy,
            TechnicianId = dto.TechnicianId,
            TechnicianName = dto.TechnicianName,
            RootCause = dto.RootCause,
            CorrectiveAction = dto.CorrectiveAction,
            PreventiveAction = dto.PreventiveAction,
            SparePartsUsed = dto.SparePartsUsed,
            RepairCost = dto.RepairCost,
            Remarks = dto.Remarks,
            CreatedOn = DateTime.Now,
            CreatedBy = user?.UserCode,
            IsActive = true
        };

        _db.TrnMachineBreakdowns.Add(record);
        await _db.SaveChangesAsync();

        // Notify employees mapped to this machine about the breakdown
        try
        {
            var mappedEmps = await _db.MstEmployeeMachineMappings
                .Include(m => m.Employee)
                .Where(m => m.MachineId == dto.MachineId && m.IsActive == true && m.IsAuthorized == true)
                .ToListAsync();

            foreach (var mapped in mappedEmps)
            {
                var email = mapped.Employee?.Email1;
                if (string.IsNullOrWhiteSpace(email)) continue;
                try
                {
                    await _notification.SendEmailAsync(
                        email,
                        $"⚠ Machine Breakdown - {machine.MachineName}",
                        $"Dear {mapped.EmployeeName},\n\nA breakdown has been reported for machine {machine.MachineName} ({machine.MachineCode}).\n\nFault Category: {dto.FaultCategory ?? "N/A"}\nSeverity: {dto.SeverityLevel ?? "N/A"}\nFault Code: {dto.FaultCode ?? "N/A"}\nDescription: {dto.FaultDescription ?? "N/A"}\nStart Time: {dto.BreakdownStartTime:dd-MMM-yyyy HH:mm}\n\nPlease take immediate action.\n\nRegards,\nMinePress ERP");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send breakdown email to {Email}", email);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send breakdown notification emails");
        }

        if (user != null)
        {
            var entry = ActivityLogEntry.FromUser(user, "PRODUCTION", "BREAKDOWN_CREATE",
                $"Reported breakdown for {machine.MachineName}");
            entry.SubModule = "MACHINE_BREAKDOWN";
            entry.EntityType = "TrnMachineBreakdown";
            entry.EntityId = record.BreakdownId;
            entry.Description = $"Breakdown reported: {dto.FaultCategory} — {dto.SeverityLevel} on {machine.MachineName}";
            await _activity.LogActivityAsync(entry);
        }

        return Ok(new { record.BreakdownId, message = "Breakdown reported" });
    }

    [HttpPut("breakdowns/{id:long}")]
    public async Task<IActionResult> UpdateBreakdown(long id, [FromBody] BreakdownDto dto)
    {
        var user = _httpCtx.HttpContext?.Session.GetCurrentUser();
        var record = await _db.TrnMachineBreakdowns.FindAsync(id);
        if (record == null) return NotFound();

        record.FaultCode = dto.FaultCode;
        record.FaultDescription = dto.FaultDescription;
        record.FaultCategory = dto.FaultCategory;
        record.SeverityLevel = dto.SeverityLevel;
        record.BreakdownStartTime = dto.BreakdownStartTime;
        record.BreakdownEndTime = dto.BreakdownEndTime;
        record.DowntimeMinutes = dto.DowntimeMinutes;
        record.BreakdownStatus = dto.BreakdownStatus;
        record.ReportedBy = dto.ReportedBy;
        record.TechnicianId = dto.TechnicianId;
        record.TechnicianName = dto.TechnicianName;
        record.RootCause = dto.RootCause;
        record.CorrectiveAction = dto.CorrectiveAction;
        record.PreventiveAction = dto.PreventiveAction;
        record.SparePartsUsed = dto.SparePartsUsed;
        record.RepairCost = dto.RepairCost;
        record.Remarks = dto.Remarks;

        await _db.SaveChangesAsync();

        if (user != null)
        {
            var machine = await _db.MstMachines.FindAsync(record.MachineId);
            var entry = ActivityLogEntry.FromUser(user, "PRODUCTION", "BREAKDOWN_UPDATE",
                $"Updated breakdown #{id} for {machine?.MachineName}");
            entry.SubModule = "MACHINE_BREAKDOWN";
            entry.EntityType = "TrnMachineBreakdown";
            entry.EntityId = id;
            await _activity.LogActivityAsync(entry);
        }

        return Ok(new { message = "Breakdown updated" });
    }

    [HttpPost("breakdowns/{id:long}/resolve")]
    public async Task<IActionResult> ResolveBreakdown(long id)
    {
        var user = _httpCtx.HttpContext?.Session.GetCurrentUser();
        var record = await _db.TrnMachineBreakdowns.FindAsync(id);
        if (record == null) return NotFound();

        record.BreakdownStatus = "Resolved";
        record.ResolvedDate = DateTime.Now;
        record.BreakdownEndTime ??= DateTime.Now;

        if (record.BreakdownEndTime.HasValue)
        {
            record.DowntimeMinutes = (decimal)(record.BreakdownEndTime.Value - record.BreakdownStartTime).TotalMinutes;
        }

        await _db.SaveChangesAsync();

        // Notify employees mapped to this machine about breakdown resolution
        try
        {
            var machine = await _db.MstMachines.FindAsync(record.MachineId);
            var mappedEmps = await _db.MstEmployeeMachineMappings
                .Include(m => m.Employee)
                .Where(m => m.MachineId == record.MachineId && m.IsActive == true && m.IsAuthorized == true)
                .ToListAsync();

            foreach (var mapped in mappedEmps)
            {
                var email = mapped.Employee?.Email1;
                if (string.IsNullOrWhiteSpace(email)) continue;
                try
                {
                    await _notification.SendEmailAsync(
                        email,
                        $"✅ Breakdown Resolved - {machine?.MachineName}",
                        $"Dear {mapped.EmployeeName},\n\nThe breakdown for machine {machine?.MachineName} ({machine?.MachineCode}) has been resolved.\n\nTotal Downtime: {record.DowntimeMinutes:N0} minutes\nResolved At: {record.ResolvedDate:dd-MMM-yyyy HH:mm}\n\nThe machine is now available for production.\n\nRegards,\nMinePress ERP");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send breakdown resolve email to {Email}", email);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send breakdown resolve notification emails");
        }

        if (user != null)
        {
            var machine = await _db.MstMachines.FindAsync(record.MachineId);
            var entry = ActivityLogEntry.FromUser(user, "PRODUCTION", "BREAKDOWN_RESOLVE",
                $"Resolved breakdown #{id} for {machine?.MachineName}");
            entry.SubModule = "MACHINE_BREAKDOWN";
            entry.EntityType = "TrnMachineBreakdown";
            entry.EntityId = id;
            entry.Description = $"Breakdown resolved. Downtime: {record.DowntimeMinutes:N0} min";
            await _activity.LogActivityAsync(entry);
        }

        return Ok(new { message = "Breakdown resolved", record.DowntimeMinutes });
    }

    [HttpPost("breakdowns/{id:long}/close")]
    public async Task<IActionResult> CloseBreakdown(long id)
    {
        var user = _httpCtx.HttpContext?.Session.GetCurrentUser();
        var record = await _db.TrnMachineBreakdowns.FindAsync(id);
        if (record == null) return NotFound();

        record.BreakdownStatus = "Closed";
        record.ResolvedDate ??= DateTime.Now;
        record.BreakdownEndTime ??= DateTime.Now;

        if (record.BreakdownEndTime.HasValue && record.DowntimeMinutes == null)
        {
            record.DowntimeMinutes = (decimal)(record.BreakdownEndTime.Value - record.BreakdownStartTime).TotalMinutes;
        }

        await _db.SaveChangesAsync();

        // Notify employees mapped to this machine about breakdown closure
        try
        {
            var machine = await _db.MstMachines.FindAsync(record.MachineId);
            var mappedEmps = await _db.MstEmployeeMachineMappings
                .Include(m => m.Employee)
                .Where(m => m.MachineId == record.MachineId && m.IsActive == true && m.IsAuthorized == true)
                .ToListAsync();

            foreach (var mapped in mappedEmps)
            {
                var email = mapped.Employee?.Email1;
                if (string.IsNullOrWhiteSpace(email)) continue;
                try
                {
                    await _notification.SendEmailAsync(
                        email,
                        $"Breakdown Closed - {machine?.MachineName}",
                        $"Dear {mapped.EmployeeName},\n\nThe breakdown for machine {machine?.MachineName} ({machine?.MachineCode}) has been closed.\n\nTotal Downtime: {record.DowntimeMinutes:N0} minutes\nClosed At: {DateTime.Now:dd-MMM-yyyy HH:mm}\n\nRegards,\nMinePress ERP");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send breakdown close email to {Email}", email);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send breakdown close notification emails");
        }

        if (user != null)
        {
            var machine = await _db.MstMachines.FindAsync(record.MachineId);
            var entry = ActivityLogEntry.FromUser(user, "PRODUCTION", "BREAKDOWN_CLOSE",
                $"Closed breakdown #{id} for {machine?.MachineName}");
            entry.SubModule = "MACHINE_BREAKDOWN";
            entry.EntityType = "TrnMachineBreakdown";
            entry.EntityId = id;
            await _activity.LogActivityAsync(entry);
        }

        return Ok(new { message = "Breakdown closed" });
    }

    [HttpPost("breakdowns/{id:long}/delete")]
    public async Task<IActionResult> DeleteBreakdown(long id)
    {
        var record = await _db.TrnMachineBreakdowns.FindAsync(id);
        if (record == null) return NotFound();

        record.IsActive = false;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Breakdown deleted" });
    }

    // ─── Dashboard Stats ────────────────────────────────────────

    [HttpGet("dashboard-stats")]
    public async Task<IActionResult> GetDashboardStats()
    {
        var today = DateTime.Today;
        var weekAgo = today.AddDays(-7);
        var monthAgo = today.AddDays(-30);

        // ── Machines ──
        var totalMachines = await _db.MstMachines.CountAsync(m => m.IsActive == true);

        var runningMachineIds = await _db.TrnJobMachineAllocations
            .Where(a => a.IsActive == true && a.AllocationStatus == "ALLOCATED")
            .Select(a => a.MachineId)
            .Distinct()
            .ToListAsync();

        var breakdownMachineIds = await _db.TrnMachineBreakdowns
            .Where(b => b.IsActive == true && b.BreakdownStatus != "Resolved" && b.BreakdownStatus != "Closed")
            .Select(b => b.MachineId)
            .Distinct()
            .ToListAsync();

        var maintenanceDueMachineIds = await _db.MstMachineMaintenances
            .Where(m => m.IsActive == true && m.NextDueDate != null && m.NextDueDate <= DateOnly.FromDateTime(today))
            .Select(m => m.MachineId ?? 0)
            .Where(id => id > 0)
            .Distinct()
            .ToListAsync();

        var runningCount = runningMachineIds.Count;
        var breakdownCount = breakdownMachineIds.Distinct().Count();
        var maintenanceCount = maintenanceDueMachineIds.Distinct().Count();
        var idleCount = totalMachines - runningCount - breakdownCount;
        if (idleCount < 0) idleCount = 0;

        // ── Jobs ──
        var totalActiveJobs = await _db.TrnJobs.CountAsync(j => j.StatusCode != "CLOSED" && j.StatusCode != "CANCELLED");
        var todayAllocations = await _db.TrnJobMachineAllocations
            .CountAsync(a => a.IsActive == true && a.CreatedOn != null && a.CreatedOn.Value.Date == today);
        var unallocatedJobs = await _db.TrnJobs
            .Where(j => j.StatusCode != "CLOSED" && j.StatusCode != "CANCELLED"
                && !_db.TrnJobMachineAllocations.Any(a => a.JobId == j.JobId && a.IsActive == true))
            .CountAsync();
        var urgentJobs = await _db.TrnJobs
            .CountAsync(j => j.StatusCode != "CLOSED" && j.StatusCode != "CANCELLED"
                && j.Priority != null && (j.Priority == "Urgent" || j.Priority == "Critical"));

        // Jobs by status
        var jobsByStatus = await _db.TrnJobs
            .Where(j => j.StatusCode != null)
            .GroupBy(j => j.StatusCode!)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        // ── Manpower ──
        var totalManpower = await _db.MstEmployeeMachineMappings
            .CountAsync(m => m.IsActive == true && m.IsAuthorized == true);
        var manpowerAssigned = await _db.TrnJobMachineManpowerAllocations
            .CountAsync(m => m.IsActive == true);

        // ── Breakdowns ──
        var openBreakdowns = await _db.TrnMachineBreakdowns
            .CountAsync(b => b.IsActive == true && (b.BreakdownStatus == "Open" || b.BreakdownStatus == "Assigned" || b.BreakdownStatus == "In Progress"));
        var resolvedThisMonth = await _db.TrnMachineBreakdowns
            .CountAsync(b => b.IsActive == true && (b.BreakdownStatus == "Resolved" || b.BreakdownStatus == "Closed")
                && b.ResolvedDate != null && b.ResolvedDate.Value >= monthAgo);
        var avgDowntime = await _db.TrnMachineBreakdowns
            .Where(b => b.IsActive == true && b.DowntimeMinutes != null && b.ResolvedDate != null && b.ResolvedDate.Value >= monthAgo)
            .AverageAsync(b => (double?)b.DowntimeMinutes) ?? 0;
        var totalRepairCost = await _db.TrnMachineBreakdowns
            .Where(b => b.IsActive == true && b.RepairCost != null && b.CreatedOn != null && b.CreatedOn.Value >= monthAgo)
            .SumAsync(b => (decimal?)b.RepairCost) ?? 0;

        // Breakdown by category
        var breakdownByCategory = await _db.TrnMachineBreakdowns
            .Where(b => b.IsActive == true && b.FaultCategory != null)
            .GroupBy(b => b.FaultCategory!)
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .ToListAsync();

        // Breakdown by severity
        var breakdownBySeverity = await _db.TrnMachineBreakdowns
            .Where(b => b.IsActive == true && b.SeverityLevel != null
                && b.BreakdownStatus != "Resolved" && b.BreakdownStatus != "Closed")
            .GroupBy(b => b.SeverityLevel!)
            .Select(g => new { Severity = g.Key, Count = g.Count() })
            .ToListAsync();

        // ── Maintenance ──
        var pendingMaintenance = await _db.MstMachineMaintenances
            .CountAsync(m => m.IsActive == true && m.NextDueDate != null && m.NextDueDate <= DateOnly.FromDateTime(today.AddDays(7)));
        var completedMaintenanceMonth = await _db.MstMachineMaintenances
            .CountAsync(m => m.IsActive == true && m.CompletionDate != null && m.CompletionDate.Value >= monthAgo);
        var totalMaintenanceCost = await _db.MstMachineMaintenances
            .Where(m => m.IsActive == true && m.EstimatedCost != null && m.CompletionDate != null && m.CompletionDate.Value >= monthAgo)
            .SumAsync(m => (decimal?)m.EstimatedCost) ?? 0;

        // ── Recent Breakdowns (last 5) ──
        var recentBreakdowns = await _db.TrnMachineBreakdowns
            .Where(b => b.IsActive == true)
            .OrderByDescending(b => b.CreatedOn)
            .Take(5)
            .Select(b => new
            {
                b.BreakdownId,
                b.Machine.MachineName,
                b.FaultCategory,
                b.SeverityLevel,
                b.BreakdownStatus,
                b.DowntimeMinutes,
                b.CreatedOn
            })
            .ToListAsync();

        // ── Recent Allocations (last 5) ──
        var recentAllocations = await _db.TrnJobMachineAllocations
            .Where(a => a.IsActive == true)
            .OrderByDescending(a => a.CreatedOn)
            .Take(5)
            .Select(a => new
            {
                a.JobNo,
                a.MachineName,
                a.AllocationStatus,
                a.CreatedOn
            })
            .ToListAsync();

        // ── Upcoming Deliveries (next 7 days) ──
        var weekFromNow = DateOnly.FromDateTime(today.AddDays(7));
        var todayDateOnly = DateOnly.FromDateTime(today);
        var upcomingDeliveries = await _db.TrnJobs
            .Where(j => j.DeliveryDate != null && j.DeliveryDate >= todayDateOnly && j.DeliveryDate <= weekFromNow
                && j.StatusCode != "CLOSED" && j.StatusCode != "CANCELLED")
            .OrderBy(j => j.DeliveryDate)
            .Take(5)
            .Select(j => new
            {
                j.JobNo,
                j.ProductName,
                j.DeliveryDate,
                j.Priority,
                PartyName = j.Party != null ? j.Party.Name : null
            })
            .ToListAsync();

        // ── Machine Efficiency (7-day trend) ──
        var efficiencyPercent = totalMachines > 0 ? Math.Round((double)runningCount / totalMachines * 100, 1) : 0;

        return Ok(new
        {
            machines = new { total = totalMachines, running = runningCount, idle = idleCount, breakdown = breakdownCount, maintenance = maintenanceCount },
            jobs = new { totalActive = totalActiveJobs, todayAllocations, unallocated = unallocatedJobs, urgent = urgentJobs, byStatus = jobsByStatus },
            manpower = new { totalMapped = totalManpower, assigned = manpowerAssigned },
            breakdowns = new
            {
                open = openBreakdowns,
                resolvedThisMonth,
                avgDowntimeMinutes = Math.Round(avgDowntime, 0),
                totalRepairCost,
                byCategory = breakdownByCategory,
                bySeverity = breakdownBySeverity,
                recent = recentBreakdowns
            },
            maintenance = new { pendingDue = pendingMaintenance, completedThisMonth = completedMaintenanceMonth, totalCost = totalMaintenanceCost },
            recentAllocations,
            upcomingDeliveries,
            efficiencyPercent
        });
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var today = DateTime.Today;
        var totalMachines = await _db.MstMachines.CountAsync(m => m.IsActive == true);

        var activeAllocations = await _db.TrnJobMachineAllocations
            .Where(a => a.IsActive == true && a.AllocationStatus == "ALLOCATED")
            .Select(a => a.MachineId)
            .Distinct()
            .CountAsync();

        var manpowerAssigned = await _db.TrnJobMachineManpowerAllocations
            .Where(m => m.IsActive == true)
            .CountAsync();

        var todayJobs = await _db.TrnJobMachineAllocations
            .Where(a => a.IsActive == true && a.CreatedOn != null && a.CreatedOn.Value.Date == today)
            .CountAsync();

        return Ok(new
        {
            totalMachines,
            running = activeAllocations,
            idle = totalMachines - activeAllocations,
            manpowerAssigned,
            todayJobs
        });
    }

    // ─── Helpdesk TV Display Data ───────────────────────────────

    [HttpGet("tv-display")]
    public async Task<IActionResult> GetTvDisplayData()
    {
        var today = DateTime.Today;
        var todayDateOnly = DateOnly.FromDateTime(today);

        // ── Running Jobs (allocated to machines, actively being worked) ──
        var runningMachineJobs = await _db.TrnJobMachineAllocations
            .Where(a => a.IsActive == true && a.AllocationStatus == "ALLOCATED")
            .Include(a => a.Job).ThenInclude(j => j.Party)
            .Include(a => a.Job).ThenInclude(j => j.JobType)
            .Include(a => a.Machine)
            .Include(a => a.TrnJobMachineManpowerAllocations.Where(mp => mp.IsActive == true))
            .OrderBy(a => a.Job.DeliveryDate)
            .ThenByDescending(a => a.Job.AiPriorityScore)
            .Select(a => new TvRunningJobVm
            {
                CardSource = "MACHINE",
                WorkspaceTaskId = null,
                AllocationId = a.AllocationId,
                JobId = a.JobId,
                JobNo = a.JobNo,
                ProductName = a.Job.ProductName,
                Quantity = a.Job.Quantity,
                Priority = a.Job.Priority,
                DeliveryDate = a.Job.DeliveryDate,
                StatusCode = a.Job.StatusCode,
                CurrentStage = a.Job.CurrentStage,
                ProgressPercent = a.Job.ProgressPercent,
                AiPriorityScore = a.Job.AiPriorityScore,
                PartyName = a.Job.Party != null ? a.Job.Party.Name : null,
                JobTypeName = a.Job.JobType != null ? a.Job.JobType.Jobtypename : null,
                MachineId = a.MachineId,
                MachineName = a.MachineName,
                MachineCode = a.MachineCode,
                PlannedQuantity = a.PlannedQuantity,
                CompletedQuantity = a.CompletedQuantity,
                PlannedStartTime = a.PlannedStartTime,
                PlannedEndTime = a.PlannedEndTime,
                TaskStatus = (string?)null,
                TaskType = (string?)null,
                TaskStartedOn = (DateTime?)null,
                TaskUserName = (string?)null,
                TaskUserCode = (string?)null,
                TaskStage = (string?)null,
                TaskWorkType = (string?)null,
                PlateName = (string?)null,
                Workers = a.TrnJobMachineManpowerAllocations
                    .Select(mp => new TvWorkerVm { EmployeeName = mp.EmployeeName, RoleCode = mp.RoleCode })
                    .ToList()
            })
            .ToListAsync();

        var inProgressStatuses = new[] { WkTaskStatus.InProgress };
        var tvTaskProcessCodes = new[] { WkProcessCode.DesDtp, "DES_ART", "PRE_DES", WkProcessCode.PrePress, "PRE_CTP", WkProcessCode.Print, "PROC" };

        var workspaceRunningTaskRows = await (
            from t in _db.TrnWorkspaceTasks
            where t.IsArchived == false
                && t.TaskType == WkTaskType.Task
                && inProgressStatuses.Contains(t.TaskStatus)
                && t.ProcessCode != null
                && tvTaskProcessCodes.Contains(t.ProcessCode)
            join j in _db.TrnJobs on (t.JobId ?? (t.SourceTable == WkSourceTable.Job ? t.SourceId : -1)) equals j.JobId into jg
            from j in jg.DefaultIfEmpty()
            orderby (j != null ? j.DeliveryDate : null), (j != null ? j.AiPriorityScore : null) descending
            select new
            {
                t.WorkspaceTaskId,
                AllocationId = 0L,
                JobId = t.JobId ?? (j != null ? j.JobId : 0),
                JobNo = t.JobNo ?? t.SourceNo ?? (j != null ? j.JobNo : "—"),
                ProductName = j != null ? j.ProductName : (string?)null,
                Quantity = j != null ? j.Quantity : (int?)null,
                Priority = j != null ? j.Priority : null,
                DeliveryDate = j != null ? j.DeliveryDate : null,
                StatusCode = t.TaskStatus,
                CurrentStage = j != null ? j.CurrentStage : null,
                ProgressPercent = j != null ? j.ProgressPercent : 0,
                AiPriorityScore = j != null ? j.AiPriorityScore : null,
                PartyName = t.PartyName ?? (j != null && j.Party != null ? j.Party.Name : null),
                JobTypeName = j != null && j.JobType != null ? j.JobType.Jobtypename : null,
                MachineId = 0L,
                MachineName = (string?)null,
                MachineCode = (string?)null,
                PlannedQuantity = (decimal?)null,
                CompletedQuantity = (decimal?)null,
                PlannedStartTime = (DateTime?)null,
                PlannedEndTime = (DateTime?)null,
                TaskStatus = t.TaskStatus,
                TaskType = t.TaskType,
                TaskStartedOn = t.ModifiedOn ?? t.AssignedOn ?? t.CreatedOn,
                TaskUserName = t.User.Name,
                TaskUserCode = t.User.Usercode,
                ProcessCode = t.ProcessCode,
                TaskWorkType = t.Title,
                Description = t.Description
            }).ToListAsync();

        var workspaceRunningTasks = workspaceRunningTaskRows
            .Select(t => new TvRunningJobVm
            {
                CardSource = "WORKSPACE_TASK",
                WorkspaceTaskId = t.WorkspaceTaskId,
                AllocationId = t.AllocationId,
                JobId = t.JobId,
                JobNo = t.JobNo,
                ProductName = t.ProductName,
                Quantity = t.Quantity,
                Priority = t.Priority,
                DeliveryDate = t.DeliveryDate,
                StatusCode = t.StatusCode,
                CurrentStage = t.CurrentStage,
                ProgressPercent = t.ProgressPercent,
                AiPriorityScore = t.AiPriorityScore,
                PartyName = t.PartyName,
                JobTypeName = t.JobTypeName,
                MachineId = t.MachineId,
                MachineName = t.MachineName,
                MachineCode = t.MachineCode,
                PlannedQuantity = t.PlannedQuantity,
                CompletedQuantity = t.CompletedQuantity,
                PlannedStartTime = t.PlannedStartTime,
                PlannedEndTime = t.PlannedEndTime,
                TaskStatus = t.TaskStatus,
                TaskType = t.TaskType,
                TaskStartedOn = t.TaskStartedOn,
                TaskUserName = t.TaskUserName,
                TaskUserCode = t.TaskUserCode,
                TaskStage = MapTvTaskStage(t.ProcessCode),
                TaskWorkType = t.TaskWorkType,
                PlateName = ExtractPlateName(t.TaskWorkType, t.Description),
                Workers = []
            })
            .ToList();

        var runningJobs = runningMachineJobs.Concat(workspaceRunningTasks).ToList();

        // ── Jobs in Queue (not allocated to any machine) ──
        var allocatedJobIds = runningMachineJobs.Select(r => r.JobId).Distinct().ToList();

        var queueJobsRaw = await _db.TrnJobs
            .Where(j => j.StatusCode != "CLOSED" && j.StatusCode != "CANCELLED"
                      && j.StatusCode != "COMPLETED" && j.StatusCode != "DELIVERED"
                      && !allocatedJobIds.Contains(j.JobId))
            .Include(j => j.Party)
            .Include(j => j.JobType)
            .OrderBy(j => j.AiPriorityScore != null ? 0 : 1)
            .ThenByDescending(j => j.AiPriorityScore)
            .ThenBy(j => j.DeliveryDate)
            .ThenByDescending(j => j.Priority)
            .Take(50)
            .Select(j => new
            {
                j.JobId,
                j.JobNo,
                j.ProductName,
                j.Quantity,
                j.Priority,
                DeliveryDate = j.DeliveryDate,
                j.StatusCode,
                j.CurrentStage,
                j.ProgressPercent,
                j.AiPriorityScore,
                PartyName = j.Party != null ? j.Party.Name : null,
                JobTypeName = j.JobType != null ? j.JobType.Jobtypename : null
            })
            .ToListAsync();

        // ── Enrich queue jobs with previous / current / next process step from workspace tasks ──
        var queueJobIds = queueJobsRaw.Select(j => j.JobId).ToList();

        var queueJobTasks = await _db.TrnWorkspaceTasks
            .Where(t => t.JobId != null && queueJobIds.Contains(t.JobId.Value)
                        && !t.IsArchived
                        && t.TaskType == WkTaskType.Task)
            .OrderBy(t => t.SequenceNo ?? int.MaxValue)
            .ThenBy(t => t.CreatedOn)
            .Select(t => new
            {
                JobId = t.JobId!.Value,
                t.ProcessCode,
                t.Title,
                t.TaskStatus,
                t.SequenceNo
            })
            .ToListAsync();

        var queueTasksByJob = queueJobTasks
            .GroupBy(t => t.JobId)
            .ToDictionary(g => g.Key, g => g.OrderBy(t => t.SequenceNo ?? int.MaxValue).ThenBy(t => t.TaskStatus).ToList());

        var queueJobs = queueJobsRaw.Select(j =>
        {
            string? prevStep = null, currentStep = null, nextStep = null;

            if (queueTasksByJob.TryGetValue(j.JobId, out var tasks))
            {
                // Current = last IN_PROGRESS or first PENDING task
                var current = tasks.FirstOrDefault(t => t.TaskStatus == WkTaskStatus.InProgress)
                           ?? tasks.FirstOrDefault(t => t.TaskStatus == WkTaskStatus.Pending);

                if (current != null)
                {
                    currentStep = current.Title ?? current.ProcessCode;

                    var currentIdx = tasks.IndexOf(current);

                    // Previous = the last completed task before current
                    var prev = tasks.Take(currentIdx)
                        .LastOrDefault(t => t.TaskStatus == WkTaskStatus.Completed || t.TaskStatus == WkTaskStatus.Approved);
                    prevStep = prev != null ? (prev.Title ?? prev.ProcessCode) : null;

                    // Next = first non-completed task after current
                    var next = tasks.Skip(currentIdx + 1)
                        .FirstOrDefault(t => t.TaskStatus != WkTaskStatus.Completed && t.TaskStatus != WkTaskStatus.Approved
                                          && t.TaskStatus != WkTaskStatus.Cancelled);
                    nextStep = next != null ? (next.Title ?? next.ProcessCode) : null;
                }
                else
                {
                    // All tasks may be completed — last completed is current context
                    var lastDone = tasks.LastOrDefault(t => t.TaskStatus == WkTaskStatus.Completed || t.TaskStatus == WkTaskStatus.Approved);
                    if (lastDone != null)
                    {
                        currentStep = lastDone.Title ?? lastDone.ProcessCode;
                        var doneIdx = tasks.IndexOf(lastDone);
                        var prev = tasks.Take(doneIdx).LastOrDefault(t => t.TaskStatus == WkTaskStatus.Completed || t.TaskStatus == WkTaskStatus.Approved);
                        prevStep = prev != null ? (prev.Title ?? prev.ProcessCode) : null;
                    }
                }
            }

            return new
            {
                j.JobId,
                j.JobNo,
                j.ProductName,
                j.Quantity,
                j.Priority,
                DeliveryDate = j.DeliveryDate,
                j.StatusCode,
                j.CurrentStage,
                j.ProgressPercent,
                j.AiPriorityScore,
                j.PartyName,
                j.JobTypeName,
                PrevStep = prevStep,
                CurrentStep = currentStep,
                NextStep = nextStep
            };
        }).ToList();

        // ── Machines with status ──
        var allMachines = await _db.MstMachines
            .Where(m => m.IsActive == true)
            .Include(m => m.TrnMachineBreakdowns.Where(b => b.IsActive == true
                && b.BreakdownStatus != "Resolved" && b.BreakdownStatus != "Closed"))
            .OrderBy(m => m.AutoSelectPriority ?? 999)
            .ThenBy(m => m.MachineName)
            .ToListAsync();

        var machineAllocMap = runningMachineJobs
            .GroupBy(r => r.MachineId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var machines = allMachines.Select(m =>
        {
            var hasBreakdown = m.TrnMachineBreakdowns.Count > 0;
            var activeBreakdown = m.TrnMachineBreakdowns
                .OrderByDescending(b => b.BreakdownStartTime)
                .FirstOrDefault();
            var jobs = machineAllocMap.GetValueOrDefault(m.MachineId);
            var status = hasBreakdown ? "BREAKDOWN"
                       : jobs != null && jobs.Count > 0 ? "RUNNING"
                       : "IDLE";

            return new
            {
                m.MachineId,
                m.MachineCode,
                m.MachineName,
                m.MachineCategory,
                m.MaxSpeedPerHour,
                Status = status,
                JobCount = jobs?.Count ?? 0,
                CurrentJob = jobs?.FirstOrDefault()?.JobNo,
                CurrentProduct = jobs?.FirstOrDefault()?.ProductName,
                BreakdownFault = activeBreakdown?.FaultCategory,
                BreakdownSeverity = activeBreakdown?.SeverityLevel
            };
        }).ToList();

        // ── Workforce allocation (all users currently working across processes) ──
        var machineWorkforceRows = await _db.TrnJobMachineManpowerAllocations
            .Where(mp => mp.IsActive == true && mp.AllocationStatus == "ASSIGNED")
            .Select(mp => new
            {
                mp.EmployeeId,
                mp.EmployeeCode,
                mp.EmployeeName,
                mp.RoleCode,
                ProcessName = mp.Allocation.ProcessName ?? mp.Allocation.ProcessCode,
                StartedOn = mp.ActualStartTime ?? mp.PlannedStartTime ?? mp.CreatedOn,
                MachineName = mp.Machine.MachineName,
                Source = "MACHINE"
            })
            .ToListAsync();

        var workspaceWorkforceRows = await _db.TrnWorkspaceTasks
            .Where(t => t.IsArchived == false
                        && t.TaskType == WkTaskType.Task
                        && t.TaskStatus == WkTaskStatus.InProgress)
            .Select(t => new
            {
                EmployeeId = t.UserId,
                EmployeeCode = t.User.Usercode,
                EmployeeName = t.User.Name,
                RoleCode = (string?)null,
                ProcessName = t.Process != null ? t.Process.Processname : (t.ProcessCode ?? "Workspace"),
                StartedOn = (DateTime?)(t.ModifiedOn ?? t.AssignedOn ?? t.CreatedOn),
                MachineName = (string?)null,
                Source = "WORKSPACE"
            })
            .ToListAsync();

        var workforceRows = machineWorkforceRows.Concat(workspaceWorkforceRows).ToList();

        var workforceBase = workforceRows
            .GroupBy(w => new { w.EmployeeId, w.EmployeeCode, w.EmployeeName })
            .Select(g => new
            {
                g.Key.EmployeeId,
                g.Key.EmployeeCode,
                g.Key.EmployeeName,
                RoleCode = g.Select(x => x.RoleCode).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)),
                JobCount = g.Count(x => x.Source == "MACHINE"),
                Machines = g.Where(x => !string.IsNullOrWhiteSpace(x.MachineName))
                    .Select(x => x.MachineName!)
                    .Distinct()
                    .ToList(),
                Processes = g.Where(x => !string.IsNullOrWhiteSpace(x.ProcessName))
                    .GroupBy(x => new { x.ProcessName, x.StartedOn })
                    .Select(x => new
                    {
                        x.Key.ProcessName,
                        x.Key.StartedOn
                    })
                    .OrderBy(x => x.StartedOn)
                    .ToList(),
                WorkStartTime = g.Where(x => x.StartedOn.HasValue)
                    .OrderBy(x => x.StartedOn)
                    .Select(x => x.StartedOn)
                    .FirstOrDefault()
            })
            .OrderBy(w => w.EmployeeName)
            .ToList();

        var workforceEmployeeIds = workforceBase.Select(w => w.EmployeeId).Distinct().ToList();
        var leaveRows = await _db.HrLeaveRequests
            .Where(l => workforceEmployeeIds.Contains(l.EmployeeId)
                        && l.Status == "APPROVED"
                        && l.FromDate <= todayDateOnly
                        && l.ToDate >= todayDateOnly)
            .Join(_db.HrLeaveTypes,
                l => l.LeaveTypeId,
                t => t.LeaveTypeId,
                (l, t) => new
                {
                    l.EmployeeId,
                    l.FromDate,
                    LeaveType = t.LeaveName,
                    l.HalfDay
                })
            .ToListAsync();

        var leaveLookup = leaveRows
            .GroupBy(x => x.EmployeeId)
            .Select(g => g.OrderByDescending(x => x.FromDate).First())
            .ToDictionary(x => x.EmployeeId, x => new
            {
                x.LeaveType,
                x.HalfDay
            });

        var workforce = workforceBase
            .Select(w =>
            {
                var isOnLeave = leaveLookup.TryGetValue(w.EmployeeId, out var lv);
                var leaveLabel = isOnLeave
                    ? ((lv!.LeaveType ?? "Leave") + (lv.HalfDay == true ? " (Half Day)" : ""))
                    : null;

                return new
                {
                    w.EmployeeId,
                    w.EmployeeCode,
                    w.EmployeeName,
                    w.RoleCode,
                    w.JobCount,
                    w.Machines,
                    w.Processes,
                    w.WorkStartTime,
                    IsOnLeave = isOnLeave,
                    LeaveLabel = leaveLabel
                };
            })
            .ToList();

        // ── Summary stats ──
        var totalMachines = allMachines.Count;
        var runningCount = machines.Count(m => m.Status == "RUNNING");
        var idleCount = machines.Count(m => m.Status == "IDLE");
        var breakdownCount = machines.Count(m => m.Status == "BREAKDOWN");
        var overdueJobs = queueJobs.Count(j => j.DeliveryDate.HasValue && j.DeliveryDate.Value < todayDateOnly);

        return Ok(new
        {
            timestamp = DateTime.Now,
            stats = new
            {
                totalMachines,
                running = runningCount,
                idle = idleCount,
                breakdown = breakdownCount,
                activeJobs = runningJobs.Count,
                queuedJobs = queueJobs.Count,
                overdueJobs,
                totalWorkers = workforce.Count
            },
            runningJobs,
            queueJobs,
            machines,
            workforce
        });
    }

    // ─── Helpdesk TV Events (for speech announcements) ──────────

    /// <summary>
    /// Returns recent activity events since a given timestamp for the TV speech system.
    /// Covers: JOB, ENQUIRY, QUOTATION, PAYMENT, DISPATCH, PRODUCTION, MACHINE_SCHEDULE, QUALITY, STOCK modules.
    /// </summary>
    [HttpGet("tv-events")]
    public async Task<IActionResult> GetTvEvents([FromQuery] DateTime? since)
    {
        var cutoff = since ?? DateTime.Now.AddMinutes(-2);

        var relevantModules = new[]
        {
            "JOB", "ENQUIRY", "QUOTATION", "PAYMENT", "DISPATCH",
            "PRODUCTION", "MACHINE_SCHEDULE", "QUALITY", "STOCK", "CRM"
        };

        var relevantTypes = new[]
        {
            "CREATE", "STATUS_CHANGE", "APPROVE", "REJECT",
            "ASSIGN", "CANCEL", "CLOSE", "REOPEN", "SEND"
        };

        var events = await _db.TrnUserActivityLogs
            .Where(a => a.ActivityOn > cutoff
                     && relevantModules.Contains(a.Module)
                     && relevantTypes.Contains(a.ActivityType))
            .OrderByDescending(a => a.ActivityOn)
            .Take(30)
            .Select(a => new
            {
                a.ActivityLogId,
                a.Module,
                a.ActivityType,
                a.Title,
                a.Description,
                a.EntityType,
                a.EntityCode,
                a.Severity,
                ActivityOn = a.ActivityOn
            })
            .ToListAsync();

        return Ok(new
        {
            serverTime = DateTime.Now,
            events
        });
    }

    private static string MapTvTaskStage(string? processCode)
    {
        var code = (processCode ?? string.Empty).ToUpperInvariant();
        return code switch
        {
            WkProcessCode.DesDtp or "DES_ART" or "PRE_DES" => "DESIGNING",
            WkProcessCode.PrePress or "PRE_CTP" => "CTP",
            WkProcessCode.Print or "PROC" => "PRINTING",
            _ => "RUNNING"
        };
    }

    private static string? ExtractPlateName(string? title, string? description)
    {
        var source = string.Join(" ", new[] { title, description }.Where(s => !string.IsNullOrWhiteSpace(s)));
        if (string.IsNullOrWhiteSpace(source)) return null;

        var markers = new[] { "plate:", "plate-", "plate " };
        foreach (var marker in markers)
        {
            var idx = source.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) continue;

            var value = source[(idx + marker.Length)..].Trim();
            if (string.IsNullOrWhiteSpace(value)) continue;

            var stopChars = new[] { ',', ';', '.', '|', ')' };
            var endIdx = value.IndexOfAny(stopChars);
            return (endIdx > 0 ? value[..endIdx] : value).Trim();
        }

        return null;
    }

    private sealed class TvRunningJobVm
    {
        public string CardSource { get; set; } = "MACHINE";
        public long? WorkspaceTaskId { get; set; }
        public long AllocationId { get; set; }
        public long JobId { get; set; }
        public string? JobNo { get; set; }
        public string? ProductName { get; set; }
        public int? Quantity { get; set; }
        public string? Priority { get; set; }
        public DateOnly? DeliveryDate { get; set; }
        public string? StatusCode { get; set; }
        public string? CurrentStage { get; set; }
        public int? ProgressPercent { get; set; }
        public int? AiPriorityScore { get; set; }
        public string? PartyName { get; set; }
        public string? JobTypeName { get; set; }
        public long MachineId { get; set; }
        public string? MachineName { get; set; }
        public string? MachineCode { get; set; }
        public decimal? PlannedQuantity { get; set; }
        public decimal? CompletedQuantity { get; set; }
        public DateTime? PlannedStartTime { get; set; }
        public DateTime? PlannedEndTime { get; set; }
        public string? TaskStatus { get; set; }
        public string? TaskType { get; set; }
        public DateTime? TaskStartedOn { get; set; }
        public string? TaskUserName { get; set; }
        public string? TaskUserCode { get; set; }
        public string? TaskStage { get; set; }
        public string? TaskWorkType { get; set; }
        public string? PlateName { get; set; }
        public List<TvWorkerVm> Workers { get; set; } = [];
    }

    private sealed class TvWorkerVm
    {
        public string? EmployeeName { get; set; }
        public string? RoleCode { get; set; }
    }

    // ─── DTOs ───────────────────────────────────────────────────

    public class MaintenanceDto
    {
        public long? MachineId { get; set; }
        public string? MaintenanceType { get; set; }
        public int? FrequencyDays { get; set; }
        public DateOnly? LastMaintenanceDate { get; set; }
        public DateOnly? NextDueDate { get; set; }
        public string? VendorName { get; set; }
        public decimal? EstimatedCost { get; set; }
        public string? Remarks { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? BreakdownStartTime { get; set; }
        public DateTime? BreakdownEndTime { get; set; }
        public decimal? DowntimeMinutes { get; set; }
        public string? RepairStatus { get; set; }
        public DateTime? CompletionDate { get; set; }
    }

    public class BreakdownDto
    {
        public long MachineId { get; set; }
        public string? FaultCode { get; set; }
        public string? FaultDescription { get; set; }
        public string? FaultCategory { get; set; }
        public string? SeverityLevel { get; set; }
        public DateTime BreakdownStartTime { get; set; }
        public DateTime? BreakdownEndTime { get; set; }
        public decimal? DowntimeMinutes { get; set; }
        public string? BreakdownStatus { get; set; }
        public string? ReportedBy { get; set; }
        public long? TechnicianId { get; set; }
        public string? TechnicianName { get; set; }
        public string? RootCause { get; set; }
        public string? CorrectiveAction { get; set; }
        public string? PreventiveAction { get; set; }
        public string? SparePartsUsed { get; set; }
        public decimal? RepairCost { get; set; }
        public string? Remarks { get; set; }
    }

    public class SaveAllocationsDto
    {
        public List<AllocationItem> Allocations { get; set; } = [];
    }

    public class AllocationItem
    {
        public long JobId { get; set; }
        public long MachineId { get; set; }
    }

    public class DeallocateJobDto
    {
        public long JobId { get; set; }
        public long MachineId { get; set; }
    }

    public class SaveManpowerDto
    {
        public long JobId { get; set; }
        public long MachineId { get; set; }
        public List<ManpowerItem> Employees { get; set; } = [];
    }

    public class ManpowerItem
    {
        public long EmployeeId { get; set; }
        public string? RoleCode { get; set; }
        public string? ShiftCode { get; set; }
    }

    public class RemoveManpowerDto
    {
        public long EmployeeId { get; set; }
        public long MachineId { get; set; }
    }

    public class MoveManpowerDto
    {
        public long EmployeeId { get; set; }
        public long FromMachineId { get; set; }
        public long ToMachineId { get; set; }
    }
}
