using System.Diagnostics;
using System.Text;
using erp.minepress.application.Reports.Dto;
using erp.minepress.application.Reports.Interfaces;
using erp.minepress.persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace erp.minepress.persistence.Services;

/// <summary>
/// Executes generated SQL against the PostgreSQL database and returns results.
/// </summary>
public class ReportExecutionService : IReportExecutionService
{
    private const string Schema = "press_db";
    private readonly ApplicationDbContext _db;
    private readonly ILogger<ReportExecutionService> _logger;

    public ReportExecutionService(ApplicationDbContext db, ILogger<ReportExecutionService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<ReportQueryResult> ExecuteAsync(GeneratedQuery query, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        var conn = _db.Database.GetDbConnection();
        await conn.OpenAsync(ct);
        try
        {
            // Total count
            long totalCount = 0;
            using (var countCmd = conn.CreateCommand())
            {
                countCmd.CommandText = query.CountSql;
                foreach (var p in query.CountParameters)
                    countCmd.Parameters.Add(new NpgsqlParameter(p.Name, p.Value ?? DBNull.Value));
                var countResult = await countCmd.ExecuteScalarAsync(ct);
                totalCount = Convert.ToInt64(countResult);
            }

            // Data rows
            var rows = new List<Dictionary<string, object?>>();
            var columnNames = new List<string>();

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = query.FullSql;
                foreach (var p in query.Parameters)
                    cmd.Parameters.Add(new NpgsqlParameter(p.Name, p.Value ?? DBNull.Value));

                using var rdr = await cmd.ExecuteReaderAsync(ct);
                columnNames = Enumerable.Range(0, rdr.FieldCount)
                    .Select(i => rdr.GetName(i))
                    .ToList();

                while (await rdr.ReadAsync(ct))
                {
                    var row = new Dictionary<string, object?>();
                    for (int i = 0; i < rdr.FieldCount; i++)
                        row[columnNames[i]] = rdr.IsDBNull(i) ? null : rdr.GetValue(i);
                    rows.Add(row);
                }
            }

            sw.Stop();

            return new ReportQueryResult
            {
                Data = rows,
                TotalCount = totalCount,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalPages = query.PageSize > 0 ? (int)Math.Ceiling((double)totalCount / query.PageSize) : 1,
                Sql = query.FullSql,
                ColumnNames = columnNames,
                ReportType = query.IsSummary ? "summary" : "detail",
                ExecutionTimeMs = (int)sw.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Report query execution failed. SQL: {Sql}", query.FullSql);
            throw;
        }
        finally
        {
            if (conn.State == System.Data.ConnectionState.Open) await conn.CloseAsync();
        }
    }

    public async Task<Dictionary<string, object?>?> ComputeTotalsAsync(
        GeneratedQuery query,
        ReportQueryRequest request,
        CancellationToken ct = default)
    {
        if (!request.ShowTotals && !request.ShowGrandTotal)
            return null;

        var conn = _db.Database.GetDbConnection();
        var wasOpen = conn.State == System.Data.ConnectionState.Open;
        if (!wasOpen) await conn.OpenAsync(ct);

        try
        {
            // Identify numeric columns from the selected set
            var selectedCols = System.Text.Json.JsonSerializer.Deserialize<List<string>>(query.SelectedColumns ?? "[]") ?? [];
            var joinDefs = request.JoinedTables ?? [];
            var numericCols = new List<string>();

            foreach (var col in selectedCols)
            {
                string table, column;
                if (col.Contains('.'))
                {
                    var parts = col.Split('.', 2);
                    table = parts[0];
                    column = parts[1];
                }
                else
                {
                    table = request.SourceTable;
                    column = col;
                }

                if (await IsNumericColumnAsync(table, column, conn, ct))
                    numericCols.Add(col);
            }

            if (numericCols.Count == 0) return null;

            var selectParts = numericCols.Select(c =>
            {
                var qualCol = QualifyCol(c, request.SourceTable);
                var alias = c.Contains('.') ? c.Replace('.', '_') : c;
                return $"COALESCE(SUM({qualCol})::numeric, 0) AS \"{alias}_sum\", COALESCE(AVG({qualCol})::numeric, 0) AS \"{alias}_avg\"";
            });

            var totalsSql = new StringBuilder();
            totalsSql.Append($"SELECT {string.Join(", ", selectParts)} FROM {query.FromClause}");
            if (!string.IsNullOrEmpty(query.WhereClause))
                totalsSql.Append($" WHERE {query.WhereClause}");

            var totalsRow = new Dictionary<string, object?>();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = totalsSql.ToString();

            // Re-create parameters for the totals query
            foreach (var p in query.Parameters)
            {
                // Only include WHERE-clause parameters (filter params)
                cmd.Parameters.Add(new NpgsqlParameter(p.Name, p.Value ?? DBNull.Value));
            }

            using var rdr = await cmd.ExecuteReaderAsync(ct);
            if (await rdr.ReadAsync(ct))
            {
                for (int i = 0; i < rdr.FieldCount; i++)
                    totalsRow[rdr.GetName(i)] = rdr.IsDBNull(i) ? null : rdr.GetValue(i);
            }

            return totalsRow;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to compute totals for report");
            return null;
        }
        finally
        {
            if (!wasOpen && conn.State == System.Data.ConnectionState.Open) await conn.CloseAsync();
        }
    }

    // ── Helpers ──

    private static string QualifyCol(string col, string sourceTable)
    {
        if (col.Contains('.'))
        {
            var parts = col.Split('.', 2);
            return $"{Schema}.\"{parts[0]}\".\"{parts[1]}\"";
        }
        return $"{Schema}.\"{sourceTable}\".\"{col}\"";
    }

    private static async Task<bool> IsNumericColumnAsync(string table, string column, System.Data.Common.DbConnection conn, CancellationToken ct)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT data_type FROM information_schema.columns WHERE table_schema=@s AND table_name=@t AND column_name=@c";
        cmd.Parameters.Add(new NpgsqlParameter("@s", Schema));
        cmd.Parameters.Add(new NpgsqlParameter("@t", table));
        cmd.Parameters.Add(new NpgsqlParameter("@c", column));
        var dt = (await cmd.ExecuteScalarAsync(ct))?.ToString() ?? "";
        return dt is "integer" or "bigint" or "smallint" or "numeric" or "real" or "double precision" or "decimal" or "money";
    }
}
