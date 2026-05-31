using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Per-activity Design/DTP progress captured in Workspace &gt; DesignWork page. One row per activity per workspace task.
/// </summary>
public partial class TrnDesignWorkEntry
{
    /// <summary>
    /// Surrogate primary key — auto-incremented.
    /// </summary>
    public long DesignWorkId { get; set; }

    /// <summary>
    /// FK to trn_workspace_task. Identifies the parent task.
    /// </summary>
    public long WorkspaceTaskId { get; set; }

    /// <summary>
    /// Denormalised FK to trn_job for fast reporting.
    /// </summary>
    public long? JobId { get; set; }

    /// <summary>
    /// Design/DTP activity label e.g. Cover Design, Text DTP.
    /// </summary>
    public string ActivityName { get; set; } = null!;

    /// <summary>
    /// Display/processing order of this activity within the task.
    /// </summary>
    public int ActivitySequence { get; set; }

    /// <summary>
    /// Total pages to be designed/DTPed for this activity.
    /// </summary>
    public int PagesRequired { get; set; }

    /// <summary>
    /// Pages finished so far — updated on Save Progress.
    /// </summary>
    public int PagesCompleted { get; set; }

    /// <summary>
    /// Computed column: MAX(0, pages_required - pages_completed).
    /// </summary>
    public int? PagesPending { get; set; }

    /// <summary>
    /// True when the row-level Complete button is clicked.
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Timestamp when this activity was marked completed.
    /// </summary>
    public DateTime? CompletedOn { get; set; }

    /// <summary>
    /// Free-text work notes entered in the right sidebar.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// User ID of the person who created this record.
    /// </summary>
    public long? CreatedBy { get; set; }

    /// <summary>
    /// Record creation timestamp.
    /// </summary>
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// User ID of the last person who modified this record.
    /// </summary>
    public long? ModifiedBy { get; set; }

    /// <summary>
    /// Last modification timestamp.
    /// </summary>
    public DateTime? ModifiedOn { get; set; }

    public virtual TrnWorkspaceTask WorkspaceTask { get; set; } = null!;
}
