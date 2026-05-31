using erp.minepress.application.Common.Interfaces;
using erp.minepress.application.Common.Models;
using erp.minepress.application.Jobs.Commands;
using erp.minepress.application.Jobs.Dto;
using erp.minepress.application.Jobs.Queries;
using Microsoft.AspNetCore.Mvc;

namespace erp.minepress.webapi.Controllers;

[Route("api/[controller]")]
public class JobRateCalculatorController : BaseApiController
{
    private readonly ICommandHandler<CreateJobRateCalculationCommand, Result<JobRateCalculatorDto>> _createHandler;
    private readonly IQueryHandler<GetJobRateCalculationByRefNoQuery, Result<JobRateCalculatorDto>> _getByRefNoHandler;
    private readonly IQueryHandler<CalculateCostQuery, Result<CostEstimationResult>> _calculateCostHandler;

    public JobRateCalculatorController(
        ICommandHandler<CreateJobRateCalculationCommand, Result<JobRateCalculatorDto>> createHandler,
        IQueryHandler<GetJobRateCalculationByRefNoQuery, Result<JobRateCalculatorDto>> getByRefNoHandler,
        IQueryHandler<CalculateCostQuery, Result<CostEstimationResult>> calculateCostHandler)
    {
        _createHandler = createHandler;
        _getByRefNoHandler = getByRefNoHandler;
        _calculateCostHandler = calculateCostHandler;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateJobRateCalculationCommand command, CancellationToken cancellationToken)
    {
        var result = await _createHandler.HandleAsync(command, cancellationToken);

        if (!result.IsSuccess)
            return ErrorResponse<JobRateCalculatorDto>(result.ErrorMessage!);

        return CreatedResponse(result.Data!);
    }

    [HttpGet("{refNo}")]
    public async Task<IActionResult> GetByRefNo(string refNo, CancellationToken cancellationToken)
    {
        var query = new GetJobRateCalculationByRefNoQuery { CalcRefNo = refNo };
        var result = await _getByRefNoHandler.HandleAsync(query, cancellationToken);

        if (!result.IsSuccess)
            return NotFoundResponse<JobRateCalculatorDto>(result.ErrorMessage);

        return OkResponse(result.Data!);
    }

    [HttpPost("calculate-cost")]
    public async Task<IActionResult> CalculateCost([FromBody] CalculateCostQuery query, CancellationToken cancellationToken)
    {
        var result = await _calculateCostHandler.HandleAsync(query, cancellationToken);

        if (!result.IsSuccess)
            return ErrorResponse<CostEstimationResult>(result.ErrorMessage!);

        return OkResponse(result.Data!);
    }
}
