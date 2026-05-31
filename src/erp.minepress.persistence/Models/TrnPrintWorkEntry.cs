using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Per-part printing process inputs captured in Workspace PrintWork page. One row per product part per task.
/// </summary>
public partial class TrnPrintWorkEntry
{
    public long PrintWorkId { get; set; }

    public long WorkspaceTaskId { get; set; }

    public long? JobId { get; set; }

    public string PartName { get; set; } = null!;

    public int? PartSequence { get; set; }

    /// <summary>
    /// Printing method selected for this part: OFFSET, DIGITAL, or SCREEN
    /// </summary>
    public string? PrintingMethod { get; set; }

    public int? MachineId { get; set; }

    public string? MachineName { get; set; }

    public int? NumberOfColors { get; set; }

    public int? NumberOfPlates { get; set; }

    /// <summary>
    /// Total sheets required for this part (pre-filled from job specs)
    /// </summary>
    public int? TotalSheetsRequired { get; set; }

    /// <summary>
    /// Actual sheets printed so far — updated during execution
    /// </summary>
    public int? TotalSheetsPrinted { get; set; }

    public bool IsSelected { get; set; }

    public bool IsStarted { get; set; }

    public DateTime? StartedOn { get; set; }

    public DateTime? CompletedOn { get; set; }

    public string? Notes { get; set; }

    public long? CreatedBy { get; set; }

    public DateTime CreatedOn { get; set; }

    public long? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual TrnWorkspaceTask WorkspaceTask { get; set; } = null!;
}
