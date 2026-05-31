using erp.minepress.application.Customers.Dto;
using erp.minepress.application.Jobs.Dto;

namespace erp.minepress.bff.service.Interfaces;

/// <summary>
/// BFF aggregation service that composes backend data for frontend consumption.
/// </summary>
public interface IBffAggregatorService
{
    Task<DashboardSummaryDto?> GetDashboardSummaryAsync(CancellationToken cancellationToken = default);
    Task<JobRateCalculatorDto?> GetJobCalculationDetailAsync(string calcRefNo, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CustomerDto>> GetActiveCustomersAsync(CancellationToken cancellationToken = default);
}

public record DashboardSummaryDto
{
    public int TotalActiveJobs { get; init; }
    public int TotalCustomers { get; init; }
    public int PendingCalculations { get; init; }
    public decimal RevenueThisMonth { get; init; }
    public DateTime GeneratedAt { get; init; }
}
