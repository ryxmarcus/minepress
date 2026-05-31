using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class TrnJobOutsourceItem
{
    public long OutsourceItemId { get; set; }

    public long OutsourceId { get; set; }

    public long JobItemId { get; set; }

    public int? ItemSequence { get; set; }

    public string? ProductName { get; set; }

    public string? ProcessName { get; set; }

    public decimal Quantity { get; set; }

    public decimal? Rate { get; set; }

    public decimal? Amount { get; set; }

    public int? UomId { get; set; }

    public string? Status { get; set; }

    public string? Remarks { get; set; }

    public DateTime? CreatedOn { get; set; }

    public virtual TrnJobItem JobItem { get; set; } = null!;

    public virtual TrnJobOutsource Outsource { get; set; } = null!;
}
