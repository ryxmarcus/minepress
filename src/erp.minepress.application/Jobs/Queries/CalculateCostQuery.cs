using erp.minepress.application.Common.Interfaces;
using erp.minepress.application.Common.Models;

namespace erp.minepress.application.Jobs.Queries;

public record CalculateCostQuery : IQuery<Result<CostEstimationResult>>
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
