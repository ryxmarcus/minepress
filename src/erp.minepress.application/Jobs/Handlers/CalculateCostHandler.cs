using erp.minepress.application.Common.Interfaces;
using erp.minepress.application.Common.Models;

namespace erp.minepress.application.Jobs.Handlers;

public class CalculateCostHandler : IQueryHandler<Jobs.Queries.CalculateCostQuery, Result<CostEstimationResult>>
{
    private readonly ICostingEngine _costingEngine;

    public CalculateCostHandler(ICostingEngine costingEngine)
    {
        _costingEngine = costingEngine;
    }

    public async Task<Result<CostEstimationResult>> HandleAsync(
        Jobs.Queries.CalculateCostQuery query,
        CancellationToken cancellationToken = default)
    {
        var request = new CostEstimationRequest
        {
            Quantity = query.Quantity,
            TotalPages = query.TotalPages,
            TrimWidthMm = query.TrimWidthMm,
            TrimHeightMm = query.TrimHeightMm,
            PrintingMode = query.PrintingMode,
            JobTypeId = query.JobTypeId,
            ProductTypeId = query.ProductTypeId,
            PaperId = query.PaperId,
            MachineId = query.MachineId,
            IsCustomerMaterial = query.IsCustomerMaterial
        };

        var result = await _costingEngine.CalculateCostAsync(request, cancellationToken);

        return Result<CostEstimationResult>.Success(result);
    }
}
