using erp.minepress.application.Common.Models;
using erp.minepress.application.Reports.Dto;

namespace erp.minepress.application.Reports.Interfaces;

/// <summary>
/// Orchestrates the full report query lifecycle: validation, SQL generation,
/// execution, totals computation, and query plan storage.
/// </summary>
public interface IQueryBuilderService
{
    /// <summary>
    /// Builds, executes, and caches a dynamic report query from the given request.
    /// Returns paginated results in JSON format.
    /// </summary>
    Task<Result<ReportQueryResult>> BuildAndExecuteAsync(ReportQueryRequest request, CancellationToken ct = default);
}
