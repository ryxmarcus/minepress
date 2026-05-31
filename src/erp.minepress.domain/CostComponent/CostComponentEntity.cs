using erp.minepress.domain.Common;

namespace erp.minepress.domain.CostComponent;

public class CostComponentEntity : AuditableEntity<int>
{
    public string ComponentCode { get; set; } = string.Empty;
    public string ComponentName { get; set; } = string.Empty;
    public string ComponentCategory { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ApplicableLevel { get; set; } = string.Empty;
    public bool ApplicableToPart { get; set; }
    public bool ApplicableToProduct { get; set; }
    public string CalculationType { get; set; } = string.Empty;
    public string? BaseUom { get; set; }
    public bool IsMandatory { get; set; }
    public bool IsOutsourceAllowed { get; set; } = true;
    public bool IsTaxable { get; set; } = true;
    public int? TaxCategoryId { get; set; }
    public decimal? DefaultRate { get; set; }
    public decimal? MinRate { get; set; }
    public decimal? MaxRate { get; set; }
    public int? SequenceNo { get; set; }
}
