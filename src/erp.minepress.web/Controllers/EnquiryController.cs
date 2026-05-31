using erp.minepress.domain.Enums;
using erp.minepress.notification.Enums;
using erp.minepress.notification.Interfaces;
using erp.minepress.notification.Models;
using erp.minepress.persistence.Context;
using erp.minepress.persistence.Models;
using erp.minepress.web.Helpers;
using erp.minepress.web.Services;
using erp.minepress.infrastructure.ErrorLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace erp.minepress.web.Controllers;


[ApiController]
[Route("api/[controller]")]
public class EnquiryController : ControllerBase
{
    private static readonly string[] WorkspaceDepartmentCodes =
    [
         "MGT", "ADM", "HR", "FIN", "IT", "SAL", "CST", "EST", "PRE",
        "PRT", "FINP", "PKG", "DSP", "INV", "PUR", "QMS", "MNT", "SEC"
    ];

    private readonly ApplicationDbContext _db;
    private readonly INotificationDispatcher _notificationDispatcher;
    private readonly INotificationService _notifier;
    private readonly IUserActivityService _activityService;
    private readonly IDocumentNumberService _documentNumberService;
    private readonly IWorkspaceProcessEngine _workspaceEngine;
    private readonly ISystemErrorLogger _systemErrorLogger;
    private readonly ILogger<EnquiryController> _logger;

    public EnquiryController(
        ApplicationDbContext db,
        INotificationDispatcher notificationDispatcher,
        INotificationService notifier,
        IUserActivityService activityService,
        IDocumentNumberService documentNumberService,
        IWorkspaceProcessEngine workspaceEngine,
        ISystemErrorLogger systemErrorLogger,
        ILogger<EnquiryController> logger)
    {
        _db = db;
        _notificationDispatcher = notificationDispatcher;
        _notifier = notifier;
        _activityService = activityService;
        _documentNumberService = documentNumberService;
        _workspaceEngine = workspaceEngine;
        _systemErrorLogger = systemErrorLogger;
        _logger = logger;
    }

    // ── Customer search (server-side, paginated) ──
    [HttpGet("customers/search")]
    public async Task<IActionResult> SearchCustomers([FromQuery] string? q, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = _db.MstParties.Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term) ||
                (p.Code != null && p.Code.ToLower().Contains(term)) ||
                (p.Gstno != null && p.Gstno.ToLower().Contains(term)) ||
                (p.Mobile != null && p.Mobile.Value.ToString().Contains(term)));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.Id,
                PartyId = p.Id,
                p.Name,
                p.Code,
                p.Email,
                Mobile = p.Mobile.HasValue ? p.Mobile.Value.ToString() : null,
                p.Gstno,
                p.Address1,
                p.Pin
            })
            .ToListAsync();

        return Ok(new { items, total, page, pageSize, totalPages = (int)Math.Ceiling(total / (double)pageSize) });
    }

    [HttpGet("customers/{id:int}")]
    public async Task<IActionResult> GetCustomerById(int id)
    {
        var p = await _db.MstParties
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                PartyId = x.Id,
                x.Name,
                x.Code,
                x.Email,
                Mobile = x.Mobile.HasValue ? x.Mobile.Value.ToString() : null,
                x.Gstno,
                x.Address1,
                x.Pin
            })
            .FirstOrDefaultAsync();

        return p == null ? NotFound() : Ok(p);
    }

    // ── HSN/SAC Codes (shared tax endpoint) ──
    [HttpGet("hsnsaccodes")]
    public async Task<IActionResult> GetHsnSacCodes([FromQuery] string? q)
    {
        var query = _db.MstHsnSacCodes
            .Where(h => h.IsActive == true);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(h =>
                h.Code.ToLower().Contains(term) ||
                h.Description.ToLower().Contains(term) ||
                (h.Category != null && h.Category.ToLower().Contains(term)));
        }

        var codes = await query
            .OrderByDescending(h => h.IsCommonlyUsed)
            .ThenBy(h => h.Code)
            .Take(50)
            .Select(h => new
            {
                h.Id,
                h.Code,
                h.CodeType,
                h.Description,
                h.TaxCategoryId,
                h.DefaultGstRate,
                h.CgstRate,
                h.SgstRate,
                h.IgstRate,
                h.Category,
                h.IsCommonlyUsed
            })
            .ToListAsync();

        return Ok(codes);
    }

    // ── Save Rate Calculator result (before enquiry) ──
    [HttpPost("saveratecalc")]
    public async Task<IActionResult> SaveRateCalculator([FromBody] RateCalcSaveRequest request)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized(new { message = "Session expired. Please login again." });

        var calcRefNo = $"RC-{DateTime.Now:yyyyMMdd-HHmmss}-{new Random().Next(100, 999)}";

        var rateCalc = new HybJobRateCalculator
        {
            CalcRefNo = calcRefNo,
            PartyId = request.PartyId > 0 ? request.PartyId : null,
            JobTypeId = request.JobTypeId,
            ProductTypeId = request.ProductTypeId,
            ProductSizeId = request.ProductSizeId,
            Quantity = request.Quantity,
            TotalPages = request.TotalPages,
            TrimWidthMm = request.TrimWidthMm,
            TrimHeightMm = request.TrimHeightMm,
            PrintingMode = request.PrintingMode,
            IsCustomerMaterial = request.IsCustomerMaterial,
            GrandTotal = request.GrandTotal,
            TaxAmount = request.TaxAmount,
            NetTotal = request.NetTotal,
            CostPerUnit = request.CostPerUnit,
            PartsData = request.PartsData,
            CostBreakdown = request.CostBreakdown,
            BomData = request.BomData,
            AiInsights = request.AiInsights,
            RecommendedMachines = request.RecommendedMachines,
            CalcInputSnapshot = request.CalcInputSnapshot,
            ConfigData = request.ConfigData,
            Status = "DRAFT",
            Version = 1,
            CreatedBy = user.UserId,
            CreatedOn = DateTime.Now
        };

        _db.HybJobRateCalculators.Add(rateCalc);
        await _db.SaveChangesAsync();

        // ── Activity Log: Rate Calculator Saved ──
        var rcActivity = ActivityLogEntry.FromUser(user, "RATE_CALC", "CREATE", $"Saved Rate Calculator {rateCalc.CalcRefNo}");
        rcActivity.EntityType = "RATE_CALCULATOR";
        rcActivity.EntityId = rateCalc.RateCalcId;
        rcActivity.EntityCode = rateCalc.CalcRefNo;
        rcActivity.Description = $"Rate calculator {rateCalc.CalcRefNo} saved. Grand Total: {rateCalc.GrandTotal:N2}, Qty: {rateCalc.Quantity}.";
        rcActivity.NewValues = JsonSerializer.Serialize(new { rateCalc.CalcRefNo, rateCalc.GrandTotal, rateCalc.Quantity, rateCalc.Status });
        await _activityService.LogActivityAsync(rcActivity);

        return Ok(new { rateCalc.RateCalcId, rateCalc.CalcRefNo });
    }

    [HttpGet("taxcategories")]
    public async Task<IActionResult> GetTaxCategories()
    {
        var categories = await _db.MstTaxCategories
            .Where(t => t.IsActive == true)
            .OrderBy(t => t.Name)
            .Select(t => new
            {
                t.Id,
                t.Code,
                t.Name,
                Rate = t.MstTaxCategoryComponents
                    .Where(c => c.IsActive == true)
                    .Sum(c => c.RatePercent)
            })
            .ToListAsync();

        return Ok(categories);
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetEnquiryList()
    {
        var list = await _db.TrnEnquiries
            .Include(e => e.Party)
            .Include(e => e.TrnEnquiryItems)
            .OrderByDescending(e => e.EnquiryId)
            .Select(e => new
            {
                e.EnquiryId,
                e.EnquiryNo,
                EnquiryDate = e.EnquiryDate.ToString("dd-MMM-yyyy"),
                CustomerName = e.Party.Name,
                CustomerCode = e.Party.Code,
                e.ContactPerson,
                e.Priority,
                e.Status,
                ItemCount = e.TrnEnquiryItems.Count,
                TotalQuantity = e.TrnEnquiryItems.Sum(i => i.Quantity),
                e.Remarks,
                CreatedOn = e.CreatedOn.HasValue ? e.CreatedOn.Value.ToString("dd-MMM-yyyy HH:mm") : ""
            })
            .ToListAsync();

        return Ok(list);
    }

    [HttpPost("save")]
    public async Task<IActionResult> SaveEnquiry([FromBody] EnquirySaveRequest request)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized(new { message = "Session expired. Please login again." });

        var enquiryNo = await _documentNumberService.GenerateNextNumberAsync(DocumentProcessCode.ENQUIRY);

        var enquiry = new TrnEnquiry
        {
            EnquiryNo = enquiryNo,
            EnquiryDate = DateOnly.FromDateTime(DateTime.Now),
            CompanyId = user.CompanyId ?? 1,
            LocationId = user.LocationId,
            PartyId = request.PartyId,
            ContactPerson = request.ContactPerson,
            ContactMobile = request.ContactMobile,
            ContactEmail = request.ContactEmail,
            EnquirySource = request.EnquirySource,
            ExpectedDeliveryDate = string.IsNullOrEmpty(request.ExpectedDeliveryDate)
                ? null
                : DateOnly.Parse(request.ExpectedDeliveryDate),
            Priority = request.Priority,
            Status = "DRAFT",
            Remarks = request.Remarks,
            CreatedBy = user.UserId,
            CreatedOn = DateTime.Now
        };

        _db.TrnEnquiries.Add(enquiry);
        await _db.SaveChangesAsync();

        // Save items with rate calculator link
        if (request.Items?.Any() == true)
        {
            foreach (var item in request.Items)
            {
                var enquiryItem = new TrnEnquiryItem
                {
                    EnquiryId = enquiry.EnquiryId,
                    RateCalculatorId = item.RateCalculatorId,
                    CalcRefNo = item.CalcRefNo,
                    ItemSequence = item.ItemSequence,
                    ProductName = item.ProductName,
                    ProductDescription = item.ProductDescription,
                    ProductTypeName = item.ProductTypeName,
                    JobTypeName = item.JobTypeName,
                    ProductSizeName = item.ProductSizeName,
                    Quantity = item.Quantity,
                    NoOfPages = item.NoOfPages,
                    TrimWidthMm = item.TrimWidthMm,
                    TrimHeightMm = item.TrimHeightMm,
                    PrintingMethod = item.PrintingMethod,
                    SpecificationsJson = item.SpecificationsJson,
                    Status = item.Status ?? "DRAFT",
                    CreatedBy = user.UserId,
                    CreatedOn = DateTime.Now
                };

                _db.TrnEnquiryItems.Add(enquiryItem);
            }

            await _db.SaveChangesAsync();

            // Link rate calculators to this enquiry
            var rateCalcIds = request.Items
                .Where(i => i.RateCalculatorId.HasValue)
                .Select(i => i.RateCalculatorId!.Value)
                .Distinct()
                .ToList();

            if (rateCalcIds.Count > 0)
            {
                var rateCalcs = await _db.HybJobRateCalculators
                    .Where(r => rateCalcIds.Contains(r.RateCalcId))
                    .ToListAsync();

                foreach (var rc in rateCalcs)
                {
                    rc.EnquiryId = enquiry.EnquiryId;
                }
                await _db.SaveChangesAsync();
            }
        }

        // ── Dispatch notification for new enquiry ──
        await DispatchEnquiryNotificationAsync(enquiry, user);

        // ── Timeline: Enquiry Created ──
        await AddTimelineEntryAsync(enquiry.EnquiryId, "CREATED", "CREATED",
            "Enquiry Created",
            $"Enquiry {enquiry.EnquiryNo} created with {request.Items?.Count ?? 0} item(s). Priority: {enquiry.Priority ?? "Normal"}.",
            newStatus: "DRAFT", remarks: enquiry.Remarks, userId: user.UserId);

        // ── Activity Log: Enquiry Created ──
        var createActivity = ActivityLogEntry.FromUser(user, "ENQUIRY", "CREATE", $"Created Enquiry {enquiry.EnquiryNo}");
        createActivity.EntityType = "ENQUIRY";
        createActivity.EntityId = enquiry.EnquiryId;
        createActivity.EntityCode = enquiry.EnquiryNo;
        createActivity.Description = $"Enquiry {enquiry.EnquiryNo} created with {request.Items?.Count ?? 0} item(s). Customer: {request.PartyId}, Priority: {enquiry.Priority ?? "Normal"}.";
        createActivity.NewValues = JsonSerializer.Serialize(new { enquiry.EnquiryNo, enquiry.PartyId, enquiry.Priority, enquiry.Status, ItemCount = request.Items?.Count ?? 0 });
        createActivity.Severity = "INFO";
        await _activityService.LogActivityAsync(createActivity);

        // ── Party Activity Log ──
        if (request.PartyId > 0)
        {
            await PartyPortalController.LogPartyActivityAsync(_db, request.PartyId,
                "ENQUIRY", "ENQUIRY_CREATED",
                $"Enquiry {enquiry.EnquiryNo} Created",
                $"Enquiry created with {request.Items?.Count ?? 0} item(s). Priority: {enquiry.Priority ?? "Normal"}.",
                "trn_enquiry", enquiry.EnquiryId, enquiry.EnquiryNo,
                enquiry.EnquiryDate, "Draft", "Not Required", null, user.Name);
        }

        // ── In-App Notification: New Enquiry ──
        await _activityService.LogNotificationAsync(new UserNotificationEntry
        {
            UserId = user.UserId,
            Title = "New Enquiry Created",
            Message = $"Enquiry {enquiry.EnquiryNo} has been created successfully.",
            Icon = "bi bi-file-earmark-plus",
            Color = "primary",
            Module = "ENQUIRY",
            EventType = "CREATED",
            ReferenceId = (int)enquiry.EnquiryId,
            ReferenceUrl = $"/Enquiry/Details?id={enquiry.EnquiryId}",
            Priority = enquiry.Priority == "HIGH" || enquiry.Priority == "URGENT" ? "HIGH" : "NORMAL"
        });

        // ── Notify All Departments ──
        await NotifyDepartmentAsync("ALL",
            $"Department Alert: New Enquiry {enquiry.EnquiryNo}",
            $"<h3>New Enquiry — Department Notification</h3>"
            + $"<p>A new enquiry <b>{enquiry.EnquiryNo}</b> has been created by <b>{user.Name}</b>.</p>"
            + $"<p>Customer: <b>{request.ContactPerson ?? "N/A"}</b> | Priority: <b>{enquiry.Priority ?? "Normal"}</b></p>"
            + $"<p>Items: {request.Items?.Count ?? 0} | Remarks: {enquiry.Remarks ?? "N/A"}</p>"
            + $"<hr/><small style='color:#888;'>MinePress ERP — Automated Enquiry Notification (All Departments)</small>",
            threadKey: $"ENQ:{enquiry.EnquiryNo}");

        // ── Generate ALL Workflow Tasks/Approvals for Enquiry ──
        var enquiryWithParty = await _db.TrnEnquiries
            .Include(e => e.Party)
            .FirstOrDefaultAsync(e => e.EnquiryId == enquiry.EnquiryId);

        if (enquiryWithParty != null)
        {
            // Try pre-generated workflow first (all tasks created upfront)
            var workflowBatchId = await _workspaceEngine.GenerateAllWorkflowTasksAsync(
                sourceTable: WkSourceTable.Enquiry,
                sourceId: enquiryWithParty.EnquiryId,
                sourceNo: enquiryWithParty.EnquiryNo,
                triggeredBy: user,
                partyId: request.PartyId,
                partyName: enquiryWithParty.Party?.Name,
                actionUrl: $"/Enquiry/Details?id={enquiryWithParty.EnquiryId}");

            // Fallback to single task creation if workflow template not found
            if (!workflowBatchId.HasValue)
            {
                await _workspaceEngine.CreateWorkspaceTaskAsync(
                    processCode: WkProcessCode.EnqJob,
                    eventTypeCode: WkEventTypeCode.ProcStart,
                    sourceTable: WkSourceTable.Enquiry,
                    sourceId: enquiryWithParty.EnquiryId,
                    sourceNo: enquiryWithParty.EnquiryNo,
                    title: $"Enquiry Received – {enquiryWithParty.EnquiryNo}",
                    description: $"New enquiry {enquiryWithParty.EnquiryNo} from {enquiryWithParty.Party?.Name ?? "customer"}. Source: {enquiryWithParty.EnquirySource ?? "N/A"}.",
                    taskType: WkTaskType.Task,
                    priority: enquiryWithParty.Priority ?? WkPriority.Normal,
                    triggeredBy: user,
                    jobNo: enquiryWithParty.EnquiryNo,
                    partyName: enquiryWithParty.Party?.Name,
                    actionUrl: $"/Enquiry/Details?id={enquiryWithParty.EnquiryId}",
                    partyId: request.PartyId);
            }
        }

        return Ok(new { enquiry.EnquiryId, enquiry.EnquiryNo, message = "Enquiry saved successfully." });
    }

    // ── Get Enquiry Detail ──
    [HttpGet("detail/{id:long}")]
    public async Task<IActionResult> GetEnquiryDetail(long id)
    {
        var enquiry = await _db.TrnEnquiries
            .Include(e => e.Party)
            .Include(e => e.CreatedByNavigation)
            .Include(e => e.TrnEnquiryItems)
                .ThenInclude(i => i.RateCalculator)
            .Include(e => e.TrnEnquiryTimelines.Where(t => t.IsActive == true))
            .FirstOrDefaultAsync(e => e.EnquiryId == id);

        if (enquiry == null)
            return NotFound(new { message = "Enquiry not found." });

        // ── Activity Log: Enquiry Viewed ──
        var viewUser = HttpContext.Session.GetCurrentUser();
        if (viewUser != null)
        {
            var viewActivity = ActivityLogEntry.FromUser(viewUser, "ENQUIRY", "VIEW", $"Viewed Enquiry {enquiry.EnquiryNo}");
            viewActivity.ActivityCategory = "NAVIGATION";
            viewActivity.EntityType = "ENQUIRY";
            viewActivity.EntityId = enquiry.EnquiryId;
            viewActivity.EntityCode = enquiry.EnquiryNo;
            viewActivity.Description = $"Viewed enquiry {enquiry.EnquiryNo} details.";
            await _activityService.LogActivityAsync(viewActivity);
        }

        var result = new
        {
            enquiry.EnquiryId,
            enquiry.EnquiryNo,
            EnquiryDate = enquiry.EnquiryDate.ToString("dd-MMM-yyyy"),
            EnquiryDateIso = enquiry.EnquiryDate.ToString("yyyy-MM-dd"),
            CustomerName = enquiry.Party.Name,
            CustomerCode = enquiry.Party.Code,
            CustomerGst = enquiry.Party.Gstno,
            CustomerEmail = enquiry.Party.Email,
            enquiry.PartyId,
            enquiry.ContactPerson,
            enquiry.ContactMobile,
            enquiry.ContactEmail,
            enquiry.EnquirySource,
            ExpectedDeliveryDate = enquiry.ExpectedDeliveryDate?.ToString("dd-MMM-yyyy"),
            ExpectedDeliveryDateIso = enquiry.ExpectedDeliveryDate?.ToString("yyyy-MM-dd"),
            enquiry.Priority,
            enquiry.Status,
            enquiry.Remarks,
            CreatedByName = enquiry.CreatedByNavigation != null ? enquiry.CreatedByNavigation.Name : "",
            CreatedOn = enquiry.CreatedOn?.ToString("dd-MMM-yyyy HH:mm"),
            Items = enquiry.TrnEnquiryItems
                .OrderBy(i => i.ItemSequence)
                .Select(i => new
                {
                    i.EnquiryItemId,
                    i.ItemSequence,
                    i.ProductName,
                    i.ProductDescription,
                    i.ProductTypeName,
                    i.JobTypeName,
                    i.ProductSizeName,
                    i.Quantity,
                    i.NoOfPages,
                    TrimWidthMm = i.TrimWidthMm ?? 0,
                    TrimHeightMm = i.TrimHeightMm ?? 0,
                    i.PrintingMethod,
                    i.CalcRefNo,
                    i.Status,
                    i.SpecificationsJson,
                    RateCalc = i.RateCalculator == null ? null : new
                    {
                        i.RateCalculator.RateCalcId,
                        i.RateCalculator.CalcRefNo,
                        i.RateCalculator.GrandTotal,
                        i.RateCalculator.TaxAmount,
                        i.RateCalculator.NetTotal,
                        i.RateCalculator.CostPerUnit,
                        i.RateCalculator.CostBreakdown,
                        i.RateCalculator.BomData,
                        i.RateCalculator.AiInsights,
                        i.RateCalculator.PartsData
                    }
                }),
            Timeline = enquiry.TrnEnquiryTimelines
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
                    t.FollowupDate,
                    t.FollowupMode,
                    t.AttachmentUrl,
                    CreatedOn = t.CreatedOn.ToString("dd-MMM-yyyy HH:mm"),
                    CreatedOnIso = t.CreatedOn.ToString("yyyy-MM-ddTHH:mm:ss")
                })
        };

        return Ok(result);
    }

    // ── Update Enquiry Status ──
    [HttpPost("updatestatus")]
    public async Task<IActionResult> UpdateEnquiryStatus([FromBody] UpdateStatusRequest request)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized(new { message = "Session expired. Please login again." });

        var enquiry = await _db.TrnEnquiries.FindAsync(request.EnquiryId);
        if (enquiry == null)
            return NotFound(new { message = "Enquiry not found." });

        var oldStatus = enquiry.Status;
        enquiry.Status = request.Status;
        enquiry.ModifiedBy = user.UserId.ToString();
        enquiry.ModifiedOn = DateTime.Now;
        await _db.SaveChangesAsync();

        // ── Dispatch notification for status change ──
        await DispatchEnquiryStatusNotificationAsync(enquiry, user, request.Status);

        // ── Timeline: Status Changed ──
        await AddTimelineEntryAsync(enquiry.EnquiryId, "STATUS_CHANGED", request.Status,
            $"Status Changed to {request.Status}",
            $"Status changed from {oldStatus ?? "N/A"} to {request.Status} by {user.Name}.",
            oldStatus: oldStatus, newStatus: request.Status, userId: user.UserId);

        // ── Activity Log: Status Change ──
        var statusActivity = ActivityLogEntry.FromUser(user, "ENQUIRY", "STATUS_CHANGE", $"Enquiry {enquiry.EnquiryNo} status changed to {request.Status}");
        statusActivity.EntityType = "ENQUIRY";
        statusActivity.EntityId = enquiry.EnquiryId;
        statusActivity.EntityCode = enquiry.EnquiryNo;
        statusActivity.Description = $"Status changed from {oldStatus ?? "N/A"} to {request.Status} by {user.Name}.";
        statusActivity.OldValues = JsonSerializer.Serialize(new { Status = oldStatus });
        statusActivity.NewValues = JsonSerializer.Serialize(new { Status = request.Status });
        statusActivity.ChangedFields = ["Status"];
        statusActivity.Severity = request.Status is WkEnquiryStatus.Cancelled or WkEnquiryStatus.Closed ? "WARNING" : "INFO";
        await _activityService.LogActivityAsync(statusActivity);

        // ── Party Activity Log: Status Change ──
        if (enquiry.PartyId > 0)
        {
            var approvalStatus = request.Status switch
            {
                WkEnquiryStatus.Approved => "Approved",
                WkEnquiryStatus.Cancelled => "Rejected",
                WkEnquiryStatus.Submitted => "Pending",
                _ => "Not Required"
            };
            await PartyPortalController.LogPartyActivityAsync(_db, enquiry.PartyId,
                "ENQUIRY", $"ENQUIRY_{request.Status}",
                $"Enquiry {enquiry.EnquiryNo} — {request.Status}",
                $"Status changed from {oldStatus ?? "N/A"} to {request.Status}.",
                "trn_enquiry", enquiry.EnquiryId, enquiry.EnquiryNo,
                enquiry.EnquiryDate, request.Status, approvalStatus, null, user.Name);
        }

        // ── In-App Notification: Status Update ──
        await _activityService.LogNotificationAsync(new UserNotificationEntry
        {
            UserId = user.UserId,
            Title = $"Enquiry {request.Status}",
            Message = $"Enquiry {enquiry.EnquiryNo} has been {request.Status.ToLower()}.",
            Icon = request.Status switch
            {
                "SUBMITTED" => "bi bi-send",
                "APPROVED" => "bi bi-check-circle",
                "CANCELLED" => "bi bi-x-circle",
                "CLOSED" => "bi bi-lock",
                "CONVERTED" => "bi bi-arrow-right-circle",
                _ => "bi bi-arrow-repeat"
            },
            Color = request.Status switch
            {
                "SUBMITTED" => "info",
                "APPROVED" => "success",
                "CANCELLED" => "warning",
                "CLOSED" => "secondary",
                "CONVERTED" => "success",
                _ => "primary"
            },
            Module = "ENQUIRY",
            EventType = "STATUS_CHANGED",
            ReferenceId = (int)enquiry.EnquiryId,
            ReferenceUrl = $"/Enquiry/Details?id={enquiry.EnquiryId}"
        });

        // ── Notify All Departments ──
        await NotifyDepartmentAsync("ALL",
            $"Department Alert: Enquiry {enquiry.EnquiryNo} — {request.Status}",
            $"<h3>Enquiry Status Update — Department Notification</h3>"
            + $"<p>Enquiry <b>{enquiry.EnquiryNo}</b> status changed from <b>{oldStatus ?? "N/A"}</b> to <b>{request.Status}</b>.</p>"
            + $"<p>Updated by: <b>{user.Name}</b></p>"
            + $"<hr/><small style='color:#888;'>MinePress ERP — Automated Enquiry Notification (All Departments)</small>",
            threadKey: $"ENQ:{enquiry.EnquiryNo}");

        return Ok(new { message = $"Enquiry status updated to {request.Status}." });
    }

    // ── Delete Enquiry ──
    [HttpDelete("delete/{id:long}")]
    public async Task<IActionResult> DeleteEnquiry(long id)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized(new { message = "Session expired. Please login again." });

        var enquiry = await _db.TrnEnquiries
            .Include(e => e.TrnEnquiryItems)
            .FirstOrDefaultAsync(e => e.EnquiryId == id);

        if (enquiry == null)
            return NotFound(new { message = "Enquiry not found." });

        if (enquiry.Status != WkEnquiryStatus.Draft)
            return BadRequest(new { message = "Only DRAFT enquiries can be deleted." });

        var enquiryNo = enquiry.EnquiryNo;
        var enquiryId = enquiry.EnquiryId;

        // Timeline entries are cascade-deleted with the enquiry (FK ON DELETE CASCADE)
        _db.TrnEnquiryItems.RemoveRange(enquiry.TrnEnquiryItems);
        _db.TrnEnquiries.Remove(enquiry);
        await _db.SaveChangesAsync();

        // ── Activity Log: Enquiry Deleted ──
        var deleteActivity = ActivityLogEntry.FromUser(user, "ENQUIRY", "DELETE", $"Deleted Enquiry {enquiryNo}");
        deleteActivity.EntityType = "ENQUIRY";
        deleteActivity.EntityId = enquiryId;
        deleteActivity.EntityCode = enquiryNo;
        deleteActivity.Description = $"Enquiry {enquiryNo} (DRAFT) was deleted by {user.Name}.";
        deleteActivity.OldValues = JsonSerializer.Serialize(new { enquiryNo, enquiry.PartyId, enquiry.Status, enquiry.Priority });
        deleteActivity.Severity = "WARNING";
        await _activityService.LogActivityAsync(deleteActivity);

        // ── Notify All Departments ──
        await NotifyDepartmentAsync("ALL",
            $"Department Alert: Enquiry {enquiryNo} Deleted",
            $"<h3>Enquiry Deleted — Department Notification</h3>"
            + $"<p>Enquiry <b>{enquiryNo}</b> (DRAFT) has been deleted by <b>{user.Name}</b>.</p>"
            + $"<hr/><small style='color:#888;'>MinePress ERP — Automated Enquiry Notification (All Departments)</small>",
            threadKey: $"ENQ:{enquiryNo}");

        return Ok(new { message = "Enquiry deleted successfully." });
    }

    // ── Get Company Info (for print header) ──
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
                c.LogoUrl,
                StateCode = c.Gstin != null && c.Gstin.Length >= 2 ? c.Gstin.Substring(0, 2) : null
            })
            .FirstOrDefaultAsync();

        return company == null ? NotFound() : Ok(company);
    }

    // ── Get Customer Activities — 360° Dashboard (top 10 each) ──
    [HttpGet("customer-activities/{partyId:int}")]
    public async Task<IActionResult> GetCustomerActivities(int partyId)
    {
        // Customer profile
        var party = await _db.MstParties
            .Where(p => p.Id == partyId)
            .Select(p => new
            {
                p.Name,
                p.Code,
                GstNo = p.Gstno,
                p.Email,
                Mobile = p.Mobile.HasValue ? p.Mobile.Value.ToString() : "",
                p.Address1,
                CreatedOn = p.CreatedOn.ToString("dd-MMM-yyyy")
            })
            .FirstOrDefaultAsync();

        // Enquiries
        var enquiries = await _db.TrnEnquiries
            .Where(e => e.PartyId == partyId)
            .OrderByDescending(e => e.EnquiryDate)
            .Take(10)
            .Select(e => new
            {
                e.EnquiryId,
                e.EnquiryNo,
                Date = e.EnquiryDate.ToString("dd-MMM-yyyy"),
                e.Status,
                e.Priority,
                ItemCount = e.TrnEnquiryItems.Count,
                TotalQuantity = e.TrnEnquiryItems.Sum(i => i.Quantity),
            })
            .ToListAsync();

        // Quotations
        var quotations = await _db.TrnQuotations
            .Where(q => q.PartyId == partyId)
            .OrderByDescending(q => q.QuotationDate)
            .Take(10)
            .Select(q => new
            {
                q.QuotationId,
                q.QuotationNo,
                Date = q.QuotationDate.ToString("dd-MMM-yyyy"),
                q.Status,
                q.TotalAmount,
                q.NetAmount,
            })
            .ToListAsync();

        // Jobs
        var jobs = await _db.TrnJobs
            .Where(j => j.PartyId == partyId)
            .OrderByDescending(j => j.JobDate)
            .Take(10)
            .Select(j => new
            {
                j.JobId,
                j.JobNo,
                Date = j.JobDate.ToString("dd-MMM-yyyy"),
                j.ProductName,
                j.Quantity,
                j.StatusCode,
                j.ProgressPercent,
                j.EstimatedCost,
            })
            .ToListAsync();

        // Invoices
        var invoices = await _db.TrnSalesInvoices
            .Where(i => i.PartyId == partyId)
            .OrderByDescending(i => i.InvoiceDate)
            .Take(10)
            .Select(i => new
            {
                i.SalesInvoiceId,
                i.InvoiceNo,
                Date = i.InvoiceDate.ToString("dd-MMM-yyyy"),
                i.Status,
                i.GrandTotal,
                i.BalanceAmount,
            })
            .ToListAsync();

        // Receipts
        var receipts = await _db.TrnReceipts
            .Where(r => r.PartyId == partyId)
            .OrderByDescending(r => r.ReceiptDate)
            .Take(10)
            .Select(r => new
            {
                r.ReceiptId,
                r.ReceiptNo,
                Date = r.ReceiptDate.ToString("dd-MMM-yyyy"),
                r.PaymentMode,
                r.Amount,
                r.Status,
            })
            .ToListAsync();

        // Payments
        var payments = await _db.TrnPayments
            .Where(p => p.PartyId == partyId)
            .OrderByDescending(p => p.PaymentDate)
            .Take(10)
            .Select(p => new
            {
                p.PaymentId,
                p.PaymentNo,
                Date = p.PaymentDate.ToString("dd-MMM-yyyy"),
                p.PaymentMode,
                p.Amount,
                p.Status,
            })
            .ToListAsync();

        // Summary counts
        var totalEnquiries = await _db.TrnEnquiries.CountAsync(e => e.PartyId == partyId);
        var totalQuotations = await _db.TrnQuotations.CountAsync(q => q.PartyId == partyId);
        var totalJobs = await _db.TrnJobs.CountAsync(j => j.PartyId == partyId);
        var totalInvoices = await _db.TrnSalesInvoices.CountAsync(i => i.PartyId == partyId);
        var totalReceipts = await _db.TrnReceipts.CountAsync(r => r.PartyId == partyId);
        var totalPayments = await _db.TrnPayments.CountAsync(p => p.PartyId == partyId);

        // Financial aggregates
        var totalQuotedAmount = await _db.TrnQuotations
            .Where(q => q.PartyId == partyId && q.Status != WkEnquiryStatus.Cancelled)
            .SumAsync(q => q.NetAmount ?? 0);

        var totalInvoicedAmount = await _db.TrnSalesInvoices
            .Where(i => i.PartyId == partyId && i.IsCancelled != true)
            .SumAsync(i => i.GrandTotal ?? 0);

        var totalReceiptAmount = await _db.TrnReceipts
            .Where(r => r.PartyId == partyId && r.Status != WkEnquiryStatus.Cancelled)
            .SumAsync(r => r.Amount);

        var totalPaymentAmount = await _db.TrnPayments
            .Where(p => p.PartyId == partyId && p.Status != WkEnquiryStatus.Cancelled)
            .SumAsync(p => p.Amount);

        var totalOutstanding = await _db.TrnSalesInvoices
            .Where(i => i.PartyId == partyId && i.IsCancelled != true)
            .SumAsync(i => i.BalanceAmount ?? 0);

        var avgJobCompletion = await _db.TrnJobs
            .Where(j => j.PartyId == partyId && j.ProgressPercent != null)
            .AverageAsync(j => (double?)j.ProgressPercent) ?? 0;

        return Ok(new
        {
            customer = party,
            enquiries,
            quotations,
            jobs,
            invoices,
            receipts,
            payments,
            summary = new
            {
                totalEnquiries,
                totalQuotations,
                totalJobs,
                totalInvoices,
                totalReceipts,
                totalPayments,
                totalQuotedAmount,
                totalInvoicedAmount,
                totalReceiptAmount,
                totalPaymentAmount,
                totalOutstanding,
                avgJobCompletion = (int)Math.Round(avgJobCompletion)
            }
        });
    }

    // ── Notification Helpers ──

    private async Task DispatchEnquiryNotificationAsync(TrnEnquiry enquiry, UserSessionData user)
    {
        try
        {
            var config = new ProcessNotificationConfig
            {
                ConfigId = 1,
                ProcessCode = nameof(ProcessCode.EnqJob),
                SubProcessCode = notification.Enums.SubProcessCode.ReceiveEnq,
                EventType = NotificationEventType.TaskAssign,
                EventLabel = "New Enquiry Received",
                RecipientType = RecipientType.Both,
                NotifyAssignee = true,
                NotifyDeptHead = true,
                NotifyInternalEmail = true,
                NotifyInternalWhatsApp = true,
                NotifyClientEmail = true,
                NotifyPush = true,
                TemplateCode = nameof(NotificationTemplateCode.TaskAssigned),
                Priority = NotificationPriority.Normal,
                IsActive = true,
                TriggerOnStatus = "DRAFT",
                AutoTrigger = true,
                SlaHours = 24,
                EscalateAfterHours = 48
            };

            var template = new NotificationTemplate
            {
                TemplateId = 1,
                TemplateCode = nameof(NotificationTemplateCode.TaskAssigned),
                TemplateName = "Task Assigned - Enquiry",
                Module = nameof(NotificationModule.Quotation),
                EventType = nameof(NotificationEventType.TaskAssign),
                Channel = NotificationChannel.Email,
                SubjectTemplate = "New Enquiry {{enquiry_no}} Received from {{customer_name}}",
                BodyTemplate = """
                    <h3>New Enquiry Received</h3>
                    <p><strong>Enquiry No:</strong> {{enquiry_no}}</p>
                    <p><strong>Customer:</strong> {{customer_name}}</p>
                    <p><strong>Contact:</strong> {{contact_person}} ({{contact_mobile}})</p>
                    <p><strong>Priority:</strong> {{priority}}</p>
                    <p><strong>Expected Delivery:</strong> {{expected_delivery}}</p>
                    <p><strong>Remarks:</strong> {{remarks}}</p>
                    <p><strong>Created By:</strong> {{created_by}}</p>
                    <p>Please review and take action.</p>
                    """,
                IsActive = true
            };

            var context = new NotificationContext
            {
                ThreadKey = $"ENQ:{enquiry.EnquiryNo}",
                AssigneeUserId = (int)user.UserId,
                AssigneeEmail = user.EmailId,
                AssigneePhone = user.MobileNo,
                ClientEmail = enquiry.ContactEmail,
                ClientPhone = enquiry.ContactMobile,
                Variables = new Dictionary<string, string>
                {
                    ["enquiry_no"] = enquiry.EnquiryNo,
                    ["customer_name"] = enquiry.Party?.Name ?? "N/A",
                    ["contact_person"] = enquiry.ContactPerson ?? "N/A",
                    ["contact_mobile"] = enquiry.ContactMobile ?? "N/A",
                    ["contact_email"] = enquiry.ContactEmail ?? "N/A",
                    ["priority"] = enquiry.Priority ?? "Normal",
                    ["expected_delivery"] = enquiry.ExpectedDeliveryDate?.ToString("dd-MMM-yyyy") ?? "Not specified",
                    ["remarks"] = enquiry.Remarks ?? "None",
                    ["created_by"] = user.Name,
                    ["enquiry_date"] = enquiry.EnquiryDate.ToString("dd-MMM-yyyy")
                }
            };

            var results = await _notificationDispatcher.DispatchAsync(config, template, context);
            _logger.LogInformation(
                "Enquiry {EnquiryNo}: Dispatched {Count} notifications, {Success} succeeded",
                enquiry.EnquiryNo, results.Count, results.Count(r => r.IsSuccess));

            // ── Timeline entries for each notification channel ──
            foreach (var r in results)
            {
                var channel = r.Channel.ToString();
                var eventType = r.IsSuccess ? "NOTIFICATION_SENT" : "NOTIFICATION_FAILED";
                var title = r.IsSuccess
                    ? $"{channel} Notification Sent"
                    : $"{channel} Notification Failed";
                var description = r.IsSuccess
                    ? $"{channel} notification dispatched successfully for enquiry {enquiry.EnquiryNo}."
                    : $"{channel} notification failed for enquiry {enquiry.EnquiryNo}. Error: {r.ErrorMessage ?? "Unknown"}";

                await AddTimelineEntryAsync(enquiry.EnquiryId, eventType, channel,
                    title, description, userId: user.UserId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to dispatch notification for enquiry {EnquiryNo}", enquiry.EnquiryNo);
            await AuditExceptionAsync(ex, $"EnquiryController.DispatchEnquiryNotificationAsync enquiryNo={enquiry.EnquiryNo}");
        }
    }

    private async Task DispatchEnquiryStatusNotificationAsync(TrnEnquiry enquiry, UserSessionData user, string newStatus)
    {
        try
        {
            var eventType = newStatus switch
            {
                "APPROVED" => NotificationEventType.ApprovalApproved,
                "REJECTED" => NotificationEventType.ApprovalRejected,
                "COMPLETED" => NotificationEventType.TaskComplete,
                _ => NotificationEventType.TaskAssign
            };

            var config = new ProcessNotificationConfig
            {
                ConfigId = 1,
                ProcessCode = nameof(ProcessCode.EnqJob),
                SubProcessCode = notification.Enums.SubProcessCode.ReceiveEnq,
                EventType = eventType,
                EventLabel = $"Enquiry Status Changed to {newStatus}",
                RecipientType = RecipientType.Internal,
                NotifyAssignee = true,
                NotifyDeptHead = true,
                NotifyInternalEmail = true,
                NotifyPush = true,
                TemplateCode = nameof(NotificationTemplateCode.TaskAssigned),
                Priority = NotificationPriority.Normal,
                IsActive = true,
                TriggerOnStatus = newStatus,
                AutoTrigger = true,
                SlaHours = 24,
                EscalateAfterHours = 48
            };

            var template = new NotificationTemplate
            {
                TemplateId = 1,
                TemplateCode = nameof(NotificationTemplateCode.TaskAssigned),
                TemplateName = "Enquiry Status Update",
                Module = nameof(NotificationModule.Quotation),
                EventType = eventType.ToString(),
                Channel = NotificationChannel.Email,
                SubjectTemplate = "Enquiry {{enquiry_no}} - Status Updated to {{new_status}}",
                BodyTemplate = """
                    <h3>Enquiry Status Updated</h3>
                    <p><strong>Enquiry No:</strong> {{enquiry_no}}</p>
                    <p><strong>New Status:</strong> {{new_status}}</p>
                    <p><strong>Updated By:</strong> {{updated_by}}</p>
                    <p>Please review and take necessary action.</p>
                    """,
                IsActive = true
            };

            var context = new NotificationContext
            {
                ThreadKey = $"ENQ:{enquiry.EnquiryNo}",
                AssigneeUserId = (int)user.UserId,
                AssigneeEmail = user.EmailId,
                AssigneePhone = user.MobileNo,
                Variables = new Dictionary<string, string>
                {
                    ["enquiry_no"] = enquiry.EnquiryNo,
                    ["new_status"] = newStatus,
                    ["updated_by"] = user.Name,
                    ["enquiry_date"] = enquiry.EnquiryDate.ToString("dd-MMM-yyyy")
                }
            };

            var results = await _notificationDispatcher.DispatchAsync(config, template, context);
            _logger.LogInformation(
                "Enquiry {EnquiryNo} status update to {Status}: Dispatched {Count} notifications",
                enquiry.EnquiryNo, newStatus, results.Count);

            // ── Timeline entries for each notification channel ──
            foreach (var r in results)
            {
                var channel = r.Channel.ToString();
                var tlEventType = r.IsSuccess ? "NOTIFICATION_SENT" : "NOTIFICATION_FAILED";
                var title = r.IsSuccess
                    ? $"{channel} Notification Sent"
                    : $"{channel} Notification Failed";
                var description = r.IsSuccess
                    ? $"{channel} notification dispatched for status change to {newStatus}."
                    : $"{channel} notification failed for status change to {newStatus}. Error: {r.ErrorMessage ?? "Unknown"}";

                await AddTimelineEntryAsync(enquiry.EnquiryId, tlEventType, channel,
                    title, description, userId: user.UserId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to dispatch status notification for enquiry {EnquiryNo}", enquiry.EnquiryNo);
            await AuditExceptionAsync(ex, $"EnquiryController.DispatchEnquiryStatusNotificationAsync enquiryNo={enquiry.EnquiryNo}");
        }
    }

    // ── Timeline API ──

    [HttpGet("timeline/{enquiryId:long}")]
    public async Task<IActionResult> GetTimeline(long enquiryId)
    {
        var timeline = await _db.TrnEnquiryTimelines
            .Where(t => t.EnquiryId == enquiryId && t.IsActive == true)
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
                t.FollowupDate,
                t.FollowupMode,
                t.AttachmentUrl,
                CreatedOn = t.CreatedOn.ToString("dd-MMM-yyyy HH:mm"),
                CreatedOnIso = t.CreatedOn.ToString("yyyy-MM-ddTHH:mm:ss")
            })
            .ToListAsync();

        return Ok(timeline);
    }

    // ── Timeline Helper ──

    private async Task AddTimelineEntryAsync(
        long enquiryId, string eventType, string? eventCode,
        string eventTitle, string? eventDescription,
        string? oldStatus = null, string? newStatus = null,
        string? remarks = null, long? assignedToUserId = null,
        DateTime? followupDate = null, string? followupMode = null,
        long userId = 0)
    {
        try
        {
            var entry = new TrnEnquiryTimeline
            {
                EnquiryId = enquiryId,
                EventType = eventType,
                EventCode = eventCode,
                EventTitle = eventTitle,
                EventDescription = eventDescription,
                Remarks = remarks,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                AssignedToUserId = assignedToUserId,
                FollowupDate = followupDate,
                FollowupMode = followupMode,
                CreatedBy = userId,
                CreatedOn = DateTime.Now,
                IsActive = true
            };

            _db.TrnEnquiryTimelines.Add(entry);
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add timeline entry for enquiry {EnquiryId}: {EventType}", enquiryId, eventType);
            await AuditExceptionAsync(ex, $"EnquiryController.AddTimelineEntryAsync enquiryId={enquiryId} eventType={eventType}");
        }
    }

    // ── Department Notification Helper ──

    private async Task NotifyDepartmentAsync(string deptCode, string subject, string htmlBody, string? threadKey = null)
    {
        try
        {
            var allowedDeptCodes = WorkspaceDepartmentCodes.ToHashSet(StringComparer.OrdinalIgnoreCase);

            var emails = await _db.MstUsers.AsNoTracking()
                .Join(_db.MstDepartments,
                    u => u.Departmentid,
                    d => d.DeptId,
                    (u, d) => new { u.Emailid, u.Isactive, d.DeptCode })
                .Where(x => x.Isactive == true &&
                            !string.IsNullOrEmpty(x.Emailid) &&
                            !string.IsNullOrEmpty(x.DeptCode) &&
                            (deptCode == "ALL"
                                ? allowedDeptCodes.Contains(x.DeptCode)
                                : string.Equals(x.DeptCode, deptCode, StringComparison.OrdinalIgnoreCase)))
                .Select(x => x.Emailid!)
                .Distinct()
                .ToListAsync();

            foreach (var email in emails)
            {
                await _notifier.SendAsync(new NotificationRequest
                {
                    Recipient = email,
                    Subject = subject,
                    Body = htmlBody,
                    Channel = NotificationChannel.Email,
                    EmailThreadKey = threadKey,
                    Module = "ENQUIRY",
                    EventType = "DEPARTMENT_NOTIFY",
                    ReferenceNo = threadKey
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to notify {DeptCode} department: {Subject}", deptCode, subject);
            await AuditExceptionAsync(ex, $"EnquiryController.NotifyDepartmentAsync deptCode={deptCode}", "Warning");
        }
    }

    private async Task AuditExceptionAsync(Exception ex, string additionalData, string severity = "Error")
    {
        await _systemErrorLogger.LogAsync(
            ex,
            HttpContext,
            severity: severity,
            additionalData: additionalData);
    }
}

// ── Request Models ──

public class RateCalcSaveRequest
{
    public int PartyId { get; set; }
    public int? JobTypeId { get; set; }
    public int? ProductTypeId { get; set; }
    public int? ProductSizeId { get; set; }
    public int Quantity { get; set; }
    public int TotalPages { get; set; }
    public decimal? TrimWidthMm { get; set; }
    public decimal? TrimHeightMm { get; set; }
    public string? PrintingMode { get; set; }
    public bool? IsCustomerMaterial { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal NetTotal { get; set; }
    public decimal CostPerUnit { get; set; }
    public string? PartsData { get; set; }
    public string? CostBreakdown { get; set; }
    public string? BomData { get; set; }
    public string? AiInsights { get; set; }
    public string? RecommendedMachines { get; set; }
    public string? CalcInputSnapshot { get; set; }
    public string? ConfigData { get; set; }
}

public class EnquirySaveRequest
{
    public int PartyId { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactMobile { get; set; }
    public string? ContactEmail { get; set; }
    public string? EnquirySource { get; set; }
    public string? ExpectedDeliveryDate { get; set; }
    public string? Priority { get; set; }
    public string? Remarks { get; set; }
    public List<EnquiryItemRequest>? Items { get; set; }
}

public class EnquiryItemRequest
{
    public int ItemSequence { get; set; }
    public long? RateCalculatorId { get; set; }
    public string? CalcRefNo { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductDescription { get; set; }
    public string? ProductTypeName { get; set; }
    public string? JobTypeName { get; set; }
    public string? ProductSizeName { get; set; }
    public int Quantity { get; set; }
    public int? NoOfPages { get; set; }
    public decimal? TrimWidthMm { get; set; }
    public decimal? TrimHeightMm { get; set; }
    public string? PrintingMethod { get; set; }
    public string? SpecificationsJson { get; set; }
    public string? Status { get; set; }
}

public class UpdateStatusRequest
{
    public long EnquiryId { get; set; }
    public string Status { get; set; } = string.Empty;
}
