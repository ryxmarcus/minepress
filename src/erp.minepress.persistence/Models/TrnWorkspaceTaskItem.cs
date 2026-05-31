using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Item-level task tracking for parallel execution. Each job item (e.g. Cover Page, Book Content) gets independent task rows per process (Design, CTP, PostPress), enabling simultaneous work across items.
/// </summary>
public partial class TrnWorkspaceTaskItem
{
    public long TaskItemId { get; set; }

    public long WorkspaceTaskId { get; set; }

    public long JobId { get; set; }

    public long JobItemId { get; set; }

    public string ProcessCode { get; set; } = null!;

    public string? ProcessName { get; set; }

    public string ItemName { get; set; } = null!;

    public string? ItemDescription { get; set; }

    public int ItemSequence { get; set; }

    /// <summary>
    /// NOT_STARTED, RUNNING, COMPLETED, CLOSED — tracked per item independently
    /// </summary>
    public string TaskStatus { get; set; } = null!;

    public long? AssignedUserId { get; set; }

    public DateTime? AssignedOn { get; set; }

    public DateTime? StartedOn { get; set; }

    public long? StartedBy { get; set; }

    public DateTime? CompletedOn { get; set; }

    public long? CompletedBy { get; set; }

    public string? Remarks { get; set; }

    public string? WorkData { get; set; }

    /// <summary>
    /// Links to the upstream item task that triggered this one (e.g. Design Cover → CTP Cover)
    /// </summary>
    public long? ParentTaskItemId { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual MstUser? AssignedUser { get; set; }

    public virtual ICollection<TrnWorkspaceTaskItem> InverseParentTaskItem { get; set; } = new List<TrnWorkspaceTaskItem>();

    public virtual TrnJob Job { get; set; } = null!;

    public virtual TrnJobItem JobItem { get; set; } = null!;

    public virtual TrnWorkspaceTaskItem? ParentTaskItem { get; set; }

    public virtual TrnWorkspaceTask WorkspaceTask { get; set; } = null!;
}
