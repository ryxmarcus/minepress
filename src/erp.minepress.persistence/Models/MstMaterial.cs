using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstMaterial
{
    public string MaterialCode { get; set; } = null!;

    public string MaterialName { get; set; } = null!;

    public string? MaterialCategory { get; set; }

    public string? MaterialSubCategory { get; set; }

    public string? ProcessStage { get; set; }

    public string? UsageArea { get; set; }

    public string? CompatibleProcess { get; set; }

    public string? CompatibleJobTypes { get; set; }

    public string? UnitOfMeasure { get; set; }

    public decimal? AvgConsumptionPerJob { get; set; }

    public decimal? RatePerUnit { get; set; }

    public decimal? CostPerJob { get; set; }

    public decimal? ReorderLevel { get; set; }

    public decimal? MaxStockLevel { get; set; }

    public string? SupplierName { get; set; }

    public string? StorageLocation { get; set; }

    public int? ShelfLifeMonths { get; set; }

    public bool? IsConsumable { get; set; }

    public bool? IsActive { get; set; }

    public string? Remarks { get; set; }
}
