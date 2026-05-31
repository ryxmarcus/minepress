using System.Text;
using System.Text.Json;
using erp.minepress.application.Reports.Dto;
using erp.minepress.application.Reports.Interfaces;
using erp.minepress.persistence.Context;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace erp.minepress.persistence.Services;

/// <summary>
/// Generates parameterized dynamic SQL from report metadata.
/// Supports SELECT, LEFT JOIN, WHERE, GROUP BY, HAVING, ORDER BY.
/// </summary>
public class DynamicSqlGenerator : IDynamicSqlGenerator
{
    private const string Schema = "press_db";
    private readonly ApplicationDbContext _db;

    public DynamicSqlGenerator(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<bool> ValidateTableAsync(string tableName, CancellationToken ct = default)
    {
        var conn = _db.Database.GetDbConnection();
        var wasOpen = conn.State == System.Data.ConnectionState.Open;
        if (!wasOpen) await conn.OpenAsync(ct);
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema=@s AND table_name=@t";
            cmd.Parameters.Add(new NpgsqlParameter("@s", Schema));
            cmd.Parameters.Add(new NpgsqlParameter("@t", tableName));
            return Convert.ToInt64(await cmd.ExecuteScalarAsync(ct)) > 0;
        }
        finally
        {
            if (!wasOpen && conn.State == System.Data.ConnectionState.Open) await conn.CloseAsync();
        }
    }

    public async Task<List<string>> GetValidColumnsAsync(string tableName, CancellationToken ct = default)
    {
        var cols = new List<string>();
        var conn = _db.Database.GetDbConnection();
        var wasOpen = conn.State == System.Data.ConnectionState.Open;
        if (!wasOpen) await conn.OpenAsync(ct);
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT column_name FROM information_schema.columns WHERE table_schema=@s AND table_name=@t ORDER BY ordinal_position";
            cmd.Parameters.Add(new NpgsqlParameter("@s", Schema));
            cmd.Parameters.Add(new NpgsqlParameter("@t", tableName));
            using var rdr = await cmd.ExecuteReaderAsync(ct);
            while (await rdr.ReadAsync(ct)) cols.Add(rdr.GetString(0));
        }
        finally
        {
            if (!wasOpen && conn.State == System.Data.ConnectionState.Open) await conn.CloseAsync();
        }
        return cols;
    }

    public async Task<GeneratedQuery> GenerateAsync(ReportQueryRequest request, CancellationToken ct = default)
    {
        var validColumns = await GetValidColumnsAsync(request.SourceTable, ct);
        var joinDefs = request.JoinedTables ?? [];

        // Build column map for joined tables
        var joinColumnMap = new Dictionary<string, List<string>>();
        foreach (var jt in joinDefs)
        {
            if (!await ValidateTableAsync(jt.Table, ct)) continue;
            joinColumnMap[jt.Table] = await GetValidColumnsAsync(jt.Table, ct);
        }

        // All valid columns: primary table + joined tables (prefixed)
        var allValidColumns = new HashSet<string>(validColumns.Select(c => c.ToLowerInvariant()));
        foreach (var (tbl, cols) in joinColumnMap)
            foreach (var c in cols)
                allValidColumns.Add($"{tbl}.{c}".ToLowerInvariant());

        // Resolve selected columns
        var selectedCols = (request.Columns ?? [])
            .Where(c => allValidColumns.Contains(c.ToLowerInvariant()) || validColumns.Contains(c.ToLowerInvariant()))
            .ToList();
        if (selectedCols.Count == 0)
            selectedCols = validColumns.Take(10).ToList();

        // FROM clause with JOINs
        string fromClause = BuildFromClause(request.SourceTable, joinDefs);
        string? joinClauseCapture = joinDefs.Count > 0 ? BuildJoinOnly(request.SourceTable, joinDefs) : null;

        var parameters = new List<QueryParameter>();
        var countParameters = new List<QueryParameter>();
        int paramIdx = 0;

        bool isSummary = request.ReportType?.Equals("summary", StringComparison.OrdinalIgnoreCase) == true
                         || (request.GroupByColumns?.Count > 0);

        var sb = new StringBuilder();
        string? whereClause = null;
        string? groupByClause = null;
        string? havingClause = null;
        string? orderByClause = null;

        if (isSummary)
        {
            var groupCols = (request.GroupByColumns ?? [])
                .Where(c => validColumns.Contains(c.ToLowerInvariant()) || allValidColumns.Contains(c.ToLowerInvariant()))
                .ToList();

            var selectParts = new List<string>();
            foreach (var col in groupCols)
                selectParts.Add($"{QualifyCol(col, request.SourceTable)} AS \"{ColAlias(col)}\"");

            foreach (var agg in request.Aggregates ?? [])
            {
                if (!allValidColumns.Contains(agg.Column.ToLowerInvariant()) &&
                    !validColumns.Contains(agg.Column.ToLowerInvariant())) continue;
                var fn = SafeAggFunction(agg.Function);
                selectParts.Add($"{fn}({QualifyCol(agg.Column, request.SourceTable)}) AS \"{ColAlias(agg.Column)}_{fn.ToLowerInvariant()}\"");
            }

            if (selectParts.Count == 0)
                selectParts.Add("COUNT(*) AS \"row_count\"");

            sb.Append($"SELECT {string.Join(", ", selectParts)} FROM {fromClause}");

            // WHERE
            whereClause = BuildWhereClause(request.Filters, validColumns, request.SourceTable, joinColumnMap, parameters, ref paramIdx);
            if (!string.IsNullOrEmpty(whereClause))
                sb.Append($" WHERE {whereClause}");

            // GROUP BY
            if (groupCols.Count > 0)
            {
                groupByClause = string.Join(", ", groupCols.Select(c => QualifyCol(c, request.SourceTable)));
                sb.Append($" GROUP BY {groupByClause}");
            }

            // HAVING
            havingClause = BuildHavingClause(request.HavingClauses, validColumns, allValidColumns, request.SourceTable, parameters, ref paramIdx);
            if (!string.IsNullOrEmpty(havingClause))
                sb.Append($" HAVING {havingClause}");

            // ORDER BY
            orderByClause = BuildOrderByClause(request.OrderByColumns, allValidColumns, validColumns, request.SourceTable);
            if (!string.IsNullOrEmpty(orderByClause))
                sb.Append($" ORDER BY {orderByClause}");
        }
        else
        {
            // Detail report
            var selectParts = selectedCols.Select(c => $"{QualifyCol(c, request.SourceTable)} AS \"{ColAlias(c)}\"");
            sb.Append($"SELECT {string.Join(", ", selectParts)} FROM {fromClause}");

            // WHERE
            whereClause = BuildWhereClause(request.Filters, validColumns, request.SourceTable, joinColumnMap, parameters, ref paramIdx);
            if (!string.IsNullOrEmpty(whereClause))
                sb.Append($" WHERE {whereClause}");

            // ORDER BY
            orderByClause = BuildOrderByClause(request.OrderByColumns, allValidColumns, validColumns, request.SourceTable);
            if (!string.IsNullOrEmpty(orderByClause))
                sb.Append($" ORDER BY {orderByClause}");
            else
                sb.Append($" ORDER BY {QualifyCol(selectedCols[0], request.SourceTable)}");
        }

        // Count SQL
        var countSb = new StringBuilder();
        countSb.Append($"SELECT COUNT(*) FROM {fromClause}");
        int cIdx = 0;
        var countWhere = BuildWhereClause(request.Filters, validColumns, request.SourceTable, joinColumnMap, countParameters, ref cIdx);
        if (!string.IsNullOrEmpty(countWhere))
            countSb.Append($" WHERE {countWhere}");

        // Pagination for detail reports
        int page = Math.Max(1, request.Page);
        int pageSize = Math.Clamp(request.PageSize, 1, 500);
        if (!isSummary)
            sb.Append($" LIMIT {pageSize} OFFSET {(page - 1) * pageSize}");

        return new GeneratedQuery
        {
            FullSql = sb.ToString(),
            CountSql = countSb.ToString(),
            SelectClause = sb.ToString(),
            FromClause = fromClause,
            JoinClause = joinClauseCapture,
            WhereClause = whereClause,
            GroupByClause = groupByClause,
            HavingClause = havingClause,
            OrderByClause = orderByClause,
            SelectedColumns = JsonSerializer.Serialize(selectedCols),
            FilterJson = request.Filters != null ? JsonSerializer.Serialize(request.Filters) : null,
            Parameters = parameters,
            CountParameters = countParameters,
            IsSummary = isSummary,
            PageSize = pageSize,
            Page = page
        };
    }

    // ── SQL Clause Builders ──

    private static string BuildFromClause(string sourceTable, List<JoinDefinition> joinDefs)
    {
        var sb = new StringBuilder($"{Schema}.\"{sourceTable}\"");
        foreach (var jt in joinDefs)
        {
            var joinType = jt.JoinType?.ToUpperInvariant() switch
            {
                "INNER" => "INNER JOIN",
                "RIGHT" => "RIGHT JOIN",
                "FULL" => "FULL JOIN",
                _ => "LEFT JOIN"
            };
            sb.Append($" {joinType} {Schema}.\"{jt.Table}\" ON {Schema}.\"{sourceTable}\".\"{jt.FkColumn}\" = {Schema}.\"{jt.Table}\".\"{jt.PkColumn}\"");
        }
        return sb.ToString();
    }

    private static string? BuildJoinOnly(string sourceTable, List<JoinDefinition> joinDefs)
    {
        if (joinDefs.Count == 0) return null;
        var sb = new StringBuilder();
        foreach (var jt in joinDefs)
        {
            var joinType = jt.JoinType?.ToUpperInvariant() switch
            {
                "INNER" => "INNER JOIN",
                "RIGHT" => "RIGHT JOIN",
                "FULL" => "FULL JOIN",
                _ => "LEFT JOIN"
            };
            if (sb.Length > 0) sb.Append(' ');
            sb.Append($"{joinType} {Schema}.\"{jt.Table}\" ON {Schema}.\"{sourceTable}\".\"{jt.FkColumn}\" = {Schema}.\"{jt.Table}\".\"{jt.PkColumn}\"");
        }
        return sb.ToString();
    }

    private static string? BuildWhereClause(
        List<ReportFilterItem>? filters,
        List<string> validColumns,
        string sourceTable,
        Dictionary<string, List<string>> joinColumnMap,
        List<QueryParameter> parameters,
        ref int paramIdx)
    {
        if (filters == null || filters.Count == 0) return null;

        var clauses = new List<string>();
        foreach (var f in filters)
        {
            string col;
            var colLower = f.ColumnName.ToLowerInvariant();

            if (f.ColumnName.Contains('.'))
            {
                var parts = f.ColumnName.Split('.', 2);
                if (!joinColumnMap.TryGetValue(parts[0], out var joinCols) ||
                    !joinCols.Contains(parts[1], StringComparer.OrdinalIgnoreCase)) continue;
                col = $"{Schema}.\"{parts[0]}\".\"{parts[1]}\"";
            }
            else
            {
                if (!validColumns.Contains(colLower)) continue;
                col = $"{Schema}.\"{sourceTable}\".\"{f.ColumnName}\"";
            }

            var pName = $"@p{paramIdx++}";

            switch (f.Operator.ToLowerInvariant())
            {
                case "eq":
                    clauses.Add($"{col} = {pName}");
                    parameters.Add(new QueryParameter { Name = pName, Value = CoerceFilterValue(f.FilterValue) });
                    break;
                case "neq":
                    clauses.Add($"{col} != {pName}");
                    parameters.Add(new QueryParameter { Name = pName, Value = CoerceFilterValue(f.FilterValue) });
                    break;
                case "gt":
                    clauses.Add($"{col} > {pName}");
                    parameters.Add(new QueryParameter { Name = pName, Value = CoerceFilterValue(f.FilterValue) });
                    break;
                case "gte":
                    clauses.Add($"{col} >= {pName}");
                    parameters.Add(new QueryParameter { Name = pName, Value = CoerceFilterValue(f.FilterValue) });
                    break;
                case "lt":
                    clauses.Add($"{col} < {pName}");
                    parameters.Add(new QueryParameter { Name = pName, Value = CoerceFilterValue(f.FilterValue) });
                    break;
                case "lte":
                    clauses.Add($"{col} <= {pName}");
                    parameters.Add(new QueryParameter { Name = pName, Value = CoerceFilterValue(f.FilterValue) });
                    break;
                case "contains":
                    clauses.Add($"{col}::text ILIKE {pName}");
                    parameters.Add(new QueryParameter { Name = pName, Value = $"%{f.FilterValue}%" });
                    break;
                case "startswith":
                    clauses.Add($"{col}::text ILIKE {pName}");
                    parameters.Add(new QueryParameter { Name = pName, Value = $"{f.FilterValue}%" });
                    break;
                case "endswith":
                    clauses.Add($"{col}::text ILIKE {pName}");
                    parameters.Add(new QueryParameter { Name = pName, Value = $"%{f.FilterValue}" });
                    break;
                case "isnull":
                    clauses.Add($"{col} IS NULL");
                    break;
                case "isnotnull":
                    clauses.Add($"{col} IS NOT NULL");
                    break;
                case "between":
                    var pName2 = $"@p{paramIdx++}";
                    clauses.Add($"{col} BETWEEN {pName} AND {pName2}");
                    parameters.Add(new QueryParameter { Name = pName, Value = CoerceFilterValue(f.FilterValue) });
                    parameters.Add(new QueryParameter { Name = pName2, Value = CoerceFilterValue(f.FilterValue2) });
                    break;
                case "in":
                    var inValues = f.FilterValue?.Split(',').Select(v => v.Trim()).ToList() ?? [];
                    if (inValues.Count > 0)
                    {
                        var inParams = new List<string>();
                        foreach (var v in inValues)
                        {
                            var ip = $"@p{paramIdx++}";
                            inParams.Add(ip);
                            parameters.Add(new QueryParameter { Name = ip, Value = CoerceFilterValue(v) });
                        }
                        clauses.Add($"{col} IN ({string.Join(", ", inParams)})");
                    }
                    break;
                default:
                    clauses.Add($"{col} = {pName}");
                    parameters.Add(new QueryParameter { Name = pName, Value = CoerceFilterValue(f.FilterValue) });
                    break;
            }
        }

        if (clauses.Count == 0) return null;

        var where = clauses[0];
        for (int i = 1; i < clauses.Count; i++)
        {
            var logic = (i < filters.Count && filters[i].LogicOperator?.ToUpperInvariant() == "OR") ? "OR" : "AND";
            where = $"{where} {logic} {clauses[i]}";
        }
        return where;
    }

    private static string? BuildHavingClause(
        List<ReportHavingItem>? havingClauses,
        List<string> validColumns,
        HashSet<string> allValidColumns,
        string sourceTable,
        List<QueryParameter> parameters,
        ref int paramIdx)
    {
        if (havingClauses == null || havingClauses.Count == 0) return null;

        var clauses = new List<string>();
        foreach (var h in havingClauses)
        {
            if (!allValidColumns.Contains(h.Column.ToLowerInvariant()) &&
                !validColumns.Contains(h.Column.ToLowerInvariant())) continue;

            var fn = SafeAggFunction(h.AggregateFunction);
            var qualCol = QualifyCol(h.Column, sourceTable);
            var pName = $"@p{paramIdx++}";

            var op = h.Operator.ToLowerInvariant() switch
            {
                "eq" => "=",
                "neq" => "!=",
                "gt" => ">",
                "gte" => ">=",
                "lt" => "<",
                "lte" => "<=",
                _ => ">"
            };

            clauses.Add($"{fn}({qualCol}) {op} {pName}");
            parameters.Add(new QueryParameter { Name = pName, Value = CoerceFilterValue(h.Value) });
        }

        return clauses.Count > 0 ? string.Join(" AND ", clauses) : null;
    }

    private static string? BuildOrderByClause(
        List<ReportOrderItem>? orderByColumns,
        HashSet<string> allValidColumns,
        List<string> validColumns,
        string sourceTable)
    {
        if (orderByColumns == null || orderByColumns.Count == 0) return null;

        var parts = orderByColumns
            .Where(o => allValidColumns.Contains(o.Column.ToLowerInvariant()) || validColumns.Contains(o.Column.ToLowerInvariant()))
            .Select(o => $"{QualifyCol(o.Column, sourceTable)} {(o.Dir?.ToUpperInvariant() == "DESC" ? "DESC" : "ASC")}")
            .ToList();

        return parts.Count > 0 ? string.Join(", ", parts) : null;
    }

    // ── Utility Methods ──

    private static string QualifyCol(string col, string sourceTable)
    {
        if (col.Contains('.'))
        {
            var parts = col.Split('.', 2);
            return $"{Schema}.\"{parts[0]}\".\"{parts[1]}\"";
        }
        return $"{Schema}.\"{sourceTable}\".\"{col}\"";
    }

    private static string ColAlias(string col) => col.Contains('.') ? col.Replace('.', '_') : col;

    private static string SafeAggFunction(string fn) =>
        fn?.ToUpperInvariant() switch
        {
            "SUM" => "SUM",
            "AVG" => "AVG",
            "MIN" => "MIN",
            "MAX" => "MAX",
            _ => "COUNT"
        };

    private static object CoerceFilterValue(string? value)
    {
        if (value == null) return DBNull.Value;
        if (long.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var l)) return l;
        if (decimal.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var d)) return d;
        if (DateTime.TryParse(value, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var dt)) return dt;
        if (bool.TryParse(value, out var b)) return b;
        return value;
    }
}
