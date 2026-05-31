using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstInk
{
    public string InkCode { get; set; } = null!;

    public string InkName { get; set; } = null!;

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

    public bool? IsActive { get; set; }

    public string? Remarks { get; set; }

    public decimal? ReorderLevel { get; set; }

    public decimal? CurrentStock { get; set; }

    public string? Uom { get; set; }

    public decimal? MinOrderQty { get; set; }

    public int? LeadTimeDays { get; set; }

    public decimal? LastPurchaseRate { get; set; }

    public DateOnly? LastPurchaseDate { get; set; }

    public string? HsnCode { get; set; }

    public decimal? GstRate { get; set; }
}
