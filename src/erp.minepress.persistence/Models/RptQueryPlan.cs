using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class RptQueryPlan
{
    public long QueryPlanId { get; set; }

    public long? ReportId { get; set; }

    public string ReportName { get; set; } = null!;

    public string SourceTable { get; set; } = null!;

    public string GeneratedSql { get; set; } = null!;

    public string? JoinClause { get; set; }

    public string? WhereClause { get; set; }

    public string? GroupByClause { get; set; }

    public string? HavingClause { get; set; }

    public string? OrderByClause { get; set; }

    public string? SelectedColumns { get; set; }

    public string? FilterJson { get; set; }

    public string? ParametersJson { get; set; }

    public long RowCount { get; set; }

    public int ExecutionTimeMs { get; set; }

    public string ExecutedBy { get; set; } = null!;

    public DateTime ExecutedOn { get; set; }

    public bool IsActive { get; set; }

    public virtual RptSavedReport? Report { get; set; }
}
