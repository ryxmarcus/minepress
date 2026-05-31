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
public class QuotationController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly INotificationDispatcher _notificationDispatcher;
    private readonly IUserActivityService _activityService;
    private readonly IDocumentNumberService _documentNumberService;
    private readonly IWorkspaceProcessEngine _workspaceEngine;
    private readonly ISystemErrorLogger _systemErrorLogger;
    private readonly ILogger<QuotationController> _logger;
    private readonly IConfiguration _configuration;

    public QuotationController(
        ApplicationDbContext db,
        INotificationDispatcher notificationDispatcher,
        IUserActivityService activityService,
        IDocumentNumberService documentNumberService,
        IWorkspaceProcessEngine workspaceEngine,
        ISystemErrorLogger systemErrorLogger,
        ILogger<QuotationController> logger,
        IConfiguration configuration)
    {
        _db = db;
        _notificationDispatcher = notificationDispatcher;
        _activityService = activityService;
        _documentNumberService = documentNumberService;
        _workspaceEngine = workspaceEngine;
        _systemErrorLogger = systemErrorLogger;
        _logger = logger;
        _configuration = configuration;
    }

    // ── Quotation List ──
    [HttpGet("list")]
    public async Task<IActionResult> GetQuotationList()
    {
        var list = await _db.TrnQuotations
            .Include(q => q.Party)
            .Include(q => q.TrnQuotationItems)
            .Include(q => q.Enquiry)
            .OrderByDescending(q => q.QuotationId)
            .Select(q => new
            {
                q.QuotationId,
                q.QuotationNo,
                QuotationDate = q.QuotationDate.ToString("dd-MMM-yyyy"),
                CustomerName = q.Party.Name,
                CustomerCode = q.Party.Code,
                q.PartyRefNo,
                q.Status,
                q.TotalAmount,
                q.NetAmount,
                q.TaxAmount,
                ValidTill = q.ValidTill.HasValue ? q.ValidTill.Value.ToString("dd-MMM-yyyy") : null,
                ItemCount = q.TrnQuotationItems.Count,
                TotalQuantity = q.TrnQuotationItems.Sum(i => i.Quantity),
                EnquiryNo = q.Enquiry != null ? q.Enquiry.EnquiryNo : null,
                q.EnquiryId,
                q.Remarks,
                CreatedOn = q.CreatedOn.HasValue ? q.CreatedOn.Value.ToString("dd-MMM-yyyy HH:mm") : ""
            })
            .ToListAsync();

        return Ok(list);
    }

    // ── Quotation Detail ──
    [HttpGet("detail/{id:long}")]
    public async Task<IActionResult> GetQuotationDetail(long id)
    {
        var quotation = await _db.TrnQuotations
            .Include(q => q.Party)
            .Include(q => q.Company)
            .Include(q => q.CreatedByNavigation)
            .Include(q => q.Enquiry)
            .Include(q => q.TrnQuotationItems)
                .ThenInclude(i => i.RateCalculator)
            .Include(q => q.TrnQuotationTimelines.Where(t => t.IsActive == true))
            .FirstOrDefaultAsync(q => q.QuotationId == id);

        if (quotation == null)
            return NotFound(new { message = "Quotation not found." });

        // ── Activity Log: Quotation Viewed ──
        var viewUser = HttpContext.Session.GetCurrentUser();
        if (viewUser != null)
        {
            var viewActivity = ActivityLogEntry.FromUser(viewUser, "QUOTATION", "VIEW", $"Viewed Quotation {quotation.QuotationNo}");
            viewActivity.ActivityCategory = "NAVIGATION";
            viewActivity.EntityType = "QUOTATION";
            viewActivity.EntityId = quotation.QuotationId;
            viewActivity.EntityCode = quotation.QuotationNo;
            viewActivity.Description = $"Viewed quotation {quotation.QuotationNo} details.";
            await _activityService.LogActivityAsync(viewActivity);
        }

        var result = new
        {
            quotation.QuotationId,
            quotation.QuotationNo,
            QuotationDate = quotation.QuotationDate.ToString("dd-MMM-yyyy"),
            QuotationDateIso = quotation.QuotationDate.ToString("yyyy-MM-dd"),
            CustomerName = quotation.Party.Name,
            CustomerCode = quotation.Party.Code,
            CustomerGst = quotation.Party.Gstno,
            CustomerEmail = quotation.Party.Email,
            CustomerAddress = quotation.Party.Address1,
            quotation.PartyId,
            quotation.PartyRefNo,
            PartyRefDate = quotation.PartyRefDate?.ToString("dd-MMM-yyyy"),
            ValidTill = quotation.ValidTill?.ToString("dd-MMM-yyyy"),
            ValidTillIso = quotation.ValidTill?.ToString("yyyy-MM-dd"),
            quotation.TotalAmount,
            quotation.DiscountAmount,
            quotation.TaxableAmount,
            quotation.TaxAmount,
            quotation.NetAmount,
            quotation.Status,
            quotation.TermsConditions,
            quotation.Remarks,
            quotation.EnquiryId,
            EnquiryNo = quotation.Enquiry?.EnquiryNo,
            CompanyName = quotation.Company?.Name,
            CompanyGstin = quotation.Company?.Gstin,
            CompanyAddress = quotation.Company?.AddressLine1,
            CompanyEmail = quotation.Company?.EmailId,
            CompanyPhone = quotation.Company?.ContactNo,
            CreatedByName = quotation.CreatedByNavigation?.Name ?? "",
            CreatedOn = quotation.CreatedOn?.ToString("dd-MMM-yyyy HH:mm"),
            Items = quotation.TrnQuotationItems
                .OrderBy(i => i.ItemSequence)
                .Select(i => new
                {
                    i.QuotationItemId,
                    i.ItemSequence,
                    i.ProductName,
                    i.ProductDescription,
                    i.Quantity,
                    i.UomId,
                    i.UnitRate,
                    i.GrossAmount,
                    i.DiscountPercent,
                    i.DiscountAmount,
                    i.TaxableValue,
                    i.CgstPercent,
                    i.CgstAmount,
                    i.SgstPercent,
                    i.SgstAmount,
                    i.IgstPercent,
                    i.IgstAmount,
                    i.TotalTaxAmount,
                    i.NetAmount,
                    i.RateCalculatorId,
                    i.CalcRefNo,
                    i.Remarks,
                    i.EnquiryItemId,
                    RateCalc = i.RateCalculator == null ? null : new
                    {
                        i.RateCalculator.RateCalcId,
                        i.RateCalculator.CalcRefNo,
                        i.RateCalculator.GrandTotal,
                        i.RateCalculator.CostPerUnit,
                        i.RateCalculator.CostBreakdown,
                        i.RateCalculator.BomData
                    }
                }),
            Timeline = quotation.TrnQuotationTimelines
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
                    t.OldAmount,
                    t.NewAmount,
                    t.CommunicationMode,
                    t.CommunicationReference,
                    t.AttachmentUrl,
                    CreatedOn = t.CreatedOn.ToString("dd-MMM-yyyy HH:mm")
                })
        };

        return Ok(result);
    }

    // ── Save Quotation ──
    [HttpPost("save")]
    public async Task<IActionResult> SaveQuotation([FromBody] QuotationSaveRequest request)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized(new { message = "Session expired. Please login again." });

        var quotationNo = await _documentNumberService.GenerateNextNumberAsync(DocumentProcessCode.QUOTATION);

        var quotation = new TrnQuotation
        {
            QuotationNo = quotationNo,
            QuotationDate = DateOnly.FromDateTime(DateTime.Now),
            CompanyId = user.CompanyId ?? 1,
            LocationId = user.LocationId,
            PartyId = request.PartyId,
            EnquiryId = request.EnquiryId,
            PartyRefNo = request.PartyRefNo,
            PartyRefDate = string.IsNullOrEmpty(request.PartyRefDate)
                ? null
                : DateOnly.Parse(request.PartyRefDate),
            ValidTill = string.IsNullOrEmpty(request.ValidTill)
                ? null
                : DateOnly.Parse(request.ValidTill),
            TotalAmount = request.TotalAmount,
            DiscountAmount = request.DiscountAmount,
            TaxableAmount = request.TaxableAmount,
            TaxAmount = request.TaxAmount,
            NetAmount = request.NetAmount,
            TermsConditions = request.TermsConditions,
            Remarks = request.Remarks,
            Status = "DRAFT",
            CreatedBy = user.UserId,
            CreatedOn = DateTime.Now
        };

        _db.TrnQuotations.Add(quotation);
        await _db.SaveChangesAsync();

        // Save items
        if (request.Items?.Any() == true)
        {
            foreach (var item in request.Items)
            {
                var quotationItem = new TrnQuotationItem
                {
                    QuotationId = quotation.QuotationId,
                    EnquiryItemId = item.EnquiryItemId,
                    ItemSequence = item.ItemSequence,
                    ProductName = item.ProductName,
                    ProductDescription = item.ProductDescription,
                    ProductTypeName = item.ProductTypeName,
                    JobTypeName = item.JobTypeName,
                    ProductSizeName = item.ProductSizeName,
                    NoOfPages = item.NoOfPages,
                    TrimWidthMm = item.TrimWidthMm,
                    TrimHeightMm = item.TrimHeightMm,
                    PrintingMethod = item.PrintingMethod,
                    Quantity = item.Quantity,
                    UomId = item.UomId,
                    UnitRate = item.UnitRate,
                    GrossAmount = item.GrossAmount,
                    DiscountPercent = item.DiscountPercent,
                    DiscountAmount = item.DiscountAmount,
                    TaxableValue = item.TaxableValue,
                    CgstPercent = item.CgstPercent,
                    CgstAmount = item.CgstAmount,
                    SgstPercent = item.SgstPercent,
                    SgstAmount = item.SgstAmount,
                    IgstPercent = item.IgstPercent,
                    IgstAmount = item.IgstAmount,
                    TotalTaxAmount = item.TotalTaxAmount,
                    NetAmount = item.NetAmount,
                    RateCalculatorId = item.RateCalculatorId,
                    CalcRefNo = item.CalcRefNo,
                    Remarks = item.Remarks,
                    CreatedBy = user.UserId,
                    CreatedOn = DateTime.Now
                };

                _db.TrnQuotationItems.Add(quotationItem);
            }

            await _db.SaveChangesAsync();

            // Link rate calculators to this quotation
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
                    rc.QuotationId = quotation.QuotationId;
                }
                await _db.SaveChangesAsync();
            }
        }

        // If converted from enquiry, update enquiry status
        if (request.EnquiryId.HasValue && request.EnquiryId > 0)
        {
            var enquiry = await _db.TrnEnquiries.FindAsync(request.EnquiryId.Value);
            if (enquiry != null)
            {
                enquiry.Status = "CONVERTED";
                enquiry.ModifiedBy = user.UserId.ToString();
                enquiry.ModifiedOn = DateTime.Now;
                await _db.SaveChangesAsync();
            }
        }

        // ── Dispatch notification ──
        await DispatchQuotationNotificationAsync(quotation, user);

        // ── Activity Log: Quotation Created ──
        var createActivity = ActivityLogEntry.FromUser(user, "QUOTATION", "CREATE", $"Created Quotation {quotation.QuotationNo}");
        createActivity.EntityType = "QUOTATION";
        createActivity.EntityId = quotation.QuotationId;
        createActivity.EntityCode = quotation.QuotationNo;
        createActivity.Description = $"Quotation {quotation.QuotationNo} created with {request.Items?.Count ?? 0} item(s). Net Amount: {quotation.NetAmount:N2}.";
        createActivity.NewValues = JsonSerializer.Serialize(new { quotation.QuotationNo, quotation.PartyId, quotation.NetAmount, quotation.Status, ItemCount = request.Items?.Count ?? 0 });
        createActivity.Severity = "INFO";
        await _activityService.LogActivityAsync(createActivity);

        // ── Party Activity Log ──
        if (request.PartyId > 0)
        {
            await PartyPortalController.LogPartyActivityAsync(_db, request.PartyId,
                "QUOTATION", "QUOTATION_CREATED",
                $"Quotation {quotation.QuotationNo} Created",
                $"Quotation created with {request.Items?.Count ?? 0} item(s). Net Amount: ₹{quotation.NetAmount:N2}.",
                "trn_quotation", quotation.QuotationId, quotation.QuotationNo,
                quotation.QuotationDate, "Draft", "Not Required", quotation.NetAmount, user.Name);
        }

        // ── In-App Notification ──
        await _activityService.LogNotificationAsync(new UserNotificationEntry
        {
            UserId = user.UserId,
            Title = "New Quotation Created",
            Message = $"Quotation {quotation.QuotationNo} has been created. Net Amount: ₹{quotation.NetAmount:N2}.",
            Icon = "bi bi-file-earmark-text",
            Color = "primary",
            Module = "QUOTATION",
            EventType = "CREATED",
            ReferenceId = (int)quotation.QuotationId,
            ReferenceUrl = $"/Quotation/Details?id={quotation.QuotationId}",
            Priority = "NORMAL"
        });

        // ── Quotation Timeline: CREATED ──
        await AddQuotationTimelineEntryAsync(
            quotation.QuotationId, "CREATED", "CREATED",
            "Quotation Created",
            $"Quotation {quotation.QuotationNo} created with {request.Items?.Count ?? 0} item(s). Net Amount: ₹{quotation.NetAmount:N2}.",
            newStatus: "DRAFT", newAmount: quotation.NetAmount,
            enquiryId: request.EnquiryId, userId: user.UserId);

        // ── If converted from enquiry, log in enquiry timeline too ──
        if (request.EnquiryId.HasValue && request.EnquiryId > 0)
        {
            await AddQuotationTimelineEntryAsync(
                quotation.QuotationId, "CONVERTED_FROM_ENQUIRY", "CONVERTED_FROM_ENQUIRY",
                "Converted from Enquiry",
                $"Quotation {quotation.QuotationNo} was created from enquiry conversion.",
                newStatus: "DRAFT", enquiryId: request.EnquiryId, userId: user.UserId);

            // Also insert into enquiry timeline
            var enq = await _db.TrnEnquiries.FindAsync(request.EnquiryId.Value);
            await AddEnquiryTimelineEntryAsync(
                request.EnquiryId.Value, "QUOTATION_SENT", "CONVERTED",
                "Converted to Quotation",
                $"Enquiry converted to Quotation {quotation.QuotationNo}. Net Amount: ₹{quotation.NetAmount:N2}.",
                oldStatus: enq?.Status, newStatus: "CONVERTED",
                userId: user.UserId);

            // ── Auto-complete all enquiry-related workspace tasks up to QUOT process ──
            await _workspaceEngine.AutoCompleteProcessTasksAsync(
                sourceTable: "trn_enquiry",
                sourceId: request.EnquiryId.Value,
                upToProcessCode: "QUOT",
                remarks: $"Enquiry converted to Quotation {quotation.QuotationNo}. Notification sent successfully.",
                completedBy: user);
        }

        // ── Generate ALL Workflow Tasks/Approvals for Quotation ──
        var quotationWithParty = await _db.TrnQuotations
            .Include(q => q.Party)
            .FirstOrDefaultAsync(q => q.QuotationId == quotation.QuotationId);

        if (quotationWithParty != null)
        {
            // Try pre-generated workflow first (all tasks created upfront)
            var workflowBatchId = await _workspaceEngine.GenerateAllWorkflowTasksAsync(
                sourceTable: WkSourceTable.Quotation,
                sourceId: quotationWithParty.QuotationId,
                sourceNo: quotationWithParty.QuotationNo,
                triggeredBy: user,
                partyId: request.PartyId,
                partyName: quotationWithParty.Party?.Name,
                actionUrl: $"/Quotation/Details?id={quotationWithParty.QuotationId}");

            // Fallback to single task creation if workflow template not found
            if (!workflowBatchId.HasValue)
            {
                await _workspaceEngine.CreateWorkspaceTaskAsync(
                    processCode: WkProcessCode.Quot,
                    eventTypeCode: WkEventTypeCode.ProcStart,
                    sourceTable: WkSourceTable.Quotation,
                    sourceId: quotationWithParty.QuotationId,
                    sourceNo: quotationWithParty.QuotationNo,
                    title: $"Quotation Created – {quotationWithParty.QuotationNo}",
                    description: $"Quotation {quotationWithParty.QuotationNo} for {quotationWithParty.Party?.Name ?? "customer"}. Amount: ₹{quotationWithParty.NetAmount:N2}.",
                    taskType: WkTaskType.Task,
                    priority: WkPriority.Normal,
                    triggeredBy: user,
                    jobNo: quotationWithParty.QuotationNo,
                    partyName: quotationWithParty.Party?.Name,
                    actionUrl: $"/Quotation/Details?id={quotationWithParty.QuotationId}",
                    partyId: request.PartyId);
            }
        }

        return Ok(new { quotation.QuotationId, quotation.QuotationNo, message = "Quotation saved successfully." });
    }

    // ── Convert from Enquiry ──
    [HttpGet("from-enquiry/{enquiryId:long}")]
    public async Task<IActionResult> GetEnquiryDataForConversion(long enquiryId)
    {
        var enquiry = await _db.TrnEnquiries
            .Include(e => e.Party)
            .Include(e => e.TrnEnquiryItems)
                .ThenInclude(i => i.RateCalculator)
            .FirstOrDefaultAsync(e => e.EnquiryId == enquiryId);

        if (enquiry == null)
            return NotFound(new { message = "Enquiry not found." });

        var result = new
        {
            enquiry.EnquiryId,
            enquiry.EnquiryNo,
            enquiry.PartyId,
            CustomerName = enquiry.Party.Name,
            CustomerCode = enquiry.Party.Code,
            CustomerEmail = enquiry.Party.Email,
            CustomerGst = enquiry.Party.Gstno,
            enquiry.ContactPerson,
            enquiry.ContactEmail,
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
                    i.NoOfPages,
                    i.TrimWidthMm,
                    i.TrimHeightMm,
                    i.PrintingMethod,
                    i.Quantity,
                    i.RateCalculatorId,
                    CalcRefNo = i.RateCalculator != null ? i.RateCalculator.CalcRefNo : null,
                    CostPerUnit = i.RateCalculator != null ? i.RateCalculator.CostPerUnit : (decimal?)null,
                    GrandTotal = i.RateCalculator != null ? i.RateCalculator.GrandTotal : (decimal?)null,
                    TaxAmount = i.RateCalculator != null ? i.RateCalculator.TaxAmount : (decimal?)null,
                    NetTotal = i.RateCalculator != null ? i.RateCalculator.NetTotal : (decimal?)null
                })
        };

        return Ok(result);
    }

    // ── Update Quotation Status ──
    [HttpPost("updatestatus")]
    public async Task<IActionResult> UpdateQuotationStatus([FromBody] QuotationStatusRequest request)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized(new { message = "Session expired. Please login again." });

        var quotation = await _db.TrnQuotations.FindAsync(request.QuotationId);
        if (quotation == null)
            return NotFound(new { message = "Quotation not found." });

        var oldStatus = quotation.Status;
        quotation.Status = request.Status;
        quotation.ModifiedBy = user.UserId.ToString();
        quotation.ModifiedOn = DateTime.Now;
        await _db.SaveChangesAsync();

        // ── Dispatch notification ──
        await DispatchQuotationStatusNotificationAsync(quotation, user, request.Status);

        // ── Activity Log ──
        var statusActivity = ActivityLogEntry.FromUser(user, "QUOTATION", "STATUS_CHANGE", $"Quotation {quotation.QuotationNo} status changed to {request.Status}");
        statusActivity.EntityType = "QUOTATION";
        statusActivity.EntityId = quotation.QuotationId;
        statusActivity.EntityCode = quotation.QuotationNo;
        statusActivity.Description = $"Status changed from {oldStatus ?? "N/A"} to {request.Status} by {user.Name}.";
        statusActivity.OldValues = JsonSerializer.Serialize(new { Status = oldStatus });
        statusActivity.NewValues = JsonSerializer.Serialize(new { Status = request.Status });
        statusActivity.ChangedFields = ["Status"];
        statusActivity.Severity = request.Status is "CANCELLED" or "CLOSED" ? "WARNING" : "INFO";
        await _activityService.LogActivityAsync(statusActivity);

        // ── Party Activity Log: Status Change ──
        if (quotation.PartyId > 0)
        {
            var approvalSt = request.Status switch
            {
                "APPROVED" => "Approved",
                "CANCELLED" => "Rejected",
                "SENT" => "Pending",
                _ => "Not Required"
            };
            await PartyPortalController.LogPartyActivityAsync(_db, quotation.PartyId,
                "QUOTATION", $"QUOTATION_{request.Status}",
                $"Quotation {quotation.QuotationNo} — {request.Status}",
                $"Status changed from {oldStatus ?? "N/A"} to {request.Status}.",
                "trn_quotation", quotation.QuotationId, quotation.QuotationNo,
                quotation.QuotationDate, request.Status, approvalSt, quotation.NetAmount, user.Name);
        }

        // ── In-App Notification ──
        await _activityService.LogNotificationAsync(new UserNotificationEntry
        {
            UserId = user.UserId,
            Title = $"Quotation {request.Status}",
            Message = $"Quotation {quotation.QuotationNo} has been {request.Status.ToLower()}.",
            Icon = request.Status switch
            {
                "SENT" => "bi bi-send",
                "APPROVED" => "bi bi-check-circle",
                "CANCELLED" => "bi bi-x-circle",
                "CLOSED" => "bi bi-lock",
                "REVISED" => "bi bi-arrow-repeat",
                _ => "bi bi-arrow-repeat"
            },
            Color = request.Status switch
            {
                "SENT" => "info",
                "APPROVED" => "success",
                "CANCELLED" => "warning",
                "CLOSED" => "secondary",
                _ => "primary"
            },
            Module = "QUOTATION",
            EventType = "STATUS_CHANGED",
            ReferenceId = (int)quotation.QuotationId,
            ReferenceUrl = $"/Quotation/Details?id={quotation.QuotationId}"
        });

        // ── Quotation Timeline: STATUS_CHANGED ──
        await AddQuotationTimelineEntryAsync(
            quotation.QuotationId, "STATUS_CHANGED", request.Status,
            $"Status Changed to {request.Status}",
            $"Status changed from {oldStatus ?? "N/A"} to {request.Status} by {user.Name}.",
            oldStatus: oldStatus, newStatus: request.Status,
            userId: user.UserId);

        return Ok(new { message = $"Quotation status updated to {request.Status}." });
    }

    // ── Delete Quotation ──
    [HttpDelete("delete/{id:long}")]
    public async Task<IActionResult> DeleteQuotation(long id)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized(new { message = "Session expired. Please login again." });

        var quotation = await _db.TrnQuotations
            .Include(q => q.TrnQuotationItems)
            .FirstOrDefaultAsync(q => q.QuotationId == id);

        if (quotation == null)
            return NotFound(new { message = "Quotation not found." });

        if (quotation.Status != "DRAFT")
            return BadRequest(new { message = "Only DRAFT quotations can be deleted." });

        var quotationNo = quotation.QuotationNo;
        var quotationId = quotation.QuotationId;

        _db.TrnQuotationItems.RemoveRange(quotation.TrnQuotationItems);
        _db.TrnQuotations.Remove(quotation);
        await _db.SaveChangesAsync();

        // ── Activity Log ──
        var deleteActivity = ActivityLogEntry.FromUser(user, "QUOTATION", "DELETE", $"Deleted Quotation {quotationNo}");
        deleteActivity.EntityType = "QUOTATION";
        deleteActivity.EntityId = quotationId;
        deleteActivity.EntityCode = quotationNo;
        deleteActivity.Description = $"Quotation {quotationNo} (DRAFT) was deleted by {user.Name}.";
        deleteActivity.OldValues = JsonSerializer.Serialize(new { quotationNo, quotation.PartyId, quotation.Status, quotation.NetAmount });
        deleteActivity.Severity = "WARNING";
        await _activityService.LogActivityAsync(deleteActivity);

        return Ok(new { message = "Quotation deleted successfully." });
    }

    // ── Send Quotation Email to Customer ──
    [HttpPost("send-email/{id:long}")]
    public async Task<IActionResult> SendQuotationEmail(long id)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized(new { message = "Session expired. Please login again." });

        var quotation = await _db.TrnQuotations
            .Include(q => q.Party)
            .Include(q => q.Company)
            .Include(q => q.TrnQuotationItems)
            .FirstOrDefaultAsync(q => q.QuotationId == id);

        if (quotation == null)
            return NotFound(new { message = "Quotation not found." });

        var customerEmail = quotation.Party?.Email;
        if (string.IsNullOrWhiteSpace(customerEmail))
            return BadRequest(new { message = "Customer does not have an email address on file." });

        try
        {
            var emailHtml = BuildQuotationEmailHtml(quotation);

            var config = new ProcessNotificationConfig
            {
                ConfigId = 1,
                ProcessCode = nameof(ProcessCode.EnqJob),
                SubProcessCode = notification.Enums.SubProcessCode.ReceiveEnq,
                EventType = NotificationEventType.TaskAssign,
                EventLabel = "Quotation Sent to Customer",
                RecipientType = RecipientType.Both,
                NotifyClientEmail = true,
                TemplateCode = nameof(NotificationTemplateCode.TaskAssigned),
                Priority = NotificationPriority.Normal,
                IsActive = true
            };

            var template = new NotificationTemplate
            {
                TemplateId = 1,
                TemplateCode = nameof(NotificationTemplateCode.TaskAssigned),
                TemplateName = "Quotation to Customer",
                Module = nameof(NotificationModule.Quotation),
                EventType = nameof(NotificationEventType.TaskAssign),
                Channel = NotificationChannel.Email,
                SubjectTemplate = $"Quotation {quotation.QuotationNo} from {quotation.Company?.Name ?? "MinePress"}",
                BodyTemplate = emailHtml,
                IsActive = true
            };

            var context = new NotificationContext
            {
                ThreadKey = $"QUOT:{quotation.QuotationNo}",
                AssigneeUserId = (int)user.UserId,
                AssigneeEmail = user.EmailId,
                ClientEmail = customerEmail,
                Variables = new Dictionary<string, string>
                {
                    ["quotation_no"] = quotation.QuotationNo,
                    ["customer_name"] = quotation.Party?.Name ?? "N/A"
                }
            };

            var results = await _notificationDispatcher.DispatchAsync(config, template, context);
            var emailResult = results.FirstOrDefault(r => r.Channel == NotificationChannel.Email);

            if (emailResult?.IsSuccess == true)
            {
                // Update status to SENT if currently DRAFT
                if (quotation.Status == "DRAFT")
                {
                    quotation.Status = "SENT";
                    quotation.ModifiedBy = user.UserId.ToString();
                    quotation.ModifiedOn = DateTime.Now;
                    await _db.SaveChangesAsync();
                }

                // ── Activity Log ──
                var emailActivity = ActivityLogEntry.FromUser(user, "QUOTATION", "EMAIL_SENT", $"Quotation {quotation.QuotationNo} emailed to customer");
                emailActivity.EntityType = "QUOTATION";
                emailActivity.EntityId = quotation.QuotationId;
                emailActivity.EntityCode = quotation.QuotationNo;
                emailActivity.Description = $"Quotation {quotation.QuotationNo} emailed to {customerEmail}.";
                emailActivity.NewValues = JsonSerializer.Serialize(new { CustomerEmail = customerEmail, quotation.NetAmount });
                await _activityService.LogActivityAsync(emailActivity);

                // ── In-App Notification ──
                await _activityService.LogNotificationAsync(new UserNotificationEntry
                {
                    UserId = user.UserId,
                    Title = "Quotation Emailed",
                    Message = $"Quotation {quotation.QuotationNo} has been sent to {customerEmail}.",
                    Icon = "bi bi-envelope-check",
                    Color = "success",
                    Module = "QUOTATION",
                    EventType = "EMAIL_SENT",
                    ReferenceId = (int)quotation.QuotationId,
                    ReferenceUrl = $"/Quotation/Details?id={quotation.QuotationId}"
                });

                // ── Quotation Timeline: SENT_TO_CUSTOMER ──
                await AddQuotationTimelineEntryAsync(
                    quotation.QuotationId, "SENT_TO_CUSTOMER", "EMAIL",
                    "Quotation Emailed to Customer",
                    $"Quotation {quotation.QuotationNo} sent to {customerEmail}.",
                    communicationMode: "EMAIL", communicationReference: customerEmail,
                    userId: user.UserId);

                return Ok(new { message = $"Quotation emailed to {customerEmail} successfully." });
            }
            else
            {
                return StatusCode(500, new { message = $"Failed to send email: {emailResult?.ErrorMessage ?? "Unknown error"}" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send quotation email for {QuotationNo}", quotation.QuotationNo);
            await AuditExceptionAsync(ex, $"QuotationController.SendQuotationEmail quotationNo={quotation.QuotationNo}");
            return StatusCode(500, new { message = $"Failed to send email: {ex.Message}" });
        }
    }

    // ── Build Stylish Quotation Email HTML ──
    private string BuildQuotationEmailHtml(TrnQuotation quotation)
    {
        var company = quotation.Company;
        var party = quotation.Party;
        var items = quotation.TrnQuotationItems.OrderBy(i => i.ItemSequence).ToList();

        var itemRows = "";
        var seq = 0;
        foreach (var item in items)
        {
            seq++;
            itemRows += $@"
            <tr>
                <td style=""padding:10px 12px;border-bottom:1px solid #e9ecef;text-align:center;color:#6c757d;"">{seq}</td>
                <td style=""padding:10px 12px;border-bottom:1px solid #e9ecef;"">
                    <strong style=""color:#1a1a2e;"">{item.ProductName}</strong>
                    {(string.IsNullOrEmpty(item.ProductDescription) ? "" : $"<br/><span style=\"font-size:12px;color:#6c757d;\">{item.ProductDescription}</span>")}
                </td>
                <td style=""padding:10px 12px;border-bottom:1px solid #e9ecef;text-align:center;"">{item.Quantity}</td>
                <td style=""padding:10px 12px;border-bottom:1px solid #e9ecef;text-align:right;"">₹{item.UnitRate:N2}</td>
                <td style=""padding:10px 12px;border-bottom:1px solid #e9ecef;text-align:right;"">₹{item.GrossAmount:N2}</td>
                <td style=""padding:10px 12px;border-bottom:1px solid #e9ecef;text-align:right;"">₹{item.TotalTaxAmount:N2}</td>
                <td style=""padding:10px 12px;border-bottom:1px solid #e9ecef;text-align:right;font-weight:600;color:#0d6efd;"">₹{item.NetAmount:N2}</td>
            </tr>";
        }

        var termsHtml = "";
        if (!string.IsNullOrWhiteSpace(quotation.TermsConditions))
        {
            var termsLines = quotation.TermsConditions.Split('\n');
            var termsList = string.Join("", termsLines.Select(t => $"<li style=\"margin-bottom:4px;color:#495057;\">{t.Trim()}</li>"));
            termsHtml = $@"
            <div style=""margin-top:24px;padding:16px 20px;background:#f8f9fa;border-radius:8px;border-left:4px solid #0d6efd;"">
                <h3 style=""margin:0 0 8px 0;font-size:14px;color:#1a1a2e;text-transform:uppercase;letter-spacing:1px;"">Terms &amp; Conditions</h3>
                <ol style=""margin:0;padding-left:20px;font-size:13px;line-height:1.6;"">{termsList}</ol>
            </div>";
        }

        return $@"
<!DOCTYPE html>
<html>
<head><meta charset=""utf-8""/></head>
<body style=""margin:0;padding:0;background:#f0f2f5;font-family:'Segoe UI',Tahoma,Geneva,Verdana,sans-serif;"">
<div style=""max-width:700px;margin:20px auto;background:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,0.08);"">

    <!-- Header -->
    <div style=""background:linear-gradient(135deg,#0d6efd,#0099ff);padding:28px 32px;color:#ffffff;"">
        <table width=""100%"" cellpadding=""0"" cellspacing=""0""><tr>
            <td>
                <h1 style=""margin:0;font-size:22px;font-weight:700;letter-spacing:-0.5px;"">{company?.Name ?? "MinePress ERP"}</h1>
                <p style=""margin:4px 0 0 0;font-size:13px;opacity:0.85;"">{company?.AddressLine1 ?? ""}</p>
                {(company?.Gstin != null ? $"<p style=\"margin:2px 0 0 0;font-size:12px;opacity:0.75;\">GSTIN: {company.Gstin}</p>" : "")}
            </td>
            <td style=""text-align:right;vertical-align:top;"">
                <div style=""background:rgba(255,255,255,0.2);border-radius:8px;padding:12px 16px;display:inline-block;"">
                    <div style=""font-size:11px;text-transform:uppercase;letter-spacing:1px;opacity:0.85;"">QUOTATION</div>
                    <div style=""font-size:20px;font-weight:700;margin-top:2px;"">{quotation.QuotationNo}</div>
                </div>
            </td>
        </tr></table>
    </div>

    <!-- Meta Info -->
    <div style=""padding:20px 32px;background:#f8f9fa;border-bottom:1px solid #e9ecef;"">
        <table width=""100%"" cellpadding=""0"" cellspacing=""0""><tr>
            <td style=""width:50%;vertical-align:top;"">
                <div style=""font-size:11px;text-transform:uppercase;letter-spacing:1px;color:#6c757d;margin-bottom:6px;"">BILL TO</div>
                <div style=""font-size:15px;font-weight:600;color:#1a1a2e;"">{party?.Name ?? "Customer"}</div>
                {(party?.Address1 != null ? $"<div style=\"font-size:13px;color:#495057;margin-top:2px;\">{party.Address1}</div>" : "")}
                {(party?.Gstno != null ? $"<div style=\"font-size:12px;color:#6c757d;margin-top:2px;\">GSTIN: {party.Gstno}</div>" : "")}
                {(party?.Email != null ? $"<div style=\"font-size:12px;color:#6c757d;margin-top:2px;\">Email: {party.Email}</div>" : "")}
            </td>
            <td style=""width:50%;vertical-align:top;text-align:right;"">
                <table cellpadding=""0"" cellspacing=""0"" style=""margin-left:auto;"">
                    <tr><td style=""font-size:12px;color:#6c757d;padding:2px 12px 2px 0;"">Date:</td><td style=""font-size:12px;font-weight:600;color:#1a1a2e;"">{quotation.QuotationDate:dd-MMM-yyyy}</td></tr>
                    {(quotation.ValidTill.HasValue ? $"<tr><td style=\"font-size:12px;color:#6c757d;padding:2px 12px 2px 0;\">Valid Till:</td><td style=\"font-size:12px;font-weight:600;color:#1a1a2e;\">{quotation.ValidTill:dd-MMM-yyyy}</td></tr>" : "")}
                    {(!string.IsNullOrEmpty(quotation.PartyRefNo) ? $"<tr><td style=\"font-size:12px;color:#6c757d;padding:2px 12px 2px 0;\">Your Ref:</td><td style=\"font-size:12px;font-weight:600;color:#1a1a2e;\">{quotation.PartyRefNo}</td></tr>" : "")}
                </table>
            </td>
        </tr></table>
    </div>

    <!-- Items Table -->
    <div style=""padding:24px 32px;"">
        <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""border-collapse:collapse;font-size:13px;"">
            <thead>
                <tr style=""background:#f8f9fa;"">
                    <th style=""padding:10px 12px;text-align:center;font-size:11px;text-transform:uppercase;letter-spacing:1px;color:#6c757d;border-bottom:2px solid #dee2e6;"">#</th>
                    <th style=""padding:10px 12px;text-align:left;font-size:11px;text-transform:uppercase;letter-spacing:1px;color:#6c757d;border-bottom:2px solid #dee2e6;"">Description</th>
                    <th style=""padding:10px 12px;text-align:center;font-size:11px;text-transform:uppercase;letter-spacing:1px;color:#6c757d;border-bottom:2px solid #dee2e6;"">Qty</th>
                    <th style=""padding:10px 12px;text-align:right;font-size:11px;text-transform:uppercase;letter-spacing:1px;color:#6c757d;border-bottom:2px solid #dee2e6;"">Rate</th>
                    <th style=""padding:10px 12px;text-align:right;font-size:11px;text-transform:uppercase;letter-spacing:1px;color:#6c757d;border-bottom:2px solid #dee2e6;"">Amount</th>
                    <th style=""padding:10px 12px;text-align:right;font-size:11px;text-transform:uppercase;letter-spacing:1px;color:#6c757d;border-bottom:2px solid #dee2e6;"">Tax</th>
                    <th style=""padding:10px 12px;text-align:right;font-size:11px;text-transform:uppercase;letter-spacing:1px;color:#6c757d;border-bottom:2px solid #dee2e6;"">Net</th>
                </tr>
            </thead>
            <tbody>
                {itemRows}
            </tbody>
        </table>

        <!-- Totals -->
        <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""margin-top:16px;border-collapse:collapse;"">
            <tr><td></td><td style=""width:200px;"">
                <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""font-size:13px;"">
                    <tr>
                        <td style=""padding:6px 0;color:#6c757d;"">Subtotal</td>
                        <td style=""padding:6px 0;text-align:right;font-weight:600;"">₹{quotation.TotalAmount:N2}</td>
                    </tr>
                    {(quotation.DiscountAmount > 0 ? $@"<tr>
                        <td style=""padding:6px 0;color:#6c757d;"">Discount</td>
                        <td style=""padding:6px 0;text-align:right;color:#dc3545;font-weight:600;"">-₹{quotation.DiscountAmount:N2}</td>
                    </tr>" : "")}
                    <tr>
                        <td style=""padding:6px 0;color:#6c757d;"">Tax</td>
                        <td style=""padding:6px 0;text-align:right;font-weight:600;"">₹{quotation.TaxAmount:N2}</td>
                    </tr>
                    <tr style=""border-top:2px solid #1a1a2e;"">
                        <td style=""padding:10px 0;font-size:16px;font-weight:700;color:#1a1a2e;"">Net Amount</td>
                        <td style=""padding:10px 0;text-align:right;font-size:18px;font-weight:700;color:#0d6efd;"">₹{quotation.NetAmount:N2}</td>
                    </tr>
                </table>
            </td></tr>
        </table>

        {termsHtml}

        {(!string.IsNullOrWhiteSpace(quotation.Remarks) ? $@"<div style=""margin-top:16px;padding:12px 16px;background:#fff3cd;border-radius:6px;border-left:4px solid #ffc107;"">
            <strong style=""font-size:12px;color:#856404;"">Remarks:</strong>
            <p style=""margin:4px 0 0 0;font-size:13px;color:#664d03;"">{quotation.Remarks}</p>
        </div>" : "")}
    </div>

    <!-- Footer -->
    <div style=""padding:20px 32px;background:#1a1a2e;color:#ffffff;text-align:center;"">
        <p style=""margin:0;font-size:13px;opacity:0.9;"">Thank you for your business!</p>
        <p style=""margin:6px 0 0 0;font-size:11px;opacity:0.6;"">
            {company?.Name ?? "MinePress ERP"}
            {(company?.ContactNo != null ? $" | Phone: {company.ContactNo}" : "")}
            {(company?.EmailId != null ? $" | Email: {company.EmailId}" : "")}
        </p>
        <p style=""margin:4px 0 0 0;font-size:11px;opacity:0.5;"">This is a computer-generated document. No signature required.</p>
    </div>

</div>
</body>
</html>";
    }

    // ── Notification Helpers ──

    private async Task DispatchQuotationNotificationAsync(TrnQuotation quotation, UserSessionData user)
    {
        try
        {
            var config = new ProcessNotificationConfig
            {
                ConfigId = 1,
                ProcessCode = nameof(ProcessCode.EnqJob),
                SubProcessCode = notification.Enums.SubProcessCode.ReceiveEnq,
                EventType = NotificationEventType.TaskAssign,
                EventLabel = "New Quotation Created",
                RecipientType = RecipientType.Internal,
                NotifyAssignee = true,
                NotifyDeptHead = true,
                NotifyInternalEmail = true,
                NotifyPush = true,
                TemplateCode = nameof(NotificationTemplateCode.TaskAssigned),
                Priority = NotificationPriority.Normal,
                IsActive = true,
                TriggerOnStatus = "DRAFT",
                AutoTrigger = true
            };

            var template = new NotificationTemplate
            {
                TemplateId = 1,
                TemplateCode = nameof(NotificationTemplateCode.TaskAssigned),
                TemplateName = "Quotation Created",
                Module = nameof(NotificationModule.Quotation),
                EventType = nameof(NotificationEventType.TaskAssign),
                Channel = NotificationChannel.Email,
                SubjectTemplate = "New Quotation {{quotation_no}} — ₹{{net_amount}}",
                BodyTemplate = """
                    <h3>New Quotation Created</h3>
                    <p><strong>Quotation No:</strong> {{quotation_no}}</p>
                    <p><strong>Customer:</strong> {{customer_name}}</p>
                    <p><strong>Net Amount:</strong> ₹{{net_amount}}</p>
                    <p><strong>Created By:</strong> {{created_by}}</p>
                    <p>Please review and take action.</p>
                    """,
                IsActive = true
            };

            var context = new NotificationContext
            {
                ThreadKey = $"QUOT:{quotation.QuotationNo}",
                AssigneeUserId = (int)user.UserId,
                AssigneeEmail = user.EmailId,
                AssigneePhone = user.MobileNo,
                Variables = new Dictionary<string, string>
                {
                    ["quotation_no"] = quotation.QuotationNo,
                    ["customer_name"] = quotation.Party?.Name ?? "N/A",
                    ["net_amount"] = quotation.NetAmount?.ToString("N2") ?? "0.00",
                    ["created_by"] = user.Name,
                    ["quotation_date"] = quotation.QuotationDate.ToString("dd-MMM-yyyy")
                }
            };

            var results = await _notificationDispatcher.DispatchAsync(config, template, context);
            _logger.LogInformation(
                "Quotation {QuotationNo}: Dispatched {Count} notifications, {Success} succeeded",
                quotation.QuotationNo, results.Count, results.Count(r => r.IsSuccess));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to dispatch notification for quotation {QuotationNo}", quotation.QuotationNo);
            await AuditExceptionAsync(ex, $"QuotationController.DispatchQuotationNotificationAsync quotationNo={quotation.QuotationNo}");
        }
    }

    // ── Quotation Timeline ──
    [HttpGet("timeline/{quotationId:long}")]
    public async Task<IActionResult> GetQuotationTimeline(long quotationId)
    {
        var timeline = await _db.TrnQuotationTimelines
            .Where(t => t.QuotationId == quotationId && t.IsActive == true)
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
                t.OldAmount,
                t.NewAmount,
                t.CommunicationMode,
                t.CommunicationReference,
                t.AttachmentUrl,
                CreatedOn = t.CreatedOn.ToString("dd-MMM-yyyy HH:mm"),
                CreatedOnIso = t.CreatedOn.ToString("yyyy-MM-ddTHH:mm:ss")
            })
            .ToListAsync();

        return Ok(timeline);
    }

    // ── Company Info for GST State Comparison ──
    [HttpGet("company-info")]
    public async Task<IActionResult> GetCompanyInfo()
    {
        var user = HttpContext.Session.GetCurrentUser();
        var companyId = user?.CompanyId ?? 1;

        var company = await _db.MstCompanies
            .Where(c => c.Id == companyId)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Gstin,
                c.StateId,
                StateCode = c.Gstin != null && c.Gstin.Length >= 2 ? c.Gstin.Substring(0, 2) : null
            })
            .FirstOrDefaultAsync();

        return Ok(company ?? new { Id = 0, Name = "", Gstin = (string?)null, StateId = (int?)null, StateCode = (string?)null });
    }

    private async Task DispatchQuotationStatusNotificationAsync(TrnQuotation quotation, UserSessionData user, string newStatus)
    {
        try
        {
            var config = new ProcessNotificationConfig
            {
                ConfigId = 1,
                ProcessCode = nameof(ProcessCode.EnqJob),
                SubProcessCode = notification.Enums.SubProcessCode.ReceiveEnq,
                EventType = NotificationEventType.TaskAssign,
                EventLabel = $"Quotation Status Changed to {newStatus}",
                RecipientType = RecipientType.Internal,
                NotifyAssignee = true,
                NotifyInternalEmail = true,
                NotifyPush = true,
                TemplateCode = nameof(NotificationTemplateCode.TaskAssigned),
                Priority = NotificationPriority.Normal,
                IsActive = true,
                TriggerOnStatus = newStatus,
                AutoTrigger = true
            };

            var template = new NotificationTemplate
            {
                TemplateId = 1,
                TemplateCode = nameof(NotificationTemplateCode.TaskAssigned),
                TemplateName = "Quotation Status Update",
                Module = nameof(NotificationModule.Quotation),
                EventType = nameof(NotificationEventType.TaskAssign),
                Channel = NotificationChannel.Email,
                SubjectTemplate = "Quotation {{quotation_no}} — Status Updated to {{new_status}}",
                BodyTemplate = """
                    <h3>Quotation Status Updated</h3>
                    <p><strong>Quotation No:</strong> {{quotation_no}}</p>
                    <p><strong>New Status:</strong> {{new_status}}</p>
                    <p><strong>Updated By:</strong> {{updated_by}}</p>
                    <p>Please review and take necessary action.</p>
                    """,
                IsActive = true
            };

            var context = new NotificationContext
            {
                ThreadKey = $"QUOT:{quotation.QuotationNo}",
                AssigneeUserId = (int)user.UserId,
                AssigneeEmail = user.EmailId,
                AssigneePhone = user.MobileNo,
                Variables = new Dictionary<string, string>
                {
                    ["quotation_no"] = quotation.QuotationNo,
                    ["new_status"] = newStatus,
                    ["updated_by"] = user.Name,
                    ["quotation_date"] = quotation.QuotationDate.ToString("dd-MMM-yyyy")
                }
            };

            var results = await _notificationDispatcher.DispatchAsync(config, template, context);
            _logger.LogInformation(
                "Quotation {QuotationNo} status update to {Status}: Dispatched {Count} notifications",
                quotation.QuotationNo, newStatus, results.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to dispatch status notification for quotation {QuotationNo}", quotation.QuotationNo);
            await AuditExceptionAsync(ex, $"QuotationController.DispatchQuotationStatusNotificationAsync quotationNo={quotation.QuotationNo}");
        }
    }

    // ── Quotation Timeline Helper ──

    private async Task AddQuotationTimelineEntryAsync(
        long quotationId, string eventType, string? eventCode,
        string eventTitle, string? eventDescription,
        string? oldStatus = null, string? newStatus = null,
        decimal? oldAmount = null, decimal? newAmount = null,
        string? remarks = null, long? enquiryId = null,
        string? communicationMode = null, string? communicationReference = null,
        long userId = 0)
    {
        try
        {
            var entry = new TrnQuotationTimeline
            {
                QuotationId = quotationId,
                EnquiryId = enquiryId,
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

            _db.TrnQuotationTimelines.Add(entry);
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add quotation timeline entry for quotation {QuotationId}: {EventType}", quotationId, eventType);
            await AuditExceptionAsync(ex, $"QuotationController.AddQuotationTimelineEntryAsync quotationId={quotationId} eventType={eventType}");
        }
    }

    // ── Enquiry Timeline Helper (for cross-module logging) ──

    private async Task AddEnquiryTimelineEntryAsync(
        long enquiryId, string eventType, string? eventCode,
        string eventTitle, string? eventDescription,
        string? oldStatus = null, string? newStatus = null,
        string? remarks = null, long userId = 0)
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
                CreatedBy = userId,
                CreatedOn = DateTime.Now,
                IsActive = true
            };

            _db.TrnEnquiryTimelines.Add(entry);
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add enquiry timeline entry for enquiry {EnquiryId}: {EventType}", enquiryId, eventType);
            await AuditExceptionAsync(ex, $"QuotationController.AddEnquiryTimelineEntryAsync enquiryId={enquiryId} eventType={eventType}");
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

public class QuotationSaveRequest
{
    public int PartyId { get; set; }
    public long? EnquiryId { get; set; }
    public string? PartyRefNo { get; set; }
    public string? PartyRefDate { get; set; }
    public string? ValidTill { get; set; }
    public decimal? TotalAmount { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? TaxableAmount { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? NetAmount { get; set; }
    public string? TermsConditions { get; set; }
    public string? Remarks { get; set; }
    public List<QuotationItemRequest>? Items { get; set; }
}

public class QuotationItemRequest
{
    public long? EnquiryItemId { get; set; }
    public int ItemSequence { get; set; }
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
    public int? UomId { get; set; }
    public decimal? UnitRate { get; set; }
    public decimal? GrossAmount { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? TaxableValue { get; set; }
    public decimal? CgstPercent { get; set; }
    public decimal? CgstAmount { get; set; }
    public decimal? SgstPercent { get; set; }
    public decimal? SgstAmount { get; set; }
    public decimal? IgstPercent { get; set; }
    public decimal? IgstAmount { get; set; }
    public decimal? TotalTaxAmount { get; set; }
    public decimal? NetAmount { get; set; }
    public long? RateCalculatorId { get; set; }
    public string? CalcRefNo { get; set; }
    public string? Remarks { get; set; }
}

public class QuotationStatusRequest
{
    public long QuotationId { get; set; }
    public string Status { get; set; } = string.Empty;
}
