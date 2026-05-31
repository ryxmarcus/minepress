namespace erp.minepress.domain.Ink;

public class InkEntity
{
    public string InkCode { get; set; } = string.Empty;
    public string InkName { get; set; } = string.Empty;
    public string? InkCategory { get; set; }
    public string? InkType { get; set; }
    public string? ColorType { get; set; }
    public string? ColorName { get; set; }
    public string? PantoneCode { get; set; }
    public string? Manufacturer { get; set; }
    public string? InkSeries { get; set; }
    public string? CompatibleProcess { get; set; }
    public string? CompatibleMachineType { get; set; }
    public string? DryingType { get; set; }
    public string? RubResistance { get; set; }
    public string? GlossLevel { get; set; }
    public decimal? CoverageSqMPerKg { get; set; }
    public decimal? ConsumptionGsm { get; set; }
    public decimal? CostPerKg { get; set; }
    public decimal? WastagePercent { get; set; }
    public string? SupportedJobTypes { get; set; }
    public int? AutoSelectPriority { get; set; }
    public int? ShelfLifeMonths { get; set; }
    public string? StorageCondition { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Remarks { get; set; }
}
