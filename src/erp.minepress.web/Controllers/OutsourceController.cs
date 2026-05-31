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
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;

namespace erp.minepress.web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OutsourceController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly INotificationDispatcher _notificationDispatcher;
    private readonly IUserActivityService _activityService;
    private readonly IDocumentNumberService _documentNumberService;
    private readonly ILogger<OutsourceController> _logger;
    private readonly ISystemErrorLogger _systemErrorLogger;

    public OutsourceController(
        ApplicationDbContext db,
        INotificationDispatcher notificationDispatcher,
        IUserActivityService activityService,
        IDocumentNumberService documentNumberService,
        ILogger<OutsourceController> logger,
        ISystemErrorLogger systemErrorLogger)
    {
        _db = db;
        _notificationDispatcher = notificationDispatcher;
        _activityService = activityService;
        _documentNumberService = documentNumberService;
        _logger = logger;
        _systemErrorLogger = systemErrorLogger;
    }

    private async Task AuditExceptionAsync(Exception ex, string additionalData, string severity = "Error")
        => await _systemErrorLogger.LogAsync(ex, HttpContext, severity, additionalData);

    // ── Outsource List ──
    [HttpGet("list")]
    public async Task<IActionResult> GetOutsourceList()
    {
        var outsources = await _db.TrnJobOutsources
            .Include(o => o.Job).ThenInclude(j => j.Party)
            .Include(o => o.TrnJobOutsourceItems)
            .OrderByDescending(o => o.OutsourceId)
            .ToListAsync();

        var vendorIds = outsources.Select(o => (int)o.VendorId).Distinct().ToList();
        var vendors = await _db.MstVendors
            .Include(v => v.Party)
            .Where(v => vendorIds.Contains(v.VendorId))
            .ToDictionaryAsync(v => v.VendorId);

        var list = outsources.Select(o =>
        {
            vendors.TryGetValue((int)o.VendorId, out var vendor);
            return new
            {
                o.OutsourceId,
                o.OutsourceNo,
                OutsourceDate = o.OutsourceDate.ToString("dd-MMM-yyyy"),
                o.JobId,
                JobNo = o.Job?.JobNo ?? "",
                CustomerName = o.Job?.Party?.Name ?? "",
                VendorName = vendor?.Party?.Name ?? "",
                VendorId = o.VendorId,
                o.ProcessType,
                o.TotalQuantity,
                o.TotalAmount,
                ExpectedDelivery = o.ExpectedDeliveryDate?.ToString("dd-MMM-yyyy") ?? "",
                ActualDelivery = o.ActualDeliveryDate?.ToString("dd-MMM-yyyy") ?? "",
                o.Status,
                ItemCount = o.TrnJobOutsourceItems.Count,
                CreatedOn = o.CreatedOn?.ToString("dd-MMM-yyyy HH:mm") ?? ""
            };
        }).ToList();

        return Ok(list);
    }

    // ── Outsource Detail ──
    [HttpGet("detail/{id:long}")]
    public async Task<IActionResult> GetOutsourceDetail(long id)
    {
        var os = await _db.TrnJobOutsources
            .Include(o => o.Job).ThenInclude(j => j!.Party)
            .Include(o => o.Job).ThenInclude(j => j!.Enquiry)
            .Include(o => o.Job).ThenInclude(j => j!.Quotation)
            .Include(o => o.TrnJobOutsourceItems).ThenInclude(i => i.JobItem)
            .Include(o => o.TrnOutsourceDispatches)
            .Include(o => o.TrnOutsourceReceives)
            .FirstOrDefaultAsync(o => o.OutsourceId == id);

        if (os == null)
            return NotFound(new { message = "Outsource order not found." });

        // Vendor info
        var vendor = await _db.MstVendors
            .Include(v => v.Party)
            .FirstOrDefaultAsync(v => v.VendorId == (int)os.VendorId);

        // Timeline
        var timeline = await _db.TrnOutsourceTimelines
            .Where(t => t.OutsourceId == id && t.IsActive == true)
            .OrderByDescending(t => t.CreatedOn)
            .ToListAsync();

        // Activity log
        var viewUser = HttpContext.Session.GetCurrentUser();
        if (viewUser != null)
        {
            var viewActivity = ActivityLogEntry.FromUser(viewUser, "OUTSOURCE", "VIEW", $"Viewed Outsource {os.OutsourceNo}");
            viewActivity.ActivityCategory = "NAVIGATION";
            viewActivity.EntityType = "OUTSOURCE";
            viewActivity.EntityId = os.OutsourceId;
            viewActivity.EntityCode = os.OutsourceNo;
            viewActivity.Description = $"Viewed outsource order {os.OutsourceNo} details.";
            await _activityService.LogActivityAsync(viewActivity);
        }

        var result = new
        {
            os.OutsourceId,
            os.OutsourceNo,
            OutsourceDate = os.OutsourceDate.ToString("dd-MMM-yyyy"),
            OutsourceDateIso = os.OutsourceDate.ToString("yyyy-MM-dd"),
            os.JobId,
            JobNo = os.Job?.JobNo,
            JobDate = os.Job?.JobDate.ToString("dd-MMM-yyyy"),
            JobStatus = os.Job?.StatusCode,
            CustomerName = os.Job?.Party?.Name,
            CustomerCode = os.Job?.Party?.Code,
            PartyId = os.Job?.PartyId,
            EnquiryNo = os.Job?.Enquiry?.EnquiryNo,
            QuotationNo = os.Job?.Quotation?.QuotationNo,
            os.VendorId,
            VendorName = vendor?.Party?.Name ?? "",
            VendorCode = vendor?.Party?.Code ?? "",
            VendorEmail = vendor?.Party?.Email ?? "",
            VendorGst = vendor?.Party?.Gstno ?? "",
            VendorAddress = vendor?.Party?.Address1 ?? "",
            ServiceArea = vendor?.ServiceArea ?? "",
            os.ProcessType,
            os.TotalQuantity,
            os.TotalAmount,
            ExpectedDeliveryDate = os.ExpectedDeliveryDate?.ToString("dd-MMM-yyyy"),
            ExpectedDeliveryIso = os.ExpectedDeliveryDate?.ToString("yyyy-MM-dd"),
            ActualDeliveryDate = os.ActualDeliveryDate?.ToString("dd-MMM-yyyy"),
            os.Status,
            os.Remarks,
            CreatedOn = os.CreatedOn?.ToString("dd-MMM-yyyy HH:mm"),
            Items = os.TrnJobOutsourceItems
                .OrderBy(i => i.ItemSequence)
                .Select(i => new
                {
                    i.OutsourceItemId,
                    i.ItemSequence,
                    i.ProductName,
                    i.ProcessName,
                    i.Quantity,
                    i.Rate,
                    i.Amount,
                    i.UomId,
                    i.Status,
                    i.Remarks,
                    i.JobItemId
                }),
            Dispatches = os.TrnOutsourceDispatches
                .OrderByDescending(d => d.DispatchDate)
                .Select(d => new
                {
                    d.DispatchId,
                    DispatchDate = d.DispatchDate?.ToString("dd-MMM-yyyy") ?? "",
                    d.ChallanNo,
                    d.TotalQuantity,
                    d.Remarks,
                    CreatedOn = d.CreatedOn?.ToString("dd-MMM-yyyy HH:mm") ?? ""
                }),
            Receives = os.TrnOutsourceReceives
                .OrderByDescending(r => r.ReceiveDate)
                .Select(r => new
                {
                    r.ReceiveId,
                    ReceiveDate = r.ReceiveDate?.ToString("dd-MMM-yyyy") ?? "",
                    r.ReceivedQuantity,
                    r.RejectedQuantity,
                    r.Remarks,
                    CreatedOn = r.CreatedOn?.ToString("dd-MMM-yyyy HH:mm") ?? ""
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
                t.VendorName,
                t.CommunicationMode,
                t.CommunicationReference,
                t.AttachmentUrl,
                t.DelayReason,
                ExpectedReturnDate = t.ExpectedReturnDate?.ToString("dd-MMM-yyyy"),
                ActualReturnDate = t.ActualReturnDate?.ToString("dd-MMM-yyyy"),
                CreatedOn = t.CreatedOn.ToString("dd-MMM-yyyy HH:mm")
            })
        };

        return Ok(result);
    }

    // ── Get Job Data for Outsource Creation ──
    [HttpGet("from-job/{jobId:long}")]
    public async Task<IActionResult> GetJobDataForOutsource(long jobId)
    {
        var job = await _db.TrnJobs
            .Include(j => j.Party)
            .Include(j => j.TrnJobItems)
            .FirstOrDefaultAsync(j => j.JobId == jobId);

        if (job == null)
            return NotFound(new { message = "Job not found." });

        // Already outsourced quantities
        var existingItems = await _db.TrnJobOutsourceItems
            .Where(oi => oi.Outsource.JobId == jobId && oi.Outsource.Status != "OUTSOURCE_CANCELLED")
            .GroupBy(oi => oi.JobItemId)
            .Select(g => new { JobItemId = g.Key, TotalOutsourced = g.Sum(oi => oi.Quantity) })
            .ToListAsync();

        var outsourcedMap = existingItems.ToDictionary(x => x.JobItemId, x => x.TotalOutsourced);

        var result = new
        {
            job.JobId,
            job.JobNo,
            job.PartyId,
            CustomerName = job.Party?.Name,
            CustomerCode = job.Party?.Code,
            job.ProductName,
            job.Quantity,
            job.NetAmount,
            Items = job.TrnJobItems
                .OrderBy(i => i.ItemSequence)
                .Select(i => new
                {
                    i.JobItemId,
                    i.ItemSequence,
                    i.ProductName,
                    i.ProductDescription,
                    JobQuantity = i.Quantity ?? 0,
                    AlreadyOutsourced = outsourcedMap.ContainsKey(i.JobItemId) ? outsourcedMap[i.JobItemId] : 0,
                    PendingQuantity = (i.Quantity ?? 0) - (outsourcedMap.ContainsKey(i.JobItemId) ? outsourcedMap[i.JobItemId] : 0),
                    i.UomId,
                    i.UnitRate,
                    i.NetAmount
                })
        };

        return Ok(result);
    }

    // ── Vendor List for Selection ──
    [HttpGet("vendors")]
    public async Task<IActionResult> GetVendors()
    {
        var vendors = await _db.MstVendors
            .Include(v => v.Party)
            .Include(v => v.VendorType)
            .Where(v => v.IsActive == true)
            .Select(v => new
            {
                v.VendorId,
                VendorName = v.Party != null ? v.Party.Name : "",
                VendorCode = v.Party != null ? v.Party.Code : "",
                VendorEmail = v.Party != null ? v.Party.Email : "",
                VendorMobile = v.Party != null ? v.Party.Mobile.ToString() : "",
                v.ServiceArea,
                VendorType = v.VendorType != null ? v.VendorType.Name : ""
            })
            .OrderBy(v => v.VendorName)
            .ToListAsync();

        return Ok(vendors);
    }

    // ── Save Outsource Order ──
    [HttpPost("save")]
    public async Task<IActionResult> SaveOutsource([FromBody] OutsourceSaveRequest request)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized(new { message = "Session expired. Please login again." });

        var outsourceNo = await _documentNumberService.GenerateNextNumberAsync(DocumentProcessCode.JOB_OUTSOURCE);

        var outsource = new TrnJobOutsource
        {
            OutsourceNo = outsourceNo,
            OutsourceDate = DateOnly.FromDateTime(DateTime.Now),
            JobId = request.JobId,
            VendorId = request.VendorId,
            ProcessType = request.ProcessType,
            TotalQuantity = request.TotalQuantity,
            TotalAmount = request.TotalAmount,
            ExpectedDeliveryDate = request.ExpectedDeliveryDate != null
                ? DateOnly.Parse(request.ExpectedDeliveryDate)
                : null,
            Status = OutsourceEventType.OUTSOURCE_CREATED.ToString(),
            Remarks = request.Remarks,
            CreatedBy = user.UserId,
            CreatedOn = DateTime.Now
        };

        _db.TrnJobOutsources.Add(outsource);
        await _db.SaveChangesAsync();

        // Save outsource items
        if (request.Items?.Count > 0)
        {
            foreach (var item in request.Items)
            {
                var osItem = new TrnJobOutsourceItem
                {
                    OutsourceId = outsource.OutsourceId,
                    JobItemId = item.JobItemId,
                    ItemSequence = item.ItemSequence,
                    ProductName = item.ProductName,
                    ProcessName = item.ProcessName,
                    Quantity = item.Quantity,
                    Rate = item.Rate,
                    Amount = item.Amount,
                    UomId = item.UomId,
                    Status = "PENDING",
                    Remarks = item.Remarks,
                    CreatedOn = DateTime.Now
                };
                _db.TrnJobOutsourceItems.Add(osItem);
            }
            await _db.SaveChangesAsync();
        }

        // Vendor name for notifications
        var vendor = await _db.MstVendors.Include(v => v.Party)
            .FirstOrDefaultAsync(v => v.VendorId == (int)request.VendorId);
        var vendorName = vendor?.Party?.Name ?? "Unknown Vendor";

        // ── Dispatch notification ──
        await DispatchOutsourceNotificationAsync(outsource, vendorName, user,
            OutsourceEventType.OUTSOURCE_CREATED,
            $"Outsource {outsource.OutsourceNo} created for vendor {vendorName}. Total Qty: {outsource.TotalQuantity}.");

        // ── Activity Log ──
        var activity = ActivityLogEntry.FromUser(user, "OUTSOURCE", "CREATE", $"Created Outsource {outsource.OutsourceNo}");
        activity.EntityType = "OUTSOURCE";
        activity.EntityId = outsource.OutsourceId;
        activity.EntityCode = outsource.OutsourceNo;
        activity.Description = $"Outsource {outsource.OutsourceNo} created for {vendorName} with {request.Items?.Count ?? 0} item(s). Qty: {outsource.TotalQuantity}.";
        activity.NewValues = JsonSerializer.Serialize(new { outsource.OutsourceNo, outsource.JobId, outsource.VendorId, vendorName, outsource.TotalQuantity, outsource.Status, ItemCount = request.Items?.Count ?? 0 });
        activity.Severity = "INFO";
        await _activityService.LogActivityAsync(activity);

        // ── Party Activity Log ──
        if (vendor?.PartyId > 0)
        {
            await PartyPortalController.LogPartyActivityAsync(_db, vendor.PartyId.Value,
                "OUTSOURCE", "OUTSOURCE_CREATED",
                $"Outsource {outsource.OutsourceNo} Assigned",
                $"Outsource order assigned with {request.Items?.Count ?? 0} item(s). Qty: {outsource.TotalQuantity}.",
                "trn_job_outsource", outsource.OutsourceId, outsource.OutsourceNo,
                outsource.OutsourceDate, "Pending", "Not Required", outsource.TotalAmount, user.Name);
        }

        // ── In-App Notification ──
        await _activityService.LogNotificationAsync(new UserNotificationEntry
        {
            UserId = user.UserId,
            Title = "Outsource Order Created",
            Message = $"Outsource {outsource.OutsourceNo} created for {vendorName}. Qty: {outsource.TotalQuantity}.",
            Icon = "bi bi-box-arrow-up-right",
            Color = "purple",
            Module = "OUTSOURCE",
            EventType = OutsourceEventType.OUTSOURCE_CREATED.ToString(),
            ReferenceId = (int)outsource.OutsourceId,
            ReferenceUrl = $"/Outsource/Details?id={outsource.OutsourceId}",
            Priority = "NORMAL"
        });

        // ── Outsource Timeline ──
        await AddOutsourceTimelineEntryAsync(
            outsource.OutsourceId, OutsourceEventType.OUTSOURCE_CREATED,
            "Outsource Order Created",
            $"Outsource {outsource.OutsourceNo} created for {vendorName} with {request.Items?.Count ?? 0} item(s). Total Qty: {outsource.TotalQuantity}.",
            newStatus: OutsourceEventType.OUTSOURCE_CREATED.ToString(),
            newQuantity: outsource.TotalQuantity,
            vendorName: vendorName,
            jobId: outsource.JobId, userId: user.UserId);

        // ── Job Timeline ──
        await AddJobTimelineEntryAsync(
            outsource.JobId, OutsourceEventType.OUTSOURCE_CREATED.ToString(), OutsourceEventType.OUTSOURCE_CREATED.ToString(),
            $"Outsource {outsource.OutsourceNo} Created",
            $"Job outsourced to {vendorName}. Outsource {outsource.OutsourceNo}, Qty: {outsource.TotalQuantity}.",
            newStatus: OutsourceEventType.OUTSOURCE_CREATED.ToString(), userId: user.UserId);

        return Ok(new { outsource.OutsourceId, outsource.OutsourceNo, message = "Outsource order saved successfully." });
    }

    // ── Update Outsource Status ──
    [HttpPost("updatestatus")]
    public async Task<IActionResult> UpdateOutsourceStatus([FromBody] OutsourceStatusRequest request)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized(new { message = "Session expired. Please login again." });

        if (!Enum.TryParse<OutsourceEventType>(request.Status, out var eventType))
            return BadRequest(new { message = $"Invalid outsource status: {request.Status}." });

        var os = await _db.TrnJobOutsources
            .Include(o => o.Job)
            .FirstOrDefaultAsync(o => o.OutsourceId == request.OutsourceId);

        if (os == null)
            return NotFound(new { message = "Outsource order not found." });

        var oldStatus = os.Status;
        os.Status = request.Status;
        os.ModifiedBy = user.UserId.ToString();
        os.ModifiedOn = DateTime.Now;

        // Update actual delivery date when material received
        if (eventType == OutsourceEventType.MATERIAL_RECEIVED)
            os.ActualDeliveryDate = DateOnly.FromDateTime(DateTime.Now);

        await _db.SaveChangesAsync();

        // Vendor name
        var vendor = await _db.MstVendors.Include(v => v.Party)
            .FirstOrDefaultAsync(v => v.VendorId == (int)os.VendorId);
        var vendorName = vendor?.Party?.Name ?? "Unknown";

        var eventLabel = GetEnumDescription(eventType);
        var (icon, color) = GetEventIconColor(eventType);

        // ── Dispatch notification ──
        await DispatchOutsourceNotificationAsync(os, vendorName, user, eventType,
            $"Outsource {os.OutsourceNo} — {eventLabel}. Status: {oldStatus ?? "N/A"} → {request.Status}.");

        // ── Activity Log ──
        var statusActivity = ActivityLogEntry.FromUser(user, "OUTSOURCE", "STATUS_CHANGE", $"Outsource {os.OutsourceNo} — {eventLabel}");
        statusActivity.EntityType = "OUTSOURCE";
        statusActivity.EntityId = os.OutsourceId;
        statusActivity.EntityCode = os.OutsourceNo;
        statusActivity.Description = $"Status changed from {oldStatus ?? "N/A"} to {request.Status} by {user.Name}.";
        statusActivity.OldValues = JsonSerializer.Serialize(new { Status = oldStatus });
        statusActivity.NewValues = JsonSerializer.Serialize(new { Status = request.Status });
        statusActivity.ChangedFields = ["Status"];
        statusActivity.Severity = eventType is OutsourceEventType.OUTSOURCE_CANCELLED or OutsourceEventType.RETURN_DELAYED or OutsourceEventType.REWORK_REQUIRED ? "WARNING" : "INFO";
        await _activityService.LogActivityAsync(statusActivity);

        // ── Party Activity Log: Status Change ──
        if (vendor?.PartyId > 0)
        {
            var osSt = eventType switch
            {
                OutsourceEventType.MATERIAL_RECEIVED => "Completed",
                OutsourceEventType.OUTSOURCE_CANCELLED => "Cancelled",
                _ => "Pending"
            };
            await PartyPortalController.LogPartyActivityAsync(_db, vendor.PartyId.Value,
                "OUTSOURCE", $"OUTSOURCE_{request.Status}",
                $"Outsource {os.OutsourceNo} — {eventLabel}",
                $"Status changed from {oldStatus ?? "N/A"} to {request.Status}.",
                "trn_job_outsource", os.OutsourceId, os.OutsourceNo,
                os.OutsourceDate, osSt, "Not Required", os.TotalAmount, user.Name);
        }

        // ── In-App Notification ──
        await _activityService.LogNotificationAsync(new UserNotificationEntry
        {
            UserId = user.UserId,
            Title = eventLabel,
            Message = $"Outsource {os.OutsourceNo} updated to {request.Status.ToLower().Replace('_', ' ')}.",
            Icon = icon,
            Color = color,
            Module = "OUTSOURCE",
            EventType = request.Status,
            ReferenceId = (int)os.OutsourceId,
            ReferenceUrl = $"/Outsource/Details?id={os.OutsourceId}"
        });

        // ── Outsource Timeline ──
        await AddOutsourceTimelineEntryAsync(
            os.OutsourceId, eventType, eventLabel,
            $"Status changed from {oldStatus ?? "N/A"} to {request.Status} by {user.Name}.",
            oldStatus: oldStatus, newStatus: request.Status,
            vendorName: vendorName, processCode: request.Status,
            processName: request.Status.Replace('_', ' '),
            jobId: os.JobId, userId: user.UserId);

        // ── Job Timeline ──
        await AddJobTimelineEntryAsync(
            os.JobId, request.Status, request.Status,
            $"Outsource {os.OutsourceNo} — {eventLabel}",
            $"Outsource {os.OutsourceNo}: {oldStatus ?? "N/A"} → {request.Status}.",
            oldStatus: oldStatus, newStatus: request.Status,
            userId: user.UserId);

        return Ok(new { message = $"Outsource status updated to {eventLabel}." });
    }

    // ── Record Dispatch ──
    [HttpPost("dispatch")]
    public async Task<IActionResult> RecordDispatch([FromBody] OutsourceDispatchRequest request)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized(new { message = "Session expired. Please login again." });

        var os = await _db.TrnJobOutsources
            .FirstOrDefaultAsync(o => o.OutsourceId == request.OutsourceId);

        if (os == null)
            return NotFound(new { message = "Outsource order not found." });

        var dispatch = new TrnOutsourceDispatch
        {
            OutsourceId = request.OutsourceId,
            DispatchDate = DateOnly.FromDateTime(DateTime.Now),
            ChallanNo = request.ChallanNo,
            TotalQuantity = request.TotalQuantity,
            Remarks = request.Remarks,
            CreatedOn = DateTime.Now
        };

        _db.TrnOutsourceDispatches.Add(dispatch);

        // Update status to MATERIAL_SENT if still in created/assigned state
        if (os.Status is "OUTSOURCE_CREATED" or "VENDOR_ASSIGNED")
        {
            os.Status = OutsourceEventType.MATERIAL_SENT.ToString();
            os.ModifiedBy = user.UserId.ToString();
            os.ModifiedOn = DateTime.Now;
        }

        await _db.SaveChangesAsync();

        var vendor = await _db.MstVendors.Include(v => v.Party)
            .FirstOrDefaultAsync(v => v.VendorId == (int)os.VendorId);
        var vendorName = vendor?.Party?.Name ?? "Unknown";

        // ── Activity + Timeline + Notification ──
        var activity = ActivityLogEntry.FromUser(user, "OUTSOURCE", "DISPATCH", $"Material dispatched for {os.OutsourceNo}");
        activity.EntityType = "OUTSOURCE";
        activity.EntityId = os.OutsourceId;
        activity.EntityCode = os.OutsourceNo;
        activity.Description = $"Material dispatched to {vendorName}. Challan: {request.ChallanNo ?? "N/A"}, Qty: {request.TotalQuantity}.";
        activity.Severity = "INFO";
        await _activityService.LogActivityAsync(activity);

        // ── In-App Notification ──
        await _activityService.LogNotificationAsync(new UserNotificationEntry
        {
            UserId = user.UserId,
            Title = "Material Dispatched",
            Message = $"Outsource {os.OutsourceNo}: Challan {request.ChallanNo ?? "N/A"}, Qty {request.TotalQuantity} sent to {vendorName}.",
            Icon = "bi bi-send",
            Color = "purple",
            Module = "OUTSOURCE",
            EventType = OutsourceEventType.MATERIAL_SENT.ToString(),
            ReferenceId = (int)os.OutsourceId,
            ReferenceUrl = $"/Outsource/Details?id={os.OutsourceId}"
        });

        await AddOutsourceTimelineEntryAsync(
            os.OutsourceId, OutsourceEventType.MATERIAL_SENT,
            "Material Dispatched to Vendor",
            $"Material sent to {vendorName}. Challan: {request.ChallanNo ?? "N/A"}, Qty: {request.TotalQuantity}.",
            newStatus: OutsourceEventType.MATERIAL_SENT.ToString(),
            newQuantity: request.TotalQuantity,
            vendorName: vendorName, movementType: "OUT",
            jobId: os.JobId, userId: user.UserId);

        // ── Job Timeline ──
        await AddJobTimelineEntryAsync(
            os.JobId,
            OutsourceEventType.MATERIAL_SENT.ToString(),
            OutsourceEventType.MATERIAL_SENT.ToString(),
            $"Outsource {os.OutsourceNo} — Material Dispatched",
            $"Material dispatched to vendor {vendorName}. Challan: {request.ChallanNo ?? "N/A"}, Qty: {request.TotalQuantity}.",
            newStatus: OutsourceEventType.MATERIAL_SENT.ToString(),
            userId: user.UserId);

        await DispatchOutsourceNotificationAsync(os, vendorName, user, OutsourceEventType.MATERIAL_SENT,
            $"Material dispatched to {vendorName}. Challan: {request.ChallanNo ?? "N/A"}, Qty: {request.TotalQuantity}.");

        return Ok(new { dispatch.DispatchId, message = "Dispatch recorded successfully." });
    }

    // ── Record Receive ──
    [HttpPost("receive")]
    public async Task<IActionResult> RecordReceive([FromBody] OutsourceReceiveRequest request)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized(new { message = "Session expired. Please login again." });

        var os = await _db.TrnJobOutsources
            .FirstOrDefaultAsync(o => o.OutsourceId == request.OutsourceId);

        if (os == null)
            return NotFound(new { message = "Outsource order not found." });

        var receive = new TrnOutsourceReceive
        {
            OutsourceId = request.OutsourceId,
            ReceiveDate = DateOnly.FromDateTime(DateTime.Now),
            ReceivedQuantity = request.ReceivedQuantity,
            RejectedQuantity = request.RejectedQuantity,
            Remarks = request.Remarks,
            CreatedOn = DateTime.Now
        };

        _db.TrnOutsourceReceives.Add(receive);

        // Auto-update status
        os.Status = OutsourceEventType.MATERIAL_RECEIVED.ToString();
        os.ActualDeliveryDate = DateOnly.FromDateTime(DateTime.Now);
        os.ModifiedBy = user.UserId.ToString();
        os.ModifiedOn = DateTime.Now;

        await _db.SaveChangesAsync();

        var vendor = await _db.MstVendors.Include(v => v.Party)
            .FirstOrDefaultAsync(v => v.VendorId == (int)os.VendorId);
        var vendorName = vendor?.Party?.Name ?? "Unknown";

        // ── Activity + Timeline ──
        var activity = ActivityLogEntry.FromUser(user, "OUTSOURCE", "RECEIVE", $"Material received for {os.OutsourceNo}");
        activity.EntityType = "OUTSOURCE";
        activity.EntityId = os.OutsourceId;
        activity.EntityCode = os.OutsourceNo;
        activity.Description = $"Received from {vendorName}. Good: {request.ReceivedQuantity}, Rejected: {request.RejectedQuantity ?? 0}.";
        activity.Severity = (request.RejectedQuantity ?? 0) > 0 ? "WARNING" : "INFO";
        await _activityService.LogActivityAsync(activity);

        await _activityService.LogNotificationAsync(new UserNotificationEntry
        {
            UserId = user.UserId,
            Title = "Material Received",
            Message = $"Outsource {os.OutsourceNo}: Received {request.ReceivedQuantity} from {vendorName}.",
            Icon = "bi bi-box-arrow-in-down",
            Color = "success",
            Module = "OUTSOURCE",
            EventType = OutsourceEventType.MATERIAL_RECEIVED.ToString(),
            ReferenceId = (int)os.OutsourceId,
            ReferenceUrl = $"/Outsource/Details?id={os.OutsourceId}"
        });

        await AddOutsourceTimelineEntryAsync(
            os.OutsourceId, OutsourceEventType.MATERIAL_RECEIVED,
            "Material Received from Vendor",
            $"Received from {vendorName}. Good: {request.ReceivedQuantity}, Rejected: {request.RejectedQuantity ?? 0}.",
            newStatus: OutsourceEventType.MATERIAL_RECEIVED.ToString(),
            newQuantity: request.ReceivedQuantity,
            vendorName: vendorName, movementType: "IN",
            jobId: os.JobId, userId: user.UserId);

        // ── Job Timeline ──
        await AddJobTimelineEntryAsync(
            os.JobId,
            OutsourceEventType.MATERIAL_RECEIVED.ToString(),
            OutsourceEventType.MATERIAL_RECEIVED.ToString(),
            $"Outsource {os.OutsourceNo} — Material Received",
            $"Material received from vendor {vendorName}. Good: {request.ReceivedQuantity}, Rejected: {request.RejectedQuantity ?? 0}.",
            newStatus: OutsourceEventType.MATERIAL_RECEIVED.ToString(),
            userId: user.UserId);

        await DispatchOutsourceNotificationAsync(os, vendorName, user, OutsourceEventType.MATERIAL_RECEIVED,
            $"Material received from {vendorName}. Good: {request.ReceivedQuantity}, Rejected: {request.RejectedQuantity ?? 0}.");

        return Ok(new { receive.ReceiveId, message = "Receive recorded successfully." });
    }

    // ── Delete Outsource ──
    [HttpDelete("delete/{id:long}")]
    public async Task<IActionResult> DeleteOutsource(long id)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized(new { message = "Session expired. Please login again." });

        var os = await _db.TrnJobOutsources
            .Include(o => o.TrnJobOutsourceItems)
            .FirstOrDefaultAsync(o => o.OutsourceId == id);

        if (os == null)
            return NotFound(new { message = "Outsource order not found." });

        if (os.Status != OutsourceEventType.OUTSOURCE_CREATED.ToString())
            return BadRequest(new { message = "Only newly created outsource orders can be deleted." });

        var osNo = os.OutsourceNo;

        _db.TrnJobOutsourceItems.RemoveRange(os.TrnJobOutsourceItems);
        _db.TrnJobOutsources.Remove(os);
        await _db.SaveChangesAsync();

        var activity = ActivityLogEntry.FromUser(user, "OUTSOURCE", "DELETE", $"Deleted Outsource {osNo}");
        activity.EntityType = "OUTSOURCE";
        activity.EntityId = id;
        activity.EntityCode = osNo;
        activity.Description = $"Outsource {osNo} deleted by {user.Name}.";
        activity.Severity = "WARNING";
        await _activityService.LogActivityAsync(activity);

        return Ok(new { message = "Outsource order deleted successfully." });
    }

    // ── Outsource Timeline ──
    [HttpGet("timeline/{outsourceId:long}")]
    public async Task<IActionResult> GetOutsourceTimeline(long outsourceId)
    {
        var timeline = await _db.TrnOutsourceTimelines
            .Where(t => t.OutsourceId == outsourceId && t.IsActive == true)
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
                t.VendorName,
                t.DelayReason,
                ExpectedReturnDate = t.ExpectedReturnDate != null ? t.ExpectedReturnDate.Value.ToString("dd-MMM-yyyy") : null,
                ActualReturnDate = t.ActualReturnDate != null ? t.ActualReturnDate.Value.ToString("dd-MMM-yyyy") : null,
                t.CommunicationMode,
                t.CommunicationReference,
                t.AttachmentUrl,
                CreatedOn = t.CreatedOn.ToString("dd-MMM-yyyy HH:mm"),
                CreatedOnIso = t.CreatedOn.ToString("yyyy-MM-ddTHH:mm:ss")
            })
            .ToListAsync();

        return Ok(timeline);
    }

    // ── Company Info (for Print page) ──
    [HttpGet("company-info")]
    public async Task<IActionResult> GetCompanyInfo()
    {
        var user = HttpContext.Session.GetCurrentUser();
        var companyId = user?.CompanyId ?? 1;

        var company = await _db.MstCompanies
            .Where(c => c.Id == companyId)
            .Select(c => new
            {
                c.Name,
                c.LegalName,
                c.Gstin,
                c.PanNo,
                c.AddressLine1,
                c.AddressLine2,
                c.Pincode,
                c.ContactPerson,
                c.ContactNo,
                c.EmailId,
                c.Website,
                c.LogoUrl
            })
            .FirstOrDefaultAsync();

        return company == null ? NotFound() : Ok(company);
    }

    // ── Email Outsource to Vendor ──
    [HttpPost("send-email-vendor")]
    public async Task<IActionResult> SendEmailToVendor([FromBody] OutsourceEmailRequest request)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized();

        var os = await _db.TrnJobOutsources
            .Include(o => o.Job).ThenInclude(j => j!.Party)
            .Include(o => o.TrnJobOutsourceItems)
            .FirstOrDefaultAsync(o => o.OutsourceId == request.OutsourceId);

        if (os == null)
            return NotFound(new { message = "Outsource order not found." });

        var vendor = await _db.MstVendors
            .Include(v => v.Party)
            .FirstOrDefaultAsync(v => v.VendorId == (int)os.VendorId);

        var vendorEmail = vendor?.Party?.Email;
        var vendorName = vendor?.Party?.Name ?? "Vendor";

        if (string.IsNullOrWhiteSpace(vendorEmail))
            return BadRequest(new { message = "Vendor does not have an email address configured." });

        // Company info for header
        var companyId = user.CompanyId ?? 1;
        var company = await _db.MstCompanies.FirstOrDefaultAsync(c => c.Id == companyId);
        var companyName = company?.Name ?? "MinePress";

        // Build items table rows
        var items = os.TrnJobOutsourceItems.OrderBy(i => i.ItemSequence).ToList();
        var itemRows = string.Join("\n", items.Select((item, idx) =>
            $"<tr><td style='padding:8px;border:1px solid #dee2e6;text-align:center;'>{idx + 1}</td>" +
            $"<td style='padding:8px;border:1px solid #dee2e6;'>{Esc(item.ProductName)}</td>" +
            $"<td style='padding:8px;border:1px solid #dee2e6;'>{Esc(item.ProcessName)}</td>" +
            $"<td style='padding:8px;border:1px solid #dee2e6;text-align:right;'>{item.Quantity:N2}</td>" +
            $"<td style='padding:8px;border:1px solid #dee2e6;text-align:right;'>{item.Rate:N2}</td>" +
            $"<td style='padding:8px;border:1px solid #dee2e6;text-align:right;'>{item.Amount:N2}</td></tr>"));

        var customerName = os.Job?.Party?.Name ?? "—";
        var jobNo = os.Job?.JobNo ?? "—";

        // Build email HTML
        var htmlBody = $"""
            <div style="font-family:Segoe UI,Arial,sans-serif;max-width:700px;margin:0 auto;">
                <div style="background:linear-gradient(135deg,#6f42c1,#8b5cf6);padding:24px 32px;border-radius:8px 8px 0 0;">
                    <h1 style="color:#fff;margin:0;font-size:22px;">{Esc(companyName)}</h1>
                    <p style="color:rgba(255,255,255,.8);margin:4px 0 0;font-size:13px;">Outsource Order</p>
                </div>
                <div style="padding:24px 32px;border:1px solid #e9ecef;border-top:none;border-radius:0 0 8px 8px;">
                    <table style="width:100%;margin-bottom:20px;">
                        <tr>
                            <td style="vertical-align:top;width:50%;">
                                <p style="margin:0 0 4px;color:#6c757d;font-size:12px;">OUTSOURCE NO</p>
                                <p style="margin:0;font-size:18px;font-weight:600;color:#6f42c1;">{Esc(os.OutsourceNo)}</p>
                            </td>
                            <td style="vertical-align:top;width:50%;text-align:right;">
                                <p style="margin:0 0 4px;color:#6c757d;font-size:12px;">DATE</p>
                                <p style="margin:0;font-weight:600;">{os.OutsourceDate:dd-MMM-yyyy}</p>
                            </td>
                        </tr>
                    </table>

                    <table style="width:100%;margin-bottom:20px;">
                        <tr>
                            <td style="vertical-align:top;width:50%;padding-right:16px;">
                                <p style="margin:0 0 6px;font-weight:600;color:#6f42c1;font-size:13px;">VENDOR</p>
                                <p style="margin:0;font-weight:600;">{Esc(vendorName)}</p>
                                <p style="margin:2px 0;color:#6c757d;font-size:13px;">{Esc(vendor?.Party?.Address1 ?? "")}</p>
                                <p style="margin:2px 0;color:#6c757d;font-size:13px;">GST: {Esc(vendor?.Party?.Gstno ?? "N/A")}</p>
                            </td>
                            <td style="vertical-align:top;width:50%;padding-left:16px;">
                                <p style="margin:0 0 6px;font-weight:600;color:#6f42c1;font-size:13px;">JOB REFERENCE</p>
                                <p style="margin:0;font-weight:600;">{Esc(jobNo)}</p>
                                <p style="margin:2px 0;color:#6c757d;font-size:13px;">Customer: {Esc(customerName)}</p>
                                <p style="margin:2px 0;color:#6c757d;font-size:13px;">Process: {Esc(os.ProcessType ?? "—")}</p>
                            </td>
                        </tr>
                    </table>

                    <table style="width:100%;border-collapse:collapse;margin-bottom:20px;">
                        <thead>
                            <tr style="background:#f8f9fa;">
                                <th style="padding:8px;border:1px solid #dee2e6;text-align:center;font-size:12px;">#</th>
                                <th style="padding:8px;border:1px solid #dee2e6;font-size:12px;">Product</th>
                                <th style="padding:8px;border:1px solid #dee2e6;font-size:12px;">Process</th>
                                <th style="padding:8px;border:1px solid #dee2e6;text-align:right;font-size:12px;">Qty</th>
                                <th style="padding:8px;border:1px solid #dee2e6;text-align:right;font-size:12px;">Rate</th>
                                <th style="padding:8px;border:1px solid #dee2e6;text-align:right;font-size:12px;">Amount</th>
                            </tr>
                        </thead>
                        <tbody>
                            {itemRows}
                        </tbody>
                        <tfoot>
                            <tr style="background:#f8f9fa;font-weight:600;">
                                <td colspan="3" style="padding:8px;border:1px solid #dee2e6;">Total</td>
                                <td style="padding:8px;border:1px solid #dee2e6;text-align:right;">{os.TotalQuantity:N2}</td>
                                <td style="padding:8px;border:1px solid #dee2e6;"></td>
                                <td style="padding:8px;border:1px solid #dee2e6;text-align:right;">{os.TotalAmount:N2}</td>
                            </tr>
                        </tfoot>
                    </table>

                    {(string.IsNullOrWhiteSpace(os.Remarks) ? "" : $"<div style='padding:12px;background:#f8f9fa;border-radius:6px;margin-bottom:20px;'><p style='margin:0 0 4px;font-weight:600;font-size:12px;color:#6c757d;'>REMARKS</p><p style='margin:0;'>{Esc(os.Remarks)}</p></div>")}

                    {(os.ExpectedDeliveryDate.HasValue ? $"<p style='color:#6c757d;font-size:13px;'>Expected Delivery: <strong>{os.ExpectedDeliveryDate.Value:dd-MMM-yyyy}</strong></p>" : "")}

                    <hr style="border:none;border-top:1px solid #e9ecef;margin:20px 0;">
                    <p style="color:#6c757d;font-size:12px;margin:0;">This is a system-generated email from {Esc(companyName)}. Please do not reply directly to this email.</p>
                </div>
            </div>
            """;

        try
        {
            var eventLabel = GetEnumDescription(OutsourceEventType.EMAIL_SENT);

            var config = new ProcessNotificationConfig
            {
                ConfigId = 1,
                ProcessCode = nameof(ProcessCode.EnqJob),
                SubProcessCode = notification.Enums.SubProcessCode.ReceiveEnq,
                EventType = NotificationEventType.TaskAssign,
                EventLabel = eventLabel,
                RecipientType = RecipientType.Both,
                NotifyAssignee = true,
                NotifyInternalEmail = true,
                NotifyPush = false,
                TemplateCode = nameof(NotificationTemplateCode.TaskAssigned),
                Priority = NotificationPriority.Normal,
                IsActive = true,
                TriggerOnStatus = os.Status,
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
                SubjectTemplate = $"Outsource Order {os.OutsourceNo} — {Esc(companyName)}",
                BodyTemplate = htmlBody,
                IsActive = true
            };

            var context = new NotificationContext
            {
                AssigneeUserId = (int)user.UserId,
                AssigneeEmail = vendorEmail,
                AssigneePhone = user.MobileNo,
                Variables = new Dictionary<string, string>
                {
                    ["outsource_no"] = os.OutsourceNo,
                    ["vendor_name"] = vendorName,
                    ["status"] = os.Status ?? "N/A",
                    ["total_qty"] = os.TotalQuantity?.ToString("N0") ?? "0",
                    ["updated_by"] = user.Name,
                    ["outsource_date"] = os.OutsourceDate.ToString("dd-MMM-yyyy")
                }
            };

            await _notificationDispatcher.DispatchAsync(config, template, context);

            // ── Outsource Timeline ──
            await AddOutsourceTimelineEntryAsync(
                os.OutsourceId, OutsourceEventType.EMAIL_SENT,
                "Outsource Order Emailed to Vendor",
                $"Outsource {os.OutsourceNo} emailed to {vendorName} ({vendorEmail}).",
                vendorName: vendorName,
                communicationMode: "EMAIL",
                communicationReference: vendorEmail,
                jobId: os.JobId,
                userId: user.UserId);

            // ── Job Timeline ──
            await AddJobTimelineEntryAsync(
                os.JobId,
                OutsourceEventType.EMAIL_SENT.ToString(),
                OutsourceEventType.EMAIL_SENT.ToString(),
                "Outsource Order Emailed to Vendor",
                $"Outsource {os.OutsourceNo} emailed to vendor {vendorName}.",
                communicationMode: "EMAIL",
                communicationReference: vendorEmail,
                userId: user.UserId);

            // ── User Activity ──
            var activity = ActivityLogEntry.FromUser(user, "OUTSOURCE", "EMAIL_SENT", $"Emailed outsource {os.OutsourceNo} to vendor");
            activity.EntityType = "OUTSOURCE";
            activity.EntityId = os.OutsourceId;
            activity.EntityCode = os.OutsourceNo;
            activity.Description = $"Outsource {os.OutsourceNo} emailed to {vendorName} ({vendorEmail}) by {user.Name}.";
            await _activityService.LogActivityAsync(activity);

            _logger.LogInformation("Outsource {OutsourceNo} emailed to vendor {VendorName} ({VendorEmail})",
                os.OutsourceNo, vendorName, vendorEmail);

            return Ok(new { message = $"Outsource order emailed to {vendorName} successfully.", email = vendorEmail });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to email outsource {OutsourceNo} to vendor", os.OutsourceNo);
            return StatusCode(500, new { message = "Failed to send email to vendor. Please try again." });
        }
    }

    private static string Esc(string? value) => System.Net.WebUtility.HtmlEncode(value ?? "");

    // ── Vendor 360° Dashboard ──
    [HttpGet("vendor-activities/{vendorId:int}")]
    public async Task<IActionResult> GetVendorActivities(int vendorId)
    {
        // Vendor profile via Party
        var vendor = await _db.MstVendors
            .Include(v => v.Party)
            .Include(v => v.VendorType)
            .Where(v => v.VendorId == vendorId)
            .Select(v => new
            {
                v.VendorId,
                Name = v.Party != null ? v.Party.Name : "",
                Code = v.Party != null ? v.Party.Code : "",
                Email = v.Party != null ? v.Party.Email : "",
                Mobile = v.Party != null && v.Party.Mobile.HasValue ? v.Party.Mobile.Value.ToString() : "",
                GstNo = v.Party != null ? v.Party.Gstno : "",
                Address = v.Party != null ? v.Party.Address1 : "",
                v.ServiceArea,
                VendorType = v.VendorType != null ? v.VendorType.Name : "",
                ContractStart = v.ContractStartDate.HasValue ? v.ContractStartDate.Value.ToString("dd-MMM-yyyy") : null,
                ContractEnd = v.ContractEndDate.HasValue ? v.ContractEndDate.Value.ToString("dd-MMM-yyyy") : null,
                v.ContractValue,
                CreatedOn = v.CreatedOn.HasValue ? v.CreatedOn.Value.ToString("dd-MMM-yyyy") : ""
            })
            .FirstOrDefaultAsync();

        if (vendor == null)
            return NotFound(new { message = "Vendor not found." });

        // Outsource orders for this vendor
        var outsourceOrders = await _db.TrnJobOutsources
            .Include(o => o.Job).ThenInclude(j => j.Party)
            .Where(o => o.VendorId == vendorId)
            .OrderByDescending(o => o.OutsourceDate)
            .Take(15)
            .Select(o => new
            {
                o.OutsourceId,
                o.OutsourceNo,
                Date = o.OutsourceDate.ToString("dd-MMM-yyyy"),
                o.ProcessType,
                o.TotalQuantity,
                o.TotalAmount,
                o.Status,
                JobNo = o.Job != null ? o.Job.JobNo : "",
                CustomerName = o.Job != null && o.Job.Party != null ? o.Job.Party.Name : ""
            })
            .ToListAsync();

        // Dispatches for this vendor's outsource orders
        var vendorOutsourceIds = await _db.TrnJobOutsources
            .Where(o => o.VendorId == vendorId)
            .Select(o => o.OutsourceId)
            .ToListAsync();

        var dispatches = await _db.TrnOutsourceDispatches
            .Include(d => d.Outsource)
            .Where(d => vendorOutsourceIds.Contains(d.OutsourceId))
            .OrderByDescending(d => d.DispatchDate)
            .Take(15)
            .Select(d => new
            {
                d.DispatchId,
                DispatchDate = d.DispatchDate != null ? d.DispatchDate.Value.ToString("dd-MMM-yyyy") : "",
                d.ChallanNo,
                d.TotalQuantity,
                d.Remarks,
                OutsourceNo = d.Outsource != null ? d.Outsource.OutsourceNo : ""
            })
            .ToListAsync();

        // Receives for this vendor's outsource orders
        var receives = await _db.TrnOutsourceReceives
            .Include(r => r.Outsource)
            .Where(r => vendorOutsourceIds.Contains(r.OutsourceId))
            .OrderByDescending(r => r.ReceiveDate)
            .Take(15)
            .Select(r => new
            {
                r.ReceiveId,
                ReceiveDate = r.ReceiveDate != null ? r.ReceiveDate.Value.ToString("dd-MMM-yyyy") : "",
                r.ReceivedQuantity,
                r.RejectedQuantity,
                r.Remarks,
                OutsourceNo = r.Outsource != null ? r.Outsource.OutsourceNo : ""
            })
            .ToListAsync();

        // Payments to this vendor (via party)
        var partyId = await _db.MstVendors
            .Where(v => v.VendorId == vendorId)
            .Select(v => v.PartyId)
            .FirstOrDefaultAsync();

        var payments = partyId.HasValue
            ? await _db.TrnPayments
                .Where(p => p.PartyId == partyId.Value)
                .OrderByDescending(p => p.PaymentDate)
                .Take(15)
                .Select(p => new
                {
                    p.PaymentId,
                    p.PaymentNo,
                    Date = p.PaymentDate.ToString("dd-MMM-yyyy"),
                    p.PaymentMode,
                    p.Amount,
                    p.Status
                })
                .ToListAsync()
            : [];

        // Summary / KPIs
        var totalOrders = await _db.TrnJobOutsources.CountAsync(o => o.VendorId == vendorId);
        var totalDispatches = await _db.TrnOutsourceDispatches.CountAsync(d => vendorOutsourceIds.Contains(d.OutsourceId));
        var totalReceives = await _db.TrnOutsourceReceives.CountAsync(r => vendorOutsourceIds.Contains(r.OutsourceId));
        var totalPayments = partyId.HasValue
            ? await _db.TrnPayments.CountAsync(p => p.PartyId == partyId.Value)
            : 0;

        var totalOutsourceValue = await _db.TrnJobOutsources
            .Where(o => o.VendorId == vendorId && o.Status != "OUTSOURCE_CANCELLED")
            .SumAsync(o => o.TotalAmount ?? 0);

        var totalDispatchedQty = await _db.TrnOutsourceDispatches
            .Where(d => vendorOutsourceIds.Contains(d.OutsourceId))
            .SumAsync(d => d.TotalQuantity ?? 0);

        var totalReceivedQty = await _db.TrnOutsourceReceives
            .Where(r => vendorOutsourceIds.Contains(r.OutsourceId))
            .SumAsync(r => r.ReceivedQuantity ?? 0);

        var totalRejectedQty = await _db.TrnOutsourceReceives
            .Where(r => vendorOutsourceIds.Contains(r.OutsourceId))
            .SumAsync(r => r.RejectedQuantity ?? 0);

        var totalPaymentAmount = partyId.HasValue
            ? await _db.TrnPayments
                .Where(p => p.PartyId == partyId.Value && p.Status != "CANCELLED")
                .SumAsync(p => p.Amount)
            : 0m;

        var activeOrders = await _db.TrnJobOutsources
            .CountAsync(o => o.VendorId == vendorId
                && o.Status != "OUTSOURCE_CLOSED"
                && o.Status != "OUTSOURCE_CANCELLED");

        var completedOrders = await _db.TrnJobOutsources
            .CountAsync(o => o.VendorId == vendorId
                && (o.Status == "OUTSOURCE_CLOSED" || o.Status == "MATERIAL_RECEIVED" || o.Status == "PAYMENT_COMPLETED"));

        var reworkOrders = await _db.TrnJobOutsources
            .CountAsync(o => o.VendorId == vendorId
                && (o.Status == "REWORK_REQUIRED" || o.Status == "REWORK_SENT"));

        var delayedOrders = await _db.TrnJobOutsources
            .CountAsync(o => o.VendorId == vendorId && o.Status == "RETURN_DELAYED");

        return Ok(new
        {
            vendor,
            outsourceOrders,
            dispatches,
            receives,
            payments,
            summary = new
            {
                totalOrders,
                activeOrders,
                completedOrders,
                reworkOrders,
                delayedOrders,
                totalDispatches,
                totalReceives,
                totalPayments,
                totalOutsourceValue,
                totalDispatchedQty,
                totalReceivedQty,
                totalRejectedQty,
                totalPaymentAmount,
                pendingAmount = totalOutsourceValue - totalPaymentAmount
            }
        });
    }

    // ══════════════════════════════════════════
    //  HELPERS
    // ══════════════════════════════════════════

    private static string GetEnumDescription(OutsourceEventType value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attr = field?.GetCustomAttribute<DescriptionAttribute>();
        return attr?.Description ?? value.ToString().Replace('_', ' ');
    }

    private static (string icon, string color) GetEventIconColor(OutsourceEventType eventType)
    {
        return eventType switch
        {
            OutsourceEventType.OUTSOURCE_CREATED   => ("bi bi-box-arrow-up-right", "purple"),
            OutsourceEventType.VENDOR_ASSIGNED      => ("bi bi-person-check", "info"),
            OutsourceEventType.MATERIAL_SENT        => ("bi bi-send", "blue"),
            OutsourceEventType.VENDOR_ACKNOWLEDGED  => ("bi bi-hand-thumbs-up", "teal"),
            OutsourceEventType.PROCESS_STARTED      => ("bi bi-gear-wide-connected", "cyan"),
            OutsourceEventType.PROCESS_COMPLETED    => ("bi bi-check2-circle", "azure"),
            OutsourceEventType.QUALITY_CHECKED      => ("bi bi-shield-check", "success"),
            OutsourceEventType.MATERIAL_RECEIVED    => ("bi bi-box-arrow-in-down", "success"),
            OutsourceEventType.RETURN_DELAYED       => ("bi bi-exclamation-triangle", "warning"),
            OutsourceEventType.REWORK_REQUIRED      => ("bi bi-arrow-repeat", "orange"),
            OutsourceEventType.REWORK_SENT          => ("bi bi-arrow-return-right", "orange"),
            OutsourceEventType.REWORK_COMPLETED     => ("bi bi-check-circle", "teal"),
            OutsourceEventType.PAYMENT_INITIATED    => ("bi bi-currency-rupee", "yellow"),
            OutsourceEventType.PAYMENT_COMPLETED    => ("bi bi-cash-stack", "success"),
            OutsourceEventType.OUTSOURCE_CLOSED     => ("bi bi-lock", "dark"),
            OutsourceEventType.OUTSOURCE_CANCELLED  => ("bi bi-x-circle", "danger"),
            _                                       => ("bi bi-arrow-repeat", "primary")
        };
    }

    private async Task DispatchOutsourceNotificationAsync(TrnJobOutsource outsource, string vendorName, UserSessionData user, OutsourceEventType eventType, string bodyText)
    {
        try
        {
            var eventLabel = GetEnumDescription(eventType);

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
                TriggerOnStatus = outsource.Status,
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
                SubjectTemplate = $"Outsource {{{{outsource_no}}}} — {eventType}",
                BodyTemplate = $$$"""
                    <h3>{{{eventLabel}}}</h3>
                    <p><strong>Outsource No:</strong> {{outsource_no}}</p>
                    <p><strong>Vendor:</strong> {{vendor_name}}</p>
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
                    ["outsource_no"] = outsource.OutsourceNo,
                    ["vendor_name"] = vendorName,
                    ["status"] = outsource.Status ?? "N/A",
                    ["total_qty"] = outsource.TotalQuantity?.ToString("N0") ?? "0",
                    ["updated_by"] = user.Name,
                    ["outsource_date"] = outsource.OutsourceDate.ToString("dd-MMM-yyyy")
                }
            };

            var results = await _notificationDispatcher.DispatchAsync(config, template, context);
            _logger.LogInformation("Outsource {OutsourceNo} {Event}: Dispatched {Count} notifications",
                outsource.OutsourceNo, eventType, results.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to dispatch notification for outsource {OutsourceNo}", outsource.OutsourceNo);
        }
    }

    private async Task AddOutsourceTimelineEntryAsync(
        long outsourceId, OutsourceEventType eventType,
        string eventTitle, string? eventDescription,
        string? oldStatus = null, string? newStatus = null,
        decimal? oldQuantity = null, decimal? newQuantity = null,
        decimal? oldAmount = null, decimal? newAmount = null,
        string? remarks = null, string? vendorName = null,
        string? processCode = null, string? processName = null,
        string? movementType = null, string? communicationMode = null,
        string? communicationReference = null,
        long? jobId = null, long userId = 0)
    {
        try
        {
            var entry = new TrnOutsourceTimeline
            {
                OutsourceId = outsourceId,
                JobId = jobId,
                EventType = eventType.ToString(),
                EventCode = eventType.ToString(),
                EventTitle = eventTitle,
                EventDescription = eventDescription,
                Remarks = remarks,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                OldQuantity = oldQuantity,
                NewQuantity = newQuantity,
                OldAmount = oldAmount,
                NewAmount = newAmount,
                VendorName = vendorName,
                MovementType = movementType,
                ProcessCode = processCode,
                ProcessName = processName,
                CommunicationMode = communicationMode,
                CommunicationReference = communicationReference,
                CreatedBy = userId,
                CreatedOn = DateTime.Now,
                IsActive = true
            };
            _db.TrnOutsourceTimelines.Add(entry);
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add outsource timeline entry for {OutsourceId}: {EventType}", outsourceId, eventType);
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

public class OutsourceSaveRequest
{
    public long JobId { get; set; }
    public long VendorId { get; set; }
    public string? ProcessType { get; set; }
    public decimal? TotalQuantity { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? ExpectedDeliveryDate { get; set; }
    public string? Remarks { get; set; }
    public List<OutsourceItemRequest>? Items { get; set; }
}

public class OutsourceItemRequest
{
    public long JobItemId { get; set; }
    public int ItemSequence { get; set; }
    public string? ProductName { get; set; }
    public string? ProcessName { get; set; }
    public decimal Quantity { get; set; }
    public decimal? Rate { get; set; }
    public decimal? Amount { get; set; }
    public int? UomId { get; set; }
    public string? Remarks { get; set; }
}

public class OutsourceStatusRequest
{
    public long OutsourceId { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class OutsourceDispatchRequest
{
    public long OutsourceId { get; set; }
    public string? ChallanNo { get; set; }
    public decimal? TotalQuantity { get; set; }
    public string? Remarks { get; set; }
}

public class OutsourceReceiveRequest
{
    public long OutsourceId { get; set; }
    public decimal? ReceivedQuantity { get; set; }
    public decimal? RejectedQuantity { get; set; }
    public string? Remarks { get; set; }
}

public class OutsourceEmailRequest
{
    public long OutsourceId { get; set; }
}
