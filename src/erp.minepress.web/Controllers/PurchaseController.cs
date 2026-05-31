using erp.minepress.domain.Enums;
using erp.minepress.infrastructure.ErrorLogging;
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
public class PurchaseController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IUserActivityService _activityService;
    private readonly IDocumentNumberService _documentNumberService;
    private readonly ILogger<PurchaseController> _logger;
    private readonly ISystemErrorLogger _systemErrorLogger;

    public PurchaseController(
        ApplicationDbContext db,
        IUserActivityService activityService,
        IDocumentNumberService documentNumberService,
        ILogger<PurchaseController> logger,
        ISystemErrorLogger systemErrorLogger)
    {
        _db = db;
        _activityService = activityService;
        _documentNumberService = documentNumberService;
        _logger = logger;
        _systemErrorLogger = systemErrorLogger;
    }

    private async Task AuditExceptionAsync(Exception ex, string additionalData, string severity = "Error")
        => await _systemErrorLogger.LogAsync(ex, HttpContext, severity, additionalData);

    // ── Dashboard Stats ──
    [HttpGet("dashboard-stats")]
    public async Task<IActionResult> GetDashboardStats()
    {
        var totalGrns = await _db.TrnPurchaseGrns.CountAsync();
        var draftGrns = await _db.TrnPurchaseGrns.CountAsync(g => g.Status == "DRAFT");
        var receivedGrns = await _db.TrnPurchaseGrns.CountAsync(g => g.Status == "RECEIVED");
        var inspectedGrns = await _db.TrnPurchaseGrns.CountAsync(g => g.Status == "INSPECTED");

        return Ok(new { totalGrns, draftGrns, receivedGrns, inspectedGrns });
    }

    // ── GRN List ──
    [HttpGet("grns")]
    public async Task<IActionResult> GetGrnList()
    {
        var list = await _db.TrnPurchaseGrns
            .Include(g => g.TrnPurchaseGrnItems)
            .OrderByDescending(g => g.GrnId)
            .Select(g => new
            {
                g.GrnId,
                g.GrnNo,
                GrnDate = g.GrnDate.ToString("dd-MMM-yyyy"),
                g.GrnType,
                g.JobNo,
                g.SupplierName,
                g.InvoiceNo,
                g.Status,
                g.QualityStatus,
                ItemCount = g.TrnPurchaseGrnItems.Count,
                g.TotalAmount,
                g.TaxAmount,
                g.NetAmount,
                g.Remarks,
                CreatedOn = g.CreatedOn.HasValue ? g.CreatedOn.Value.ToString("dd-MMM-yyyy HH:mm") : ""
            })
            .ToListAsync();

        return Ok(list);
    }

    // ── Save GRN ──
    [HttpPost("grns/save")]
    public async Task<IActionResult> SaveGrn([FromBody] PurchaseGrnSaveRequest request)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized(new { message = "Session expired. Please login again." });

        var grnNo = await _documentNumberService.GenerateNextNumberAsync(DocumentProcessCode.PURCHASE_GRN);

        var grn = new TrnPurchaseGrn
        {
            GrnNo = grnNo,
            GrnDate = DateOnly.FromDateTime(DateTime.Now),
            GrnType = request.GrnType ?? "JOB",
            JobId = request.JobId,
            JobNo = request.JobNo,
            RateCalcId = request.RateCalcId,
            PurchaseOrderId = request.PurchaseOrderId,
            PurchaseOrderNo = request.PurchaseOrderNo,
            SupplierId = request.SupplierId,
            SupplierName = request.SupplierName,
            InvoiceNo = request.InvoiceNo,
            InvoiceDate = string.IsNullOrEmpty(request.InvoiceDate) ? null : DateOnly.Parse(request.InvoiceDate),
            LocationId = request.LocationId,
            CompanyId = user.CompanyId ?? 1,
            Status = "DRAFT",
            QualityStatus = "PENDING",
            Remarks = request.Remarks,
            CreatedBy = user.UserId,
            CreatedOn = DateTime.Now
        };

        _db.TrnPurchaseGrns.Add(grn);
        await _db.SaveChangesAsync();

        if (request.Items?.Any() == true)
        {
            int seq = 1;
            decimal totalAmount = 0;
            decimal totalTax = 0;
            foreach (var item in request.Items)
            {
                var amount = (item.ReceivedQuantity) * (item.Rate ?? 0);
                var taxAmount = amount * (item.TaxRate ?? 18) / 100;
                var grnItem = new TrnPurchaseGrnItem
                {
                    GrnId = grn.GrnId,
                    ItemSequence = seq++,
                    MaterialCategory = item.MaterialCategory,
                    MaterialId = item.MaterialId,
                    MaterialCode = item.MaterialCode,
                    MaterialName = item.MaterialName,
                    Specification = item.Specification,
                    BomQuantity = item.BomQuantity,
                    OrderedQuantity = item.OrderedQuantity,
                    ReceivedQuantity = item.ReceivedQuantity,
                    RejectedQuantity = item.RejectedQuantity,
                    AcceptedQuantity = item.AcceptedQuantity,
                    Uom = item.Uom,
                    Rate = item.Rate,
                    Amount = amount,
                    TaxRate = item.TaxRate,
                    TaxAmount = taxAmount,
                    NetAmount = amount + taxAmount,
                    BatchNo = item.BatchNo,
                    AvailableStock = item.AvailableStock,
                    ForPart = item.ForPart,
                    QualityStatus = "PENDING",
                    Remarks = item.Remarks,
                    IsSelected = item.IsSelected ?? true,
                    CreatedOn = DateTime.Now
                };
                _db.TrnPurchaseGrnItems.Add(grnItem);
                totalAmount += amount;
                totalTax += taxAmount;
            }

            grn.TotalItems = request.Items.Count;
            grn.TotalAmount = totalAmount;
            grn.TaxAmount = totalTax;
            grn.NetAmount = totalAmount + totalTax;
            await _db.SaveChangesAsync();
        }

        // Timeline
        await AddTimelineEntryAsync("PURCHASE_GRN", grn.GrnId, "CREATED", "CREATED",
            "Purchase GRN Created",
            $"GRN {grn.GrnNo} created with {request.Items?.Count ?? 0} item(s). Supplier: {grn.SupplierName ?? "N/A"}.",
            newStatus: "DRAFT", userId: user.UserId);

        // Activity Log
        var activity = ActivityLogEntry.FromUser(user, "PURCHASE", "CREATE", $"Created Purchase GRN {grn.GrnNo}");
        activity.EntityType = "PURCHASE_GRN";
        activity.EntityId = grn.GrnId;
        activity.EntityCode = grn.GrnNo;
        activity.Description = $"GRN {grn.GrnNo} created. Supplier: {grn.SupplierName ?? "N/A"}, Job: {grn.JobNo ?? "N/A"}, Net: {grn.NetAmount:N2}.";
        activity.NewValues = JsonSerializer.Serialize(new { grn.GrnNo, grn.GrnType, grn.SupplierName, grn.Status, grn.NetAmount });
        await _activityService.LogActivityAsync(activity);

        // ── Party Activity Log ──
        if (grn.SupplierId.HasValue && grn.SupplierId > 0)
        {
            await PartyPortalController.LogPartyActivityAsync(_db, grn.SupplierId.Value,
                "PURCHASE", "PURCHASE_GRN_CREATED",
                $"Purchase GRN {grn.GrnNo} Created",
                $"GRN created with {request.Items?.Count ?? 0} item(s). Net Amount: ₹{grn.NetAmount:N2}.",
                "trn_purchase_grn", grn.GrnId, grn.GrnNo,
                grn.GrnDate, "Draft", "Not Required", grn.NetAmount, user.Name);
        }

        await _activityService.LogNotificationAsync(new UserNotificationEntry
        {
            UserId = user.UserId,
            Title = "Purchase GRN Created",
            Message = $"GRN {grn.GrnNo} has been created successfully.",
            Icon = "bi bi-box-seam",
            Color = "info",
            Module = "PURCHASE",
            EventType = "CREATED",
            ReferenceId = (int)grn.GrnId,
            ReferenceUrl = $"/Store/Purchase/Details?id={grn.GrnId}",
            Priority = "NORMAL"
        });

        return Ok(new { grn.GrnId, grn.GrnNo, message = "Purchase GRN saved successfully." });
    }

    // ── GRN Detail ──
    [HttpGet("grns/{id:long}")]
    public async Task<IActionResult> GetGrnDetail(long id)
    {
        var grn = await _db.TrnPurchaseGrns
            .Include(g => g.TrnPurchaseGrnItems)
            .FirstOrDefaultAsync(g => g.GrnId == id);

        if (grn == null)
            return NotFound(new { message = "Purchase GRN not found." });

        var timeline = await _db.TrnStoreTimelines
            .Where(t => t.Module == "PURCHASE_GRN" && t.ReferenceId == id && t.IsActive == true)
            .OrderByDescending(t => t.CreatedOn)
            .Select(t => new
            {
                t.TimelineId,
                t.EventType,
                t.EventCode,
                t.EventTitle,
                t.EventDescription,
                t.OldStatus,
                t.NewStatus,
                t.Remarks,
                CreatedOn = t.CreatedOn.HasValue ? t.CreatedOn.Value.ToString("dd-MMM-yyyy HH:mm") : ""
            })
            .ToListAsync();

        return Ok(new
        {
            grn.GrnId,
            grn.GrnNo,
            GrnDate = grn.GrnDate.ToString("dd-MMM-yyyy"),
            GrnDateIso = grn.GrnDate.ToString("yyyy-MM-dd"),
            grn.GrnType,
            grn.JobId,
            grn.JobNo,
            grn.RateCalcId,
            grn.PurchaseOrderId,
            grn.PurchaseOrderNo,
            grn.SupplierId,
            grn.SupplierName,
            grn.InvoiceNo,
            InvoiceDate = grn.InvoiceDate?.ToString("dd-MMM-yyyy"),
            InvoiceDateIso = grn.InvoiceDate?.ToString("yyyy-MM-dd"),
            grn.LocationId,
            grn.TotalItems,
            grn.TotalAmount,
            grn.TaxAmount,
            grn.NetAmount,
            grn.Status,
            grn.QualityStatus,
            grn.Remarks,
            CreatedOn = grn.CreatedOn?.ToString("dd-MMM-yyyy HH:mm"),
            Items = grn.TrnPurchaseGrnItems
                .OrderBy(i => i.ItemSequence)
                .Select(i => new
                {
                    i.GrnItemId,
                    i.ItemSequence,
                    i.MaterialCategory,
                    i.MaterialId,
                    i.MaterialCode,
                    i.MaterialName,
                    i.Specification,
                    i.BomQuantity,
                    i.OrderedQuantity,
                    i.ReceivedQuantity,
                    i.RejectedQuantity,
                    i.AcceptedQuantity,
                    i.Uom,
                    i.Rate,
                    i.Amount,
                    i.TaxRate,
                    i.TaxAmount,
                    i.NetAmount,
                    i.BatchNo,
                    i.AvailableStock,
                    i.ForPart,
                    i.QualityStatus,
                    i.Remarks,
                    i.IsSelected
                }),
            Timeline = timeline
        });
    }

    // ── Update GRN Status ──
    [HttpPost("grns/updatestatus")]
    public async Task<IActionResult> UpdateGrnStatus([FromBody] StoreStatusRequest request)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized(new { message = "Session expired. Please login again." });

        var grn = await _db.TrnPurchaseGrns
            .Include(g => g.TrnPurchaseGrnItems)
            .FirstOrDefaultAsync(g => g.GrnId == request.Id);
        if (grn == null)
            return NotFound(new { message = "Purchase GRN not found." });

        var oldStatus = grn.Status;
        grn.Status = request.Status;
        grn.ModifiedBy = user.UserId.ToString();
        grn.ModifiedOn = DateTime.Now;

        // If RECEIVED, create stock ledger entries
        if (request.Status == "RECEIVED")
        {
            foreach (var item in grn.TrnPurchaseGrnItems.Where(i => i.IsSelected == true))
            {
                _db.TrnStockLedgers.Add(new TrnStockLedger
                {
                    TransactionDate = grn.GrnDate,
                    TransactionType = "GRN",
                    ReferenceType = "PURCHASE_GRN",
                    ReferenceId = grn.GrnId,
                    ReferenceNo = grn.GrnNo,
                    MaterialCategory = item.MaterialCategory,
                    MaterialId = item.MaterialId,
                    MaterialCode = item.MaterialCode,
                    MaterialName = item.MaterialName,
                    Uom = item.Uom,
                    QuantityIn = item.ReceivedQuantity,
                    Rate = item.Rate,
                    Amount = item.Amount,
                    JobId = grn.JobId,
                    JobNo = grn.JobNo,
                    CompanyId = grn.CompanyId,
                    CreatedBy = user.UserId,
                    CreatedOn = DateTime.Now
                });
            }
        }

        await _db.SaveChangesAsync();

        await AddTimelineEntryAsync("PURCHASE_GRN", grn.GrnId, "STATUS_CHANGED", request.Status,
            $"Status Changed to {request.Status}",
            $"Status changed from {oldStatus} to {request.Status} by {user.Name}.",
            oldStatus: oldStatus, newStatus: request.Status, userId: user.UserId);

        var statusActivity = ActivityLogEntry.FromUser(user, "PURCHASE", "STATUS_CHANGE", $"GRN {grn.GrnNo} status changed to {request.Status}");
        statusActivity.EntityType = "PURCHASE_GRN";
        statusActivity.EntityId = grn.GrnId;
        statusActivity.EntityCode = grn.GrnNo;
        statusActivity.Description = $"Status changed from {oldStatus} to {request.Status} by {user.Name}.";
        statusActivity.OldValues = JsonSerializer.Serialize(new { Status = oldStatus });
        statusActivity.NewValues = JsonSerializer.Serialize(new { Status = request.Status });
        statusActivity.ChangedFields = ["Status"];
        await _activityService.LogActivityAsync(statusActivity);

        // ── Party Activity Log: Status Change ──
        if (grn.SupplierId.HasValue && grn.SupplierId > 0)
        {
            var purchSt = request.Status == "RECEIVED" ? "Completed" : request.Status == "CANCELLED" ? "Cancelled" : "Pending";
            await PartyPortalController.LogPartyActivityAsync(_db, grn.SupplierId.Value,
                "PURCHASE", $"PURCHASE_GRN_{request.Status}",
                $"GRN {grn.GrnNo} — {request.Status}",
                $"Status changed from {oldStatus} to {request.Status}.",
                "trn_purchase_grn", grn.GrnId, grn.GrnNo,
                grn.GrnDate, purchSt, "Not Required", grn.NetAmount, user.Name);
        }

        return Ok(new { message = $"GRN status updated to {request.Status}." });
    }

    // ── Delete GRN ──
    [HttpDelete("grns/{id:long}")]
    public async Task<IActionResult> DeleteGrn(long id)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized(new { message = "Session expired. Please login again." });

        var grn = await _db.TrnPurchaseGrns
            .Include(g => g.TrnPurchaseGrnItems)
            .FirstOrDefaultAsync(g => g.GrnId == id);

        if (grn == null)
            return NotFound(new { message = "Purchase GRN not found." });

        if (grn.Status != "DRAFT")
            return BadRequest(new { message = "Only DRAFT GRNs can be deleted." });

        var grnNo = grn.GrnNo;
        _db.TrnPurchaseGrnItems.RemoveRange(grn.TrnPurchaseGrnItems);
        _db.TrnPurchaseGrns.Remove(grn);
        await _db.SaveChangesAsync();

        var deleteActivity = ActivityLogEntry.FromUser(user, "PURCHASE", "DELETE", $"Deleted Purchase GRN {grnNo}");
        deleteActivity.EntityType = "PURCHASE_GRN";
        deleteActivity.EntityId = id;
        deleteActivity.EntityCode = grnNo;
        deleteActivity.Severity = "WARNING";
        await _activityService.LogActivityAsync(deleteActivity);

        return Ok(new { message = "Purchase GRN deleted successfully." });
    }

    // ── Helpers ──

    private async Task AddTimelineEntryAsync(
        string module, long referenceId, string eventType, string? eventCode,
        string eventTitle, string? eventDescription,
        string? oldStatus = null, string? newStatus = null,
        string? remarks = null, long userId = 0)
    {
        try
        {
            _db.TrnStoreTimelines.Add(new TrnStoreTimeline
            {
                Module = module,
                ReferenceId = referenceId,
                EventType = eventType,
                EventCode = eventCode,
                EventTitle = eventTitle,
                EventDescription = eventDescription,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                Remarks = remarks,
                CreatedBy = userId,
                CreatedOn = DateTime.Now,
                IsActive = true
            });
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add timeline entry for {Module} {ReferenceId}", module, referenceId);
        }
    }
}

// ── Request Models ──

public class PurchaseGrnSaveRequest
{
    public string? GrnType { get; set; }
    public long? JobId { get; set; }
    public string? JobNo { get; set; }
    public long? RateCalcId { get; set; }
    public long? PurchaseOrderId { get; set; }
    public string? PurchaseOrderNo { get; set; }
    public int? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public string? InvoiceNo { get; set; }
    public string? InvoiceDate { get; set; }
    public int? LocationId { get; set; }
    public string? Remarks { get; set; }
    public List<PurchaseGrnItemRequest>? Items { get; set; }
}

public class PurchaseGrnItemRequest
{
    public string MaterialCategory { get; set; } = string.Empty;
    public long? MaterialId { get; set; }
    public string? MaterialCode { get; set; }
    public string MaterialName { get; set; } = string.Empty;
    public string? Specification { get; set; }
    public decimal? BomQuantity { get; set; }
    public decimal? OrderedQuantity { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public decimal? RejectedQuantity { get; set; }
    public decimal? AcceptedQuantity { get; set; }
    public string? Uom { get; set; }
    public decimal? Rate { get; set; }
    public decimal? TaxRate { get; set; }
    public string? BatchNo { get; set; }
    public decimal? AvailableStock { get; set; }
    public string? ForPart { get; set; }
    public string? Remarks { get; set; }
    public bool? IsSelected { get; set; }
}
