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
public class PartyPortalController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IWorkspaceProcessEngine _workspaceEngine;
    private readonly ILogger<PartyPortalController> _logger;
    private readonly ISystemErrorLogger _systemErrorLogger;

    public PartyPortalController(ApplicationDbContext db, IWorkspaceProcessEngine workspaceEngine, ILogger<PartyPortalController> logger, ISystemErrorLogger systemErrorLogger)
    {
        _db = db;
        _workspaceEngine = workspaceEngine;
        _logger = logger;
        _systemErrorLogger = systemErrorLogger;
    }

    private async Task AuditExceptionAsync(Exception ex, string additionalData, string severity = "Error")
        => await _systemErrorLogger.LogAsync(ex, HttpContext, severity, additionalData);

    private UserSessionData? CurrentUser =>
        HttpContext.Session.GetObject<UserSessionData>("CurrentUser");

    // ═══════════════════════════════════════════════════════════════
    // DASHBOARD STATS
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var user = CurrentUser;
        if (user == null || !user.IsPartyUser || !user.RefId.HasValue)
            return Unauthorized();

        var partyId = (int)user.RefId.Value;
        var roles = user.PartyRoles;
        var now = DateTime.Now;
        var weekAgo = now.AddDays(-7);
        var twoWeeksAgo = now.AddDays(-14);

        var result = new Dictionary<string, object>();

        // ── Activity summary (for all roles) ──
        var activityBreakdown = await _db.PartyActivityLogs
            .Where(a => a.PartyId == partyId && a.IsActive == true)
            .GroupBy(a => a.ActivityType)
            .Select(g => new { type = g.Key, count = g.Count() })
            .ToListAsync();

        var recentActivityCount = await _db.PartyActivityLogs
            .CountAsync(a => a.PartyId == partyId && a.IsActive == true && a.CreatedOn >= weekAgo);
        var priorActivityCount = await _db.PartyActivityLogs
            .CountAsync(a => a.PartyId == partyId && a.IsActive == true && a.CreatedOn >= twoWeeksAgo && a.CreatedOn < weekAgo);

        result["activityBreakdown"] = activityBreakdown;
        result["recentActivityCount"] = recentActivityCount;
        result["activityTrend"] = priorActivityCount > 0
            ? Math.Round((recentActivityCount - priorActivityCount) / (decimal)priorActivityCount * 100, 1)
            : recentActivityCount > 0 ? 100m : 0m;

        // ── AI Insights ──
        var insights = new List<object>();

        if (roles.Contains("Customer"))
        {
            var totalEnquiries = await _db.TrnEnquiries.CountAsync(e => e.PartyId == partyId);
            var activeEnquiries = await _db.TrnEnquiries.CountAsync(e => e.PartyId == partyId && e.Status != "CLOSED" && e.Status != "CANCELLED" && e.Status != "CONVERTED");
            var totalQuotations = await _db.TrnQuotations.CountAsync(q => q.PartyId == partyId);
            var pendingQuotations = await _db.TrnQuotations.CountAsync(q => q.PartyId == partyId && q.Status != "APPROVED" && q.Status != "CANCELLED" && q.Status != "CLOSED");
            var totalJobs = await _db.TrnJobs.CountAsync(j => j.PartyId == partyId);
            var activeJobs = await _db.TrnJobs.CountAsync(j => j.PartyId == partyId && j.StatusCode != "DELIVERED" && j.StatusCode != "JOB_CANCELLED" && j.StatusCode != "CLOSED");
            var totalChallans = await _db.TrnChallans.CountAsync(c => c.PartyId == partyId);

            var recentEnq = await _db.TrnEnquiries.CountAsync(e => e.PartyId == partyId && e.CreatedOn >= weekAgo);
            var priorEnq = await _db.TrnEnquiries.CountAsync(e => e.PartyId == partyId && e.CreatedOn >= twoWeeksAgo && e.CreatedOn < weekAgo);

            result["customerEnquiries"] = totalEnquiries;
            result["customerActiveEnquiries"] = activeEnquiries;
            result["customerQuotations"] = totalQuotations;
            result["customerPendingQuotations"] = pendingQuotations;
            result["customerJobs"] = totalJobs;
            result["customerActiveJobs"] = activeJobs;
            result["customerChallans"] = totalChallans;
            result["customerEnqTrend"] = priorEnq > 0
                ? Math.Round((recentEnq - priorEnq) / (decimal)priorEnq * 100, 1)
                : recentEnq > 0 ? 100m : 0m;

            // Customer-specific insights
            if (pendingQuotations > 0)
                insights.Add(new { icon = "bi-file-earmark-ruled", color = "purple", title = "Quotations Awaiting Review", message = $"You have {pendingQuotations} quotation(s) pending your review.", priority = "high" });
            if (activeJobs > 0)
                insights.Add(new { icon = "bi-gear-wide-connected", color = "blue", title = "Jobs In Progress", message = $"{activeJobs} job(s) are currently being processed for you.", priority = "info" });
            if (totalEnquiries > 0 && totalQuotations > 0)
            {
                var convRate = Math.Round((decimal)totalQuotations / totalEnquiries * 100, 0);
                insights.Add(new { icon = "bi-graph-up-arrow", color = "green", title = "Enquiry Conversion", message = $"{convRate}% of your enquiries have been quoted. Your engagement rate is {(convRate > 60 ? "excellent" : convRate > 30 ? "good" : "building up")}.", priority = "info" });
            }
            if (totalChallans > 0)
                insights.Add(new { icon = "bi-truck", color = "teal", title = "Delivery Summary", message = $"{totalChallans} delivery challan(s) processed for your orders.", priority = "info" });
        }

        if (roles.Contains("Supplier"))
        {
            var totalGrns = await _db.TrnPurchaseGrns.CountAsync(g => g.SupplierId == partyId);
            var pendingGrns = await _db.TrnPurchaseGrns.CountAsync(g => g.SupplierId == partyId && g.Status != "CLOSED" && g.Status != "CANCELLED");
            var totalPayments = await _db.TrnPayments.CountAsync(p => p.PartyId == partyId);
            var paymentTotal = await _db.TrnPayments.Where(p => p.PartyId == partyId && p.Status != "CANCELLED").SumAsync(p => (decimal?)p.Amount ?? 0);

            result["supplierGrns"] = totalGrns;
            result["supplierPendingGrns"] = pendingGrns;
            result["supplierPaymentCount"] = totalPayments;
            result["supplierPaymentTotal"] = paymentTotal;

            if (pendingGrns > 0)
                insights.Add(new { icon = "bi-box-seam", color = "orange", title = "Pending Deliveries", message = $"{pendingGrns} purchase receipt(s) are pending completion.", priority = "high" });
            if (paymentTotal > 0)
                insights.Add(new { icon = "bi-currency-rupee", color = "green", title = "Payment Summary", message = $"Total payments processed: ₹{paymentTotal:N0}.", priority = "info" });
        }

        if (roles.Contains("Vendor"))
        {
            var vendorId = await _db.MstVendors
                .Where(v => v.PartyId == partyId && v.IsActive == true)
                .Select(v => (int?)v.VendorId).FirstOrDefaultAsync();

            var totalOutsource = vendorId.HasValue ? await _db.TrnJobOutsources.CountAsync(o => o.VendorId == vendorId.Value) : 0;
            var activeOutsource = vendorId.HasValue ? await _db.TrnJobOutsources.CountAsync(o => o.VendorId == vendorId.Value && o.Status != "OUTSOURCE_CLOSED" && o.Status != "OUTSOURCE_CANCELLED") : 0;
            var completedOutsource = vendorId.HasValue ? await _db.TrnJobOutsources.CountAsync(o => o.VendorId == vendorId.Value && (o.Status == "OUTSOURCE_CLOSED" || o.Status == "MATERIAL_RECEIVED" || o.Status == "PAYMENT_COMPLETED")) : 0;
            var vendorPayments = await _db.TrnPayments.Where(p => p.PartyId == partyId && p.Status != "CANCELLED").SumAsync(p => (decimal?)p.Amount ?? 0);

            result["vendorOutsourceTotal"] = totalOutsource;
            result["vendorActiveOutsource"] = activeOutsource;
            result["vendorCompletedOutsource"] = completedOutsource;
            result["vendorPaymentTotal"] = vendorPayments;

            if (activeOutsource > 0)
                insights.Add(new { icon = "bi-box-arrow-up-right", color = "purple", title = "Active Outsource Orders", message = $"{activeOutsource} outsource order(s) are currently assigned to you.", priority = "high" });
            if (totalOutsource > 0 && completedOutsource > 0)
            {
                var compRate = Math.Round((decimal)completedOutsource / totalOutsource * 100, 0);
                insights.Add(new { icon = "bi-trophy", color = "gold", title = "Completion Rate", message = $"Your completion rate is {compRate}%. {(compRate > 80 ? "Outstanding performance!" : compRate > 50 ? "Good progress." : "Keep it up!")}", priority = "info" });
            }
        }

        // General time-based insight
        var hour = now.Hour;
        if (hour < 12)
            insights.Insert(0, new { icon = "bi-sunrise", color = "amber", title = "Good Morning!", message = "Start your day by reviewing pending items and recent updates.", priority = "greeting" });
        else if (hour < 17)
            insights.Insert(0, new { icon = "bi-sun", color = "amber", title = "Good Afternoon!", message = "Here's an overview of your current activity and pending tasks.", priority = "greeting" });
        else
            insights.Insert(0, new { icon = "bi-moon-stars", color = "indigo", title = "Good Evening!", message = "Wrap up your day with a quick look at today's progress.", priority = "greeting" });

        result["insights"] = insights;

        return Ok(result);
    }

    // ═══════════════════════════════════════════════════════════════
    // PARTY WORKSPACE: TASKS & APPROVALS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Get all pending tasks assigned to the party (quotation reviews, document approvals, etc.)
    /// </summary>
    [HttpGet("workspace/tasks")]
    public async Task<IActionResult> GetPartyWorkspaceTasks([FromQuery] string filter = "pending")
    {
        var user = CurrentUser;
        if (user == null || !user.IsPartyUser || !user.RefId.HasValue)
            return Unauthorized();

        var partyId = (int)user.RefId.Value;

        // Find tasks related to this party (via enquiry, quotation, job, etc.)
        var query = _db.TrnWorkspaceTasks
            .Include(t => t.Process)
            .Include(t => t.Department)
            .Where(t => !t.IsArchived && t.TaskType == "TASK")
            .Where(t =>
                // Match by party-related source tables
                (t.SourceTable == "trn_enquiry" && _db.TrnEnquiries.Any(e => e.EnquiryId == t.SourceId && e.PartyId == partyId)) ||
                (t.SourceTable == "trn_quotation" && _db.TrnQuotations.Any(q => q.QuotationId == t.SourceId && q.PartyId == partyId)) ||
                (t.SourceTable == "trn_job" && _db.TrnJobs.Any(j => j.JobId == t.SourceId && j.PartyId == partyId)) ||
                (t.SourceTable == "trn_challan" && _db.TrnChallans.Any(c => c.ChallanId == t.SourceId && c.PartyId == partyId)) ||
                (t.SourceTable == "trn_sales_invoice" && _db.TrnSalesInvoices.Any(i => i.SalesInvoiceId == t.SourceId && i.PartyId == partyId))
            );

        query = filter.ToLower() switch
        {
            "pending" => query.Where(t => t.TaskStatus == "PENDING" || t.TaskStatus == "IN_PROGRESS"),
            "completed" => query.Where(t => t.TaskStatus == "COMPLETED"),
            "all" => query,
            _ => query.Where(t => t.TaskStatus == "PENDING" || t.TaskStatus == "IN_PROGRESS")
        };

        var tasks = await query
            .OrderByDescending(t => t.Priority == "CRITICAL" ? 5 :
                                    t.Priority == "URGENT" ? 4 :
                                    t.Priority == "HIGH" ? 3 :
                                    t.Priority == "NORMAL" ? 2 : 1)
            .ThenBy(t => t.DueDate)
            .ThenByDescending(t => t.CreatedOn)
            .Take(50)
            .Select(t => new
            {
                t.WorkspaceTaskId,
                t.TaskType,
                t.TaskStatus,
                t.Title,
                t.Description,
                t.ProcessCode,
                ProcessName = t.Process != null ? t.Process.Processname : null,
                DepartmentName = t.Department != null ? t.Department.DeptName : null,
                t.Priority,
                DueDate = t.DueDate.HasValue ? t.DueDate.Value.ToString("dd-MMM-yyyy HH:mm") : null,
                t.IsOverdue,
                t.ActionUrl,
                t.JobId,
                t.JobNo,
                t.SourceTable,
                t.SourceId,
                t.SourceNo,
                CreatedOn = t.CreatedOn.ToString("dd-MMM-yyyy HH:mm"),
                CanTakeAction = t.TaskStatus == "PENDING" || t.TaskStatus == "IN_PROGRESS"
            })
            .ToListAsync();

        return Ok(new { items = tasks, total = tasks.Count });
    }

    /// <summary>
    /// Get all pending approvals that require party action (quotation approval, artwork approval, etc.)
    /// </summary>
    [HttpGet("workspace/approvals")]
    public async Task<IActionResult> GetPartyWorkspaceApprovals([FromQuery] string filter = "pending")
    {
        var user = CurrentUser;
        if (user == null || !user.IsPartyUser || !user.RefId.HasValue)
            return Unauthorized();

        var partyId = (int)user.RefId.Value;

        // Find approval tasks that require party action
        // These are typically quotation approvals, artwork/design approvals, proof approvals
        var partyApprovalProcessCodes = new[] { "QUOT_CUST_APPR", "QUOT_APPROVAL", "ART_CUST_APPR", "PROOF_CUST_APPR", "DESIGN_CUST_APPR", "CUST_APPR" };

        var query = _db.TrnWorkspaceTasks
            .Include(t => t.Process)
            .Include(t => t.Department)
            .Where(t => !t.IsArchived && t.TaskType == "APPROVAL")
            .Where(t =>
                // Match approvals that need party/customer action
                partyApprovalProcessCodes.Contains(t.ProcessCode!) ||
                // Or approvals linked to party's documents
                (t.SourceTable == "trn_enquiry" && _db.TrnEnquiries.Any(e => e.EnquiryId == t.SourceId && e.PartyId == partyId)) ||
                (t.SourceTable == "trn_quotation" && _db.TrnQuotations.Any(q => q.QuotationId == t.SourceId && q.PartyId == partyId)) ||
                (t.SourceTable == "trn_job" && _db.TrnJobs.Any(j => j.JobId == t.SourceId && j.PartyId == partyId))
            );

        query = filter.ToLower() switch
        {
            "pending" => query.Where(t => t.TaskStatus == "PENDING"),
            "approved" => query.Where(t => t.TaskStatus == "APPROVED"),
            "rejected" => query.Where(t => t.TaskStatus == "REJECTED"),
            "all" => query,
            _ => query.Where(t => t.TaskStatus == "PENDING")
        };

        var approvals = await query
            .OrderByDescending(t => t.Priority == "CRITICAL" ? 5 :
                                    t.Priority == "URGENT" ? 4 :
                                    t.Priority == "HIGH" ? 3 :
                                    t.Priority == "NORMAL" ? 2 : 1)
            .ThenBy(t => t.DueDate)
            .ThenByDescending(t => t.CreatedOn)
            .Take(50)
            .Select(t => new
            {
                t.WorkspaceTaskId,
                t.TaskType,
                t.TaskStatus,
                t.Title,
                t.Description,
                t.ProcessCode,
                ProcessName = t.Process != null ? t.Process.Processname : null,
                DepartmentName = t.Department != null ? t.Department.DeptName : null,
                t.Priority,
                DueDate = t.DueDate.HasValue ? t.DueDate.Value.ToString("dd-MMM-yyyy HH:mm") : null,
                t.IsOverdue,
                t.ActionUrl,
                t.JobId,
                t.JobNo,
                t.SourceTable,
                t.SourceId,
                t.SourceNo,
                CreatedOn = t.CreatedOn.ToString("dd-MMM-yyyy HH:mm"),
                CanApprove = t.TaskStatus == "PENDING",
                CanReject = t.TaskStatus == "PENDING"
            })
            .ToListAsync();

        return Ok(new { items = approvals, total = approvals.Count });
    }

    /// <summary>
    /// Party approves a pending approval task (quotation, artwork, etc.)
    /// </summary>
    [HttpPost("workspace/approvals/{id}/approve")]
    public async Task<IActionResult> ApprovePartyApproval(long id, [FromBody] PartyApprovalActionDto? dto = null)
    {
        var user = CurrentUser;
        if (user == null || !user.IsPartyUser || !user.RefId.HasValue)
            return Unauthorized();

        var partyId = (int)user.RefId.Value;
        var task = await _db.TrnWorkspaceTasks.FindAsync(id);

        if (task == null)
            return NotFound(new { message = "Approval not found." });

        if (task.TaskStatus != "PENDING")
            return BadRequest(new { message = "This approval has already been processed." });

        // Verify the task belongs to this party
        var isPartyTask = await VerifyPartyOwnership(task, partyId);
        if (!isPartyTask)
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "You do not have permission to approve this item." });

        // Update task status
        var oldStatus = task.TaskStatus;
        task.TaskStatus = "APPROVED";
        task.CompletedBy = user.UserId;
        task.CompletedOn = DateTime.Now;
        task.CompletionRemarks = $"[Party Approved] {dto?.Remarks ?? "Approved by customer"}";
        task.ModifiedOn = DateTime.Now;

        await _db.SaveChangesAsync();

        // Log party activity
        await LogPartyActivityAsync(
            _db, partyId,
            "APPROVAL", "APPROVED",
            title: $"Approved: {task.Title}",
            description: $"Approval '{task.Title}' has been approved. {dto?.Remarks ?? ""}",
            referenceTable: task.SourceTable,
            referenceId: task.SourceId,
            documentNo: task.SourceNo ?? task.JobNo,
            status: "APPROVED",
            approvalStatus: "APPROVED",
            createdBy: user.Name);

        // Add job timeline if job-linked
        if (task.JobId.HasValue)
        {
            _db.TrnJobTimelines.Add(new TrnJobTimeline
            {
                JobId = task.JobId.Value,
                EventType = "PARTY_APPROVAL",
                EventCode = "PARTY_APPROVED",
                EventTitle = "Customer Approved",
                EventDescription = $"'{task.Title}' approved by customer. Remarks: {dto?.Remarks ?? "—"}",
                OldStatus = oldStatus,
                NewStatus = "APPROVED",
                ProcessCode = task.ProcessCode,
                ProcessName = task.Title,
                CreatedBy = user.UserId,
                CreatedOn = DateTime.Now,
                IsActive = true
            });
            await _db.SaveChangesAsync();
        }

        // ── Generate next workflow step tasks ──
        // Get a system user to trigger the workflow engine (use assigned user or find workflow admin)
        var workflowUser = await GetWorkflowTriggerUserAsync(task);
        if (workflowUser != null && !ShouldSkipNextStepGenerationOnApproval(task))
        {
            await _workspaceEngine.GenerateNextStepTasksAsync(task, workflowUser);
        }

        return Ok(new { success = true, message = "Approved successfully." });
    }

    /// <summary>
    /// Party rejects a pending approval task
    /// </summary>
    [HttpPost("workspace/approvals/{id}/reject")]
    public async Task<IActionResult> RejectPartyApproval(long id, [FromBody] PartyApprovalActionDto? dto = null)
    {
        var user = CurrentUser;
        if (user == null || !user.IsPartyUser || !user.RefId.HasValue)
            return Unauthorized();

        var partyId = (int)user.RefId.Value;
        var task = await _db.TrnWorkspaceTasks.FindAsync(id);

        if (task == null)
            return NotFound(new { message = "Approval not found." });

        if (task.TaskStatus != "PENDING")
            return BadRequest(new { message = "This approval has already been processed." });

        // Verify the task belongs to this party
        var isPartyTask = await VerifyPartyOwnership(task, partyId);
        if (!isPartyTask)
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "You do not have permission to reject this item." });

        if (string.IsNullOrWhiteSpace(dto?.Remarks))
            return BadRequest(new { message = "Please provide a reason for rejection." });

        // Update task status
        var oldStatus = task.TaskStatus;
        task.TaskStatus = "REJECTED";
        task.CompletedBy = user.UserId;
        task.CompletedOn = DateTime.Now;
        task.CompletionRemarks = $"[Party Rejected] {dto.Remarks}";
        task.ModifiedOn = DateTime.Now;

        await _db.SaveChangesAsync();

        // Log party activity
        await LogPartyActivityAsync(
            _db, partyId,
            "APPROVAL", "REJECTED",
            title: $"Rejected: {task.Title}",
            description: $"Approval '{task.Title}' has been rejected. Reason: {dto.Remarks}",
            referenceTable: task.SourceTable,
            referenceId: task.SourceId,
            documentNo: task.SourceNo ?? task.JobNo,
            status: "REJECTED",
            approvalStatus: "REJECTED",
            createdBy: user.Name);

        // Add job timeline if job-linked
        if (task.JobId.HasValue)
        {
            _db.TrnJobTimelines.Add(new TrnJobTimeline
            {
                JobId = task.JobId.Value,
                EventType = "PARTY_APPROVAL",
                EventCode = "PARTY_REJECTED",
                EventTitle = "Customer Rejected",
                EventDescription = $"'{task.Title}' rejected by customer. Reason: {dto.Remarks}",
                OldStatus = oldStatus,
                NewStatus = "REJECTED",
                ProcessCode = task.ProcessCode,
                ProcessName = task.Title,
                CreatedBy = user.UserId,
                CreatedOn = DateTime.Now,
                IsActive = true
            });
            await _db.SaveChangesAsync();
        }

        return Ok(new { success = true, message = "Rejected." });
    }

    /// <summary>
    /// Mark a task as viewed by the party
    /// </summary>
    [HttpPost("workspace/tasks/{id}/viewed")]
    public async Task<IActionResult> MarkTaskViewed(long id)
    {
        var user = CurrentUser;
        if (user == null || !user.IsPartyUser || !user.RefId.HasValue)
            return Unauthorized();

        var partyId = (int)user.RefId.Value;
        var task = await _db.TrnWorkspaceTasks.FindAsync(id);

        if (task == null)
            return NotFound(new { message = "Task not found." });

        var isPartyTask = await VerifyPartyOwnership(task, partyId);
        if (!isPartyTask)
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "Access denied." });

        // Log that party viewed this task
        await LogPartyActivityAsync(
            _db, partyId,
            task.TaskType ?? "TASK", "VIEWED",
            title: $"Viewed: {task.Title}",
            description: $"Customer viewed task '{task.Title}'",
            referenceTable: task.SourceTable,
            referenceId: task.SourceId,
            documentNo: task.SourceNo ?? task.JobNo,
            status: task.TaskStatus,
            createdBy: user.Name);

        return Ok(new { success = true });
    }

    /// <summary>
    /// Get count summary of party workspace items
    /// </summary>
    [HttpGet("workspace/summary")]
    public async Task<IActionResult> GetPartyWorkspaceSummary()
    {
        var user = CurrentUser;
        if (user == null || !user.IsPartyUser || !user.RefId.HasValue)
            return Unauthorized();

        var partyId = (int)user.RefId.Value;

        var partyApprovalProcessCodes = new[] { "QUOT_CUST_APPR", "QUOT_APPROVAL", "ART_CUST_APPR", "PROOF_CUST_APPR", "DESIGN_CUST_APPR", "CUST_APPR" };

        // Count pending approvals
        var pendingApprovals = await _db.TrnWorkspaceTasks
            .Where(t => !t.IsArchived && t.TaskType == "APPROVAL" && t.TaskStatus == "PENDING")
            .Where(t =>
                partyApprovalProcessCodes.Contains(t.ProcessCode!) ||
                (t.SourceTable == "trn_enquiry" && _db.TrnEnquiries.Any(e => e.EnquiryId == t.SourceId && e.PartyId == partyId)) ||
                (t.SourceTable == "trn_quotation" && _db.TrnQuotations.Any(q => q.QuotationId == t.SourceId && q.PartyId == partyId)) ||
                (t.SourceTable == "trn_job" && _db.TrnJobs.Any(j => j.JobId == t.SourceId && j.PartyId == partyId))
            )
            .CountAsync();

        // Count active tasks
        var activeTasks = await _db.TrnWorkspaceTasks
            .Where(t => !t.IsArchived && t.TaskType == "TASK" && (t.TaskStatus == "PENDING" || t.TaskStatus == "IN_PROGRESS"))
            .Where(t =>
                (t.SourceTable == "trn_enquiry" && _db.TrnEnquiries.Any(e => e.EnquiryId == t.SourceId && e.PartyId == partyId)) ||
                (t.SourceTable == "trn_quotation" && _db.TrnQuotations.Any(q => q.QuotationId == t.SourceId && q.PartyId == partyId)) ||
                (t.SourceTable == "trn_job" && _db.TrnJobs.Any(j => j.JobId == t.SourceId && j.PartyId == partyId)) ||
                (t.SourceTable == "trn_challan" && _db.TrnChallans.Any(c => c.ChallanId == t.SourceId && c.PartyId == partyId)) ||
                (t.SourceTable == "trn_sales_invoice" && _db.TrnSalesInvoices.Any(i => i.SalesInvoiceId == t.SourceId && i.PartyId == partyId))
            )
            .CountAsync();

        // Count overdue items
        var overdueItems = await _db.TrnWorkspaceTasks
            .Where(t => !t.IsArchived && t.IsOverdue && t.TaskStatus != "COMPLETED" && t.TaskStatus != "APPROVED" && t.TaskStatus != "REJECTED")
            .Where(t =>
                (t.SourceTable == "trn_enquiry" && _db.TrnEnquiries.Any(e => e.EnquiryId == t.SourceId && e.PartyId == partyId)) ||
                (t.SourceTable == "trn_quotation" && _db.TrnQuotations.Any(q => q.QuotationId == t.SourceId && q.PartyId == partyId)) ||
                (t.SourceTable == "trn_job" && _db.TrnJobs.Any(j => j.JobId == t.SourceId && j.PartyId == partyId))
            )
            .CountAsync();

        return Ok(new
        {
            pendingApprovals,
            activeTasks,
            overdueItems,
            totalActionRequired = pendingApprovals + activeTasks
        });
    }

    /// <summary>
    /// Verify that the task belongs to the specified party
    /// </summary>
    private async Task<bool> VerifyPartyOwnership(TrnWorkspaceTask task, int partyId)
    {
        return task.SourceTable switch
        {
            "trn_enquiry" => await _db.TrnEnquiries.AnyAsync(e => e.EnquiryId == task.SourceId && e.PartyId == partyId),
            "trn_quotation" => await _db.TrnQuotations.AnyAsync(q => q.QuotationId == task.SourceId && q.PartyId == partyId),
            "trn_job" => await _db.TrnJobs.AnyAsync(j => j.JobId == task.SourceId && j.PartyId == partyId),
            "trn_challan" => await _db.TrnChallans.AnyAsync(c => c.ChallanId == task.SourceId && c.PartyId == partyId),
            "trn_sales_invoice" => await _db.TrnSalesInvoices.AnyAsync(i => i.SalesInvoiceId == task.SourceId && i.PartyId == partyId),
            _ => false
        };
    }

    /// <summary>
    /// Get a user session to trigger the workflow engine after party approval.
    /// Falls back to assigned user, then assignedBy user, then a system admin.
    /// </summary>
    private async Task<UserSessionData?> GetWorkflowTriggerUserAsync(TrnWorkspaceTask task)
    {
        // Priority 1: Use the user who was assigned this task (likely department manager)
        if (task.UserId > 0)
        {
            var assignedUser = await _db.MstUsers.FindAsync(task.UserId);
            if (assignedUser != null)
            {
                return new UserSessionData
                {
                    UserId = assignedUser.Userid,
                    Name = assignedUser.Name ?? "System",
                    UserCode = assignedUser.Usercode ?? string.Empty,
                    UserType = "SYSTEM"
                };
            }
        }

        // Priority 2: Use the user who assigned this task
        if (task.AssignedBy.HasValue && task.AssignedBy > 0)
        {
            var assignerUser = await _db.MstUsers.FindAsync(task.AssignedBy.Value);
            if (assignerUser != null)
            {
                return new UserSessionData
                {
                    UserId = assignerUser.Userid,
                    Name = assignerUser.Name ?? "System",
                    UserCode = assignerUser.Usercode ?? string.Empty,
                    UserType = "SYSTEM"
                };
            }
        }

        // Priority 3: Fall back to any active admin user (system admin)
        var adminUser = await _db.MstUsers
            .Where(u => u.Isactive == true && u.Issystemadmin == true)
            .OrderBy(u => u.Userid)
            .FirstOrDefaultAsync();

        if (adminUser != null)
        {
            return new UserSessionData
            {
                UserId = adminUser.Userid,
                Name = adminUser.Name ?? "System",
                UserCode = adminUser.Usercode ?? string.Empty,
                UserType = "SYSTEM"
            };
        }

        return null;
    }

    /// <summary>
    /// Determines if next step generation should be skipped for this approval type.
    /// Matches logic from WorkspaceController to avoid duplicate task generation.
    /// </summary>
    private static bool ShouldSkipNextStepGenerationOnApproval(TrnWorkspaceTask task)
    {
        var processCode = (task.ProcessCode ?? string.Empty).ToUpperInvariant();
        var taskTitle = (task.Title ?? string.Empty).ToUpperInvariant();
        var sourceTable = (task.SourceTable ?? string.Empty).ToLowerInvariant();

        // Skip next-step for quotation/job conversion approvals (handled by dedicated conversion logic)
        // Only applies when source is ENQUIRY or QUOTATION — NOT for manual jobs (source = trn_job)
        var isQuotationConversionApproval = sourceTable == "trn_enquiry" &&
                                            (processCode.Contains("QUOT") || taskTitle.Contains("QUOTATION GENERATION"));

        // Job conversion only applies when source is enquiry/quotation — manual jobs proceed to next step
        var isJobConversionApproval = (sourceTable == "trn_enquiry" || sourceTable == "trn_quotation") &&
                                      (processCode.Contains("JOB_CREATE") || taskTitle.Contains("JOB CREATION"));

        return isQuotationConversionApproval || isJobConversionApproval;
    }

    // ═══════════════════════════════════════════════════════════════
    // CUSTOMER: APPROVALS (Legacy - redirects to workspace)
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("customer/approvals")]
    public async Task<IActionResult> GetCustomerApprovals()
    {
        // Redirect to new workspace approvals endpoint
        return await GetPartyWorkspaceApprovals("pending");
    }

    // ═══════════════════════════════════════════════════════════════
    // CUSTOMER: JOB TRACKING
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("customer/job-tracking")]
    public IActionResult GetCustomerJobTracking()
    {
        var user = CurrentUser;
        if (user == null || !user.IsPartyUser)
            return Unauthorized();

        return Ok(new { items = Array.Empty<object>(), total = 0 });
    }

    // ═══════════════════════════════════════════════════════════════
    // CUSTOMER: REQUESTS
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("customer/requests")]
    public IActionResult GetCustomerRequests()
    {
        var user = CurrentUser;
        if (user == null || !user.IsPartyUser)
            return Unauthorized();

        return Ok(new { items = Array.Empty<object>(), total = 0 });
    }

    // ═══════════════════════════════════════════════════════════════
    // CUSTOMER: COMPLAINTS
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("customer/complaints")]
    public IActionResult GetCustomerComplaints()
    {
        var user = CurrentUser;
        if (user == null || !user.IsPartyUser)
            return Unauthorized();

        return Ok(new { items = Array.Empty<object>(), total = 0 });
    }

    // ═══════════════════════════════════════════════════════════════
    // CUSTOMER: FEEDBACK
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("customer/feedback")]
    public IActionResult GetCustomerFeedback()
    {
        var user = CurrentUser;
        if (user == null || !user.IsPartyUser)
            return Unauthorized();

        return Ok(new { items = Array.Empty<object>(), total = 0 });
    }

    // ═══════════════════════════════════════════════════════════════
    // SUPPLIER: PURCHASE ORDERS
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("supplier/purchase-orders")]
    public IActionResult GetSupplierPurchaseOrders()
    {
        var user = CurrentUser;
        if (user == null || !user.IsPartyUser)
            return Unauthorized();

        return Ok(new { items = Array.Empty<object>(), total = 0 });
    }

    // ═══════════════════════════════════════════════════════════════
    // VENDOR: CONTRACTS
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("vendor/contracts")]
    public IActionResult GetVendorContracts()
    {
        var user = CurrentUser;
        if (user == null || !user.IsPartyUser)
            return Unauthorized();

        return Ok(new { items = Array.Empty<object>(), total = 0 });
    }

    // ═══════════════════════════════════════════════════════════════
    // PROFILE
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var user = CurrentUser;
        if (user == null || !user.IsPartyUser || !user.RefId.HasValue)
            return Unauthorized();

        var partyId = (int)user.RefId.Value;

        var party = await _db.MstParties
            .Include(p => p.MstPartyRoles)
            .Include(p => p.MstPartyContacts)
            .Include(p => p.MstPartyAddresses).ThenInclude(a => a.State)
            .Include(p => p.MstPartyAddresses).ThenInclude(a => a.City)
            .Include(p => p.MstPartyBanks)
            .FirstOrDefaultAsync(p => p.Id == partyId);

        if (party == null)
            return NotFound(new { message = "Party not found." });

        string? cityName = null;
        if (party.CityId.HasValue)
        {
            cityName = await _db.MstCities
                .Where(c => c.Id == party.CityId.Value)
                .Select(c => c.Name)
                .FirstOrDefaultAsync();
        }

        return Ok(new
        {
            party.Id,
            party.Name,
            party.Code,
            party.Email,
            party.Mobile,
            GstNo = party.Gstno,
            PanNo = party.PanNo,
            party.Address1,
            party.Address2,
            party.CityId,
            CityName = cityName,
            party.Pin,
            party.IsActive,
            CreatedOn = party.CreatedOn.ToString("dd MMM yyyy"),
            Roles = party.MstPartyRoles
                .Where(r => r.IsActive)
                .Select(r => r.RoleType)
                .ToList(),
            Contacts = party.MstPartyContacts
                .Where(c => c.IsActive)
                .Select(c => new
                {
                    c.ContactName,
                    c.Designation,
                    c.Email,
                    c.Mobile
                }).ToList(),
            Addresses = party.MstPartyAddresses
                .Where(a => a.IsActive == true)
                .Select(a => new
                {
                    a.AddressType,
                    a.AddressLabel,
                    a.IsDefault,
                    a.AddressLine1,
                    a.AddressLine2,
                    a.StateId,
                    StateName = a.State != null ? a.State.Name : null,
                    a.CityId,
                    CityName = a.City != null ? a.City.Name : null,
                    a.PostalCode,
                    a.Gstin,
                    a.ContactPersonName,
                    a.ContactPhone,
                    a.ContactEmail
                }).ToList(),
            Banks = party.MstPartyBanks.Select(b => new
            {
                b.BankName,
                b.BranchName,
                b.AccountNo,
                b.IfscCode,
                b.MicrNo
            }).ToList()
        });
    }

    [HttpPost("profile/update")]
    public async Task<IActionResult> UpdateProfile([FromBody] ProfileUpdateDto dto)
    {
        var user = CurrentUser;
        if (user == null || !user.IsPartyUser || !user.RefId.HasValue)
            return Unauthorized();

        var partyId = (int)user.RefId.Value;
        var party = await _db.MstParties.FindAsync(partyId);
        if (party == null)
            return NotFound(new { message = "Party not found." });

        // Party can update limited fields only
        if (!string.IsNullOrWhiteSpace(dto.Email))
            party.Email = dto.Email.Trim();

        if (dto.Mobile.HasValue)
            party.Mobile = dto.Mobile;

        if (dto.Address1 != null)
            party.Address1 = dto.Address1.Trim();

        if (dto.Address2 != null)
            party.Address2 = dto.Address2.Trim();

        if (dto.Pin != null)
            party.Pin = dto.Pin.Trim();

        await _db.SaveChangesAsync();

        // Also update the user_master email/mobile if present
        var mstUser = await _db.MstUsers
            .FirstOrDefaultAsync(u => u.UserType == "PARTY" && u.RefId == partyId);
        if (mstUser != null)
        {
            if (!string.IsNullOrWhiteSpace(dto.Email))
                mstUser.Emailid = dto.Email.Trim();
            if (dto.Mobile.HasValue)
                mstUser.Mobileno = dto.Mobile.Value.ToString();
            await _db.SaveChangesAsync();
        }

        return Ok(new { success = true, message = "Profile updated successfully." });
    }

    [HttpPost("profile/change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var user = CurrentUser;
        if (user == null || !user.IsPartyUser || !user.RefId.HasValue)
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(dto.CurrentPassword) ||
            string.IsNullOrWhiteSpace(dto.NewPassword))
            return BadRequest(new { message = "Current and new passwords are required." });

        if (dto.NewPassword.Length < 6)
            return BadRequest(new { message = "New password must be at least 6 characters." });

        if (dto.NewPassword != dto.ConfirmPassword)
            return BadRequest(new { message = "New password and confirmation do not match." });

        var mstUser = await _db.MstUsers
            .FirstOrDefaultAsync(u => u.UserType == "PARTY" && u.RefId == user.RefId);
        if (mstUser == null)
            return NotFound(new { message = "User account not found." });

        var currentHash = ComputeSha256(dto.CurrentPassword);
        if (mstUser.Passwordhash != currentHash)
            return BadRequest(new { message = "Current password is incorrect." });

        mstUser.Passwordhash = ComputeSha256(dto.NewPassword);
        await _db.SaveChangesAsync();

        return Ok(new { success = true, message = "Password changed successfully." });
    }

    private static string ComputeSha256(string input)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(input));
        var sb = new System.Text.StringBuilder();
        foreach (var b in bytes)
            sb.Append(b.ToString("x2"));
        return sb.ToString();
    }

    // ═══════════════════════════════════════════════════════════════
    // ACTIVITY LOG
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("activities")]
    public async Task<IActionResult> GetActivities(
        [FromQuery] string? type = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var user = CurrentUser;
        if (user == null || !user.IsPartyUser || !user.RefId.HasValue)
            return Unauthorized();

        var partyId = (int)user.RefId.Value;

        var query = _db.PartyActivityLogs
            .Where(a => a.PartyId == partyId && a.IsActive == true)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(a => a.ActivityType == type);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.CreatedOn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new
            {
                a.ActivityId,
                a.ActivityType,
                a.ActivityCode,
                a.ReferenceTable,
                a.ReferenceId,
                a.DocumentNo,
                DocumentDate = a.DocumentDate.HasValue
                    ? a.DocumentDate.Value.ToString("dd MMM yyyy") : null,
                a.ActivityTitle,
                a.ActivityDescription,
                a.Status,
                a.ApprovalStatus,
                a.Amount,
                a.CreatedBy,
                CreatedOn = a.CreatedOn.HasValue
                    ? a.CreatedOn.Value.ToString("dd MMM yyyy HH:mm") : null,
                CreatedOnRaw = a.CreatedOn
            })
            .ToListAsync();

        return Ok(new { items, total, page, pageSize });
    }

    [HttpGet("activities/recent")]
    public async Task<IActionResult> GetRecentActivities([FromQuery] int count = 10)
    {
        var user = CurrentUser;
        if (user == null || !user.IsPartyUser || !user.RefId.HasValue)
            return Unauthorized();

        var partyId = (int)user.RefId.Value;

        var items = await _db.PartyActivityLogs
            .Where(a => a.PartyId == partyId && a.IsActive == true)
            .OrderByDescending(a => a.CreatedOn)
            .Take(count)
            .Select(a => new
            {
                a.ActivityId,
                a.ActivityType,
                a.ActivityCode,
                a.ActivityTitle,
                a.ActivityDescription,
                a.Status,
                a.Amount,
                a.DocumentNo,
                CreatedOn = a.CreatedOn.HasValue
                    ? a.CreatedOn.Value.ToString("dd MMM yyyy HH:mm") : null,
                CreatedOnRaw = a.CreatedOn
            })
            .ToListAsync();

        var totalCount = await _db.PartyActivityLogs
            .CountAsync(a => a.PartyId == partyId && a.IsActive == true);

        return Ok(new { items, totalCount });
    }

    [HttpGet("activities/summary")]
    public async Task<IActionResult> GetActivitySummary()
    {
        var user = CurrentUser;
        if (user == null || !user.IsPartyUser || !user.RefId.HasValue)
            return Unauthorized();

        var partyId = (int)user.RefId.Value;

        var summary = await _db.PartyActivityLogs
            .Where(a => a.PartyId == partyId && a.IsActive == true)
            .GroupBy(a => a.ActivityType)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToListAsync();

        var recentCount = await _db.PartyActivityLogs
            .CountAsync(a => a.PartyId == partyId && a.IsActive == true
                && a.CreatedOn >= DateTime.UtcNow.AddDays(-7));

        return Ok(new { summary, recentCount });
    }

    // ═══════════════════════════════════════════════════════════════
    // STATIC — Log Party Activity (used by other controllers)
    // ═══════════════════════════════════════════════════════════════

    public static async Task LogPartyActivityAsync(
        ApplicationDbContext db,
        int partyId,
        string activityType,
        string activityCode,
        string? title,
        string? description = null,
        string? referenceTable = null,
        long? referenceId = null,
        string? documentNo = null,
        DateOnly? documentDate = null,
        string? status = null,
        string? approvalStatus = null,
        decimal? amount = null,
        string? createdBy = null)
    {
        try
        {
            var log = new PartyActivityLog
            {
                PartyId = partyId,
                ActivityType = activityType,
                ActivityCode = activityCode,
                ActivityTitle = title,
                ActivityDescription = description,
                ReferenceTable = referenceTable,
                ReferenceId = referenceId,
                DocumentNo = documentNo,
                DocumentDate = documentDate,
                Status = status ?? "Completed",
                ApprovalStatus = approvalStatus ?? "Not Required",
                Amount = amount,
                CreatedBy = createdBy,
                CreatedOn = DateTime.Now,
                IsActive = true
            };
            db.PartyActivityLogs.Add(log);
            await db.SaveChangesAsync();
        }
        catch
        {
            // Fire-and-forget: don't break the main flow
        }
    }

    // ─── Profile DTOs ───

    public class ProfileUpdateDto
    {
        public string? Email { get; set; }
        public long? Mobile { get; set; }
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? Pin { get; set; }
    }

    public class ChangePasswordDto
    {
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
        public string? ConfirmPassword { get; set; }
    }

    public class PartyApprovalActionDto
    {
        public string? Remarks { get; set; }
    }
}
