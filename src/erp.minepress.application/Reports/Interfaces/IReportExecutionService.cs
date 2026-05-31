using erp.minepress.application.Reports.Dto;

namespace erp.minepress.application.Reports.Interfaces;

/// <summary>
/// Executes generated SQL against the database and returns typed results.
/// </summary>
public interface IReportExecutionService
{
    /// <summary>
    /// Executes a generated query and returns paginated results in JSON-ready format.
    /// </summary>
    Task<ReportQueryResult> ExecuteAsync(GeneratedQuery query, CancellationToken ct = default);

    /// <summary>
    /// Computes totals (SUM, AVG) for numeric columns in the result set.
    /// </summary>
    Task<Dictionary<string, object?>?> ComputeTotalsAsync(
        GeneratedQuery query,
        ReportQueryRequest request,
        CancellationToken ct = default);
}
