using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Stores machine fault and breakdown records
/// </summary>
public partial class TrnMachineBreakdown
{
    /// <summary>
    /// Primary key for machine breakdown record
    /// </summary>
    public long BreakdownId { get; set; }

    /// <summary>
    /// Reference to machine where breakdown occurred
    /// </summary>
    public long MachineId { get; set; }

    /// <summary>
    /// Unique code identifying the fault type
    /// </summary>
    public string? FaultCode { get; set; }

    /// <summary>
    /// Detailed description of the fault
    /// </summary>
    public string? FaultDescription { get; set; }

    /// <summary>
    /// Fault category (Mechanical, Electrical, Software, Operator Error)
    /// </summary>
    public string? FaultCategory { get; set; }

    /// <summary>
    /// Severity level of breakdown (Low, Medium, High, Critical)
    /// </summary>
    public string? SeverityLevel { get; set; }

    /// <summary>
    /// Timestamp when machine breakdown started
    /// </summary>
    public DateTime BreakdownStartTime { get; set; }

    /// <summary>
    /// Timestamp when breakdown ended
    /// </summary>
    public DateTime? BreakdownEndTime { get; set; }

    /// <summary>
    /// Total downtime in minutes caused by breakdown
    /// </summary>
    public decimal? DowntimeMinutes { get; set; }

    /// <summary>
    /// Current breakdown status (Open, Assigned, In Progress, Resolved, Closed)
    /// </summary>
    public string? BreakdownStatus { get; set; }

    /// <summary>
    /// Name or ID of person who reported the breakdown
    /// </summary>
    public string? ReportedBy { get; set; }

    /// <summary>
    /// Reference to technician assigned to fix the issue
    /// </summary>
    public long? TechnicianId { get; set; }

    /// <summary>
    /// Name of technician handling the repair
    /// </summary>
    public string? TechnicianName { get; set; }

    /// <summary>
    /// Root cause identified after analysis
    /// </summary>
    public string? RootCause { get; set; }

    /// <summary>
    /// Action taken to fix the breakdown
    /// </summary>
    public string? CorrectiveAction { get; set; }

    /// <summary>
    /// Preventive steps taken to avoid recurrence
    /// </summary>
    public string? PreventiveAction { get; set; }

    /// <summary>
    /// List of spare parts used during repair
    /// </summary>
    public string? SparePartsUsed { get; set; }

    /// <summary>
    /// Total repair cost incurred
    /// </summary>
    public decimal? RepairCost { get; set; }

    /// <summary>
    /// Date when issue was resolved
    /// </summary>
    public DateTime? ResolvedDate { get; set; }

    /// <summary>
    /// Additional notes related to breakdown
    /// </summary>
    public string? Remarks { get; set; }

    /// <summary>
    /// Record creation timestamp
    /// </summary>
    public DateTime? CreatedOn { get; set; }

    /// <summary>
    /// User who created the breakdown record
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Indicates whether breakdown record is active
    /// </summary>
    public bool? IsActive { get; set; }

    public virtual MstMachine Machine { get; set; } = null!;
}
