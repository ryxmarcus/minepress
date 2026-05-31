using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Stores preventive maintenance schedule details for machines
/// </summary>
public partial class MstMachineMaintenance
{
    /// <summary>
    /// Primary key for maintenance record
    /// </summary>
    public long MaintenanceId { get; set; }

    /// <summary>
    /// Reference to machine requiring maintenance
    /// </summary>
    public long? MachineId { get; set; }

    /// <summary>
    /// Type of maintenance (Preventive, Calibration, AMC, Routine)
    /// </summary>
    public string? MaintenanceType { get; set; }

    /// <summary>
    /// Maintenance frequency in days
    /// </summary>
    public int? FrequencyDays { get; set; }

    /// <summary>
    /// Date when last maintenance was performed
    /// </summary>
    public DateOnly? LastMaintenanceDate { get; set; }

    /// <summary>
    /// Next scheduled maintenance date
    /// </summary>
    public DateOnly? NextDueDate { get; set; }

    /// <summary>
    /// Vendor or service provider performing maintenance
    /// </summary>
    public string? VendorName { get; set; }

    /// <summary>
    /// Estimated cost for scheduled maintenance
    /// </summary>
    public decimal? EstimatedCost { get; set; }

    /// <summary>
    /// Additional notes related to maintenance
    /// </summary>
    public string? Remarks { get; set; }

    /// <summary>
    /// Indicates whether the maintenance schedule is active
    /// </summary>
    public bool? IsActive { get; set; }

    public DateTime? BreakdownStartTime { get; set; }

    public DateTime? BreakdownEndTime { get; set; }

    public decimal? DowntimeMinutes { get; set; }

    public string? RepairStatus { get; set; }

    public DateTime? CompletionDate { get; set; }

    public virtual MstMachine? Machine { get; set; }
}
