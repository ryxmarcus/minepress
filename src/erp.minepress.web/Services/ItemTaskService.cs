using erp.minepress.domain.Enums;
using erp.minepress.persistence.Context;
using erp.minepress.persistence.Models;
using erp.minepress.web.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace erp.minepress.web.Services;

/// <inheritdoc />
public class ItemTaskService : IItemTaskService
{
    private sealed class ItemTaskSeed
    {
        public required long JobItemId { get; init; }
        public required int ItemSequence { get; init; }
        public required string ItemName { get; init; }
        public string? ItemDescription { get; init; }
        public string? WorkData { get; init; }
        public required string UpstreamKey { get; init; }
    }

    private readonly ApplicationDbContext _db;
    private readonly IWorkspaceProcessEngine _workspaceEngine;
    private readonly ILogger<ItemTaskService> _logger;

    public ItemTaskService(
        ApplicationDbContext db,
        IWorkspaceProcessEngine workspaceEngine,
        ILogger<ItemTaskService> logger)
    {
        _db = db;
        _workspaceEngine = workspaceEngine;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task CreateItemTasksAsync(TrnWorkspaceTask workspaceTask)
    {
        if (!WkParallelProcessCodes.Eligible.Contains(workspaceTask.ProcessCode))
            return;

        if (!workspaceTask.JobId.HasValue)
        {
            _logger.LogWarning("Task {TaskId}: Cannot create item tasks — no JobId.", workspaceTask.WorkspaceTaskId);
            return;
        }

        // Check if item tasks already exist for this workspace task
        var existing = await _db.TrnWorkspaceTaskItems
            .AnyAsync(ti => ti.WorkspaceTaskId == workspaceTask.WorkspaceTaskId);
        if (existing) return;

        var jobItems = await _db.TrnJobItems
            .Include(ji => ji.RateCalculator)
            .Where(ji => ji.JobId == workspaceTask.JobId.Value)
            .OrderBy(ji => ji.ItemSequence)
            .Select(ji => new
            {
                ji.JobItemId,
                ji.ItemSequence,
                ji.ProductName,
                ji.ProductDescription,
                RateCalcConfigData = ji.RateCalculator != null ? ji.RateCalculator.ConfigData : null
            })
            .ToListAsync();

        if (jobItems.Count == 0)
        {
            _logger.LogInformation("Task {TaskId}: No job items found for Job {JobId}.",
                workspaceTask.WorkspaceTaskId, workspaceTask.JobId);
            return;
        }

        // Look for upstream item tasks to link parent_task_item_id (e.g. Design → CTP)
        var upstreamProcess = WkParallelProcessCodes.NextProcessPerItem
            .Where(kvp => kvp.Value == workspaceTask.ProcessCode)
            .Select(kvp => kvp.Key)
            .FirstOrDefault();

        Dictionary<string, long>? upstreamItemMap = null;
        if (upstreamProcess != null)
        {
            var upstreamItems = await _db.TrnWorkspaceTaskItems
                .Where(ti => ti.JobId == workspaceTask.JobId.Value
                    && ti.ProcessCode == upstreamProcess
                    && ti.TaskStatus == WkItemTaskStatus.Completed)
                .Select(ti => new { ti.JobItemId, ti.ItemName, ti.TaskItemId })
                .ToListAsync();

            upstreamItemMap = upstreamItems
                .GroupBy(x => BuildUpstreamKey(x.JobItemId, x.ItemName))
                .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.TaskItemId).First().TaskItemId);
        }

        var processName = await _db.MstProcesses
            .Where(p => p.Processcode == workspaceTask.ProcessCode && p.Isactive)
            .Select(p => p.Processname)
            .FirstOrDefaultAsync();

        var now = DateTime.Now;
        var itemSeeds = new List<ItemTaskSeed>();
        foreach (var item in jobItems)
        {
            var fromConfig = BuildSeedsFromRateCalcConfig(
                item.JobItemId,
                item.ItemSequence,
                item.ProductName,
                item.ProductDescription,
                item.RateCalcConfigData);

            if (fromConfig.Count > 0)
            {
                itemSeeds.AddRange(fromConfig);
                continue;
            }

            itemSeeds.Add(new ItemTaskSeed
            {
                JobItemId = item.JobItemId,
                ItemSequence = item.ItemSequence,
                ItemName = item.ProductName ?? $"Item #{item.ItemSequence}",
                ItemDescription = item.ProductDescription,
                WorkData = null,
                UpstreamKey = BuildUpstreamKey(item.JobItemId, item.ProductName)
            });
        }

        foreach (var seed in itemSeeds)
        {
            long? parentId = null;
            if (upstreamItemMap != null && upstreamItemMap.TryGetValue(seed.UpstreamKey, out var foundParentId))
                parentId = foundParentId;

            var taskItem = new TrnWorkspaceTaskItem
            {
                WorkspaceTaskId = workspaceTask.WorkspaceTaskId,
                JobId = workspaceTask.JobId.Value,
                JobItemId = seed.JobItemId,
                ProcessCode = workspaceTask.ProcessCode,
                ProcessName = processName,
                ItemName = seed.ItemName,
                ItemDescription = seed.ItemDescription,
                ItemSequence = seed.ItemSequence,
                TaskStatus = WkItemTaskStatus.NotStarted,
                AssignedUserId = workspaceTask.UserId,
                AssignedOn = now,
                CreatedOn = now,
                WorkData = seed.WorkData,
                ParentTaskItemId = parentId
            };
            _db.TrnWorkspaceTaskItems.Add(taskItem);
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("Task {TaskId}: Created {Count} item tasks for process {Process}.",
            workspaceTask.WorkspaceTaskId, itemSeeds.Count, workspaceTask.ProcessCode);
    }

    /// <inheritdoc />
    public async Task<List<TrnWorkspaceTaskItem>> GetItemTasksAsync(long workspaceTaskId)
    {
        return await _db.TrnWorkspaceTaskItems
            .Where(ti => ti.WorkspaceTaskId == workspaceTaskId)
            .OrderBy(ti => ti.ItemSequence)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task StartItemTaskAsync(long taskItemId, UserSessionData user)
    {
        var item = await _db.TrnWorkspaceTaskItems.FindAsync(taskItemId);
        if (item == null) return;

        item.TaskStatus = WkItemTaskStatus.Running;
        item.StartedOn = DateTime.Now;
        item.StartedBy = user.UserId;
        item.ModifiedOn = DateTime.Now;
        await _db.SaveChangesAsync();

        _logger.LogInformation("ItemTask {ItemId}: Started by user {UserId}.", taskItemId, user.UserId);
    }

    /// <inheritdoc />
    public async Task CompleteItemTaskAsync(long taskItemId, UserSessionData user, string? remarks = null)
    {
        var item = await _db.TrnWorkspaceTaskItems.FindAsync(taskItemId);
        if (item == null) return;

        item.TaskStatus = WkItemTaskStatus.Completed;
        item.CompletedOn = DateTime.Now;
        item.CompletedBy = user.UserId;
        item.Remarks = remarks;
        item.ModifiedOn = DateTime.Now;
        await _db.SaveChangesAsync();

        _logger.LogInformation("ItemTask {ItemId}: Completed by user {UserId}.", taskItemId, user.UserId);

        // ── Trigger next process for this specific item ──
        await TriggerNextProcessForItemAsync(item, user);

        // ── Check if all items are done → auto-complete parent ──
        await CheckAndAutoCompleteParentAsync(item.WorkspaceTaskId, user);
    }

    /// <inheritdoc />
    public async Task<bool> CheckAndAutoCompleteParentAsync(long workspaceTaskId, UserSessionData user)
    {
        var allDone = !await _db.TrnWorkspaceTaskItems
            .AnyAsync(ti => ti.WorkspaceTaskId == workspaceTaskId
                && ti.TaskStatus != WkItemTaskStatus.Completed
                && ti.TaskStatus != WkItemTaskStatus.Closed);

        if (!allDone) return false;

        var task = await _db.TrnWorkspaceTasks.FindAsync(workspaceTaskId);
        if (task == null || task.TaskStatus == WkTaskStatus.Completed) return false;

        task.TaskStatus = WkTaskStatus.Completed;
        task.CompletedBy = user.UserId;
        task.CompletedOn = DateTime.Now;
        task.CompletionRemarks = "Auto-completed: all item tasks finished.";
        task.ModifiedOn = DateTime.Now;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Task {TaskId}: Auto-completed — all item tasks done.", workspaceTaskId);

        // Generate next step tasks (workflow-level progression)
        await _workspaceEngine.GenerateNextStepTasksAsync(task, user);

        return true;
    }

    /// <summary>
    /// When an item task completes in a parallel-eligible process,
    /// find or create the downstream workspace task for the next process
    /// and create an item task for this specific job item.
    /// </summary>
    private async Task TriggerNextProcessForItemAsync(TrnWorkspaceTaskItem completedItem, UserSessionData user)
    {
        if (!WkParallelProcessCodes.NextProcessPerItem.TryGetValue(completedItem.ProcessCode, out var nextProcessCode))
            return;

        // Find existing workspace task for the next process + same job
        var parentTask = await _db.TrnWorkspaceTasks.FindAsync(completedItem.WorkspaceTaskId);
        if (parentTask == null) return;

        var nextTask = await _db.TrnWorkspaceTasks
            .FirstOrDefaultAsync(t => t.ProcessCode == nextProcessCode
                && t.JobId == completedItem.JobId
                && !t.IsArchived
                && t.TaskStatus != WkTaskStatus.Cancelled
                && t.TaskStatus != WkTaskStatus.Rejected);

        if (nextTask == null)
        {
            // Create the workspace task for next process via engine
            await _workspaceEngine.CreateWorkspaceTaskAsync(
                processCode: nextProcessCode,
                eventTypeCode: WkEventTypeCode.ProcStart,
                sourceTable: parentTask.SourceTable,
                sourceId: parentTask.SourceId,
                sourceNo: parentTask.SourceNo,
                title: $"{nextProcessCode} — {parentTask.JobNo ?? parentTask.SourceNo}",
                description: $"Item-level parallel task triggered from {completedItem.ProcessCode}.",
                taskType: WkTaskType.Task,
                priority: parentTask.Priority ?? WkPriority.Normal,
                triggeredBy: user,
                jobId: parentTask.JobId,
                jobNo: parentTask.JobNo,
                partyName: parentTask.PartyName,
                actionUrl: parentTask.ActionUrl);

            // Re-fetch the just-created task
            nextTask = await _db.TrnWorkspaceTasks
                .OrderByDescending(t => t.WorkspaceTaskId)
                .FirstOrDefaultAsync(t => t.ProcessCode == nextProcessCode
                    && t.JobId == completedItem.JobId
                    && !t.IsArchived);
        }

        if (nextTask == null)
        {
            _logger.LogWarning("ItemTask {ItemId}: Could not find/create next task for {Process}.",
                completedItem.TaskItemId, nextProcessCode);
            return;
        }

        // Check if item task already exists for this job item in the next process task
        var exists = await _db.TrnWorkspaceTaskItems
            .AnyAsync(ti => ti.WorkspaceTaskId == nextTask.WorkspaceTaskId
                && ti.JobItemId == completedItem.JobItemId
                && ti.ItemName == completedItem.ItemName);
        if (exists) return;

        var processName = await _db.MstProcesses
            .Where(p => p.Processcode == nextProcessCode && p.Isactive)
            .Select(p => p.Processname)
            .FirstOrDefaultAsync();

        var nextItem = new TrnWorkspaceTaskItem
        {
            WorkspaceTaskId = nextTask.WorkspaceTaskId,
            JobId = completedItem.JobId,
            JobItemId = completedItem.JobItemId,
            ProcessCode = nextProcessCode,
            ProcessName = processName,
            ItemName = completedItem.ItemName,
            ItemDescription = completedItem.ItemDescription,
            ItemSequence = completedItem.ItemSequence,
            TaskStatus = WkItemTaskStatus.NotStarted,
            AssignedUserId = nextTask.UserId,
            AssignedOn = DateTime.Now,
            CreatedOn = DateTime.Now,
            WorkData = completedItem.WorkData,
            ParentTaskItemId = completedItem.TaskItemId
        };
        _db.TrnWorkspaceTaskItems.Add(nextItem);
        await _db.SaveChangesAsync();

        _logger.LogInformation("ItemTask {ItemId}: Triggered {Process} item task for JobItem {JobItemId}.",
            completedItem.TaskItemId, nextProcessCode, completedItem.JobItemId);
    }

    private List<ItemTaskSeed> BuildSeedsFromRateCalcConfig(
        long jobItemId,
        int baseSequence,
        string? fallbackName,
        string? fallbackDescription,
        string? configData)
    {
        var result = new List<ItemTaskSeed>();
        if (string.IsNullOrWhiteSpace(configData)) return result;

        try
        {
            using var doc = JsonDocument.Parse(configData);
            if (!doc.RootElement.TryGetProperty("productParts", out var parts) || parts.ValueKind != JsonValueKind.Array)
                return result;

            var index = 0;
            foreach (var part in parts.EnumerateArray())
            {
                index++;
                var partName = part.TryGetProperty("partName", out var partNameEl)
                    ? (partNameEl.GetString() ?? string.Empty).Trim()
                    : string.Empty;

                if (string.IsNullOrWhiteSpace(partName))
                    partName = $"{fallbackName ?? "Part"} #{index}";

                var partDesc = BuildPartDescription(part, fallbackDescription);
                var sequence = (baseSequence * 100) + index;

                result.Add(new ItemTaskSeed
                {
                    JobItemId = jobItemId,
                    ItemSequence = sequence,
                    ItemName = partName,
                    ItemDescription = partDesc,
                    WorkData = part.GetRawText(),
                    UpstreamKey = BuildUpstreamKey(jobItemId, partName)
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not parse RateCalculator config_data for JobItem {JobItemId}.", jobItemId);
        }

        return result;
    }

    private static string? BuildPartDescription(JsonElement part, string? fallbackDescription)
    {
        if (!part.TryGetProperty("specification", out var spec) || spec.ValueKind != JsonValueKind.Object)
            return fallbackDescription;

        int? pages = null;
        int? color = null;
        string? paper = null;

        if (spec.TryGetProperty("pages", out var pagesEl) && pagesEl.ValueKind == JsonValueKind.Number && pagesEl.TryGetInt32(out var p))
            pages = p;

        if (spec.TryGetProperty("color", out var colorEl) && colorEl.ValueKind == JsonValueKind.Number && colorEl.TryGetInt32(out var c))
            color = c;

        if (spec.TryGetProperty("paper", out var paperEl) && paperEl.ValueKind == JsonValueKind.String)
            paper = paperEl.GetString();

        var bits = new List<string>();
        if (pages.HasValue) bits.Add($"Pages: {pages.Value}");
        if (color.HasValue) bits.Add($"Color: {color.Value}");
        if (!string.IsNullOrWhiteSpace(paper)) bits.Add($"Paper: {paper}");

        if (bits.Count == 0) return fallbackDescription;
        if (string.IsNullOrWhiteSpace(fallbackDescription)) return string.Join(" | ", bits);
        return $"{fallbackDescription} | {string.Join(" | ", bits)}";
    }

    private static string BuildUpstreamKey(long jobItemId, string? itemName)
        => $"{jobItemId}|{(itemName ?? string.Empty).Trim().ToUpperInvariant()}";
}
