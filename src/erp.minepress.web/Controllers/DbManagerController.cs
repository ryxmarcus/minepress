using erp.minepress.infrastructure.ErrorLogging;
using erp.minepress.persistence.Context;
using erp.minepress.web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Text;
using System.Text.Json;

namespace erp.minepress.web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DbManagerController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<DbManagerController> _logger;
    private readonly ISystemErrorLogger _systemErrorLogger;
    private const string Schema = "press_db";

    // ── Protected Tables: full read-only (no insert/update/delete) ──
    private static readonly HashSet<string> ProtectedTables = new(StringComparer.OrdinalIgnoreCase)
    {
        "__efmigrationshistory", "__EFMigrationsHistory",
        "sys_users", "sys_roles", "sys_user_roles", "sys_permissions",
        "sys_audit_log", "sys_config", "sys_settings"
    };

    // ── Master Tables: dynamically matched by prefix — delete blocked, code/id columns immutable ──
    private static bool IsMasterTable(string tableName) =>
        tableName.StartsWith("mst_", StringComparison.OrdinalIgnoreCase);

    // ── Immutable columns on master tables: id and code columns cannot be updated ──
    private static List<string> GetImmutableColumns(List<ColumnDetail> columns, string tableName)
    {
        if (!IsMasterTable(tableName)) return new();
        return columns
            .Where(c => c.Name.Equals("id", StringComparison.OrdinalIgnoreCase)
                     || c.Name.EndsWith("_id", StringComparison.OrdinalIgnoreCase)
                     || c.Name.Equals("code", StringComparison.OrdinalIgnoreCase)
                     || c.Name.EndsWith("_code", StringComparison.OrdinalIgnoreCase)
                     || c.IsPk)
            .Select(c => c.Name)
            .ToList();
    }

    public DbManagerController(ApplicationDbContext db, ILogger<DbManagerController> logger, ISystemErrorLogger systemErrorLogger)
    {
        _db = db;
        _logger = logger;
        _systemErrorLogger = systemErrorLogger;
    }

    private async Task AuditExceptionAsync(Exception ex, string additionalData, string severity = "Error")
        => await _systemErrorLogger.LogAsync(ex, HttpContext, severity, additionalData);

    // ── Admin Guard ──
    private bool IsAdmin()
    {
        var user = HttpContext.Session.GetObject<UserSessionData>("CurrentUser");
        return user?.IsSystemAdmin == true;
    }

    private UserSessionData? GetCurrentUser()
    {
        return HttpContext.Session.GetObject<UserSessionData>("CurrentUser");
    }

    private bool IsProtected(string tableName) => ProtectedTables.Contains(tableName);
    private bool IsDeleteRestricted(string tableName) => IsMasterTable(tableName);

    private void AuditLog(string operation, string tableName, object? detail = null)
    {
        var user = GetCurrentUser();
        var userName = user?.UserName ?? "unknown";
        var userId = user?.UserId ?? 0;
        var detailJson = detail != null ? System.Text.Json.JsonSerializer.Serialize(detail) : "";
        _logger.LogWarning(
            "[DBManager AUDIT] {Operation} on {Schema}.{Table} by {User} (ID:{UserId}) | {Detail}",
            operation, Schema, tableName, userName, userId, detailJson);
    }

    // ══════════════════════════════════════════════════════════════════════
    //  Schema Discovery
    // ══════════════════════════════════════════════════════════════════════

    [HttpGet("tables")]
    public async Task<IActionResult> GetTables()
    {
        if (!IsAdmin()) return StatusCode(403, new { message = "Access denied." });

        var sql = @"
            SELECT t.table_name, t.table_type,
                   (SELECT COUNT(*) FROM information_schema.columns c
                    WHERE c.table_schema = t.table_schema AND c.table_name = t.table_name) AS col_count,
                   COALESCE(pgd.description, '') AS table_comment
            FROM information_schema.tables t
            LEFT JOIN pg_catalog.pg_namespace pn ON pn.nspname = @schema
            LEFT JOIN pg_catalog.pg_class pc ON pc.relname = t.table_name AND pc.relnamespace = pn.oid
            LEFT JOIN pg_catalog.pg_description pgd ON pgd.objoid = pc.oid AND pgd.objsubid = 0
            WHERE t.table_schema = @schema
              AND t.table_type IN ('BASE TABLE','VIEW')
            ORDER BY t.table_type, t.table_name";

        var tables = new List<object>();
        var conn = _db.Database.GetDbConnection();
        await conn.OpenAsync();
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.Add(new NpgsqlParameter("@schema", Schema));
            using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                var name = rdr.GetString(0);
                tables.Add(new
                {
                    name,
                    friendlyName = HumanizeName(name),
                    type = rdr.GetString(1) == "VIEW" ? "view" : "table",
                    category = CategorizeTable(name),
                    columnCount = rdr.GetInt64(2),
                    comment = rdr.IsDBNull(3) ? "" : rdr.GetString(3)
                });
            }
        }
        finally { if (conn.State == System.Data.ConnectionState.Open) await conn.CloseAsync(); }

        return Ok(tables);
    }

    [HttpGet("tables/{tableName}/columns")]
    public async Task<IActionResult> GetColumns(string tableName)
    {
        if (!IsAdmin()) return StatusCode(403, new { message = "Access denied." });
        if (!await IsValidTable(tableName)) return BadRequest(new { message = "Invalid table." });

        var columns = await GetColumnDetails(tableName);
        return Ok(columns);
    }

    [HttpGet("tables/{tableName}/pk")]
    public async Task<IActionResult> GetPrimaryKey(string tableName)
    {
        if (!IsAdmin()) return StatusCode(403, new { message = "Access denied." });
        if (!await IsValidTable(tableName)) return BadRequest(new { message = "Invalid table." });

        var pkCols = await GetPrimaryKeyColumns(tableName);
        return Ok(pkCols);
    }

    [HttpGet("tables/{tableName}/fk")]
    public async Task<IActionResult> GetForeignKeys(string tableName)
    {
        if (!IsAdmin()) return StatusCode(403, new { message = "Access denied." });
        if (!await IsValidTable(tableName)) return BadRequest(new { message = "Invalid table." });

        var sql = @"
            SELECT kcu.column_name AS fk_column,
                   ccu.table_name AS ref_table,
                   ccu.column_name AS ref_column,
                   tc.constraint_name
            FROM information_schema.table_constraints tc
            JOIN information_schema.key_column_usage kcu
                 ON tc.constraint_name = kcu.constraint_name AND tc.table_schema = kcu.table_schema
            JOIN information_schema.constraint_column_usage ccu
                 ON ccu.constraint_name = tc.constraint_name AND ccu.table_schema = tc.table_schema
            WHERE tc.constraint_type = 'FOREIGN KEY'
              AND tc.table_schema = @schema AND tc.table_name = @tbl
            ORDER BY kcu.column_name";

        var fks = new List<object>();
        var conn = _db.Database.GetDbConnection();
        await conn.OpenAsync();
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.Add(new NpgsqlParameter("@schema", Schema));
            cmd.Parameters.Add(new NpgsqlParameter("@tbl", tableName));
            using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                fks.Add(new
                {
                    fkColumn = rdr.GetString(0),
                    refTable = rdr.GetString(1),
                    refColumn = rdr.GetString(2),
                    constraintName = rdr.GetString(3)
                });
            }
        }
        finally { if (conn.State == System.Data.ConnectionState.Open) await conn.CloseAsync(); }

        return Ok(fks);
    }

    [HttpGet("tables/{tableName}/references")]
    public async Task<IActionResult> GetIncomingReferences(string tableName)
    {
        if (!IsAdmin()) return StatusCode(403, new { message = "Access denied." });
        if (!await IsValidTable(tableName)) return BadRequest(new { message = "Invalid table." });

        var sql = @"
            SELECT kcu.table_name AS referencing_table,
                   kcu.column_name AS referencing_column,
                   ccu.column_name AS pk_column,
                   tc.constraint_name
            FROM information_schema.table_constraints tc
            JOIN information_schema.key_column_usage kcu
                 ON tc.constraint_name = kcu.constraint_name AND tc.table_schema = kcu.table_schema
            JOIN information_schema.constraint_column_usage ccu
                 ON ccu.constraint_name = tc.constraint_name AND ccu.table_schema = tc.table_schema
            WHERE tc.constraint_type = 'FOREIGN KEY'
              AND tc.table_schema = @schema AND ccu.table_name = @tbl
            ORDER BY kcu.table_name";

        var refs = new List<object>();
        var conn = _db.Database.GetDbConnection();
        await conn.OpenAsync();
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.Add(new NpgsqlParameter("@schema", Schema));
            cmd.Parameters.Add(new NpgsqlParameter("@tbl", tableName));
            using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                refs.Add(new
                {
                    referencingTable = rdr.GetString(0),
                    referencingColumn = rdr.GetString(1),
                    pkColumn = rdr.GetString(2),
                    constraintName = rdr.GetString(3)
                });
            }
        }
        finally { if (conn.State == System.Data.ConnectionState.Open) await conn.CloseAsync(); }

        return Ok(refs);
    }

    [HttpGet("tables/{tableName}/fk-lookup/{columnName}")]
    public async Task<IActionResult> GetFkLookup(string tableName, string columnName)
    {
        if (!IsAdmin()) return StatusCode(403, new { message = "Access denied." });
        if (!await IsValidTable(tableName)) return BadRequest(new { message = "Invalid table." });

        // Find the FK definition
        var conn = _db.Database.GetDbConnection();
        await conn.OpenAsync();
        try
        {
            string? refTable = null, refColumn = null;
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT ccu.table_name, ccu.column_name
                    FROM information_schema.table_constraints tc
                    JOIN information_schema.key_column_usage kcu
                         ON tc.constraint_name = kcu.constraint_name AND tc.table_schema = kcu.table_schema
                    JOIN information_schema.constraint_column_usage ccu
                         ON ccu.constraint_name = tc.constraint_name AND ccu.table_schema = tc.table_schema
                    WHERE tc.constraint_type = 'FOREIGN KEY'
                      AND tc.table_schema = @schema AND tc.table_name = @tbl AND kcu.column_name = @col
                    LIMIT 1";
                cmd.Parameters.Add(new NpgsqlParameter("@schema", Schema));
                cmd.Parameters.Add(new NpgsqlParameter("@tbl", tableName));
                cmd.Parameters.Add(new NpgsqlParameter("@col", columnName));
                using var rdr = await cmd.ExecuteReaderAsync();
                if (await rdr.ReadAsync())
                {
                    refTable = rdr.GetString(0);
                    refColumn = rdr.GetString(1);
                }
            }

            if (refTable == null) return Ok(Array.Empty<object>());

            // Get a display column (first text column after PK)
            string? displayCol = null;
            using (var cmd2 = conn.CreateCommand())
            {
                cmd2.CommandText = @"
                    SELECT column_name FROM information_schema.columns
                    WHERE table_schema = @schema AND table_name = @tbl
                      AND data_type IN ('character varying','text','character')
                      AND column_name != @pk
                    ORDER BY ordinal_position LIMIT 1";
                cmd2.Parameters.Add(new NpgsqlParameter("@schema", Schema));
                cmd2.Parameters.Add(new NpgsqlParameter("@tbl", refTable));
                cmd2.Parameters.Add(new NpgsqlParameter("@pk", refColumn));
                displayCol = (await cmd2.ExecuteScalarAsync())?.ToString();
            }

            var selectExpr = displayCol != null
                ? $"\"{refColumn}\" AS id, \"{displayCol}\" AS label"
                : $"\"{refColumn}\" AS id, \"{refColumn}\"::text AS label";

            using (var cmd3 = conn.CreateCommand())
            {
                cmd3.CommandText = $"SELECT {selectExpr} FROM {Schema}.\"{refTable}\" ORDER BY 2 LIMIT 500";
                using var rdr3 = await cmd3.ExecuteReaderAsync();
                var items = new List<object>();
                while (await rdr3.ReadAsync())
                {
                    items.Add(new
                    {
                        id = rdr3.IsDBNull(0) ? null : rdr3.GetValue(0),
                        label = rdr3.IsDBNull(1) ? "(null)" : rdr3.GetValue(1)?.ToString()
                    });
                }
                return Ok(new { refTable, refColumn, displayCol, items });
            }
        }
        finally { if (conn.State == System.Data.ConnectionState.Open) await conn.CloseAsync(); }
    }

    [HttpGet("tables/{tableName}/row-count")]
    public async Task<IActionResult> GetRowCount(string tableName)
    {
        if (!IsAdmin()) return StatusCode(403, new { message = "Access denied." });
        if (!await IsValidTable(tableName)) return BadRequest(new { message = "Invalid table." });

        var conn = _db.Database.GetDbConnection();
        await conn.OpenAsync();
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT COUNT(*) FROM {Schema}.\"{tableName}\"";
            var count = Convert.ToInt64(await cmd.ExecuteScalarAsync());
            return Ok(new { count });
        }
        finally { if (conn.State == System.Data.ConnectionState.Open) await conn.CloseAsync(); }
    }

    // ══════════════════════════════════════════════════════════════════════
    //  Read Rows (paginated, searchable, sortable)
    // ══════════════════════════════════════════════════════════════════════

    [HttpPost("tables/{tableName}/rows")]
    public async Task<IActionResult> GetRows(string tableName, [FromBody] DbRowsRequestDto dto)
    {
        if (!IsAdmin()) return StatusCode(403, new { message = "Access denied." });
        if (!await IsValidTable(tableName)) return BadRequest(new { message = "Invalid table." });

        var validCols = await GetValidColumnNames(tableName);
        var columns = await GetColumnDetails(tableName);

        var sb = new StringBuilder();
        var countSb = new StringBuilder();
        var parameters = new List<NpgsqlParameter>();
        int paramIdx = 0;

        // SELECT
        sb.Append($"SELECT * FROM {Schema}.\"{tableName}\"");
        countSb.Append($"SELECT COUNT(*) FROM {Schema}.\"{tableName}\"");

        // WHERE (search)
        if (!string.IsNullOrWhiteSpace(dto.Search))
        {
            var textCols = columns
                .Where(c => c.DataType is "character varying" or "text" or "character" or "name" or "uuid")
                .Select(c => c.Name)
                .Take(10)
                .ToList();

            if (textCols.Count > 0)
            {
                var searchParam = $"@search{paramIdx++}";
                var clauses = textCols.Select(c => $"\"{c}\"::text ILIKE {searchParam}");
                var whereClause = $" WHERE ({string.Join(" OR ", clauses)})";
                sb.Append(whereClause);
                countSb.Append(whereClause);
                parameters.Add(new NpgsqlParameter(searchParam, $"%{dto.Search}%"));
            }
        }

        // ORDER BY
        if (!string.IsNullOrWhiteSpace(dto.SortColumn) && validCols.Contains(dto.SortColumn))
        {
            var dir = dto.SortDir?.ToUpperInvariant() == "DESC" ? "DESC" : "ASC";
            sb.Append($" ORDER BY \"{dto.SortColumn}\" {dir} NULLS LAST");
        }
        else
        {
            // Default: order by first column
            if (validCols.Count > 0)
                sb.Append($" ORDER BY \"{validCols[0]}\"");
        }

        // Pagination
        int page = Math.Max(1, dto.Page);
        int pageSize = Math.Clamp(dto.PageSize, 1, 200);
        sb.Append($" LIMIT {pageSize} OFFSET {(page - 1) * pageSize}");

        var conn = _db.Database.GetDbConnection();
        await conn.OpenAsync();
        try
        {
            // Count
            long totalCount = 0;
            using (var countCmd = conn.CreateCommand())
            {
                countCmd.CommandText = countSb.ToString();
                foreach (var p in parameters)
                    countCmd.Parameters.Add(new NpgsqlParameter(p.ParameterName, p.Value));
                totalCount = Convert.ToInt64(await countCmd.ExecuteScalarAsync());
            }

            // Data
            var rows = new List<Dictionary<string, object?>>();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sb.ToString();
                foreach (var p in parameters)
                    cmd.Parameters.Add(new NpgsqlParameter(p.ParameterName, p.Value));

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

            return Ok(new
            {
                data = rows,
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }
        finally { if (conn.State == System.Data.ConnectionState.Open) await conn.CloseAsync(); }
    }

    // ══════════════════════════════════════════════════════════════════════
    //  INSERT Row
    // ══════════════════════════════════════════════════════════════════════

    [HttpPost("tables/{tableName}/insert")]
    public async Task<IActionResult> InsertRow(string tableName, [FromBody] Dictionary<string, object?> rowData)
    {
        if (!IsAdmin()) return StatusCode(403, new { message = "Access denied." });
        if (!await IsValidTable(tableName)) return BadRequest(new { message = "Invalid table." });
        if (await IsView(tableName)) return BadRequest(new { message = "Cannot insert into a view." });
        if (IsProtected(tableName))
            return BadRequest(new { message = $"Table '{tableName}' is protected. Insert operations are not allowed." });

        var validCols = await GetValidColumnNames(tableName);
        var columns = await GetColumnDetails(tableName);
        var filtered = new Dictionary<string, object?>();

        foreach (var kv in rowData)
        {
            if (!validCols.Contains(kv.Key)) continue;
            var colInfo = columns.FirstOrDefault(c => c.Name == kv.Key);
            if (colInfo != null && colInfo.HasDefault && (kv.Value == null || kv.Value.ToString() == ""))
                continue; // let DB handle default
            filtered[kv.Key] = kv.Value;
        }

        if (filtered.Count == 0) return BadRequest(new { message = "No valid columns provided." });

        var colList = string.Join(", ", filtered.Keys.Select(c => $"\"{c}\""));
        var paramList = string.Join(", ", filtered.Keys.Select((_, i) => $"@p{i}"));

        var conn = _db.Database.GetDbConnection();
        await conn.OpenAsync();
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"INSERT INTO {Schema}.\"{tableName}\" ({colList}) VALUES ({paramList})";

            int idx = 0;
            foreach (var kv in filtered)
            {
                var colInfo = columns.FirstOrDefault(c => c.Name == kv.Key);
                var val = CoerceValue(kv.Value, colInfo?.DataType);
                cmd.Parameters.Add(new NpgsqlParameter($"@p{idx++}", val ?? DBNull.Value));
            }

            await cmd.ExecuteNonQueryAsync();
            AuditLog("INSERT", tableName, new { columns = filtered.Keys.ToList() });
            return Ok(new { message = "Row inserted successfully." });
        }
        catch (PostgresException ex) when (ex.SqlState == "23503")
        {
            return BadRequest(new { message = $"Foreign key violation: {ex.MessageText}" });
        }
        catch (PostgresException ex) when (ex.SqlState == "23505")
        {
            return BadRequest(new { message = $"Duplicate key: {ex.MessageText}" });
        }
        catch (PostgresException ex) when (ex.SqlState == "23502")
        {
            return BadRequest(new { message = $"NOT NULL violation: {ex.MessageText}" });
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "Insert failed for {Table}", tableName);
            return BadRequest(new { message = $"Database error: {ex.MessageText}" });
        }
        finally { if (conn.State == System.Data.ConnectionState.Open) await conn.CloseAsync(); }
    }

    // ══════════════════════════════════════════════════════════════════════
    //  UPDATE Row
    // ══════════════════════════════════════════════════════════════════════

    [HttpPut("tables/{tableName}/update")]
    public async Task<IActionResult> UpdateRow(string tableName, [FromBody] DbUpdateDto dto)
    {
        if (!IsAdmin()) return StatusCode(403, new { message = "Access denied." });
        if (!await IsValidTable(tableName)) return BadRequest(new { message = "Invalid table." });
        if (await IsView(tableName)) return BadRequest(new { message = "Cannot update a view." });
        if (IsProtected(tableName))
            return BadRequest(new { message = $"Table '{tableName}' is protected. Update operations are not allowed." });

        var pkCols = await GetPrimaryKeyColumns(tableName);
        if (pkCols.Count == 0) return BadRequest(new { message = "Table has no primary key — cannot update." });

        var validCols = await GetValidColumnNames(tableName);
        var columns = await GetColumnDetails(tableName);

        // Validate PK values provided
        foreach (var pk in pkCols)
        {
            if (!dto.PkValues.ContainsKey(pk))
                return BadRequest(new { message = $"Missing primary key value for '{pk}'." });
            var pkVal = dto.PkValues[pk];
            if (pkVal == null || (pkVal is JsonElement je && je.ValueKind == JsonValueKind.Null) || string.IsNullOrWhiteSpace(pkVal.ToString()))
                return BadRequest(new { message = $"Primary key '{pk}' cannot be empty. WHERE clause is mandatory for UPDATE." });
        }

        var setCols = new Dictionary<string, object?>();
        var immutableCols = GetImmutableColumns(columns, tableName);
        foreach (var kv in dto.RowData)
        {
            if (!validCols.Contains(kv.Key)) continue;
            if (pkCols.Contains(kv.Key)) continue; // don't update PK
            if (immutableCols.Contains(kv.Key))
                continue; // don't update immutable id/code columns on master tables
            setCols[kv.Key] = kv.Value;
        }

        if (setCols.Count == 0) return BadRequest(new { message = "No valid columns to update." });

        var setParts = new List<string>();
        var parameters = new List<NpgsqlParameter>();
        int idx = 0;

        foreach (var kv in setCols)
        {
            var colInfo = columns.FirstOrDefault(c => c.Name == kv.Key);
            setParts.Add($"\"{kv.Key}\" = @p{idx}");
            parameters.Add(new NpgsqlParameter($"@p{idx}", CoerceValue(kv.Value, colInfo?.DataType) ?? DBNull.Value));
            idx++;
        }

        var whereParts = new List<string>();
        foreach (var pk in pkCols)
        {
            var colInfo = columns.FirstOrDefault(c => c.Name == pk);
            whereParts.Add($"\"{pk}\" = @pk{idx}");
            parameters.Add(new NpgsqlParameter($"@pk{idx}", CoerceValue(dto.PkValues[pk], colInfo?.DataType) ?? DBNull.Value));
            idx++;
        }

        var conn = _db.Database.GetDbConnection();
        await conn.OpenAsync();
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"UPDATE {Schema}.\"{tableName}\" SET {string.Join(", ", setParts)} WHERE {string.Join(" AND ", whereParts)}";
            foreach (var p in parameters) cmd.Parameters.Add(p);

            var affected = await cmd.ExecuteNonQueryAsync();
            if (affected == 0) return NotFound(new { message = "Row not found." });
            AuditLog("UPDATE", tableName, new { pkValues = dto.PkValues, changedColumns = setCols.Keys.ToList() });
            return Ok(new { message = "Row updated successfully.", affected });
        }
        catch (PostgresException ex) when (ex.SqlState == "23503")
        {
            return BadRequest(new { message = $"Foreign key violation: {ex.MessageText}" });
        }
        catch (PostgresException ex) when (ex.SqlState == "23505")
        {
            return BadRequest(new { message = $"Duplicate key: {ex.MessageText}" });
        }
        catch (PostgresException ex) when (ex.SqlState == "23502")
        {
            return BadRequest(new { message = $"NOT NULL violation: {ex.MessageText}" });
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "Update failed for {Table}", tableName);
            return BadRequest(new { message = $"Database error: {ex.MessageText}" });
        }
        finally { if (conn.State == System.Data.ConnectionState.Open) await conn.CloseAsync(); }
    }

    // ══════════════════════════════════════════════════════════════════════
    //  DELETE Row (with referential integrity check)
    // ══════════════════════════════════════════════════════════════════════

    [HttpPost("tables/{tableName}/delete")]
    public async Task<IActionResult> DeleteRow(string tableName, [FromBody] Dictionary<string, object?> pkValues)
    {
        if (!IsAdmin()) return StatusCode(403, new { message = "Access denied." });
        if (!await IsValidTable(tableName)) return BadRequest(new { message = "Invalid table." });
        if (await IsView(tableName)) return BadRequest(new { message = "Cannot delete from a view." });
        if (IsProtected(tableName))
            return BadRequest(new { message = $"Table '{tableName}' is protected. Delete operations are not allowed." });
        if (IsDeleteRestricted(tableName))
            return BadRequest(new { message = $"Table '{tableName}' is delete-restricted. Rows cannot be removed from this master table." });

        var pkCols = await GetPrimaryKeyColumns(tableName);
        if (pkCols.Count == 0) return BadRequest(new { message = "Table has no primary key — cannot delete." });

        var columns = await GetColumnDetails(tableName);

        foreach (var pk in pkCols)
        {
            if (!pkValues.ContainsKey(pk))
                return BadRequest(new { message = $"Missing primary key value for '{pk}'." });
            var pkVal = pkValues[pk];
            if (pkVal == null || (pkVal is JsonElement je && je.ValueKind == JsonValueKind.Null) || string.IsNullOrWhiteSpace(pkVal.ToString()))
                return BadRequest(new { message = $"Primary key '{pk}' cannot be empty. WHERE clause is mandatory for DELETE." });
        }

        // Check referential integrity — are there child records?
        var conn = _db.Database.GetDbConnection();
        await conn.OpenAsync();
        try
        {
            var refCheckSql = @"
                SELECT kcu.table_name, kcu.column_name, ccu.column_name AS pk_col
                FROM information_schema.table_constraints tc
                JOIN information_schema.key_column_usage kcu
                     ON tc.constraint_name = kcu.constraint_name AND tc.table_schema = kcu.table_schema
                JOIN information_schema.constraint_column_usage ccu
                     ON ccu.constraint_name = tc.constraint_name AND ccu.table_schema = tc.table_schema
                WHERE tc.constraint_type = 'FOREIGN KEY'
                  AND tc.table_schema = @schema AND ccu.table_name = @tbl";

            var childRefs = new List<(string table, string fkCol, string pkCol)>();
            using (var refCmd = conn.CreateCommand())
            {
                refCmd.CommandText = refCheckSql;
                refCmd.Parameters.Add(new NpgsqlParameter("@schema", Schema));
                refCmd.Parameters.Add(new NpgsqlParameter("@tbl", tableName));
                using var rdr = await refCmd.ExecuteReaderAsync();
                while (await rdr.ReadAsync())
                    childRefs.Add((rdr.GetString(0), rdr.GetString(1), rdr.GetString(2)));
            }

            // For each child FK, check if records exist
            var violations = new List<string>();
            foreach (var (childTable, fkCol, pkCol) in childRefs)
            {
                if (!pkValues.ContainsKey(pkCol)) continue;
                using var chkCmd = conn.CreateCommand();
                var colInfo = columns.FirstOrDefault(c => c.Name == pkCol);
                chkCmd.CommandText = $"SELECT COUNT(*) FROM {Schema}.\"{childTable}\" WHERE \"{fkCol}\" = @v";
                chkCmd.Parameters.Add(new NpgsqlParameter("@v", CoerceValue(pkValues[pkCol], colInfo?.DataType) ?? DBNull.Value));
                var cnt = Convert.ToInt64(await chkCmd.ExecuteScalarAsync());
                if (cnt > 0)
                    violations.Add($"{HumanizeName(childTable)} ({cnt} record{(cnt > 1 ? "s" : "")})");
            }

            if (violations.Count > 0)
            {
                return BadRequest(new
                {
                    message = "Cannot delete — referenced by child records.",
                    references = violations
                });
            }

            // Perform DELETE
            var whereParts = new List<string>();
            var parameters = new List<NpgsqlParameter>();
            int idx = 0;
            foreach (var pk in pkCols)
            {
                var colInfo = columns.FirstOrDefault(c => c.Name == pk);
                whereParts.Add($"\"{pk}\" = @d{idx}");
                parameters.Add(new NpgsqlParameter($"@d{idx}", CoerceValue(pkValues[pk], colInfo?.DataType) ?? DBNull.Value));
                idx++;
            }

            using var delCmd = conn.CreateCommand();
            delCmd.CommandText = $"DELETE FROM {Schema}.\"{tableName}\" WHERE {string.Join(" AND ", whereParts)}";
            foreach (var p in parameters) delCmd.Parameters.Add(p);

            var affected = await delCmd.ExecuteNonQueryAsync();
            if (affected == 0) return NotFound(new { message = "Row not found." });
            AuditLog("DELETE", tableName, new { pkValues });
            return Ok(new { message = "Row deleted successfully.", affected });
        }
        catch (PostgresException ex) when (ex.SqlState == "23503")
        {
            return BadRequest(new { message = $"Cannot delete — foreign key violation: {ex.MessageText}" });
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "Delete failed for {Table}", tableName);
            return BadRequest(new { message = $"Database error: {ex.MessageText}" });
        }
        finally { if (conn.State == System.Data.ConnectionState.Open) await conn.CloseAsync(); }
    }

    // ══════════════════════════════════════════════════════════════════════
    //  Table Stats
    // ══════════════════════════════════════════════════════════════════════

    [HttpGet("tables/{tableName}/stats")]
    public async Task<IActionResult> GetTableStats(string tableName)
    {
        if (!IsAdmin()) return StatusCode(403, new { message = "Access denied." });
        if (!await IsValidTable(tableName)) return BadRequest(new { message = "Invalid table." });

        var conn = _db.Database.GetDbConnection();
        await conn.OpenAsync();
        try
        {
            long rowCount = 0;
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"SELECT COUNT(*) FROM {Schema}.\"{tableName}\"";
                rowCount = Convert.ToInt64(await cmd.ExecuteScalarAsync());
            }

            long fkCount = 0;
            using (var cmd2 = conn.CreateCommand())
            {
                cmd2.CommandText = @"SELECT COUNT(*) FROM information_schema.table_constraints
                                     WHERE constraint_type='FOREIGN KEY' AND table_schema=@s AND table_name=@t";
                cmd2.Parameters.Add(new NpgsqlParameter("@s", Schema));
                cmd2.Parameters.Add(new NpgsqlParameter("@t", tableName));
                fkCount = Convert.ToInt64(await cmd2.ExecuteScalarAsync());
            }

            long refCount = 0;
            using (var cmd3 = conn.CreateCommand())
            {
                cmd3.CommandText = @"SELECT COUNT(*) FROM information_schema.table_constraints tc
                                     JOIN information_schema.constraint_column_usage ccu
                                          ON ccu.constraint_name=tc.constraint_name AND ccu.table_schema=tc.table_schema
                                     WHERE tc.constraint_type='FOREIGN KEY' AND tc.table_schema=@s AND ccu.table_name=@t";
                cmd3.Parameters.Add(new NpgsqlParameter("@s", Schema));
                cmd3.Parameters.Add(new NpgsqlParameter("@t", tableName));
                refCount = Convert.ToInt64(await cmd3.ExecuteScalarAsync());
            }

            var columns = await GetColumnDetails(tableName);
            var pkCols = await GetPrimaryKeyColumns(tableName);
            var isView = await IsView(tableName);
            var isProtected = IsProtected(tableName);
            var isMasterTable = IsMasterTable(tableName);
            var isDeleteRestricted = isMasterTable;
            var immutableColumns = GetImmutableColumns(columns, tableName);

            return Ok(new
            {
                rowCount,
                columnCount = columns.Count,
                fkCount,
                refCount,
                pkColumns = pkCols,
                isView,
                readOnly = isView || isProtected,
                isProtected,
                isDeleteRestricted,
                isMasterTable,
                immutableColumns
            });
        }
        finally { if (conn.State == System.Data.ConnectionState.Open) await conn.CloseAsync(); }
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
            cmd.CommandText = "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema=@s AND table_name=@t";
            cmd.Parameters.Add(new NpgsqlParameter("@s", Schema));
            cmd.Parameters.Add(new NpgsqlParameter("@t", tableName));
            return Convert.ToInt64(await cmd.ExecuteScalarAsync()) > 0;
        }
        finally { if (!wasOpen && conn.State == System.Data.ConnectionState.Open) await conn.CloseAsync(); }
    }

    private async Task<bool> IsView(string tableName)
    {
        var conn = _db.Database.GetDbConnection();
        var wasOpen = conn.State == System.Data.ConnectionState.Open;
        if (!wasOpen) await conn.OpenAsync();
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT table_type FROM information_schema.tables WHERE table_schema=@s AND table_name=@t";
            cmd.Parameters.Add(new NpgsqlParameter("@s", Schema));
            cmd.Parameters.Add(new NpgsqlParameter("@t", tableName));
            var result = (await cmd.ExecuteScalarAsync())?.ToString();
            return result == "VIEW";
        }
        finally { if (!wasOpen && conn.State == System.Data.ConnectionState.Open) await conn.CloseAsync(); }
    }

    private async Task<List<string>> GetPrimaryKeyColumns(string tableName)
    {
        var pkCols = new List<string>();
        var conn = _db.Database.GetDbConnection();
        var wasOpen = conn.State == System.Data.ConnectionState.Open;
        if (!wasOpen) await conn.OpenAsync();
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT kcu.column_name
                FROM information_schema.table_constraints tc
                JOIN information_schema.key_column_usage kcu
                     ON tc.constraint_name = kcu.constraint_name AND tc.table_schema = kcu.table_schema
                WHERE tc.constraint_type = 'PRIMARY KEY'
                  AND tc.table_schema = @s AND tc.table_name = @t
                ORDER BY kcu.ordinal_position";
            cmd.Parameters.Add(new NpgsqlParameter("@s", Schema));
            cmd.Parameters.Add(new NpgsqlParameter("@t", tableName));
            using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
                pkCols.Add(rdr.GetString(0));
        }
        finally { if (!wasOpen && conn.State == System.Data.ConnectionState.Open) await conn.CloseAsync(); }
        return pkCols;
    }

    private async Task<List<string>> GetValidColumnNames(string tableName)
    {
        var cols = new List<string>();
        var conn = _db.Database.GetDbConnection();
        var wasOpen = conn.State == System.Data.ConnectionState.Open;
        if (!wasOpen) await conn.OpenAsync();
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT column_name FROM information_schema.columns WHERE table_schema=@s AND table_name=@t ORDER BY ordinal_position";
            cmd.Parameters.Add(new NpgsqlParameter("@s", Schema));
            cmd.Parameters.Add(new NpgsqlParameter("@t", tableName));
            using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync()) cols.Add(rdr.GetString(0));
        }
        finally { if (!wasOpen && conn.State == System.Data.ConnectionState.Open) await conn.CloseAsync(); }
        return cols;
    }

    private async Task<List<ColumnDetail>> GetColumnDetails(string tableName)
    {
        var columns = new List<ColumnDetail>();
        var conn = _db.Database.GetDbConnection();
        var wasOpen = conn.State == System.Data.ConnectionState.Open;
        if (!wasOpen) await conn.OpenAsync();
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT c.column_name, c.data_type, c.is_nullable,
                       c.character_maximum_length, c.numeric_precision,
                       c.column_default,
                       COALESCE(pgd.description, '') AS col_comment,
                       CASE WHEN pk.column_name IS NOT NULL THEN true ELSE false END AS is_pk,
                       c.udt_name
                FROM information_schema.columns c
                LEFT JOIN pg_catalog.pg_statio_all_tables st
                     ON st.schemaname = c.table_schema AND st.relname = c.table_name
                LEFT JOIN pg_catalog.pg_description pgd
                     ON pgd.objoid = st.relid AND pgd.objsubid = c.ordinal_position
                LEFT JOIN (
                    SELECT kcu.column_name
                    FROM information_schema.table_constraints tc
                    JOIN information_schema.key_column_usage kcu
                         ON tc.constraint_name = kcu.constraint_name AND tc.table_schema = kcu.table_schema
                    WHERE tc.constraint_type = 'PRIMARY KEY' AND tc.table_schema = @s AND tc.table_name = @t
                ) pk ON pk.column_name = c.column_name
                WHERE c.table_schema = @s AND c.table_name = @t
                ORDER BY c.ordinal_position";
            cmd.Parameters.Add(new NpgsqlParameter("@s", Schema));
            cmd.Parameters.Add(new NpgsqlParameter("@t", tableName));
            using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                columns.Add(new ColumnDetail
                {
                    Name = rdr.GetString(0),
                    DisplayName = HumanizeName(rdr.GetString(0)),
                    DataType = rdr.GetString(1),
                    IsNullable = rdr.GetString(2) == "YES",
                    MaxLength = rdr.IsDBNull(3) ? null : rdr.GetInt32(3),
                    Precision = rdr.IsDBNull(4) ? null : rdr.GetInt32(4),
                    HasDefault = !rdr.IsDBNull(5),
                    DefaultValue = rdr.IsDBNull(5) ? null : rdr.GetString(5),
                    Comment = rdr.IsDBNull(6) ? "" : rdr.GetString(6),
                    IsPk = rdr.GetBoolean(7),
                    IsNumeric = IsNumericType(rdr.GetString(1)),
                    IsDate = IsDateType(rdr.GetString(1)),
                    IsBoolean = rdr.GetString(1) == "boolean",
                    UdtName = rdr.IsDBNull(8) ? null : rdr.GetString(8)
                });
            }
        }
        finally { if (!wasOpen && conn.State == System.Data.ConnectionState.Open) await conn.CloseAsync(); }
        return columns;
    }

    private static object? CoerceValue(object? value, string? dataType)
    {
        if (value == null) return null;

        var str = value is JsonElement je ? je.GetRawText().Trim('"') : value.ToString();
        if (string.IsNullOrEmpty(str)) return null;

        return dataType switch
        {
            "integer" or "smallint" => int.TryParse(str, out var i) ? i : (object)str,
            "bigint" => long.TryParse(str, out var l) ? l : (object)str,
            "numeric" or "decimal" or "money" => decimal.TryParse(str, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var d) ? d : (object)str,
            "real" => float.TryParse(str, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var f) ? f : (object)str,
            "double precision" => double.TryParse(str, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var db) ? db : (object)str,
            "boolean" => bool.TryParse(str, out var b) ? b : str == "1" || str.Equals("true", StringComparison.OrdinalIgnoreCase),
            "date" => DateTime.TryParse(str, out var dt) ? DateOnly.FromDateTime(dt) : (object)str,
            var t when t != null && t.Contains("timestamp") => DateTime.TryParse(str, out var ts) ? ts : (object)str,
            _ => str
        };
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
        var n = name;
        foreach (var pfx in new[] { "mst_", "trn_", "hr_", "hyb_", "rpt_", "vw_", "sys_", "txn_" })
            if (n.StartsWith(pfx)) { n = n[pfx.Length..]; break; }
        return string.Join(" ", n.Split('_').Select(w => w.Length > 0 ? char.ToUpper(w[0]) + w[1..] : w));
    }

    // ── Inner Classes ──
    private class ColumnDetail
    {
        public string Name { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string DataType { get; set; } = "";
        public bool IsNullable { get; set; }
        public int? MaxLength { get; set; }
        public int? Precision { get; set; }
        public bool HasDefault { get; set; }
        public string? DefaultValue { get; set; }
        public string Comment { get; set; } = "";
        public bool IsPk { get; set; }
        public bool IsNumeric { get; set; }
        public bool IsDate { get; set; }
        public bool IsBoolean { get; set; }
        public string? UdtName { get; set; }
    }
}

// ── DTOs ──
public class DbRowsRequestDto
{
    public string? Search { get; set; }
    public string? SortColumn { get; set; }
    public string? SortDir { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class DbUpdateDto
{
    public Dictionary<string, object?> PkValues { get; set; } = new();
    public Dictionary<string, object?> RowData { get; set; } = new();
}
