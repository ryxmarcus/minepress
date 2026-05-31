using erp.minepress.domain.Common;

namespace erp.minepress.domain.Machine;

public class MachineEntity : BaseEntity<long>
{
    public string MachineCode { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public string DepartmentCode { get; set; } = string.Empty;
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
    public int? MaxSpeedPerHour { get; set; }
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
    public bool IsProduction { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
