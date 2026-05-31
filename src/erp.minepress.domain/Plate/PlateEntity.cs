using erp.minepress.domain.Common;

namespace erp.minepress.domain.Plate;

public class PlateEntity : BaseEntity<long>
{
    public string PlateCode { get; set; } = string.Empty;
    public string PlateName { get; set; } = string.Empty;
    public string? PlateType { get; set; }
    public string? CoatingType { get; set; }
    public string? ExposureType { get; set; }
    public decimal? ThicknessMm { get; set; }
    public int? PlateLengthMm { get; set; }
    public int? PlateWidthMm { get; set; }
    public int? MinGsmSupported { get; set; }
    public int? MaxGsmSupported { get; set; }
    public int? MaxImpressions { get; set; }
    public bool Reusability { get; set; }
    public string? CompatibleCtp { get; set; }
    public string? CompatibleMachineType { get; set; }
    public decimal? PlateCost { get; set; }
    public decimal? ProcessingCost { get; set; }
    public decimal? WastagePercent { get; set; }
    public int? StorageLifeMonths { get; set; }
    public string? SupportedJobTypes { get; set; }
    public int? AutoSelectPriority { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
