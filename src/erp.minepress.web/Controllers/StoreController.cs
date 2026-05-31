using erp.minepress.domain.Enums;
using erp.minepress.infrastructure.ErrorLogging;
using erp.minepress.notification.Interfaces;
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
public class StoreController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IUserActivityService _activityService;
    private readonly INotificationService _notifier;
    private readonly IDocumentNumberService _documentNumberService;
    private readonly ILogger<StoreController> _logger;
    private readonly ISystemErrorLogger _systemErrorLogger;

    public StoreController(
        ApplicationDbContext db,
        IUserActivityService activityService,
        INotificationService notifier,
        IDocumentNumberService documentNumberService,
        ILogger<StoreController> logger,
        ISystemErrorLogger systemErrorLogger)
    {
        _db = db;
        _activityService = activityService;
        _notifier = notifier;
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
        var totalIssues = await _db.TrnStoreIssues.CountAsync();
        var draftIssues = await _db.TrnStoreIssues.CountAsync(i => i.Status == "DRAFT");
        var issuedCount = await _db.TrnStoreIssues.CountAsync(i => i.Status == "ISSUED");
        var totalReceives = await _db.TrnStoreReceives.CountAsync();
        var draftReceives = await _db.TrnStoreReceives.CountAsync(r => r.Status == "DRAFT");
        var receivedCount = await _db.TrnStoreReceives.CountAsync(r => r.Status == "RECEIVED");
        // Compute low-stock count using real-time ledger stock keyed by MaterialCode
        var allActiveItems = await _db.VwMstItems
            .Where(i => i.IsActive == true && i.ReorderLevel != null && i.ReorderLevel > 0)
            .Select(i => new { Code = i.ItemCode, i.ReorderLevel })
            .ToListAsync();

        var allCodes = allActiveItems.Select(i => i.Code).ToList();
        var stockMap = await GetRealTimeStockMapAsync(allCodes);

        var lowStockItems = allActiveItems.Count(i =>
        {
            var key = i.Code?.ToLower() ?? "";
            var stock = string.IsNullOrEmpty(key) ? 0m : stockMap.GetValueOrDefault(key, 0m);
            return stock < i.ReorderLevel!.Value;
        });

        return Ok(new
        {
            totalIssues,
            draftIssues,
            issuedCount,
            totalReceives,
            draftReceives,
            receivedCount,
            lowStockItems
        });
    }

    // ── Issue List ──
    [HttpGet("issues")]
    public async Task<IActionResult> GetIssueList()
    {
        var list = await _db.TrnStoreIssues
            .Include(i => i.TrnStoreIssueItems)
            .OrderByDescending(i => i.IssueId)
            .Select(i => new
            {
                i.IssueId,
                i.IssueNo,
                IssueDate = i.IssueDate.ToString("dd-MMM-yyyy"),
                i.IssueType,
                i.JobNo,
                i.Status,
                ItemCount = i.TrnStoreIssueItems.Count,
                i.TotalAmount,
                i.Remarks,
                CreatedOn = i.CreatedOn.HasValue ? i.CreatedOn.Value.ToString("dd-MMM-yyyy HH:mm") : ""
            })
            .ToListAsync();

        return Ok(list);
    }

    // ── Save Issue ──
    [HttpPost("issues/save")]
    public async Task<IActionResult> SaveIssue([FromBody] StoreIssueSaveRequest request)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized(new { message = "Session expired. Please login again." });

        // Only selected (checked) items will be saved
        var selectedItems = request.Items?.Where(i => i.IsSelected == true).ToList() ?? [];
        if (selectedItems.Count == 0)
            return BadRequest(new { message = "Please select at least one item to issue." });

        // Resolve material_id and material_code for BOM-loaded items (where they may be null)
        var unresolvedNames = selectedItems
            .Where(i => i.MaterialId == null && !string.IsNullOrWhiteSpace(i.MaterialName))
            .Select(i => i.MaterialName!.Trim().ToLower())
            .Distinct()
            .ToList();

        if (unresolvedNames.Count > 0)
        {
            var materialLookup = await _db.VwMstItems
                .Where(m => m.IsActive == true && m.ItemName != null && unresolvedNames.Contains(m.ItemName.ToLower()))
                .Select(m => new { Name = m.ItemName!.ToLower(), m.SourceId, m.ItemCode })
                .ToListAsync();

            var materialDict = materialLookup
                .GroupBy(m => m.Name)
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var item in selectedItems.Where(i => i.MaterialId == null && !string.IsNullOrWhiteSpace(i.MaterialName)))
            {
                if (materialDict.TryGetValue(item.MaterialName!.Trim().ToLower(), out var match))
                {
                    item.MaterialId = match.SourceId;
                    item.MaterialCode = match.ItemCode;
                }
            }
        }

        var issueNo = await _documentNumberService.GenerateNextNumberAsync(DocumentProcessCode.STOCK_OUT);

        // ── Validate & enrich material master data from vw_mst_items (server-side authority) ──
        List<(string?, string?)> issueHints = selectedItems.ConvertAll(i => ((string?)i.MaterialCategory, (string?)i.MaterialCode));
        var (masterMap, invalidKeys) = await ValidateAndEnrichMaterialsAsync(issueHints);
        if (invalidKeys.Count > 0)
            return BadRequest(new { message = $"Unrecognised material(s): {string.Join(", ", invalidKeys)}. Please re-select from the item list." });

        // Override MaterialCode, MaterialCategory, and Uom from the master view — never trust client values
        foreach (var item in selectedItems)
        {
            var key = (item.MaterialCategory?.Trim().ToUpperInvariant() ?? "", item.MaterialCode?.Trim() ?? "");
            if (masterMap.TryGetValue(key, out var master))
            {
                item.MaterialId       = master.MaterialId;
                item.MaterialCode     = master.MaterialCode;
                item.MaterialCategory = master.MaterialCategory;
                item.Uom              = master.Uom;
            }
        }

        var issue = new TrnStoreIssue
        {
            IssueNo = issueNo,
            IssueDate = DateOnly.FromDateTime(DateTime.Now),
            IssueType = request.IssueType ?? "JOB",
            JobId = request.JobId,
            JobNo = request.JobNo,
            RateCalcId = request.RateCalcId,
            FromLocationId = user.LocationId,
            ToLocationId = user.LocationId,
            CompanyId = user.CompanyId ?? 1,
            Status = "DRAFT",
            Remarks = request.Remarks,
            CreatedBy = user.UserId,
            CreatedOn = DateTime.Now
        };

        _db.TrnStoreIssues.Add(issue);
        await _db.SaveChangesAsync();

        int seq = 1;
        decimal totalAmount = 0;
        foreach (var item in selectedItems)
        {
            var amount = (item.IssuedQuantity) * (item.Rate ?? 0);
            var issueItem = new TrnStoreIssueItem
            {
                IssueId = issue.IssueId,
                ItemSequence = seq++,
                MaterialCategory = item.MaterialCategory,
                MaterialId = item.MaterialId,
                MaterialCode = item.MaterialCode,
                MaterialName = item.MaterialName,
                Specification = item.Specification,
                BomQuantity = item.BomQuantity,
                IssuedQuantity = item.IssuedQuantity,
                Uom = item.Uom,
                Rate = item.Rate,
                Amount = amount,
                AvailableStock = item.AvailableStock,
                ForPart = item.ForPart,
                Remarks = item.Remarks,
                IsSelected = true,
                CreatedOn = DateTime.Now
            };
            _db.TrnStoreIssueItems.Add(issueItem);
            totalAmount += amount;
        }

        issue.TotalItems = selectedItems.Count;
        issue.TotalAmount = totalAmount;
        await _db.SaveChangesAsync();

        // Timeline
        await AddTimelineEntryAsync("STORE_ISSUE", issue.IssueId, "CREATED", "CREATED",
            "Store Issue Created",
            $"Store Issue {issue.IssueNo} created with {selectedItems.Count} item(s). Type: {issue.IssueType}.",
            newStatus: "DRAFT", userId: user.UserId);

        // Job Timeline (if issue is against a job)
        if (issue.JobId.HasValue && issue.JobId.Value > 0)
        {
            try
            {
                _db.TrnJobTimelines.Add(new TrnJobTimeline
                {
                    JobId = issue.JobId.Value,
                    EventType = "STORE_ISSUE",
                    EventCode = "MATERIAL_ISSUED",
                    EventTitle = "Store Issue Created",
                    EventDescription = $"Store Issue {issue.IssueNo} created with {selectedItems.Count} item(s). Total: ₹{totalAmount:N2}.",
                    NewStatus = issue.Status,
                    Remarks = issue.Remarks,
                    CreatedBy = user.UserId,
                    CreatedOn = DateTime.Now,
                    IsActive = true
                });
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add job timeline entry for Job {JobId} from Store Issue {IssueNo}", issue.JobId, issue.IssueNo);
            }

            // Mark the STORE_ISSUE workspace task as IN_PROGRESS
            try
            {
                var workspaceTask = await _db.TrnWorkspaceTasks
                    .FirstOrDefaultAsync(t => t.JobId == issue.JobId.Value
                                           && t.ProcessCode == "STORE_ISSUE"
                                           && t.TaskStatus != "COMPLETED"
                                           && t.TaskStatus != "CANCELLED");
                if (workspaceTask != null)
                {
                    workspaceTask.TaskStatus  = "IN_PROGRESS";
                    workspaceTask.ModifiedOn  = DateTime.Now;
                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update workspace task for Job {JobId} STORE_ISSUE", issue.JobId);
            }
        }

        // Activity Log
        var activity = ActivityLogEntry.FromUser(user, "STORE", "CREATE", $"Created Store Issue {issue.IssueNo}");
        activity.EntityType = "STORE_ISSUE";
        activity.EntityId = issue.IssueId;
        activity.EntityCode = issue.IssueNo;
        activity.Description = $"Store Issue {issue.IssueNo} created with {selectedItems.Count} item(s). Job: {issue.JobNo ?? "N/A"}.";
        activity.NewValues = JsonSerializer.Serialize(new { issue.IssueNo, issue.IssueType, issue.JobNo, issue.Status, ItemCount = selectedItems.Count });
        await _activityService.LogActivityAsync(activity);

        // In-App Notification
        await _activityService.LogNotificationAsync(new UserNotificationEntry
        {
            UserId = user.UserId,
            Title = "Store Issue Created",
            Message = $"Store Issue {issue.IssueNo} has been created successfully.",
            Icon = "bi bi-box-arrow-up",
            Color = "primary",
            Module = "STORE",
            EventType = "CREATED",
            ReferenceId = (int)issue.IssueId,
            ReferenceUrl = $"/Store/Issue/Details?id={issue.IssueId}",
            Priority = "NORMAL"
        });

        return Ok(new { issue.IssueId, issue.IssueNo, message = "Store Issue saved successfully." });
    }

    // ── Issue Detail ──
    [HttpGet("issues/{id:long}")]
    public async Task<IActionResult> GetIssueDetail(long id)
    {
        var issue = await _db.TrnStoreIssues
            .Include(i => i.TrnStoreIssueItems)
            .FirstOrDefaultAsync(i => i.IssueId == id);

        if (issue == null)
            return NotFound(new { message = "Store Issue not found." });

        var timeline = await _db.TrnStoreTimelines
            .Where(t => t.Module == "STORE_ISSUE" && t.ReferenceId == id && t.IsActive == true)
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
            issue.IssueId,
            issue.IssueNo,
            IssueDate = issue.IssueDate.ToString("dd-MMM-yyyy"),
            IssueDateIso = issue.IssueDate.ToString("yyyy-MM-dd"),
            issue.IssueType,
            issue.JobId,
            issue.JobNo,
            issue.RateCalcId,
            issue.FromLocationId,
            issue.ToLocationId,
            issue.TotalItems,
            issue.TotalAmount,
            issue.Status,
            issue.Remarks,
            CreatedOn = issue.CreatedOn?.ToString("dd-MMM-yyyy HH:mm"),
            Items = issue.TrnStoreIssueItems
                .OrderBy(i => i.ItemSequence)
                .Select(i => new
                {
                    i.IssueItemId,
                    i.ItemSequence,
                    i.MaterialCategory,
                    i.MaterialId,
                    i.MaterialCode,
                    i.MaterialName,
                    i.Specification,
                    i.BomQuantity,
                    i.IssuedQuantity,
                    i.Uom,
                    i.Rate,
                    i.Amount,
                    i.AvailableStock,
                    i.ForPart,
                    i.Remarks,
                    i.IsSelected
                }),
            Timeline = timeline
        });
    }

    // ── Update Issue Status ──
    [HttpPost("issues/updatestatus")]
    public async Task<IActionResult> UpdateIssueStatus([FromBody] StoreStatusRequest request)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized(new { message = "Session expired. Please login again." });

        var issue = await _db.TrnStoreIssues
            .Include(i => i.TrnStoreIssueItems)
            .FirstOrDefaultAsync(i => i.IssueId == request.Id);
        if (issue == null)
            return NotFound(new { message = "Store Issue not found." });

        var oldStatus = issue.Status;
        issue.Status = request.Status;
        issue.ModifiedBy = user.UserId.ToString();
        issue.ModifiedOn = DateTime.Now;

        // If ISSUED, create stock ledger entries with correct running BalanceQuantity
        if (request.Status == "ISSUED")
        {
            var selectedItems = issue.TrnStoreIssueItems.Where(i => i.IsSelected == true).ToList();
            var materialCodes = selectedItems.Select(i => i.MaterialCode).Distinct().ToList();

            // Fetch current running balances before we write new rows
            var balanceMap = await GetRealTimeStockMapAsync(materialCodes);

            foreach (var item in selectedItems)
            {
                var matCode = item.MaterialCode?.ToLower() ?? "";
                var prevBalance = string.IsNullOrEmpty(matCode) ? 0m : balanceMap.GetValueOrDefault(matCode, 0m);
                var newBalance = prevBalance - item.IssuedQuantity;

                // Update in-memory map so subsequent items in the same issue are consistent
                if (!string.IsNullOrEmpty(matCode)) balanceMap[matCode] = newBalance;

                _db.TrnStockLedgers.Add(new TrnStockLedger
                {
                    TransactionDate = issue.IssueDate,
                    TransactionType = "ISSUE",
                    ReferenceType = "STORE_ISSUE",
                    ReferenceId = issue.IssueId,
                    ReferenceNo = issue.IssueNo,
                    MaterialCategory = item.MaterialCategory,
                    MaterialId = item.MaterialId,
                    MaterialCode = item.MaterialCode,
                    MaterialName = item.MaterialName,
                    Uom = item.Uom,
                    QuantityOut = item.IssuedQuantity,
                    BalanceQuantity = newBalance,
                    Rate = item.Rate,
                    Amount = item.Amount,
                    JobId = issue.JobId,
                    JobNo = issue.JobNo,
                    CompanyId = issue.CompanyId,
                    CreatedBy = user.UserId,
                    CreatedOn = DateTime.Now
                });
            }
        }

        await _db.SaveChangesAsync();

        // Gate pass auto-creation on ISSUED
        if (request.Status == "ISSUED")
        {
            var gpItems = issue.TrnStoreIssueItems
                .Where(i => i.IsSelected == true)
                .Select(i => ($"{i.MaterialName} ({i.MaterialCode})", i.IssuedQuantity, i.Uom))
                .ToList();

            var gatePass = await CreateGatePassAsync(
                "OUT", "STORE_ISSUE", issue.IssueNo!, issue.IssueDate,
                issue.CompanyId, $"Store Issue {issue.IssueNo} — {issue.IssueType}",
                gpItems, user.UserId);

            // Timeline for gate pass
            await AddTimelineEntryAsync("STORE_ISSUE", issue.IssueId, "GATE_PASS", "GATE_PASS_OUT",
                $"Gate Pass Issued: {gatePass.GatePassNo}",
                $"Outward Gate Pass {gatePass.GatePassNo} auto-created for Store Issue {issue.IssueNo} with {gpItems.Count} item(s).",
                newStatus: "ISSUED", userId: user.UserId);

            // Activity log for gate pass
            var gpActivity = ActivityLogEntry.FromUser(user, "STORE", "GATE_PASS", $"Gate Pass {gatePass.GatePassNo} issued for {issue.IssueNo}");
            gpActivity.EntityType = "GATE_PASS";
            gpActivity.EntityId = gatePass.GatePassId;
            gpActivity.EntityCode = gatePass.GatePassNo;
            gpActivity.Description = $"Outward Gate Pass {gatePass.GatePassNo} auto-created for Store Issue {issue.IssueNo}.";
            gpActivity.NewValues = JsonSerializer.Serialize(new { gatePass.GatePassNo, gatePass.GatepassType, ReferenceNo = issue.IssueNo, ItemCount = gpItems.Count });
            await _activityService.LogActivityAsync(gpActivity);

            // In-app notification for gate pass
            await _activityService.LogNotificationAsync(new UserNotificationEntry
            {
                UserId = user.UserId,
                Title = $"Gate Pass Issued: {gatePass.GatePassNo}",
                Message = $"Outward Gate Pass {gatePass.GatePassNo} created for Store Issue {issue.IssueNo}.",
                Icon = "bi bi-door-open",
                Color = "warning",
                Module = "STORE",
                EventType = "GATE_PASS",
                ReferenceId = (int)gatePass.GatePassId,
                Priority = "NORMAL"
            });

            // Email notification to store users
            await NotifyStoreUsersAsync(gatePass.GatePassNo, "OUT", issue.IssueNo!, user.Name);
        }

        await AddTimelineEntryAsync("STORE_ISSUE", issue.IssueId, "STATUS_CHANGED", request.Status,
            $"Status Changed to {request.Status}",
            $"Status changed from {oldStatus} to {request.Status} by {user.Name}.",
            oldStatus: oldStatus, newStatus: request.Status, userId: user.UserId);

        var statusActivity = ActivityLogEntry.FromUser(user, "STORE", "STATUS_CHANGE", $"Store Issue {issue.IssueNo} status changed to {request.Status}");
        statusActivity.EntityType = "STORE_ISSUE";
        statusActivity.EntityId = issue.IssueId;
        statusActivity.EntityCode = issue.IssueNo;
        statusActivity.Description = $"Status changed from {oldStatus} to {request.Status} by {user.Name}.";
        statusActivity.OldValues = JsonSerializer.Serialize(new { Status = oldStatus });
        statusActivity.NewValues = JsonSerializer.Serialize(new { Status = request.Status });
        statusActivity.ChangedFields = ["Status"];
        await _activityService.LogActivityAsync(statusActivity);

        return Ok(new { message = $"Store Issue status updated to {request.Status}." });
    }

    // ── Delete Issue ──
    [HttpDelete("issues/{id:long}")]
    public async Task<IActionResult> DeleteIssue(long id)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized(new { message = "Session expired. Please login again." });

        var issue = await _db.TrnStoreIssues
            .Include(i => i.TrnStoreIssueItems)
            .FirstOrDefaultAsync(i => i.IssueId == id);

        if (issue == null)
            return NotFound(new { message = "Store Issue not found." });

        if (issue.Status != "DRAFT")
            return BadRequest(new { message = "Only DRAFT issues can be deleted." });

        var issueNo = issue.IssueNo;
        _db.TrnStoreIssueItems.RemoveRange(issue.TrnStoreIssueItems);
        _db.TrnStoreIssues.Remove(issue);
        await _db.SaveChangesAsync();

        var deleteActivity = ActivityLogEntry.FromUser(user, "STORE", "DELETE", $"Deleted Store Issue {issueNo}");
        deleteActivity.EntityType = "STORE_ISSUE";
        deleteActivity.EntityId = id;
        deleteActivity.EntityCode = issueNo;
        deleteActivity.Description = $"Store Issue {issueNo} (DRAFT) was deleted by {user.Name}.";
        deleteActivity.Severity = "WARNING";
        await _activityService.LogActivityAsync(deleteActivity);

        return Ok(new { message = "Store Issue deleted successfully." });
    }

    // ── Receive List ──
    [HttpGet("receives")]
    public async Task<IActionResult> GetReceiveList()
    {
        var list = await _db.TrnStoreReceives
            .Include(r => r.TrnStoreReceiveItems)
            .OrderByDescending(r => r.ReceiveId)
            .Select(r => new
            {
                r.ReceiveId,
                r.ReceiveNo,
                ReceiveDate = r.ReceiveDate.ToString("dd-MMM-yyyy"),
                r.ReceiveType,
                r.GrnNo,
                r.SupplierName,
                r.Status,
                ItemCount = r.TrnStoreReceiveItems.Count,
                r.TotalAmount,
                r.Remarks,
                CreatedOn = r.CreatedOn.HasValue ? r.CreatedOn.Value.ToString("dd-MMM-yyyy HH:mm") : ""
            })
            .ToListAsync();

        return Ok(list);
    }

    // ── Save Receive ──
    [HttpPost("receives/save")]
    public async Task<IActionResult> SaveReceive([FromBody] StoreReceiveSaveRequest request)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized(new { message = "Session expired. Please login again." });

        var receiveNo = await _documentNumberService.GenerateNextNumberAsync(DocumentProcessCode.STORE_RECEIVE);

        // ── Validate & enrich material master data from vw_mst_items (server-side authority) ──
        if (request.Items?.Any() == true)
        {
            List<(string?, string?)> receiveHints = request.Items.ConvertAll(i => ((string?)i.MaterialCategory, (string?)i.MaterialCode));
            var (masterMap, invalidKeys) = await ValidateAndEnrichMaterialsAsync(receiveHints);
            if (invalidKeys.Count > 0)
                return BadRequest(new { message = $"Unrecognised material(s): {string.Join(", ", invalidKeys)}. Please re-select from the item list." });

            // Override MaterialId, MaterialCode, MaterialCategory, and Uom from the master view
            foreach (var item in request.Items)
            {
                var key = (item.MaterialCategory?.Trim().ToUpperInvariant() ?? "", item.MaterialCode?.Trim() ?? "");
                if (masterMap.TryGetValue(key, out var master))
                {
                    item.MaterialId       = master.MaterialId;
                    item.MaterialCode     = master.MaterialCode;
                    item.MaterialCategory = master.MaterialCategory;
                    item.Uom              = master.Uom;
                }
            }
        }

        var receive = new TrnStoreReceive
        {
            ReceiveNo = receiveNo,
            ReceiveDate = DateOnly.FromDateTime(DateTime.Now),
            ReceiveType = request.ReceiveType ?? "PURCHASE",
            GrnId = request.GrnId,
            GrnNo = request.GrnNo,
            JobId = request.JobId,
            JobNo = request.JobNo,
            SupplierId = request.SupplierId,
            SupplierName = request.SupplierName,
            LocationId = request.LocationId,
            CompanyId = user.CompanyId ?? 1,
            Status = "DRAFT",
            Remarks = request.Remarks,
            CreatedBy = user.UserId,
            CreatedOn = DateTime.Now
        };

        _db.TrnStoreReceives.Add(receive);
        await _db.SaveChangesAsync();

        if (request.Items?.Any() == true)
        {
            int seq = 1;
            decimal totalAmount = 0;
            foreach (var item in request.Items)
            {
                var amount = (item.ReceivedQuantity) * (item.Rate ?? 0);
                var receiveItem = new TrnStoreReceiveItem
                {
                    ReceiveId = receive.ReceiveId,
                    ItemSequence = seq++,
                    MaterialCategory = item.MaterialCategory,
                    MaterialId = item.MaterialId,
                    MaterialCode = item.MaterialCode,
                    MaterialName = item.MaterialName,
                    Specification = item.Specification,
                    OrderedQuantity = item.OrderedQuantity,
                    ReceivedQuantity = item.ReceivedQuantity,
                    RejectedQuantity = item.RejectedQuantity,
                    AcceptedQuantity = item.AcceptedQuantity,
                    Uom = item.Uom,
                    Rate = item.Rate,
                    Amount = amount,
                    BatchNo = item.BatchNo,
                    ForPart = item.ForPart,
                    Remarks = item.Remarks,
                    IsSelected = item.IsSelected ?? true,
                    CreatedOn = DateTime.Now
                };
                _db.TrnStoreReceiveItems.Add(receiveItem);
                totalAmount += amount;
            }

            receive.TotalItems = request.Items.Count;
            receive.TotalAmount = totalAmount;
            await _db.SaveChangesAsync();
        }

        await AddTimelineEntryAsync("STORE_RECEIVE", receive.ReceiveId, "CREATED", "CREATED",
            "Store Receive Created",
            $"Store Receive {receive.ReceiveNo} created with {request.Items?.Count ?? 0} item(s). Type: {receive.ReceiveType}.",
            newStatus: "DRAFT", userId: user.UserId);

        var activity = ActivityLogEntry.FromUser(user, "STORE", "CREATE", $"Created Store Receive {receive.ReceiveNo}");
        activity.EntityType = "STORE_RECEIVE";
        activity.EntityId = receive.ReceiveId;
        activity.EntityCode = receive.ReceiveNo;
        activity.Description = $"Store Receive {receive.ReceiveNo} created with {request.Items?.Count ?? 0} item(s). Supplier: {receive.SupplierName ?? "N/A"}.";
        activity.NewValues = JsonSerializer.Serialize(new { receive.ReceiveNo, receive.ReceiveType, receive.SupplierName, receive.Status });
        await _activityService.LogActivityAsync(activity);

        await _activityService.LogNotificationAsync(new UserNotificationEntry
        {
            UserId = user.UserId,
            Title = "Store Receive Created",
            Message = $"Store Receive {receive.ReceiveNo} has been created successfully.",
            Icon = "bi bi-box-arrow-in-down",
            Color = "success",
            Module = "STORE",
            EventType = "CREATED",
            ReferenceId = (int)receive.ReceiveId,
            ReferenceUrl = $"/Store/Receive/Details?id={receive.ReceiveId}",
            Priority = "NORMAL"
        });

        return Ok(new { receive.ReceiveId, receive.ReceiveNo, message = "Store Receive saved successfully." });
    }

    // ── Receive Detail ──
    [HttpGet("receives/{id:long}")]
    public async Task<IActionResult> GetReceiveDetail(long id)
    {
        var receive = await _db.TrnStoreReceives
            .Include(r => r.TrnStoreReceiveItems)
            .FirstOrDefaultAsync(r => r.ReceiveId == id);

        if (receive == null)
            return NotFound(new { message = "Store Receive not found." });

        var timeline = await _db.TrnStoreTimelines
            .Where(t => t.Module == "STORE_RECEIVE" && t.ReferenceId == id && t.IsActive == true)
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
            receive.ReceiveId,
            receive.ReceiveNo,
            ReceiveDate = receive.ReceiveDate.ToString("dd-MMM-yyyy"),
            ReceiveDateIso = receive.ReceiveDate.ToString("yyyy-MM-dd"),
            receive.ReceiveType,
            receive.GrnId,
            receive.GrnNo,
            receive.JobId,
            receive.JobNo,
            receive.SupplierId,
            receive.SupplierName,
            receive.LocationId,
            receive.TotalItems,
            receive.TotalAmount,
            receive.Status,
            receive.Remarks,
            CreatedOn = receive.CreatedOn?.ToString("dd-MMM-yyyy HH:mm"),
            Items = receive.TrnStoreReceiveItems
                .OrderBy(i => i.ItemSequence)
                .Select(i => new
                {
                    i.ReceiveItemId,
                    i.ItemSequence,
                    i.MaterialCategory,
                    i.MaterialId,
                    i.MaterialCode,
                    i.MaterialName,
                    i.Specification,
                    i.OrderedQuantity,
                    i.ReceivedQuantity,
                    i.RejectedQuantity,
                    i.AcceptedQuantity,
                    i.Uom,
                    i.Rate,
                    i.Amount,
                    i.BatchNo,
                    i.ForPart,
                    i.Remarks,
                    i.IsSelected
                }),
            Timeline = timeline
        });
    }

    // ── Update Receive Status ──
    [HttpPost("receives/updatestatus")]
    public async Task<IActionResult> UpdateReceiveStatus([FromBody] StoreStatusRequest request)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized(new { message = "Session expired. Please login again." });

        var receive = await _db.TrnStoreReceives
            .Include(r => r.TrnStoreReceiveItems)
            .FirstOrDefaultAsync(r => r.ReceiveId == request.Id);
        if (receive == null)
            return NotFound(new { message = "Store Receive not found." });

        var oldStatus = receive.Status;
        receive.Status = request.Status;
        receive.ModifiedBy = user.UserId.ToString();
        receive.ModifiedOn = DateTime.Now;

        // If RECEIVED, create stock ledger entries with correct running BalanceQuantity
        if (request.Status == "RECEIVED")
        {
            var selectedItems = receive.TrnStoreReceiveItems.Where(i => i.IsSelected == true).ToList();
            var materialCodes = selectedItems.Select(i => i.MaterialCode).Distinct().ToList();

            // Fetch current running balances before we write new rows
            var balanceMap = await GetRealTimeStockMapAsync(materialCodes);

            foreach (var item in selectedItems)
            {
                var matCode = item.MaterialCode?.ToLower() ?? "";
                var prevBalance = string.IsNullOrEmpty(matCode) ? 0m : balanceMap.GetValueOrDefault(matCode, 0m);
                var newBalance = prevBalance + item.ReceivedQuantity;

                // Update in-memory map so subsequent items in the same receive are consistent
                if (!string.IsNullOrEmpty(matCode)) balanceMap[matCode] = newBalance;

                _db.TrnStockLedgers.Add(new TrnStockLedger
                {
                    TransactionDate = receive.ReceiveDate,
                    TransactionType = "RECEIVE",
                    ReferenceType = "STORE_RECEIVE",
                    ReferenceId = receive.ReceiveId,
                    ReferenceNo = receive.ReceiveNo,
                    MaterialCategory = item.MaterialCategory,
                    MaterialId = item.MaterialId,
                    MaterialCode = item.MaterialCode,
                    MaterialName = item.MaterialName,
                    Uom = item.Uom,
                    QuantityIn = item.ReceivedQuantity,
                    BalanceQuantity = newBalance,
                    Rate = item.Rate,
                    Amount = item.Amount,
                    JobId = receive.JobId,
                    JobNo = receive.JobNo,
                    CompanyId = receive.CompanyId,
                    CreatedBy = user.UserId,
                    CreatedOn = DateTime.Now
                });
            }
        }

        await _db.SaveChangesAsync();

        // Gate pass auto-creation on RECEIVED
        if (request.Status == "RECEIVED")
        {
            var gpItems = receive.TrnStoreReceiveItems
                .Where(i => i.IsSelected == true)
                .Select(i => ($"{i.MaterialName} ({i.MaterialCode})", i.ReceivedQuantity, i.Uom))
                .ToList();

            var gatePass = await CreateGatePassAsync(
                "IN", "STORE_RECEIVE", receive.ReceiveNo!, receive.ReceiveDate,
                receive.CompanyId, $"Store Receive {receive.ReceiveNo} — {receive.ReceiveType}",
                gpItems, user.UserId);

            // Timeline for gate pass
            await AddTimelineEntryAsync("STORE_RECEIVE", receive.ReceiveId, "GATE_PASS", "GATE_PASS_IN",
                $"Gate Pass Issued: {gatePass.GatePassNo}",
                $"Inward Gate Pass {gatePass.GatePassNo} auto-created for Store Receive {receive.ReceiveNo} with {gpItems.Count} item(s).",
                newStatus: "RECEIVED", userId: user.UserId);

            // Activity log for gate pass
            var gpActivity = ActivityLogEntry.FromUser(user, "STORE", "GATE_PASS", $"Gate Pass {gatePass.GatePassNo} issued for {receive.ReceiveNo}");
            gpActivity.EntityType = "GATE_PASS";
            gpActivity.EntityId = gatePass.GatePassId;
            gpActivity.EntityCode = gatePass.GatePassNo;
            gpActivity.Description = $"Inward Gate Pass {gatePass.GatePassNo} auto-created for Store Receive {receive.ReceiveNo}.";
            gpActivity.NewValues = JsonSerializer.Serialize(new { gatePass.GatePassNo, gatePass.GatepassType, ReferenceNo = receive.ReceiveNo, ItemCount = gpItems.Count });
            await _activityService.LogActivityAsync(gpActivity);

            // In-app notification for gate pass
            await _activityService.LogNotificationAsync(new UserNotificationEntry
            {
                UserId = user.UserId,
                Title = $"Gate Pass Issued: {gatePass.GatePassNo}",
                Message = $"Inward Gate Pass {gatePass.GatePassNo} created for Store Receive {receive.ReceiveNo}.",
                Icon = "bi bi-door-closed",
                Color = "success",
                Module = "STORE",
                EventType = "GATE_PASS",
                ReferenceId = (int)gatePass.GatePassId,
                Priority = "NORMAL"
            });

            // Email notification to store users
            await NotifyStoreUsersAsync(gatePass.GatePassNo, "IN", receive.ReceiveNo!, user.Name);
        }

        await AddTimelineEntryAsync("STORE_RECEIVE", receive.ReceiveId, "STATUS_CHANGED", request.Status,
            $"Status Changed to {request.Status}",
            $"Status changed from {oldStatus} to {request.Status} by {user.Name}.",
            oldStatus: oldStatus, newStatus: request.Status, userId: user.UserId);

        // Activity log for receive status change
        var statusActivity = ActivityLogEntry.FromUser(user, "STORE", "STATUS_CHANGE", $"Store Receive {receive.ReceiveNo} status changed to {request.Status}");
        statusActivity.EntityType = "STORE_RECEIVE";
        statusActivity.EntityId = receive.ReceiveId;
        statusActivity.EntityCode = receive.ReceiveNo;
        statusActivity.Description = $"Status changed from {oldStatus} to {request.Status} by {user.Name}.";
        statusActivity.OldValues = JsonSerializer.Serialize(new { Status = oldStatus });
        statusActivity.NewValues = JsonSerializer.Serialize(new { Status = request.Status });
        statusActivity.ChangedFields = ["Status"];
        await _activityService.LogActivityAsync(statusActivity);

        return Ok(new { message = $"Store Receive status updated to {request.Status}." });
    }

    // ── Delete Receive ──
    [HttpDelete("receives/{id:long}")]
    public async Task<IActionResult> DeleteReceive(long id)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized(new { message = "Session expired. Please login again." });

        var receive = await _db.TrnStoreReceives
            .Include(r => r.TrnStoreReceiveItems)
            .FirstOrDefaultAsync(r => r.ReceiveId == id);

        if (receive == null)
            return NotFound(new { message = "Store Receive not found." });

        if (receive.Status != "DRAFT")
            return BadRequest(new { message = "Only DRAFT receives can be deleted." });

        var receiveNo = receive.ReceiveNo;
        _db.TrnStoreReceiveItems.RemoveRange(receive.TrnStoreReceiveItems);
        _db.TrnStoreReceives.Remove(receive);
        await _db.SaveChangesAsync();

        var deleteActivity = ActivityLogEntry.FromUser(user, "STORE", "DELETE", $"Deleted Store Receive {receiveNo}");
        deleteActivity.EntityType = "STORE_RECEIVE";
        deleteActivity.EntityId = id;
        deleteActivity.EntityCode = receiveNo;
        deleteActivity.Severity = "WARNING";
        await _activityService.LogActivityAsync(deleteActivity);

        return Ok(new { message = "Store Receive deleted successfully." });
    }

    // ── Stock Ledger ──
    [HttpGet("stock-ledger")]
    public async Task<IActionResult> GetStockLedger([FromQuery] string? category, [FromQuery] string? q)
    {
        var query = _db.TrnStockLedgers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(s => s.MaterialCategory == category);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(s =>
                s.MaterialName.ToLower().Contains(term) ||
                (s.MaterialCode != null && s.MaterialCode.ToLower().Contains(term)) ||
                (s.ReferenceNo != null && s.ReferenceNo.ToLower().Contains(term)));
        }

        var list = await query
            .OrderByDescending(s => s.LedgerId)
            .Take(500)
            .Select(s => new
            {
                s.LedgerId,
                TransactionDate = s.TransactionDate.ToString("dd-MMM-yyyy"),
                s.TransactionType,
                s.ReferenceType,
                s.ReferenceNo,
                s.MaterialCategory,
                s.MaterialCode,
                s.MaterialName,
                s.Uom,
                s.QuantityIn,
                s.QuantityOut,
                s.BalanceQuantity,
                s.Rate,
                s.Amount,
                s.JobNo,
                s.Remarks
            })
            .ToListAsync();

        return Ok(list);
    }

    // ── Job Search (for linking issues to jobs) ──
    [HttpGet("jobs/search")]
    public async Task<IActionResult> SearchJobs([FromQuery] string? q, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = _db.TrnJobs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(j =>
                j.JobNo.ToLower().Contains(term) ||
                (j.ProductName != null && j.ProductName.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(j => j.StatusCode == status);
        }

        var totalCount = await query.CountAsync();

        var jobs = await (from j in query
                          join p in _db.MstParties on j.PartyId equals p.Id into pj
                          from party in pj.DefaultIfEmpty()
                          orderby j.JobId descending
                          select new
                          {
                              j.JobId,
                              j.JobNo,
                              j.ProductName,
                              j.Quantity,
                              j.StatusCode,
                              j.RateCalcId,
                              j.Priority,
                              j.ProgressPercent,
                              JobDate = j.JobDate.ToString(),
                              DeliveryDate = j.DeliveryDate != null ? j.DeliveryDate.Value.ToString() : null,
                              CustomerName = party != null ? party.Name : null
                          })
                          .Skip((page - 1) * pageSize)
                          .Take(pageSize)
                          .ToListAsync();

        return Ok(new { items = jobs, totalCount });
    }

    // ── BOM from Job (resolves rate calculator automatically) ──
    [HttpGet("jobs/{jobId:long}/bom")]
    public async Task<IActionResult> GetJobBomData(long jobId)
    {
        // Try via TrnJob.RateCalcId FK first
        var job = await _db.TrnJobs
            .Include(j => j.RateCalc)
            .FirstOrDefaultAsync(j => j.JobId == jobId);

        if (job == null)
            return NotFound(new { message = "Job not found." });

        if (job.RateCalc != null)
            return Ok(new { job.RateCalc.BomData, job.RateCalc.PartsData, job.RateCalc.RateCalcId });

        // Fallback: find the latest rate calculator linked by HybJobRateCalculator.JobId
        var rc = await _db.HybJobRateCalculators
            .Where(r => r.JobId == jobId )
            .OrderByDescending(r => r.RateCalcId)
            .Select(r => new { r.RateCalcId, r.BomData, r.PartsData })
            .FirstOrDefaultAsync();

        if (rc != null)
            return Ok(new { rc.BomData, rc.PartsData, rc.RateCalcId });

        return Ok(new { bomData = (string?)null, partsData = (string?)null, rateCalcId = (long?)null });
    }

    // ── Previously Issued Items for a Job ──
    [HttpGet("jobs/{jobId:long}/issued-items")]
    public async Task<IActionResult> GetJobIssuedItems(long jobId)
    {
        var issues = await _db.TrnStoreIssues
            .Where(i => i.JobId == jobId && i.Status != "CANCELLED")
            .Include(i => i.TrnStoreIssueItems)
            .OrderByDescending(i => i.IssueId)
            .Select(i => new
            {
                i.IssueId,
                i.IssueNo,
                IssueDate = i.IssueDate.ToString("dd-MMM-yyyy"),
                i.Status,
                CreatedByName = _db.MstUsers
                    .Where(u => u.Userid == i.CreatedBy)
                    .Select(u => u.Name)
                    .FirstOrDefault() ?? "System",
                Items = i.TrnStoreIssueItems
                    .Where(it => it.IsSelected == true)
                    .OrderBy(it => it.ItemSequence)
                    .Select(it => new
                    {
                        it.MaterialName,
                        it.MaterialCode,
                        it.IssuedQuantity,
                        it.Uom,
                        it.Rate,
                        it.Amount
                    })
            })
            .ToListAsync();

        return Ok(issues);
    }

    // ── BOM from Rate Calculator (for auto-populating issue items) ──
    [HttpGet("bom/{rateCalcId:long}")]
    public async Task<IActionResult> GetBomData(long rateCalcId)
    {
        var rc = await _db.HybJobRateCalculators
            .Where(r => r.RateCalcId == rateCalcId )
            .Select(r => new { r.BomData, r.PartsData })
            .FirstOrDefaultAsync();

        if (rc == null)
            return NotFound(new { message = "Rate Calculator not found." });

        return Ok(new { rc.BomData, rc.PartsData });
    }

    // ── Supplier Search ──
    [HttpGet("suppliers/search")]
    public async Task<IActionResult> SearchSuppliers([FromQuery] string? q, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = _db.MstParties.Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term) ||
                (p.Code != null && p.Code.ToLower().Contains(term)) ||
                (p.Gstno != null && p.Gstno.ToLower().Contains(term)) ||
                (p.Email != null && p.Email.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync();

        var suppliers = await (from p in query
                               join c in _db.MstCities on p.CityId equals c.Id into cj
                               from city in cj.DefaultIfEmpty()
                               orderby p.Name
                               select new
                               {
                                   p.Id,
                                   p.Name,
                                   p.Code,
                                   p.Address1,
                                   p.Email,
                                   p.Mobile,
                                   p.Gstno,
                                   City = city != null ? city.Name : null
                               })
                               .Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync();

        return Ok(new { items = suppliers, totalCount });
    }

    // ── Central Stock API (single source of truth for all pages) ──
    // Returns real-time current stock computed from TrnStockLedger.BalanceQuantity.
    // All pages (Issue/Create, StockLedger, ItemPicker) should consume this endpoint.
    [HttpGet("stock/current")]
    public async Task<IActionResult> GetCurrentStock(
        [FromQuery] string? group,
        [FromQuery] string? q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = _db.VwMstItems.Where(i => i.IsActive == true).AsQueryable();

        if (!string.IsNullOrWhiteSpace(group))
            query = query.Where(i => i.ItemGroup == group);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(i =>
                (i.ItemName != null && i.ItemName.ToLower().Contains(term)) ||
                (i.ItemCode != null && i.ItemCode.ToLower().Contains(term)) ||
                (i.ItemCategory != null && i.ItemCategory.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync();

        var rawItems = await query
            .OrderBy(i => i.ItemGroup)
            .ThenBy(i => i.ItemName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Real-time stock from ledger keyed by MaterialCode
        var materialCodes = rawItems.Select(i => i.ItemCode).ToList();
        var stockMap = await GetRealTimeStockMapAsync(materialCodes);

        // Available groups for filter dropdown
        var groups = await _db.VwMstItems
            .Where(i => i.IsActive == true && i.ItemGroup != null)
            .Select(i => i.ItemGroup!)
            .Distinct()
            .OrderBy(g => g)
            .ToListAsync();

        var result = rawItems.Select(i =>
        {
            var id = i.SourceId ?? i.ItemId ?? 0L;
            var codeKey = i.ItemCode?.ToLower() ?? "";
            var stock = (!string.IsNullOrEmpty(codeKey) && stockMap.TryGetValue(codeKey, out var s)) ? s : (i.CurrentStock ?? 0m);
            return new
            {
                materialId = id,
                itemCode = i.ItemCode ?? "",
                itemName = i.ItemName ?? "",
                itemGroup = i.ItemGroup ?? "",
                itemCategory = i.ItemCategory ?? "",
                uom = i.Uom ?? "Pcs",
                purchaseRate = i.PurchaseRate ?? 0m,
                reorderLevel = i.ReorderLevel ?? 0m,
                currentStock = stock,
                isInStock = stock > 0,
                isLowStock = stock > 0 && i.ReorderLevel.HasValue && stock <= i.ReorderLevel.Value,
                isOutOfStock = stock <= 0
            };
        }).ToList();

        return Ok(new { items = result, totalCount, page, pageSize, groups });
    }

    // ── Items Picker (paginated, filterable, sortable — for shared item picker modal) ──
    [HttpGet("items/search")]
    public async Task<IActionResult> SearchItems(
        [FromQuery] string? group,
        [FromQuery] string? q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortField = "itemName",
        [FromQuery] string? sortDir = "asc")
    {
        var query = _db.VwMstItems.Where(i => i.IsActive == true).AsQueryable();

        if (!string.IsNullOrWhiteSpace(group))
            query = query.Where(i => i.ItemGroup == group);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(i =>
                (i.ItemName != null && i.ItemName.ToLower().Contains(term)) ||
                (i.ItemCode != null && i.ItemCode.ToLower().Contains(term)) ||
                (i.ItemDescription != null && i.ItemDescription.ToLower().Contains(term)) ||
                (i.HsnCode != null && i.HsnCode.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync();

        // Sorting
        query = (sortField?.ToLower(), sortDir?.ToLower()) switch
        {
            ("itemcode", "desc") => query.OrderByDescending(i => i.ItemCode),
            ("itemcode", _) => query.OrderBy(i => i.ItemCode),
            ("currentstock", "desc") => query.OrderByDescending(i => i.CurrentStock),
            ("currentstock", _) => query.OrderBy(i => i.CurrentStock),
            ("purchaserate", "desc") => query.OrderByDescending(i => i.PurchaseRate),
            ("purchaserate", _) => query.OrderBy(i => i.PurchaseRate),
            ("itemgroup", "desc") => query.OrderByDescending(i => i.ItemGroup),
            ("itemgroup", _) => query.OrderBy(i => i.ItemGroup),
            (_, "desc") => query.OrderByDescending(i => i.ItemName),
            _ => query.OrderBy(i => i.ItemName)
        };

        // Distinct item groups for filter
        var groups = await _db.VwMstItems
            .Where(i => i.IsActive == true && i.ItemGroup != null)
            .Select(i => i.ItemGroup!)
            .Distinct()
            .OrderBy(g => g)
            .ToListAsync();

        var rawItems = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Enrich currentStock from ledger keyed by MaterialCode
        var materialCodes = rawItems.Select(i => i.ItemCode).ToList();
        var stockMap = await GetRealTimeStockMapAsync(materialCodes);

        var items = rawItems.Select(i =>
        {
            var id = i.SourceId ?? i.ItemId ?? 0L;
            var codeKey = i.ItemCode?.ToLower() ?? "";
            var stock = (!string.IsNullOrEmpty(codeKey) && stockMap.TryGetValue(codeKey, out var s)) ? s : (i.CurrentStock ?? 0m);
            return new
            {
                itemId = i.ItemId ?? 0L,
                sourceId = i.SourceId ?? 0L,
                itemGroup = i.ItemGroup ?? "",
                itemCode = i.ItemCode ?? "",
                itemName = i.ItemName ?? "",
                itemDescription = i.ItemDescription ?? "",
                itemCategory = i.ItemCategory ?? "",
                uom = i.Uom ?? "Pcs",
                purchaseRate = i.PurchaseRate ?? 0m,
                currentStock = stock,
                reorderLevel = i.ReorderLevel ?? 0m,
                isInStock = stock > 0,
                isLowStock = stock > 0 && i.ReorderLevel.HasValue && stock <= i.ReorderLevel.Value,
                hsnCode = i.HsnCode ?? "",
                gstRate = i.GstRate ?? 0m,
                lastPurchaseRate = i.LastPurchaseRate ?? 0m,
                lastPurchaseDate = i.LastPurchaseDate.HasValue ? i.LastPurchaseDate.Value.ToString("dd-MMM-yyyy") : ""
            };
        }).ToList();

        return Ok(new { items, totalCount, page, pageSize, groups });
    }

    // ── Materials Search (unified via vw_mst_items view) ──
    [HttpGet("materials/search")]
    public async Task<IActionResult> SearchMaterials([FromQuery] string? category, [FromQuery] string? q)
    {
        var query = _db.VwMstItems.Where(i => i.IsActive == true).AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(i => i.ItemGroup == category);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(i =>
                (i.ItemName != null && i.ItemName.ToLower().Contains(term)) ||
                (i.ItemCode != null && i.ItemCode.ToLower().Contains(term)));
        }

        var rawResults = await query
            .OrderBy(i => i.ItemName)
            .Take(50)
            .ToListAsync();

        // Enrich with real-time stock from TrnStockLedger keyed by MaterialCode
        var materialCodes = rawResults.Select(i => i.ItemCode).ToList();
        var stockMap = await GetRealTimeStockMapAsync(materialCodes);

        var results = rawResults.Select(i =>
        {
            var id = i.SourceId ?? i.ItemId ?? 0L;
            var codeKey = i.ItemCode?.ToLower() ?? "";
            var stock = (!string.IsNullOrEmpty(codeKey) && stockMap.TryGetValue(codeKey, out var s)) ? s : (i.CurrentStock ?? 0m);
            return new
            {
                category = i.ItemGroup ?? "OTHER",
                materialId = id,
                materialCode = i.ItemCode ?? "",
                materialName = i.ItemName ?? "",
                specification = i.ItemDescription ?? i.ItemCategory ?? "",
                uom = i.Uom ?? "Pcs",
                rate = i.PurchaseRate ?? 0m,
                currentStock = stock,
                isInStock = stock > 0,
                isLowStock = stock > 0 && i.ReorderLevel.HasValue && stock <= i.ReorderLevel.Value,
                reorderLevel = i.ReorderLevel ?? 0m,
                hsnCode = i.HsnCode ?? "",
                gstRate = i.GstRate ?? 0m
            };
        }).ToList();

        return Ok(results);
    }

    // ── Timeline API ──
    [HttpGet("timeline/{module}/{referenceId:long}")]
    public async Task<IActionResult> GetTimeline(string module, long referenceId)
    {
        var timeline = await _db.TrnStoreTimelines
            .Where(t => t.Module == module && t.ReferenceId == referenceId && t.IsActive == true)
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

        return Ok(timeline);
    }

    // ── Stock Summary ──

    [HttpGet("stock-summary")]
    public async Task<IActionResult> GetStockSummary([FromQuery] string? group, [FromQuery] string? category)
    {
        // Purchased quantities (non-cancelled receives) grouped by MaterialCode
        var purchaseMap = await _db.TrnStoreReceiveItems
            .Where(ri => ri.Receive.Status != "CANCELLED" && ri.MaterialCode != null)
            .GroupBy(ri => ri.MaterialCode!.ToLower())
            .Select(g => new { Code = g.Key, Total = g.Sum(x => x.ReceivedQuantity) })
            .ToDictionaryAsync(x => x.Code, x => x.Total);

        // Issued quantities (non-cancelled issues, selected items only) grouped by MaterialCode
        var issueMap = await _db.TrnStoreIssueItems
            .Where(ii => ii.Issue.Status != "CANCELLED" && ii.MaterialCode != null && ii.IsSelected == true)
            .GroupBy(ii => ii.MaterialCode!.ToLower())
            .Select(g => new { Code = g.Key, Total = g.Sum(x => x.IssuedQuantity) })
            .ToDictionaryAsync(x => x.Code, x => x.Total);

        var query = _db.VwMstItems.Where(i => i.IsActive == true);

        if (!string.IsNullOrWhiteSpace(group))
            query = query.Where(i => i.ItemGroup == group);
        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(i => i.ItemCategory == category);

        var items = await query.OrderBy(i => i.ItemCategory).ThenBy(i => i.ItemGroup).ThenBy(i => i.ItemName).ToListAsync();

        // Real-time closing stock from ledger keyed by MaterialCode
        var allCodes = items.Select(i => i.ItemCode).ToList();
        var stockMap = await GetRealTimeStockMapAsync(allCodes);

        var result = items.Select(i =>
        {
            var id = i.SourceId ?? i.ItemId ?? 0;
            var codeKey = i.ItemCode?.ToLower() ?? "";
            var purchased = string.IsNullOrEmpty(codeKey) ? 0m : purchaseMap.GetValueOrDefault(codeKey);
            var issued = string.IsNullOrEmpty(codeKey) ? 0m : issueMap.GetValueOrDefault(codeKey);
            var closing = (!string.IsNullOrEmpty(codeKey) && stockMap.TryGetValue(codeKey, out var s)) ? s : (i.CurrentStock ?? 0m);
            var opening = closing - purchased + issued;
            var rate = i.PurchaseRate ?? 0m;

            return new
            {
                materialId = id,
                itemCode = i.ItemCode ?? "",
                itemName = i.ItemName ?? "",
                itemGroup = i.ItemGroup ?? "Ungrouped",
                itemCategory = i.ItemCategory ?? "Uncategorized",
                uom = i.Uom ?? "Pcs",
                rate,
                reorderLevel = i.ReorderLevel ?? 0m,
                opening,
                purchased,
                issued,
                closing,
                purchaseValue = purchased * rate,
                issueValue = issued * rate,
                closingValue = closing * rate,
                isLowStock = closing > 0 && i.ReorderLevel.HasValue && closing <= i.ReorderLevel.Value
            };
        }).ToList();

        return Ok(result);
    }

    [HttpGet("stock-summary/filters")]
    public async Task<IActionResult> GetStockSummaryFilters()
    {
        var groups = await _db.VwMstItems
            .Where(i => i.IsActive == true && i.ItemGroup != null)
            .Select(i => i.ItemGroup!)
            .Distinct()
            .OrderBy(g => g)
            .Select(g => new { groupName = g })
            .ToListAsync();

        var categories = await _db.VwMstItems
            .Where(i => i.IsActive == true && i.ItemCategory != null)
            .Select(i => i.ItemCategory!)
            .Distinct()
            .OrderBy(c => c)
            .Select(c => new { categoryName = c })
            .ToListAsync();

        return Ok(new { groups, categories });
    }

    [HttpGet("stock-summary/purchases/{materialId:long}")]
    public async Task<IActionResult> GetStockPurchases(long materialId)
    {
        var items = await _db.TrnStoreReceiveItems
            .Where(ri => ri.MaterialId == materialId && ri.Receive.Status != "CANCELLED")
            .OrderByDescending(ri => ri.Receive.ReceiveDate)
            .Select(ri => new
            {
                receiveNo = ri.Receive.ReceiveNo,
                receiveDate = ri.Receive.ReceiveDate.ToString("dd-MMM-yyyy"),
                supplierName = ri.Receive.SupplierName ?? "-",
                status = ri.Receive.Status,
                quantity = ri.ReceivedQuantity,
                rate = ri.Rate ?? 0m,
                amount = ri.Amount ?? 0m,
                uom = ri.Uom ?? "Pcs"
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("stock-summary/issues/{materialId:long}")]
    public async Task<IActionResult> GetStockIssues(long materialId)
    {
        var items = await _db.TrnStoreIssueItems
            .Where(ii => ii.MaterialId == materialId && ii.Issue.Status != "CANCELLED" && ii.IsSelected == true)
            .OrderByDescending(ii => ii.Issue.IssueDate)
            .Select(ii => new
            {
                issueNo = ii.Issue.IssueNo,
                issueDate = ii.Issue.IssueDate.ToString("dd-MMM-yyyy"),
                jobNo = ii.Issue.JobNo ?? "-",
                status = ii.Issue.Status,
                quantity = ii.IssuedQuantity,
                rate = ii.Rate ?? 0m,
                amount = ii.Amount ?? 0m,
                uom = ii.Uom ?? "Pcs"
            })
            .ToListAsync();

        return Ok(items);
    }

    // ── Helpers ──

    /// <summary>
    /// Creates a gate pass (IN or OUT) linked to a store issue or receive,
    /// with line items copied from the source document.
    /// </summary>
    private async Task<TrnGatePass> CreateGatePassAsync(
        string gatepassType, string referenceType, string referenceNo, DateOnly referenceDate,
        int companyId, string? purpose, List<(string Description, decimal Quantity, string? Uom)> items,
        long userId)
    {
        var processCode = gatepassType == "IN" ? DocumentProcessCode.GATE_PASS_IN : DocumentProcessCode.GATE_PASS_OUT;
        var gatePassNo = await _documentNumberService.GenerateNextNumberAsync(processCode);

        var gatePass = new TrnGatePass
        {
            GatePassNo = gatePassNo,
            GatePassDate = DateOnly.FromDateTime(DateTime.Now),
            GatepassType = gatepassType,
            CompanyId = companyId,
            ReferenceType = referenceType,
            ReferenceNo = referenceNo,
            ReferenceDate = referenceDate,
            Purpose = purpose,
            TotalQuantity = items.Sum(i => i.Quantity),
            Status = "PENDING",
            CreatedBy = userId,
            CreatedOn = DateTime.Now
        };

        _db.TrnGatePasses.Add(gatePass);
        await _db.SaveChangesAsync();

        int seq = 1;
        foreach (var (desc, qty, uom) in items)
        {
            _db.TrnGatePassItems.Add(new TrnGatePassItem
            {
                GatePassId = gatePass.GatePassId,
                ItemSequence = seq++,
                Description = desc,
                Quantity = qty,
                Status = "PENDING",
                CreatedOn = DateTime.Now
            });
        }
        await _db.SaveChangesAsync();

        return gatePass;
    }

    /// <summary>
    /// Sends email notification to all active store / inventory users about a gate pass.
    /// </summary>
    private async Task NotifyStoreUsersAsync(string gatePassNo, string gatepassType, string referenceNo, string userName)
    {
        try
        {
            // Find store department ids (INV, PUR, ADM)
            var storeDeptCodes = new[] { "INV", "PUR" };
            var storeDeptIds = await _db.MstDepartments
                .AsNoTracking()
                .Where(d => storeDeptCodes.Contains(d.DeptCode))
                .Select(d => d.DeptId)
                .ToListAsync();

            var storeUsers = await _db.MstUsers
                .AsNoTracking()
                .Where(u => u.Isactive == true
                    && u.Emailid != null && u.Emailid != ""
                    && (storeDeptIds.Contains(u.Departmentid) || u.Issystemadmin == true))
                .Select(u => new { u.Emailid, u.Name })
                .ToListAsync();

            var direction = gatepassType == "IN" ? "Inward" : "Outward";
            var subject = $"Gate Pass {direction}: {gatePassNo}";
            var body = $"<h3>Gate Pass {direction} — {gatePassNo}</h3>"
                     + $"<p>A new <b>{direction}</b> gate pass has been issued.</p>"
                     + $"<table style='border-collapse:collapse;'>"
                     + $"<tr><td style='padding:4px 12px;'><b>Gate Pass No:</b></td><td>{gatePassNo}</td></tr>"
                     + $"<tr><td style='padding:4px 12px;'><b>Reference:</b></td><td>{referenceNo}</td></tr>"
                     + $"<tr><td style='padding:4px 12px;'><b>Type:</b></td><td>{direction}</td></tr>"
                     + $"<tr><td style='padding:4px 12px;'><b>Issued By:</b></td><td>{userName}</td></tr>"
                     + $"<tr><td style='padding:4px 12px;'><b>Date:</b></td><td>{DateTime.Now:dd-MMM-yyyy hh:mm tt}</td></tr>"
                     + $"</table>"
                     + $"<p>Please review and approve the gate pass at the earliest.</p>"
                     + $"<hr/><small style='color:#888;'>MinePress ERP — Automated Gate Pass Notification</small>";

            foreach (var u in storeUsers)
            {
                _ = _notifier.SendEmailAsync(u.Emailid!, subject, body);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send gate pass notification emails for {GatePassNo}", gatePassNo);
        }
    }

    // ── Material Validation & Enrichment Helper ──
    // Looks up items by the composite key (ItemGroup, ItemCode) — the only truly unique
    // identifier in vw_mst_items. source_id is NOT reliable: CHEMICAL, INK and OTHER
    // all hard-code source_id = 1, so the same id can appear in multiple source tables.
    // Returns a map keyed by (ItemGroup.ToUpper(), ItemCode) and a list of missing keys.
    private async Task<(Dictionary<(string Group, string Code), MaterialMasterInfo> map, List<string> invalid)>
        ValidateAndEnrichMaterialsAsync(IEnumerable<(string? Group, string? Code)> materialItems)
    {
        var itemList = materialItems
            .Where(x => !string.IsNullOrWhiteSpace(x.Group) && !string.IsNullOrWhiteSpace(x.Code))
            .Select(x => (Group: x.Group!.Trim().ToUpperInvariant(), Code: x.Code!.Trim()))
            .Distinct()
            .ToList();

        if (itemList.Count == 0)
            return ([], []);

        var groups = itemList.Select(x => x.Group).Distinct().ToList();
        var codes  = itemList.Select(x => x.Code).Distinct().ToList();

        var rows = await _db.VwMstItems
            .Where(i => i.IsActive == true
                     && i.ItemGroup != null && groups.Contains(i.ItemGroup)
                     && i.ItemCode  != null && codes.Contains(i.ItemCode))
            .Select(i => new MaterialMasterInfo
            {
                MaterialId       = i.SourceId ?? 0L,
                MaterialCategory = i.ItemGroup ?? "OTHER",
                MaterialCode     = i.ItemCode  ?? "",
                Uom              = i.Uom       ?? "Pcs",
                HsnCode          = i.HsnCode   ?? "",
                GstRate          = i.GstRate   ?? 0m
            })
            .ToListAsync();

        // Build map by (Group, Code) — group to handle any remaining view duplicates
        var map = rows
            .GroupBy(r => (r.MaterialCategory.ToUpperInvariant(), r.MaterialCode))
            .ToDictionary(g => g.Key, g => g.First());

        var invalid = itemList
            .Where(x => !map.ContainsKey((x.Group, x.Code)))
            .Select(x => $"{x.Group}:{x.Code}")
            .ToList();

        return (map, invalid);
    }

    // ── Real-Time Stock Helper ──
    // Computes current stock as SUM(QuantityIn) - SUM(QuantityOut) grouped by MaterialCode.
    // Keyed by lowercase MaterialCode so it works even when MaterialId is NULL on older ledger rows.
    // EF Core translates GroupBy + Sum into a single SQL GROUP BY — no BalanceQuantity dependency.
    private async Task<Dictionary<string, decimal>> GetRealTimeStockMapAsync(IEnumerable<string?> materialCodes)
    {
        var codes = materialCodes
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c!.ToLower())
            .Distinct()
            .ToList();

        if (codes.Count == 0) return [];

        var rows = await _db.TrnStockLedgers
            .Where(s => s.MaterialCode != null && codes.Contains(s.MaterialCode.ToLower()))
            .GroupBy(s => s.MaterialCode!.ToLower())
            .Select(g => new
            {
                MaterialCode = g.Key,
                Stock = g.Sum(x => x.QuantityIn ?? 0m) - g.Sum(x => x.QuantityOut ?? 0m)
            })
            .ToListAsync();

        return rows.ToDictionary(x => x.MaterialCode, x => x.Stock);
    }

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

public class StoreIssueSaveRequest
{
    public string? IssueType { get; set; }
    public long? JobId { get; set; }
    public string? JobNo { get; set; }
    public long? RateCalcId { get; set; }
    public int? FromLocationId { get; set; }
    public int? ToLocationId { get; set; }
    public string? Remarks { get; set; }
    public List<StoreIssueItemRequest>? Items { get; set; }
}

public class StoreIssueItemRequest
{
    public string MaterialCategory { get; set; } = string.Empty;
    public long? MaterialId { get; set; }
    public string? MaterialCode { get; set; }
    public string MaterialName { get; set; } = string.Empty;
    public string? Specification { get; set; }
    public decimal? BomQuantity { get; set; }
    public decimal IssuedQuantity { get; set; }
    public string? Uom { get; set; }
    public decimal? Rate { get; set; }
    public decimal? AvailableStock { get; set; }
    public string? ForPart { get; set; }
    public string? Remarks { get; set; }
    public bool? IsSelected { get; set; }
}

public class StoreReceiveSaveRequest
{
    public string? ReceiveType { get; set; }
    public long? GrnId { get; set; }
    public string? GrnNo { get; set; }
    public long? JobId { get; set; }
    public string? JobNo { get; set; }
    public int? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public int? LocationId { get; set; }
    public string? Remarks { get; set; }
    public List<StoreReceiveItemRequest>? Items { get; set; }
}

public class StoreReceiveItemRequest
{
    public string MaterialCategory { get; set; } = string.Empty;
    public long? MaterialId { get; set; }
    public string? MaterialCode { get; set; }
    public string MaterialName { get; set; } = string.Empty;
    public string? Specification { get; set; }
    public decimal? OrderedQuantity { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public decimal? RejectedQuantity { get; set; }
    public decimal? AcceptedQuantity { get; set; }
    public string? Uom { get; set; }
    public decimal? Rate { get; set; }
    public string? BatchNo { get; set; }
    public string? ForPart { get; set; }
    public string? Remarks { get; set; }
    public bool? IsSelected { get; set; }
}

public class StoreStatusRequest
{
    public long Id { get; set; }
    public string Status { get; set; } = string.Empty;
}

// Authoritative material master data resolved from vw_mst_items
// Composite key: (MaterialCategory, MaterialCode) — SourceId/MaterialId is unreliable
// because CHEMICAL, INK, and OTHER source tables all hard-code source_id = 1.
public sealed class MaterialMasterInfo
{
    public long   MaterialId       { get; init; }   // source_id — store for FK; NOT unique across groups
    public string MaterialCategory { get; init; } = "OTHER";   // item_group (PLATE, PAPER, INK …)
    public string MaterialCode     { get; init; } = "";        // item_code  — unique within a group
    public string Uom              { get; init; } = "Pcs";
    public string HsnCode          { get; init; } = "";
    public decimal GstRate         { get; init; }
}
