using erp.minepress.domain.Common;

namespace erp.minepress.domain.Finishing;

public class FinishingEntity : BaseEntity<long>
{
    public string FinishingCode { get; set; } = string.Empty;
    public string FinishingName { get; set; } = string.Empty;
    public string? FinishingCategory { get; set; }
    public string? FinishingType { get; set; }
    public string? SupportedJobTypes { get; set; }
    public string? SupportedProducts { get; set; }
    public int? MinSheetLengthMm { get; set; }
    public int? MinSheetWidthMm { get; set; }
    public int? MaxSheetLengthMm { get; set; }
    public int? MaxSheetWidthMm { get; set; }
    public int? MinGsm { get; set; }
    public int? MaxGsm { get; set; }
    public string? SpeedUnit { get; set; }
    public int? MaxSpeedPerHour { get; set; }
    public int? SetupTimeMin { get; set; }
    public int? ChangeoverTimeMin { get; set; }
    public decimal? CostPerSheet { get; set; }
    public decimal? SetupCost { get; set; }
    public decimal? LabourCostPerHour { get; set; }
    public int? ManpowerRequired { get; set; }
    public bool MachineRequired { get; set; } = true;
    public bool ManualAllowed { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Remarks { get; set; }
}
