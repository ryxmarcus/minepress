using erp.minepress.application.Common.Interfaces;
using erp.minepress.application.Common.Models;
using erp.minepress.application.Jobs.Dto;

namespace erp.minepress.application.Jobs.Queries;

public record GetJobRateCalculationByRefNoQuery : IQuery<Result<JobRateCalculatorDto>>
{
    public string CalcRefNo { get; init; } = string.Empty;
}
