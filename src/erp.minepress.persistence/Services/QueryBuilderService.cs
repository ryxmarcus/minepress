using System.Diagnostics;
using System.Text.Json;
using erp.minepress.application.Common.Models;
using erp.minepress.application.Reports.Dto;
using erp.minepress.application.Reports.Interfaces;
using erp.minepress.persistence.Context;
using erp.minepress.persistence.Models;
using Microsoft.Extensions.Logging;

namespace erp.minepress.persistence.Services;

/// <summary>
/// Orchestrates the full report query lifecycle: validation, SQL generation,
/// execution, totals computation, and query plan storage in rpt_query_plan.
/// </summary>
public class QueryBuilderService : IQueryBuilderService
{
    private readonly IDynamicSqlGenerator _sqlGenerator;
    private readonly IReportExecutionService _executionService;
    private readonly ApplicationDbContext _db;
    private readonly ILogger<QueryBuilderService> _logger;

    public QueryBuilderService(
        IDynamicSqlGenerator sqlGenerator,
        IReportExecutionService executionService,
        ApplicationDbContext db,
        ILogger<QueryBuilderService> logger)
    {
        _sqlGenerator = sqlGenerator;
        _executionService = executionService;
        _db = db;
        _logger = logger;
    }

    public async Task<Result<ReportQueryResult>> BuildAndExecuteAsync(ReportQueryRequest request, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();

        // 1. Validate source table
        if (!await _sqlGenerator.ValidateTableAsync(request.SourceTable, ct))
            return Result<ReportQueryResult>.Failure("Invalid or non-existent source table.");

        // 2. Validate joined tables
        foreach (var jt in request.JoinedTables ?? [])
        {
            if (!await _sqlGenerator.ValidateTableAsync(jt.Table, ct))
                return Result<ReportQueryResult>.Failure($"Invalid joined table: {jt.Table}");
        }

        // 3. Generate SQL
        GeneratedQuery query;
        try
        {
            query = await _sqlGenerator.GenerateAsync(request, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SQL generation failed for table {Table}", request.SourceTable);
            return Result<ReportQueryResult>.Failure($"SQL generation failed: {ex.Message}");
        }

        // 4. Execute query
        ReportQueryResult result;
        try
        {
            result = await _executionService.ExecuteAsync(query, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Query execution failed for table {Table}", request.SourceTable);
            return Result<ReportQueryResult>.Failure($"Query execution failed: {ex.Message}");
        }

        // 5. Compute totals if requested
        if (request.ShowTotals || request.ShowGrandTotal)
        {
            result.Totals = await _executionService.ComputeTotalsAsync(query, request, ct);
        }

        sw.Stop();
        result.ExecutionTimeMs = (int)sw.ElapsedMilliseconds;

        // 6. Store query plan for auditing and caching
        try
        {
            var queryPlan = new RptQueryPlan
            {
                ReportId = request.ReportId,
                ReportName = request.ReportId?.ToString() ?? request.SourceTable,
                SourceTable = request.SourceTable,
                GeneratedSql = query.FullSql,
                JoinClause = query.JoinClause,
                WhereClause = query.WhereClause,
                GroupByClause = query.GroupByClause,
                HavingClause = query.HavingClause,
                OrderByClause = query.OrderByClause,
                SelectedColumns = query.SelectedColumns,
                FilterJson = query.FilterJson,
                ParametersJson = JsonSerializer.Serialize(query.Parameters.Select(p => new { p.Name, Value = p.Value?.ToString() })),
                RowCount = result.TotalCount,
                ExecutionTimeMs = result.ExecutionTimeMs,
                ExecutedBy = request.ExecutedBy ?? "System",
                ExecutedOn = DateTime.Now,
                IsActive = true
            };

            _db.RptQueryPlans.Add(queryPlan);
            await _db.SaveChangesAsync(ct);
            result.QueryPlanId = queryPlan.QueryPlanId;
        }
        catch (Exception ex)
        {
            // Query plan storage is non-critical — log and continue
            _logger.LogWarning(ex, "Failed to store query plan for table {Table}", request.SourceTable);
        }

        return Result<ReportQueryResult>.Success(result);
    }
}
