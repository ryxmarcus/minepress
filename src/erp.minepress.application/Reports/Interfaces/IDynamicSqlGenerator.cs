using erp.minepress.application.Reports.Dto;

namespace erp.minepress.application.Reports.Interfaces;

/// <summary>
/// Generates parameterized dynamic SQL from report metadata.
/// Supports SELECT, LEFT JOIN, WHERE, GROUP BY, HAVING, ORDER BY.
/// </summary>
public interface IDynamicSqlGenerator
{
    /// <summary>
    /// Builds a fully parameterized SQL query from the report request metadata.
    /// </summary>
    Task<GeneratedQuery> GenerateAsync(ReportQueryRequest request, CancellationToken ct = default);

    /// <summary>
    /// Validates that the source table and requested columns exist in the database.
    /// </summary>
    Task<bool> ValidateTableAsync(string tableName, CancellationToken ct = default);

    /// <summary>
    /// Returns the valid column names for a given table.
    /// </summary>
    Task<List<string>> GetValidColumnsAsync(string tableName, CancellationToken ct = default);
}
