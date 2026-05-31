using erp.minepress.persistence.Models;
using erp.minepress.web.Helpers;

namespace erp.minepress.web.Services;

/// <summary>
/// Manages item-level parallel task execution for workspace tasks.
/// Creates, starts, and completes per-item sub-tasks and triggers
/// the dependency chain (Design → CTP → PostPress) per item independently.
/// </summary>
public interface IItemTaskService
{
    /// <summary>
    /// Creates item-level sub-tasks for a workspace task by reading job items.
    /// Only creates items for parallel-eligible process codes (DES_DTP, PRE_PRESS, POST_PRESS).
    /// </summary>
    Task CreateItemTasksAsync(TrnWorkspaceTask workspaceTask);

    /// <summary>
    /// Returns all item tasks for a given workspace task.
    /// </summary>
    Task<List<TrnWorkspaceTaskItem>> GetItemTasksAsync(long workspaceTaskId);

    /// <summary>
    /// Starts an individual item task (sets status to RUNNING).
    /// </summary>
    Task StartItemTaskAsync(long taskItemId, UserSessionData user);

    /// <summary>
    /// Completes an individual item task. If all sibling items are done,
    /// auto-completes the parent workspace task.
    /// Also triggers the next process item task per the dependency chain.
    /// </summary>
    Task CompleteItemTaskAsync(long taskItemId, UserSessionData user, string? remarks = null);

    /// <summary>
    /// Checks if all item tasks for the parent workspace task are completed
    /// and auto-completes the parent if so.
    /// </summary>
    Task<bool> CheckAndAutoCompleteParentAsync(long workspaceTaskId, UserSessionData user);
}
