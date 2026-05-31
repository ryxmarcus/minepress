namespace erp.minepress.application.Customers.Dto;

public record CustomerDto
{
    public int Id { get; init; }
    public int PartyId { get; init; }
    public string PartyName { get; init; } = string.Empty;
    public string? PartyCode { get; init; }
    public string? Email { get; init; }
    public long? Mobile { get; init; }
    public string? GstNo { get; init; }
    public int? CustomerType { get; init; }
    public int? CustomerGroup { get; init; }
    public decimal? MaxCreditLimit { get; init; }
    public decimal? AvailableCreditLimitAmt { get; init; }
    public string? Salesperson { get; init; }
    public bool IsActive { get; init; }
}
