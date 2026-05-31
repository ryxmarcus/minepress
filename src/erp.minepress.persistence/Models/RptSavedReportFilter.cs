using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Filter conditions for saved reports
/// </summary>
public partial class RptSavedReportFilter
{
    public long ReportFilterId { get; set; }

    public long ReportId { get; set; }

    public string ColumnName { get; set; } = null!;

    public string Operator { get; set; } = null!;

    public string? FilterValue { get; set; }

    public string? FilterValue2 { get; set; }

    public int FilterOrder { get; set; }

    public string LogicOperator { get; set; } = null!;

    public bool IsActive { get; set; }

    public virtual RptSavedReport Report { get; set; } = null!;
}
