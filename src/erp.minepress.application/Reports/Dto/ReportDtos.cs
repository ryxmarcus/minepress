namespace erp.minepress.application.Reports.Dto;

/// <summary>
/// Request DTO for building and executing a dynamic report query
/// </summary>
public class ReportQueryRequest
{
    public long? ReportId { get; set; }
    public string SourceTable { get; set; } = "";
    public List<string>? Columns { get; set; }
    public List<ReportFilterItem>? Filters { get; set; }
    public List<ReportOrderItem>? OrderByColumns { get; set; }
    public List<string>? GroupByColumns { get; set; }
    public List<ReportAggregateItem>? Aggregates { get; set; }
    public List<ReportHavingItem>? HavingClauses { get; set; }
    public List<JoinDefinition>? JoinedTables { get; set; }
    public string? ReportType { get; set; }
    public bool ShowTotals { get; set; }
    public bool ShowGrandTotal { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string? ExecutedBy { get; set; }
}

public class JoinDefinition
{
    public string Table { get; set; } = "";
    public string JoinType { get; set; } = "LEFT";
    public string FkColumn { get; set; } = "";
    public string PkColumn { get; set; } = "";
}

public class ReportFilterItem
{
    public string ColumnName { get; set; } = "";
    public string Operator { get; set; } = "eq";
    public string? FilterValue { get; set; }
    public string? FilterValue2 { get; set; }
    public string? LogicOperator { get; set; } = "AND";
}

public class ReportOrderItem
{
    public string Column { get; set; } = "";
    public string? Dir { get; set; } = "ASC";
}

public class ReportAggregateItem
{
    public string Column { get; set; } = "";
    public string Function { get; set; } = "COUNT";
}

public class ReportHavingItem
{
    public string AggregateFunction { get; set; } = "COUNT";
    public string Column { get; set; } = "";
    public string Operator { get; set; } = "gt";
    public string Value { get; set; } = "0";
}

/// <summary>
/// Result of SQL generation — the decomposed query components
/// </summary>
public class GeneratedQuery
{
    public string FullSql { get; set; } = "";
    public string CountSql { get; set; } = "";
    public string SelectClause { get; set; } = "";
    public string FromClause { get; set; } = "";
    public string? JoinClause { get; set; }
    public string? WhereClause { get; set; }
    public string? GroupByClause { get; set; }
    public string? HavingClause { get; set; }
    public string? OrderByClause { get; set; }
    public string? SelectedColumns { get; set; }
    public string? FilterJson { get; set; }
    public List<QueryParameter> Parameters { get; set; } = [];
    public List<QueryParameter> CountParameters { get; set; } = [];
    public bool IsSummary { get; set; }
    public int PageSize { get; set; }
    public int Page { get; set; }
}

public class QueryParameter
{
    public string Name { get; set; } = "";
    public object? Value { get; set; }
}

/// <summary>
/// Paginated report execution result returned as JSON
/// </summary>
public class ReportQueryResult
{
    public List<Dictionary<string, object?>> Data { get; set; } = [];
    public long TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public string Sql { get; set; } = "";
    public List<string> ColumnNames { get; set; } = [];
    public Dictionary<string, object?>? Totals { get; set; }
    public string ReportType { get; set; } = "detail";
    public long? QueryPlanId { get; set; }
    public int ExecutionTimeMs { get; set; }
}

/// <summary>
/// Metadata about a valid column used during query building
/// </summary>
public class ColumnMetadata
{
    public string Name { get; set; } = "";
    public string DataType { get; set; } = "";
    public bool IsNumeric { get; set; }
    public bool IsDate { get; set; }
    public bool IsBoolean { get; set; }
}
