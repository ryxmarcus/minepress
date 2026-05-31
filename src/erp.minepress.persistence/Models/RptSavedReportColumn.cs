using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Column selection and display config for saved reports
/// </summary>
public partial class RptSavedReportColumn
{
    public long ReportColumnId { get; set; }

    public long ReportId { get; set; }

    public string ColumnName { get; set; } = null!;

    public string? DisplayName { get; set; }

    public int ColumnOrder { get; set; }

    public bool IsVisible { get; set; }

    public string? AggregateFunction { get; set; }

    public string? FormatString { get; set; }

    public int? ColumnWidth { get; set; }

    public bool IsActive { get; set; }

    public virtual RptSavedReport Report { get; set; } = null!;
}
