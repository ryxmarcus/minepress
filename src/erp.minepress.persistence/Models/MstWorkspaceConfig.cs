using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Per-user workspace configuration. Controls widget visibility, default filters, calendar view, notification preferences, pinned items and layout options for the My Workspace dashboard.
/// </summary>
public partial class MstWorkspaceConfig
{
    public long ConfigId { get; set; }

    public long UserId { get; set; }

    public bool ShowPendingTasks { get; set; }

    public bool ShowCompletedTasks { get; set; }

    public bool ShowAssignedTasks { get; set; }

    public bool ShowApprovals { get; set; }

    public bool ShowCalendar { get; set; }

    public bool ShowNotifications { get; set; }

    public bool ShowHistory { get; set; }

    /// <summary>
    /// Default calendar view when workspace loads: DAILY, WEEKLY, MONTHLY
    /// </summary>
    public string DefaultCalendarView { get; set; } = null!;

    public string DefaultTaskFilter { get; set; } = null!;

    public string DefaultApprovalFilter { get; set; } = null!;

    public bool NotifyOnTaskAssign { get; set; }

    public bool NotifyOnTaskOverdue { get; set; }

    public bool NotifyOnApprovalRequest { get; set; }

    public bool NotifyOnApprovalComplete { get; set; }

    /// <summary>
    /// JSON array defining display order of workspace widgets: PENDING_TASKS, APPROVALS, CALENDAR, NOTIFICATIONS, HISTORY
    /// </summary>
    public string? WidgetOrder { get; set; }

    /// <summary>
    /// JSON array of job_ids that user has pinned for quick access on workspace
    /// </summary>
    public string? PinnedJobs { get; set; }

    /// <summary>
    /// JSON array of process_ids pinned by user for quick navigation
    /// </summary>
    public string? PinnedProcesses { get; set; }

    public int HistoryDays { get; set; }

    /// <summary>
    /// Auto-refresh interval in seconds for workspace widgets. 0 disables auto-refresh.
    /// </summary>
    public int AutoRefreshSeconds { get; set; }

    public bool CompactMode { get; set; }

    public int ItemsPerPage { get; set; }

    public bool IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual MstUser User { get; set; } = null!;
}
