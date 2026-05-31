using erp.minepress.infrastructure.ErrorLogging;
using erp.minepress.persistence.Context;
using erp.minepress.persistence.Models;
using erp.minepress.web.Helpers;
using erp.minepress.web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace erp.minepress.web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GatePassController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IUserActivityService _activityService;
    private readonly IWorkspaceProcessEngine _workspaceEngine;
    private readonly ILogger<GatePassController> _logger;
    private readonly ISystemErrorLogger _systemErrorLogger;

    public GatePassController(
        ApplicationDbContext db,
        IUserActivityService activityService,
        IWorkspaceProcessEngine workspaceEngine,
        ILogger<GatePassController> logger,
        ISystemErrorLogger systemErrorLogger)
    {
        _db = db;
        _activityService = activityService;
        _workspaceEngine = workspaceEngine;
        _logger = logger;
        _systemErrorLogger = systemErrorLogger;
    }

    private async Task AuditExceptionAsync(Exception ex, string additionalData, string severity = "Error")
        => await _systemErrorLogger.LogAsync(ex, HttpContext, severity, additionalData);

    // ── Gate Pass List ──
    [HttpGet("list")]
    public async Task<IActionResult> GetGatePassList()
    {
        var list = await _db.TrnGatePasses
            .Include(gp => gp.TrnGatePassItems)
            .Include(gp => gp.CreatedByNavigation)
            .OrderByDescending(gp => gp.GatePassId)
            .Select(gp => new
            {
                gp.GatePassId,
                gp.GatePassNo,
                GatePassDate = gp.GatePassDate.ToString("dd-MMM-yyyy"),
                gp.GatepassType,
                gp.ReferenceType,
                gp.ReferenceNo,
                ReferenceDate = gp.ReferenceDate.HasValue ? gp.ReferenceDate.Value.ToString("dd-MMM-yyyy") : null,
                gp.VehicleNo,
                gp.DriverName,
                gp.DriverContact,
                gp.Purpose,
                gp.TotalQuantity,
                gp.Status,
                gp.Remarks,
                ItemCount = gp.TrnGatePassItems.Count,
                ApprovedOn = gp.ApprovedOn.HasValue ? gp.ApprovedOn.Value.ToString("dd-MMM-yyyy HH:mm") : null,
                CreatedByName = gp.CreatedByNavigation != null ? gp.CreatedByNavigation.Name : "",
                CreatedOn = gp.CreatedOn.HasValue ? gp.CreatedOn.Value.ToString("dd-MMM-yyyy HH:mm") : ""
            })
            .ToListAsync();

        return Ok(list);
    }

    // ── Gate Pass Detail ──
    [HttpGet("detail/{id:long}")]
    public async Task<IActionResult> GetGatePassDetail(long id)
    {
        var gp = await _db.TrnGatePasses
            .Include(g => g.Company)
            .Include(g => g.CreatedByNavigation)
            .Include(g => g.TrnGatePassItems)
            .FirstOrDefaultAsync(g => g.GatePassId == id);

        if (gp == null)
            return NotFound(new { message = "Gate Pass not found." });

        // Try to load linked challan info
        object? linkedChallan = null;
        if (gp.ReferenceType == "CHALLAN" && !string.IsNullOrEmpty(gp.ReferenceNo))
        {
            linkedChallan = await _db.TrnChallans
                .Where(c => c.ChallanNo == gp.ReferenceNo)
                .Include(c => c.Party)
                .Include(c => c.Job)
                .Select(c => new
                {
                    c.ChallanId,
                    c.ChallanNo,
                    ChallanDate = c.ChallanDate.ToString("dd-MMM-yyyy"),
                    c.Status,
                    CustomerName = c.Party != null ? c.Party.Name : "",
                    JobNo = c.Job != null ? c.Job.JobNo : "",
                    c.JobId,
                    c.TotalQty,
                    c.TotalAmount
                })
                .FirstOrDefaultAsync();
        }

        // Activity log
        var viewUser = HttpContext.Session.GetCurrentUser();
        if (viewUser != null)
        {
            var viewActivity = ActivityLogEntry.FromUser(viewUser, "GATE_PASS", "VIEW", $"Viewed Gate Pass {gp.GatePassNo}");
            viewActivity.ActivityCategory = "NAVIGATION";
            viewActivity.EntityType = "GATE_PASS";
            viewActivity.EntityId = gp.GatePassId;
            viewActivity.EntityCode = gp.GatePassNo;
            viewActivity.Description = $"Viewed Gate Pass {gp.GatePassNo} details.";
            await _activityService.LogActivityAsync(viewActivity);
        }

        var result = new
        {
            gp.GatePassId,
            gp.GatePassNo,
            GatePassDate = gp.GatePassDate.ToString("dd-MMM-yyyy"),
            GatePassDateIso = gp.GatePassDate.ToString("yyyy-MM-dd"),
            gp.GatepassType,
            gp.ReferenceType,
            gp.ReferenceNo,
            ReferenceDate = gp.ReferenceDate?.ToString("dd-MMM-yyyy"),
            gp.VehicleNo,
            gp.DriverName,
            gp.DriverContact,
            gp.Purpose,
            gp.TotalQuantity,
            gp.Status,
            gp.Remarks,
            CompanyName = gp.Company?.Name,
            CompanyGstin = gp.Company?.Gstin,
            CompanyAddress = gp.Company?.AddressLine1,
            ApprovedOn = gp.ApprovedOn?.ToString("dd-MMM-yyyy HH:mm"),
            CreatedByName = gp.CreatedByNavigation?.Name ?? "",
            CreatedOn = gp.CreatedOn?.ToString("dd-MMM-yyyy HH:mm"),
            Items = gp.TrnGatePassItems
                .OrderBy(i => i.ItemSequence)
                .Select(i => new
                {
                    i.GatePassItemId,
                    i.ItemSequence,
                    i.Description,
                    i.Quantity,
                    i.UomId,
                    i.ReceivedQuantity,
                    i.PendingQuantity,
                    i.Status,
                    i.Remarks
                }),
            LinkedChallan = linkedChallan
        };

        return Ok(result);
    }

    // ── Update Gate Pass Status ──
    [HttpPost("updatestatus")]
    public async Task<IActionResult> UpdateGatePassStatus([FromBody] GatePassStatusRequest request)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized(new { message = "Session expired. Please login again." });

        var gp = await _db.TrnGatePasses.FindAsync(request.GatePassId);
        if (gp == null)
            return NotFound(new { message = "Gate Pass not found." });

        var oldStatus = gp.Status;
        gp.Status = request.Status;
        gp.ModifiedBy = user.UserId.ToString();
        gp.ModifiedOn = DateTime.Now;

        if (request.Status == "APPROVED")
        {
            gp.ApprovedBy = user.UserId;
            gp.ApprovedOn = DateTime.Now;
        }

        await _db.SaveChangesAsync();

        var statusActivity = ActivityLogEntry.FromUser(user, "GATE_PASS", "STATUS_CHANGE", $"Gate Pass {gp.GatePassNo} — {request.Status}");
        statusActivity.EntityType = "GATE_PASS";
        statusActivity.EntityId = gp.GatePassId;
        statusActivity.EntityCode = gp.GatePassNo;
        statusActivity.Description = $"Status changed from {oldStatus ?? "N/A"} to {request.Status} by {user.Name}.";
        statusActivity.Severity = "INFO";
        await _activityService.LogActivityAsync(statusActivity);

        await _activityService.LogNotificationAsync(new UserNotificationEntry
        {
            UserId = user.UserId,
            Title = $"Gate Pass {request.Status}",
            Message = $"Gate Pass {gp.GatePassNo} status updated to {request.Status.ToLower().Replace('_', ' ')}.",
            Icon = request.Status switch
            {
                "APPROVED" => "bi bi-check-circle",
                "COMPLETED" => "bi bi-check-all",
                "CANCELLED" => "bi bi-x-circle",
                _ => "bi bi-shield-check"
            },
            Color = request.Status switch
            {
                "APPROVED" => "success",
                "COMPLETED" => "green",
                "CANCELLED" => "danger",
                _ => "primary"
            },
            Module = "GATE_PASS",
            EventType = "STATUS_CHANGED",
            ReferenceId = (int)gp.GatePassId,
            ReferenceUrl = $"/GatePass/Details?id={gp.GatePassId}"
        });

        // If linked to challan, add challan timeline entry
        if (gp.ReferenceType == "CHALLAN" && !string.IsNullOrEmpty(gp.ReferenceNo))
        {
            var challan = await _db.TrnChallans.FirstOrDefaultAsync(c => c.ChallanNo == gp.ReferenceNo);
            if (challan != null)
            {
                var tlEntry = new TrnChallanTimeline
                {
                    ChallanId = challan.ChallanId,
                    JobId = challan.JobId,
                    EventType = $"GATE_PASS_{request.Status}",
                    EventCode = request.Status,
                    EventTitle = $"Gate Pass {gp.GatePassNo} — {request.Status}",
                    EventDescription = $"Gate Pass {gp.GatePassNo} status updated to {request.Status}.",
                    NewStatus = request.Status,
                    CreatedBy = user.UserId,
                    CreatedOn = DateTime.Now,
                    IsActive = true
                };
                _db.TrnChallanTimelines.Add(tlEntry);
                await _db.SaveChangesAsync();

                // ── Auto-complete GATE_PASS workspace tasks when approved/completed ──
                if (request.Status is "APPROVED" or "COMPLETED")
                {
                    await _workspaceEngine.AutoCompleteProcessTasksAsync(
                        sourceTable: "trn_challan",
                        sourceId: challan.ChallanId,
                        upToProcessCode: "GATE_PASS",
                        remarks: $"Gate Pass {gp.GatePassNo} {request.Status.ToLower()}. Workspace task auto-completed.",
                        completedBy: user,
                        jobId: challan.JobId);
                }
            }
        }

        return Ok(new { message = $"Gate Pass status updated to {request.Status}." });
    }
}

public class GatePassStatusRequest
{
    public long GatePassId { get; set; }
    public string Status { get; set; } = string.Empty;
}
