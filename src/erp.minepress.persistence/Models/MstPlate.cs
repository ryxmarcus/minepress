using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstPlate
{
    public long PlateId { get; set; }

    public string PlateCode { get; set; } = null!;

    public string PlateName { get; set; } = null!;

    public string? PlateType { get; set; }

    public string? CoatingType { get; set; }

    public string? ExposureType { get; set; }

    public decimal? ThicknessMm { get; set; }

    public int? PlateLengthMm { get; set; }

    public int? PlateWidthMm { get; set; }

    public int? MinGsmSupported { get; set; }

    public int? MaxGsmSupported { get; set; }

    public int? MaxImpressions { get; set; }

    public bool? Reusability { get; set; }

    public string? CompatibleCtp { get; set; }

    public string? CompatibleMachineType { get; set; }

    public decimal? PlateCost { get; set; }

    public decimal? ProcessingCost { get; set; }

    public decimal? WastagePercent { get; set; }

    public int? StorageLifeMonths { get; set; }

    public string? ShelfCondition { get; set; }

    public string? SupportedJobTypes { get; set; }

    public int? AutoSelectPriority { get; set; }

    public bool? IsActive { get; set; }

    public string? Remarks { get; set; }

    public DateTime? CreatedAt { get; set; }

    public decimal? ReorderLevel { get; set; }

    public decimal? CurrentStock { get; set; }

    public string? Uom { get; set; }

    public decimal? MinOrderQty { get; set; }

    public decimal? LastPurchaseRate { get; set; }

    public DateOnly? LastPurchaseDate { get; set; }

    public string? HsnCode { get; set; }

    public decimal? GstRate { get; set; }
}
