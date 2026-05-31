using erp.minepress.domain.Common;

namespace erp.minepress.domain.Job;

public class JobRateCalculatorEntity : BaseEntity<long>
{
    public string CalcRefNo { get; set; } = string.Empty;
    public long? EnquiryId { get; set; }
    public long? QuotationId { get; set; }
    public long? JobId { get; set; }
    public int? PartyId { get; set; }
    public int? JobTypeId { get; set; }
    public int? ProductTypeId { get; set; }
    public int? ProductSizeId { get; set; }
    public int Quantity { get; set; }
    public int TotalPages { get; set; }
    public decimal? TrimWidthMm { get; set; }
    public decimal? TrimHeightMm { get; set; }
    public string? PrintingMode { get; set; }
    public bool IsCustomerMaterial { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal NetTotal { get; set; }
    public decimal CostPerUnit { get; set; }
    public string? PartsData { get; set; }
    public string? CostBreakdown { get; set; }
    public string? BomData { get; set; }
    public string? AiInsights { get; set; }
    public string? RecommendedMachines { get; set; }
    public string? CalcInputSnapshot { get; set; }
    public string Status { get; set; } = "DRAFT";
    public DateTime? ValidityDate { get; set; }
    public int Version { get; set; } = 1;
    public long? ParentCalcId { get; set; }
    public string? InternalRemarks { get; set; }
    public string? ClientRemarks { get; set; }
    public long CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
}
