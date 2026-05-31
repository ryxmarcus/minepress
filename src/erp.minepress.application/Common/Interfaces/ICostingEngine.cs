namespace erp.minepress.application.Common.Interfaces;

public interface ICostingEngine
{
    Task<CostEstimationResult> CalculateCostAsync(CostEstimationRequest request, CancellationToken cancellationToken = default);
}

public record CostEstimationRequest
{
    public int Quantity { get; init; }
    public int TotalPages { get; init; }
    public decimal TrimWidthMm { get; init; }
    public decimal TrimHeightMm { get; init; }
    public string? PrintingMode { get; init; }
    public int? JobTypeId { get; init; }
    public int? ProductTypeId { get; init; }
    public long? PaperId { get; init; }
    public long? MachineId { get; init; }
    public bool IsCustomerMaterial { get; init; }
}

public record CostEstimationResult
{
    public decimal PaperCost { get; init; }
    public decimal PlateCost { get; init; }
    public decimal InkCost { get; init; }
    public decimal MachineCost { get; init; }
    public decimal FinishingCost { get; init; }
    public decimal BindingCost { get; init; }
    public decimal DesigningCost { get; init; }
    public decimal GrandTotal { get; init; }
    public decimal CostPerUnit { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal NetTotal { get; init; }
    public IReadOnlyList<CostLineItem> Breakdown { get; init; } = [];
}

public record CostLineItem(string Name, string Category, string Detail, decimal Amount);
