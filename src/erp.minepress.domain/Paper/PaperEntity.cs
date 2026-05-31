using erp.minepress.domain.Common;

namespace erp.minepress.domain.Paper;

public class PaperEntity : BaseEntity<long>
{
    public string PaperCode { get; set; } = string.Empty;
    public string PaperName { get; set; } = string.Empty;
    public string? PaperCategory { get; set; }
    public string? PaperType { get; set; }
    public string? PaperFinish { get; set; }
    public int Gsm { get; set; }
    public int? SheetLengthMm { get; set; }
    public int? SheetWidthMm { get; set; }
    public string? SheetSizeName { get; set; }
    public int? ReelWidthMm { get; set; }
    public int? ReelDiameterMm { get; set; }
    public string? GrainDirection { get; set; }
    public string? SupportedJobTypes { get; set; }
    public string? SupportedUsage { get; set; }
    public decimal? CostPerKg { get; set; }
    public decimal? CostPerSheet { get; set; }
    public string? SupplierName { get; set; }
    public string? BrandName { get; set; }
    public string? CountryOfOrigin { get; set; }
    public bool IsFscCertified { get; set; }
    public bool IsRecycled { get; set; }
    public decimal? MinOrderQtyKg { get; set; }
    public int? LeadTimeDays { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Remarks { get; set; }
}
