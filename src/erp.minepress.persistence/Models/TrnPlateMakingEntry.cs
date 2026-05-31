using System;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Per-activity Plate Making progress captured in Workspace &gt; PlateMaking page. One row per activity per workspace task.
/// </summary>
public partial class TrnPlateMakingEntry
{
    /// <summary>
    /// Surrogate primary key — auto-incremented.
    /// </summary>
    public long PlateMakingId { get; set; }

    /// <summary>
    /// FK to trn_workspace_task. Identifies the parent task.
    /// </summary>
    public long WorkspaceTaskId { get; set; }

    /// <summary>
    /// Denormalised FK to trn_job for fast reporting.
    /// </summary>
    public long? JobId { get; set; }

    /// <summary>
    /// Plate making activity label e.g. Cover Plates, Text Plates.
    /// </summary>
    public string ActivityName { get; set; } = null!;

    /// <summary>
    /// Display/processing order of this activity within the task.
    /// </summary>
    public int ActivitySequence { get; set; }

    /// <summary>
    /// Product part name from job config (e.g. Cover, Text).
    /// </summary>
    public string? PartName { get; set; }

    /// <summary>
    /// Plate technology: CTP, Conventional, Violet, Thermal, etc.
    /// </summary>
    public string? PlateType { get; set; }

    /// <summary>
    /// Number of ink colors for this activity.
    /// </summary>
    public int NumberOfColors { get; set; }

    /// <summary>
    /// Total plates to be made.
    /// </summary>
    public int NumberOfPlates { get; set; }

    /// <summary>
    /// Plates finished so far — updated on Save Progress.
    /// </summary>
    public int PlatesMade { get; set; }

    /// <summary>
    /// Computed column: GREATEST(0, number_of_plates - plates_made).
    /// </summary>
    public int? PlatesPending { get; set; }

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
