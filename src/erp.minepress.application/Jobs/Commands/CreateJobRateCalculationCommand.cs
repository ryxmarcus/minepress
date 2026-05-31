using erp.minepress.application.Common.Interfaces;
using erp.minepress.application.Common.Models;
using erp.minepress.application.Jobs.Dto;

namespace erp.minepress.application.Jobs.Commands;

public record CreateJobRateCalculationCommand : ICommand<Result<JobRateCalculatorDto>>
{
    public int? PartyId { get; init; }
    public int? JobTypeId { get; init; }
    public int? ProductTypeId { get; init; }
    public int? ProductSizeId { get; init; }
    public int Quantity { get; init; }
    public int TotalPages { get; init; }
    public decimal? TrimWidthMm { get; init; }
    public decimal? TrimHeightMm { get; init; }
    public string? PrintingMode { get; init; }
    public bool IsCustomerMaterial { get; init; }
    public string? InternalRemarks { get; init; }
    public string? ClientRemarks { get; init; }
    public long CreatedBy { get; init; }
}
