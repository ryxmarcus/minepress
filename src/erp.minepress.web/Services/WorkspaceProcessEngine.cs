using erp.minepress.infrastructure.ErrorLogging;
using erp.minepress.domain.Enums;
using erp.minepress.notification.Interfaces;
using erp.minepress.persistence.Context;
using erp.minepress.persistence.Models;
using erp.minepress.web.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace erp.minepress.web.Services;

/// <summary>
/// Central workspace engine using role-based dynamic routing:
///   User → map_user_role → Role → mst_process_role_map → Process → mst_process_department_map → Department
/// Reads mst_process_notification_config to drive SLA, priority, and notification flags.
/// </summary>
public class WorkspaceProcessEngine : IWorkspaceProcessEngine
{
    // Ref: prompt — "dont use hard code names, use code instead, make enums for all"
    private static readonly string[] DisabledProcessCodes = WkProcessCode.Disabled;

    private readonly ApplicationDbContext _db;
    private readonly INotificationService _notifier;
    private readonly IUserActivityService _activityService;
    private readonly ISystemErrorLogger _systemErrorLogger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<WorkspaceProcessEngine> _logger;

    public WorkspaceProcessEngine(
        ApplicationDbContext db,
        INotificationService notifier,
        IUserActivityService activityService,
        ISystemErrorLogger systemErrorLogger,
        IHttpContextAccessor httpContextAccessor,
        ILogger<WorkspaceProcessEngine> logger)
    {
        _db = db;
        _notifier = notifier;
        _activityService = activityService;
        _systemErrorLogger = systemErrorLogger;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    // ═══════════════════════════════════════════════════════════
    //  CENTRAL TASK CREATION — Called from every controller
    // ═══════════════════════════════════════════════════════════

    /// <inheritdoc />
    public async Task CreateWorkspaceTaskAsync(
        string processCode,
        string eventTypeCode,
        string sourceTable,
        long sourceId,
        string? sourceNo,
        string title,
        string? description,
        string taskType,
        string priority,
        UserSessionData triggeredBy,
        long? jobId = null,
        string? jobNo = null,
        string? partyName = null,
        string? actionUrl = null,
        int? partyId = null)
    {
        try
        {
            // Ref: skip disabled processes and quotation approval events
            if (IsDisabledProcessCode(processCode) ||
                (string.Equals(eventTypeCode, WkEventTypeCode.ProcApproval, StringComparison.OrdinalIgnoreCase) &&
                 !string.IsNullOrWhiteSpace(processCode) &&
                 processCode.StartsWith(WkProcessCode.Quot, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogInformation("Skipping workspace task creation for disabled process {ProcessCode}.", processCode);
                return;
            }

            // ── 0. Resolve job type context (new config is job-type aware) ──
            var jobType = await ResolveJobTypeForTaskContextAsync(sourceTable, sourceId, jobId);

            // ── 1. Look up notification config for this process + event ──
            var config = await _db.MstProcessNotificationConfigs
                .Where(c => c.ProcessCode == processCode &&
                            c.EventTypeCode == eventTypeCode &&
                            c.IsActive)
                .Where(c =>
                    (jobType.jobTypeId.HasValue && c.JobTypeId == jobType.jobTypeId.Value) ||
                    (jobType.jobTypeCode != null && c.JobTypeCode == jobType.jobTypeCode) ||
                    c.JobTypeCode == "ALL")
                .OrderByDescending(c => jobType.jobTypeId.HasValue && c.JobTypeId == jobType.jobTypeId.Value)
                .ThenByDescending(c => jobType.jobTypeCode != null && c.JobTypeCode == jobType.jobTypeCode)
                .FirstOrDefaultAsync();

            // ── 2. Resolve target users via role-based routing ──
            List<long> targetUsers;

            // Job approval processes must only route to MGT/ADM/EST — never to party/customer (dept 9999)
            var isJobApprovalProcess = WkProcessCode.ApprovalProcessCodes
                .Contains(processCode, StringComparer.OrdinalIgnoreCase);

            // Party-related process (dept 9999) should route to party workspace users
            var process = await _db.MstProcesses.FirstOrDefaultAsync(p => p.Processcode == processCode);
            // Ref: dept 9999 = Party Related Activity (mst_department)
            if (!isJobApprovalProcess &&
                ((process?.Departmentid ?? 0) == WkDepartment.PartyDeptId || (config?.DepartmentId ?? 0) == WkDepartment.PartyDeptId))
            {
                targetUsers = await ResolvePartyTargetUsersAsync(partyId);
            }
            else
            {
                targetUsers = await ResolveTargetUsers(processCode, triggeredBy, isJobApprovalProcess);
            }

            if (targetUsers.Count == 0)
            {
                _logger.LogWarning("No target users resolved for process {ProcessCode}/{EventType}. Assigning to triggering user.",
                    processCode, eventTypeCode);
                targetUsers = [triggeredBy.UserId];
            }

            // ── 3. Resolve process metadata ──
            var processId = process?.Processid;
            var departmentId = process?.Departmentid;

            // ── 4. Apply config/process overrides (SLA, priority, approval/task type) ──
            var now = DateTime.Now;
            var slaHours = config?.SlaHours ?? 12;
            var dueDate = now.AddHours((double)slaHours);
            var effectivePriority = config?.Priority ?? priority;
            var effectiveTaskType = ResolveTaskType(config, process, eventTypeCode, taskType);

            var metadata = BuildMetadata(config, processCode, eventTypeCode, sourceNo, partyName, jobType.jobTypeId, jobType.jobTypeCode);
            var createdTasks = new List<TrnWorkspaceTask>();

            foreach (var userId in targetUsers)
            {
                // ── Avoid duplicates: same user + same process + same source + same event ──
                var exists = await _db.TrnWorkspaceTasks.AnyAsync(t =>
                    t.UserId == userId &&
                    t.ProcessCode == processCode &&
                    t.SourceTable == sourceTable &&
                    t.SourceId == sourceId &&
                    t.TaskType == effectiveTaskType &&
                    t.TaskStatus != WkTaskStatus.Completed && t.TaskStatus != WkTaskStatus.Cancelled &&
                    t.TaskStatus != WkTaskStatus.Rejected && t.TaskStatus != WkTaskStatus.Approved);

                if (exists) continue;

                var task = new TrnWorkspaceTask
                {
                    UserId = userId,
                    SourceTable = sourceTable,
                    SourceId = sourceId,
                    SourceNo = sourceNo,
                    TaskType = effectiveTaskType,
                    TaskStatus = WkTaskStatus.Pending,
                    Title = config?.EventLabel ?? title,
                    Description = description ?? config?.BodyTemplate?.Replace("{job_no}", jobNo ?? sourceNo ?? ""),
                    ProcessId = processId,
                    ProcessCode = processCode,
                    DepartmentId = departmentId,
                    AssignedBy = triggeredBy.UserId,
                    AssignedOn = now,
                    Priority = effectivePriority,
                    DueDate = dueDate,
                    SlaHours = slaHours,
                    IsOverdue = false,
                    ApprovalTypeId = config?.ApprovalTypeId,
                    ApprovalLevel = config?.ApprovalLevel,
                    ActionUrl = actionUrl,
                    JobId = jobId,
                    JobNo = jobNo ?? sourceNo,
                    PartyName = partyName,
                    Metadata = metadata,
                    IsRead = false,
                    IsArchived = false,
                    CreatedOn = now
                };

                _db.TrnWorkspaceTasks.Add(task);
                createdTasks.Add(task);
            }

            if (createdTasks.Count > 0)
            {
                await _db.SaveChangesAsync();
                await NotifyAssignedUsersAsync(createdTasks, triggeredBy);

                // ── Auto-create item-level sub-tasks for parallel-eligible processes ──
                if (WkParallelProcessCodes.Eligible.Contains(processCode))
                {
                    foreach (var task in createdTasks)
                    {
                        await CreateItemTasksForParallelProcessAsync(task);
                    }
                }

                // ── Party Activity Log: Log when party is involved ──
                await LogPartyActivityIfApplicableAsync(
                    partyId, partyName, sourceTable, sourceId, sourceNo,
                    processCode, eventTypeCode, title, description,
                    "Pending", null, triggeredBy.Name);
            }

            _logger.LogInformation("Created {Count} workspace task(s) for {ProcessCode}/{EventType} — {SourceTable} #{SourceId}",
                createdTasks.Count, processCode, eventTypeCode, sourceTable, sourceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create workspace tasks for {ProcessCode}/{EventType} — {SourceTable} #{SourceId}",
                processCode, eventTypeCode, sourceTable, sourceId);
            await AuditExceptionAsync(ex, $"WorkspaceProcessEngine.CreateWorkspaceTaskAsync process={processCode} source={sourceTable}:{sourceId}");
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  NEXT-STEP GENERATION — Called when task/approval completes
    // ═══════════════════════════════════════════════════════════

    /// <inheritdoc />
    public async Task GenerateNextStepTasksAsync(TrnWorkspaceTask completedTask, UserSessionData completedBy)
    {
        try
        {
            // ══════════════════════════════════════════════════════════════════════
            // PRE-GENERATED WORKFLOW: If task has WorkflowBatchId, use sequential activation
            // ══════════════════════════════════════════════════════════════════════
            if (completedTask.WorkflowBatchId.HasValue)
            {
                _logger.LogInformation("Task {TaskId} is part of pre-generated workflow batch {BatchId}. Using sequential activation.",
                    completedTask.WorkspaceTaskId, completedTask.WorkflowBatchId);
                await ActivateNextQueuedTaskAsync(completedTask, completedBy);
                return;
            }

            // ══════════════════════════════════════════════════════════════════════
            // LEGACY: Ad-hoc task generation for tasks without pre-generated workflow
            // ══════════════════════════════════════════════════════════════════════
            if (string.IsNullOrEmpty(completedTask.ProcessCode))
            {
                _logger.LogInformation("Task {TaskId} has no process context; skipping next-step generation.",
                    completedTask.WorkspaceTaskId);
                return;
            }

            // ── Check if ALL sibling tasks for this step are done ──
            // Scope by JobId when available; otherwise scope by source document.
            var siblingQuery = _db.TrnWorkspaceTasks
                .Where(t => t.ProcessCode == completedTask.ProcessCode &&
                            !t.IsArchived &&
                            t.WorkspaceTaskId != completedTask.WorkspaceTaskId &&
                            t.TaskStatus != WkTaskStatus.Completed && t.TaskStatus != WkTaskStatus.Approved &&
                            t.TaskStatus != WkTaskStatus.Cancelled && t.TaskStatus != WkTaskStatus.Rejected);

            if (!string.IsNullOrWhiteSpace(completedTask.TaskType))
            {
                siblingQuery = siblingQuery.Where(t => t.TaskType == completedTask.TaskType);
            }

            if (completedTask.JobId.HasValue)
            {
                siblingQuery = siblingQuery.Where(t => t.JobId == completedTask.JobId);
            }
            else
            {
                siblingQuery = siblingQuery.Where(t => t.SourceTable == completedTask.SourceTable &&
                                                       t.SourceId == completedTask.SourceId);
            }

            var pendingSiblings = await siblingQuery.AnyAsync();

            if (pendingSiblings)
            {
                _logger.LogInformation("Task {TaskId}: Sibling tasks still pending for process {ProcessCode}. Waiting.",
                    completedTask.WorkspaceTaskId, completedTask.ProcessCode);
                return;
            }

            // ── Find current process sequence ──
            var currentProcess = await _db.MstProcesses
                .FirstOrDefaultAsync(p => p.Processcode == completedTask.ProcessCode && p.Isactive);

            if (currentProcess == null) return;

            var jobType = await ResolveJobTypeForTaskContextAsync(completedTask.SourceTable, completedTask.SourceId, completedTask.JobId);

            // ── Job-aware routing: skip pre-job processes when source is already a Job ──
            var isJobSource = string.Equals(completedTask.SourceTable, WkSourceTable.Job, StringComparison.OrdinalIgnoreCase);
            var skipCodes = isJobSource
                ? DisabledProcessCodes.Concat(WkProcessCode.PreJobProcesses).Distinct().ToArray()
                : DisabledProcessCodes;

            // ── Scenario 3: Manual job (no enquiry/quotation) requires costing approval ──
            if (isJobSource && completedTask.ProcessCode == WkProcessCode.JobCreate)
            {
                var job = await _db.TrnJobs.AsNoTracking()
                    .Where(j => j.JobId == completedTask.SourceId)
                    .Select(j => new { j.EnquiryId, j.QuotationId })
                    .FirstOrDefaultAsync();

                var isManualJob = (job?.EnquiryId == null || job.EnquiryId == 0)
                               && (job?.QuotationId == null || job.QuotationId == 0);

                if (isManualJob)
                {
                    // Manual job → route to JOB_APPROVAL for costing approval
                    var approvalProcess = await _db.MstProcesses
                        .FirstOrDefaultAsync(p => p.Processcode == WkProcessCode.JobApproval && p.Isactive);

                    if (approvalProcess != null)
                    {
                        var manualJobPartyId = await ResolvePartyIdFromSourceAsync(completedTask.SourceTable, completedTask.SourceId);
                        var approvalConfig = await _db.MstProcessNotificationConfigs
                            .Where(c => c.ProcessCode == approvalProcess.Processcode &&
                                        c.EventTypeCode == WkEventTypeCode.ProcStart &&
                                        c.IsActive && c.AutoTrigger)
                            .FirstOrDefaultAsync();

                        await CreateWorkspaceTaskAsync(
                            processCode: approvalProcess.Processcode,
                            eventTypeCode: WkEventTypeCode.ProcStart,
                            sourceTable: completedTask.SourceTable,
                            sourceId: completedTask.SourceId,
                            sourceNo: completedTask.SourceNo,
                            title: approvalConfig?.EventLabel ?? "Costing Approval Required",
                            description: $"Manual job {completedTask.JobNo ?? completedTask.SourceNo} requires costing approval before proceeding.",
                            taskType: WkTaskType.Approval,
                            priority: completedTask.Priority ?? WkPriority.Normal,
                            triggeredBy: completedBy,
                            jobId: completedTask.JobId,
                            jobNo: completedTask.JobNo,
                            partyName: completedTask.PartyName,
                            actionUrl: completedTask.ActionUrl,
                            partyId: manualJobPartyId);

                        _logger.LogInformation("Task {TaskId}: Manual job — routed to costing approval {Process}",
                            completedTask.WorkspaceTaskId, approvalProcess.Processcode);
                        return;
                    }
                }
            }

            // ── Scenario 4: Multi-item job after JOB_APPROVAL → per-item production workflows ──
            if (isJobSource &&
                string.Equals(completedTask.ProcessCode, WkProcessCode.JobApproval, StringComparison.OrdinalIgnoreCase) &&
                completedTask.JobId.HasValue)
            {
                var itemCount = await _db.TrnJobItems
                    .CountAsync(ji => ji.JobId == completedTask.JobId.Value);

                if (itemCount > 1)
                {
                    _logger.LogInformation("Task {TaskId}: Job {JobId} has {Count} item(s) — generating per-item production workflows.",
                        completedTask.WorkspaceTaskId, completedTask.JobId.Value, itemCount);
                    await GenerateItemScopedWorkflowsAsync(completedTask, completedBy);
                    return;
                }
            }

            // ── Find next process from workflow template (job-type aware), fallback to global sequence ──
            var nextProcess = await ResolveNextProcessFromWorkflowTemplateAsync(
                completedTask.ProcessCode,
                completedTask.JobId,
                jobType.jobTypeId);

            // If workflow template returned a pre-job process for a job source, skip it
            if (isJobSource && nextProcess != null && WkProcessCode.PreJobProcesses.Contains(nextProcess.Processcode))
                nextProcess = null;

            nextProcess ??= await _db.MstProcesses
                .Where(p => p.Isactive &&
                            p.Sequenceno > currentProcess.Sequenceno &&
                            !skipCodes.Contains(p.Processcode) &&
                            !(p.Processcode.StartsWith("QUOT") && p.Processcode.Contains("APPR")))
                .OrderBy(p => p.Sequenceno)
                .FirstOrDefaultAsync();

            if (nextProcess == null)
            {
                _logger.LogInformation("Task {TaskId}: No next process after {ProcessCode}. Workflow may be complete.",
                    completedTask.WorkspaceTaskId, completedTask.ProcessCode);
                return;
            }

            // ── Get notification config for the next process PROC_START event (job-type aware) ──
            var nextConfig = await _db.MstProcessNotificationConfigs
                .Where(c => c.ProcessCode == nextProcess.Processcode &&
                            c.EventTypeCode == WkEventTypeCode.ProcStart &&
                            c.IsActive && c.AutoTrigger)
                .Where(c =>
                    (jobType.jobTypeId.HasValue && c.JobTypeId == jobType.jobTypeId.Value) ||
                    (jobType.jobTypeCode != null && c.JobTypeCode == jobType.jobTypeCode) ||
                    c.JobTypeCode == "ALL")
                .OrderByDescending(c => jobType.jobTypeId.HasValue && c.JobTypeId == jobType.jobTypeId.Value)
                .ThenByDescending(c => jobType.jobTypeCode != null && c.JobTypeCode == jobType.jobTypeCode)
                .FirstOrDefaultAsync();

            // ── Resolve partyId from source table for party_activity_log ──
            var partyId = await ResolvePartyIdFromSourceAsync(completedTask.SourceTable, completedTask.SourceId);

            // ── Create workspace task for next process ──
            await CreateWorkspaceTaskAsync(
                processCode: nextProcess.Processcode,
                eventTypeCode: WkEventTypeCode.ProcStart,
                sourceTable: completedTask.SourceTable,
                sourceId: completedTask.SourceId,
                sourceNo: completedTask.SourceNo,
                title: nextConfig?.EventLabel ?? $"{nextProcess.Processname} Started",
                description: nextConfig?.BodyTemplate?.Replace("{job_no}", completedTask.JobNo ?? completedTask.SourceNo ?? "")
                             ?? $"Process {nextProcess.Processname} started for {completedTask.JobNo ?? completedTask.SourceNo}",
                taskType: nextProcess.Isapprovalrequired == true ? WkTaskType.Approval : WkTaskType.Task,
                priority: nextConfig?.Priority ?? completedTask.Priority ?? WkPriority.Normal,
                triggeredBy: completedBy,
                jobId: completedTask.JobId,
                jobNo: completedTask.JobNo,
                partyName: completedTask.PartyName,
                actionUrl: completedTask.ActionUrl,
                partyId: partyId);

            _logger.LogInformation("Task {TaskId}: Generated next-step tasks for process {NextProcess} (seq {Seq})",
                completedTask.WorkspaceTaskId, nextProcess.Processcode, nextProcess.Sequenceno);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate next-step tasks for completed task {TaskId}", completedTask.WorkspaceTaskId);
            await AuditExceptionAsync(ex, $"WorkspaceProcessEngine.GenerateNextStepTasksAsync taskId={completedTask.WorkspaceTaskId}");
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  ROLE-BASED USER RESOLUTION — Core routing engine
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Resolves target users for a process using the dynamic routing chain:
    ///   ProcessCode → mst_process_role_map (roleIds)
    ///   ProcessCode → mst_process_department_map (deptIds)
    ///   map_user_role (users with matching roles) filtered by department
    /// </summary>
    private async Task<List<long>> ResolveTargetUsers(string processCode, UserSessionData triggeredBy, bool restrictToApprovalDepts = false)
    {
        // ── Get roles allowed for this process ──
        var roleIds = await _db.MstProcessRoleMaps
            .Where(pr => pr.ProcessCode == processCode && pr.IsActive == true)
            .Where(pr => pr.Roleid.HasValue)
            .Select(pr => pr.Roleid!.Value)
            .ToListAsync();

        // ── Get departments allowed for this process ──
        var deptIds = await _db.MstProcessDepartmentMaps
            .Where(pd => pd.ProcessCode == processCode && pd.IsActive == true)
            .Where(pd => pd.DeptId.HasValue)
            .Select(pd => pd.DeptId!.Value)
            .ToListAsync();

        if (roleIds.Count == 0 || deptIds.Count == 0)
        {
            _logger.LogWarning("No role/department mappings for process {ProcessCode}. Fallback to triggering user.", processCode);
            return [triggeredBy.UserId];
        }

        // ── Find users with matching roles AND departments ──
        // Approval processes are restricted to MGT/ADM/EST only (WkDepartment.ApprovalDeptIds)
        var longDeptIds = restrictToApprovalDepts
            ? WkDepartment.ApprovalDeptIds.Select(d => (long)d).ToList()
            : deptIds.Select(d => (long)d).ToList();

        var userIds = await _db.MapUserRoles
            .Where(ur => roleIds.Contains(ur.Roleid) && ur.Isactive == true)
            .Join(
                _db.MstUsers.Where(u => u.Isactive == true && longDeptIds.Contains(u.Departmentid)),
                ur => ur.Userid,
                u => u.Userid,
                (ur, u) => u.Userid)
            .Distinct()
            .ToListAsync();

        if (userIds.Count == 0)
        {
            _logger.LogWarning("No users found matching roles {Roles} + departments {Depts} for process {ProcessCode}. Fallback to triggering user.",
                string.Join(",", roleIds), string.Join(",", deptIds), processCode);
            return [triggeredBy.UserId];
        }

        return userIds;
    }

    // ═══════════════════════════════════════════════════════════
    //  HELPERS
    // ═══════════════════════════════════════════════════════════

    private static string ResolveTaskType(MstProcessNotificationConfig? config, MstProcess? process, string eventTypeCode, string fallbackTaskType)
    {
        // Ref: resolve task type from config/process/event — approval takes precedence
        if (config?.ApprovalTypeId.HasValue == true || (config?.ApprovalLevel ?? 0) > 0 || (process?.Isapprovalrequired ?? false))
            return WkTaskType.Approval;

        return eventTypeCode switch
        {
            WkEventTypeCode.ProcApproval => WkTaskType.Approval,
            WkEventTypeCode.ProcStart or WkEventTypeCode.ProcComplete => WkTaskType.Task,
            WkEventTypeCode.TaskAssign or WkEventTypeCode.TaskStart or WkEventTypeCode.TaskComplete => WkTaskType.Task,
            WkEventTypeCode.ApprovalRequest or WkEventTypeCode.ApprovalApproved or WkEventTypeCode.ApprovalRejected => WkTaskType.Approval,
            WkEventTypeCode.OverdueAlert or WkEventTypeCode.TopupAlert => WkTaskType.FollowUp,
            WkEventTypeCode.ClientNotify => WkTaskType.Task,
            WkEventTypeCode.AiInsight => WkTaskType.Review,
            _ => string.IsNullOrWhiteSpace(fallbackTaskType) ? WkTaskType.Task : fallbackTaskType
        };
    }

    private static string BuildMetadata(MstProcessNotificationConfig? config, string processCode,
        string eventTypeCode, string? sourceNo, string? partyName, int? jobTypeId, string? jobTypeCode)
    {
        var meta = new Dictionary<string, object?>
        {
            ["process_code"] = processCode,
            ["event_type"] = eventTypeCode,
            ["source_no"] = sourceNo,
            ["party_name"] = partyName,
            ["job_type_id"] = jobTypeId,
            ["job_type_code"] = jobTypeCode
        };

        if (config != null)
        {
            meta["config_id"] = config.ConfigId;
            meta["template_code"] = config.TemplateCode;
            meta["sla_hours"] = config.SlaHours;
            meta["escalate_after_hours"] = config.EscalateAfterHours;
            meta["escalate_to"] = config.EscalateTo;

            if (!string.IsNullOrEmpty(config.AiConfig) && config.AiConfig != "{}" &&
                config.AiConfig.Contains("\"ai_enabled\": true"))
            {
                meta["ai_config"] = config.AiConfig;
            }
        }

        return JsonSerializer.Serialize(meta);
    }

    private async Task<(int? jobTypeId, string? jobTypeCode)> ResolveJobTypeForTaskContextAsync(string sourceTable, long sourceId, long? jobId)
    {
        try
        {
            if (jobId.HasValue && jobId.Value > 0)
            {
                var j = await _db.TrnJobs
                    .Where(x => x.JobId == jobId.Value)
                    .Select(x => new { x.JobTypeId, JobTypeCode = x.JobType != null ? x.JobType.Jobtypecode : null })
                    .FirstOrDefaultAsync();
                return (j?.JobTypeId, j?.JobTypeCode);
            }

            if (sourceTable == WkSourceTable.Job)
            {
                var j = await _db.TrnJobs
                    .Where(x => x.JobId == sourceId)
                    .Select(x => new { x.JobTypeId, JobTypeCode = x.JobType != null ? x.JobType.Jobtypecode : null })
                    .FirstOrDefaultAsync();
                return (j?.JobTypeId, j?.JobTypeCode);
            }

            if (sourceTable == WkSourceTable.Challan)
            {
                var j = await _db.TrnChallans
                    .Where(x => x.ChallanId == sourceId)
                    .Select(x => new { x.Job.JobTypeId, JobTypeCode = x.Job.JobType != null ? x.Job.JobType.Jobtypecode : null })
                    .FirstOrDefaultAsync();
                return (j?.JobTypeId, j?.JobTypeCode);
            }

            if (sourceTable == WkSourceTable.Quotation)
            {
                var j = await _db.TrnJobs
                    .Where(x => x.QuotationId == sourceId)
                    .OrderByDescending(x => x.CreatedOn)
                    .Select(x => new { x.JobTypeId, JobTypeCode = x.JobType != null ? x.JobType.Jobtypecode : null })
                    .FirstOrDefaultAsync();
                return (j?.JobTypeId, j?.JobTypeCode);
            }

            if (sourceTable == WkSourceTable.Enquiry)
            {
                var j = await _db.TrnJobs
                    .Where(x => x.EnquiryId == sourceId)
                    .OrderByDescending(x => x.CreatedOn)
                    .Select(x => new { x.JobTypeId, JobTypeCode = x.JobType != null ? x.JobType.Jobtypecode : null })
                    .FirstOrDefaultAsync();
                return (j?.JobTypeId, j?.JobTypeCode);
            }
        }
        catch { }

        return (null, null);
    }

    private async Task<MstProcess?> ResolveNextProcessFromWorkflowTemplateAsync(string currentProcessCode, long? jobId, int? jobTypeId)
    {
        if (string.IsNullOrWhiteSpace(currentProcessCode))
            return null;

        var workflowTemplateId = await ResolveWorkflowTemplateIdForContextAsync(jobId, jobTypeId);
        if (!workflowTemplateId.HasValue)
            return null;

        var currentSteps = await _db.MstWorkflowSteps
            .Where(s => s.WorkflowTemplateId == workflowTemplateId.Value && s.IsActive)
            .Where(s =>
                (s.Process != null && s.Process.Processcode == currentProcessCode) ||
                s.StepCode == currentProcessCode)
            .OrderBy(s => s.SequenceNo)
            .Select(s => new { s.WorkflowStepId, s.SequenceNo })
            .ToListAsync();

        if (currentSteps.Count == 0)
            return null;

        var currentStepIds = currentSteps.Select(s => s.WorkflowStepId).ToList();

        var nextFromConnections = await _db.MstWorkflowConnections
            .Where(c => c.WorkflowTemplateId == workflowTemplateId.Value &&
                        c.IsActive &&
                        currentStepIds.Contains(c.FromStepId))
            .OrderBy(c => c.SequenceNo)
            .Join(_db.MstWorkflowSteps.Where(s => s.IsActive && s.ProcessId.HasValue),
                c => c.ToStepId,
                s => s.WorkflowStepId,
                (c, s) => s)
            .Join(_db.MstProcesses.Where(p => p.Isactive &&
                                              !DisabledProcessCodes.Contains(p.Processcode) &&
                                              !(p.Processcode.StartsWith("QUOT") && p.Processcode.Contains("APPR")) &&
                                              p.Processcode != currentProcessCode),
                s => s.ProcessId!.Value,
                p => p.Processid,
                (s, p) => p)
            .FirstOrDefaultAsync();

        if (nextFromConnections != null)
            return nextFromConnections;

        var currentSequence = currentSteps.Min(s => s.SequenceNo);

        var nextBySequence = await _db.MstWorkflowSteps
            .Where(s => s.WorkflowTemplateId == workflowTemplateId.Value &&
                        s.IsActive &&
                        s.ProcessId.HasValue &&
                        s.SequenceNo > currentSequence)
            .OrderBy(s => s.SequenceNo)
            .Join(_db.MstProcesses.Where(p => p.Isactive &&
                                              !DisabledProcessCodes.Contains(p.Processcode) &&
                                              !(p.Processcode.StartsWith("QUOT") && p.Processcode.Contains("APPR")) &&
                                              p.Processcode != currentProcessCode),
                s => s.ProcessId!.Value,
                p => p.Processid,
                (s, p) => p)
            .FirstOrDefaultAsync();

        return nextBySequence;
    }

    private async Task<long?> ResolveWorkflowTemplateIdForContextAsync(long? jobId, int? jobTypeId)
    {
        int? productTypeId = null;

        if (jobId.HasValue && jobId.Value > 0)
        {
            productTypeId = await _db.TrnJobItems
                .Where(i => i.JobId == jobId.Value && i.PrintProductTypeId.HasValue)
                .OrderBy(i => i.ItemSequence)
                .Select(i => i.PrintProductTypeId)
                .FirstOrDefaultAsync();
        }

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

    private static bool IsDisabledProcessCode(string? processCode)
    {
        if (string.IsNullOrWhiteSpace(processCode))
            return false;

        var code = processCode.Trim();

        if (DisabledProcessCodes.Contains(code, StringComparer.OrdinalIgnoreCase))
            return true;

        return code.StartsWith("QUOT", StringComparison.OrdinalIgnoreCase) &&
               code.Contains("APPR", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Creates item-level sub-tasks for parallel-eligible workspace tasks.
    /// </summary>
    private async Task CreateItemTasksForParallelProcessAsync(TrnWorkspaceTask task)
    {
        if (!task.JobId.HasValue) return;

        var exists = await _db.TrnWorkspaceTaskItems
            .AnyAsync(ti => ti.WorkspaceTaskId == task.WorkspaceTaskId);
        if (exists) return;

        var jobItems = await _db.TrnJobItems
            .Where(ji => ji.JobId == task.JobId.Value)
            .OrderBy(ji => ji.ItemSequence)
            .Select(ji => new { ji.JobItemId, ji.ItemSequence, ji.ProductName, ji.ProductDescription })
            .ToListAsync();

        if (jobItems.Count == 0) return;

        var processName = await _db.MstProcesses
            .Where(p => p.Processcode == task.ProcessCode && p.Isactive)
            .Select(p => p.Processname)
            .FirstOrDefaultAsync();

        // Find upstream completed item tasks for parent linking
        var upstreamProcess = WkParallelProcessCodes.NextProcessPerItem
            .Where(kvp => kvp.Value == task.ProcessCode)
            .Select(kvp => kvp.Key)
            .FirstOrDefault();

        Dictionary<long, long>? upstreamMap = null;
        if (upstreamProcess != null)
        {
            upstreamMap = await _db.TrnWorkspaceTaskItems
                .Where(ti => ti.JobId == task.JobId.Value
                    && ti.ProcessCode == upstreamProcess
                    && ti.TaskStatus == WkItemTaskStatus.Completed)
                .ToDictionaryAsync(ti => ti.JobItemId, ti => ti.TaskItemId);
        }

        var now = DateTime.Now;
        foreach (var item in jobItems)
        {
            _db.TrnWorkspaceTaskItems.Add(new TrnWorkspaceTaskItem
            {
                WorkspaceTaskId = task.WorkspaceTaskId,
                JobId = task.JobId.Value,
                JobItemId = item.JobItemId,
                ProcessCode = task.ProcessCode,
                ProcessName = processName,
                ItemName = item.ProductName ?? $"Item #{item.ItemSequence}",
                ItemDescription = item.ProductDescription,
                ItemSequence = item.ItemSequence,
                TaskStatus = WkItemTaskStatus.NotStarted,
                AssignedUserId = task.UserId,
                AssignedOn = now,
                CreatedOn = now,
                ParentTaskItemId = upstreamMap?.GetValueOrDefault(item.JobItemId)
            });
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("Auto-created {Count} item tasks for Task {TaskId} process {Process}.",
            jobItems.Count, task.WorkspaceTaskId, task.ProcessCode);
    }

    private async Task<List<long>> ResolvePartyTargetUsersAsync(int? partyId)
    {
        // Ref: dept 9999 = Party users (mst_department.PTY)
        var query = _db.MstUsers.Where(u => u.Isactive == true && u.Departmentid == WkDepartment.PartyDeptId);

        if (partyId.HasValue && partyId.Value > 0)
        {
            var forParty = await query
                .Where(u => u.RefId == partyId.Value)
                .Select(u => u.Userid)
                .Distinct()
                .ToListAsync();

            if (forParty.Count > 0) return forParty;
        }

        return await query
            .Select(u => u.Userid)
            .Distinct()
            .ToListAsync();
    }

    // ═══════════════════════════════════════════════════════════
    //  POST-TASK-CREATION: Email, Activity Log & In-App Notification
    // ═══════════════════════════════════════════════════════════

    private async Task NotifyAssignedUsersAsync(List<TrnWorkspaceTask> createdTasks, UserSessionData triggeredBy)
    {
        if (createdTasks.Count == 0) return;

        try
        {
            var userIds = createdTasks.Select(t => t.UserId).Distinct().ToList();
            var users = await _db.MstUsers
                .Where(u => userIds.Contains(u.Userid) && u.Isactive == true)
                .Select(u => new { u.Userid, u.Name, u.Emailid })
                .ToListAsync();

            var userLookup = users.ToDictionary(u => u.Userid);

            foreach (var task in createdTasks)
            {
                // Ref: map source table → module name for activity logging
                var moduleName = task.SourceTable switch
                {
                    WkSourceTable.Enquiry => WkModuleName.Enquiry,
                    WkSourceTable.Quotation => WkModuleName.Quotation,
                    WkSourceTable.Job => WkModuleName.Job,
                    WkSourceTable.Challan => WkModuleName.Challan,
                    WkSourceTable.PurchaseOrder => WkModuleName.Purchase,
                    WkSourceTable.SalesInvoice => WkModuleName.Invoice,
                    _ => WkModuleName.Workspace
                };

                var assigneeName = userLookup.TryGetValue(task.UserId, out var u) ? u.Name : "User";

                // ── 1. Email Notification to Assigned User ──
                if (userLookup.TryGetValue(task.UserId, out var assignee) && !string.IsNullOrEmpty(assignee.Emailid))
                {
                    var subject = task.TaskType == WkTaskType.Approval
                        ? $"🔔 Approval Required: {task.Title}"
                        : $"📋 New Task Assigned: {task.Title}";

                    var htmlBody = $"""
                        <div style="font-family:Segoe UI,Arial,sans-serif;max-width:600px;margin:0 auto;">
                            <div style="background:{(task.TaskType == WkTaskType.Approval ? "#e67700" : "#1971c2")};color:#fff;padding:16px 24px;border-radius:8px 8px 0 0;">
                                <h2 style="margin:0;font-size:18px;">{(task.TaskType == WkTaskType.Approval ? "⚡ Approval Required" : "📋 New Task Assigned")}</h2>
                            </div>
                            <div style="border:1px solid #dee2e6;border-top:none;padding:20px 24px;border-radius:0 0 8px 8px;">
                                <table style="width:100%;border-collapse:collapse;font-size:14px;">
                                    <tr><td style="padding:6px 0;color:#666;width:130px;"><b>{(task.TaskType == WkTaskType.Approval ? "Approval" : "Task")}:</b></td><td>{task.Title}</td></tr>
                                    <tr><td style="padding:6px 0;color:#666;"><b>Reference:</b></td><td>{task.SourceNo}</td></tr>
                                    {(task.PartyName != null ? $"<tr><td style=\"padding:6px 0;color:#666;\"><b>Customer:</b></td><td>{task.PartyName}</td></tr>" : "")}
                                    <tr><td style="padding:6px 0;color:#666;"><b>Process:</b></td><td>{task.ProcessCode}</td></tr>
                                    <tr><td style="padding:6px 0;color:#666;"><b>Priority:</b></td><td><span style="background:{PriorityColor(task.Priority)};color:#fff;padding:2px 10px;border-radius:4px;font-size:12px;">{task.Priority ?? WkPriority.Normal}</span></td></tr>
                                    {(task.DueDate.HasValue ? $"<tr><td style=\"padding:6px 0;color:#666;\"><b>Due Date:</b></td><td>{task.DueDate.Value:dd-MMM-yyyy HH:mm}</td></tr>" : "")}
                                    <tr><td style="padding:6px 0;color:#666;"><b>Assigned By:</b></td><td>{triggeredBy.Name}</td></tr>
                                </table>
                                <p style="margin:16px 0 8px;">{task.Description}</p>
                                <div style="text-align:center;margin:20px 0;">
                                    <a href="{task.ActionUrl ?? "/Workspace/MyTasks"}" style="background:{(task.TaskType == WkTaskType.Approval ? "#e67700" : "#1971c2")};color:#fff;padding:10px 28px;border-radius:6px;text-decoration:none;font-weight:600;">
                                        {(task.TaskType == WkTaskType.Approval ? "Review & Approve" : "View Task")}
                                    </a>
                                </div>
                            </div>
                            <p style="text-align:center;margin:12px 0 0;font-size:11px;color:#888;">MinePress ERP — Automated Workspace Notification</p>
                        </div>
                        """;

                    await _notifier.SendEmailAsync(assignee.Emailid, subject, htmlBody);
                }

                // ── 2. Activity Log: Task Assigned ──
                var activity = ActivityLogEntry.FromUser(triggeredBy, WkModuleName.Workspace, WkEventTypeCode.TaskAssigned,
                    $"{(task.TaskType == WkTaskType.Approval ? "Approval" : "Task")} assigned: {task.Title}");
                activity.EntityType = "WORKSPACE_TASK";
                activity.EntityId = task.WorkspaceTaskId;
                activity.EntityCode = task.SourceNo;
                activity.Description = $"{task.TaskType} \"{task.Title}\" assigned to {assigneeName} for {moduleName} {task.SourceNo}.";
                activity.RelatedEntityType = moduleName;
                activity.RelatedEntityId = task.SourceId;
                activity.RelatedEntityCode = task.SourceNo;
                activity.ProcessId = task.ProcessId;
                activity.NewValues = JsonSerializer.Serialize(new
                {
                    task.WorkspaceTaskId,
                    task.TaskType,
                    task.TaskStatus,
                    AssignedTo = task.UserId,
                    task.SourceNo,
                    task.Priority,
                    task.DueDate
                });
                activity.Severity = "INFO";
                await _activityService.LogActivityAsync(activity);

                // ── 3. In-App Popup Notification for Assigned User ──
                await _activityService.LogNotificationAsync(new UserNotificationEntry
                {
                    UserId = task.UserId,
                    Title = task.TaskType == WkTaskType.Approval
                        ? $"Approval Required: {task.SourceNo}"
                        : $"New Task: {task.SourceNo}",
                    Message = $"{task.Title} — assigned by {triggeredBy.Name}. {(task.DueDate.HasValue ? $"Due: {task.DueDate.Value:dd-MMM-yyyy HH:mm}." : "")}",
                    Icon = task.TaskType == WkTaskType.Approval ? "bi bi-shield-check" : "bi bi-clipboard-check",
                    Color = task.TaskType == WkTaskType.Approval ? "warning" : "info",
                    Module = WkModuleName.Workspace,
                    EventType = WkEventTypeCode.TaskAssigned,
                    ReferenceId = (int)task.WorkspaceTaskId,
                    ReferenceUrl = task.ActionUrl ?? "/Workspace/MyTasks",
                    Priority = task.Priority is WkPriority.High or WkPriority.Urgent ? WkPriority.High : WkPriority.Normal,
                    ActionRequired = true,
                    ActionUrl = task.ActionUrl ?? "/Workspace/MyTasks",
                    ActionLabel = task.TaskType == WkTaskType.Approval ? "Review & Approve" : "View Task"
                });
            }

            _logger.LogInformation("Dispatched notifications for {Count} workspace task(s) — {Source} {SourceNo}",
                createdTasks.Count, createdTasks[0].SourceTable, createdTasks[0].SourceNo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to dispatch post-creation notifications for workspace tasks");
            await AuditExceptionAsync(ex, "WorkspaceProcessEngine.NotifyAssignedUsersAsync");
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  PARTY ACTIVITY LOG — Automatic logging for party-involved transactions
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Logs a party_activity_log entry when the source transaction involves a party.
    /// Resolves partyId from source table if not provided.
    /// </summary>
    private async Task LogPartyActivityIfApplicableAsync(
        int? partyId, string? partyName, string sourceTable, long sourceId,
        string? sourceNo, string processCode, string eventTypeCode,
        string title, string? description,
        string? status, decimal? amount, string? createdBy)
    {
        try
        {
            var resolvedPartyId = partyId ?? await ResolvePartyIdFromSourceAsync(sourceTable, sourceId);
            if (!resolvedPartyId.HasValue || resolvedPartyId.Value <= 0) return;

            var activityCode = $"{processCode}_{eventTypeCode}";
            // Ref: map source table → activity type for party activity log
            var activityType = sourceTable switch
            {
                WkSourceTable.Enquiry => WkModuleName.Enquiry,
                WkSourceTable.Quotation => WkModuleName.Quotation,
                WkSourceTable.Job => WkModuleName.Job,
                WkSourceTable.Challan => WkModuleName.Challan,
                WkSourceTable.SalesInvoice => WkModuleName.Invoice,
                WkSourceTable.PurchaseOrder => WkModuleName.Purchase,
                _ => WkModuleName.Workspace
            };

            var log = new PartyActivityLog
            {
                PartyId = resolvedPartyId.Value,
                ActivityType = activityType,
                ActivityCode = activityCode,
                ActivityTitle = title,
                ActivityDescription = description ?? $"{title} — {sourceNo}",
                ReferenceTable = sourceTable,
                ReferenceId = sourceId,
                DocumentNo = sourceNo,
                Status = status ?? "Pending",
                ApprovalStatus = eventTypeCode.Contains("APPROVAL") ? "Pending" : "Not Required",
                Amount = amount,
                CreatedBy = createdBy,
                CreatedOn = DateTime.Now,
                IsActive = true
            };
            _db.PartyActivityLogs.Add(log);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Party activity logged: Party {PartyId}, {ActivityCode} for {SourceTable} #{SourceId}",
                resolvedPartyId.Value, activityCode, sourceTable, sourceId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log party activity for {SourceTable} #{SourceId}", sourceTable, sourceId);
            await AuditExceptionAsync(ex, $"WorkspaceProcessEngine.LogPartyActivityIfApplicableAsync source={sourceTable}:{sourceId}", "Warning");
        }
    }

    /// <summary>
    /// Resolves partyId from the source transaction table.
    /// </summary>
    private async Task<int?> ResolvePartyIdFromSourceAsync(string sourceTable, long sourceId)
    {
        try
        {
            // Ref: resolve party ID from source transaction table
            return sourceTable switch
            {
                WkSourceTable.Enquiry => await _db.TrnEnquiries
                    .Where(e => e.EnquiryId == sourceId)
                    .Select(e => (int?)e.PartyId)
                    .FirstOrDefaultAsync(),
                WkSourceTable.Quotation => await _db.TrnQuotations
                    .Where(q => q.QuotationId == sourceId)
                    .Select(q => (int?)q.PartyId)
                    .FirstOrDefaultAsync(),
                WkSourceTable.Job => await _db.TrnJobs
                    .Where(j => j.JobId == sourceId)
                    .Select(j => j.PartyId)
                    .FirstOrDefaultAsync(),
                WkSourceTable.Challan => await _db.TrnChallans
                    .Where(c => c.ChallanId == sourceId)
                    .Select(c => c.PartyId)
                    .FirstOrDefaultAsync(),
                WkSourceTable.SalesInvoice => await _db.TrnSalesInvoices
                    .Where(i => i.SalesInvoiceId == sourceId)
                    .Select(i => i.PartyId)
                    .FirstOrDefaultAsync(),
                _ => null
            };
        }
        catch (Exception ex)
        {
            await AuditExceptionAsync(ex, $"WorkspaceProcessEngine.ResolvePartyIdFromSourceAsync source={sourceTable}:{sourceId}", "Warning");
            return null;
        }
    }

    // Ref: priority badge color mapping for email notifications
    private static string PriorityColor(string? priority) => priority switch
    {
        WkPriority.Urgent => "#e03131",
        WkPriority.High => "#e67700",
        WkPriority.Normal => "#1971c2",
        WkPriority.Low => "#868e96",
        _ => "#1971c2"
    };

    // ═══════════════════════════════════════════════════════════
    //  AUTO-COMPLETE — Bulk-close tasks when action is performed from main page
    // ═══════════════════════════════════════════════════════════

    /// <inheritdoc />
    public async Task<int> AutoCompleteProcessTasksAsync(
        string sourceTable,
        long sourceId,
        string upToProcessCode,
        string remarks,
        UserSessionData completedBy,
        long? jobId = null)
    {
        try
        {
            // ── 1. Find the target process sequence boundary ──
            var targetProcess = await _db.MstProcesses
                .FirstOrDefaultAsync(p => p.Processcode == upToProcessCode && p.Isactive);

            if (targetProcess == null)
            {
                _logger.LogWarning("AutoComplete: Target process {ProcessCode} not found.", upToProcessCode);
                return 0;
            }

            var maxSequence = targetProcess.Sequenceno;

            // ── 2. Build list of process codes within the sequence boundary ──
            var processCodesInRange = await _db.MstProcesses
                .Where(p => p.Isactive && p.Sequenceno <= maxSequence)
                .Select(p => p.Processcode)
                .ToListAsync();

            // ── 3. Find all pending/in-progress tasks for this source within range ──
            var pendingTasksQuery = _db.TrnWorkspaceTasks
                .Where(t => !t.IsArchived &&
                            (t.TaskStatus == WkTaskStatus.Pending || t.TaskStatus == WkTaskStatus.InProgress) &&
                            processCodesInRange.Contains(t.ProcessCode!));

            // Match by source or by job
            if (jobId.HasValue)
            {
                pendingTasksQuery = pendingTasksQuery.Where(t =>
                    (t.SourceTable == sourceTable && t.SourceId == sourceId) ||
                    t.JobId == jobId.Value);
            }
            else
            {
                pendingTasksQuery = pendingTasksQuery.Where(t =>
                    t.SourceTable == sourceTable && t.SourceId == sourceId);
            }

            var pendingTasks = await pendingTasksQuery.ToListAsync();

            if (pendingTasks.Count == 0)
            {
                _logger.LogInformation("AutoComplete: No pending tasks found for {SourceTable} #{SourceId} up to {ProcessCode}.",
                    sourceTable, sourceId, upToProcessCode);
                return 0;
            }

            // ── 4. Mark each task as completed ──
            var now = DateTime.Now;
            foreach (var task in pendingTasks)
            {
                var oldStatus = task.TaskStatus;
                var newStatus = task.TaskType == WkTaskType.Approval ? WkTaskStatus.Approved : WkTaskStatus.Completed;

                task.TaskStatus = newStatus;
                task.CompletedBy = completedBy.UserId;
                task.CompletedOn = now;
                task.CompletionRemarks = $"[Auto-completed] {remarks}";
                task.ModifiedOn = now;

                // Set started time if it was still PENDING
                if (oldStatus == WkTaskStatus.Pending)
                {
                    task.TaskStatus = newStatus;
                }
            }

            await _db.SaveChangesAsync();

            // ── 5. Log activity for bulk auto-completion ──
            var activity = ActivityLogEntry.FromUser(completedBy, WkModuleName.Workspace, "AUTO_COMPLETE",
                $"Auto-completed {pendingTasks.Count} task(s) for {sourceTable} #{sourceId}");
            activity.EntityType = "WORKSPACE_TASK";
            activity.Description = $"{pendingTasks.Count} workspace task(s) auto-completed up to process '{upToProcessCode}'. Remarks: {remarks}";
            activity.RelatedEntityType = sourceTable switch
            {
                WkSourceTable.Enquiry => WkModuleName.Enquiry,
                WkSourceTable.Quotation => WkModuleName.Quotation,
                WkSourceTable.Job => WkModuleName.Job,
                WkSourceTable.Challan => WkModuleName.Challan,
                WkSourceTable.SalesInvoice => WkModuleName.Invoice,
                _ => WkModuleName.Workspace
            };
            activity.RelatedEntityId = sourceId;
            activity.Severity = "INFO";
            await _activityService.LogActivityAsync(activity);

            // ── 6. Party activity log ──
            var partyId = await ResolvePartyIdFromSourceAsync(sourceTable, sourceId);
            if (partyId.HasValue && partyId.Value > 0)
            {
                var log = new PartyActivityLog
                {
                    PartyId = partyId.Value,
                    ActivityType = activity.RelatedEntityType!,
                    ActivityCode = $"AUTO_COMPLETE_{upToProcessCode}",
                    ActivityTitle = $"Process tasks auto-completed up to {targetProcess.Processname}",
                    ActivityDescription = $"{pendingTasks.Count} task(s) auto-completed. {remarks}",
                    ReferenceTable = sourceTable,
                    ReferenceId = sourceId,
                    Status = "Completed",
                    CreatedBy = completedBy.Name,
                    CreatedOn = now,
                    IsActive = true
                };
                _db.PartyActivityLogs.Add(log);
                await _db.SaveChangesAsync();
            }

            _logger.LogInformation("AutoComplete: Marked {Count} task(s) as completed for {SourceTable} #{SourceId} up to {ProcessCode}.",
                pendingTasks.Count, sourceTable, sourceId, upToProcessCode);

            return pendingTasks.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AutoComplete: Failed to auto-complete tasks for {SourceTable} #{SourceId} up to {ProcessCode}.",
                sourceTable, sourceId, upToProcessCode);
            await AuditExceptionAsync(ex, $"WorkspaceProcessEngine.AutoCompleteProcessTasksAsync source={sourceTable}:{sourceId} upto={upToProcessCode}");
            return 0;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    //  PRE-GENERATED WORKFLOW: Generate ALL tasks upfront at creation time
    // ═══════════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public async Task<Guid?> GenerateAllWorkflowTasksAsync(
        string sourceTable,
        long sourceId,
        string? sourceNo,
        UserSessionData triggeredBy,
        long? jobId = null,
        string? jobNo = null,
        int? jobTypeId = null,
        int? partyId = null,
        string? partyName = null,
        string? actionUrl = null)
    {
        try
        {
            // ── 1. Resolve workflow template based on source and job type ──
            var workflowTemplateId = await ResolveWorkflowTemplateIdForContextAsync(jobId, jobTypeId);
            if (!workflowTemplateId.HasValue)
            {
                _logger.LogWarning("GenerateAllWorkflowTasks: No workflow template found for {SourceTable} #{SourceId}. Using fallback sequential generation.",
                    sourceTable, sourceId);
                return null;
            }

            // ── 2. Load all workflow steps in sequence order ──
            var workflowSteps = await _db.MstWorkflowSteps
                .Include(s => s.Process)
                .Include(s => s.Department)
                .Where(s => s.WorkflowTemplateId == workflowTemplateId.Value && s.IsActive)
                .Where(s => s.ProcessId.HasValue && s.Process != null && s.Process.Isactive)
                .Where(s => !DisabledProcessCodes.Contains(s.Process!.Processcode))
                .OrderBy(s => s.SequenceNo)
                .ToListAsync();

            // ── 2b. Filter steps by source table (bypass enquiry/quotation steps when appropriate) ──
            workflowSteps = FilterStepsBySourceTable(workflowSteps, sourceTable);

            if (workflowSteps.Count == 0)
            {
                _logger.LogWarning("GenerateAllWorkflowTasks: No active workflow steps found for template {TemplateId}.",
                    workflowTemplateId.Value);
                return null;
            }

            // ── 3. Generate batch ID for this workflow instance ──
            var batchId = Guid.NewGuid();
            var now = DateTime.Now;
            var createdTasks = new List<TrnWorkspaceTask>();

            _logger.LogInformation("GenerateAllWorkflowTasks: Creating {Count} tasks for {SourceTable} #{SourceId} with batch {BatchId}.",
                workflowSteps.Count, sourceTable, sourceId, batchId);

            // ── 4. Create tasks for all steps ──
            for (int i = 0; i < workflowSteps.Count; i++)
            {
                var step = workflowSteps[i];
                var process = step.Process!;
                var isFirstStep = (i == 0);

                // Determine task type based on step configuration
                var taskType = step.ApprovalTypeId.HasValue || step.StepType?.Contains("APPROVAL", StringComparison.OrdinalIgnoreCase) == true
                    ? WkTaskType.Approval
                    : WkTaskType.Task;

                // Resolve target users for this step
                var targetUsers = await ResolveTargetUsersForStep(step, triggeredBy, partyId);
                if (targetUsers.Count == 0)
                {
                    // Fall back to triggering user if no specific assignment
                    targetUsers = [triggeredBy.UserId];
                }

                // Calculate SLA and due date
                var slaHours = step.SlaHours ?? 12m;
                var dueDate = now.AddHours((double)(slaHours * (i + 1))); // Stagger due dates

                // Get notification config for labels and templates
                var config = await _db.MstProcessNotificationConfigs
                    .Where(c => c.ProcessCode == process.Processcode && c.IsActive)
                    .FirstOrDefaultAsync();

                var title = config?.EventLabel ?? step.StepName ?? process.Processname;
                var description = config?.BodyTemplate?.Replace("{job_no}", jobNo ?? sourceNo ?? "") 
                                 ?? $"Step {i + 1} of workflow: {step.StepName}";

                // Status: PENDING for first step, QUEUED for others
                var status = isFirstStep ? WkTaskStatus.Pending : WkTaskStatus.Queued;

                foreach (var userId in targetUsers)
                {
                    // Avoid duplicates
                    var exists = await _db.TrnWorkspaceTasks.AnyAsync(t =>
                        t.WorkflowBatchId == batchId &&
                        t.WorkflowStepId == step.WorkflowStepId &&
                        t.UserId == userId);

                    if (exists) continue;

                    var task = new TrnWorkspaceTask
                    {
                        UserId = userId,
                        SourceTable = sourceTable,
                        SourceId = sourceId,
                        SourceNo = sourceNo,
                        TaskType = taskType,
                        TaskStatus = status,
                        Title = title,
                        Description = description,
                        ProcessId = process.Processid,
                        ProcessCode = process.Processcode,
                        DepartmentId = step.DepartmentId ?? process.Departmentid,
                        AssignedBy = triggeredBy.UserId,
                        AssignedOn = isFirstStep ? now : null, // Only assign first task immediately
                        Priority = config?.Priority ?? WkPriority.Normal,
                        DueDate = isFirstStep ? now.AddHours((double)slaHours) : null, // Due date only for active task
                        SlaHours = slaHours,
                        IsOverdue = false,
                        ApprovalTypeId = step.ApprovalTypeId,
                        ApprovalLevel = step.ApprovalLevelId,
                        ActionUrl = actionUrl,
                        JobId = jobId,
                        JobNo = jobNo ?? sourceNo,
                        PartyName = partyName,
                        SequenceNo = step.SequenceNo,
                        WorkflowStepId = step.WorkflowStepId,
                        WorkflowTemplateId = workflowTemplateId.Value,
                        WorkflowBatchId = batchId,
                        // Set IsBlocking from workflow step; party-related tasks (dept 9999) are non-blocking
                        IsBlocking = step.IsBlocking && (step.DepartmentId ?? process.Departmentid) != WkDepartment.PartyDeptId,
                        Metadata = BuildWorkflowMetadata(step, workflowSteps.Count, i + 1),
                        IsRead = false,
                        IsArchived = false,
                        CreatedOn = now
                    };

                    _db.TrnWorkspaceTasks.Add(task);
                    createdTasks.Add(task);
                }
            }

            await _db.SaveChangesAsync();

            // ── 5. Notify users assigned to the first (active) task ──
            var firstTasks = createdTasks.Where(t => t.TaskStatus == WkTaskStatus.Pending).ToList();
            if (firstTasks.Count > 0)
            {
                await NotifyAssignedUsersAsync(firstTasks, triggeredBy);
            }

            // ── 6. Log party activity ──
            if (partyId.HasValue && partyId > 0)
            {
                var firstStep = workflowSteps.FirstOrDefault();
                await LogPartyActivityIfApplicableAsync(
                    partyId, partyName, sourceTable, sourceId, sourceNo,
                    firstStep?.Process?.Processcode ?? "WORKFLOW",
                    WkEventTypeCode.ProcStart,
                    $"Workflow started: {firstStep?.StepName ?? "First step"}",
                    $"{workflowSteps.Count} workflow steps queued for processing.",
                    "Pending", null, triggeredBy.Name);
            }

            _logger.LogInformation("GenerateAllWorkflowTasks: Successfully created {Count} tasks for batch {BatchId}.",
                createdTasks.Count, batchId);

            return batchId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GenerateAllWorkflowTasks: Failed to generate workflow tasks for {SourceTable} #{SourceId}.",
                sourceTable, sourceId);
            await AuditExceptionAsync(ex, $"WorkspaceProcessEngine.GenerateAllWorkflowTasksAsync source={sourceTable}:{sourceId}");
            return null;
        }
    }

    /// <inheritdoc />
    public async Task ActivateNextQueuedTaskAsync(TrnWorkspaceTask completedTask, UserSessionData completedBy)
    {
        try
        {
            // Only process tasks that are part of a pre-generated workflow
            if (!completedTask.WorkflowBatchId.HasValue || !completedTask.SequenceNo.HasValue)
            {
                // Fall back to legacy behavior for ad-hoc tasks
                await GenerateNextStepTasksAsync(completedTask, completedBy);
                return;
            }

            var batchId = completedTask.WorkflowBatchId.Value;
            var currentSequence = completedTask.SequenceNo.Value;

            // ══════════════════════════════════════════════════════════════════════════
            // NON-BLOCKING TASK LOGIC:
            // Only BLOCKING tasks prevent workflow progression. Non-blocking tasks
            // (like party-related tasks) can remain pending while workflow continues.
            // ══════════════════════════════════════════════════════════════════════════

            // Check if there are any BLOCKING siblings at the same sequence still pending
            var pendingBlockingSiblings = await _db.TrnWorkspaceTasks
                .AnyAsync(t => t.WorkflowBatchId == batchId &&
                              t.SequenceNo == currentSequence &&
                              t.IsBlocking && // Only check blocking tasks
                              t.TaskStatus != WkTaskStatus.Completed &&
                              t.TaskStatus != WkTaskStatus.Approved &&
                              t.TaskStatus != WkTaskStatus.Cancelled &&
                              t.TaskStatus != WkTaskStatus.Rejected);

            if (pendingBlockingSiblings)
            {
                _logger.LogInformation("ActivateNextQueued: BLOCKING sibling tasks still pending at sequence {Seq} for batch {BatchId}. Waiting.",
                    currentSequence, batchId);
                return;
            }

            // Log if non-blocking tasks are being bypassed
            var pendingNonBlockingSiblings = await _db.TrnWorkspaceTasks
                .CountAsync(t => t.WorkflowBatchId == batchId &&
                                t.SequenceNo == currentSequence &&
                                !t.IsBlocking &&
                                t.TaskStatus != WkTaskStatus.Completed &&
                                t.TaskStatus != WkTaskStatus.Approved &&
                                t.TaskStatus != WkTaskStatus.Cancelled &&
                                t.TaskStatus != WkTaskStatus.Rejected);

            if (pendingNonBlockingSiblings > 0)
            {
                _logger.LogInformation("ActivateNextQueued: {Count} non-blocking task(s) still pending at sequence {Seq} for batch {BatchId}. Proceeding to next step.",
                    pendingNonBlockingSiblings, currentSequence, batchId);
            }

            // ── Multi-item job: after JOB_APPROVAL, generate per-item production workflows ──
            if (string.Equals(completedTask.ProcessCode, WkProcessCode.JobApproval, StringComparison.OrdinalIgnoreCase) &&
                completedTask.JobId.HasValue)
            {
                var itemCount = await _db.TrnJobItems
                    .CountAsync(ji => ji.JobId == completedTask.JobId.Value);

                if (itemCount > 1)
                {
                    _logger.LogInformation("ActivateNextQueued: Job {JobId} has {Count} item(s) — cancelling remaining queued tasks and generating per-item workflows.",
                        completedTask.JobId.Value, itemCount);

                    var remainingQueued = await _db.TrnWorkspaceTasks
                        .Where(t => t.WorkflowBatchId == batchId &&
                                   t.SequenceNo > currentSequence &&
                                   t.TaskStatus == WkTaskStatus.Queued)
                        .ToListAsync();

                    var cancelNow = DateTime.Now;
                    foreach (var qt in remainingQueued)
                    {
                        qt.TaskStatus = WkTaskStatus.Cancelled;
                        qt.CompletionRemarks = "Superseded by per-item workflow batches.";
                        qt.ModifiedOn = cancelNow;
                    }
                    await _db.SaveChangesAsync();

                    await GenerateItemScopedWorkflowsAsync(completedTask, completedBy);
                    return;
                }
            }

            // Find the next QUEUED task(s) in sequence
            var nextSequence = await _db.TrnWorkspaceTasks
                .Where(t => t.WorkflowBatchId == batchId &&
                           t.SequenceNo > currentSequence &&
                           t.TaskStatus == WkTaskStatus.Queued)
                .OrderBy(t => t.SequenceNo)
                .Select(t => t.SequenceNo)
                .FirstOrDefaultAsync();

            if (!nextSequence.HasValue)
            {
                _logger.LogInformation("ActivateNextQueued: No more QUEUED tasks for batch {BatchId}. Workflow complete.",
                    batchId);
                return;
            }

            // Activate all tasks at this sequence level
            var tasksToActivate = await _db.TrnWorkspaceTasks
                .Where(t => t.WorkflowBatchId == batchId &&
                           t.SequenceNo == nextSequence.Value &&
                           t.TaskStatus == WkTaskStatus.Queued)
                .ToListAsync();

            var now = DateTime.Now;
            foreach (var task in tasksToActivate)
            {
                task.TaskStatus = WkTaskStatus.Pending;
                task.AssignedOn = now;
                task.DueDate = now.AddHours((double)(task.SlaHours ?? 12));
                task.ModifiedOn = now;
            }

            await _db.SaveChangesAsync();

            // Notify newly assigned users
            await NotifyAssignedUsersAsync(tasksToActivate, completedBy);

            _logger.LogInformation("ActivateNextQueued: Activated {Count} task(s) at sequence {Seq} for batch {BatchId}.",
                tasksToActivate.Count, nextSequence.Value, batchId);

            // Log party activity if applicable
            var partyId = await ResolvePartyIdFromSourceAsync(completedTask.SourceTable, completedTask.SourceId);
            if (partyId.HasValue)
            {
                var firstTask = tasksToActivate.FirstOrDefault();
                await LogPartyActivityIfApplicableAsync(
                    partyId, completedTask.PartyName, completedTask.SourceTable, completedTask.SourceId, completedTask.SourceNo,
                    firstTask?.ProcessCode ?? "WORKFLOW",
                    WkEventTypeCode.ProcStart,
                    $"Next step activated: {firstTask?.Title ?? "Task"}",
                    $"Step {nextSequence.Value} of workflow is now active.",
                    "Pending", null, completedBy.Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ActivateNextQueued: Failed to activate next queued task for batch {BatchId}.",
                completedTask.WorkflowBatchId);
            await AuditExceptionAsync(ex, $"WorkspaceProcessEngine.ActivateNextQueuedTaskAsync batch={completedTask.WorkflowBatchId}");
        }
    }

    /// <summary>
    /// Resolve target users for a specific workflow step.
    /// </summary>
    private async Task<List<long>> ResolveTargetUsersForStep(MstWorkflowStep step, UserSessionData triggeredBy, int? partyId)
    {
        var users = new List<long>();

        // Priority 1: Direct user assignment on step
        if (step.AssignedUserId.HasValue && step.AssignedUserId > 0)
        {
            users.Add(step.AssignedUserId.Value);
            return users;
        }

        // Priority 2: Party-related process (dept 9999) routes to party workspace users
        if ((step.DepartmentId ?? 0) == WkDepartment.PartyDeptId && partyId.HasValue)
        {
            return await ResolvePartyTargetUsersAsync(partyId);
        }

        // Priority 3: Use process-based role routing
        if (step.Process != null)
        {
            users = await ResolveTargetUsers(step.Process.Processcode, triggeredBy);
        }

        // Priority 4: Department-based assignment
        if (users.Count == 0 && step.DepartmentId.HasValue)
        {
            users = await _db.MstUsers
                .Where(u => u.Departmentid == step.DepartmentId.Value && u.Isactive == true)
                .OrderBy(u => u.Userid)
                .Take(5)
                .Select(u => u.Userid)
                .ToListAsync();
        }

        return users;
    }

    /// <summary>
    /// Build metadata JSON for workflow task tracking.
    /// </summary>
    private static string BuildWorkflowMetadata(MstWorkflowStep step, int totalSteps, int currentStep)
    {
        return System.Text.Json.JsonSerializer.Serialize(new
        {
            workflow_step_id = step.WorkflowStepId,
            step_code = step.StepCode,
            step_name = step.StepName,
            step_type = step.StepType,
            total_steps = totalSteps,
            current_step = currentStep,
            is_mandatory = step.IsMandatory,
            is_blocking = step.IsBlocking,
            escalate_after_hours = step.EscalateAfterHours,
            escalate_to = step.EscalateTo,
            notify_customer = step.NotifyCustomer,
            notify_vendor = step.NotifyVendor,
            notify_supplier = step.NotifySupplier
        });
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    //  SOURCE-AWARE WORKFLOW FILTERING — Skip steps based on entry point
    // ═══════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Filters workflow steps based on the source table to implement entry-point-aware workflows:
    ///   - Direct Job creation: Skip enquiry and quotation steps (start from JOB_CREATE)
    ///   - Direct Quotation creation: Skip enquiry steps (start from QUOT)
    ///   - Enquiry-based: Include all steps
    /// Uses step-level AppliesToEnquiry, AppliesToQuotation, AppliesToJob flags.
    /// </summary>
    private static List<MstWorkflowStep> FilterStepsBySourceTable(List<MstWorkflowStep> allSteps, string sourceTable)
    {
        // Determine which applicability filter to use based on source table
        return sourceTable.ToUpperInvariant() switch
        {
            "TRN_JOB" => allSteps.Where(s => s.AppliesToJob).ToList(),
            "TRN_QUOTATION" => allSteps.Where(s => s.AppliesToQuotation).ToList(),
            "TRN_ENQUIRY" => allSteps.Where(s => s.AppliesToEnquiry).ToList(),
            _ => allSteps // Default: include all steps for other sources
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    //  PER-ITEM WORKFLOW GENERATION — Separate workflow per job item after approval
    // ═══════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// After JOB_APPROVAL completes for a multi-item job, generates a separate production
    /// workflow batch for each job item based on that item's product type and job type.
    /// Job creation and job approval remain as a single shared step; this method handles
    /// the subsequent production steps only.
    /// </summary>
    private async Task GenerateItemScopedWorkflowsAsync(TrnWorkspaceTask approvedTask, UserSessionData triggeredBy)
    {
        if (!approvedTask.JobId.HasValue) return;

        var jobId = approvedTask.JobId.Value;

        var jobItems = await _db.TrnJobItems
            .Where(ji => ji.JobId == jobId)
            .OrderBy(ji => ji.ItemSequence)
            .Select(ji => new
            {
                ji.JobItemId,
                ji.ItemSequence,
                ji.ProductName,
                ji.PrintProductTypeId,
                ji.JobTypeId
            })
            .ToListAsync();

        if (jobItems.Count <= 1)
        {
            _logger.LogInformation("GenerateItemScopedWorkflows: Job {JobId} has only {Count} item(s) — using standard single-batch flow.",
                jobId, jobItems.Count);
            return;
        }

        var job = await _db.TrnJobs
            .Where(j => j.JobId == jobId)
            .Select(j => new { j.JobTypeId, j.JobNo, j.PartyId, PartyName = j.Party != null ? j.Party.Name : null })
            .FirstOrDefaultAsync();

        var jobNo = job?.JobNo ?? approvedTask.JobNo;
        var jobTypeId = job?.JobTypeId;
        var partyId = job?.PartyId;
        var partyName = job?.PartyName ?? approvedTask.PartyName;

        _logger.LogInformation("GenerateItemScopedWorkflows: Job {JobId} has {Count} items — generating separate workflow batches per item.",
            jobId, jobItems.Count);

        foreach (var item in jobItems)
        {
            var itemJobTypeId = item.JobTypeId ?? jobTypeId;
            var workflowTemplateId = await ResolveWorkflowTemplateIdForItemAsync(itemJobTypeId, item.PrintProductTypeId);

            if (!workflowTemplateId.HasValue)
            {
                _logger.LogWarning("GenerateItemScopedWorkflows: No workflow template for item {ItemId} (ProductType: {ProdType}, JobType: {JobType}). Skipping.",
                    item.JobItemId, item.PrintProductTypeId, itemJobTypeId);
                continue;
            }

            await GenerateItemWorkflowBatchAsync(
                workflowTemplateId.Value,
                approvedTask,
                item.JobItemId,
                item.ItemSequence,
                item.ProductName ?? $"Item #{item.ItemSequence}",
                jobId,
                jobNo,
                itemJobTypeId,
                partyId,
                partyName,
                triggeredBy);
        }
    }

    /// <summary>
    /// Creates a workflow batch for a single job item, covering production steps only
    /// (JOB_CREATE and JOB_APPROVAL are excluded — they are shared across all items).
    /// Task titles are prefixed with the item name for easy identification.
    /// </summary>
    private async Task<Guid?> GenerateItemWorkflowBatchAsync(
        long workflowTemplateId,
        TrnWorkspaceTask sourceTask,
        long jobItemId,
        int itemSequence,
        string itemName,
        long jobId,
        string? jobNo,
        int? jobTypeId,
        int? partyId,
        string? partyName,
        UserSessionData triggeredBy)
    {
        try
        {
            // Production steps only — skip all pre-job and approval steps
            var excludedCodes = WkProcessCode.PreJobProcesses
                .Concat([WkProcessCode.JobApproval])
                .ToArray();

            var workflowSteps = await _db.MstWorkflowSteps
                .Include(s => s.Process)
                .Where(s => s.WorkflowTemplateId == workflowTemplateId && s.IsActive)
                .Where(s => s.ProcessId.HasValue && s.Process != null && s.Process.Isactive)
                .Where(s => !DisabledProcessCodes.Contains(s.Process!.Processcode))
                .Where(s => !excludedCodes.Contains(s.Process!.Processcode))
                .Where(s => s.AppliesToJob)
                .OrderBy(s => s.SequenceNo)
                .ToListAsync();

            if (workflowSteps.Count == 0)
            {
                _logger.LogWarning("GenerateItemWorkflowBatch: No production steps found for template {TemplateId}, item {ItemId}.",
                    workflowTemplateId, jobItemId);
                return null;
            }

            var batchId = Guid.NewGuid();
            var now = DateTime.Now;
            var createdTasks = new List<TrnWorkspaceTask>();

            _logger.LogInformation("GenerateItemWorkflowBatch: Creating {Count} tasks for [{ItemName}] (item {ItemId}) with batch {BatchId}.",
                workflowSteps.Count, itemName, jobItemId, batchId);

            for (int i = 0; i < workflowSteps.Count; i++)
            {
                var step = workflowSteps[i];
                var process = step.Process!;
                var isFirstStep = (i == 0);

                var taskType = step.ApprovalTypeId.HasValue ||
                               step.StepType?.Contains("APPROVAL", StringComparison.OrdinalIgnoreCase) == true
                    ? WkTaskType.Approval
                    : WkTaskType.Task;

                var targetUsers = await ResolveTargetUsersForStep(step, triggeredBy, partyId);
                if (targetUsers.Count == 0)
                    targetUsers = [triggeredBy.UserId];

                var slaHours = step.SlaHours ?? 12m;

                var config = await _db.MstProcessNotificationConfigs
                    .Where(c => c.ProcessCode == process.Processcode && c.IsActive)
                    .Where(c =>
                        (jobTypeId.HasValue && c.JobTypeId == jobTypeId.Value) ||
                        c.JobTypeCode == "ALL")
                    .OrderByDescending(c => jobTypeId.HasValue && c.JobTypeId == jobTypeId.Value)
                    .FirstOrDefaultAsync();

                var baseTitle = config?.EventLabel ?? step.StepName ?? process.Processname;
                var title = $"[{itemName}] {baseTitle}";
                var description = config?.BodyTemplate?.Replace("{job_no}", jobNo ?? sourceTask.SourceNo ?? "")
                                  ?? $"[{itemName}] {step.StepName ?? process.Processname} for job {jobNo}";

                var status = isFirstStep ? WkTaskStatus.Pending : WkTaskStatus.Queued;

                foreach (var userId in targetUsers)
                {
                    var exists = await _db.TrnWorkspaceTasks.AnyAsync(t =>
                        t.WorkflowBatchId == batchId &&
                        t.WorkflowStepId == step.WorkflowStepId &&
                        t.UserId == userId);

                    if (exists) continue;

                    var task = new TrnWorkspaceTask
                    {
                        UserId = userId,
                        SourceTable = sourceTask.SourceTable,
                        SourceId = sourceTask.SourceId,
                        SourceNo = sourceTask.SourceNo,
                        TaskType = taskType,
                        TaskStatus = status,
                        Title = title,
                        Description = description,
                        ProcessId = process.Processid,
                        ProcessCode = process.Processcode,
                        DepartmentId = step.DepartmentId ?? process.Departmentid,
                        AssignedBy = triggeredBy.UserId,
                        AssignedOn = isFirstStep ? now : null,
                        Priority = config?.Priority ?? sourceTask.Priority ?? WkPriority.Normal,
                        DueDate = isFirstStep ? now.AddHours((double)slaHours) : null,
                        SlaHours = slaHours,
                        IsOverdue = false,
                        ApprovalTypeId = step.ApprovalTypeId,
                        ApprovalLevel = step.ApprovalLevelId,
                        ActionUrl = sourceTask.ActionUrl,
                        JobId = jobId,
                        JobNo = jobNo,
                        PartyName = partyName,
                        SequenceNo = step.SequenceNo,
                        WorkflowStepId = step.WorkflowStepId,
                        WorkflowTemplateId = workflowTemplateId,
                        WorkflowBatchId = batchId,
                        IsBlocking = step.IsBlocking &&
                                     (step.DepartmentId ?? process.Departmentid) != WkDepartment.PartyDeptId,
                        Metadata = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            workflow_step_id = step.WorkflowStepId,
                            step_code = step.StepCode,
                            step_name = step.StepName,
                            total_steps = workflowSteps.Count,
                            current_step = i + 1,
                            job_item_id = jobItemId,
                            item_name = itemName,
                            item_sequence = itemSequence
                        }),
                        IsRead = false,
                        IsArchived = false,
                        CreatedOn = now
                    };

                    _db.TrnWorkspaceTasks.Add(task);
                    createdTasks.Add(task);
                }
            }

            await _db.SaveChangesAsync();

            var firstTasks = createdTasks.Where(t => t.TaskStatus == WkTaskStatus.Pending).ToList();
            if (firstTasks.Count > 0)
                await NotifyAssignedUsersAsync(firstTasks, triggeredBy);

            _logger.LogInformation("GenerateItemWorkflowBatch: Created {Count} task(s) for [{ItemName}] batch {BatchId}.",
                createdTasks.Count, itemName, batchId);

            return batchId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GenerateItemWorkflowBatch: Failed for item {ItemId} in template {TemplateId}.",
                jobItemId, workflowTemplateId);
            await AuditExceptionAsync(ex, $"WorkspaceProcessEngine.GenerateItemWorkflowBatchAsync item={jobItemId}");
            return null;
        }
    }

    /// <summary>
    /// Resolves the best-matching workflow template for a specific job item,
    /// using the item's product type and job type for precise matching.
    /// </summary>
    private async Task<long?> ResolveWorkflowTemplateIdForItemAsync(int? jobTypeId, int? productTypeId)
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
            .Select(t => (long?)t.WorkflowTemplateId)
            .FirstOrDefaultAsync();

        return template;
    }

    private async Task AuditExceptionAsync(Exception ex, string additionalData, string severity = "Error")
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return;
        }

        await _systemErrorLogger.LogAsync(
            ex,
            httpContext,
            severity: severity,
            additionalData: additionalData);
    }
}
