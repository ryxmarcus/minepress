using erp.minepress.application.Reports.Dto;
using erp.minepress.application.Reports.Interfaces;
using erp.minepress.infrastructure.ErrorLogging;
using erp.minepress.persistence.Context;
using erp.minepress.persistence.Models;
using erp.minepress.web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Text;
using System.Text.Json;

namespace erp.minepress.web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<ReportController> _logger;
    private readonly IQueryBuilderService _queryBuilderService;
    private readonly ISystemErrorLogger _systemErrorLogger;

    public ReportController(
        ApplicationDbContext db,
        ILogger<ReportController> logger,
        IQueryBuilderService queryBuilderService,
        ISystemErrorLogger systemErrorLogger)
    {
        _db = db;
        _logger = logger;
        _queryBuilderService = queryBuilderService;
        _systemErrorLogger = systemErrorLogger;
    }

    private async Task AuditExceptionAsync(Exception ex, string additionalData, string severity = "Error")
        => await _systemErrorLogger.LogAsync(ex, HttpContext, severity, additionalData);

    // ══════════════════════════════════════════════════════════════════════
    //  Schema Discovery
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Returns all user-facing tables/views grouped by category</summary>
    [HttpGet("tables")]
    public async Task<IActionResult> GetTables()
    {
        var sql = @"
            SELECT table_name, table_type
            FROM   information_schema.tables
            WHERE  table_schema = 'press_db'
              AND  table_type IN ('BASE TABLE','VIEW')
            ORDER  BY table_type, table_name";

        var tables = new List<object>();
        var conn = _db.Database.GetDbConnection();
        await conn.OpenAsync();
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                var name = rdr.GetString(0);
                var type = rdr.GetString(1);
                var category = CategorizeTable(name);
                var friendlyName = HumanizeName(name);
                tables.Add(new { name, friendlyName, type = type == "VIEW" ? "view" : "table", category });
            }
        }
        finally { if (conn.State == System.Data.ConnectionState.Open) await conn.CloseAsync(); }

        return Ok(tables);
    }

    /// <summary>Returns columns for a given table with data types</summary>
    [HttpGet("tables/{tableName}/columns")]
    public async Task<IActionResult> GetColumns(string tableName)
    {
        if (!await IsValidTable(tableName))
            return BadRequest(new { message = "Invalid table name." });

        var sql = @"
            SELECT c.column_name, c.data_type, c.is_nullable,
                   c.character_maximum_length, c.numeric_precision,
                   c.column_default,
                   COALESCE(pgd.description, '') AS column_comment
            FROM   information_schema.columns c
            LEFT   JOIN pg_catalog.pg_statio_all_tables st
                   ON st.schemaname = c.table_schema AND st.relname = c.table_name
            LEFT   JOIN pg_catalog.pg_description pgd
                   ON pgd.objoid = st.relid AND pgd.objsubid = c.ordinal_position
            WHERE  c.table_schema = 'press_db' AND c.table_name = @tbl
            ORDER  BY c.ordinal_position";

        var columns = new List<object>();
        var conn = _db.Database.GetDbConnection();
        await conn.OpenAsync();
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            var p = cmd.CreateParameter();
            p.ParameterName = "@tbl";
            p.Value = tableName;
            cmd.Parameters.Add(p);
            using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                columns.Add(new
                {
                    name = rdr.GetString(0),
                    displayName = HumanizeName(rdr.GetString(0)),
                    dataType = rdr.GetString(1),
                    isNullable = rdr.GetString(2) == "YES",
                    maxLength = rdr.IsDBNull(3) ? (int?)null : rdr.GetInt32(3),
                    precision = rdr.IsDBNull(4) ? (int?)null : rdr.GetInt32(4),
                    hasDefault = !rdr.IsDBNull(5),
                    comment = rdr.IsDBNull(6) ? "" : rdr.GetString(6),
                    isNumeric = IsNumericType(rdr.GetString(1)),
                    isDate = IsDateType(rdr.GetString(1)),
                    isBoolean = rdr.GetString(1) == "boolean"
                });
            }
        }
        finally { if (conn.State == System.Data.ConnectionState.Open) await conn.CloseAsync(); }

        return Ok(columns);
    }

    /// <summary>Returns FK relationships from a source table to other tables</summary>
    [HttpGet("tables/{tableName}/relationships")]
    public async Task<IActionResult> GetRelationships(string tableName)
    {
        if (!await IsValidTable(tableName))
            return BadRequest(new { message = "Invalid table name." });

        var sql = @"
            SELECT
                kcu.column_name       AS fk_column,
                ccu.table_name        AS referenced_table,
                ccu.column_name       AS referenced_column,
                tc.constraint_name
            FROM   information_schema.table_constraints tc
            JOIN   information_schema.key_column_usage kcu
                   ON tc.constraint_name = kcu.constraint_name
                   AND tc.table_schema  = kcu.table_schema
            JOIN   information_schema.constraint_column_usage ccu
                   ON ccu.constraint_name = tc.constraint_name
                   AND ccu.table_schema   = tc.table_schema
            WHERE  tc.constraint_type = 'FOREIGN KEY'
              AND  tc.table_schema    = 'press_db'
              AND  tc.table_name      = @tbl
            ORDER  BY kcu.column_name";

        var rels = new List<object>();
        var conn = _db.Database.GetDbConnection();
        await conn.OpenAsync();
        try
        {
            // Outgoing FK references
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.Parameters.Add(new NpgsqlParameter("@tbl", tableName));
                using var rdr = await cmd.ExecuteReaderAsync();
                while (await rdr.ReadAsync())
                {
                    rels.Add(new
                    {
                        fkColumn = rdr.GetString(0),
                        referencedTable = rdr.GetString(1),
                        referencedTableName = HumanizeName(rdr.GetString(1)),
                        referencedColumn = rdr.GetString(2),
                        constraintName = rdr.GetString(3)
                    });
                }
            }

            // Reverse FK references (tables that reference this table)
            var reverseRels = new List<object>();
            using (var cmd2 = conn.CreateCommand())
            {
                cmd2.CommandText = @"
                    SELECT
                        kcu.table_name        AS referencing_table,
                        kcu.column_name       AS referencing_column,
                        ccu.column_name       AS pk_column,
                        tc.constraint_name
                    FROM   information_schema.table_constraints tc
                    JOIN   information_schema.key_column_usage kcu
                           ON tc.constraint_name = kcu.constraint_name
                           AND tc.table_schema  = kcu.table_schema
                    JOIN   information_schema.constraint_column_usage ccu
                           ON ccu.constraint_name = tc.constraint_name
                           AND ccu.table_schema   = tc.table_schema
                    WHERE  tc.constraint_type = 'FOREIGN KEY'
                      AND  tc.table_schema    = 'press_db'
                      AND  ccu.table_name     = @tbl
                    ORDER  BY kcu.table_name";
                cmd2.Parameters.Add(new NpgsqlParameter("@tbl", tableName));
                using var rdr2 = await cmd2.ExecuteReaderAsync();
                while (await rdr2.ReadAsync())
                {
                    reverseRels.Add(new
                    {
                        referencingTable = rdr2.GetString(0),
                        referencingTableName = HumanizeName(rdr2.GetString(0)),
                        referencingColumn = rdr2.GetString(1),
                        pkColumn = rdr2.GetString(2),
                        constraintName = rdr2.GetString(3)
                    });
                }
            }

            return Ok(new { outgoing = rels, incoming = reverseRels });
        }
        finally { if (conn.State == System.Data.ConnectionState.Open) await conn.CloseAsync(); }
    }

    // ══════════════════════════════════════════════════════════════════════
    //  Dynamic Query Execution (Service Layer)
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Executes a report using the Query Builder Engine service layer.
    /// Builds dynamic SQL, executes, computes totals, and stores the query plan.
    /// </summary>
    [HttpPost("execute")]
    public async Task<IActionResult> ExecuteReport([FromBody] ReportExecuteDto dto)
    {
        var user = HttpContext.Session.GetObject<UserSessionData>("CurrentUser");
        var userName = user?.Name ?? "System";

        var request = new ReportQueryRequest
        {
            ReportId = null,
            SourceTable = dto.SourceTable,
            Columns = dto.Columns,
            Filters = dto.Filters?.Select(f => new ReportFilterItem
            {
                ColumnName = f.ColumnName,
                Operator = f.Operator,
                FilterValue = f.FilterValue,
                FilterValue2 = f.FilterValue2,
                LogicOperator = f.LogicOperator
            }).ToList(),
            OrderByColumns = dto.OrderByColumns?.Select(o => new ReportOrderItem
            {
                Column = o.Column,
                Dir = o.Dir
            }).ToList(),
            GroupByColumns = dto.GroupByColumns,
            Aggregates = dto.Aggregates?.Select(a => new ReportAggregateItem
            {
                Column = a.Column,
                Function = a.Function
            }).ToList(),
            JoinedTables = dto.JoinedTables?.Select(j => new JoinDefinition
            {
                Table = j.Table,
                JoinType = j.JoinType,
                FkColumn = j.FkColumn,
                PkColumn = j.PkColumn
            }).ToList(),
            ReportType = dto.ReportType,
            ShowTotals = dto.ShowTotals,
            ShowGrandTotal = dto.ShowGrandTotal,
            Page = dto.Page,
            PageSize = dto.PageSize,
            ExecutedBy = userName
        };

        var result = await _queryBuilderService.BuildAndExecuteAsync(request);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        var data = result.Data!;
        return Ok(new
        {
            data = data.Data,
            totalCount = data.TotalCount,
            page = data.Page,
            pageSize = data.PageSize,
            totalPages = data.TotalPages,
            sql = data.Sql,
            columnNames = data.ColumnNames,
            totals = data.Totals,
            reportType = data.ReportType,
            queryPlanId = data.QueryPlanId,
            executionTimeMs = data.ExecutionTimeMs
        });
    }

    /// <summary>
    /// Legacy direct execution — kept for backward compatibility with export endpoints.
    /// </summary>
    [HttpPost("execute-legacy")]
    public async Task<IActionResult> ExecuteReportLegacy([FromBody] ReportExecuteDto dto)
    {
        if (!await IsValidTable(dto.SourceTable))
            return BadRequest(new { message = "Invalid table name." });

        var validColumns = await GetValidColumns(dto.SourceTable);

        // Build merged column map for joined tables { qualifiedCol -> alias }
        var joinDefs = dto.JoinedTables ?? [];
        var joinColumnMap = new Dictionary<string, List<string>>(); // table -> columns
        foreach (var jt in joinDefs)
        {
            if (!await IsValidTable(jt.Table)) continue;
            joinColumnMap[jt.Table] = await GetValidColumns(jt.Table);
        }

        // All valid columns: primary table + joined tables (prefixed)
        var allValidColumns = new HashSet<string>(validColumns.Select(c => c.ToLowerInvariant()));
        foreach (var (tbl, cols) in joinColumnMap)
            foreach (var c in cols)
                allValidColumns.Add($"{tbl}.{c}".ToLowerInvariant());

        var selectedCols = (dto.Columns ?? []).Where(c =>
            allValidColumns.Contains(c.ToLowerInvariant()) ||
            validColumns.Contains(c.ToLowerInvariant())
        ).ToList();
        if (selectedCols.Count == 0)
            selectedCols = validColumns.Take(10).ToList();

        // Build FROM clause with JOINs
        string fromClause = BuildFromClause(dto.SourceTable, joinDefs);

        // Qualify column references
        string QualifyCol(string col)
        {
            if (col.Contains('.'))
            {
                var parts = col.Split('.', 2);
                return $"press_db.\"{parts[0]}\".\"{parts[1]}\"";
            }
            return $"press_db.\"{dto.SourceTable}\".\"{col}\"";
        }

        string ColAlias(string col) => col.Contains('.') ? col.Replace('.', '_') : col;

        // Build SQL
        var sb = new StringBuilder();
        var parameters = new List<NpgsqlParameter>();
        int paramIdx = 0;

        bool isSummary = dto.ReportType?.ToLowerInvariant() == "summary";

        if (isSummary || dto.GroupByColumns?.Count > 0)
        {
            var groupCols = (dto.GroupByColumns ?? []).Where(c =>
                validColumns.Contains(c.ToLowerInvariant()) ||
                allValidColumns.Contains(c.ToLowerInvariant())
            ).ToList();

            var selectParts = new List<string>();
            foreach (var col in groupCols)
                selectParts.Add($"{QualifyCol(col)} AS \"{ColAlias(col)}\"");

            foreach (var agg in dto.Aggregates ?? [])
            {
                if (!allValidColumns.Contains(agg.Column.ToLowerInvariant()) &&
                    !validColumns.Contains(agg.Column.ToLowerInvariant())) continue;
                var fn = SafeAggFunction(agg.Function);
                selectParts.Add($"{fn}({QualifyCol(agg.Column)}) AS \"{ColAlias(agg.Column)}_{fn.ToLowerInvariant()}\"");
            }

            if (selectParts.Count == 0)
                selectParts.Add("COUNT(*) AS \"row_count\"");

            sb.Append($"SELECT {string.Join(", ", selectParts)} FROM {fromClause}");
            AppendWhereClause(sb, parameters, dto.Filters, validColumns, ref paramIdx, dto.SourceTable, joinColumnMap);
            if (groupCols.Count > 0)
                sb.Append($" GROUP BY {string.Join(", ", groupCols.Select(c => QualifyCol(c)))}");

            if (dto.OrderByColumns?.Count > 0)
            {
                var orderParts = dto.OrderByColumns
                    .Where(o => allValidColumns.Contains(o.Column.ToLowerInvariant()) || validColumns.Contains(o.Column.ToLowerInvariant()))
                    .Select(o => $"{QualifyCol(o.Column)} {(o.Dir?.ToUpperInvariant() == "DESC" ? "DESC" : "ASC")}");
                if (orderParts.Any())
                    sb.Append($" ORDER BY {string.Join(", ", orderParts)}");
            }
        }
        else
        {
            var selectParts = selectedCols.Select(c => $"{QualifyCol(c)} AS \"{ColAlias(c)}\"");
            sb.Append($"SELECT {string.Join(", ", selectParts)} FROM {fromClause}");

            AppendWhereClause(sb, parameters, dto.Filters, validColumns, ref paramIdx, dto.SourceTable, joinColumnMap);

            if (dto.OrderByColumns?.Count > 0)
            {
                var orderParts = dto.OrderByColumns
                    .Where(o => allValidColumns.Contains(o.Column.ToLowerInvariant()) || validColumns.Contains(o.Column.ToLowerInvariant()))
                    .Select(o => $"{QualifyCol(o.Column)} {(o.Dir?.ToUpperInvariant() == "DESC" ? "DESC" : "ASC")}");
                if (orderParts.Any())
                    sb.Append($" ORDER BY {string.Join(", ", orderParts)}");
            }
            else
            {
                sb.Append($" ORDER BY {QualifyCol(selectedCols[0])}");
            }
        }

        // Count query
        var countSb = new StringBuilder();
        countSb.Append($"SELECT COUNT(*) FROM {fromClause}");
        var countParams = new List<NpgsqlParameter>();
        int cIdx = 0;
        AppendWhereClause(countSb, countParams, dto.Filters, validColumns, ref cIdx, dto.SourceTable, joinColumnMap);

        // Pagination
        int page = Math.Max(1, dto.Page);
        int pageSize = Math.Clamp(dto.PageSize, 1, 500);
        if (!isSummary && (dto.GroupByColumns == null || dto.GroupByColumns.Count == 0))
        {
            sb.Append($" LIMIT {pageSize} OFFSET {(page - 1) * pageSize}");
        }

        // Execute
        var conn = _db.Database.GetDbConnection();
        await conn.OpenAsync();
        try
        {
            // Total count
            long totalCount = 0;
            using (var countCmd = conn.CreateCommand())
            {
                countCmd.CommandText = countSb.ToString();
                foreach (var p in countParams)
                    countCmd.Parameters.Add(new NpgsqlParameter(p.ParameterName, p.Value));
                var countResult = await countCmd.ExecuteScalarAsync();
                totalCount = Convert.ToInt64(countResult);
            }

            // Data
            var rows = new List<Dictionary<string, object?>>();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sb.ToString();
                foreach (var p in parameters)
                    cmd.Parameters.Add(p);

                using var rdr = await cmd.ExecuteReaderAsync();
                var colNames = Enumerable.Range(0, rdr.FieldCount).Select(i => rdr.GetName(i)).ToList();
                while (await rdr.ReadAsync())
                {
                    var row = new Dictionary<string, object?>();
                    for (int i = 0; i < rdr.FieldCount; i++)
                        row[colNames[i]] = rdr.IsDBNull(i) ? null : rdr.GetValue(i);
                    rows.Add(row);
                }
            }

            // Totals computation
            Dictionary<string, object?>? totalsRow = null;
            if (dto.ShowTotals || dto.ShowGrandTotal)
            {
                totalsRow = await ComputeTotals(conn, fromClause, dto, validColumns, joinColumnMap, selectedCols, QualifyCol, ColAlias);
            }

            var resultColNames = rows.Count > 0 ? rows[0].Keys.ToList() : selectedCols.Select(ColAlias).ToList();

            return Ok(new
            {
                data = rows,
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                sql = sb.ToString(),
                columnNames = resultColNames,
                totals = totalsRow,
                reportType = dto.ReportType ?? "detail"
            });
        }
        finally { if (conn.State == System.Data.ConnectionState.Open) await conn.CloseAsync(); }
    }

    // ══════════════════════════════════════════════════════════════════════
    //  AI Summary
    // ══════════════════════════════════════════════════════════════════════

    [HttpPost("ai-summary")]
    public async Task<IActionResult> AiSummary([FromBody] ReportExecuteDto dto)
    {
        if (!await IsValidTable(dto.SourceTable))
            return BadRequest(new { message = "Invalid table name." });

        var validColumns = await GetValidColumns(dto.SourceTable);
        var selectedCols = (dto.Columns ?? []).Where(c => validColumns.Contains(c.ToLowerInvariant())).ToList();
        if (selectedCols.Count == 0) selectedCols = validColumns.Take(8).ToList();

        // Get count + sample + aggregates
        var conn = _db.Database.GetDbConnection();
        await conn.OpenAsync();
        try
        {
            long totalRows = 0;
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"SELECT COUNT(*) FROM press_db.\"{dto.SourceTable}\"";
                totalRows = Convert.ToInt64(await cmd.ExecuteScalarAsync());
            }

            // Numeric column stats
            var numericCols = new List<string>();
            foreach (var c in selectedCols)
            {
                if (await IsNumericColumnFromDbAsync(dto.SourceTable, c, conn))
                    numericCols.Add(c);
            }
            var stats = new List<object>();
            foreach (var col in numericCols.Take(5))
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = $"SELECT COUNT(\"{col}\"), COALESCE(SUM(\"{col}\")::numeric,0), COALESCE(AVG(\"{col}\")::numeric,0), COALESCE(MIN(\"{col}\")::numeric,0), COALESCE(MAX(\"{col}\")::numeric,0) FROM press_db.\"{dto.SourceTable}\"";
                using var rdr = await cmd.ExecuteReaderAsync();
                if (await rdr.ReadAsync())
                {
                    stats.Add(new
                    {
                        column = HumanizeName(col),
                        count = rdr.GetValue(0),
                        sum = rdr.GetDecimal(1),
                        avg = Math.Round(rdr.GetDecimal(2), 2),
                        min = rdr.GetDecimal(3),
                        max = rdr.GetDecimal(4)
                    });
                }
            }

            // Date range
            object? dateRange = null;
            var dateCols = new List<string>();
            foreach (var c in selectedCols)
            {
                if (await IsDateColumnFromDbAsync(dto.SourceTable, c, conn))
                {
                    dateCols.Add(c);
                    if (dateCols.Count >= 1) break;
                }
            }
            if (dateCols.Count > 0)
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = $"SELECT MIN(\"{dateCols[0]}\"), MAX(\"{dateCols[0]}\") FROM press_db.\"{dto.SourceTable}\"";
                using var rdr = await cmd.ExecuteReaderAsync();
                if (await rdr.ReadAsync() && !rdr.IsDBNull(0))
                {
                    dateRange = new { column = HumanizeName(dateCols[0]), from = rdr.GetValue(0), to = rdr.GetValue(1) };
                }
            }

            // Top values for first text column
            object? topValues = null;
            var textCols = selectedCols.Where(c => !numericCols.Contains(c) && !dateCols.Contains(c)).Take(1).ToList();
            if (textCols.Count > 0)
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = $"SELECT \"{textCols[0]}\", COUNT(*) as cnt FROM press_db.\"{dto.SourceTable}\" WHERE \"{textCols[0]}\" IS NOT NULL GROUP BY \"{textCols[0]}\" ORDER BY cnt DESC LIMIT 5";
                using var rdr = await cmd.ExecuteReaderAsync();
                var tvList = new List<object>();
                while (await rdr.ReadAsync())
                    tvList.Add(new { value = rdr.GetValue(0)?.ToString(), count = rdr.GetValue(1) });
                if (tvList.Count > 0)
                    topValues = new { column = HumanizeName(textCols[0]), values = tvList };
            }

            return Ok(new
            {
                tableName = HumanizeName(dto.SourceTable),
                totalRows,
                selectedColumns = selectedCols.Select(c => HumanizeName(c)),
                numericStats = stats,
                dateRange,
                topValues,
                generatedAt = DateTime.Now.ToString("dd-MMM-yyyy HH:mm")
            });
        }
        finally { if (conn.State == System.Data.ConnectionState.Open) await conn.CloseAsync(); }
    }

    // ══════════════════════════════════════════════════════════════════════
    //  Report CRUD
    // ══════════════════════════════════════════════════════════════════════

    [HttpGet("saved")]
    public async Task<IActionResult> GetSavedReports()
    {
        var user = HttpContext.Session.GetObject<UserSessionData>("CurrentUser");
        var userName = user?.Name ?? "System";

        var list = await _db.RptSavedReports
            .Where(r => r.IsActive && (r.CreatedBy == userName || r.IsShared))
            .OrderByDescending(r => r.CreatedOn)
            .Select(r => new
            {
                r.ReportId,
                r.ReportCode,
                r.ReportName,
                r.Description,
                r.SourceTable,
                sourceTableName = r.SourceTable,
                r.IsShared,
                r.IsDefault,
                r.ChartType,
                r.CreatedBy,
                createdOn = r.CreatedOn.ToString("dd-MMM-yyyy HH:mm"),
                columnCount = r.RptSavedReportColumns.Count(c => c.IsActive),
                filterCount = r.RptSavedReportFilters.Count(f => f.IsActive),
                isOwner = r.CreatedBy == userName
            })
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("saved/{id}")]
    public async Task<IActionResult> GetSavedReport(long id)
    {
        try
        {
            var report = await _db.RptSavedReports
                .Include(r => r.RptSavedReportColumns.Where(c => c.IsActive).OrderBy(c => c.ColumnOrder))
                .Include(r => r.RptSavedReportFilters.Where(f => f.IsActive).OrderBy(f => f.FilterOrder))
                .FirstOrDefaultAsync(r => r.ReportId == id && r.IsActive);

            if (report == null) return NotFound(new { message = "Report not found." });

            return Ok(new
            {
                report.ReportId,
                report.ReportCode,
                report.ReportName,
                report.Description,
                report.SourceTable,
                report.IsShared,
                report.IsDefault,
                report.ReportType,
                report.ShowTotals,
                report.ShowGrandTotal,
                report.JoinedTables,
                report.GroupByColumns,
                report.OrderByColumns,
                report.PageSize,
                report.ChartType,
                report.ChartConfig,
                report.AiSummaryPrompt,
                columns = report.RptSavedReportColumns.Select(c => new
                {
                    c.ReportColumnId,
                    c.ColumnName,
                    c.DisplayName,
                    c.ColumnOrder,
                    c.IsVisible,
                    c.AggregateFunction,
                    c.FormatString,
                    c.ColumnWidth
                }),
                filters = report.RptSavedReportFilters.Select(f => new
                {
                    f.ReportFilterId,
                    f.ColumnName,
                    f.Operator,
                    f.FilterValue,
                    f.FilterValue2,
                    f.FilterOrder,
                    f.LogicOperator
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load saved report {ReportId}", id);
            return StatusCode(500, new { message = "Failed to load report. Please try again." });
        }
    }

    [HttpPost("saved")]
    public async Task<IActionResult> SaveReport([FromBody] ReportSaveDto dto)
    {
        var user = HttpContext.Session.GetObject<UserSessionData>("CurrentUser");
        var userName = user?.Name ?? "System";

        // Check duplicate code
        if (dto.ReportId == 0)
        {
            var exists = await _db.RptSavedReports.AnyAsync(r => r.ReportCode == dto.ReportCode && r.IsActive);
            if (exists) return BadRequest(new { message = $"Report code '{dto.ReportCode}' already exists." });
        }

        RptSavedReport report;
        var jsonOpts = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        if (dto.ReportId > 0)
        {
            report = await _db.RptSavedReports
                .Include(r => r.RptSavedReportColumns)
                .Include(r => r.RptSavedReportFilters)
                .FirstOrDefaultAsync(r => r.ReportId == dto.ReportId);

            if (report == null) return NotFound(new { message = "Report not found." });

            report.ReportName = dto.ReportName;
            report.Description = dto.Description;
            report.SourceTable = dto.SourceTable;
            report.IsShared = dto.IsShared;
            report.ReportType = dto.ReportType;
            report.ShowTotals = dto.ShowTotals;
            report.ShowGrandTotal = dto.ShowGrandTotal;
            report.JoinedTables = dto.JoinedTables != null ? JsonSerializer.Serialize(dto.JoinedTables, jsonOpts) : null;
            report.GroupByColumns = dto.GroupByColumns != null ? JsonSerializer.Serialize(dto.GroupByColumns, jsonOpts) : null;
            report.OrderByColumns = dto.OrderByColumns != null ? JsonSerializer.Serialize(dto.OrderByColumns, jsonOpts) : null;
            report.PageSize = dto.PageSize;
            report.ChartType = dto.ChartType;
            report.ChartConfig = dto.ChartConfig;
            report.ModifiedBy = userName;
            report.ModifiedOn = DateTime.Now;

            _db.RptSavedReportColumns.RemoveRange(report.RptSavedReportColumns);
            _db.RptSavedReportFilters.RemoveRange(report.RptSavedReportFilters);
            await _db.SaveChangesAsync();
        }
        else
        {
            report = new RptSavedReport
            {
                ReportCode = dto.ReportCode,
                ReportName = dto.ReportName,
                Description = dto.Description,
                SourceTable = dto.SourceTable,
                IsShared = dto.IsShared,
                IsDefault = false,
                ReportType = dto.ReportType,
                ShowTotals = dto.ShowTotals,
                ShowGrandTotal = dto.ShowGrandTotal,
                JoinedTables = dto.JoinedTables != null ? JsonSerializer.Serialize(dto.JoinedTables, jsonOpts) : null,
                GroupByColumns = dto.GroupByColumns != null ? JsonSerializer.Serialize(dto.GroupByColumns, jsonOpts) : null,
                OrderByColumns = dto.OrderByColumns != null ? JsonSerializer.Serialize(dto.OrderByColumns, jsonOpts) : null,
                PageSize = dto.PageSize,
                ChartType = dto.ChartType,
                ChartConfig = dto.ChartConfig,
                CreatedBy = userName,
                CreatedOn = DateTime.Now,
                IsActive = true
            };
            _db.RptSavedReports.Add(report);
            await _db.SaveChangesAsync();
        }

        // Save columns
        int colOrder = 0;
        foreach (var col in dto.Columns ?? [])
        {
            _db.RptSavedReportColumns.Add(new RptSavedReportColumn
            {
                ReportId = report.ReportId,
                ColumnName = col.ColumnName,
                DisplayName = col.DisplayName,
                ColumnOrder = colOrder++,
                IsVisible = col.IsVisible,
                AggregateFunction = col.AggregateFunction,
                FormatString = col.FormatString,
                ColumnWidth = col.ColumnWidth,
                IsActive = true
            });
        }

        // Save filters
        int filterOrder = 0;
        foreach (var f in dto.Filters ?? [])
        {
            _db.RptSavedReportFilters.Add(new RptSavedReportFilter
            {
                ReportId = report.ReportId,
                ColumnName = f.ColumnName,
                Operator = f.Operator,
                FilterValue = f.FilterValue,
                FilterValue2 = f.FilterValue2,
                FilterOrder = filterOrder++,
                LogicOperator = f.LogicOperator ?? "AND",
                IsActive = true
            });
        }

        await _db.SaveChangesAsync();

        return Ok(new { id = report.ReportId, message = dto.ReportId > 0 ? "Report updated." : "Report saved." });
    }

    [HttpDelete("saved/{id}")]
    public async Task<IActionResult> DeleteReport(long id)
    {
        var report = await _db.RptSavedReports.FindAsync(id);
        if (report == null) return NotFound(new { message = "Report not found." });
        report.IsActive = false;
        report.ModifiedOn = DateTime.Now;
        await _db.SaveChangesAsync();
        return Ok(new { message = "Report deleted." });
    }

    // ══════════════════════════════════════════════════════════════════════
    //  Export (CSV)
    // ══════════════════════════════════════════════════════════════════════

    [HttpPost("export/csv")]
    public async Task<IActionResult> ExportCsv([FromBody] ReportExecuteDto dto)
    {
        if (!await IsValidTable(dto.SourceTable))
            return BadRequest(new { message = "Invalid table name." });

        var validColumns = await GetValidColumns(dto.SourceTable);
        var joinDefs = dto.JoinedTables ?? [];
        var joinColumnMap = new Dictionary<string, List<string>>();
        foreach (var jt in joinDefs)
        {
            if (!await IsValidTable(jt.Table)) continue;
            joinColumnMap[jt.Table] = await GetValidColumns(jt.Table);
        }

        var allValidColumns = new HashSet<string>(validColumns.Select(c => c.ToLowerInvariant()));
        foreach (var (tbl, cols) in joinColumnMap)
            foreach (var c in cols)
                allValidColumns.Add($"{tbl}.{c}".ToLowerInvariant());

        var selectedCols = (dto.Columns ?? []).Where(c =>
            allValidColumns.Contains(c.ToLowerInvariant()) ||
            validColumns.Contains(c.ToLowerInvariant())
        ).ToList();
        if (selectedCols.Count == 0) selectedCols = validColumns.Take(10).ToList();

        string fromClause = BuildFromClause(dto.SourceTable, joinDefs);

        string QualifyCol(string col)
        {
            if (col.Contains('.'))
            {
                var parts = col.Split('.', 2);
                return $"press_db.\"{parts[0]}\".\"{parts[1]}\"";
            }
            return $"press_db.\"{dto.SourceTable}\".\"{col}\"";
        }

        string ColAlias(string col) => col.Contains('.') ? col.Replace('.', '_') : col;

        var sb = new StringBuilder();
        var selectParts = selectedCols.Select(c => $"{QualifyCol(c)} AS \"{ColAlias(c)}\"");
        sb.Append($"SELECT {string.Join(", ", selectParts)} FROM {fromClause}");
        var parameters = new List<NpgsqlParameter>();
        int paramIdx = 0;
        AppendWhereClause(sb, parameters, dto.Filters, validColumns, ref paramIdx, dto.SourceTable, joinColumnMap);
        if (dto.OrderByColumns?.Count > 0)
        {
            var parts = dto.OrderByColumns
                .Where(o => allValidColumns.Contains(o.Column.ToLowerInvariant()) || validColumns.Contains(o.Column.ToLowerInvariant()))
                .Select(o => $"{QualifyCol(o.Column)} {(o.Dir?.ToUpperInvariant() == "DESC" ? "DESC" : "ASC")}");
            if (parts.Any()) sb.Append($" ORDER BY {string.Join(", ", parts)}");
        }
        sb.Append(" LIMIT 10000");

        var user = HttpContext.Session.GetObject<UserSessionData>("CurrentUser");
        var userName = user?.Name ?? "System";

        var csv = new StringBuilder();
        if (dto.IncludeHeader)
        {
            var reportName = string.IsNullOrWhiteSpace(dto.ReportName) ? "Ad-hoc Report" : dto.ReportName.Trim();
            var reportCode = string.IsNullOrWhiteSpace(dto.ReportCode) ? "N/A" : dto.ReportCode.Trim();
            csv.AppendLine($"\"Report Name\",\"{CsvEsc(reportName)}\"");
            csv.AppendLine($"\"Report Code\",\"{CsvEsc(reportCode)}\"");
            csv.AppendLine($"\"Generated By\",\"{CsvEsc(userName)}\"");
            csv.AppendLine($"\"Generated On\",\"{DateTime.Now:dd-MMM-yyyy HH:mm:ss}\"");
            csv.AppendLine();
        }

        if (dto.IncludeFilters)
        {
            csv.AppendLine("\"Applied Filters\"");
            var filterDescriptions = dto.FilterDescriptions ?? [];
            if (filterDescriptions.Count == 0)
            {
                csv.AppendLine("\"No filters applied\"");
            }
            else
            {
                foreach (var f in filterDescriptions)
                    csv.AppendLine($"\"{CsvEsc(f)}\"");
            }
            csv.AppendLine();
        }

        csv.AppendLine(string.Join(",", selectedCols.Select(c => $"\"{HumanizeName(ColAlias(c))}\"")));

        var exportedRows = 0;
        var conn = _db.Database.GetDbConnection();
        await conn.OpenAsync();
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sb.ToString();
            foreach (var p in parameters) cmd.Parameters.Add(p);
            using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                var vals = new List<string>();
                for (int i = 0; i < rdr.FieldCount; i++)
                {
                    var val = rdr.IsDBNull(i) ? "" : rdr.GetValue(i)?.ToString() ?? "";
                    vals.Add($"\"{val.Replace("\"", "\"\"")}\"");
                }
                csv.AppendLine(string.Join(",", vals));
                exportedRows++;
            }
        }
        finally { if (conn.State == System.Data.ConnectionState.Open) await conn.CloseAsync(); }

        if (dto.IncludeFooter)
        {
            csv.AppendLine();
            csv.AppendLine($"\"Row Count\",\"{exportedRows}\"");
            csv.AppendLine($"\"Exported By\",\"{CsvEsc(userName)}\"");
        }

        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"report_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
    }

    // ══════════════════════════════════════════════════════════════════════
    //  Private Helpers
    // ══════════════════════════════════════════════════════════════════════

    private async Task<bool> IsValidTable(string tableName)
    {
        var conn = _db.Database.GetDbConnection();
        var wasOpen = conn.State == System.Data.ConnectionState.Open;
        if (!wasOpen) await conn.OpenAsync();
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema='press_db' AND table_name=@t";
            var p = cmd.CreateParameter(); p.ParameterName = "@t"; p.Value = tableName;
            cmd.Parameters.Add(p);
            return Convert.ToInt64(await cmd.ExecuteScalarAsync()) > 0;
        }
        finally { if (!wasOpen && conn.State == System.Data.ConnectionState.Open) await conn.CloseAsync(); }
    }

    private static string CsvEsc(string? value) => (value ?? string.Empty).Replace("\"", "\"\"");

    private async Task<List<string>> GetValidColumns(string tableName)
    {
        var cols = new List<string>();
        var conn = _db.Database.GetDbConnection();
        var wasOpen = conn.State == System.Data.ConnectionState.Open;
        if (!wasOpen) await conn.OpenAsync();
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT column_name FROM information_schema.columns WHERE table_schema='press_db' AND table_name=@t ORDER BY ordinal_position";
            var p = cmd.CreateParameter(); p.ParameterName = "@t"; p.Value = tableName;
            cmd.Parameters.Add(p);
            using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync()) cols.Add(rdr.GetString(0));
        }
        finally { if (!wasOpen && conn.State == System.Data.ConnectionState.Open) await conn.CloseAsync(); }
        return cols;
    }

    private static void AppendWhereClause(StringBuilder sb, List<NpgsqlParameter> parameters,
        List<ReportFilterDto>? filters, List<string> validColumns, ref int paramIdx)
    {
        if (filters == null || filters.Count == 0) return;

        var clauses = new List<string>();
        foreach (var f in filters)
        {
            if (!validColumns.Contains(f.ColumnName.ToLowerInvariant())) continue;

            var pName = $"@p{paramIdx++}";
            var col = $"\"{f.ColumnName}\"";

            switch (f.Operator.ToLowerInvariant())
            {
                case "eq":
                    clauses.Add($"{col} = {pName}");
                    parameters.Add(new NpgsqlParameter(pName, CoerceFilterValue(f.FilterValue)));
                    break;
                case "neq":
                    clauses.Add($"{col} != {pName}");
                    parameters.Add(new NpgsqlParameter(pName, CoerceFilterValue(f.FilterValue)));
                    break;
                case "gt":
                    clauses.Add($"{col} > {pName}");
                    parameters.Add(new NpgsqlParameter(pName, CoerceFilterValue(f.FilterValue)));
                    break;
                case "gte":
                    clauses.Add($"{col} >= {pName}");
                    parameters.Add(new NpgsqlParameter(pName, CoerceFilterValue(f.FilterValue)));
                    break;
                case "lt":
                    clauses.Add($"{col} < {pName}");
                    parameters.Add(new NpgsqlParameter(pName, CoerceFilterValue(f.FilterValue)));
                    break;
                case "lte":
                    clauses.Add($"{col} <= {pName}");
                    parameters.Add(new NpgsqlParameter(pName, CoerceFilterValue(f.FilterValue)));
                    break;
                case "contains":
                    clauses.Add($"{col}::text ILIKE {pName}");
                    parameters.Add(new NpgsqlParameter(pName, $"%{f.FilterValue}%"));
                    break;
                case "startswith":
                    clauses.Add($"{col}::text ILIKE {pName}");
                    parameters.Add(new NpgsqlParameter(pName, $"{f.FilterValue}%"));
                    break;
                case "endswith":
                    clauses.Add($"{col}::text ILIKE {pName}");
                    parameters.Add(new NpgsqlParameter(pName, $"%{f.FilterValue}"));
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
                    parameters.Add(new NpgsqlParameter(pName, CoerceFilterValue(f.FilterValue)));
                    parameters.Add(new NpgsqlParameter(pName2, CoerceFilterValue(f.FilterValue2)));
                    break;
                default:
                    clauses.Add($"{col} = {pName}");
                    parameters.Add(new NpgsqlParameter(pName, CoerceFilterValue(f.FilterValue)));
                    break;
            }
        }

        if (clauses.Count > 0)
        {
            // Join with AND/OR respecting each filter's logic operator
            var where = clauses[0];
            for (int i = 1; i < clauses.Count; i++)
            {
                var logic = (filters != null && i < filters.Count && filters[i].LogicOperator?.ToUpperInvariant() == "OR") ? "OR" : "AND";
                where = $"{where} {logic} {clauses[i]}";
            }
            sb.Append($" WHERE {where}");
        }
    }

    private async Task<bool> IsNumericColumnFromDbAsync(string table, string column, System.Data.Common.DbConnection conn)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT data_type FROM information_schema.columns WHERE table_schema='press_db' AND table_name=@t AND column_name=@c";
        var p1 = cmd.CreateParameter(); p1.ParameterName = "@t"; p1.Value = table; cmd.Parameters.Add(p1);
        var p2 = cmd.CreateParameter(); p2.ParameterName = "@c"; p2.Value = column; cmd.Parameters.Add(p2);
        var dt = (await cmd.ExecuteScalarAsync())?.ToString() ?? "";
        return IsNumericType(dt);
    }

    private async Task<bool> IsDateColumnFromDbAsync(string table, string column, System.Data.Common.DbConnection conn)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT data_type FROM information_schema.columns WHERE table_schema='press_db' AND table_name=@t AND column_name=@c";
        var p1 = cmd.CreateParameter(); p1.ParameterName = "@t"; p1.Value = table; cmd.Parameters.Add(p1);
        var p2 = cmd.CreateParameter(); p2.ParameterName = "@c"; p2.Value = column; cmd.Parameters.Add(p2);
        var dt = (await cmd.ExecuteScalarAsync())?.ToString() ?? "";
        return IsDateType(dt);
    }

    private static object CoerceFilterValue(string? value)
    {
        if (value == null) return DBNull.Value;
        if (long.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var l)) return l;
        if (decimal.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var d)) return d;
        if (DateTime.TryParse(value, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var dt)) return dt;
        if (bool.TryParse(value, out var b)) return b;
        return value;
    }

    private static bool IsNumericType(string dataType) =>
        dataType is "integer" or "bigint" or "smallint" or "numeric" or "real" or "double precision" or "decimal" or "money";

    private static bool IsDateType(string dataType) =>
        dataType.Contains("timestamp") || dataType == "date" || dataType.Contains("time");

    private static string CategorizeTable(string name)
    {
        if (name.StartsWith("mst_")) return "Masters";
        if (name.StartsWith("trn_")) return "Transactions";
        if (name.StartsWith("hr_") || name.StartsWith("hyb_employee")) return "HR & Payroll";
        if (name.StartsWith("rpt_")) return "Reports";
        if (name.StartsWith("vw_")) return "Views";
        if (name.StartsWith("sys_") || name.StartsWith("error_")) return "System";
        if (name.StartsWith("txn_")) return "Activities";
        if (name.StartsWith("hyb_")) return "Hybrid";
        return "Other";
    }

    private static string HumanizeName(string name)
    {
        // Remove common prefixes
        var n = name;
        foreach (var pfx in new[] { "mst_", "trn_", "hr_", "hyb_", "rpt_", "vw_", "sys_", "txn_" })
            if (n.StartsWith(pfx)) { n = n[pfx.Length..]; break; }

        return string.Join(" ", n.Split('_').Select(w => w.Length > 0 ? char.ToUpper(w[0]) + w[1..] : w));
    }

    private static string BuildFromClause(string sourceTable, List<JoinedTableDto> joinDefs)
    {
        var sb = new StringBuilder($"press_db.\"{sourceTable}\"");
        foreach (var jt in joinDefs)
        {
            var joinType = jt.JoinType?.ToUpperInvariant() switch
            {
                "INNER" => "INNER JOIN",
                "RIGHT" => "RIGHT JOIN",
                "FULL" => "FULL JOIN",
                _ => "LEFT JOIN"
            };
            sb.Append($" {joinType} press_db.\"{jt.Table}\" ON press_db.\"{sourceTable}\".\"{jt.FkColumn}\" = press_db.\"{jt.Table}\".\"{jt.PkColumn}\"");
        }
        return sb.ToString();
    }

    private static string SafeAggFunction(string fn) =>
        fn?.ToUpperInvariant() switch
        {
            "SUM" => "SUM",
            "AVG" => "AVG",
            "MIN" => "MIN",
            "MAX" => "MAX",
            _ => "COUNT"
        };

    private static void AppendWhereClause(StringBuilder sb, List<NpgsqlParameter> parameters,
        List<ReportFilterDto>? filters, List<string> validColumns, ref int paramIdx,
        string sourceTable, Dictionary<string, List<string>> joinColumnMap)
    {
        if (filters == null || filters.Count == 0) return;

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
                col = $"press_db.\"{parts[0]}\".\"{parts[1]}\"";
            }
            else
            {
                if (!validColumns.Contains(colLower)) continue;
                col = $"press_db.\"{sourceTable}\".\"{f.ColumnName}\"";
            }

            var pName = $"@p{paramIdx++}";

            switch (f.Operator.ToLowerInvariant())
            {
                case "eq":
                    clauses.Add($"{col} = {pName}");
                    parameters.Add(new NpgsqlParameter(pName, CoerceFilterValue(f.FilterValue)));
                    break;
                case "neq":
                    clauses.Add($"{col} != {pName}");
                    parameters.Add(new NpgsqlParameter(pName, CoerceFilterValue(f.FilterValue)));
                    break;
                case "gt":
                    clauses.Add($"{col} > {pName}");
                    parameters.Add(new NpgsqlParameter(pName, CoerceFilterValue(f.FilterValue)));
                    break;
                case "gte":
                    clauses.Add($"{col} >= {pName}");
                    parameters.Add(new NpgsqlParameter(pName, CoerceFilterValue(f.FilterValue)));
                    break;
                case "lt":
                    clauses.Add($"{col} < {pName}");
                    parameters.Add(new NpgsqlParameter(pName, CoerceFilterValue(f.FilterValue)));
                    break;
                case "lte":
                    clauses.Add($"{col} <= {pName}");
                    parameters.Add(new NpgsqlParameter(pName, CoerceFilterValue(f.FilterValue)));
                    break;
                case "contains":
                    clauses.Add($"{col}::text ILIKE {pName}");
                    parameters.Add(new NpgsqlParameter(pName, $"%{f.FilterValue}%"));
                    break;
                case "startswith":
                    clauses.Add($"{col}::text ILIKE {pName}");
                    parameters.Add(new NpgsqlParameter(pName, $"{f.FilterValue}%"));
                    break;
                case "endswith":
                    clauses.Add($"{col}::text ILIKE {pName}");
                    parameters.Add(new NpgsqlParameter(pName, $"%{f.FilterValue}"));
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
                    parameters.Add(new NpgsqlParameter(pName, CoerceFilterValue(f.FilterValue)));
                    parameters.Add(new NpgsqlParameter(pName2, CoerceFilterValue(f.FilterValue2)));
                    break;
                default:
                    clauses.Add($"{col} = {pName}");
                    parameters.Add(new NpgsqlParameter(pName, CoerceFilterValue(f.FilterValue)));
                    break;
            }
        }

        if (clauses.Count > 0)
        {
            var where = clauses[0];
            for (int i = 1; i < clauses.Count; i++)
            {
                var logic = (filters != null && i < filters.Count && filters[i].LogicOperator?.ToUpperInvariant() == "OR") ? "OR" : "AND";
                where = $"{where} {logic} {clauses[i]}";
            }
            sb.Append($" WHERE {where}");
        }
    }

    private async Task<Dictionary<string, object?>?> ComputeTotals(
        System.Data.Common.DbConnection conn, string fromClause, ReportExecuteDto dto,
        List<string> validColumns, Dictionary<string, List<string>> joinColumnMap,
        List<string> selectedCols, Func<string, string> qualifyCol, Func<string, string> colAlias)
    {
        var numericCols = new List<string>();
        foreach (var col in selectedCols)
        {
            if (col.Contains('.'))
            {
                var parts = col.Split('.', 2);
                if (joinColumnMap.TryGetValue(parts[0], out _))
                {
                    if (await IsNumericColumnFromDbAsync(parts[0], parts[1], conn))
                        numericCols.Add(col);
                }
            }
            else
            {
                if (await IsNumericColumnFromDbAsync(dto.SourceTable, col, conn))
                    numericCols.Add(col);
            }
        }

        if (numericCols.Count == 0) return null;

        var selectParts = numericCols.Select(c =>
            $"COALESCE(SUM({qualifyCol(c)})::numeric, 0) AS \"{colAlias(c)}_sum\", COALESCE(AVG({qualifyCol(c)})::numeric, 0) AS \"{colAlias(c)}_avg\"");

        var totalsSql = new StringBuilder();
        totalsSql.Append($"SELECT {string.Join(", ", selectParts)} FROM {fromClause}");
        var totalsParams = new List<NpgsqlParameter>();
        int tIdx = 0;
        AppendWhereClause(totalsSql, totalsParams, dto.Filters, validColumns, ref tIdx, dto.SourceTable, joinColumnMap);

        var totalsRow = new Dictionary<string, object?>();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = totalsSql.ToString();
        foreach (var p in totalsParams)
            cmd.Parameters.Add(new NpgsqlParameter(p.ParameterName, p.Value));

        using var rdr = await cmd.ExecuteReaderAsync();
        if (await rdr.ReadAsync())
        {
            for (int i = 0; i < rdr.FieldCount; i++)
                totalsRow[rdr.GetName(i)] = rdr.IsDBNull(i) ? null : rdr.GetValue(i);
        }

        return totalsRow;
    }
}

// ── DTOs ──
public class ReportExecuteDto
{
    public string SourceTable { get; set; } = "";
    public List<string>? Columns { get; set; }
    public List<ReportFilterDto>? Filters { get; set; }
    public List<ReportOrderDto>? OrderByColumns { get; set; }
    public List<string>? GroupByColumns { get; set; }
    public List<ReportAggregateDto>? Aggregates { get; set; }
    public List<JoinedTableDto>? JoinedTables { get; set; }
    public string? ReportType { get; set; }
    public bool ShowTotals { get; set; }
    public bool ShowGrandTotal { get; set; }
    public string? ReportName { get; set; }
    public string? ReportCode { get; set; }
    public bool IncludeHeader { get; set; }
    public bool IncludeFilters { get; set; }
    public bool IncludeFooter { get; set; }
    public List<string>? FilterDescriptions { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public class JoinedTableDto
{
    public string Table { get; set; } = "";
    public string JoinType { get; set; } = "LEFT";
    public string FkColumn { get; set; } = "";
    public string PkColumn { get; set; } = "";
}

public class ReportFilterDto
{
    public string ColumnName { get; set; } = "";
    public string Operator { get; set; } = "eq";
    public string? FilterValue { get; set; }
    public string? FilterValue2 { get; set; }
    public string? LogicOperator { get; set; } = "AND";
}

public class ReportOrderDto
{
    public string Column { get; set; } = "";
    public string? Dir { get; set; } = "ASC";
}

public class ReportAggregateDto
{
    public string Column { get; set; } = "";
    public string Function { get; set; } = "COUNT";
}

public class ReportSaveDto
{
    public long ReportId { get; set; }
    public string ReportCode { get; set; } = "";
    public string ReportName { get; set; } = "";
    public string? Description { get; set; }
    public string SourceTable { get; set; } = "";
    public bool IsShared { get; set; }
    public int PageSize { get; set; } = 25;
    public string? ReportType { get; set; }
    public bool ShowTotals { get; set; }
    public bool ShowGrandTotal { get; set; }
    public List<string>? GroupByColumns { get; set; }
    public List<ReportOrderDto>? OrderByColumns { get; set; }
    public List<JoinedTableDto>? JoinedTables { get; set; }
    public string? ChartType { get; set; }
    public string? ChartConfig { get; set; }
    public List<ReportSaveColumnDto>? Columns { get; set; }
    public List<ReportSaveFilterDto>? Filters { get; set; }
}

public class ReportSaveColumnDto
{
    public string ColumnName { get; set; } = "";
    public string? DisplayName { get; set; }
    public bool IsVisible { get; set; } = true;
    public string? AggregateFunction { get; set; }
    public string? FormatString { get; set; }
    public int? ColumnWidth { get; set; }
}

public class ReportSaveFilterDto
{
    public string ColumnName { get; set; } = "";
    public string Operator { get; set; } = "eq";
    public string? FilterValue { get; set; }
    public string? FilterValue2 { get; set; }
    public string? LogicOperator { get; set; } = "AND";
}
