using erp.minepress.domain.Common;

namespace erp.minepress.domain.Binding;

public class BindingEntity : BaseEntity<long>
{
    public string BindingCode { get; set; } = string.Empty;
    public string BindingName { get; set; } = string.Empty;
    public string? BindingCategory { get; set; }
    public string? BindingType { get; set; }
    public string? SupportedJobTypes { get; set; }
    public int? MinPages { get; set; }
    public int? MaxPages { get; set; }
    public int? MinGsm { get; set; }
    public int? MaxGsm { get; set; }
    public decimal? MaxBookThicknessMm { get; set; }
    public string? SpeedUnit { get; set; }
    public int? MaxSpeedPerHour { get; set; }
    public int? SetupTimeMin { get; set; }
    public int? ChangeoverTimeMin { get; set; }
    public decimal? CostPerBook { get; set; }
    public decimal? SetupCost { get; set; }
    public decimal? LabourCostPerHour { get; set; }
    public int? ManpowerRequired { get; set; }
    public bool MachineRequired { get; set; } = true;
    public bool ManualAllowed { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Remarks { get; set; }
}
