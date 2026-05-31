using erp.minepress.application.Customers.Dto;
using erp.minepress.application.Common.Interfaces;
using erp.minepress.application.Common.Models;
using erp.minepress.application.Jobs.Dto;
using erp.minepress.application.Jobs.Queries;
using erp.minepress.bff.service.Interfaces;
using Microsoft.Extensions.Logging;

namespace erp.minepress.bff.service.Services;

public class BffAggregatorService : IBffAggregatorService
{
    private readonly IQueryHandler<GetJobRateCalculationByRefNoQuery, Result<JobRateCalculatorDto>> _getJobHandler;
    private readonly ILogger<BffAggregatorService> _logger;

    public BffAggregatorService(
        IQueryHandler<GetJobRateCalculationByRefNoQuery, Result<JobRateCalculatorDto>> getJobHandler,
        ILogger<BffAggregatorService> logger)
    {
        _getJobHandler = getJobHandler;
        _logger = logger;
    }

    public Task<DashboardSummaryDto?> GetDashboardSummaryAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating dashboard summary");

        // TODO: Aggregate data from multiple backend services
        var summary = new DashboardSummaryDto
        {
            TotalActiveJobs = 0,
            TotalCustomers = 0,
            PendingCalculations = 0,
            RevenueThisMonth = 0m,
            GeneratedAt = DateTime.UtcNow
        };

        return Task.FromResult<DashboardSummaryDto?>(summary);
    }

    public async Task<JobRateCalculatorDto?> GetJobCalculationDetailAsync(string calcRefNo, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching job calculation detail for {CalcRefNo}", calcRefNo);

        var query = new GetJobRateCalculationByRefNoQuery { CalcRefNo = calcRefNo };
        var result = await _getJobHandler.HandleAsync(query, cancellationToken);

        return result.IsSuccess ? result.Data : null;
    }

    public Task<IReadOnlyList<CustomerDto>> GetActiveCustomersAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching active customers for BFF");

        // TODO: Wire customer query handler when available
        IReadOnlyList<CustomerDto> customers = Array.Empty<CustomerDto>();
        return Task.FromResult(customers);
    }
}
