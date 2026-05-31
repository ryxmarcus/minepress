using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstMachine
{
    public long MachineId { get; set; }

    public string MachineCode { get; set; } = null!;

    public string MachineName { get; set; } = null!;

    public string DepartmentCode { get; set; } = null!;

    public string? MachineCategory { get; set; }

    public string? MachineType { get; set; }

    public string? Manufacturer { get; set; }

    public string? ModelNo { get; set; }

    public int? InstallationYear { get; set; }

    public int? MaxSheetLengthMm { get; set; }

    public int? MaxSheetWidthMm { get; set; }

    public int? MinSheetLengthMm { get; set; }

    public int? MinSheetWidthMm { get; set; }

    public int? MinGsm { get; set; }

    public int? MaxGsm { get; set; }

    public int? MaxColors { get; set; }

    public string? PrintingSide { get; set; }

    public string? SpeedUnit { get; set; }

    public int? MaxSpeed { get; set; }

    public int? SetupTimeMinutes { get; set; }

    public int? ChangeoverTimeMinutes { get; set; }

    public decimal? HourlyRunningCost { get; set; }

    public decimal? SetupCost { get; set; }

    public decimal? PowerCostPerHour { get; set; }

    public decimal? LabourCostPerHour { get; set; }

    public decimal? PowerConsumptionKw { get; set; }

    public bool? AirRequired { get; set; }

    public int? ManpowerRequired { get; set; }

    public string? SupportedJobTypes { get; set; }

    public int? AutoSelectPriority { get; set; }

    public bool? IsProduction { get; set; }

    public bool? IsActive { get; set; }

    public string? Remarks { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? MaxSpeedPerHour { get; set; }

    public int? SetupTimeMin { get; set; }

    public int? ChangeoverTimeMin { get; set; }

    public int? MaintenanceCycleDays { get; set; }

    public int? AvgDowntimeHours { get; set; }

    public bool? IsProductionMachine { get; set; }

    public virtual ICollection<HybEmployeeAttendance> HybEmployeeAttendances { get; set; } = new List<HybEmployeeAttendance>();

    public virtual ICollection<MstEmployeeMachineMapping> MstEmployeeMachineMappings { get; set; } = new List<MstEmployeeMachineMapping>();

    public virtual ICollection<MstMachineMaintenance> MstMachineMaintenances { get; set; } = new List<MstMachineMaintenance>();

    public virtual ICollection<TrnJobMachineAllocation> TrnJobMachineAllocations { get; set; } = new List<TrnJobMachineAllocation>();

    public virtual ICollection<TrnJobMachineManpowerAllocation> TrnJobMachineManpowerAllocations { get; set; } = new List<TrnJobMachineManpowerAllocation>();

    public virtual ICollection<TrnMachineBreakdown> TrnMachineBreakdowns { get; set; } = new List<TrnMachineBreakdown>();
}
