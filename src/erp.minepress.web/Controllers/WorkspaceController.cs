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
public class WorkspaceController : ControllerBase
{
    private static readonly string[] DisabledProcessCodes = ["QUOT_APPR", "QUOT_APPROVAL", "QUOTATION_APPROVAL", "PROC", "GRN", "QC_IN"];

    private readonly ApplicationDbContext _db;
    private readonly IUserActivityService _activityService;
    private readonly INotificationDispatcher _notificationDispatcher;
    private readonly IWorkspaceProcessEngine _workspaceEngine;
    private readonly IItemTaskService _itemTaskService;
    private readonly ISystemErrorLogger _systemErrorLogger;
    private readonly ILogger<WorkspaceController> _logger;

    public WorkspaceController(
        ApplicationDbContext db,
        IUserActivityService activityService,
        INotificationDispatcher notificationDispatcher,
        IWorkspaceProcessEngine workspaceEngine,
        IItemTaskService itemTaskService,
        ISystemErrorLogger systemErrorLogger,
        ILogger<WorkspaceController> logger)
    {
        _db = db;
        _activityService = activityService;
        _notificationDispatcher = notificationDispatcher;
        _workspaceEngine = workspaceEngine;
        _itemTaskService = itemTaskService;
        _systemErrorLogger = systemErrorLogger;
        _logger = logger;
    }

    // ── Dashboard Summary ──
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized(new { message = "Session expired." });

        var userId = user.UserId;
        var departmentId = user.DepartmentId;

        // Show all tasks for department OR assigned to user - no workflow sequence check
        var tasks = _db.TrnWorkspaceTasks.Where(t => (t.DepartmentId == departmentId || t.UserId == userId) && !t.IsArchived);

        var pendingTasks = await tasks.CountAsync(t => t.TaskType == "TASK" && t.TaskStatus == "PENDING");
        var inProgressTasks = await tasks.CountAsync(t => t.TaskType == "TASK" && t.TaskStatus == "IN_PROGRESS");
        var completedTasks = await tasks.CountAsync(t => t.TaskType == "TASK" && t.TaskStatus == "COMPLETED");
        var assignedByMe = await _db.TrnWorkspaceTasks.CountAsync(t => t.AssignedBy == userId && !t.IsArchived && t.TaskType == "TASK");
        var pendingApprovals = await tasks.CountAsync(t => t.TaskType == "APPROVAL" && t.TaskStatus == "PENDING");
        var overdueTasks = await tasks.CountAsync(t => t.IsOverdue && t.TaskStatus != "COMPLETED" && t.TaskStatus != "CANCELLED");
        var todayDue = await tasks.CountAsync(t => t.DueDate.HasValue && t.DueDate.Value.Date == DateTime.Today && t.TaskStatus != "COMPLETED");
        var totalActive = await tasks.CountAsync(t => t.TaskStatus != "COMPLETED" && t.TaskStatus != "CANCELLED" && t.TaskStatus != "REJECTED");

        return Ok(new
        {
            pendingTasks,
            inProgressTasks,
            completedTasks,
            assignedByMe,
            pendingApprovals,
            overdueTasks,
            todayDue,
            totalActive
        });
    }

    // ── Real-Time Alerts (for toast notifications) ──
    [HttpGet("alerts/new")]
    public async Task<IActionResult> GetNewAlerts([FromQuery] string? since = null)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized(new { message = "Session expired." });

        var userId = user.UserId;
        var checkTime = DateTime.Now;

        // Parse since parameter or default to last 5 minutes
        DateTime sinceTime;
        if (!string.IsNullOrEmpty(since) && DateTime.TryParse(since, out var parsedSince))
        {
            sinceTime = parsedSince;
        }
        else
        {
            sinceTime = checkTime.AddMinutes(-5);
        }

        var departmentId = user.DepartmentId;

        // Get new/unread tasks and approvals - show all for department OR assigned to user
        var newTasks = await _db.TrnWorkspaceTasks
            .Include(t => t.Process)
            .Where(t => (t.DepartmentId == departmentId || t.UserId == userId) && 
                        !t.IsArchived && 
                        !t.IsRead &&
                        t.CreatedOn >= sinceTime &&
                        (t.TaskStatus == WkTaskStatus.Pending || t.TaskStatus == WkTaskStatus.InProgress))
            .OrderByDescending(t => t.CreatedOn)
            .Take(10)
            .Select(t => new
            {
                id = $"{t.TaskType}-{t.WorkspaceTaskId}",
                taskId = t.WorkspaceTaskId,
                type = t.TaskType == "APPROVAL" ? "approval" : "task",
                tag = t.TaskType,
                title = t.Title,
                description = t.Description,
                priority = t.Priority ?? "NORMAL",
                processName = t.Process != null ? t.Process.Processname : null,
                jobNo = t.JobNo,
                partyName = t.PartyName,
                isOverdue = t.IsOverdue,
                actionUrl = t.ActionUrl ?? (t.TaskType == "APPROVAL" ? "/Workspace/Approvals" : "/Workspace/MyTasks"),
                createdOn = t.CreatedOn
            })
            .ToListAsync();

        // Get recent notifications that may need toast display
        var recentNotifications = await _db.TrnUserNotifications
            .Where(n => n.UserId == userId && 
                        (n.IsRead == false || n.IsRead == null) && 
                        n.CreatedOn >= sinceTime &&
                        n.ActionRequired == true)
            .OrderByDescending(n => n.CreatedOn)
            .Take(5)
            .Select(n => new
            {
                id = $"notif-{n.UserNotificationId}",
                type = "info",
                tag = n.Module,
                title = n.Title,
                description = n.Message,
                priority = n.Priority ?? "NORMAL",
                actionUrl = n.ActionUrl ?? n.ReferenceUrl,
                createdOn = n.CreatedOn
            })
            .ToListAsync();

        // Combine and return
        var alerts = newTasks
            .Select(t => new
            {
                t.id,
                t.type,
                t.tag,
                t.title,
                message = t.description,
                t.priority,
                t.jobNo,
                t.partyName,
                t.isOverdue,
                t.actionUrl,
                createdOn = t.createdOn.ToString("yyyy-MM-ddTHH:mm:ss")
            })
            .Concat(recentNotifications.Select(n => new
            {
                n.id,
                n.type,
                n.tag,
                n.title,
                message = n.description,
                n.priority,
                jobNo = (string?)null,
                partyName = (string?)null,
                isOverdue = false,
                n.actionUrl,
                createdOn = n.createdOn.HasValue ? n.createdOn.Value.ToString("yyyy-MM-ddTHH:mm:ss") : null
            }))
            .OrderByDescending(a => a.createdOn)
            .Take(10)
            .ToList();

        return Ok(new
        {
            checkTime = checkTime.ToString("yyyy-MM-ddTHH:mm:ss"),
            alerts
        });
    }

    [HttpGet("resolve-quotation/{enquiryId:long}")]
    public async Task<IActionResult> ResolveLatestQuotationFromEnquiry(long enquiryId)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized(new { message = "Session expired." });

        var quotation = await _db.TrnQuotations
            .Where(q => q.EnquiryId == enquiryId)
            .OrderByDescending(q => q.QuotationId)
            .Select(q => new { q.QuotationId, q.QuotationNo, q.Status })
            .FirstOrDefaultAsync();

        if (quotation == null)
            return NotFound(new { message = "No quotation found for enquiry." });

        return Ok(quotation);
    }

    // ── My Tasks ──
    [HttpGet("tasks")]
    public async Task<IActionResult> GetTasks([FromQuery] string filter = "pending", [FromQuery] string? priority = null, [FromQuery] string? search = null)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized(new { message = "Session expired." });

        var userId = user.UserId;
        var departmentId = user.DepartmentId;

        // Show all tasks for department OR assigned to user - no workflow sequence check
        var query = _db.TrnWorkspaceTasks
            .Where(t => (t.DepartmentId == departmentId || t.UserId == userId) && !t.IsArchived && t.TaskType == "TASK");

        query = filter.ToLower() switch
        {
            "pending" => query.Where(t => t.TaskStatus == "PENDING" || t.TaskStatus == "IN_PROGRESS"),
            "completed" => query.Where(t => t.TaskStatus == "COMPLETED"),
            "assigned" => _db.TrnWorkspaceTasks
                .Where(t => t.AssignedBy == userId && !t.IsArchived && t.TaskType == "TASK"),
            "overdue" => query.Where(t => t.IsOverdue && t.TaskStatus != "COMPLETED"),
            _ => query
        };

        if (!string.IsNullOrEmpty(priority))
            query = query.Where(t => t.Priority == priority.ToUpper());

        if (!string.IsNullOrEmpty(search))
            query = query.Where(t => (t.Title != null && t.Title.Contains(search)) ||
                                      (t.JobNo != null && t.JobNo.Contains(search)) ||
                                      (t.PartyName != null && t.PartyName.Contains(search)));

        var tasks = await query
            .OrderBy(t => t.SequenceNo ?? int.MaxValue)
            .ThenByDescending(t => t.Priority == "CRITICAL" ? 5 :
                                    t.Priority == "URGENT" ? 4 :
                                    t.Priority == "HIGH" ? 3 :
                                    t.Priority == "NORMAL" ? 2 : 1)
            .ThenBy(t => t.DueDate)
            .ThenByDescending(t => t.CreatedOn)
            .Select(t => new
            {
                t.WorkspaceTaskId,
                TaskId = t.WorkspaceTaskId,
                t.UserId,
                t.SourceTable,
                t.SourceId,
                t.SourceNo,
                t.TaskType,
                t.TaskStatus,
                t.Title,
                t.Description,
                t.ProcessId,
                t.ProcessCode,
                t.SubprocessId,
                t.SubprocessCode,
                t.DepartmentId,
                t.AssignedBy,
                AssignedOn = t.AssignedOn.HasValue ? t.AssignedOn.Value.ToString("dd-MMM-yyyy HH:mm") : null,
                AssignedOnIso = t.AssignedOn.HasValue ? t.AssignedOn.Value.ToString("yyyy-MM-ddTHH:mm:ss") : null,
                t.Priority,
                DueDate = t.DueDate.HasValue ? t.DueDate.Value.ToString("dd-MMM-yyyy HH:mm") : null,
                DueDateIso = t.DueDate.HasValue ? t.DueDate.Value.ToString("yyyy-MM-ddTHH:mm:ss") : null,
                t.SlaHours,
                t.IsOverdue,
                t.ApprovalTypeId,
                t.ApprovalLevel,
                t.CompletedBy,
                CompletedOn = t.CompletedOn.HasValue ? t.CompletedOn.Value.ToString("dd-MMM-yyyy HH:mm") : null,
                CompletedOnIso = t.CompletedOn.HasValue ? t.CompletedOn.Value.ToString("yyyy-MM-ddTHH:mm:ss") : null,
                t.CompletionRemarks,
                t.ActionUrl,
                t.JobId,
                t.JobNo,
                t.PartyName,
                t.IsRead,
                ReadAt = t.ReadAt.HasValue ? t.ReadAt.Value.ToString("dd-MMM-yyyy HH:mm") : null,
                t.IsArchived,
                CreatedOn = t.CreatedOn.ToString("dd-MMM-yyyy HH:mm"),
                CreatedOnIso = t.CreatedOn.ToString("yyyy-MM-ddTHH:mm:ss"),
                ModifiedOn = t.ModifiedOn.HasValue ? t.ModifiedOn.Value.ToString("dd-MMM-yyyy HH:mm") : null,
                t.SequenceNo,
                t.WorkflowStepId,
                t.WorkflowTemplateId,
                t.WorkflowBatchId,
                t.IsBlocking,
                t.Metadata
            })
            .ToListAsync();

        return Ok(tasks);
    }

    // ── Approvals ──
    [HttpGet("approvals")]
    public async Task<IActionResult> GetApprovals([FromQuery] string filter = "pending", [FromQuery] string? search = null)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized(new { message = "Session expired." });

        var userId = user.UserId;
        var departmentId = user.DepartmentId;

        // Show all approvals for department OR assigned to user - no workflow sequence check
        var query = _db.TrnWorkspaceTasks
            .Where(t => (t.DepartmentId == departmentId || t.UserId == userId) && !t.IsArchived && t.TaskType == "APPROVAL");

        query = filter.ToLower() switch
        {
            "pending" => query.Where(t => t.TaskStatus == "PENDING"),
            "approved" => query.Where(t => t.TaskStatus == "APPROVED"),
            "rejected" => query.Where(t => t.TaskStatus == "REJECTED"),
            _ => query
        };

        if (!string.IsNullOrEmpty(search))
            query = query.Where(t => (t.Title != null && t.Title.Contains(search)) ||
                                      (t.JobNo != null && t.JobNo.Contains(search)) ||
                                      (t.PartyName != null && t.PartyName.Contains(search)));

        var approvals = await query
            .OrderBy(t => t.SequenceNo ?? int.MaxValue)
            .ThenByDescending(t => t.Priority == "CRITICAL" ? 5 :
                                    t.Priority == "URGENT" ? 4 :
                                    t.Priority == "HIGH" ? 3 :
                                    t.Priority == "NORMAL" ? 2 : 1)
            .ThenBy(t => t.DueDate)
            .ThenByDescending(t => t.CreatedOn)
            .Select(t => new
            {
                t.WorkspaceTaskId,
                TaskId = t.WorkspaceTaskId,
                t.UserId,
                t.SourceTable,
                t.SourceId,
                t.SourceNo,
                t.TaskType,
                t.TaskStatus,
                t.Title,
                t.Description,
                t.ProcessId,
                t.ProcessCode,
                t.SubprocessId,
                t.SubprocessCode,
                t.DepartmentId,
                t.AssignedBy,
                AssignedOn = t.AssignedOn.HasValue ? t.AssignedOn.Value.ToString("dd-MMM-yyyy HH:mm") : null,
                AssignedOnIso = t.AssignedOn.HasValue ? t.AssignedOn.Value.ToString("yyyy-MM-ddTHH:mm:ss") : null,
                t.Priority,
                DueDate = t.DueDate.HasValue ? t.DueDate.Value.ToString("dd-MMM-yyyy HH:mm") : null,
                DueDateIso = t.DueDate.HasValue ? t.DueDate.Value.ToString("yyyy-MM-ddTHH:mm:ss") : null,
                t.SlaHours,
                t.IsOverdue,
                t.ApprovalTypeId,
                t.ApprovalLevel,
                t.CompletedBy,
                CompletedOn = t.CompletedOn.HasValue ? t.CompletedOn.Value.ToString("dd-MMM-yyyy HH:mm") : null,
                CompletedOnIso = t.CompletedOn.HasValue ? t.CompletedOn.Value.ToString("yyyy-MM-ddTHH:mm:ss") : null,
                t.CompletionRemarks,
                t.ActionUrl,
                t.JobId,
                t.JobNo,
                t.PartyName,
                t.IsRead,
                ReadAt = t.ReadAt.HasValue ? t.ReadAt.Value.ToString("dd-MMM-yyyy HH:mm") : null,
                t.IsArchived,
                CreatedOn = t.CreatedOn.ToString("dd-MMM-yyyy HH:mm"),
                CreatedOnIso = t.CreatedOn.ToString("yyyy-MM-ddTHH:mm:ss"),
                ModifiedOn = t.ModifiedOn.HasValue ? t.ModifiedOn.Value.ToString("dd-MMM-yyyy HH:mm") : null,
                t.SequenceNo,
                t.WorkflowStepId,
                t.WorkflowTemplateId,
                t.WorkflowBatchId,
                t.IsBlocking,
                t.Metadata
            })
            .ToListAsync();

        return Ok(approvals);
    }

    // ── Calendar Data ──
    [HttpGet("calendar")]
    public async Task<IActionResult> GetCalendarData([FromQuery] string? start = null, [FromQuery] string? end = null)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized(new { message = "Session expired." });

        var userId = user.UserId;
        var departmentId = user.DepartmentId;
        var startDate = string.IsNullOrEmpty(start) ? DateTime.Today.AddDays(-30) : DateTime.Parse(start);
        var endDate = string.IsNullOrEmpty(end) ? DateTime.Today.AddDays(30) : DateTime.Parse(end);

        var events = await _db.TrnWorkspaceTasks
            .Where(t => (t.DepartmentId == departmentId || t.UserId == userId) && !t.IsArchived &&
                         t.DueDate.HasValue && t.DueDate >= startDate && t.DueDate <= endDate)
            .Select(t => new
            {
                id = t.WorkspaceTaskId,
                title = t.Title,
                date = t.DueDate!.Value.ToString("yyyy-MM-dd"),
                start = t.DueDate!.Value.ToString("yyyy-MM-ddTHH:mm:ss"),
                end = t.DueDate!.Value.AddHours((double)(t.SlaHours ?? 1)).ToString("yyyy-MM-ddTHH:mm:ss"),
                color = t.Priority == "CRITICAL" ? "#d63939" :
                        t.Priority == "URGENT" ? "#f76707" :
                        t.Priority == "HIGH" ? "#f59f00" :
                        t.TaskType == "APPROVAL" ? "#4263eb" : "#2fb344",
                className = t.IsOverdue ? "fc-event-overdue" : "",
                extendedProps = new
                {
                    t.TaskType,
                    t.TaskStatus,
                    t.Priority,
                    t.JobNo,
                    t.PartyName,
                    t.IsOverdue,
                    t.ActionUrl
                }
            })
            .ToListAsync();

        return Ok(events);
    }

    // ── Calendar Day View ──
    [HttpGet("calendar/day")]
    public async Task<IActionResult> GetCalendarDayData([FromQuery] string? date = null)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized(new { message = "Session expired." });

        var userId = user.UserId;
        var departmentId = user.DepartmentId;
        var targetDate = string.IsNullOrEmpty(date) ? DateTime.Today : DateTime.Parse(date).Date;

        var tasks = await _db.TrnWorkspaceTasks
            .Include(t => t.Department)
            .Include(t => t.Process)
            .Include(t => t.AssignedByNavigation)
            .Where(t => (t.DepartmentId == departmentId || t.UserId == userId) && !t.IsArchived &&
                         t.DueDate.HasValue && t.DueDate.Value.Date == targetDate)
            .OrderBy(t => t.DueDate)
            .ThenByDescending(t => t.Priority == "CRITICAL" ? 5 :
                                    t.Priority == "URGENT" ? 4 :
                                    t.Priority == "HIGH" ? 3 :
                                    t.Priority == "NORMAL" ? 2 : 1)
            .Select(t => new
            {
                t.WorkspaceTaskId,
                TaskId = t.WorkspaceTaskId,
                t.Title,
                t.Description,
                t.TaskType,
                t.TaskStatus,
                t.Priority,
                t.IsOverdue,
                t.JobNo,
                t.JobId,
                t.PartyName,
                t.ProcessCode,
                DepartmentName = t.Department != null ? t.Department.DeptName : null,
                ProcessName = t.Process != null ? t.Process.Processname : null,
                AssignedByName = t.AssignedByNavigation != null ? t.AssignedByNavigation.Name : null,
                DueDate = t.DueDate!.Value.ToString("yyyy-MM-ddTHH:mm:ss"),
                DueTime = t.DueDate!.Value.ToString("HH:mm"),
                DueHour = t.DueDate!.Value.Hour,
                EndTime = t.DueDate!.Value.AddHours((double)(t.SlaHours ?? 1)).ToString("HH:mm"),
                EndHour = t.DueDate!.Value.AddHours((double)(t.SlaHours ?? 1)).Hour,
                t.SlaHours,
                t.ActionUrl,
                color = t.Priority == "CRITICAL" ? "#d63939" :
                        t.Priority == "URGENT" ? "#f76707" :
                        t.Priority == "HIGH" ? "#f59f00" :
                        t.TaskType == "APPROVAL" ? "#4263eb" : "#2fb344"
            })
            .ToListAsync();

        // Summary counts for the day
        var summary = new
        {
            date = targetDate.ToString("yyyy-MM-dd"),
            dateFormatted = targetDate.ToString("dddd, dd MMMM yyyy"),
            total = tasks.Count,
            pending = tasks.Count(t => t.TaskStatus == "PENDING"),
            inProgress = tasks.Count(t => t.TaskStatus == "IN_PROGRESS"),
            completed = tasks.Count(t => t.TaskStatus == "COMPLETED" || t.TaskStatus == "APPROVED"),
            overdue = tasks.Count(t => t.IsOverdue),
            approvals = tasks.Count(t => t.TaskType == "APPROVAL"),
            isToday = targetDate == DateTime.Today
        };

        return Ok(new { summary, tasks });
    }

    // ── Notifications ──
    [HttpGet("notifications")]
    public async Task<IActionResult> GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized(new { message = "Session expired." });

        var query = _db.TrnUserNotifications
            .Where(n => n.UserId == user.UserId)
            .OrderByDescending(n => n.CreatedOn);

        var total = await query.CountAsync();
        var notifications = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new
            {
                n.UserNotificationId,
                n.Title,
                n.Message,
                n.Icon,
                n.Color,
                n.Module,
                n.EventType,
                n.IsRead,
                n.ActionRequired,
                n.ActionUrl,
                n.ReferenceId,
                n.ReferenceUrl,
                n.Priority,
                CreatedOn = n.CreatedOn.HasValue ? n.CreatedOn.Value.ToString("dd-MMM-yyyy HH:mm") : null
            })
            .ToListAsync();

        return Ok(new { total, page, pageSize, data = notifications });
    }

    // ── History ──
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] int days = 30)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized(new { message = "Session expired." });

        var sinceDate = DateTime.Now.AddDays(-days);
        var query = _db.TrnUserActivityLogs
            .Where(a => a.UserId == user.UserId && a.ActivityOn >= sinceDate)
            .OrderByDescending(a => a.ActivityOn);

        var total = await query.CountAsync();
        var history = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new
            {
                a.ActivityLogId,
                a.Module,
                a.SubModule,
                a.ActivityType,
                a.ActivityCategory,
                a.EntityType,
                a.EntityId,
                a.EntityCode,
                a.Title,
                a.Description,
                ActivityOn = a.ActivityOn.ToString("dd-MMM-yyyy HH:mm"),
                a.Severity,
                a.Channel,
                a.RequestPath
            })
            .ToListAsync();

        return Ok(new { total, page, pageSize, data = history });
    }

    // ── Process Flow (Prev / Current / Next) ──
    [HttpGet("process-flow/{taskId}")]
    public async Task<IActionResult> GetProcessFlow(long taskId)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized(new { message = "Session expired." });

        var task = await _db.TrnWorkspaceTasks.FindAsync(taskId);
        if (task == null) return NotFound(new { message = "Task not found." });

        if (!task.ProcessId.HasValue || !task.JobId.HasValue)
            return Ok(new { steps = Array.Empty<object>(), currentIndex = -1 });

        // Get ordered process flow (template-aware by job type/product type)
        var allProcesses = await ResolveJobProcessStepsAsync(task.JobId.Value);

        if (allProcesses.Count == 0)
            return Ok(new { steps = Array.Empty<object>(), currentIndex = -1 });

        // Get all workspace tasks for this job to determine step statuses
        var jobTasks = await _db.TrnWorkspaceTasks
            .Include(t => t.User)
            .Include(t => t.Department)
            .Where(t => t.JobId == task.JobId && !t.IsArchived)
            .ToListAsync();

        var steps = new List<object>();
        int currentIndex = -1;

        for (int i = 0; i < allProcesses.Count; i++)
        {
            var proc = allProcesses[i];

            // Find matching workspace task(s) for this process step
            var matchingTasks = jobTasks
                .Where(t => t.ProcessCode == proc.ProcessCode)
                .OrderByDescending(t => t.CreatedOn)
                .ToList();

            var latestTask = matchingTasks.FirstOrDefault();
            string stepStatus = latestTask?.TaskStatus ?? "NOT_STARTED";
            string? assignedUserName = latestTask?.User?.Name;
            long? assignedUserId = latestTask?.UserId;
            string? departmentName = latestTask?.Department?.DeptName;
            string? completedOn = latestTask?.CompletedOn?.ToString("dd-MMM-yyyy HH:mm");

            // Determine if this is the current step
            bool isCurrent = latestTask != null &&
                             (latestTask.TaskStatus == "PENDING" || latestTask.TaskStatus == "IN_PROGRESS");

            if (isCurrent && currentIndex == -1)
                currentIndex = i;

            steps.Add(new
            {
                index = i,
                processId = proc.ProcessId,
                processCode = proc.ProcessCode,
                processName = proc.ProcessName,
                eventLabel = proc.ProcessName,
                eventTypeCode = proc.IsApprovalRequired == true ? WkEventTypeCode.ProcApproval : WkEventTypeCode.ProcStart,
                sequenceNo = proc.SequenceNo,
                priority = latestTask?.Priority ?? WkPriority.Normal,
                stepStatus,
                assignedUserName,
                assignedUserId,
                departmentName,
                completedOn,
                isCurrent,
                isPrevious = currentIndex >= 0 && i == currentIndex - 1,
                isNext = currentIndex >= 0 && i == currentIndex + 1,
                taskId = latestTask?.WorkspaceTaskId,
                taskType = latestTask?.TaskType
            });
        }

        // If no active step found, find the last completed one and mark next as current
        if (currentIndex == -1)
        {
            var lastCompleted = steps.Cast<dynamic>()
                .Select((s, idx) => new { s, idx })
                .LastOrDefault(x => x.s.stepStatus == WkTaskStatus.Completed || x.s.stepStatus == WkTaskStatus.Approved);

            if (lastCompleted != null)
                currentIndex = lastCompleted.idx + 1;
        }

        return Ok(new { steps, currentIndex, jobId = task.JobId, processId = task.ProcessId });
    }

    // ── Process Flow for Job (all processes) ──
    [HttpGet("process-flow/job/{jobId}")]
    public async Task<IActionResult> GetJobProcessFlow(long jobId)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized(new { message = "Session expired." });

        // Get all workspace tasks for this job
        var jobTasks = await _db.TrnWorkspaceTasks
            .Include(t => t.User)
            .Include(t => t.Department)
            .Include(t => t.Process)
            .Where(t => t.JobId == jobId && !t.IsArchived)
            .OrderBy(t => t.ProcessId)
            .ToListAsync();

        var currentTask = jobTasks
            .FirstOrDefault(t => t.TaskStatus == "PENDING" || t.TaskStatus == "IN_PROGRESS");

        var previousTask = jobTasks
            .Where(t => t.TaskStatus == "COMPLETED" || t.TaskStatus == "APPROVED")
            .OrderByDescending(t => t.CompletedOn ?? t.ModifiedOn ?? t.CreatedOn)
            .FirstOrDefault();

        var orderedFlow = await ResolveJobProcessStepsAsync(jobId);
        ProcessFlowStepMeta? placeholderCurrentStep = null;

        // Find next process step from resolved job flow (template-aware)
        ProcessFlowStepMeta? nextProcess = null;
        if (!string.IsNullOrEmpty(currentTask?.ProcessCode))
        {
            var currentIndex = orderedFlow.FindIndex(p => p.ProcessCode == currentTask.ProcessCode);

            if (currentIndex >= 0 && currentIndex + 1 < orderedFlow.Count)
            {
                nextProcess = orderedFlow[currentIndex + 1];
            }
            else
            {
                var currentProcess = await _db.MstProcesses
                    .FirstOrDefaultAsync(p => p.Processcode == currentTask.ProcessCode && p.Isactive);

                if (currentProcess != null)
                {
                    var fallbackNext = await _db.MstProcesses
                    .Include(p => p.Department)
                    .Where(p => p.Isactive &&
                                p.Sequenceno > currentProcess.Sequenceno &&
                                !DisabledProcessCodes.Contains(p.Processcode) &&
                                !(p.Processcode.StartsWith("QUOT") && p.Processcode.Contains("APPR")))
                    .OrderBy(p => p.Sequenceno)
                    .FirstOrDefaultAsync();

                    if (fallbackNext != null)
                    {
                        nextProcess = new ProcessFlowStepMeta
                        {
                            ProcessId = fallbackNext.Processid,
                            ProcessCode = fallbackNext.Processcode,
                            ProcessName = fallbackNext.Processname,
                            SequenceNo = fallbackNext.Sequenceno,
                            DepartmentId = fallbackNext.Departmentid,
                            DepartmentName = fallbackNext.Department?.DeptName,
                            IsApprovalRequired = fallbackNext.Isapprovalrequired,
                            IsMandatory = fallbackNext.Ismandatory
                        };
                    }
                }
            }
        }
        else if (orderedFlow.Count > 0)
        {
            // No active workspace task yet: derive placeholder current and next from process flow
            // FIX: Start search from AFTER the last completed task position, not from the beginning
            var latestByProcess = jobTasks
                .Where(t => !string.IsNullOrWhiteSpace(t.ProcessCode))
                .GroupBy(t => t.ProcessCode!)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(x => x.CreatedOn).First());

            // Find the highest sequence position of any completed/approved task
            int lastCompletedIndex = -1;
            for (int i = 0; i < orderedFlow.Count; i++)
            {
                if (latestByProcess.TryGetValue(orderedFlow[i].ProcessCode, out var task) &&
                    (task.TaskStatus == WkTaskStatus.Completed || task.TaskStatus == WkTaskStatus.Approved))
                {
                    lastCompletedIndex = i;
                }
            }

            // Search for current step starting AFTER the last completed step
            int searchStartIndex = lastCompletedIndex >= 0 ? lastCompletedIndex + 1 : 0;
            placeholderCurrentStep = orderedFlow
                .Skip(searchStartIndex)
                .FirstOrDefault(p =>
                {
                    if (!latestByProcess.TryGetValue(p.ProcessCode, out var latest)) return true;
                    return latest.TaskStatus != WkTaskStatus.Completed && latest.TaskStatus != WkTaskStatus.Approved;
                });

            // Fallback: if no step found after last completed, use the step right after last completed
            if (placeholderCurrentStep == null && lastCompletedIndex >= 0 && lastCompletedIndex + 1 < orderedFlow.Count)
            {
                placeholderCurrentStep = orderedFlow[lastCompletedIndex + 1];
            }

            if (placeholderCurrentStep != null)
            {
                var idx = orderedFlow.FindIndex(p => p.ProcessCode == placeholderCurrentStep.ProcessCode);
                if (idx >= 0 && idx + 1 < orderedFlow.Count)
                    nextProcess = orderedFlow[idx + 1];
            }
        }

        object? currentCard;
        if (currentTask != null)
        {
            currentCard = new
            {
                taskId = currentTask.WorkspaceTaskId,
                title = currentTask.Title,
                status = currentTask.TaskStatus,
                taskType = currentTask.TaskType,
                assignedTo = currentTask.User?.Name,
                department = currentTask.Department?.DeptName,
                process = currentTask.Process?.Processname,
                priority = currentTask.Priority,
                dueDate = currentTask.DueDate?.ToString("dd-MMM-yyyy HH:mm"),
                isOverdue = currentTask.IsOverdue
            };
        }
        else if (placeholderCurrentStep != null)
        {
            currentCard = new
            {
                taskId = (long?)null,
                title = $"{placeholderCurrentStep.ProcessName} — Awaiting assignment",
                status = WkTaskStatus.Pending,
                taskType = WkTaskType.Task,
                assignedTo = "-",
                department = placeholderCurrentStep.DepartmentName,
                process = placeholderCurrentStep.ProcessName,
                priority = WkPriority.Normal,
                dueDate = (string?)null,
                isOverdue = false
            };
        }
        else
        {
            currentCard = null;
        }

        return Ok(new
        {
            jobId,
            previous = previousTask != null ? new
            {
                taskId = previousTask.WorkspaceTaskId,
                title = previousTask.Title,
                status = previousTask.TaskStatus,
                assignedTo = previousTask.User?.Name,
                department = previousTask.Department?.DeptName,
                completedOn = previousTask.CompletedOn?.ToString("dd-MMM-yyyy HH:mm")
            } : null,
            current = currentCard,
            next = nextProcess != null ? new
            {
                label = nextProcess.ProcessName,
                department = nextProcess.DepartmentName,
                eventType = nextProcess.IsApprovalRequired == true ? "PROC_APPROVAL" : "PROC_START"
            } : null,
            totalSteps = jobTasks.Count,
            completedSteps = jobTasks.Count(t => t.TaskStatus == "COMPLETED" || t.TaskStatus == "APPROVED")
        });
    }

    // ── Full Pipeline Visualization (Modern UI) ──
    [HttpGet("pipeline/job/{jobId}")]
    public async Task<IActionResult> GetJobPipeline(long jobId)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized(new { message = "Session expired." });

        // Get ordered process flow (template-aware by job type/product type)
        var allProcesses = await ResolveJobProcessStepsAsync(jobId);

        if (allProcesses.Count == 0)
            return Ok(new { steps = Array.Empty<object>(), currentIndex = -1, totalSteps = 0, completedSteps = 0 });

        // Get all workspace tasks for this job to determine step statuses
        var jobTasks = await _db.TrnWorkspaceTasks
            .Include(t => t.User)
            .Include(t => t.Department)
            .Where(t => t.JobId == jobId && !t.IsArchived)
            .ToListAsync();

        // Determine if this is a multi-item job (per-item batches generated after JOB_APPROVAL)
        var itemCount = await _db.TrnJobItems.CountAsync(ji => ji.JobId == jobId);
        var sharedProcessCodes = new HashSet<string>(
            erp.minepress.domain.Enums.WkProcessCode.PreJobProcesses
                .Concat([erp.minepress.domain.Enums.WkProcessCode.JobApproval]),
            StringComparer.OrdinalIgnoreCase);

        var steps = new List<object>();
        int currentIndex = -1;
        int completedCount = 0;

        // Process icon mapping based on process codes
        var processIcons = new Dictionary<string, string>
        {
            ["ADV_PAY"] = "bi-cash-coin",
            ["ENQ_JOB"] = "bi-chat-left-text",
            ["ENQ_EST"] = "bi-calculator",
            ["QUOT"] = "bi-file-earmark-text",
            ["QUOT_APPR"] = "bi-file-check",
            ["JOB_CREATE"] = "bi-briefcase",
            ["JOB_APPROVAL"] = "bi-person-check",
            ["DES_DTP"] = "bi-palette",
            ["PROOF"] = "bi-eye",
            ["PRE_PRESS"] = "bi-layers",
            ["PROC"] = "bi-cart3",
            ["GRN"] = "bi-box-seam",
            ["QC_IN"] = "bi-clipboard-check",
            ["STORE_ISSUE"] = "bi-box-arrow-right",
            ["JOB_PLAN"] = "bi-calendar3",
            ["JOB_SCHED"] = "bi-clock",
            ["JOB_CARD"] = "bi-card-heading",
            ["CUT"] = "bi-scissors",
            ["PRINT"] = "bi-printer",
            ["QC_PROC"] = "bi-shield-check",
            ["DRY"] = "bi-sun",
            ["POST_PRESS"] = "bi-stack",
            ["FOLD"] = "bi-journal-bookmark",
            ["BIND"] = "bi-book",
            ["TRIM"] = "bi-crop",
            ["QC_POST"] = "bi-patch-check",
            ["PACK"] = "bi-box",
            ["LOAD"] = "bi-truck",
            ["CHALLAN"] = "bi-receipt",
            ["GATE_PASS"] = "bi-door-open",
            ["DISPATCH"] = "bi-send",
            ["DELIVERY_CONF"] = "bi-check2-circle",
            ["BILL"] = "bi-currency-rupee",
            ["PAY_REC"] = "bi-wallet2",
            ["CREDIT_NOTE"] = "bi-file-minus",
            ["DEBIT_NOTE"] = "bi-file-plus",
            ["STORE_RETURN"] = "bi-box-arrow-in-left",
            ["WASTE_ENTRY"] = "bi-trash3",
            ["COST_FINAL"] = "bi-graph-up",
            ["PROFIT_ANALYSIS"] = "bi-pie-chart",
            ["JOB_CLOSE"] = "bi-check-circle",
            ["JOB_ARCHIVE"] = "bi-archive"
        };

        // Status colors for the pipeline
        var statusColors = new Dictionary<string, string>
        {
            ["COMPLETED"] = "#10b981",      // Emerald green
            ["APPROVED"] = "#059669",       // Dark green
            ["IN_PROGRESS"] = "#f59e0b",    // Amber
            ["PENDING"] = "#3b82f6",        // Blue
            ["QUEUED"] = "#94a3b8",         // Slate gray
            ["REJECTED"] = "#ef4444",       // Red
            ["CANCELLED"] = "#6b7280",      // Gray
            ["NOT_STARTED"] = "#e2e8f0"     // Light slate
        };

        for (int i = 0; i < allProcesses.Count; i++)
        {
            var proc = allProcesses[i];

            // Find matching workspace task(s) for this process step
            var matchingTasks = jobTasks
                .Where(t => t.ProcessCode == proc.ProcessCode)
                .OrderByDescending(t => t.CreatedOn)
                .ToList();

            // For multi-item jobs, production steps have one task per item — aggregate their statuses
            bool isPerItemStep = itemCount > 1 && !sharedProcessCodes.Contains(proc.ProcessCode ?? "");
            TrnWorkspaceTask? latestTask;
            string stepStatus;

            if (isPerItemStep && matchingTasks.Count > 0)
            {
                // Exclude cancelled tasks for status determination (cancelled = superseded shared tasks)
                var activeTasks = matchingTasks.Where(t => t.TaskStatus != "CANCELLED").ToList();
                if (activeTasks.Count == 0)
                {
                    stepStatus = "NOT_STARTED";
                    latestTask = null;
                }
                else if (activeTasks.All(t => t.TaskStatus == "COMPLETED" || t.TaskStatus == "APPROVED"))
                {
                    stepStatus = "COMPLETED";
                    latestTask = activeTasks.First();
                }
                else if (activeTasks.Any(t => t.TaskStatus == "IN_PROGRESS"))
                {
                    stepStatus = "IN_PROGRESS";
                    latestTask = activeTasks.First(t => t.TaskStatus == "IN_PROGRESS");
                }
                else if (activeTasks.Any(t => t.TaskStatus == "PENDING"))
                {
                    stepStatus = "PENDING";
                    latestTask = activeTasks.First(t => t.TaskStatus == "PENDING");
                }
                else if (activeTasks.All(t => t.TaskStatus == "QUEUED"))
                {
                    stepStatus = "QUEUED";
                    latestTask = activeTasks.First();
                }
                else
                {
                    stepStatus = activeTasks.First().TaskStatus ?? "NOT_STARTED";
                    latestTask = activeTasks.First();
                }
            }
            else
            {
                latestTask = matchingTasks.FirstOrDefault();
                stepStatus = latestTask?.TaskStatus ?? "NOT_STARTED";
            }

            string? assignedUserName = latestTask?.User?.Name;
            string? departmentName = latestTask?.Department?.DeptName ?? proc.DepartmentName;
            string? completedOn = latestTask?.CompletedOn?.ToString("dd-MMM-yyyy HH:mm");
            string? dueDate = latestTask?.DueDate?.ToString("dd-MMM-yyyy HH:mm");

            // Determine if this is the current step
            bool isCurrent = latestTask != null &&
                             (latestTask.TaskStatus == "PENDING" || latestTask.TaskStatus == "IN_PROGRESS");

            if (isCurrent && currentIndex == -1)
                currentIndex = i;

            if (stepStatus == "COMPLETED" || stepStatus == "APPROVED")
                completedCount++;

            // Get icon for process
            string icon = processIcons.GetValueOrDefault(proc.ProcessCode ?? "", "bi-circle");
            string color = statusColors.GetValueOrDefault(stepStatus, "#e2e8f0");

            // Build per-item breakdown for multi-item production steps
            object[]? itemBreakdown = null;
            if (isPerItemStep && matchingTasks.Count > 0)
            {
                itemBreakdown = matchingTasks
                    .Where(t => t.TaskStatus != "CANCELLED")
                    .Select(t =>
                    {
                        // Extract item name from title prefix "[ItemName] ..." or metadata
                        string itemLabel = t.Title ?? "";
                        if (itemLabel.StartsWith("[") && itemLabel.Contains("]"))
                            itemLabel = itemLabel[1..itemLabel.IndexOf(']')];
                        return (object)new
                        {
                            taskId = t.WorkspaceTaskId,
                            itemLabel,
                            status = t.TaskStatus,
                            statusLabel = GetStatusLabel(t.TaskStatus ?? "NOT_STARTED"),
                            assignedTo = t.User?.Name,
                            completedOn = t.CompletedOn?.ToString("dd-MMM-yyyy HH:mm")
                        };
                    })
                    .ToArray();
            }

            steps.Add(new
            {
                index = i,
                sequenceNo = i + 1,
                processId = proc.ProcessId,
                processCode = proc.ProcessCode,
                processName = proc.ProcessName,
                shortName = GetShortProcessName(proc.ProcessName ?? ""),
                icon,
                color,
                status = stepStatus,
                statusLabel = GetStatusLabel(stepStatus),
                assignedTo = assignedUserName,
                department = departmentName,
                completedOn,
                dueDate,
                isCurrent,
                isCompleted = stepStatus == "COMPLETED" || stepStatus == "APPROVED",
                isPending = stepStatus == "PENDING",
                isInProgress = stepStatus == "IN_PROGRESS",
                isWaiting = stepStatus == "NOT_STARTED" || stepStatus == "QUEUED",
                isBlocking = proc.IsMandatory ?? true,
                taskId = latestTask?.WorkspaceTaskId,
                isOverdue = latestTask?.IsOverdue ?? false,
                isPerItemStep,
                itemTaskCount = isPerItemStep ? matchingTasks.Count(t => t.TaskStatus != "CANCELLED") : 0,
                itemBreakdown
            });
        }

        // If no active step found, find the last completed one and mark next as current
        if (currentIndex == -1)
        {
            var lastCompletedIdx = -1;
            for (int i = 0; i < steps.Count; i++)
            {
                var step = (dynamic)steps[i];
                if (step.isCompleted)
                    lastCompletedIdx = i;
            }
            if (lastCompletedIdx >= 0 && lastCompletedIdx + 1 < steps.Count)
                currentIndex = lastCompletedIdx + 1;
        }

        // Calculate progress percentage
        int progressPct = allProcesses.Count > 0 ? (int)Math.Round((double)completedCount / allProcesses.Count * 100) : 0;

        return Ok(new
        {
            jobId,
            steps,
            currentIndex,
            totalSteps = allProcesses.Count,
            completedSteps = completedCount,
            progressPct,
            phases = GetWorkflowPhases(steps)
        });
    }

    private static string GetShortProcessName(string processName)
    {
        if (string.IsNullOrWhiteSpace(processName)) return "";
        var words = processName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 1) return words[0].Length > 8 ? words[0][..8] : words[0];
        return string.Join(" ", words.Take(2));
    }

    private static string GetStatusLabel(string status) => status switch
    {
        "COMPLETED" => "Completed",
        "APPROVED" => "Approved",
        "IN_PROGRESS" => "In Progress",
        "PENDING" => "Pending",
        "QUEUED" => "Queued",
        "REJECTED" => "Rejected",
        "CANCELLED" => "Cancelled",
        "NOT_STARTED" => "Not Started",
        _ => status
    };

    private static object[] GetWorkflowPhases(List<object> steps)
    {
        // Group steps into workflow phases for visual grouping
        var phases = new[]
        {
            new { name = "Pre-Sales", codes = new[] { "ADV_PAY", "ENQ_JOB", "ENQ_EST", "QUOT", "QUOT_APPR" } },
            new { name = "Job Setup", codes = new[] { "JOB_CREATE", "JOB_APPROVAL", "DES_DTP", "PROOF", "PRE_PRESS" } },
            new { name = "Procurement", codes = new[] { "PROC", "GRN", "QC_IN", "STORE_ISSUE" } },
            new { name = "Production", codes = new[] { "JOB_PLAN", "JOB_SCHED", "JOB_CARD", "CUT", "PRINT", "QC_PROC", "DRY" } },
            new { name = "Finishing", codes = new[] { "POST_PRESS", "FOLD", "BIND", "TRIM", "QC_POST" } },
            new { name = "Dispatch", codes = new[] { "PACK", "LOAD", "CHALLAN", "GATE_PASS", "DISPATCH", "DELIVERY_CONF" } },
            new { name = "Finance", codes = new[] { "BILL", "PAY_REC", "CREDIT_NOTE", "DEBIT_NOTE" } },
            new { name = "Closure", codes = new[] { "STORE_RETURN", "WASTE_ENTRY", "COST_FINAL", "PROFIT_ANALYSIS", "JOB_CLOSE", "JOB_ARCHIVE" } }
        };

        return phases.Select(p =>
        {
            var phaseSteps = steps.Cast<dynamic>().Where(s => p.codes.Contains((string)s.processCode)).ToList();
            var completed = phaseSteps.Count(s => s.isCompleted);
            var total = phaseSteps.Count;
            return new
            {
                p.name,
                stepCount = total,
                completedCount = completed,
                progressPct = total > 0 ? (int)Math.Round((double)completed / total * 100) : 0,
                hasCurrentStep = phaseSteps.Any(s => s.isCurrent),
                stepIndices = phaseSteps.Select(s => (int)s.index).ToArray()
            };
        }).Where(p => p.stepCount > 0).ToArray();
    }

    // ── Task Actions ──
    [HttpPost("task/{id}/start")]
    public async Task<IActionResult> StartTask(long id)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized(new { message = "Session expired." });

        var task = await _db.TrnWorkspaceTasks.FindAsync(id);
        if (task == null) return NotFound(new { message = "Task not found." });
        if (task.UserId != user.UserId) return StatusCode(StatusCodes.Status403Forbidden, new { message = "Access denied." });

        var processCode = (task.ProcessCode ?? string.Empty).ToUpperInvariant();

        var isPlateMakingTask = processCode == "PRE_PRESS" || processCode == "PRE_CTP"
            || processCode.Contains("CTP") || processCode.Contains("PLATE");
        if (isPlateMakingTask)
        {
            var jobId = task.JobId ?? (task.SourceTable == WkSourceTable.Job ? task.SourceId : (long?)null);
            if (jobId.HasValue)
            {
                var hasPlateIssued = await _db.TrnStoreIssueItems
                    .AnyAsync(i => i.Issue.JobId == jobId.Value
                                   && i.Issue.Status != "CANCELLED"
                                   && i.IsSelected == true
                                   && i.MaterialCategory == "PLATE");

                if (!hasPlateIssued)
                    return BadRequest(new { message = "Cannot start Plate Making: no plate has been issued against this job from the store. Please issue plates before starting this process." });
            }
        }

        if (processCode == "CUT")
        {
            var jobId = task.JobId ?? (task.SourceTable == WkSourceTable.Job ? task.SourceId : (long?)null);
            if (jobId.HasValue)
            {
                var hasPaperIssued = await _db.TrnStoreIssueItems
                    .AnyAsync(i => i.Issue.JobId == jobId.Value
                                   && i.Issue.Status != "CANCELLED"
                                   && i.IsSelected == true
                                   && i.MaterialCategory == "PAPER");

                if (!hasPaperIssued)
                    return BadRequest(new { message = "Cannot start Cutting: no paper has been issued against this job from the store. Please issue paper from the store before starting this process." });
            }
        }

        var isPrintingTask = processCode == WkProcessCode.Print || processCode == "PROC";
        if (isPrintingTask)
        {
            var jobId = task.JobId ?? (task.SourceTable == WkSourceTable.Job ? task.SourceId : (long?)null);
            if (!jobId.HasValue)
            {
                return BadRequest(new { message = "No machine is allotted for this job. Check workforce allocation for this machine." });
            }

            var allocatedMachineIds = await _db.TrnJobMachineAllocations
                .Where(a => a.IsActive == true
                            && a.JobId == jobId.Value
                            && a.AllocationStatus == "ALLOCATED")
                .Select(a => a.MachineId)
                .Distinct()
                .ToListAsync();

            var hasMachineAllotment = allocatedMachineIds.Count > 0;
            var hasWorkforceAllotment = hasMachineAllotment && await _db.TrnJobMachineManpowerAllocations
                .AnyAsync(mp => mp.IsActive == true
                                && mp.JobId == jobId.Value
                                && mp.AllocationStatus == "ASSIGNED"
                                && allocatedMachineIds.Contains(mp.MachineId));

            if (!hasMachineAllotment || !hasWorkforceAllotment)
            {
                return BadRequest(new { message = "No machine is allotted for this job. Check workforce allocation for this machine." });
            }
        }

        var oldStatus = task.TaskStatus;
        task.TaskStatus = WkTaskStatus.InProgress;
        task.ModifiedOn = DateTime.Now;
        await _db.SaveChangesAsync();

        // ── Cross-cutting: Activity + Notification + Timeline ──
        await LogWorkspaceActivityAsync(user, task, WkEventTypeCode.TaskStart, $"Started task: {task.Title}");
        await LogWorkspaceInAppNotificationAsync(user, task, WkEventTypeCode.TaskStarted, "Task Started", $"Task '{task.Title}' has been started.");
        await AddWorkspaceJobTimelineAsync(task, WkEventTypeCode.TaskStarted, "Task Started", $"Task '{task.Title}' started by {user.Name}.", oldStatus, WkTaskStatus.InProgress, user.UserId);
        await DispatchWorkspaceNotificationAsync(task, user, WkTaskStatus.InProgress, "Task Started");

        return Ok(new { message = "Task started." });
    }

    [HttpPost("task/{id}/complete")]
    public async Task<IActionResult> CompleteTask(long id, [FromBody] WorkNoteRequest? request = null)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized(new { message = "Session expired." });

        var task = await _db.TrnWorkspaceTasks.FindAsync(id);
        if (task == null) return NotFound(new { message = "Task not found." });
        if (task.UserId != user.UserId) return StatusCode(StatusCodes.Status403Forbidden, new { message = "Access denied." });

        if (task.TaskStatus != WkTaskStatus.InProgress)
            return BadRequest(new { message = "This process has not been started yet. Please start the process before marking it as complete." });

        // ── Finalise design progress rows ────────────────────────────────
        if (request?.DesignProgress != null && request.DesignProgress.Count > 0)
        {
            var now = DateTime.Now;
            var seq = 1;
            foreach (var item in request.DesignProgress)
            {
                if (string.IsNullOrWhiteSpace(item.Activity)) { seq++; continue; }

                if (item.DesignWorkId.HasValue && item.DesignWorkId > 0)
                {
                    var existing = await _db.TrnDesignWorkEntries.FindAsync(item.DesignWorkId.Value);
                    if (existing != null && existing.WorkspaceTaskId == id)
                    {
                        existing.ActivitySequence = seq;
                        existing.PagesRequired    = item.Required;
                        existing.PagesCompleted   = item.Completed;
                        existing.IsCompleted      = true;
                        existing.CompletedOn      = now;
                        existing.Notes            = request.Remarks;
                        existing.ModifiedBy       = user.UserId;
                        existing.ModifiedOn       = now;
                    }
                }
                else
                {
                    _db.TrnDesignWorkEntries.Add(new TrnDesignWorkEntry
                    {
                        WorkspaceTaskId  = id,
                        JobId            = task.JobId,
                        ActivityName     = item.Activity,
                        ActivitySequence = seq,
                        PagesRequired    = item.Required,
                        PagesCompleted   = item.Completed,
                        IsCompleted      = true,
                        CompletedOn      = now,
                        Notes            = request.Remarks,
                        CreatedBy        = user.UserId,
                        CreatedOn        = now
                    });
                }
                seq++;
            }
        }

        var oldStatus = task.TaskStatus;
        task.TaskStatus = WkTaskStatus.Completed;
        task.CompletedBy = user.UserId;
        task.CompletedOn = DateTime.Now;
        task.CompletionRemarks = request?.Remarks;
        task.ModifiedOn = DateTime.Now;
        await _db.SaveChangesAsync();

        // ── Cross-cutting: Activity + Notification + Timeline ──
        await LogWorkspaceActivityAsync(user, task, WkEventTypeCode.TaskComplete, $"Completed task: {task.Title}");
        await LogWorkspaceInAppNotificationAsync(user, task, WkEventTypeCode.TaskCompleted, "Task Completed", $"Task '{task.Title}' has been completed.");
        await AddWorkspaceJobTimelineAsync(task, WkEventTypeCode.TaskCompleted, "Task Completed", $"Task '{task.Title}' completed by {user.Name}. Remarks: {request?.Remarks ?? "—"}", oldStatus, WkTaskStatus.Completed, user.UserId);
        await DispatchWorkspaceNotificationAsync(task, user, WkTaskStatus.Completed, "Task Completed");
        await LogPartyActivityForTaskAsync(task, WkEventTypeCode.TaskCompleted, $"Task '{task.Title}' completed", user.Name);

        // ── Generate next step tasks ──
        await _workspaceEngine.GenerateNextStepTasksAsync(task, user);

        return Ok(new { message = "Task completed." });
    }

    // ── Item-Level Parallel Task Endpoints ──

    [HttpGet("task/{id}/item-tasks")]
    public async Task<IActionResult> GetItemTasks(long id)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized(new { message = "Session expired." });

        var items = await _itemTaskService.GetItemTasksAsync(id);
        return Ok(items.Select(i => new
        {
            i.TaskItemId,
            i.WorkspaceTaskId,
            i.JobId,
            i.JobItemId,
            i.ProcessCode,
            i.ProcessName,
            i.ItemName,
            i.ItemDescription,
            i.ItemSequence,
            i.TaskStatus,
            i.AssignedUserId,
            i.StartedOn,
            i.CompletedOn,
            i.Remarks,
            i.WorkData,
            i.ParentTaskItemId
        }));
    }

    [HttpPost("task/{id}/create-item-tasks")]
    public async Task<IActionResult> CreateItemTasks(long id)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized(new { message = "Session expired." });

        var task = await _db.TrnWorkspaceTasks.FindAsync(id);
        if (task == null) return NotFound(new { message = "Task not found." });

        await _itemTaskService.CreateItemTasksAsync(task);
        return Ok(new { message = "Item tasks created." });
    }

    [HttpPost("item-task/{itemId}/start")]
    public async Task<IActionResult> StartItemTask(long itemId)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized(new { message = "Session expired." });

        await _itemTaskService.StartItemTaskAsync(itemId, user);
        return Ok(new { message = "Item task started." });
    }

    [HttpPost("item-task/{itemId}/complete")]
    public async Task<IActionResult> CompleteItemTask(long itemId, [FromBody] TaskActionRequest? request = null)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized(new { message = "Session expired." });

        await _itemTaskService.CompleteItemTaskAsync(itemId, user, request?.Remarks);
        return Ok(new { message = "Item task completed." });
    }

    [HttpGet("task/{id}/process-input-options")]
    public async Task<IActionResult> GetProcessInputOptions(long id)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized(new { message = "Session expired." });

        var task = await _db.TrnWorkspaceTasks.FindAsync(id);
        if (task == null) return NotFound(new { message = "Task not found." });
        if (task.UserId != user.UserId) return StatusCode(StatusCodes.Status403Forbidden, new { message = "Access denied." });

        var code = (task.ProcessCode ?? string.Empty).ToUpperInvariant();
        var title = (task.Title ?? string.Empty).ToUpperInvariant();

        var designCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "DES_DTP", "PRE_DES", "DES_ART", "PROOF"
        };

        var plateCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "PRE_PRESS", "PRE_CTP"
        };

        var bindingCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "BIND"
        };

        var finishingCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "POST_PRESS", "FOLD", "TRIM", "QC_POST"
        };

        if (designCodes.Contains(code) || code.Contains("DES") || code.Contains("DTP") || title.Contains("DESIGN") || title.Contains("DTP"))
        {
            var options = await _db.MstDesignings
                .Where(x => x.IsActive == true)
                .OrderBy(x => x.DesignName)
                .Select(x => new { id = x.DesigningId, code = x.DesignCode, name = x.DesignName })
                .ToListAsync();

            return Ok(new { processType = "DESIGN_DTP", label = "Designing / DTP Inputs", options });
        }

        if (plateCodes.Contains(code) || code.Contains("CTP") || code.Contains("PLATE") || title.Contains("PLATE") || title.Contains("PRE PRESS") || title.Contains("PRE-PRESS"))
        {
            var options = await _db.MstPlates
                .Where(x => x.IsActive == true)
                .OrderBy(x => x.PlateName)
                .Select(x => new { id = x.PlateId, code = x.PlateCode, name = x.PlateName })
                .ToListAsync();

            return Ok(new { processType = "PLATE_MAKING", label = "Plate Making Inputs", options });
        }

        if (bindingCodes.Contains(code) || code.Contains("BIND") || title.Contains("BIND"))
        {
            var options = await _db.MstBindings
                .Where(x => x.IsActive == true)
                .OrderBy(x => x.BindingName)
                .Select(x => new { id = x.BindingId, code = x.BindingCode, name = x.BindingName })
                .ToListAsync();

            return Ok(new { processType = "BINDING", label = "Binding Inputs", options });
        }

        if (finishingCodes.Contains(code) || code.Contains("FINISH") || code.Contains("POST_PRESS") || title.Contains("FINISH") || title.Contains("POST PRESS") || title.Contains("POST-PRESS") || title.Contains("FOLD") || title.Contains("TRIM"))
        {
            var options = await _db.MstFinishings
                .Where(x => x.IsActive == true)
                .OrderBy(x => x.FinishingName)
                .Select(x => new { id = x.FinishingId, code = x.FinishingCode, name = x.FinishingName })
                .ToListAsync();

            return Ok(new { processType = "FINISHING", label = "Finishing Inputs", options });
        }

        return Ok(new { processType = "DEFAULT", label = "Process Inputs", options = Array.Empty<object>() });
    }

    [HttpPost("task/{id}/work-note")]
    public async Task<IActionResult> SaveWorkNote(long id, [FromBody] WorkNoteRequest? request = null)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized(new { message = "Session expired." });

        var task = await _db.TrnWorkspaceTasks.FindAsync(id);
        if (task == null) return NotFound(new { message = "Task not found." });
        if (task.UserId != user.UserId) return StatusCode(StatusCodes.Status403Forbidden, new { message = "Access denied." });

        if (task.TaskStatus != WkTaskStatus.Pending && task.TaskStatus != WkTaskStatus.InProgress)
            return BadRequest(new { message = "Only pending/in-progress tasks can be updated." });

        task.CompletionRemarks = string.IsNullOrWhiteSpace(request?.Remarks)
            ? task.CompletionRemarks
            : request?.Remarks;

        task.Metadata = JsonSerializer.Serialize(new
        {
            process_code = task.ProcessCode,
            work_notes = request?.Remarks,
            part_ids = request?.PartIds ?? [],
            process_input_ids = request?.ProcessInputIds ?? [],
            checks_completed = request?.ChecksCompleted ?? 0,
            saved_by = user.UserId,
            saved_on = DateTime.Now
        });

        task.ModifiedOn = DateTime.Now;

        // ── Upsert design progress rows ──────────────────────────────────
        if (request?.DesignProgress != null && request.DesignProgress.Count > 0)
        {
            var now = DateTime.Now;
            var seq = 1;
            foreach (var item in request.DesignProgress)
            {
                if (string.IsNullOrWhiteSpace(item.Activity)) { seq++; continue; }

                if (item.DesignWorkId.HasValue && item.DesignWorkId > 0)
                {
                    var existing = await _db.TrnDesignWorkEntries.FindAsync(item.DesignWorkId.Value);
                    if (existing != null && existing.WorkspaceTaskId == id)
                    {
                        existing.ActivityName     = item.Activity;
                        existing.ActivitySequence = seq;
                        existing.PagesRequired    = item.Required;
                        existing.PagesCompleted   = item.Completed;
                        existing.Notes            = request.Remarks;
                        existing.ModifiedBy       = user.UserId;
                        existing.ModifiedOn       = now;
                    }
                }
                else
                {
                    _db.TrnDesignWorkEntries.Add(new TrnDesignWorkEntry
                    {
                        WorkspaceTaskId   = id,
                        JobId             = task.JobId,
                        ActivityName      = item.Activity,
                        ActivitySequence  = seq,
                        PagesRequired     = item.Required,
                        PagesCompleted    = item.Completed,
                        IsCompleted       = false,
                        Notes             = request.Remarks,
                        CreatedBy         = user.UserId,
                        CreatedOn         = now
                    });
                }
                seq++;
            }
        }

        await _db.SaveChangesAsync();

        return Ok(new { message = "Work progress saved." });
    }

    [HttpPost("approval/{id}/approve")]
    public async Task<IActionResult> ApproveTask(long id, [FromBody] TaskActionRequest? request = null)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized(new { message = "Session expired." });

        var task = await _db.TrnWorkspaceTasks.FindAsync(id);
        if (task == null) return NotFound(new { message = "Approval not found." });
        if (task.UserId != user.UserId) return StatusCode(StatusCodes.Status403Forbidden, new { message = "Access denied." });

        var oldStatus = task.TaskStatus;
        task.TaskStatus = WkTaskStatus.Approved;
        task.CompletedBy = user.UserId;
        task.CompletedOn = DateTime.Now;
        task.CompletionRemarks = request?.Remarks;
        task.ModifiedOn = DateTime.Now;
        await _db.SaveChangesAsync();

        // ── Business Rule: first-step enquiry approval should submit enquiry ──
        await ApplySourceSubmissionOnApprovalAsync(task, user, request?.Remarks);

        // ── Business Rule: job approval should update job status to APPROVED ──
        await ApplyJobStatusUpdateOnApprovalAsync(task, user, request?.Remarks);

        // ── Cross-cutting: Activity + Notification + Timeline ──
        await LogWorkspaceActivityAsync(user, task, WkEventTypeCode.ApprovalApproved, $"Approved: {task.Title}");
        await LogWorkspaceInAppNotificationAsync(user, task, WkEventTypeCode.ApprovalApproved, "Approval Granted", $"'{task.Title}' has been approved.");
        await AddWorkspaceJobTimelineAsync(task, WkEventTypeCode.ApprovalApproved, "Approval Granted", $"'{task.Title}' approved by {user.Name}. Remarks: {request?.Remarks ?? "—"}", oldStatus, WkTaskStatus.Approved, user.UserId);
        await DispatchWorkspaceNotificationAsync(task, user, WkTaskStatus.Approved, "Approval Granted");
        await LogPartyActivityForTaskAsync(task, WkEventTypeCode.ApprovalApproved, $"'{task.Title}' approved", user.Name);

        // ── Generate next step tasks ──
        if (!ShouldSkipNextStepGenerationOnApproval(task))
        {
            await _workspaceEngine.GenerateNextStepTasksAsync(task, user);
        }

        return Ok(new { message = "Approved successfully." });
    }

    [HttpPost("approval/{id}/reject")]
    public async Task<IActionResult> RejectTask(long id, [FromBody] TaskActionRequest? request = null)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized(new { message = "Session expired." });

        var task = await _db.TrnWorkspaceTasks.FindAsync(id);
        if (task == null) return NotFound(new { message = "Approval not found." });
        if (task.UserId != user.UserId) return StatusCode(StatusCodes.Status403Forbidden, new { message = "Access denied." });

        var oldStatus = task.TaskStatus;
        task.TaskStatus = WkTaskStatus.Rejected;
        task.CompletedBy = user.UserId;
        task.CompletedOn = DateTime.Now;
        task.CompletionRemarks = request?.Remarks;
        task.ModifiedOn = DateTime.Now;
        await _db.SaveChangesAsync();

        // ── Business Rule: specific approval rejections should cancel source document ──
        await ApplySourceCancellationOnRejectionAsync(task, user, request?.Remarks);

        // ── Cross-cutting: Activity + Notification + Timeline ──
        await LogWorkspaceActivityAsync(user, task, WkEventTypeCode.ApprovalRejected, $"Rejected: {task.Title}");
        await LogWorkspaceInAppNotificationAsync(user, task, WkEventTypeCode.ApprovalRejected, "Approval Rejected", $"'{task.Title}' has been rejected. Reason: {request?.Remarks ?? "—"}");
        await AddWorkspaceJobTimelineAsync(task, WkEventTypeCode.ApprovalRejected, "Approval Rejected", $"'{task.Title}' rejected by {user.Name}. Reason: {request?.Remarks ?? "—"}", oldStatus, WkTaskStatus.Rejected, user.UserId);
        await DispatchWorkspaceNotificationAsync(task, user, WkTaskStatus.Rejected, "Approval Rejected");
        await LogPartyActivityForTaskAsync(task, WkEventTypeCode.ApprovalRejected, $"'{task.Title}' rejected", user.Name);

        return Ok(new { message = "Rejected." });
    }

    // ── Mark Task as Read ──
    [HttpPost("task/{id}/read")]
    public async Task<IActionResult> MarkRead(long id)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized(new { message = "Session expired." });

        var task = await _db.TrnWorkspaceTasks.FindAsync(id);
        if (task == null) return NotFound();
        if (task.UserId != user.UserId) return StatusCode(StatusCodes.Status403Forbidden, new { message = "Access denied." });

        task.IsRead = true;
        task.ReadAt = DateTime.Now;
        task.ModifiedOn = DateTime.Now;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Marked as read." });
    }

    // ── AI Suggestions ──
    [HttpGet("ai-suggestions")]
    public async Task<IActionResult> GetAiSuggestions()
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized(new { message = "Session expired." });

        var userId = user.UserId;
        var departmentId = user.DepartmentId;

        // Get overdue tasks - show all for department OR assigned to user
        var overdueTasks = await _db.TrnWorkspaceTasks
            .Where(t => (t.DepartmentId == departmentId || t.UserId == userId) && t.IsOverdue && !t.IsArchived &&
                         t.TaskStatus != WkTaskStatus.Completed && t.TaskStatus != WkTaskStatus.Cancelled)
            .Select(t => new { t.Title, t.JobNo, t.Priority, t.DueDate, t.SlaHours })
            .Take(5)
            .ToListAsync();

        // Get high priority pending items - show all for department OR assigned to user
        var urgentItems = await _db.TrnWorkspaceTasks
            .Where(t => (t.DepartmentId == departmentId || t.UserId == userId) && !t.IsArchived &&
                        (t.Priority == WkPriority.Critical || t.Priority == WkPriority.Urgent) &&
                         t.TaskStatus == WkTaskStatus.Pending)
            .Select(t => new { t.Title, t.JobNo, t.Priority, t.DueDate, t.TaskType })
            .Take(5)
            .ToListAsync();

        // Get today's workload - show all for department OR assigned to user
        var todayDue = await _db.TrnWorkspaceTasks
            .CountAsync(t => (t.DepartmentId == departmentId || t.UserId == userId) && !t.IsArchived &&
                             t.DueDate.HasValue && t.DueDate.Value.Date == DateTime.Today &&
                             t.TaskStatus != WkTaskStatus.Completed);

        var suggestions = new List<object>();

        if (overdueTasks.Count > 0)
        {
            suggestions.Add(new
            {
                type = "OVERDUE_ALERT",
                icon = "bi-exclamation-triangle-fill",
                color = "danger",
                title = $"{overdueTasks.Count} Overdue Task(s)",
                message = $"You have {overdueTasks.Count} task(s) past SLA. Priority attention required for: {string.Join(", ", overdueTasks.Select(t => t.JobNo ?? t.Title).Take(3))}.",
                actionLabel = "View Overdue",
                actionUrl = "/Workspace/MyTasks?filter=overdue"
            });
        }

        if (urgentItems.Count > 0)
        {
            suggestions.Add(new
            {
                type = "PRIORITY_ALERT",
                icon = "bi-lightning-charge-fill",
                color = "warning",
                title = $"{urgentItems.Count} Urgent/Critical Item(s)",
                message = $"Focus on: {string.Join(", ", urgentItems.Select(t => t.Title).Take(3))}.",
                actionLabel = "View Urgent",
                actionUrl = "/Workspace/MyTasks?priority=urgent"
            });
        }

        if (todayDue > 3)
        {
            suggestions.Add(new
            {
                type = "WORKLOAD_INSIGHT",
                icon = "bi-bar-chart-line-fill",
                color = "info",
                title = "Heavy Workload Today",
                message = $"You have {todayDue} tasks due today. Consider delegating or prioritizing.",
                actionLabel = "View Today",
                actionUrl = "/Workspace/Calendar?view=daily"
            });
        }

        if (suggestions.Count == 0)
        {
            suggestions.Add(new
            {
                type = "ALL_CLEAR",
                icon = "bi-check-circle-fill",
                color = "success",
                title = "All Clear!",
                message = "No urgent items or overdue tasks. You're on track!",
                actionLabel = "View Tasks",
                actionUrl = "/Workspace/MyTasks"
            });
        }

        return Ok(suggestions);
    }

    // ── Task Machine & Workforce Allocation ──
    [HttpGet("task/{id}/allocation")]
    public async Task<IActionResult> GetTaskAllocation(long id)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized(new { message = "Session expired." });

        var task = await _db.TrnWorkspaceTasks.FindAsync(id);
        if (task == null) return NotFound(new { message = "Task not found." });

        var jobId = task.JobId ?? (task.SourceTable == WkSourceTable.Job ? task.SourceId : (long?)null);
        if (!jobId.HasValue)
            return Ok(new { machines = Array.Empty<object>() });

        var allocations = await _db.TrnJobMachineAllocations
            .Where(a => a.IsActive == true && a.JobId == jobId.Value && a.AllocationStatus == "ALLOCATED")
            .Include(a => a.TrnJobMachineManpowerAllocations.Where(mp => mp.IsActive == true))
            .OrderBy(a => a.MachineName)
            .Select(a => new
            {
                a.AllocationId,
                a.MachineId,
                a.MachineName,
                a.MachineCode,
                a.ProcessCode,
                a.ProcessName,
                a.PlannedQuantity,
                a.CompletedQuantity,
                Employees = a.TrnJobMachineManpowerAllocations.Select(mp => new
                {
                    mp.EmployeeName,
                    mp.RoleCode,
                    mp.ShiftCode,
                    mp.AllocationStatus
                })
            })
            .ToListAsync();

        return Ok(new { machines = allocations });
    }

    // ── Process Detail (enriched task context for DTP / Artwork / CTP / etc.) ──
    [HttpGet("task/{id}/process-detail")]
    public async Task<IActionResult> GetProcessDetail(long id)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized(new { message = "Session expired." });

        var task = await _db.TrnWorkspaceTasks
            .Include(t => t.Process)
            .Include(t => t.Department)
            .Include(t => t.AssignedByNavigation)
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.WorkspaceTaskId == id);

        if (task == null) return NotFound(new { message = "Task not found." });

        // ── Task base info ──
        var taskInfo = new
        {
            task.WorkspaceTaskId,
            task.Title,
            task.Description,
            task.TaskType,
            task.TaskStatus,
            task.ProcessCode,
            ProcessName = task.Process?.Processname,
            DepartmentName = task.Department?.DeptName,
            AssignedByName = task.AssignedByNavigation?.Name,
            AssignedToName = task.User?.Name,
            AssignedOn = task.AssignedOn?.ToString("dd-MMM-yyyy HH:mm"),
            task.Priority,
            DueDate = task.DueDate?.ToString("dd-MMM-yyyy HH:mm"),
            task.SlaHours,
            task.IsOverdue,
            CompletedOn = task.CompletedOn?.ToString("dd-MMM-yyyy HH:mm"),
            task.CompletionRemarks,
            task.ActionUrl,
            task.JobId,
            task.JobNo,
            task.PartyName,
            task.SourceTable,
            task.SourceId,
            task.SourceNo,
            CreatedOn = task.CreatedOn.ToString("dd-MMM-yyyy HH:mm")
        };

        // ── Load Job + Items + Rate Calculators (if linked) ──
        object? jobInfo = null;
        object? jobItems = null;
        object? rateCalculators = null;

        if (task.JobId.HasValue)
        {
            var job = await _db.TrnJobs
                .Include(j => j.Party)
                .Include(j => j.Company)
                .Include(j => j.JobType)
                .Include(j => j.JobCategory)
                .Include(j => j.TrnJobItems)
                    .ThenInclude(i => i.RateCalculator)
                .Include(j => j.TrnJobItems)
                    .ThenInclude(i => i.JobType)
                .FirstOrDefaultAsync(j => j.JobId == task.JobId.Value);

            if (job != null)
            {
                jobInfo = new
                {
                    job.JobId,
                    job.JobNo,
                    JobDate = job.JobDate.ToString("dd-MMM-yyyy"),
                    CustomerName = job.Party?.Name,
                    CustomerCode = job.Party?.Code,
                    CustomerGst = job.Party?.Gstno,
                    job.PartyRefNo,
                    DeliveryDate = job.DeliveryDate?.ToString("dd-MMM-yyyy"),
                    job.ProductName,
                    job.ProductDescription,
                    job.Quantity,
                    job.TotalPages,
                    job.Priority,
                    job.EstimatedCost,
                    job.QuotedAmount,
                    job.GrossAmount,
                    job.TaxAmount,
                    job.NetAmount,
                    Status = job.StatusCode,
                    job.CurrentStage,
                    job.ProgressPercent,
                    JobTypeName = job.JobType?.Jobtypename,
                    JobTypeCode = job.JobType?.Jobtypecode,
                    IsSingleProcess = job.JobType?.Issingleprocess ?? false,
                    JobCategoryName = job.JobCategory?.JobCategoryName,
                    CompanyName = job.Company?.Name,
                    job.SpecificationsJson
                };

                jobItems = job.TrnJobItems
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
                        i.DeliveredQuantity,
                        i.PendingQuantity,
                        i.NoOfPages,
                        i.UnitRate,
                        i.GrossAmount,
                        i.TaxableValue,
                        i.TotalTaxAmount,
                        i.NetAmount,
                        i.RateCalculatorId,
                        i.CalcRefNo,
                        i.Status,
                        i.Remarks,
                        RateCalc = i.RateCalculator == null ? null : new
                        {
                            i.RateCalculator.RateCalcId,
                            i.RateCalculator.CalcRefNo,
                            i.RateCalculator.Quantity,
                            i.RateCalculator.TotalPages,
                            i.RateCalculator.TrimWidthMm,
                            i.RateCalculator.TrimHeightMm,
                            i.RateCalculator.PrintingMode,
                            i.RateCalculator.GrandTotal,
                            i.RateCalculator.TaxAmount,
                            i.RateCalculator.NetTotal,
                            i.RateCalculator.CostPerUnit,
                            i.RateCalculator.CostBreakdown,
                            i.RateCalculator.BomData,
                            i.RateCalculator.PartsData,
                            i.RateCalculator.AiInsights,
                            i.RateCalculator.RecommendedMachines
                        }
                    })
                    .ToList();

                // Load all rate calculators linked to this job
                var rateCalcs = await _db.HybJobRateCalculators
                    .Include(r => r.ProductType)
                    .Include(r => r.JobType)
                    .Include(r => r.ProductSize)
                    .Where(r => r.JobId == job.JobId)
                    .OrderBy(r => r.RateCalcId)
                    .Select(r => new
                    {
                        r.RateCalcId,
                        r.CalcRefNo,
                        ProductTypeName = r.ProductType != null ? r.ProductType.Productname : null,
                        JobTypeName = r.JobType != null ? r.JobType.Jobtypename : null,
                        ProductSizeName = r.ProductSize != null ? r.ProductSize.Sizename : null,
                        r.Quantity,
                        r.TotalPages,
                        r.TrimWidthMm,
                        r.TrimHeightMm,
                        r.PrintingMode,
                        r.IsCustomerMaterial,
                        r.GrandTotal,
                        r.TaxAmount,
                        r.NetTotal,
                        r.CostPerUnit,
                        r.CostBreakdown,
                        r.BomData,
                        r.PartsData,
                        r.AiInsights,
                        r.RecommendedMachines,
                        r.Status,
                        r.InternalRemarks
                    })
                    .ToListAsync();

                rateCalculators = rateCalcs;
            }
        }

        // ── Process-specific context label ──
        var processContext = task.ProcessCode switch
        {
            "DES_DTP" => "DTP / Desktop Publishing — Review layout, typesetting, and design files.",
            "DES_ART" => "Artwork — Verify artwork quality, color profiles, and print readiness.",
            "PRE_CTP" => "CTP / Computer-to-Plate — Prepare plates from approved artwork.",
            "PROOF" => "Proofing — Generate and review proof copies before production.",
            "PRE_PRESS" => "Pre-Press — Final pre-press checks, trapping, imposition, and plate output.",
            "PRE_DES" => "Pre-Design — Initial design assessment and planning.",
            "PROC" => "Production / Printing — Execute the print run per job specifications.",
            "POST_PRESS" => "Post-Press — Binding, lamination, die-cutting, and finishing operations.",
            "QC_CHECK" => "Quality Check — Inspect output against job specifications and standards.",
            "PACKING" => "Packing — Pack finished goods for dispatch.",
            "CHALLAN" => "Challan — Prepare delivery challan for dispatching goods.",
            "GATE_PASS" => "Gate Pass — Generate and approve gate pass for vehicle dispatch.",
            "BILL" => "Sales Invoice — Generate and verify the sales invoice.",
            "PAY_REC" => "Payment Receipt — Record and verify customer payment.",
            "STORE_ISSUE" => "Store Issue — Issue raw materials from store for the job.",
            "GRN" => "Goods Receipt — Receive and inspect incoming materials.",
            _ => task.Process?.Processname ?? task.ProcessCode ?? "General Task"
        };

        return Ok(new
        {
            task = taskInfo,
            processContext,
            job = jobInfo,
            items = jobItems,
            rateCalculators
        });
    }

    // ═══════════════════════════════════════════════════════════
    //  PRIVATE HELPERS — Activity, Notification, Timeline
    // ═══════════════════════════════════════════════════════════

    private async Task ApplySourceSubmissionOnApprovalAsync(TrnWorkspaceTask task, UserSessionData user, string? remarks)
    {
        var processCode = (task.ProcessCode ?? string.Empty).ToUpperInvariant();
        var taskTitle = (task.Title ?? string.Empty).ToUpperInvariant();

        // Ref: auto-submit enquiry when first workspace approval (ENQ step) is approved
        var isEnquiryReceivedStep = task.SourceTable == WkSourceTable.Enquiry &&
                                    (processCode.Contains("ENQ") ||
                                     taskTitle.Contains("ENQUIRY RECEIVED") ||
                                     taskTitle.Contains("ENQ RECEIVED") ||
                                     taskTitle.Contains("ENQUIRY RECEIPT"));

        if (!isEnquiryReceivedStep)
            return;

        var enquiry = await _db.TrnEnquiries.FirstOrDefaultAsync(e => e.EnquiryId == task.SourceId);
        if (enquiry == null)
            return;

        if (!string.IsNullOrEmpty(enquiry.Status) &&
            !string.Equals(enquiry.Status, WkEnquiryStatus.Draft, StringComparison.OrdinalIgnoreCase))
            return;

        var now = DateTime.Now;
        var oldStatus = enquiry.Status;

        enquiry.Status = WkEnquiryStatus.Approved;
        enquiry.ModifiedBy = user.UserId.ToString();
        enquiry.ModifiedOn = now;

        _db.TrnEnquiryTimelines.Add(new TrnEnquiryTimeline
        {
            EnquiryId = enquiry.EnquiryId,
            EventType = "STATUS_CHANGED",
            EventCode = "APPROVED",
            EventTitle = "Enquiry Approved",
            EventDescription = $"Enquiry {enquiry.EnquiryNo} approved from workspace approval.",
            Remarks = remarks,
            OldStatus = oldStatus,
            NewStatus = "APPROVED",
            CreatedBy = user.UserId,
            CreatedOn = now,
            IsActive = true
        });

        await _db.SaveChangesAsync();
        _logger.LogInformation("Approved enquiry {EnquiryId} from approved workspace task {TaskId}.", enquiry.EnquiryId, task.WorkspaceTaskId);
    }

    private async Task ApplyJobStatusUpdateOnApprovalAsync(TrnWorkspaceTask task, UserSessionData user, string? remarks)
    {
        var processCode = (task.ProcessCode ?? string.Empty).ToUpperInvariant();
        var taskTitle = (task.Title ?? string.Empty).ToUpperInvariant();

        // Ref: update job status when JOB_CREATE or JOB_APPROVAL (costing approval) task is approved
        var isJobApprovalStep = task.SourceTable == WkSourceTable.Job &&
                                (processCode.Contains(WkProcessCode.JobCreate) ||
                                 processCode.Contains(WkProcessCode.JobApproval) ||
                                 taskTitle.Contains("JOB CREATION") ||
                                 taskTitle.Contains("COSTING APPROVAL") ||
                                 taskTitle.Contains("JOB APPROVAL"));

        if (!isJobApprovalStep)
            return;

        var job = await _db.TrnJobs.FirstOrDefaultAsync(j => j.JobId == task.SourceId);
        if (job == null)
            return;

        // Only update if not already approved or in a terminal state
        if (string.Equals(job.StatusCode, "APPROVED", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(job.StatusCode, "COMPLETED", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(job.StatusCode, "CANCELLED", StringComparison.OrdinalIgnoreCase))
            return;

        var now = DateTime.Now;
        var oldStatus = job.StatusCode;

        job.StatusCode = "APPROVED";
        job.CurrentStage = "APPROVED";
        job.ModifiedBy = user.UserId.ToString();
        job.ModifiedOn = now;

        _db.TrnJobTimelines.Add(new TrnJobTimeline
        {
            JobId = job.JobId,
            EventType = "STATUS_CHANGED",
            EventCode = "APPROVED",
            EventTitle = "Job Approved",
            EventDescription = $"Job {job.JobNo} approved from workspace approval.",
            Remarks = remarks,
            OldStatus = oldStatus,
            NewStatus = "APPROVED",
            ProcessCode = task.ProcessCode,
            ProcessName = task.Title,
            AssignedToUserId = task.UserId,
            CreatedBy = user.UserId,
            CreatedOn = now,
            IsActive = true
        });

        await _db.SaveChangesAsync();
        _logger.LogInformation("Approved job {JobId} (JobNo: {JobNo}) from approved workspace task {TaskId}.", job.JobId, job.JobNo, task.WorkspaceTaskId);
    }

    private static bool ShouldSkipNextStepGenerationOnApproval(TrnWorkspaceTask task)
    {
        var processCode = (task.ProcessCode ?? string.Empty).ToUpperInvariant();
        var taskTitle = (task.Title ?? string.Empty).ToUpperInvariant();
        var sourceTable = (task.SourceTable ?? string.Empty).ToLowerInvariant();

        // Ref: skip next-step for quotation/job conversion approvals (handled by dedicated conversion logic)
        // Only applies when source is ENQUIRY or QUOTATION — NOT for manual jobs (source = trn_job)
        var isQuotationConversionApproval = sourceTable == WkSourceTable.Enquiry &&
                                            (processCode.Contains(WkProcessCode.Quot) || taskTitle.Contains("QUOTATION GENERATION"));

        // Job conversion only applies when source is enquiry/quotation — manual jobs proceed to next step
        var isJobConversionApproval = (sourceTable == WkSourceTable.Enquiry || sourceTable == WkSourceTable.Quotation) &&
                                      (processCode.Contains(WkProcessCode.JobCreate) || taskTitle.Contains("JOB CREATION"));

        return isQuotationConversionApproval || isJobConversionApproval;
    }

    private async Task ApplySourceCancellationOnRejectionAsync(TrnWorkspaceTask task, UserSessionData user, string? remarks)
    {
        var processCode = (task.ProcessCode ?? string.Empty).ToUpperInvariant();
        var taskTitle = (task.Title ?? string.Empty).ToUpperInvariant();
        var now = DateTime.Now;

        // Quotation generation approval rejected -> cancel enquiry
        var isQuotationGenerationStep = task.SourceTable == WkSourceTable.Enquiry &&
                                        (processCode.Contains("QUOT") || taskTitle.Contains("QUOTATION GENERATION"));

        // Estimation / Costing started approval rejected -> cancel enquiry
        var isEstimationCostingStep = task.SourceTable == WkSourceTable.Enquiry &&
                                      (processCode.Contains("EST") || processCode.Contains("COST") ||
                                       taskTitle.Contains("ESTIMATION / COSTING") ||
                                       (taskTitle.Contains("ESTIMATION") && taskTitle.Contains("COSTING")));

        if (isQuotationGenerationStep || isEstimationCostingStep)
        {
            var enquiry = await _db.TrnEnquiries.FirstOrDefaultAsync(e => e.EnquiryId == task.SourceId);
            if (enquiry != null && !string.Equals(enquiry.Status, WkEnquiryStatus.Cancelled, StringComparison.OrdinalIgnoreCase))
            {
                var oldEnquiryStatus = enquiry.Status;
                enquiry.Status = WkEnquiryStatus.Cancelled;
                enquiry.ModifiedBy = user.UserId.ToString();
                enquiry.ModifiedOn = now;

                var cancelReason = isEstimationCostingStep
                    ? "estimation/costing approval was rejected"
                    : "quotation generation approval was rejected";

                _db.TrnEnquiryTimelines.Add(new TrnEnquiryTimeline
                {
                    EnquiryId = enquiry.EnquiryId,
                    EventType = "STATUS_CHANGED",
                    EventCode = "CANCELLED",
                    EventTitle = "Enquiry Cancelled",
                    EventDescription = $"Enquiry {enquiry.EnquiryNo} cancelled because {cancelReason}.",
                    Remarks = remarks,
                    OldStatus = oldEnquiryStatus,
                    NewStatus = "CANCELLED",
                    CreatedBy = user.UserId,
                    CreatedOn = now,
                    IsActive = true
                });

                await _db.SaveChangesAsync();
                _logger.LogInformation("Cancelled enquiry {EnquiryId} due to rejected quotation-generation approval task {TaskId}.", enquiry.EnquiryId, task.WorkspaceTaskId);
            }

            return;
        }

        // Job creation approval rejected -> cancel quotation
        var isJobCreationStep = (processCode.Contains(WkProcessCode.JobCreate) || processCode.Contains(WkProcessCode.EnqJob) || taskTitle.Contains("JOB CREATION"));

        if (isJobCreationStep)
        {
            TrnQuotation? quotation;

            if (task.SourceTable == WkSourceTable.Quotation)
            {
                quotation = await _db.TrnQuotations.FirstOrDefaultAsync(q => q.QuotationId == task.SourceId);
            }
            else if (task.SourceTable == WkSourceTable.Enquiry)
            {
                quotation = await _db.TrnQuotations
                    .Where(q => q.EnquiryId == task.SourceId)
                    .OrderByDescending(q => q.QuotationId)
                    .FirstOrDefaultAsync();
            }
            else
            {
                quotation = null;
            }

            if (quotation != null && !string.Equals(quotation.Status, WkEnquiryStatus.Cancelled, StringComparison.OrdinalIgnoreCase))
            {
                var oldQuotationStatus = quotation.Status;
                quotation.Status = WkEnquiryStatus.Cancelled;
                quotation.ModifiedBy = user.UserId.ToString();
                quotation.ModifiedOn = now;

                _db.TrnQuotationTimelines.Add(new TrnQuotationTimeline
                {
                    QuotationId = quotation.QuotationId,
                    EnquiryId = quotation.EnquiryId,
                    EventType = "STATUS_CHANGED",
                    EventCode = "CANCELLED",
                    EventTitle = "Quotation Cancelled",
                    EventDescription = $"Quotation {quotation.QuotationNo} cancelled because job creation approval was rejected.",
                    Remarks = remarks,
                    OldStatus = oldQuotationStatus,
                    NewStatus = "CANCELLED",
                    CreatedBy = user.UserId,
                    CreatedOn = now,
                    IsActive = true
                });

                await _db.SaveChangesAsync();
                _logger.LogInformation("Cancelled quotation {QuotationId} due to rejected job-creation approval task {TaskId}.", quotation.QuotationId, task.WorkspaceTaskId);

                await CascadeEnquiryCancellationFromQuotationAsync(
                    quotation,
                    user,
                    remarks,
                    "job creation approval was rejected");
            }
            else if (quotation == null)
            {
                _logger.LogWarning("Could not resolve quotation for rejected job-creation approval task {TaskId} (SourceTable={SourceTable}, SourceId={SourceId}).", task.WorkspaceTaskId, task.SourceTable, task.SourceId);
            }

            return;
        }

        // Job costing approval rejected (Scenario 3: manual job) -> cancel the job
        var isJobApprovalStep = task.SourceTable == WkSourceTable.Job &&
                                (processCode.Contains(WkProcessCode.JobApproval) || taskTitle.Contains("COSTING APPROVAL"));

        if (isJobApprovalStep)
        {
            var job = await _db.TrnJobs.FirstOrDefaultAsync(j => j.JobId == task.SourceId);
            if (job != null && !string.Equals(job.StatusCode, "CANCELLED", StringComparison.OrdinalIgnoreCase))
            {
                var oldJobStatus = job.StatusCode;
                job.StatusCode = "CANCELLED";
                job.CurrentStage = "COSTING_REJECTED";
                job.ModifiedBy = user.UserId.ToString();
                job.ModifiedOn = now;

                _db.TrnJobTimelines.Add(new TrnJobTimeline
                {
                    JobId = job.JobId,
                    EventType = "STATUS_CHANGED",
                    EventCode = "CANCELLED",
                    EventTitle = "Job Cancelled — Costing Rejected",
                    EventDescription = $"Job {job.JobNo} cancelled because costing approval was rejected.",
                    Remarks = remarks,
                    OldStatus = oldJobStatus,
                    NewStatus = "CANCELLED",
                    CreatedBy = user.UserId,
                    CreatedOn = now,
                    IsActive = true
                });

                await _db.SaveChangesAsync();
                _logger.LogInformation("Cancelled job {JobId} due to rejected costing approval task {TaskId}.", job.JobId, task.WorkspaceTaskId);
            }

            return;
        }

        // Quotation cancelled in workspace -> cancel related enquiry
        if (task.SourceTable == "trn_quotation")
        {
            var quotation = await _db.TrnQuotations.FirstOrDefaultAsync(q => q.QuotationId == task.SourceId);
            if (quotation != null && string.Equals(quotation.Status, "CANCELLED", StringComparison.OrdinalIgnoreCase))
            {
                await CascadeEnquiryCancellationFromQuotationAsync(
                    quotation,
                    user,
                    remarks,
                    "quotation was cancelled in workspace");
            }

            return;
        }

        // Job cancelled in workspace -> cancel related quotation + enquiry
        if (task.SourceTable == "trn_job")
        {
            var job = await _db.TrnJobs.FirstOrDefaultAsync(j => j.JobId == task.SourceId);
            if (job != null && string.Equals(job.StatusCode, "CANCELLED", StringComparison.OrdinalIgnoreCase))
            {
                await CascadeQuotationAndEnquiryCancellationFromJobAsync(
                    job,
                    user,
                    remarks,
                    "job was cancelled in workspace");
            }
        }
    }

    private async Task CascadeEnquiryCancellationFromQuotationAsync(TrnQuotation quotation, UserSessionData user, string? remarks, string reason)
    {
        if (!quotation.EnquiryId.HasValue)
            return;

        var enquiry = await _db.TrnEnquiries.FirstOrDefaultAsync(e => e.EnquiryId == quotation.EnquiryId.Value);
        if (enquiry == null || string.Equals(enquiry.Status, "CANCELLED", StringComparison.OrdinalIgnoreCase))
            return;

        var now = DateTime.Now;
        var oldEnquiryStatus = enquiry.Status;
        enquiry.Status = "CANCELLED";
        enquiry.ModifiedBy = user.UserId.ToString();
        enquiry.ModifiedOn = now;

        _db.TrnEnquiryTimelines.Add(new TrnEnquiryTimeline
        {
            EnquiryId = enquiry.EnquiryId,
            EventType = "STATUS_CHANGED",
            EventCode = "CANCELLED",
            EventTitle = "Enquiry Cancelled",
            EventDescription = $"Enquiry {enquiry.EnquiryNo} cancelled because {reason}.",
            Remarks = remarks,
            OldStatus = oldEnquiryStatus,
            NewStatus = "CANCELLED",
            CreatedBy = user.UserId,
            CreatedOn = now,
            IsActive = true
        });

        await _db.SaveChangesAsync();
        _logger.LogInformation("Cancelled enquiry {EnquiryId} due to quotation {QuotationId} cancellation.", enquiry.EnquiryId, quotation.QuotationId);
    }

    private async Task CascadeQuotationAndEnquiryCancellationFromJobAsync(TrnJob job, UserSessionData user, string? remarks, string reason)
    {
        TrnQuotation? quotation = null;

        if (job.QuotationId.HasValue)
        {
            quotation = await _db.TrnQuotations.FirstOrDefaultAsync(q => q.QuotationId == job.QuotationId.Value);
        }
        else if (job.EnquiryId.HasValue)
        {
            quotation = await _db.TrnQuotations
                .Where(q => q.EnquiryId == job.EnquiryId.Value)
                .OrderByDescending(q => q.QuotationId)
                .FirstOrDefaultAsync();
        }

        if (quotation != null && !string.Equals(quotation.Status, "CANCELLED", StringComparison.OrdinalIgnoreCase))
        {
            var now = DateTime.Now;
            var oldQuotationStatus = quotation.Status;

            quotation.Status = "CANCELLED";
            quotation.ModifiedBy = user.UserId.ToString();
            quotation.ModifiedOn = now;

            _db.TrnQuotationTimelines.Add(new TrnQuotationTimeline
            {
                QuotationId = quotation.QuotationId,
                EnquiryId = quotation.EnquiryId,
                EventType = "STATUS_CHANGED",
                EventCode = "CANCELLED",
                EventTitle = "Quotation Cancelled",
                EventDescription = $"Quotation {quotation.QuotationNo} cancelled because {reason}.",
                Remarks = remarks,
                OldStatus = oldQuotationStatus,
                NewStatus = "CANCELLED",
                CreatedBy = user.UserId,
                CreatedOn = now,
                IsActive = true
            });

            await _db.SaveChangesAsync();
            _logger.LogInformation("Cancelled quotation {QuotationId} due to job {JobId} cancellation.", quotation.QuotationId, job.JobId);
        }

        if (quotation != null)
        {
            await CascadeEnquiryCancellationFromQuotationAsync(quotation, user, remarks, reason);
            return;
        }

        if (job.EnquiryId.HasValue)
        {
            var enquiry = await _db.TrnEnquiries.FirstOrDefaultAsync(e => e.EnquiryId == job.EnquiryId.Value);
            if (enquiry != null && !string.Equals(enquiry.Status, "CANCELLED", StringComparison.OrdinalIgnoreCase))
            {
                var now = DateTime.Now;
                var oldEnquiryStatus = enquiry.Status;

                enquiry.Status = "CANCELLED";
                enquiry.ModifiedBy = user.UserId.ToString();
                enquiry.ModifiedOn = now;

                _db.TrnEnquiryTimelines.Add(new TrnEnquiryTimeline
                {
                    EnquiryId = enquiry.EnquiryId,
                    EventType = "STATUS_CHANGED",
                    EventCode = "CANCELLED",
                    EventTitle = "Enquiry Cancelled",
                    EventDescription = $"Enquiry {enquiry.EnquiryNo} cancelled because {reason}.",
                    Remarks = remarks,
                    OldStatus = oldEnquiryStatus,
                    NewStatus = "CANCELLED",
                    CreatedBy = user.UserId,
                    CreatedOn = now,
                    IsActive = true
                });

                await _db.SaveChangesAsync();
                _logger.LogInformation("Cancelled enquiry {EnquiryId} due to job {JobId} cancellation.", enquiry.EnquiryId, job.JobId);
            }
        }
    }

    private async Task<List<ProcessFlowStepMeta>> ResolveJobProcessStepsAsync(long jobId)
    {
        var jobContext = await _db.TrnJobs
            .Where(j => j.JobId == jobId)
            .Select(j => new { j.JobTypeId })
            .FirstOrDefaultAsync();

        var productTypeId = await _db.TrnJobItems
            .Where(i => i.JobId == jobId && i.PrintProductTypeId.HasValue)
            .OrderBy(i => i.ItemSequence)
            .Select(i => i.PrintProductTypeId)
            .FirstOrDefaultAsync();

        var workflowTemplateId = await ResolveWorkflowTemplateIdAsync(jobContext?.JobTypeId, productTypeId);

        if (workflowTemplateId.HasValue)
        {
            var templateSteps = await _db.MstWorkflowSteps
                .Where(s => s.WorkflowTemplateId == workflowTemplateId.Value && s.IsActive && s.ProcessId.HasValue)
                .Join(_db.MstProcesses.Where(p => p.Isactive &&
                                                  !DisabledProcessCodes.Contains(p.Processcode) &&
                                                  !WkProcessCode.PreJobProcesses.Contains(p.Processcode) &&
                                                  !(p.Processcode.StartsWith("QUOT") && p.Processcode.Contains("APPR"))),
                    s => s.ProcessId!.Value,
                    p => p.Processid,
                    (s, p) => new ProcessFlowStepMeta
                    {
                        ProcessId = p.Processid,
                        ProcessCode = p.Processcode,
                        ProcessName = p.Processname,
                        SequenceNo = s.SequenceNo,
                        DepartmentId = p.Departmentid,
                        DepartmentName = p.Department != null ? p.Department.DeptName : null,
                        IsApprovalRequired = p.Isapprovalrequired,
                        IsMandatory = p.Ismandatory
                    })
                .OrderBy(s => s.SequenceNo)
                .ToListAsync();

            if (templateSteps.Count > 0)
            {
                return templateSteps
                    .GroupBy(s => s.ProcessCode)
                    .Select(g => g.OrderBy(x => x.SequenceNo).First())
                    .OrderBy(s => s.SequenceNo)
                    .ToList();
            }
        }

        return await _db.MstProcesses
            .Where(p => p.Isactive &&
                        !DisabledProcessCodes.Contains(p.Processcode) &&
                        !WkProcessCode.PreJobProcesses.Contains(p.Processcode) &&
                        !(p.Processcode.StartsWith("QUOT") && p.Processcode.Contains("APPR")))
            .OrderBy(p => p.Sequenceno)
            .Select(p => new ProcessFlowStepMeta
            {
                ProcessId = p.Processid,
                ProcessCode = p.Processcode,
                ProcessName = p.Processname,
                SequenceNo = p.Sequenceno,
                DepartmentId = p.Departmentid,
                DepartmentName = p.Department != null ? p.Department.DeptName : null,
                IsApprovalRequired = p.Isapprovalrequired,
                IsMandatory = p.Ismandatory
            })
            .ToListAsync();
    }

    private async Task<long?> ResolveWorkflowTemplateIdAsync(int? jobTypeId, int? productTypeId)
    {
        var template = await _db.MstWorkflowTemplates
            .Where(t => t.IsActive)
            .Where(t =>
                (jobTypeId.HasValue && t.JobTypeId == jobTypeId.Value) ||
                t.IsDefault)
            .OrderByDescending(t => jobTypeId.HasValue && t.JobTypeId == jobTypeId.Value)
            .ThenByDescending(t => productTypeId.HasValue && t.PrintProductTypeId == productTypeId.Value)
            .ThenByDescending(t => t.IsDefault)
            .ThenByDescending(t => t.Version)
            .Select(t => new { t.WorkflowTemplateId })
            .FirstOrDefaultAsync();

        return template?.WorkflowTemplateId;
    }

    private sealed class ProcessFlowStepMeta
    {
        public int ProcessId { get; set; }
        public string ProcessCode { get; set; } = string.Empty;
        public string ProcessName { get; set; } = string.Empty;
        public int SequenceNo { get; set; }
        public long? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public bool? IsApprovalRequired { get; set; }
        public bool? IsMandatory { get; set; }
    }

    private async Task LogWorkspaceActivityAsync(UserSessionData user, TrnWorkspaceTask task, string activityType, string title)
    {
        try
        {
            var entry = ActivityLogEntry.FromUser(user, "WORKSPACE", activityType, title);
            entry.SubModule = task.TaskType;
            entry.EntityType = "WORKSPACE_TASK";
            entry.EntityId = task.WorkspaceTaskId;
            entry.EntityCode = task.SourceNo;
            entry.Description = $"{title} | Job: {task.JobNo ?? "—"} | Party: {task.PartyName ?? "—"}";
            entry.RelatedEntityType = task.SourceTable;
            entry.RelatedEntityId = task.SourceId;
            entry.RelatedEntityCode = task.JobNo;
            entry.JobId = task.JobId;
            entry.ProcessId = task.ProcessId;
            entry.Severity = "INFO";
            entry.NewValues = JsonSerializer.Serialize(new { task.TaskStatus, task.CompletionRemarks });
            await _activityService.LogActivityAsync(entry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log workspace activity for task {TaskId}", task.WorkspaceTaskId);
            await AuditExceptionAsync(ex, $"WorkspaceController.LogWorkspaceActivityAsync taskId={task.WorkspaceTaskId}");
        }
    }

    private async Task LogWorkspaceInAppNotificationAsync(UserSessionData user, TrnWorkspaceTask task, string eventType, string title, string message)
    {
        try
        {
            // Notify the task owner
            await _activityService.LogNotificationAsync(new UserNotificationEntry
            {
                UserId = task.UserId,
                Title = title,
                Message = message,
                Icon = task.TaskType == "APPROVAL" ? "bi bi-shield-check" : "bi bi-list-task",
                Color = eventType.Contains("REJECT") ? "danger" : eventType.Contains("APPROVE") ? "success" : "primary",
                Module = "WORKSPACE",
                EventType = eventType,
                ReferenceId = (int)task.WorkspaceTaskId,
                ReferenceUrl = task.ActionUrl,
                Priority = task.Priority ?? "NORMAL",
                ActionRequired = task.TaskStatus == "PENDING" || task.TaskStatus == "IN_PROGRESS",
                ActionUrl = task.ActionUrl
            });

            // Also notify the assigner if different from current user
            if (task.AssignedBy.HasValue && task.AssignedBy.Value != user.UserId)
            {
                await _activityService.LogNotificationAsync(new UserNotificationEntry
                {
                    UserId = task.AssignedBy.Value,
                    Title = $"[Update] {title}",
                    Message = $"{user.Name}: {message}",
                    Icon = "bi bi-bell-fill",
                    Color = "info",
                    Module = "WORKSPACE",
                    EventType = eventType,
                    ReferenceId = (int)task.WorkspaceTaskId,
                    ReferenceUrl = task.ActionUrl,
                    Priority = "NORMAL"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log workspace notification for task {TaskId}", task.WorkspaceTaskId);
            await AuditExceptionAsync(ex, $"WorkspaceController.LogWorkspaceInAppNotificationAsync taskId={task.WorkspaceTaskId}");
        }
    }

    private async Task AddWorkspaceJobTimelineAsync(TrnWorkspaceTask task, string eventType, string eventTitle,
        string description, string? oldStatus, string? newStatus, long userId)
    {
        if (!task.JobId.HasValue) return;

        try
        {
            var entry = new TrnJobTimeline
            {
                JobId = task.JobId.Value,
                EventType = eventType,
                EventCode = task.ProcessCode ?? eventType,
                EventTitle = eventTitle,
                EventDescription = description,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                ProcessCode = task.ProcessCode,
                ProcessName = task.Title,
                AssignedToUserId = task.UserId,
                CreatedBy = userId,
                CreatedOn = DateTime.Now,
                IsActive = true
            };
            _db.TrnJobTimelines.Add(entry);
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add job timeline for task {TaskId}, job {JobId}", task.WorkspaceTaskId, task.JobId);
            await AuditExceptionAsync(ex, $"WorkspaceController.AddWorkspaceJobTimelineAsync taskId={task.WorkspaceTaskId}");
        }
    }

    private async Task LogPartyActivityForTaskAsync(TrnWorkspaceTask task, string eventCode, string title, string userName)
    {
        try
        {
            if (string.IsNullOrEmpty(task.SourceTable)) return;

            // Resolve partyId from source table
            int? partyId = task.SourceTable switch
            {
                "trn_enquiry" => await _db.TrnEnquiries
                    .Where(e => e.EnquiryId == task.SourceId).Select(e => (int?)e.PartyId).FirstOrDefaultAsync(),
                "trn_quotation" => await _db.TrnQuotations
                    .Where(q => q.QuotationId == task.SourceId).Select(q => (int?)q.PartyId).FirstOrDefaultAsync(),
                "trn_job" => await _db.TrnJobs
                    .Where(j => j.JobId == task.SourceId).Select(j => j.PartyId).FirstOrDefaultAsync(),
                "trn_challan" => await _db.TrnChallans
                    .Where(c => c.ChallanId == task.SourceId).Select(c => c.PartyId).FirstOrDefaultAsync(),
                "trn_sales_invoice" => await _db.TrnSalesInvoices
                    .Where(i => i.SalesInvoiceId == task.SourceId).Select(i => i.PartyId).FirstOrDefaultAsync(),
                _ => null
            };

            if (!partyId.HasValue || partyId.Value <= 0) return;

            var activityType = task.SourceTable switch
            {
                "trn_enquiry" => "ENQUIRY",
                "trn_quotation" => "QUOTATION",
                "trn_job" => "JOB",
                "trn_challan" => "CHALLAN",
                "trn_sales_invoice" => "INVOICE",
                _ => "WORKSPACE"
            };

            await PartyPortalController.LogPartyActivityAsync(
                _db, partyId.Value, activityType, eventCode,
                title: title,
                description: $"{title} — {task.SourceNo}",
                referenceTable: task.SourceTable,
                referenceId: task.SourceId,
                documentNo: task.SourceNo,
                status: task.TaskStatus,
                createdBy: userName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log party activity for workspace task {TaskId}", task.WorkspaceTaskId);
            await AuditExceptionAsync(ex, $"WorkspaceController.LogPartyActivityForTaskAsync taskId={task.WorkspaceTaskId}", "Warning");
        }
    }

    private async Task DispatchWorkspaceNotificationAsync(TrnWorkspaceTask task, UserSessionData user, string newStatus, string eventLabel)
    {
        try
        {
            var config = new ProcessNotificationConfig
            {
                ConfigId = 0,
                ProcessCode = task.ProcessCode ?? "WORKSPACE",
                EventType = task.TaskType == "APPROVAL" ? NotificationEventType.ApprovalRequest : NotificationEventType.TaskAssign,
                EventLabel = eventLabel,
                RecipientType = RecipientType.Internal,
                NotifyAssignee = true,
                NotifyInternalEmail = true,
                NotifyPush = true,
                TemplateCode = task.TaskType == "APPROVAL"
                    ? nameof(NotificationTemplateCode.ApprovalPending)
                    : nameof(NotificationTemplateCode.TaskAssigned),
                Priority = (task.Priority?.ToUpper()) switch
                {
                    "CRITICAL" => NotificationPriority.Critical,
                    "URGENT" => NotificationPriority.Urgent,
                    "HIGH" => NotificationPriority.High,
                    _ => NotificationPriority.Normal
                },
                IsActive = true,
                TriggerOnStatus = newStatus,
                AutoTrigger = true
            };

            var template = new NotificationTemplate
            {
                TemplateId = 0,
                TemplateCode = config.TemplateCode,
                TemplateName = $"Workspace {eventLabel}",
                Module = nameof(NotificationModule.Job),
                EventType = nameof(NotificationEventType.TaskAssign),
                Channel = NotificationChannel.Email,
                SubjectTemplate = $"{{{{task_type}}}} — {{{{event_label}}}} | Job {{{{job_no}}}}",
                BodyTemplate = "<h3>" + eventLabel + "</h3>" +
                    "<p><strong>Task:</strong> {{task_title}}</p>" +
                    "<p><strong>Job No:</strong> {{job_no}}</p>" +
                    "<p><strong>Customer:</strong> {{party_name}}</p>" +
                    "<p><strong>Status:</strong> {{new_status}}</p>" +
                    "<p><strong>Action By:</strong> {{action_by}}</p>" +
                    "<p>Please review and take necessary action.</p>",
                IsActive = true
            };

            // Resolve the task owner's email
            var taskOwner = await _db.MstUsers.FirstOrDefaultAsync(u => u.Userid == task.UserId);

            var context = new NotificationContext
            {
                AssigneeUserId = (int)task.UserId,
                AssigneeEmail = taskOwner?.Emailid,
                Variables = new Dictionary<string, string>
                {
                    ["task_type"] = task.TaskType ?? "TASK",
                    ["event_label"] = eventLabel,
                    ["task_title"] = task.Title ?? "—",
                    ["job_no"] = task.JobNo ?? "—",
                    ["party_name"] = task.PartyName ?? "—",
                    ["new_status"] = newStatus,
                    ["action_by"] = user.Name,
                    ["action_date"] = DateTime.Now.ToString("dd-MMM-yyyy HH:mm")
                }
            };

            // If assigner is different, also set them as supervisor for notification
            if (task.AssignedBy.HasValue && task.AssignedBy.Value != task.UserId)
            {
                var assigner = await _db.MstUsers.FirstOrDefaultAsync(u => u.Userid == task.AssignedBy.Value);
                if (assigner != null)
                {
                    context.SupervisorUserId = (int)assigner.Userid;
                    context.SupervisorEmail = assigner.Emailid;
                    config.NotifySupervisor = true;
                }
            }

            var results = await _notificationDispatcher.DispatchAsync(config, template, context);
            _logger.LogInformation("Workspace task {TaskId}: Dispatched {Count} notifications for {Event}",
                task.WorkspaceTaskId, results.Count, eventLabel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to dispatch notification for workspace task {TaskId}", task.WorkspaceTaskId);
            await AuditExceptionAsync(ex, $"WorkspaceController.DispatchWorkspaceNotificationAsync taskId={task.WorkspaceTaskId}");
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

    // ══════════════════════════════════════════════════════════
    // ── Design Work Endpoints ─────────────────────────────────
    // ══════════════════════════════════════════════════════════

    /// <summary>Returns saved design work entries for a workspace task.</summary>
    [HttpGet("design-work/{taskId:long}")]
    public async Task<IActionResult> GetDesignWork(long taskId)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized(new { message = "Session expired." });

        var task = await _db.TrnWorkspaceTasks.AsNoTracking().FirstOrDefaultAsync(t => t.WorkspaceTaskId == taskId);
        if (task == null) return NotFound(new { message = "Task not found." });
        if (task.UserId != user.UserId) return StatusCode(StatusCodes.Status403Forbidden, new { message = "Access denied." });

        var entries = await _db.TrnDesignWorkEntries
            .AsNoTracking()
            .Where(e => e.WorkspaceTaskId == taskId)
            .OrderBy(e => e.ActivitySequence)
            .ThenBy(e => e.DesignWorkId)
            .Select(e => new
            {
                e.DesignWorkId,
                e.WorkspaceTaskId,
                e.JobId,
                e.ActivityName,
                e.ActivitySequence,
                e.PagesRequired,
                e.PagesCompleted,
                e.PagesPending,
                e.IsCompleted,
                e.CompletedOn,
                e.Notes,
                e.CreatedOn,
                e.ModifiedOn
            })
            .ToListAsync();

        return Ok(entries);
    }

    /// <summary>
    /// Upserts design work entries for checked rows.
    /// Called by Start Step, individual Complete, and Complete Step buttons.
    /// Update if DesignWorkId exists; insert new row otherwise.
    /// </summary>
    [HttpPost("design-work/{taskId:long}/upsert")]
    public async Task<IActionResult> UpsertDesignWork(long taskId, [FromBody] DesignWorkUpsertRequest? request = null)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized(new { message = "Session expired." });

        var task = await _db.TrnWorkspaceTasks.FindAsync(taskId);
        if (task == null) return NotFound(new { message = "Task not found." });
        if (task.UserId != user.UserId) return StatusCode(StatusCodes.Status403Forbidden, new { message = "Access denied." });

        if (request?.Rows == null || request.Rows.Count == 0)
            return Ok(new { message = "Nothing to save.", entries = Array.Empty<object>() });

        var now = DateTime.Now;
        var seq = 1;

        foreach (var item in request.Rows)
        {
            if (string.IsNullOrWhiteSpace(item.Activity)) { seq++; continue; }

            if (item.DesignWorkId.HasValue && item.DesignWorkId > 0)
            {
                // ── Update existing ──────────────────────────────────────
                var existing = await _db.TrnDesignWorkEntries.FindAsync(item.DesignWorkId.Value);
                if (existing != null && existing.WorkspaceTaskId == taskId)
                {
                    existing.ActivityName     = item.Activity;
                    existing.ActivitySequence = seq;
                    existing.PagesRequired    = item.Required;
                    existing.PagesCompleted   = item.Completed;
                    existing.IsCompleted      = item.IsCompleted;
                    if (item.IsCompleted && existing.CompletedOn == null)
                        existing.CompletedOn = now;
                    existing.Notes      = request.Notes;
                    existing.ModifiedBy = user.UserId;
                    existing.ModifiedOn = now;
                }
            }
            else
            {
                // ── Insert new ───────────────────────────────────────────
                _db.TrnDesignWorkEntries.Add(new TrnDesignWorkEntry
                {
                    WorkspaceTaskId  = taskId,
                    JobId            = task.JobId,
                    ActivityName     = item.Activity,
                    ActivitySequence = seq,
                    PagesRequired    = item.Required,
                    PagesCompleted   = item.Completed,
                    IsCompleted      = item.IsCompleted,
                    CompletedOn      = item.IsCompleted ? now : null,
                    Notes            = request.Notes,
                    CreatedBy        = user.UserId,
                    CreatedOn        = now
                });
            }
            seq++;
        }

        await _db.SaveChangesAsync();

        // Return all current entries for this task so the client can stamp IDs
        var entries = await _db.TrnDesignWorkEntries
            .AsNoTracking()
            .Where(e => e.WorkspaceTaskId == taskId)
            .OrderBy(e => e.ActivitySequence)
            .ThenBy(e => e.DesignWorkId)
            .Select(e => new
            {
                e.DesignWorkId,
                e.ActivityName,
                e.ActivitySequence,
                e.PagesRequired,
                e.PagesCompleted,
                e.PagesPending,
                e.IsCompleted,
                e.CompletedOn
            })
            .ToListAsync();

        return Ok(new { message = "Design work saved.", entries });
    }

    // ══════════════════════════════════════════════════════════
    // ── Plate Making Endpoints ────────────────────────────────
    // ══════════════════════════════════════════════════════════

    /// <summary>Returns saved plate making entries for a workspace task.</summary>
    [HttpGet("plate-making/{taskId:long}")]
    public async Task<IActionResult> GetPlateMaking(long taskId)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized(new { message = "Session expired." });

        var task = await _db.TrnWorkspaceTasks.AsNoTracking().FirstOrDefaultAsync(t => t.WorkspaceTaskId == taskId);
        if (task == null) return NotFound(new { message = "Task not found." });
        if (task.UserId != user.UserId) return StatusCode(StatusCodes.Status403Forbidden, new { message = "Access denied." });

        var entries = await _db.TrnPlateMakingEntries
            .AsNoTracking()
            .Where(e => e.WorkspaceTaskId == taskId)
            .OrderBy(e => e.ActivitySequence)
            .ThenBy(e => e.PlateMakingId)
            .Select(e => new
            {
                e.PlateMakingId,
                e.WorkspaceTaskId,
                e.JobId,
                e.ActivityName,
                e.ActivitySequence,
                e.PartName,
                e.PlateType,
                e.NumberOfColors,
                e.NumberOfPlates,
                e.PlatesMade,
                e.PlatesPending,
                e.IsCompleted,
                e.CompletedOn,
                e.Notes,
                e.CreatedOn,
                e.ModifiedOn
            })
            .ToListAsync();

        return Ok(entries);
    }

    /// <summary>
    /// Returns issued plate items from the store for the job linked to this workspace task.
    /// Only items where MaterialCategory contains 'PLATE' (case-insensitive) are returned.
    /// </summary>
    [HttpGet("plate-making/{taskId:long}/store-check")]
    public async Task<IActionResult> GetPlateMakingStoreCheck(long taskId)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized(new { message = "Session expired." });

        var task = await _db.TrnWorkspaceTasks.AsNoTracking().FirstOrDefaultAsync(t => t.WorkspaceTaskId == taskId);
        if (task == null) return NotFound(new { message = "Task not found." });
        if (task.UserId != user.UserId) return StatusCode(StatusCodes.Status403Forbidden, new { message = "Access denied." });

        if (task.JobId == null)
            return Ok(new { jobId = (long?)null, totalIssued = 0, items = Array.Empty<object>() });

        var plateItems = await _db.TrnStoreIssues
            .Where(i => i.JobId == task.JobId && i.Status != "CANCELLED")
            .SelectMany(i => i.TrnStoreIssueItems
                .Where(it => it.IsSelected == true && it.MaterialCategory != null && it.MaterialCategory.ToUpper().Contains("PLATE"))
                .Select(it => new
                {
                    issueNo       = i.IssueNo,
                    issueDate     = i.IssueDate.ToString("dd-MMM-yyyy"),
                    issueStatus   = i.Status,
                    materialName  = it.MaterialName,
                    materialCode  = it.MaterialCode,
                    forPart       = it.ForPart,
                    issuedQty     = it.IssuedQuantity,
                    uom           = it.Uom,
                    specification = it.Specification
                }))
            .OrderBy(it => it.issueNo)
            .ThenBy(it => it.forPart)
            .ToListAsync();

        return Ok(new
        {
            jobId        = task.JobId,
            totalIssued  = plateItems.Sum(p => p.issuedQty),
            itemCount    = plateItems.Count,
            items        = plateItems
        });
    }

    /// <summary>
    /// Upserts plate making entries. Update if PlateMakingId exists; insert new row otherwise.
    /// </summary>
    [HttpPost("plate-making/{taskId:long}/upsert")]
    public async Task<IActionResult> UpsertPlateMaking(long taskId, [FromBody] PlateMakingUpsertRequest? request = null)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized(new { message = "Session expired." });

        var task = await _db.TrnWorkspaceTasks.FindAsync(taskId);
        if (task == null) return NotFound(new { message = "Task not found." });
        if (task.UserId != user.UserId) return StatusCode(StatusCodes.Status403Forbidden, new { message = "Access denied." });

        if (request?.Rows == null || request.Rows.Count == 0)
            return Ok(new { message = "Nothing to save.", entries = Array.Empty<object>() });

        var now = DateTime.Now;
        var seq = 1;

        foreach (var item in request.Rows)
        {
            if (string.IsNullOrWhiteSpace(item.Activity)) { seq++; continue; }

            if (item.PlateMakingId.HasValue && item.PlateMakingId > 0)
            {
                var existing = await _db.TrnPlateMakingEntries.FindAsync(item.PlateMakingId.Value);
                if (existing != null && existing.WorkspaceTaskId == taskId)
                {
                    existing.ActivityName     = item.Activity;
                    existing.ActivitySequence = seq;
                    existing.PartName         = item.PartName;
                    existing.PlateType        = item.PlateType;
                    existing.NumberOfColors   = item.NumberOfColors;
                    existing.NumberOfPlates   = item.NumberOfPlates;
                    existing.PlatesMade       = item.PlatesMade;
                    existing.IsCompleted      = item.IsCompleted;
                    if (item.IsCompleted && existing.CompletedOn == null)
                        existing.CompletedOn = now;
                    existing.Notes      = request.Notes;
                    existing.ModifiedBy = user.UserId;
                    existing.ModifiedOn = now;
                }
            }
            else
            {
                _db.TrnPlateMakingEntries.Add(new TrnPlateMakingEntry
                {
                    WorkspaceTaskId  = taskId,
                    JobId            = task.JobId,
                    ActivityName     = item.Activity,
                    ActivitySequence = seq,
                    PartName         = item.PartName,
                    PlateType        = item.PlateType,
                    NumberOfColors   = item.NumberOfColors,
                    NumberOfPlates   = item.NumberOfPlates,
                    PlatesMade       = item.PlatesMade,
                    IsCompleted      = item.IsCompleted,
                    CompletedOn      = item.IsCompleted ? now : null,
                    Notes            = request.Notes,
                    CreatedBy        = user.UserId,
                    CreatedOn        = now
                });
            }
            seq++;
        }

        await _db.SaveChangesAsync();

        var entries = await _db.TrnPlateMakingEntries
            .AsNoTracking()
            .Where(e => e.WorkspaceTaskId == taskId)
            .OrderBy(e => e.ActivitySequence)
            .ThenBy(e => e.PlateMakingId)
            .Select(e => new
            {
                e.PlateMakingId,
                e.ActivityName,
                e.ActivitySequence,
                e.PartName,
                e.PlateType,
                e.NumberOfColors,
                e.NumberOfPlates,
                e.PlatesMade,
                e.PlatesPending,
                e.IsCompleted,
                e.CompletedOn
            })
            .ToListAsync();

        return Ok(new { message = "Plate making saved.", entries });
    }

    // ══════════════════════════════════════════════════════════
    // ── Print Work Endpoints ──────────────────────────────────
    // ══════════════════════════════════════════════════════════

    /// <summary>Returns printing machines (category = Printing or all if none tagged).</summary>
    [HttpGet("print-machines")]
    public async Task<IActionResult> GetPrintMachines()
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized(new { message = "Session expired." });

        var machines = await _db.MstMachines
            .AsNoTracking()
            .Where(m => m.IsActive == true || m.IsActive == null)
            .OrderBy(m => m.MachineName)
            .Select(m => new
            {
                m.MachineId,
                m.MachineName,
                m.MachineCode,
                m.MachineCategory,
                m.MaxColors
            })
            .ToListAsync();

        return Ok(machines);
    }

    /// <summary>Returns saved print work entries for a workspace task.</summary>
    [HttpGet("print-work/{taskId:long}")]
    public async Task<IActionResult> GetPrintWork(long taskId)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized(new { message = "Session expired." });

        var entries = await _db.TrnPrintWorkEntries
            .AsNoTracking()
            .Where(e => e.WorkspaceTaskId == taskId)
            .OrderBy(e => e.PartSequence)
            .ThenBy(e => e.PrintWorkId)
            .Select(e => new
            {
                e.PrintWorkId,
                e.WorkspaceTaskId,
                e.JobId,
                e.PartName,
                e.PartSequence,
                e.PrintingMethod,
                e.MachineId,
                e.MachineName,
                e.NumberOfColors,
                e.NumberOfPlates,
                e.TotalSheetsRequired,
                e.TotalSheetsPrinted,
                Balance = (e.TotalSheetsRequired ?? 0) - (e.TotalSheetsPrinted ?? 0),
                e.IsSelected,
                e.IsStarted,
                StartedOn = e.StartedOn.HasValue ? e.StartedOn.Value.ToString("dd-MMM-yyyy HH:mm") : null,
                CompletedOn = e.CompletedOn.HasValue ? e.CompletedOn.Value.ToString("dd-MMM-yyyy HH:mm") : null,
                e.Notes
            })
            .ToListAsync();

        return Ok(entries);
    }

    /// <summary>Upserts all print work entries for a task in a single call.</summary>
    [HttpPost("print-work/{taskId:long}/save")]
    public async Task<IActionResult> SavePrintWork(long taskId, [FromBody] PrintWorkSaveRequest request)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized(new { message = "Session expired." });

        var task = await _db.TrnWorkspaceTasks.FindAsync(taskId);
        if (task == null) return NotFound(new { message = "Task not found." });

        // Resolve machine names once
        var machineIds = request.Entries
            .Where(e => e.MachineId.HasValue)
            .Select(e => e.MachineId!.Value)
            .Distinct()
            .ToList();

        var machineNames = await _db.MstMachines
            .AsNoTracking()
            .Where(m => machineIds.Contains((int)m.MachineId))
            .ToDictionaryAsync(m => (int)m.MachineId, m => m.MachineName);

        var now = DateTime.Now;

        foreach (var entry in request.Entries)
        {
            var machineName = entry.MachineId.HasValue && machineNames.TryGetValue(entry.MachineId.Value, out var mn) ? mn : null;

            if (entry.PrintWorkId.HasValue)
            {
                // Update existing
                var existing = await _db.TrnPrintWorkEntries.FindAsync(entry.PrintWorkId.Value);
                if (existing == null || existing.WorkspaceTaskId != taskId) continue;

                existing.PartName = entry.PartName ?? existing.PartName;
                existing.PartSequence = entry.PartSequence;
                existing.PrintingMethod = entry.PrintingMethod;
                existing.MachineId = entry.MachineId;
                existing.MachineName = machineName;
                existing.NumberOfColors = entry.NumberOfColors;
                existing.NumberOfPlates = entry.NumberOfPlates;
                existing.TotalSheetsRequired = entry.TotalSheetsRequired;
                existing.TotalSheetsPrinted = entry.TotalSheetsPrinted;
                existing.IsSelected = entry.IsSelected;
                existing.ModifiedBy = user.UserId;
                existing.ModifiedOn = now;
            }
            else
            {
                // Insert new
                _db.TrnPrintWorkEntries.Add(new TrnPrintWorkEntry
                {
                    WorkspaceTaskId = taskId,
                    JobId = task.JobId,
                    PartName = entry.PartName ?? "—",
                    PartSequence = entry.PartSequence,
                    PrintingMethod = entry.PrintingMethod,
                    MachineId = entry.MachineId,
                    MachineName = machineName,
                    NumberOfColors = entry.NumberOfColors,
                    NumberOfPlates = entry.NumberOfPlates,
                    TotalSheetsRequired = entry.TotalSheetsRequired,
                    TotalSheetsPrinted = entry.TotalSheetsPrinted,
                    IsSelected = entry.IsSelected,
                    IsStarted = false,
                    CreatedBy = user.UserId,
                    CreatedOn = now
                });
            }
        }

        // Persist work notes on the task
        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            task.CompletionRemarks = request.Notes;
            task.ModifiedOn = now;
        }

        await _db.SaveChangesAsync();
        return Ok(new { message = "Print work saved successfully." });
    }

    /// <summary>Marks a single print work entry as started.</summary>
    [HttpPost("print-work/{entryId:long}/start-part")]
    public async Task<IActionResult> StartPrintPart(long entryId)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized(new { message = "Session expired." });

        var entry = await _db.TrnPrintWorkEntries.FindAsync(entryId);
        if (entry == null) return NotFound(new { message = "Print work entry not found." });

        entry.IsStarted = true;
        entry.StartedOn = DateTime.Now;
        entry.ModifiedBy = user.UserId;
        entry.ModifiedOn = DateTime.Now;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Part printing started." });
    }
}

public class TaskActionRequest
{
    public string? Remarks { get; set; }
}

public class WorkNoteRequest
{
    public string? Remarks { get; set; }
    public List<int>? PartIds { get; set; }
    public List<long>? ProcessInputIds { get; set; }
    public int ChecksCompleted { get; set; }
    public List<DesignProgressItem>? DesignProgress { get; set; }
}

public class DesignProgressItem
{
    public long? DesignWorkId { get; set; }
    public string? Activity { get; set; }
    public int Required { get; set; }
    public int Completed { get; set; }
    public int Pending { get; set; }
    public bool IsCompleted { get; set; }
}

public class DesignWorkUpsertRequest
{
    public string? Notes { get; set; }
    public List<DesignProgressItem> Rows { get; set; } = [];
}

public class PlateMakingUpsertRequest
{
    public string? Notes { get; set; }
    public List<PlateMakingRowItem> Rows { get; set; } = [];
}

public class PlateMakingRowItem
{
    public long? PlateMakingId { get; set; }
    public string Activity { get; set; } = string.Empty;
    public string? PartName { get; set; }
    public string? PlateType { get; set; }
    public int NumberOfColors { get; set; }
    public int NumberOfPlates { get; set; }
    public int PlatesMade { get; set; }
    public int PlatesPending { get; set; }
    public bool IsCompleted { get; set; }
}

public class PrintWorkSaveRequest
{
    public long WorkspaceTaskId { get; set; }
    public string? Notes { get; set; }
    public List<PrintWorkEntryDto> Entries { get; set; } = [];
}

public class PrintWorkEntryDto
{
    public long? PrintWorkId { get; set; }
    public string? PartName { get; set; }
    public int PartSequence { get; set; }
    public string? PrintingMethod { get; set; }
    public int? MachineId { get; set; }
    public int? NumberOfColors { get; set; }
    public int? NumberOfPlates { get; set; }
    public int? TotalSheetsRequired { get; set; }
    public int? TotalSheetsPrinted { get; set; }
    public bool IsSelected { get; set; }
}
