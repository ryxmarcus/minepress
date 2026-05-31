using erp.minepress.persistence.Models;
using erp.minepress.web.Helpers;

namespace erp.minepress.web.Services;

/// <summary>
/// Central workspace engine: creates tasks/approvals and dispatches notifications
/// using role-based routing via mst_process_role_map → map_user_role → mst_process_department_map.
/// Also logs party_activity_log entries automatically when partyId is provided.
/// </summary>
public interface IWorkspaceProcessEngine
{
    /// <summary>
    /// Central method: creates workspace tasks for the given process event.
    /// Resolves target users dynamically via:
    ///   ProcessCode → mst_process_role_map → map_user_role → mst_user filtered by mst_process_department_map.
    /// Looks up mst_process_notification_config by processCode + eventTypeCode to drive
    /// SLA, priority, notification flags, and template settings.
    /// When partyId is provided, also logs a party_activity_log entry.
    /// </summary>
    Task CreateWorkspaceTaskAsync(
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
        int? partyId = null);

    /// <summary>
    /// When a workspace task/approval is completed or approved, find the next step
    /// in the process notification config (by SequenceNo) and generate tasks for it.
    /// Also logs party_activity_log if the source record involves a party.
    /// </summary>
    Task GenerateNextStepTasksAsync(TrnWorkspaceTask completedTask, UserSessionData completedBy);

    /// <summary>
    /// Auto-completes all pending/in-progress workspace tasks for a given source
    /// whose process sequence is ≤ the target process code.
    /// Called when a downstream action (e.g. enquiry→quotation, quotation→job, invoice, payment)
    /// is performed from the main page, so users don't need to close tasks manually in workspace.
    /// Sets StartedOn, CompletedOn, and CompletionRemarks on each auto-closed task.
    /// </summary>
    Task<int> AutoCompleteProcessTasksAsync(
        string sourceTable,
        long sourceId,
        string upToProcessCode,
        string remarks,
        UserSessionData completedBy,
        long? jobId = null);

    /// <summary>
    /// Generates ALL workflow tasks upfront at creation time (enquiry/quotation/job).
    /// Tasks are created with QUEUED status, except the first one which is PENDING.
    /// As each task is completed, the next QUEUED task in sequence is activated.
    /// Returns the batch ID for the generated workflow instance.
    /// </summary>
    /// <param name="sourceTable">Source table: trn_enquiry, trn_quotation, trn_job</param>
    /// <param name="sourceId">Primary key of the source record</param>
    /// <param name="sourceNo">Document number (ENQ-xxx, QUOT-xxx, JOB-xxx)</param>
    /// <param name="triggeredBy">User who triggered the workflow</param>
    /// <param name="jobId">Job ID if applicable</param>
    /// <param name="jobNo">Job number if applicable</param>
    /// <param name="jobTypeId">Job type ID for workflow template selection</param>
    /// <param name="partyId">Party ID for notifications</param>
    /// <param name="partyName">Party name for display</param>
    /// <param name="actionUrl">Base URL for task actions</param>
    /// <returns>Workflow batch ID (GUID) for tracking all tasks in this workflow instance</returns>
    Task<Guid?> GenerateAllWorkflowTasksAsync(
        string sourceTable,
        long sourceId,
        string? sourceNo,
        UserSessionData triggeredBy,
        long? jobId = null,
        string? jobNo = null,
        int? jobTypeId = null,
        int? partyId = null,
        string? partyName = null,
        string? actionUrl = null);

    /// <summary>
    /// Activates the next QUEUED task in the workflow sequence after a task is completed.
    /// Changes the status from QUEUED to PENDING and assigns to appropriate users.
    /// </summary>
    /// <param name="completedTask">The task that was just completed</param>
    /// <param name="completedBy">User who completed the task</param>
    Task ActivateNextQueuedTaskAsync(TrnWorkspaceTask completedTask, UserSessionData completedBy);
}
