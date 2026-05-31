using erp.minepress.domain.Common;

namespace erp.minepress.domain.Designing;

public class DesigningEntity : BaseEntity<long>
{
    public string DesignCode { get; set; } = string.Empty;
    public string DesignName { get; set; } = string.Empty;
    public string? DesignCategory { get; set; }
    public string? DesignType { get; set; }
    public string? JobTypesSupported { get; set; }
    public bool IsDesignByParty { get; set; }
    public bool IsPlateByParty { get; set; }
    public string? SoftwareUsed { get; set; }
    public string? FileFormat { get; set; }
    public string? ColorMode { get; set; }
    public int RevisionAllowed { get; set; } = 1;
    public decimal? ReworkChargePerRevision { get; set; }
    public decimal? BaseCost { get; set; }
    public string? CostUnit { get; set; }
    public decimal? AvgTimeHours { get; set; }
    public int? ManpowerRequired { get; set; }
    public bool IsCostApplicable { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
