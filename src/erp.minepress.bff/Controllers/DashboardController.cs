using erp.minepress.bff.service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace erp.minepress.bff.Controllers;

[ApiController]
[Route("api/bff/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IBffAggregatorService _bffService;

    public DashboardController(IBffAggregatorService bffService)
    {
        _bffService = bffService;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetDashboardSummary(CancellationToken cancellationToken)
    {
        var summary = await _bffService.GetDashboardSummaryAsync(cancellationToken);
        return Ok(summary);
    }

    [HttpGet("customers")]
    public async Task<IActionResult> GetActiveCustomers(CancellationToken cancellationToken)
    {
        var customers = await _bffService.GetActiveCustomersAsync(cancellationToken);
        return Ok(customers);
    }

    [HttpGet("job-calculation/{calcRefNo}")]
    public async Task<IActionResult> GetJobCalculationDetail(string calcRefNo, CancellationToken cancellationToken)
    {
        var detail = await _bffService.GetJobCalculationDetailAsync(calcRefNo, cancellationToken);
        if (detail is null)
            return NotFound();
        return Ok(detail);
    }
}
