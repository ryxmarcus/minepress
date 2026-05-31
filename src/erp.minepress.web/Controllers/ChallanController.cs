using erp.minepress.domain.Enums;
using erp.minepress.infrastructure.ErrorLogging;
using erp.minepress.notification.Enums;
using erp.minepress.notification.Interfaces;
using erp.minepress.notification.Models;
using erp.minepress.persistence.Context;
using erp.minepress.persistence.Models;
using erp.minepress.web.Helpers;
using erp.minepress.web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace erp.minepress.web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChallanController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly INotificationDispatcher _notificationDispatcher;
    private readonly IUserActivityService _activityService;
    private readonly IDocumentNumberService _documentNumberService;
    private readonly IWorkspaceProcessEngine _workspaceEngine;
    private readonly ILogger<ChallanController> _logger;
    private readonly ISystemErrorLogger _systemErrorLogger;

    public ChallanController(
        ApplicationDbContext db,
        INotificationDispatcher notificationDispatcher,
        IUserActivityService activityService,
        IDocumentNumberService documentNumberService,
        IWorkspaceProcessEngine workspaceEngine,
        ILogger<ChallanController> logger,
        ISystemErrorLogger systemErrorLogger)
    {
        _db = db;
        _notificationDispatcher = notificationDispatcher;
        _activityService = activityService;
        _documentNumberService = documentNumberService;
        _workspaceEngine = workspaceEngine;
        _logger = logger;
        _systemErrorLogger = systemErrorLogger;
    }

    private async Task AuditExceptionAsync(Exception ex, string additionalData, string severity = "Error")
        => await _systemErrorLogger.LogAsync(ex, HttpContext, severity, additionalData);

    // ── Challan List ──
    [HttpGet("list")]
    public async Task<IActionResult> GetChallanList()
    {
        var list = await _db.TrnChallans
            .Include(c => c.Party)
            .Include(c => c.Job)
            .Include(c => c.TrnChallanItems)
            .Include(c => c.CreatedByNavigation)
            .OrderByDescending(c => c.ChallanId)
            .Select(c => new
            {
                c.ChallanId,
                c.ChallanNo,
                ChallanDate = c.ChallanDate.ToString("dd-MMM-yyyy"),
                CustomerName = c.Party != null ? c.Party.Name : "",
                CustomerCode = c.Party != null ? c.Party.Code : "",
                JobNo = c.Job != null ? c.Job.JobNo : "",
                c.JobId,
                c.Status,
                c.TotalQty,
                c.TotalAmount,
                c.VehicleNo,
                c.TransportDetails,
                c.DeliveryAddress,
                c.Remarks,
                ItemCount = c.TrnChallanItems.Count,
                CreatedByName = c.CreatedByNavigation != null ? c.CreatedByNavigation.Name : "",
                CreatedOn = c.CreatedOn.HasValue ? c.CreatedOn.Value.ToString("dd-MMM-yyyy HH:mm") : ""
            })
            .ToListAsync();

        return Ok(list);
    }

    // ── Challan Detail ──
    [HttpGet("detail/{id:long}")]
    public async Task<IActionResult> GetChallanDetail(long id)
    {
        var challan = await _db.TrnChallans
            .Include(c => c.Party)
            .Include(c => c.Job).ThenInclude(j => j!.Party)
            .Include(c => c.Job).ThenInclude(j => j!.Enquiry)
            .Include(c => c.Job).ThenInclude(j => j!.Quotation)
            .Include(c => c.Company)
            .Include(c => c.CreatedByNavigation)
            .Include(c => c.TrnChallanItems)
                .ThenInclude(ci => ci.JobItem)
            .FirstOrDefaultAsync(c => c.ChallanId == id);

        if (challan == null)
            return NotFound(new { message = "Challan not found." });

        var timeline = await _db.TrnChallanTimelines
            .Where(t => t.ChallanId == id && t.IsActive == true)
            .OrderByDescending(t => t.CreatedOn)
            .ToListAsync();

        // Gate passes linked by reference
        var gatePasses = await _db.TrnGatePasses
            .Where(gp => gp.ReferenceType == "CHALLAN" && gp.ReferenceNo == challan.ChallanNo)
            .OrderByDescending(gp => gp.GatePassId)
            .Select(gp => new
            {
                gp.GatePassId,
                gp.GatePassNo,
                GatePassDate = gp.GatePassDate.ToString("dd-MMM-yyyy"),
                gp.GatepassType,
                gp.VehicleNo,
                gp.DriverName,
                gp.Status,
                gp.TotalQuantity,
                gp.Purpose,
                CreatedOn = gp.CreatedOn.HasValue ? gp.CreatedOn.Value.ToString("dd-MMM-yyyy HH:mm") : ""
            })
            .ToListAsync();

        // Activity log
        var viewUser = HttpContext.Session.GetCurrentUser();
        if (viewUser != null)
        {
            var viewActivity = ActivityLogEntry.FromUser(viewUser, "CHALLAN", "VIEW", $"Viewed Challan {challan.ChallanNo}");
            viewActivity.ActivityCategory = "NAVIGATION";
            viewActivity.EntityType = "CHALLAN";
            viewActivity.EntityId = challan.ChallanId;
            viewActivity.EntityCode = challan.ChallanNo;
            viewActivity.Description = $"Viewed challan {challan.ChallanNo} details.";
            await _activityService.LogActivityAsync(viewActivity);
        }

        var result = new
        {
            challan.ChallanId,
            challan.ChallanNo,
            ChallanDate = challan.ChallanDate.ToString("dd-MMM-yyyy"),
            ChallanDateIso = challan.ChallanDate.ToString("yyyy-MM-dd"),
            CustomerName = challan.Party?.Name,
            CustomerCode = challan.Party?.Code,
            CustomerGst = challan.Party?.Gstno,
            CustomerEmail = challan.Party?.Email,
            CustomerAddress = challan.Party?.Address1,
            PartyId = challan.PartyId,
            challan.JobId,
            JobNo = challan.Job?.JobNo,
            JobDate = challan.Job?.JobDate.ToString("dd-MMM-yyyy"),
            JobStatus = challan.Job?.StatusCode,
            challan.DeliveryAddress,
            challan.TransportDetails,
            challan.VehicleNo,
            challan.ReferenceNo,
            challan.TotalQty,
            challan.TotalAmount,
            challan.Status,
            challan.Remarks,
            CompanyName = challan.Company?.Name,
            CompanyGstin = challan.Company?.Gstin,
            CompanyAddress = challan.Company?.AddressLine1,
            CompanyEmail = challan.Company?.EmailId,
            CompanyPhone = challan.Company?.ContactNo,
            EnquiryId = challan.Job?.EnquiryId,
            EnquiryNo = challan.Job?.Enquiry?.EnquiryNo,
            QuotationId = challan.Job?.QuotationId,
            QuotationNo = challan.Job?.Quotation?.QuotationNo,
            CreatedByName = challan.CreatedByNavigation?.Name ?? "",
            CreatedOn = challan.CreatedOn?.ToString("dd-MMM-yyyy HH:mm"),
            Items = challan.TrnChallanItems
                .OrderBy(ci => ci.ItemSequence)
                .Select(ci => new
                {
                    ci.ChallanItemId,
                    ci.ItemSequence,
                    ci.ProductName,
                    ci.ProductDescription,
                    ci.JobQuantity,
                    ci.DeliveredQuantity,
                    ci.PendingQuantity,
                    ci.UomId,
                    ci.Rate,
                    ci.Amount,
                    ci.Remarks,
                    ci.JobItemId
                }),
            Timeline = timeline.Select(t => new
            {
                t.TimelineId,
                t.EventType,
                t.EventCode,
                t.EventTitle,
                t.EventDescription,
                t.Remarks,
                t.OldStatus,
                t.NewStatus,
                t.OldQuantity,
                t.NewQuantity,
                t.OldAmount,
                t.NewAmount,
                t.MovementType,
                t.ProcessCode,
                t.ProcessName,
                t.CommunicationMode,
                t.CommunicationReference,
                t.AttachmentUrl,
                CreatedOn = t.CreatedOn.ToString("dd-MMM-yyyy HH:mm")
            }),
            GatePasses = gatePasses
        };

        return Ok(result);
    }

    // ── Get Job Data for Challan Creation ──
    [HttpGet("from-job/{jobId:long}")]
    public async Task<IActionResult> GetJobDataForChallan(long jobId)
    {
        var job = await _db.TrnJobs
            .Include(j => j.Party)
            .Include(j => j.Company)
            .Include(j => j.RateCalc)
            .Include(j => j.TrnJobItems)
            .FirstOrDefaultAsync(j => j.JobId == jobId);

        if (job == null)
            return NotFound(new { message = "Job not found." });

        // Get existing challans for this job to calculate already delivered qty
        var existingChallanItems = await _db.TrnChallanItems
            .Where(ci => ci.Challan.JobId == jobId && ci.Challan.Status != "CANCELLED")
            .GroupBy(ci => ci.JobItemId)
            .Select(g => new { JobItemId = g.Key, TotalDelivered = g.Sum(ci => ci.DeliveredQuantity ?? 0) })
            .ToListAsync();

        var deliveredMap = existingChallanItems.ToDictionary(x => x.JobItemId, x => x.TotalDelivered);

        var result = new
        {
            job.JobId,
            job.JobNo,
            job.PartyId,
            CustomerName = job.Party?.Name,
            CustomerCode = job.Party?.Code,
            CustomerEmail = job.Party?.Email,
            CustomerGst = job.Party?.Gstno,
            CustomerAddress = job.Party?.Address1,
            CompanyId = job.CompanyId,
            job.LocationId,
            job.ProductName,
            job.Quantity,
            job.NetAmount,
            BomDataJson = job.RateCalc?.BomData,
            Items = job.TrnJobItems
                .OrderBy(i => i.ItemSequence)
                .Select(i => new
                {
                    i.JobItemId,
                    i.ItemSequence,
                    i.ProductName,
                    i.ProductDescription,
                    JobQuantity = i.Quantity ?? 0,
                    AlreadyDelivered = deliveredMap.ContainsKey(i.JobItemId) ? deliveredMap[i.JobItemId] : 0,
                    PendingQuantity = (i.Quantity ?? 0) - (deliveredMap.ContainsKey(i.JobItemId) ? deliveredMap[i.JobItemId] : 0),
                    i.UomId,
                    i.UnitRate,
                    i.NetAmount
                })
        };

        return Ok(result);
    }

    // ── AI Quantity Validation ──
    [HttpPost("validate-quantity")]
    public async Task<IActionResult> ValidateQuantity([FromBody] ChallanQuantityValidationRequest request)
    {
        var job = await _db.TrnJobs
            .Include(j => j.TrnJobItems)
            .FirstOrDefaultAsync(j => j.JobId == request.JobId);

        if (job == null)
            return NotFound(new { message = "Job not found." });

        var existingChallanItems = await _db.TrnChallanItems
            .Where(ci => ci.Challan.JobId == request.JobId && ci.Challan.Status != "CANCELLED")
            .GroupBy(ci => ci.JobItemId)
            .Select(g => new { JobItemId = g.Key, TotalDelivered = g.Sum(ci => ci.DeliveredQuantity ?? 0) })
            .ToListAsync();

        var deliveredMap = existingChallanItems.ToDictionary(x => x.JobItemId, x => x.TotalDelivered);

        var warnings = new List<object>();
        var errors = new List<object>();
        var insights = new List<object>();
        var totalJobQty = 0;
        var totalChallanQty = 0;
        var totalAlreadyDelivered = 0;

        foreach (var reqItem in request.Items ?? [])
        {
            var jobItem = job.TrnJobItems.FirstOrDefault(ji => ji.JobItemId == reqItem.JobItemId);
            if (jobItem == null) continue;

            var jobQty = jobItem.Quantity ?? 0;
            var alreadyDelivered = deliveredMap.ContainsKey(reqItem.JobItemId) ? deliveredMap[reqItem.JobItemId] : 0;
            var pendingQty = jobQty - alreadyDelivered;
            var challanQty = reqItem.Quantity;

            totalJobQty += jobQty;
            totalChallanQty += challanQty;
            totalAlreadyDelivered += alreadyDelivered;

            if (challanQty > pendingQty)
            {
                errors.Add(new
                {
                    jobItemId = reqItem.JobItemId,
                    product = jobItem.ProductName,
                    message = $"Challan qty ({challanQty}) exceeds pending qty ({pendingQty}). Job qty: {jobQty}, Already delivered: {alreadyDelivered}.",
                    severity = "ERROR"
                });
            }
            else if (challanQty == pendingQty)
            {
                insights.Add(new
                {
                    jobItemId = reqItem.JobItemId,
                    product = jobItem.ProductName,
                    message = $"Full pending quantity dispatched. This completes delivery for {jobItem.ProductName}.",
                    severity = "SUCCESS"
                });
            }
            else if (challanQty > 0 && challanQty < pendingQty)
            {
                var remaining = pendingQty - challanQty;
                insights.Add(new
                {
                    jobItemId = reqItem.JobItemId,
                    product = jobItem.ProductName,
                    message = $"Partial delivery: {challanQty} of {pendingQty} pending. Remaining after this: {remaining}.",
                    severity = "INFO"
                });
            }

            if (challanQty <= 0)
            {
                warnings.Add(new
                {
                    jobItemId = reqItem.JobItemId,
                    product = jobItem.ProductName,
                    message = $"Quantity is zero or negative for {jobItem.ProductName}.",
                    severity = "WARNING"
                });
            }
        }

        // AI summary insights
        if (totalChallanQty > 0)
        {
            var overallPending = totalJobQty - totalAlreadyDelivered;
            var pctDelivered = overallPending > 0 ? Math.Round((double)totalChallanQty / overallPending * 100, 1) : 0;
            insights.Add(new
            {
                jobItemId = (long?)null,
                product = "Overall",
                message = $"This challan covers {pctDelivered}% of pending quantities. Total job qty: {totalJobQty}, already delivered: {totalAlreadyDelivered}, this challan: {totalChallanQty}.",
                severity = "INFO"
            });
        }

        return Ok(new
        {
            isValid = errors.Count == 0,
            errors,
            warnings,
            insights,
            summary = new
            {
                totalJobQty,
                totalAlreadyDelivered,
                totalChallanQty,
                totalPending = totalJobQty - totalAlreadyDelivered - totalChallanQty
            }
        });
    }

    // ── Save Challan ──
    [HttpPost("save")]
    public async Task<IActionResult> SaveChallan([FromBody] ChallanSaveRequest request)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized(new { message = "Session expired. Please login again." });

        var challanNo = await _documentNumberService.GenerateNextNumberAsync(DocumentProcessCode.DELIVERY_CHALLAN);

        var challan = new TrnChallan
        {
            ChallanNo = challanNo,
            ChallanDate = DateOnly.FromDateTime(DateTime.Now),
            JobId = request.JobId,
            CompanyId = user.CompanyId ?? 1,
            LocationId = user.LocationId,
            PartyId = request.PartyId,
            DeliveryAddress = request.DeliveryAddress,
            TransportDetails = request.TransportDetails,
            VehicleNo = request.VehicleNo,
            ReferenceNo = request.ReferenceNo,
            TotalQty = request.TotalQty,
            TotalAmount = request.TotalAmount,
            Status = "CREATED",
            Remarks = request.Remarks,
            CreatedBy = user.UserId,
            CreatedOn = DateTime.Now
        };

        _db.TrnChallans.Add(challan);
        await _db.SaveChangesAsync();

        // Save challan items
        if (request.Items?.Any() == true)
        {
            foreach (var item in request.Items)
            {
                var challanItem = new TrnChallanItem
                {
                    ChallanId = challan.ChallanId,
                    JobItemId = item.JobItemId,
                    ItemSequence = item.ItemSequence,
                    ProductName = item.ProductName,
                    ProductDescription = item.ProductDescription,
                    JobQuantity = item.JobQuantity,
                    DeliveredQuantity = item.DeliveredQuantity,
                    PendingQuantity = item.PendingQuantity,
                    UomId = item.UomId,
                    Rate = item.Rate,
                    Amount = item.Amount,
                    Remarks = item.Remarks,
                    CreatedBy = user.UserId,
                    CreatedOn = DateTime.Now
                };
                _db.TrnChallanItems.Add(challanItem);
            }
            await _db.SaveChangesAsync();
        }

        // ── Dispatch notification ──
        await DispatchChallanNotificationAsync(challan, user, "CHALLAN_CREATED", "New Challan Created",
            $"Challan {challan.ChallanNo} created for Job. Total Qty: {challan.TotalQty}.");

        // ── Activity Log ──
        var createActivity = ActivityLogEntry.FromUser(user, "CHALLAN", "CREATE", $"Created Challan {challan.ChallanNo}");
        createActivity.EntityType = "CHALLAN";
        createActivity.EntityId = challan.ChallanId;
        createActivity.EntityCode = challan.ChallanNo;
        createActivity.Description = $"Challan {challan.ChallanNo} created with {request.Items?.Count ?? 0} item(s). Total Qty: {challan.TotalQty}.";
        createActivity.NewValues = JsonSerializer.Serialize(new { challan.ChallanNo, challan.JobId, challan.TotalQty, challan.Status, ItemCount = request.Items?.Count ?? 0 });
        createActivity.Severity = "INFO";
        await _activityService.LogActivityAsync(createActivity);

        // ── Party Activity Log ──
        if (challan.PartyId > 0)
        {
            await PartyPortalController.LogPartyActivityAsync(_db, challan.PartyId,
                "CHALLAN", "CHALLAN_CREATED",
                $"Challan {challan.ChallanNo} Created",
                $"Delivery challan created with {request.Items?.Count ?? 0} item(s). Total Qty: {challan.TotalQty}.",
                "trn_challan", challan.ChallanId, challan.ChallanNo,
                challan.ChallanDate, "Pending", "Not Required", challan.TotalAmount, user.Name);
        }

        // ── In-App Notification ──
        await _activityService.LogNotificationAsync(new UserNotificationEntry
        {
            UserId = user.UserId,
            Title = "New Challan Created",
            Message = $"Challan {challan.ChallanNo} has been created. Total Qty: {challan.TotalQty}.",
            Icon = "bi bi-truck",
            Color = "primary",
            Module = "CHALLAN",
            EventType = "CHALLAN_CREATED",
            ReferenceId = (int)challan.ChallanId,
            ReferenceUrl = $"/Challan/Details?id={challan.ChallanId}",
            Priority = "NORMAL"
        });

        // ── Challan Timeline: CREATED ──
        await AddChallanTimelineEntryAsync(
            challan.ChallanId, "CHALLAN_CREATED", "CHALLAN_CREATED",
            "Challan Created",
            $"Challan {challan.ChallanNo} created with {request.Items?.Count ?? 0} item(s). Total Qty: {challan.TotalQty}.",
            newStatus: "CREATED", newQuantity: challan.TotalQty,
            jobId: challan.JobId, userId: user.UserId);

        // ── Job Timeline: CHALLAN_CREATED ──
        await AddJobTimelineEntryAsync(
            challan.JobId, "CHALLAN_CREATED", "CHALLAN_CREATED",
            $"Challan {challan.ChallanNo} Created",
            $"Delivery challan {challan.ChallanNo} created. Total Qty: {challan.TotalQty}.",
            newStatus: "CHALLAN_CREATED", userId: user.UserId);

        // ── Auto-complete CHALLAN workspace tasks for this job ──
        await _workspaceEngine.AutoCompleteProcessTasksAsync(
            sourceTable: "trn_challan",
            sourceId: challan.ChallanId,
            upToProcessCode: "CHALLAN",
            remarks: $"Challan {challan.ChallanNo} created. Workspace task auto-completed.",
            completedBy: user,
            jobId: challan.JobId);

        return Ok(new { challan.ChallanId, challan.ChallanNo, message = "Challan saved successfully." });
    }

    // ── Update Challan Status ──
    [HttpPost("updatestatus")]
    public async Task<IActionResult> UpdateChallanStatus([FromBody] ChallanStatusRequest request)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized(new { message = "Session expired. Please login again." });

        var challan = await _db.TrnChallans
            .Include(c => c.Job)
            .FirstOrDefaultAsync(c => c.ChallanId == request.ChallanId);
        if (challan == null)
            return NotFound(new { message = "Challan not found." });

        var oldStatus = challan.Status;
        challan.Status = request.Status;
        challan.ModifiedBy = user.UserId.ToString();
        challan.ModifiedOn = DateTime.Now;
        await _db.SaveChangesAsync();

        // Map status to event type and notification details
        var (eventType, eventTitle, icon, color) = request.Status switch
        {
            "MATERIAL_ISSUED" => ("MATERIAL_ISSUED", "Material Issued", "bi bi-box-arrow-right", "info"),
            "MATERIAL_RECEIVED" => ("MATERIAL_RECEIVED", "Material Received", "bi bi-box-arrow-in-down", "success"),
            "MATERIAL_RETURNED" => ("MATERIAL_RETURNED", "Material Returned", "bi bi-arrow-return-left", "warning"),
            "OUTSOURCED_SENT" => ("OUTSOURCED_SENT", "Outsourced Material Sent", "bi bi-send", "purple"),
            "OUTSOURCED_RECEIVED" => ("OUTSOURCED_RECEIVED", "Outsourced Material Received", "bi bi-inbox", "teal"),
            "DISPATCHED" => ("DISPATCHED", "Challan Dispatched", "bi bi-truck", "blue"),
            "DELIVERED" => ("DELIVERED", "Challan Delivered", "bi bi-check-circle-fill", "success"),
            "CANCELLED" => ("CANCELLED", "Challan Cancelled", "bi bi-x-circle", "danger"),
            "CLOSED" => ("CLOSED", "Challan Closed", "bi bi-lock", "dark"),
            _ => ("CHALLAN_UPDATED", $"Status Changed to {request.Status}", "bi bi-arrow-repeat", "primary")
        };

        // ── Dispatch notification ──
        await DispatchChallanNotificationAsync(challan, user, eventType, eventTitle,
            $"Challan {challan.ChallanNo} status changed from {oldStatus ?? "N/A"} to {request.Status}.");

        // ── Activity Log ──
        var statusActivity = ActivityLogEntry.FromUser(user, "CHALLAN", "STATUS_CHANGE", $"Challan {challan.ChallanNo} — {request.Status}");
        statusActivity.EntityType = "CHALLAN";
        statusActivity.EntityId = challan.ChallanId;
        statusActivity.EntityCode = challan.ChallanNo;
        statusActivity.Description = $"Status changed from {oldStatus ?? "N/A"} to {request.Status} by {user.Name}.";
        statusActivity.OldValues = JsonSerializer.Serialize(new { Status = oldStatus });
        statusActivity.NewValues = JsonSerializer.Serialize(new { Status = request.Status });
        statusActivity.ChangedFields = ["Status"];
        statusActivity.Severity = request.Status is "CANCELLED" ? "WARNING" : "INFO";
        await _activityService.LogActivityAsync(statusActivity);

        // ── Party Activity Log: Status Change ──
        if (challan.PartyId > 0)
        {
            var challanSt = request.Status switch
            {
                "DELIVERED" => "Completed",
                "CANCELLED" => "Cancelled",
                "DISPATCHED" => "Pending",
                _ => "Pending"
            };
            await PartyPortalController.LogPartyActivityAsync(_db, challan.PartyId,
                "CHALLAN", $"CHALLAN_{request.Status}",
                $"Challan {challan.ChallanNo} — {request.Status.Replace('_', ' ')}",
                $"Status changed from {oldStatus ?? "N/A"} to {request.Status}.",
                "trn_challan", challan.ChallanId, challan.ChallanNo,
                challan.ChallanDate, challanSt, "Not Required", challan.TotalAmount, user.Name);
        }

        // ── In-App Notification ──
        await _activityService.LogNotificationAsync(new UserNotificationEntry
        {
            UserId = user.UserId,
            Title = eventTitle,
            Message = $"Challan {challan.ChallanNo} has been updated to {request.Status.ToLower().Replace('_', ' ')}.",
            Icon = icon,
            Color = color,
            Module = "CHALLAN",
            EventType = eventType,
            ReferenceId = (int)challan.ChallanId,
            ReferenceUrl = $"/Challan/Details?id={challan.ChallanId}"
        });

        // ── Challan Timeline ──
        await AddChallanTimelineEntryAsync(
            challan.ChallanId, eventType, request.Status,
            eventTitle,
            $"Status changed from {oldStatus ?? "N/A"} to {request.Status} by {user.Name}.",
            oldStatus: oldStatus, newStatus: request.Status,
            processCode: request.Status, processName: request.Status.Replace('_', ' '),
            jobId: challan.JobId, userId: user.UserId);

        // ── Job Timeline ──
        await AddJobTimelineEntryAsync(
            challan.JobId, eventType, request.Status,
            $"Challan {challan.ChallanNo} — {eventTitle}",
            $"Challan {challan.ChallanNo} status: {oldStatus ?? "N/A"} → {request.Status}.",
            oldStatus: oldStatus, newStatus: request.Status,
            userId: user.UserId);

        // If delivered, update job item delivered quantities
        if (request.Status == "DELIVERED")
        {
            var challanItems = await _db.TrnChallanItems
                .Where(ci => ci.ChallanId == challan.ChallanId)
                .ToListAsync();

            foreach (var ci in challanItems)
            {
                var jobItem = await _db.TrnJobItems.FindAsync(ci.JobItemId);
                if (jobItem != null)
                {
                    jobItem.DeliveredQuantity = (jobItem.DeliveredQuantity ?? 0) + (ci.DeliveredQuantity ?? 0);
                    jobItem.PendingQuantity = (jobItem.Quantity ?? 0) - (jobItem.DeliveredQuantity ?? 0);
                }
            }
            await _db.SaveChangesAsync();
        }

        return Ok(new { message = $"Challan status updated to {request.Status}." });
    }

    // ── Issue Gate Pass from Challan ──
    [HttpPost("issue-gatepass")]
    public async Task<IActionResult> IssueGatePass([FromBody] GatePassFromChallanRequest request)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized(new { message = "Session expired. Please login again." });

        var challan = await _db.TrnChallans
            .Include(c => c.TrnChallanItems)
            .Include(c => c.Job)
            .FirstOrDefaultAsync(c => c.ChallanId == request.ChallanId);

        if (challan == null)
            return NotFound(new { message = "Challan not found." });

        var gatePassNo = await _documentNumberService.GenerateNextNumberAsync(DocumentProcessCode.GATE_PASS_OUT);

        var gatePass = new TrnGatePass
        {
            GatePassNo = gatePassNo,
            GatePassDate = DateOnly.FromDateTime(DateTime.Now),
            GatepassType = "OUT",
            CompanyId = user.CompanyId ?? 1,
            LocationId = user.LocationId,
            ReferenceType = "CHALLAN",
            ReferenceNo = challan.ChallanNo,
            ReferenceDate = challan.ChallanDate,
            VehicleNo = request.VehicleNo ?? challan.VehicleNo,
            DriverName = request.DriverName,
            DriverContact = request.DriverContact,
            Purpose = request.Purpose ?? $"Delivery against Challan {challan.ChallanNo}",
            TotalQuantity = challan.TotalQty,
            Status = "ISSUED",
            Remarks = request.Remarks,
            CreatedBy = user.UserId,
            CreatedOn = DateTime.Now,
            ApprovedBy = user.UserId,
            ApprovedOn = DateTime.Now
        };

        _db.TrnGatePasses.Add(gatePass);
        await _db.SaveChangesAsync();

        // Save gate pass items from challan items
        foreach (var ci in challan.TrnChallanItems.OrderBy(c => c.ItemSequence))
        {
            var gpItem = new TrnGatePassItem
            {
                GatePassId = gatePass.GatePassId,
                ItemSequence = ci.ItemSequence,
                Description = ci.ProductName,
                Quantity = ci.DeliveredQuantity ?? 0,
                UomId = ci.UomId,
                PendingQuantity = ci.DeliveredQuantity ?? 0,
                Status = "PENDING",
                CreatedOn = DateTime.Now
            };
            _db.TrnGatePassItems.Add(gpItem);
        }
        await _db.SaveChangesAsync();

        // ── Dispatch notification ──
        await DispatchChallanNotificationAsync(challan, user, "GATE_PASS_ISSUED", "Gate Pass Issued",
            $"Gate Pass {gatePass.GatePassNo} issued for Challan {challan.ChallanNo}.");

        // ── Activity Log ──
        var gpActivity = ActivityLogEntry.FromUser(user, "CHALLAN", "GATE_PASS_ISSUED", $"Gate Pass {gatePass.GatePassNo} issued");
        gpActivity.EntityType = "GATE_PASS";
        gpActivity.EntityId = gatePass.GatePassId;
        gpActivity.EntityCode = gatePass.GatePassNo;
        gpActivity.RelatedEntityType = "CHALLAN";
        gpActivity.RelatedEntityId = challan.ChallanId;
        gpActivity.RelatedEntityCode = challan.ChallanNo;
        gpActivity.Description = $"Gate Pass {gatePass.GatePassNo} issued for Challan {challan.ChallanNo}. Vehicle: {gatePass.VehicleNo ?? "N/A"}.";
        gpActivity.Severity = "INFO";
        await _activityService.LogActivityAsync(gpActivity);

        // ── In-App Notification ──
        await _activityService.LogNotificationAsync(new UserNotificationEntry
        {
            UserId = user.UserId,
            Title = "Gate Pass Issued",
            Message = $"Gate Pass {gatePass.GatePassNo} issued for Challan {challan.ChallanNo}.",
            Icon = "bi bi-shield-check",
            Color = "success",
            Module = "GATE_PASS",
            EventType = "GATE_PASS_ISSUED",
            ReferenceId = (int)gatePass.GatePassId,
            ReferenceUrl = $"/GatePass/Details?id={gatePass.GatePassId}"
        });

        // ── Challan Timeline ──
        await AddChallanTimelineEntryAsync(
            challan.ChallanId, "GATE_PASS_ISSUED", "GATE_PASS_ISSUED",
            $"Gate Pass {gatePass.GatePassNo} Issued",
            $"Gate Pass {gatePass.GatePassNo} issued. Vehicle: {gatePass.VehicleNo ?? "N/A"}, Driver: {gatePass.DriverName ?? "N/A"}.",
            newStatus: "GATE_PASS_ISSUED",
            jobId: challan.JobId, userId: user.UserId);

        // ── Job Timeline ──
        await AddJobTimelineEntryAsync(
            challan.JobId, "GATE_PASS_ISSUED", "GATE_PASS_ISSUED",
            $"Gate Pass {gatePass.GatePassNo} Issued",
            $"Gate Pass {gatePass.GatePassNo} issued for Challan {challan.ChallanNo}.",
            userId: user.UserId);

        return Ok(new { gatePass.GatePassId, gatePass.GatePassNo, message = "Gate Pass issued successfully." });
    }

    // ── Delete Challan ──
    [HttpDelete("delete/{id:long}")]
    public async Task<IActionResult> DeleteChallan(long id)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized(new { message = "Session expired. Please login again." });

        var challan = await _db.TrnChallans
            .Include(c => c.TrnChallanItems)
            .FirstOrDefaultAsync(c => c.ChallanId == id);

        if (challan == null)
            return NotFound(new { message = "Challan not found." });

        if (challan.Status != "CREATED")
            return BadRequest(new { message = "Only CREATED challans can be deleted." });

        var challanNo = challan.ChallanNo;

        _db.TrnChallanItems.RemoveRange(challan.TrnChallanItems);
        _db.TrnChallans.Remove(challan);
        await _db.SaveChangesAsync();

        var deleteActivity = ActivityLogEntry.FromUser(user, "CHALLAN", "DELETE", $"Deleted Challan {challanNo}");
        deleteActivity.EntityType = "CHALLAN";
        deleteActivity.EntityId = id;
        deleteActivity.EntityCode = challanNo;
        deleteActivity.Description = $"Challan {challanNo} (CREATED) was deleted by {user.Name}.";
        deleteActivity.Severity = "WARNING";
        await _activityService.LogActivityAsync(deleteActivity);

        return Ok(new { message = "Challan deleted successfully." });
    }

    // ── Challan Timeline ──
    [HttpGet("timeline/{challanId:long}")]
    public async Task<IActionResult> GetChallanTimeline(long challanId)
    {
        var timeline = await _db.TrnChallanTimelines
            .Where(t => t.ChallanId == challanId && t.IsActive == true)
            .OrderByDescending(t => t.CreatedOn)
            .Select(t => new
            {
                t.TimelineId,
                t.EventType,
                t.EventCode,
                t.EventTitle,
                t.EventDescription,
                t.Remarks,
                t.OldStatus,
                t.NewStatus,
                t.OldQuantity,
                t.NewQuantity,
                t.OldAmount,
                t.NewAmount,
                t.MovementType,
                t.ProcessCode,
                t.ProcessName,
                t.CommunicationMode,
                t.CommunicationReference,
                t.AttachmentUrl,
                CreatedOn = t.CreatedOn.ToString("dd-MMM-yyyy HH:mm"),
                CreatedOnIso = t.CreatedOn.ToString("yyyy-MM-ddTHH:mm:ss")
            })
            .ToListAsync();

        return Ok(timeline);
    }

    // ══════════════════════════════════════════
    //  HELPERS
    // ══════════════════════════════════════════

    private async Task DispatchChallanNotificationAsync(TrnChallan challan, UserSessionData user, string eventType, string eventLabel, string bodyText)
    {
        try
        {
            var config = new ProcessNotificationConfig
            {
                ConfigId = 1,
                ProcessCode = nameof(ProcessCode.EnqJob),
                SubProcessCode = notification.Enums.SubProcessCode.ReceiveEnq,
                EventType = NotificationEventType.TaskAssign,
                EventLabel = eventLabel,
                RecipientType = RecipientType.Internal,
                NotifyAssignee = true,
                NotifyDeptHead = true,
                NotifyInternalEmail = true,
                NotifyPush = true,
                TemplateCode = nameof(NotificationTemplateCode.TaskAssigned),
                Priority = NotificationPriority.Normal,
                IsActive = true,
                TriggerOnStatus = challan.Status,
                AutoTrigger = true
            };

            var template = new NotificationTemplate
            {
                TemplateId = 1,
                TemplateCode = nameof(NotificationTemplateCode.TaskAssigned),
                TemplateName = eventLabel,
                Module = nameof(NotificationModule.Quotation),
                EventType = nameof(NotificationEventType.TaskAssign),
                Channel = NotificationChannel.Email,
                SubjectTemplate = $"Challan {{{{challan_no}}}} — {eventType}",
                BodyTemplate = $$$"""
                    <h3>{{{eventLabel}}}</h3>
                    <p><strong>Challan No:</strong> {{challan_no}}</p>
                    <p><strong>Status:</strong> {{status}}</p>
                    <p><strong>Total Qty:</strong> {{total_qty}}</p>
                    <p><strong>Updated By:</strong> {{updated_by}}</p>
                    <p>{{{bodyText}}}</p>
                    """,
                IsActive = true
            };

            var context = new NotificationContext
            {
                AssigneeUserId = (int)user.UserId,
                AssigneeEmail = user.EmailId,
                AssigneePhone = user.MobileNo,
                Variables = new Dictionary<string, string>
                {
                    ["challan_no"] = challan.ChallanNo,
                    ["status"] = challan.Status ?? "N/A",
                    ["total_qty"] = challan.TotalQty?.ToString("N0") ?? "0",
                    ["updated_by"] = user.Name,
                    ["challan_date"] = challan.ChallanDate.ToString("dd-MMM-yyyy")
                }
            };

            var results = await _notificationDispatcher.DispatchAsync(config, template, context);
            _logger.LogInformation("Challan {ChallanNo} {Event}: Dispatched {Count} notifications",
                challan.ChallanNo, eventType, results.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to dispatch notification for challan {ChallanNo}", challan.ChallanNo);
        }
    }

    private async Task AddChallanTimelineEntryAsync(
        long challanId, string eventType, string? eventCode,
        string eventTitle, string? eventDescription,
        string? oldStatus = null, string? newStatus = null,
        decimal? oldQuantity = null, decimal? newQuantity = null,
        decimal? oldAmount = null, decimal? newAmount = null,
        string? remarks = null, long? jobId = null,
        string? communicationMode = null, string? communicationReference = null,
        string? processCode = null, string? processName = null,
        string? movementType = null, long userId = 0)
    {
        try
        {
            var entry = new TrnChallanTimeline
            {
                ChallanId = challanId,
                JobId = jobId,
                EventType = eventType,
                EventCode = eventCode,
                EventTitle = eventTitle,
                EventDescription = eventDescription,
                Remarks = remarks,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                OldQuantity = oldQuantity,
                NewQuantity = newQuantity,
                OldAmount = oldAmount,
                NewAmount = newAmount,
                MovementType = movementType,
                ProcessCode = processCode,
                ProcessName = processName,
                CommunicationMode = communicationMode,
                CommunicationReference = communicationReference,
                CreatedBy = userId,
                CreatedOn = DateTime.Now,
                IsActive = true
            };
            _db.TrnChallanTimelines.Add(entry);
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add challan timeline entry for challan {ChallanId}: {EventType}", challanId, eventType);
        }
    }

    private async Task AddJobTimelineEntryAsync(
        long jobId, string eventType, string? eventCode,
        string eventTitle, string? eventDescription,
        string? oldStatus = null, string? newStatus = null,
        decimal? oldAmount = null, decimal? newAmount = null,
        string? remarks = null, string? communicationMode = null,
        string? communicationReference = null, long userId = 0)
    {
        try
        {
            var entry = new TrnJobTimeline
            {
                JobId = jobId,
                EventType = eventType,
                EventCode = eventCode,
                EventTitle = eventTitle,
                EventDescription = eventDescription,
                Remarks = remarks,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                OldAmount = oldAmount,
                NewAmount = newAmount,
                CommunicationMode = communicationMode,
                CommunicationReference = communicationReference,
                CreatedBy = userId,
                CreatedOn = DateTime.Now,
                IsActive = true
            };
            _db.TrnJobTimelines.Add(entry);
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add job timeline entry for job {JobId}: {EventType}", jobId, eventType);
        }
    }
}

// ── Request Models ──

public class ChallanSaveRequest
{
    public long JobId { get; set; }
    public int PartyId { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? TransportDetails { get; set; }
    public string? VehicleNo { get; set; }
    public string? ReferenceNo { get; set; }
    public decimal? TotalQty { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? Remarks { get; set; }
    public List<ChallanItemRequest>? Items { get; set; }
}

public class ChallanItemRequest
{
    public long JobItemId { get; set; }
    public int ItemSequence { get; set; }
    public string? ProductName { get; set; }
    public string? ProductDescription { get; set; }
    public int? JobQuantity { get; set; }
    public int? DeliveredQuantity { get; set; }
    public int? PendingQuantity { get; set; }
    public int? UomId { get; set; }
    public decimal? Rate { get; set; }
    public decimal? Amount { get; set; }
    public string? Remarks { get; set; }
}

public class ChallanStatusRequest
{
    public long ChallanId { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class GatePassFromChallanRequest
{
    public long ChallanId { get; set; }
    public string? VehicleNo { get; set; }
    public string? DriverName { get; set; }
    public string? DriverContact { get; set; }
    public string? Purpose { get; set; }
    public string? Remarks { get; set; }
}

public class ChallanQuantityValidationRequest
{
    public long JobId { get; set; }
    public List<ChallanQuantityItem>? Items { get; set; }
}

public class ChallanQuantityItem
{
    public long JobItemId { get; set; }
    public int Quantity { get; set; }
}
