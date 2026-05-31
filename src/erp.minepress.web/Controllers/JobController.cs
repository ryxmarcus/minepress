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
public class JobController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly INotificationDispatcher _notificationDispatcher;
    private readonly IUserActivityService _activityService;
    private readonly IDocumentNumberService _documentNumberService;
    private readonly IWorkspaceProcessEngine _workspaceEngine;
    private readonly ISystemErrorLogger _systemErrorLogger;
    private readonly ILogger<JobController> _logger;
    private readonly IConfiguration _configuration;

    public JobController(
        ApplicationDbContext db,
        INotificationDispatcher notificationDispatcher,
        IUserActivityService activityService,
        IDocumentNumberService documentNumberService,
        IWorkspaceProcessEngine workspaceEngine,
        ISystemErrorLogger systemErrorLogger,
        ILogger<JobController> logger,
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

    // ── Job List ──
    [HttpGet("list")]
    public async Task<IActionResult> GetJobList()
    {
        var list = await _db.TrnJobs
            .Include(j => j.Party)
            .Include(j => j.TrnJobItems)
            .Include(j => j.Enquiry)
            .Include(j => j.Quotation)
            .Include(j => j.JobType)
            .OrderByDescending(j => j.JobId)
            .Select(j => new
            {
                j.JobId,
                j.JobNo,
                JobDate = j.JobDate.ToString("dd-MMM-yyyy"),
                CustomerName = j.Party != null ? j.Party.Name : "",
                CustomerCode = j.Party != null ? j.Party.Code : "",
                j.PartyRefNo,
                Status = j.StatusCode,
                j.ProductName,
                j.Quantity,
                j.NetAmount,
                j.EstimatedCost,
                j.Priority,
                DeliveryDate = j.DeliveryDate.HasValue ? j.DeliveryDate.Value.ToString("dd-MMM-yyyy") : null,
                ItemCount = j.TrnJobItems.Count,
                TotalQuantity = j.TrnJobItems.Sum(i => i.Quantity ?? 0),
                EnquiryNo = j.Enquiry != null ? j.Enquiry.EnquiryNo : null,
                j.EnquiryId,
                QuotationNo = j.Quotation != null ? j.Quotation.QuotationNo : null,
                j.QuotationId,
                JobTypeName = j.JobType != null ? j.JobType.Jobtypename : null,
                j.CurrentStage,
                j.ProgressPercent,
                CreatedOn = j.CreatedOn.HasValue ? j.CreatedOn.Value.ToString("dd-MMM-yyyy HH:mm") : ""
            })
            .ToListAsync();

        return Ok(list);
    }

    // ── Job Detail ──
    [HttpGet("detail/{id:long}")]
    public async Task<IActionResult> GetJobDetail(long id)
    {
        var job = await _db.TrnJobs
            .Include(j => j.Party)
            .Include(j => j.Company)
            .Include(j => j.CreatedByNavigation)
            .Include(j => j.Enquiry)
            .Include(j => j.Quotation)
            .Include(j => j.JobType)
            .Include(j => j.JobCategory)
            .Include(j => j.AssignedToNavigation)
            .Include(j => j.TrnJobItems)
                .ThenInclude(i => i.RateCalculator)
            .Include(j => j.TrnJobItems)
                .ThenInclude(i => i.JobType)
            .FirstOrDefaultAsync(j => j.JobId == id);

        if (job == null)
            return NotFound(new { message = "Job not found." });

        // Load timeline separately
        var timeline = await _db.TrnJobTimelines
            .Where(t => t.JobId == id && t.IsActive == true)
            .OrderByDescending(t => t.CreatedOn)
            .ToListAsync();

        // ── Activity Log: Job Viewed ──
        var viewUser = HttpContext.Session.GetCurrentUser();
        if (viewUser != null)
        {
            var viewActivity = ActivityLogEntry.FromUser(viewUser, "JOB", "VIEW", $"Viewed Job {job.JobNo}");
            viewActivity.ActivityCategory = "NAVIGATION";
            viewActivity.EntityType = "JOB";
            viewActivity.EntityId = job.JobId;
            viewActivity.EntityCode = job.JobNo;
            viewActivity.Description = $"Viewed job {job.JobNo} details.";
            await _activityService.LogActivityAsync(viewActivity);
        }

        var result = new
        {
            job.JobId,
            job.JobNo,
            JobDate = job.JobDate.ToString("dd-MMM-yyyy"),
            JobDateIso = job.JobDate.ToString("yyyy-MM-dd"),
            CustomerName = job.Party?.Name,
            CustomerCode = job.Party?.Code,
            CustomerGst = job.Party?.Gstno,
            CustomerEmail = job.Party?.Email,
            CustomerAddress = job.Party?.Address1,
            PartyId = job.PartyId,
            job.PartyRefNo,
            PartyRefNoDate = job.PartyRefNoDate?.ToString("dd-MMM-yyyy"),
            DeliveryDate = job.DeliveryDate?.ToString("dd-MMM-yyyy"),
            DeliveryDateIso = job.DeliveryDate?.ToString("yyyy-MM-dd"),
            job.ProductName,
            job.ProductDescription,
            job.Quantity,
            job.TotalPages,
            job.Priority,
            job.EstimatedCost,
            job.ActualCost,
            job.QuotedAmount,
            job.GrossAmount,
            job.DiscountAmount,
            job.TaxableAmount,
            job.TaxAmount,
            job.NetAmount,
            Status = job.StatusCode,
            job.CurrentStage,
            job.ProgressPercent,
            job.SpecificationsJson,
            job.EnquiryId,
            EnquiryNo = job.Enquiry?.EnquiryNo,
            job.QuotationId,
            QuotationNo = job.Quotation?.QuotationNo,
            JobTypeName = job.JobType?.Jobtypename,
            JobCategoryName = job.JobCategory?.JobCategoryName,
            AssignedToName = job.AssignedToNavigation?.Name,
            CompanyName = job.Company?.Name,
            CompanyGstin = job.Company?.Gstin,
            CompanyAddress = job.Company?.AddressLine1,
            CompanyEmail = job.Company?.EmailId,
            CompanyPhone = job.Company?.ContactNo,
            CreatedByName = job.CreatedByNavigation?.Name ?? "",
            CreatedOn = job.CreatedOn?.ToString("dd-MMM-yyyy HH:mm"),
            Items = job.TrnJobItems
                .OrderBy(i => i.ItemSequence)
                .Select(i => new
                {
                    i.JobItemId,
                    i.ItemSequence,
                    i.ProductName,
                    i.ProductDescription,
                    i.ProductTypeName,
                    i.JobTypeName,
                    i.ProductSizeName,
                    i.TrimWidthMm,
                    i.TrimHeightMm,
                    i.PrintingMethod,
                    i.Quantity,
                    i.UomId,
                    i.NoOfPages,
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
                    i.Status,
                    i.Remarks,
                    i.EnquiryItemId,
                    i.QuotationItemId,
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
            Timeline = timeline
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
                    t.ProcessCode,
                    t.ProcessName,
                    t.CommunicationMode,
                    t.CommunicationReference,
                    t.AttachmentUrl,
                    CreatedOn = t.CreatedOn.ToString("dd-MMM-yyyy HH:mm")
                })
        };

        return Ok(result);
    }

    // ── Save Job ──
    [HttpPost("save")]
    public async Task<IActionResult> SaveJob([FromBody] JobSaveRequest request)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized(new { message = "Session expired. Please login again." });

        var jobNo = await _documentNumberService.GenerateNextNumberAsync(DocumentProcessCode.JOB_CARD);

        var job = new TrnJob
        {
            JobNo = jobNo,
            JobDate = DateOnly.FromDateTime(DateTime.Now),
            CompanyId = user.CompanyId ?? 1,
            LocationId = user.LocationId,
            PartyId = request.PartyId,
            JobTypeId = request.Items?
                .Where(i => i.JobTypeId.HasValue)
                .GroupBy(i => i.JobTypeId)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key,
            EnquiryId = request.EnquiryId,
            QuotationId = request.QuotationId,
            PartyRefNo = request.PartyRefNo,
            PartyRefNoDate = string.IsNullOrEmpty(request.PartyRefNoDate)
                ? null
                : DateTime.Parse(request.PartyRefNoDate),
            DeliveryDate = string.IsNullOrEmpty(request.DeliveryDate)
                ? null
                : DateOnly.Parse(request.DeliveryDate),
            ProductName = request.ProductName,
            ProductDescription = request.ProductDescription,
            Quantity = request.Quantity,
            TotalPages = request.TotalPages,
            Priority = request.Priority ?? "NORMAL",
            GrossAmount = request.GrossAmount,
            DiscountAmount = request.DiscountAmount,
            TaxableAmount = request.TaxableAmount,
            TaxAmount = request.TaxAmount,
            NetAmount = request.NetAmount,
            EstimatedCost = request.EstimatedCost,
            QuotedAmount = request.QuotedAmount,
            StatusCode = "CREATED",
            CurrentStage = "JOB_CREATED",
            ProgressPercent = 0,
            CreatedBy = user.UserId,
            CreatedOn = DateTime.Now
        };

        _db.TrnJobs.Add(job);
        await _db.SaveChangesAsync();

        // Save items
        if (request.Items?.Any() == true)
        {
            foreach (var item in request.Items)
            {
                var jobItem = new TrnJobItem
                {
                    JobId = job.JobId,
                    EnquiryItemId = item.EnquiryItemId,
                    QuotationItemId = item.QuotationItemId,
                    ItemSequence = item.ItemSequence,
                    PrintProductTypeId = item.PrintProductTypeId,
                    JobTypeId = item.JobTypeId,
                    ProductName = item.ProductName,
                    ProductDescription = item.ProductDescription,
                    ProductTypeName = item.ProductTypeName,
                    JobTypeName = item.JobTypeName,
                    ProductSizeName = item.ProductSizeName,
                    TrimWidthMm = item.TrimWidthMm,
                    TrimHeightMm = item.TrimHeightMm,
                    PrintingMethod = item.PrintingMethod,
                    Quantity = (item.Quantity.HasValue && item.Quantity.Value > 0) ? item.Quantity.Value : 1,
                    DeliveredQuantity = 0,
                    PendingQuantity = (item.Quantity.HasValue && item.Quantity.Value > 0) ? item.Quantity.Value : 1,
                    UomId = item.UomId,
                    NoOfPages = item.NoOfPages,
                    UnitRate = item.UnitRate,
                    GrossAmount = item.GrossAmount,
                    DiscountPercent = item.DiscountPercent,
                    DiscountAmount = item.DiscountAmount,
                    TaxableValue = item.TaxableValue,
                    TaxCategoryId = item.TaxCategoryId,
                    HsnSacCode = item.HsnSacCode,
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
                    Status = "PENDING",
                    Remarks = item.Remarks,
                    CreatedBy = user.UserId,
                    CreatedOn = DateTime.Now
                };

                _db.TrnJobItems.Add(jobItem);
            }

            await _db.SaveChangesAsync();

            // Link rate calculators to this job
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
                    rc.JobId = job.JobId;
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

            // Add enquiry timeline entry
            await AddEnquiryTimelineEntryAsync(
                request.EnquiryId.Value, "JOB_CREATED", "CONVERTED",
                "Converted to Job",
                $"Enquiry converted to Job {job.JobNo}. Net Amount: ₹{job.NetAmount:N2}.",
                oldStatus: enquiry?.Status, newStatus: "CONVERTED",
                userId: user.UserId);

            // ── Auto-complete all enquiry workspace tasks up to ENQ_JOB ──
            await _workspaceEngine.AutoCompleteProcessTasksAsync(
                sourceTable: "trn_enquiry",
                sourceId: request.EnquiryId.Value,
                upToProcessCode: "ENQ_JOB",
                remarks: $"Enquiry converted to Job {job.JobNo}. All preceding tasks auto-completed.",
                completedBy: user);
        }

        // If converted from quotation, update quotation status
        if (request.QuotationId.HasValue && request.QuotationId > 0)
        {
            var quotation = await _db.TrnQuotations.FindAsync(request.QuotationId.Value);
            if (quotation != null)
            {
                var oldStatus = quotation.Status;
                quotation.Status = "CONVERTED";
                quotation.ModifiedBy = user.UserId.ToString();
                quotation.ModifiedOn = DateTime.Now;
                await _db.SaveChangesAsync();

                // Add quotation timeline entry
                await AddQuotationTimelineEntryAsync(
                    request.QuotationId.Value, "JOB_CREATED", "CONVERTED",
                    "Converted to Job",
                    $"Quotation converted to Job {job.JobNo}.",
                    oldStatus: oldStatus, newStatus: "CONVERTED",
                    enquiryId: quotation.EnquiryId, userId: user.UserId);

                // ── Auto-complete all quotation workspace tasks up to ENQ_JOB ──
                await _workspaceEngine.AutoCompleteProcessTasksAsync(
                    sourceTable: "trn_quotation",
                    sourceId: request.QuotationId.Value,
                    upToProcessCode: "ENQ_JOB",
                    remarks: $"Quotation converted to Job {job.JobNo}. All preceding tasks auto-completed.",
                    completedBy: user);
            }
        }

        // ── Dispatch notification ──
        await DispatchJobNotificationAsync(job, user);

        // ── Generate ALL Workflow Tasks/Approvals Upfront ──
        // This creates all tasks for the complete job workflow with QUEUED status
        // Tasks are activated sequentially as each step is completed
        var workflowBatchId = await _workspaceEngine.GenerateAllWorkflowTasksAsync(
            sourceTable: WkSourceTable.Job,
            sourceId: job.JobId,
            sourceNo: job.JobNo,
            triggeredBy: user,
            jobId: job.JobId,
            jobNo: job.JobNo,
            jobTypeId: job.JobTypeId,
            partyId: request.PartyId,
            partyName: job.Party?.Name,
            actionUrl: $"/Job/Details?id={job.JobId}");

        // Fallback to single task creation if workflow template not found
        if (!workflowBatchId.HasValue)
        {
            await _workspaceEngine.CreateWorkspaceTaskAsync(
                processCode: WkProcessCode.JobCreate,
                eventTypeCode: WkEventTypeCode.ProcStart,
                sourceTable: WkSourceTable.Job,
                sourceId: job.JobId,
                sourceNo: job.JobNo,
                title: $"Job Card Generated – {job.JobNo}",
                description: $"New job {job.JobNo} for {job.Party?.Name ?? "customer"}. Product: {job.ProductName}.",
                taskType: WkTaskType.Task,
                priority: job.Priority ?? WkPriority.Normal,
                triggeredBy: user,
                jobId: job.JobId,
                jobNo: job.JobNo,
                partyName: job.Party?.Name,
                actionUrl: $"/Job/Details?id={job.JobId}",
                partyId: request.PartyId);
        }

        // ── Activity Log: Job Created ──
        var createActivity = ActivityLogEntry.FromUser(user, "JOB", "CREATE", $"Created Job {job.JobNo}");
        createActivity.EntityType = "JOB";
        createActivity.EntityId = job.JobId;
        createActivity.EntityCode = job.JobNo;
        createActivity.Description = $"Job {job.JobNo} created with {request.Items?.Count ?? 0} item(s). Net Amount: {job.NetAmount:N2}.{(workflowBatchId.HasValue ? $" Workflow batch: {workflowBatchId}" : "")}";
        createActivity.NewValues = JsonSerializer.Serialize(new { job.JobNo, job.PartyId, job.NetAmount, job.StatusCode, ItemCount = request.Items?.Count ?? 0, WorkflowBatchId = workflowBatchId });
        createActivity.Severity = "INFO";
        await _activityService.LogActivityAsync(createActivity);

        // ── Party Activity Log ──
        if (request.PartyId > 0)
        {
            await PartyPortalController.LogPartyActivityAsync(_db, request.PartyId,
                "JOB", "JOB_CREATED",
                $"Job {job.JobNo} Created",
                $"Job created with {request.Items?.Count ?? 0} item(s). Net Amount: ₹{job.NetAmount:N2}.",
                "trn_job", job.JobId, job.JobNo,
                job.JobDate, "Pending", "Not Required", job.NetAmount, user.Name);
        }

        // ── In-App Notification ──
        await _activityService.LogNotificationAsync(new UserNotificationEntry
        {
            UserId = user.UserId,
            Title = "New Job Created",
            Message = $"Job {job.JobNo} has been created. Net Amount: ₹{job.NetAmount:N2}.",
            Icon = "bi bi-briefcase-fill",
            Color = "primary",
            Module = "JOB",
            EventType = "CREATED",
            ReferenceId = (int)job.JobId,
            ReferenceUrl = $"/Job/Details?id={job.JobId}",
            Priority = "NORMAL"
        });

        // ── Job Timeline: CREATED ──
        await AddJobTimelineEntryAsync(
            job.JobId, "JOB_CREATED", "JOB_CREATED",
            "Job Created",
            $"Job {job.JobNo} created with {request.Items?.Count ?? 0} item(s). Net Amount: ₹{job.NetAmount:N2}.",
            newStatus: "CREATED", newAmount: job.NetAmount,
            enquiryId: request.EnquiryId, quotationId: request.QuotationId,
            userId: user.UserId);

        // ── If converted from enquiry, log in job timeline too ──
        if (request.EnquiryId.HasValue && request.EnquiryId > 0)
        {
            await AddJobTimelineEntryAsync(
                job.JobId, "CONVERTED_FROM_ENQUIRY", "CONVERTED_FROM_ENQUIRY",
                "Converted from Enquiry",
                $"Job {job.JobNo} was created from enquiry conversion.",
                newStatus: "CREATED", enquiryId: request.EnquiryId, userId: user.UserId);
        }

        // ── If converted from quotation, log in job timeline too ──
        if (request.QuotationId.HasValue && request.QuotationId > 0)
        {
            await AddJobTimelineEntryAsync(
                job.JobId, "CONVERTED_FROM_QUOTATION", "CONVERTED_FROM_QUOTATION",
                "Converted from Quotation",
                $"Job {job.JobNo} was created from quotation conversion.",
                newStatus: "CREATED", quotationId: request.QuotationId, userId: user.UserId);
        }

        return Ok(new { job.JobId, job.JobNo, message = "Job saved successfully." });
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
                    i.Quantity,
                    i.NoOfPages,
                    i.TrimWidthMm,
                    i.TrimHeightMm,
                    i.PrintingMethod,
                    i.RateCalculatorId,
                    PrintProductTypeId = i.RateCalculator != null ? i.RateCalculator.ProductTypeId : (int?)null,
                    JobTypeId = i.RateCalculator != null ? i.RateCalculator.JobTypeId : (int?)null,
                    CalcRefNo = i.RateCalculator != null ? i.RateCalculator.CalcRefNo : null,
                    CostPerUnit = i.RateCalculator != null ? i.RateCalculator.CostPerUnit : (decimal?)null,
                    GrandTotal = i.RateCalculator != null ? i.RateCalculator.GrandTotal : (decimal?)null,
                    TaxAmount = i.RateCalculator != null ? i.RateCalculator.TaxAmount : (decimal?)null,
                    NetTotal = i.RateCalculator != null ? i.RateCalculator.NetTotal : (decimal?)null
                })
        };

        return Ok(result);
    }

    // ── Convert from Quotation ──
    [HttpGet("from-quotation/{quotationId:long}")]
    public async Task<IActionResult> GetQuotationDataForConversion(long quotationId)
    {
        var quotation = await _db.TrnQuotations
            .Include(q => q.Party)
            .Include(q => q.TrnQuotationItems)
                .ThenInclude(i => i.RateCalculator)
            .Include(q => q.TrnQuotationItems)
                .ThenInclude(i => i.EnquiryItem)
            .FirstOrDefaultAsync(q => q.QuotationId == quotationId);

        if (quotation == null)
            return NotFound(new { message = "Quotation not found." });

        var result = new
        {
            quotation.QuotationId,
            quotation.QuotationNo,
            quotation.PartyId,
            CustomerName = quotation.Party.Name,
            CustomerCode = quotation.Party.Code,
            CustomerEmail = quotation.Party.Email,
            CustomerGst = quotation.Party.Gstno,
            quotation.EnquiryId,
            quotation.TotalAmount,
            quotation.DiscountAmount,
            quotation.TaxableAmount,
            quotation.TaxAmount,
            quotation.NetAmount,
            quotation.TermsConditions,
            quotation.Remarks,
            Items = quotation.TrnQuotationItems
                .OrderBy(i => i.ItemSequence)
                .Select(i => new
                {
                    i.QuotationItemId,
                    i.EnquiryItemId,
                    i.ItemSequence,
                    i.ProductName,
                    i.ProductDescription,
                    i.Quantity,
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
                    // Source product detail fields from EnquiryItem or RateCalculator
                    ProductTypeName = i.EnquiryItem != null ? i.EnquiryItem.ProductTypeName : null,
                    JobTypeName = i.EnquiryItem != null ? i.EnquiryItem.JobTypeName : null,
                    ProductSizeName = i.EnquiryItem != null ? i.EnquiryItem.ProductSizeName : null,
                    NoOfPages = i.EnquiryItem != null ? i.EnquiryItem.NoOfPages : (i.RateCalculator != null ? (int?)i.RateCalculator.TotalPages : null),
                    TrimWidthMm = i.EnquiryItem != null ? i.EnquiryItem.TrimWidthMm : (i.RateCalculator != null ? i.RateCalculator.TrimWidthMm : null),
                    TrimHeightMm = i.EnquiryItem != null ? i.EnquiryItem.TrimHeightMm : (i.RateCalculator != null ? i.RateCalculator.TrimHeightMm : null),
                    PrintingMethod = i.EnquiryItem != null ? i.EnquiryItem.PrintingMethod : (i.RateCalculator != null ? i.RateCalculator.PrintingMode : null),
                    PrintProductTypeId = i.PrintProductTypeId ?? (i.RateCalculator != null ? i.RateCalculator.ProductTypeId : (int?)null),
                    JobTypeId = i.JobTypeId ?? (i.RateCalculator != null ? i.RateCalculator.JobTypeId : (int?)null),
                    CostPerUnit = i.RateCalculator != null ? i.RateCalculator.CostPerUnit : (decimal?)null,
                    GrandTotal = i.RateCalculator != null ? i.RateCalculator.GrandTotal : (decimal?)null
                })
        };

        return Ok(result);
    }

    // ── Update Job Status ──
    [HttpPost("updatestatus")]
    public async Task<IActionResult> UpdateJobStatus([FromBody] JobStatusRequest request)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized(new { message = "Session expired. Please login again." });

        var job = await _db.TrnJobs.FindAsync(request.JobId);
        if (job == null)
            return NotFound(new { message = "Job not found." });

        var oldStatus = job.StatusCode;
        job.StatusCode = request.Status;
        job.CurrentStage = request.Status;
        job.ModifiedBy = user.UserId.ToString();
        job.ModifiedOn = DateTime.Now;

        if (request.Status == "DELIVERED" || request.Status == "JOB_CANCELLED")
        {
            job.ClosedOn = DateTime.Now;
            job.ClosedBy = user.UserId;
        }
        if (request.Status == "DELIVERED")
        {
            job.CompletedOn = DateTime.Now;
            job.ProgressPercent = 100;
        }

        await _db.SaveChangesAsync();

        // ── Cross-update Enquiry Timeline ──
        if (job.EnquiryId.HasValue && job.EnquiryId > 0)
        {
            await AddEnquiryTimelineEntryAsync(
                job.EnquiryId.Value, "JOB_STATUS_CHANGED", request.Status,
                $"Job {job.JobNo} — {request.Status}",
                $"Job {job.JobNo} status changed from {oldStatus ?? "N/A"} to {request.Status}.",
                oldStatus: oldStatus, newStatus: request.Status,
                userId: user.UserId);
        }

        // ── Cross-update Quotation Timeline ──
        if (job.QuotationId.HasValue && job.QuotationId > 0)
        {
            await AddQuotationTimelineEntryAsync(
                job.QuotationId.Value, "JOB_STATUS_CHANGED", request.Status,
                $"Job {job.JobNo} — {request.Status}",
                $"Job {job.JobNo} status changed from {oldStatus ?? "N/A"} to {request.Status}.",
                oldStatus: oldStatus, newStatus: request.Status,
                enquiryId: job.EnquiryId, userId: user.UserId);
        }

        // ── Dispatch notification ──
        await DispatchJobStatusNotificationAsync(job, user, request.Status);

        // ── Activity Log ──
        var statusActivity = ActivityLogEntry.FromUser(user, "JOB", "STATUS_CHANGE", $"Job {job.JobNo} status changed to {request.Status}");
        statusActivity.EntityType = "JOB";
        statusActivity.EntityId = job.JobId;
        statusActivity.EntityCode = job.JobNo;
        statusActivity.Description = $"Status changed from {oldStatus ?? "N/A"} to {request.Status} by {user.Name}.";
        statusActivity.OldValues = JsonSerializer.Serialize(new { Status = oldStatus });
        statusActivity.NewValues = JsonSerializer.Serialize(new { Status = request.Status });
        statusActivity.ChangedFields = ["Status"];
        statusActivity.Severity = request.Status is "JOB_CANCELLED" or "JOB_ON_HOLD" ? "WARNING" : "INFO";
        await _activityService.LogActivityAsync(statusActivity);

        // ── Party Activity Log: Status Change ──
        if (job.PartyId > 0)
        {
            var jobApproval = request.Status switch
            {
                "DELIVERED" => "Approved",
                "JOB_CANCELLED" => "Rejected",
                _ => "Not Required"
            };
            var jobStatus = request.Status switch
            {
                "DELIVERED" => "Completed",
                "JOB_CANCELLED" => "Cancelled",
                _ => "Pending"
            };
            await PartyPortalController.LogPartyActivityAsync(_db, job.PartyId.Value,
                "JOB", $"JOB_{request.Status}",
                $"Job {job.JobNo} — {request.Status.Replace('_', ' ')}",
                $"Status changed from {oldStatus ?? "N/A"} to {request.Status}.",
                "trn_job", job.JobId, job.JobNo,
                job.JobDate, jobStatus, jobApproval, job.NetAmount, user.Name);
        }

        // ── In-App Notification ──
        await _activityService.LogNotificationAsync(new UserNotificationEntry
        {
            UserId = user.UserId,
            Title = $"Job {request.Status}",
            Message = $"Job {job.JobNo} has been updated to {request.Status.ToLower().Replace('_', ' ')}.",
            Icon = request.Status switch
            {
                "JOB_ASSIGNED" => "bi bi-person-check",
                "PRINTING_STARTED" => "bi bi-printer",
                "PRINTING_COMPLETED" => "bi bi-printer-fill",
                "DISPATCHED" => "bi bi-truck",
                "DELIVERED" => "bi bi-check-circle",
                "JOB_CANCELLED" => "bi bi-x-circle",
                "JOB_ON_HOLD" => "bi bi-pause-circle",
                _ => "bi bi-arrow-repeat"
            },
            Color = request.Status switch
            {
                "DELIVERED" => "success",
                "JOB_CANCELLED" => "danger",
                "JOB_ON_HOLD" => "warning",
                "DISPATCHED" => "info",
                _ => "primary"
            },
            Module = "JOB",
            EventType = "STATUS_CHANGED",
            ReferenceId = (int)job.JobId,
            ReferenceUrl = $"/Job/Details?id={job.JobId}"
        });

        // ── Job Timeline ──
        await AddJobTimelineEntryAsync(
            job.JobId, request.Status, request.Status,
            $"Status Changed to {request.Status.Replace('_', ' ')}",
            $"Status changed from {oldStatus ?? "N/A"} to {request.Status} by {user.Name}.",
            oldStatus: oldStatus, newStatus: request.Status,
            processCode: request.Status, processName: request.Status.Replace('_', ' '),
            enquiryId: job.EnquiryId, quotationId: job.QuotationId,
            userId: user.UserId);

        return Ok(new { message = $"Job status updated to {request.Status}." });
    }

    // ── Delete Job ──
    [HttpDelete("delete/{id:long}")]
    public async Task<IActionResult> DeleteJob(long id)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized(new { message = "Session expired. Please login again." });

        var job = await _db.TrnJobs
            .Include(j => j.TrnJobItems)
            .FirstOrDefaultAsync(j => j.JobId == id);

        if (job == null)
            return NotFound(new { message = "Job not found." });

        if (job.StatusCode != "CREATED")
            return BadRequest(new { message = "Only CREATED jobs can be deleted." });

        var jobNo = job.JobNo;
        var jobId = job.JobId;

        _db.TrnJobItems.RemoveRange(job.TrnJobItems);
        _db.TrnJobs.Remove(job);
        await _db.SaveChangesAsync();

        // ── Activity Log ──
        var deleteActivity = ActivityLogEntry.FromUser(user, "JOB", "DELETE", $"Deleted Job {jobNo}");
        deleteActivity.EntityType = "JOB";
        deleteActivity.EntityId = jobId;
        deleteActivity.EntityCode = jobNo;
        deleteActivity.Description = $"Job {jobNo} (CREATED) was deleted by {user.Name}.";
        deleteActivity.OldValues = JsonSerializer.Serialize(new { jobNo, job.PartyId, job.StatusCode, job.NetAmount });
        deleteActivity.Severity = "WARNING";
        await _activityService.LogActivityAsync(deleteActivity);

        return Ok(new { message = "Job deleted successfully." });
    }

    // ── Send Job Email to Customer ──
    [HttpPost("send-email/{id:long}")]
    public async Task<IActionResult> SendJobEmail(long id)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized(new { message = "Session expired. Please login again." });

        var job = await _db.TrnJobs
            .Include(j => j.Party)
            .Include(j => j.Company)
            .Include(j => j.TrnJobItems)
            .FirstOrDefaultAsync(j => j.JobId == id);

        if (job == null)
            return NotFound(new { message = "Job not found." });

        var customerEmail = job.Party?.Email;
        if (string.IsNullOrWhiteSpace(customerEmail))
            return BadRequest(new { message = "Customer does not have an email address on file." });

        try
        {
            var emailHtml = BuildJobEmailHtml(job);

            var config = new ProcessNotificationConfig
            {
                ConfigId = 1,
                ProcessCode = nameof(ProcessCode.EnqJob),
                SubProcessCode = notification.Enums.SubProcessCode.ReceiveEnq,
                EventType = NotificationEventType.TaskAssign,
                EventLabel = "Job Confirmation to Customer",
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
                TemplateName = "Job Confirmation to Customer",
                Module = nameof(NotificationModule.Quotation),
                EventType = nameof(NotificationEventType.TaskAssign),
                Channel = NotificationChannel.Email,
                SubjectTemplate = $"Job Confirmation {job.JobNo} from {job.Company?.Name ?? "MinePress"}",
                BodyTemplate = emailHtml,
                IsActive = true
            };

            var context = new NotificationContext
            {
                ThreadKey = $"JOB:{job.JobNo}",
                AssigneeUserId = (int)user.UserId,
                AssigneeEmail = user.EmailId,
                ClientEmail = customerEmail,
                Variables = new Dictionary<string, string>
                {
                    ["job_no"] = job.JobNo,
                    ["customer_name"] = job.Party?.Name ?? "N/A"
                }
            };

            var results = await _notificationDispatcher.DispatchAsync(config, template, context);
            var emailResult = results.FirstOrDefault(r => r.Channel == NotificationChannel.Email);

            if (emailResult?.IsSuccess == true)
            {
                // ── Activity Log ──
                var emailActivity = ActivityLogEntry.FromUser(user, "JOB", "EMAIL_SENT", $"Job {job.JobNo} emailed to customer");
                emailActivity.EntityType = "JOB";
                emailActivity.EntityId = job.JobId;
                emailActivity.EntityCode = job.JobNo;
                emailActivity.Description = $"Job {job.JobNo} emailed to {customerEmail}.";
                emailActivity.NewValues = JsonSerializer.Serialize(new { CustomerEmail = customerEmail, job.NetAmount });
                await _activityService.LogActivityAsync(emailActivity);

                // ── In-App Notification ──
                await _activityService.LogNotificationAsync(new UserNotificationEntry
                {
                    UserId = user.UserId,
                    Title = "Job Emailed",
                    Message = $"Job {job.JobNo} confirmation sent to {customerEmail}.",
                    Icon = "bi bi-envelope-check",
                    Color = "success",
                    Module = "JOB",
                    EventType = "EMAIL_SENT",
                    ReferenceId = (int)job.JobId,
                    ReferenceUrl = $"/Job/Details?id={job.JobId}"
                });

                // ── Job Timeline ──
                await AddJobTimelineEntryAsync(
                    job.JobId, "SENT_TO_CUSTOMER", "EMAIL",
                    "Job Confirmation Emailed to Customer",
                    $"Job {job.JobNo} confirmation sent to {customerEmail}.",
                    communicationMode: "EMAIL", communicationReference: customerEmail,
                    userId: user.UserId);

                return Ok(new { message = $"Job emailed to {customerEmail} successfully." });
            }
            else
            {
                return StatusCode(500, new { message = $"Failed to send email: {emailResult?.ErrorMessage ?? "Unknown error"}" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send job email for {JobNo}", job.JobNo);
            await AuditExceptionAsync(ex, $"JobController.SendJobEmail jobNo={job.JobNo}");
            return StatusCode(500, new { message = $"Failed to send email: {ex.Message}" });
        }
    }

    // ── Job Timeline ──
    [HttpGet("timeline/{jobId:long}")]
    public async Task<IActionResult> GetJobTimeline(long jobId)
    {
        var timeline = await _db.TrnJobTimelines
            .Where(t => t.JobId == jobId && t.IsActive == true)
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

    // ── Company Info ──
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

    // ── Build Job Email HTML ──
    private string BuildJobEmailHtml(TrnJob job)
    {
        var company = job.Company;
        var party = job.Party;
        var items = job.TrnJobItems.OrderBy(i => i.ItemSequence).ToList();

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

        return $@"
<!DOCTYPE html>
<html>
<head><meta charset=""utf-8""/></head>
<body style=""margin:0;padding:0;background:#f0f2f5;font-family:'Segoe UI',Tahoma,Geneva,Verdana,sans-serif;"">
<div style=""max-width:700px;margin:20px auto;background:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,0.08);"">

    <!-- Header -->
    <div style=""background:linear-gradient(135deg,#6f42c1,#9b59b6);padding:28px 32px;color:#ffffff;"">
        <table width=""100%"" cellpadding=""0"" cellspacing=""0""><tr>
            <td>
                <h1 style=""margin:0;font-size:22px;font-weight:700;letter-spacing:-0.5px;"">{company?.Name ?? "MinePress ERP"}</h1>
                <p style=""margin:4px 0 0 0;font-size:13px;opacity:0.85;"">{company?.AddressLine1 ?? ""}</p>
                {(company?.Gstin != null ? $"<p style=\"margin:2px 0 0 0;font-size:12px;opacity:0.75;\">GSTIN: {company.Gstin}</p>" : "")}
            </td>
            <td style=""text-align:right;vertical-align:top;"">
                <div style=""background:rgba(255,255,255,0.2);border-radius:8px;padding:12px 16px;display:inline-block;"">
                    <div style=""font-size:11px;text-transform:uppercase;letter-spacing:1px;opacity:0.85;"">JOB</div>
                    <div style=""font-size:20px;font-weight:700;margin-top:2px;"">{job.JobNo}</div>
                </div>
            </td>
        </tr></table>
    </div>

    <!-- Meta Info -->
    <div style=""padding:20px 32px;background:#f8f9fa;border-bottom:1px solid #e9ecef;"">
        <table width=""100%"" cellpadding=""0"" cellspacing=""0""><tr>
            <td style=""width:50%;vertical-align:top;"">
                <div style=""font-size:11px;text-transform:uppercase;letter-spacing:1px;color:#6c757d;margin-bottom:6px;"">CUSTOMER</div>
                <div style=""font-size:15px;font-weight:600;color:#1a1a2e;"">{party?.Name ?? "Customer"}</div>
                {(party?.Address1 != null ? $"<div style=\"font-size:13px;color:#495057;margin-top:2px;\">{party.Address1}</div>" : "")}
                {(party?.Gstno != null ? $"<div style=\"font-size:12px;color:#6c757d;margin-top:2px;\">GSTIN: {party.Gstno}</div>" : "")}
            </td>
            <td style=""width:50%;vertical-align:top;text-align:right;"">
                <table cellpadding=""0"" cellspacing=""0"" style=""margin-left:auto;"">
                    <tr><td style=""font-size:12px;color:#6c757d;padding:2px 12px 2px 0;"">Date:</td><td style=""font-size:12px;font-weight:600;color:#1a1a2e;"">{job.JobDate:dd-MMM-yyyy}</td></tr>
                    {(job.DeliveryDate.HasValue ? $"<tr><td style=\"font-size:12px;color:#6c757d;padding:2px 12px 2px 0;\">Delivery:</td><td style=\"font-size:12px;font-weight:600;color:#1a1a2e;\">{job.DeliveryDate:dd-MMM-yyyy}</td></tr>" : "")}
                    {(!string.IsNullOrEmpty(job.PartyRefNo) ? $"<tr><td style=\"font-size:12px;color:#6c757d;padding:2px 12px 2px 0;\">Your Ref:</td><td style=\"font-size:12px;font-weight:600;color:#1a1a2e;\">{job.PartyRefNo}</td></tr>" : "")}
                    <tr><td style=""font-size:12px;color:#6c757d;padding:2px 12px 2px 0;"">Priority:</td><td style=""font-size:12px;font-weight:600;color:#1a1a2e;"">{job.Priority ?? "NORMAL"}</td></tr>
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
                        <td style=""padding:6px 0;text-align:right;font-weight:600;"">₹{job.GrossAmount:N2}</td>
                    </tr>
                    {(job.DiscountAmount > 0 ? $@"<tr>
                        <td style=""padding:6px 0;color:#6c757d;"">Discount</td>
                        <td style=""padding:6px 0;text-align:right;color:#dc3545;font-weight:600;"">-₹{job.DiscountAmount:N2}</td>
                    </tr>" : "")}
                    <tr>
                        <td style=""padding:6px 0;color:#6c757d;"">Tax</td>
                        <td style=""padding:6px 0;text-align:right;font-weight:600;"">₹{job.TaxAmount:N2}</td>
                    </tr>
                    <tr style=""border-top:2px solid #1a1a2e;"">
                        <td style=""padding:10px 0;font-size:16px;font-weight:700;color:#1a1a2e;"">Net Amount</td>
                        <td style=""padding:10px 0;text-align:right;font-size:18px;font-weight:700;color:#6f42c1;"">₹{job.NetAmount:N2}</td>
                    </tr>
                </table>
            </td></tr>
        </table>
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

    private async Task DispatchJobNotificationAsync(TrnJob job, UserSessionData user)
    {
        try
        {
            var config = new ProcessNotificationConfig
            {
                ConfigId = 1,
                ProcessCode = nameof(ProcessCode.EnqJob),
                SubProcessCode = notification.Enums.SubProcessCode.ReceiveEnq,
                EventType = NotificationEventType.TaskAssign,
                EventLabel = "New Job Created",
                RecipientType = RecipientType.Internal,
                NotifyAssignee = true,
                NotifyDeptHead = true,
                NotifyInternalEmail = true,
                NotifyPush = true,
                TemplateCode = nameof(NotificationTemplateCode.TaskAssigned),
                Priority = NotificationPriority.Normal,
                IsActive = true,
                TriggerOnStatus = "CREATED",
                AutoTrigger = true
            };

            var template = new NotificationTemplate
            {
                TemplateId = 1,
                TemplateCode = nameof(NotificationTemplateCode.TaskAssigned),
                TemplateName = "Job Created",
                Module = nameof(NotificationModule.Quotation),
                EventType = nameof(NotificationEventType.TaskAssign),
                Channel = NotificationChannel.Email,
                SubjectTemplate = "New Job {{job_no}} — ₹{{net_amount}}",
                BodyTemplate = """
                    <h3>New Job Created</h3>
                    <p><strong>Job No:</strong> {{job_no}}</p>
                    <p><strong>Customer:</strong> {{customer_name}}</p>
                    <p><strong>Net Amount:</strong> ₹{{net_amount}}</p>
                    <p><strong>Priority:</strong> {{priority}}</p>
                    <p><strong>Created By:</strong> {{created_by}}</p>
                    <p>Please review and take action.</p>
                    """,
                IsActive = true
            };

            var context = new NotificationContext
            {
                ThreadKey = $"JOB:{job.JobNo}",
                AssigneeUserId = (int)user.UserId,
                AssigneeEmail = user.EmailId,
                AssigneePhone = user.MobileNo,
                Variables = new Dictionary<string, string>
                {
                    ["job_no"] = job.JobNo,
                    ["customer_name"] = job.Party?.Name ?? "N/A",
                    ["net_amount"] = job.NetAmount?.ToString("N2") ?? "0.00",
                    ["priority"] = job.Priority ?? "NORMAL",
                    ["created_by"] = user.Name,
                    ["job_date"] = job.JobDate.ToString("dd-MMM-yyyy")
                }
            };

            var results = await _notificationDispatcher.DispatchAsync(config, template, context);
            _logger.LogInformation(
                "Job {JobNo}: Dispatched {Count} notifications, {Success} succeeded",
                job.JobNo, results.Count, results.Count(r => r.IsSuccess));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to dispatch notification for job {JobNo}", job.JobNo);
            await AuditExceptionAsync(ex, $"JobController.DispatchJobNotificationAsync jobNo={job.JobNo}");
        }
    }

    private async Task DispatchJobStatusNotificationAsync(TrnJob job, UserSessionData user, string newStatus)
    {
        try
        {
            var config = new ProcessNotificationConfig
            {
                ConfigId = 1,
                ProcessCode = nameof(ProcessCode.EnqJob),
                SubProcessCode = notification.Enums.SubProcessCode.ReceiveEnq,
                EventType = NotificationEventType.TaskAssign,
                EventLabel = $"Job Status Changed to {newStatus}",
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
                TemplateName = "Job Status Update",
                Module = nameof(NotificationModule.Quotation),
                EventType = nameof(NotificationEventType.TaskAssign),
                Channel = NotificationChannel.Email,
                SubjectTemplate = "Job {{job_no}} — Status Updated to {{new_status}}",
                BodyTemplate = """
                    <h3>Job Status Updated</h3>
                    <p><strong>Job No:</strong> {{job_no}}</p>
                    <p><strong>New Status:</strong> {{new_status}}</p>
                    <p><strong>Updated By:</strong> {{updated_by}}</p>
                    <p>Please review and take necessary action.</p>
                    """,
                IsActive = true
            };

            var context = new NotificationContext
            {
                ThreadKey = $"JOB:{job.JobNo}",
                AssigneeUserId = (int)user.UserId,
                AssigneeEmail = user.EmailId,
                AssigneePhone = user.MobileNo,
                Variables = new Dictionary<string, string>
                {
                    ["job_no"] = job.JobNo,
                    ["new_status"] = newStatus,
                    ["updated_by"] = user.Name,
                    ["job_date"] = job.JobDate.ToString("dd-MMM-yyyy")
                }
            };

            var results = await _notificationDispatcher.DispatchAsync(config, template, context);
            _logger.LogInformation(
                "Job {JobNo} status update to {Status}: Dispatched {Count} notifications",
                job.JobNo, newStatus, results.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to dispatch status notification for job {JobNo}", job.JobNo);
            await AuditExceptionAsync(ex, $"JobController.DispatchJobStatusNotificationAsync jobNo={job.JobNo}");
        }
    }

    // ── Job Timeline Helper ──

    private async Task AddJobTimelineEntryAsync(
        long jobId, string eventType, string? eventCode,
        string eventTitle, string? eventDescription,
        string? oldStatus = null, string? newStatus = null,
        decimal? oldAmount = null, decimal? newAmount = null,
        string? remarks = null, long? enquiryId = null, long? quotationId = null,
        string? communicationMode = null, string? communicationReference = null,
        string? processCode = null, string? processName = null,
        long userId = 0)
    {
        try
        {
            var entry = new TrnJobTimeline
            {
                JobId = jobId,
                EnquiryId = enquiryId,
                QuotationId = quotationId,
                EventType = eventType,
                EventCode = eventCode,
                EventTitle = eventTitle,
                EventDescription = eventDescription,
                Remarks = remarks,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                OldAmount = oldAmount,
                NewAmount = newAmount,
                ProcessCode = processCode,
                ProcessName = processName,
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
            await AuditExceptionAsync(ex, $"JobController.AddJobTimelineEntryAsync jobId={jobId} eventType={eventType}");
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
            await AuditExceptionAsync(ex, $"JobController.AddEnquiryTimelineEntryAsync enquiryId={enquiryId} eventType={eventType}");
        }
    }

    // ── Quotation Timeline Helper (for cross-module logging) ──

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
            await AuditExceptionAsync(ex, $"JobController.AddQuotationTimelineEntryAsync quotationId={quotationId} eventType={eventType}");
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

public class JobSaveRequest
{
    public int PartyId { get; set; }
    public long? EnquiryId { get; set; }
    public long? QuotationId { get; set; }
    public string? PartyRefNo { get; set; }
    public string? PartyRefNoDate { get; set; }
    public string? DeliveryDate { get; set; }
    public string? ProductName { get; set; }
    public string? ProductDescription { get; set; }
    public int Quantity { get; set; }
    public int? TotalPages { get; set; }
    public string? Priority { get; set; }
    public decimal? EstimatedCost { get; set; }
    public decimal? QuotedAmount { get; set; }
    public decimal? GrossAmount { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? TaxableAmount { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? NetAmount { get; set; }
    public string? Remarks { get; set; }
    public List<JobItemRequest>? Items { get; set; }
}

public class JobItemRequest
{
    public long? EnquiryItemId { get; set; }
    public long? QuotationItemId { get; set; }
    public int ItemSequence { get; set; }
    public int? PrintProductTypeId { get; set; }
    public int? JobTypeId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductDescription { get; set; }
    public string? ProductTypeName { get; set; }
    public string? JobTypeName { get; set; }
    public string? ProductSizeName { get; set; }
    public decimal? TrimWidthMm { get; set; }
    public decimal? TrimHeightMm { get; set; }
    public string? PrintingMethod { get; set; }
    public int? Quantity { get; set; }
    public int? UomId { get; set; }
    public int? NoOfPages { get; set; }
    public decimal? UnitRate { get; set; }
    public decimal? GrossAmount { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? TaxableValue { get; set; }
    public int? TaxCategoryId { get; set; }
    public string? HsnSacCode { get; set; }
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

public class JobStatusRequest
{
    public long JobId { get; set; }
    public string Status { get; set; } = string.Empty;
}
