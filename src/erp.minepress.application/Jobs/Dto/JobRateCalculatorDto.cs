namespace erp.minepress.application.Jobs.Dto;

public record JobRateCalculatorDto
{
    public long Id { get; init; }
    public string CalcRefNo { get; init; } = string.Empty;
    public long? EnquiryId { get; init; }
    public long? QuotationId { get; init; }
    public long? JobId { get; init; }
    public int? PartyId { get; init; }
    public string? PartyName { get; init; }
    public int? JobTypeId { get; init; }
    public string? JobTypeName { get; init; }
    public int? ProductTypeId { get; init; }
    public int? ProductSizeId { get; init; }
    public int Quantity { get; init; }
    public int TotalPages { get; init; }
    public decimal? TrimWidthMm { get; init; }
    public decimal? TrimHeightMm { get; init; }
    public string? PrintingMode { get; init; }
    public bool IsCustomerMaterial { get; init; }
    public decimal GrandTotal { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal NetTotal { get; init; }
    public decimal CostPerUnit { get; init; }
    public string Status { get; init; } = string.Empty;
    public int Version { get; init; }
    public DateTime CreatedOn { get; init; }
}
