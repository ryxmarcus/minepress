using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// User-saved report definitions for the self-service report builder
/// </summary>
public partial class RptSavedReport
{
    public long ReportId { get; set; }

    public string ReportCode { get; set; } = null!;

    public string ReportName { get; set; } = null!;

    public string? Description { get; set; }

    public string SourceTable { get; set; } = null!;

    public bool IsShared { get; set; }

    public bool IsDefault { get; set; }

    public string? GroupByColumns { get; set; }

    public string? OrderByColumns { get; set; }

    public int PageSize { get; set; }

    public string? ChartType { get; set; }

    public string? ChartConfig { get; set; }

    public string? AiSummaryPrompt { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public bool IsActive { get; set; }

    /// <summary>
    /// detail = row-level, summary = grouped aggregates
    /// </summary>
    public string ReportType { get; set; } = null!;

    /// <summary>
    /// Show column totals for numeric columns
    /// </summary>
    public bool ShowTotals { get; set; }

    /// <summary>
    /// Show grand total row at the bottom
    /// </summary>
    public bool ShowGrandTotal { get; set; }

    /// <summary>
    /// JSON array of joined tables with FK/PK mapping
    /// </summary>
    public string? JoinedTables { get; set; }

    public virtual ICollection<RptQueryPlan> RptQueryPlans { get; set; } = new List<RptQueryPlan>();

    public virtual ICollection<RptSavedReportColumn> RptSavedReportColumns { get; set; } = new List<RptSavedReportColumn>();

    public virtual ICollection<RptSavedReportFilter> RptSavedReportFilters { get; set; } = new List<RptSavedReportFilter>();
}
