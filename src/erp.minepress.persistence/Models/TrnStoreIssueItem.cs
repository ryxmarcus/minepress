using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class TrnStoreIssueItem
{
    public long IssueItemId { get; set; }

    public long IssueId { get; set; }

    public int ItemSequence { get; set; }

    public string MaterialCategory { get; set; } = null!;

    public long? MaterialId { get; set; }

    public string? MaterialCode { get; set; }

    public string MaterialName { get; set; } = null!;

    public string? Specification { get; set; }

    public decimal? BomQuantity { get; set; }

    public decimal IssuedQuantity { get; set; }

    public string? Uom { get; set; }

    public decimal? Rate { get; set; }

    public decimal? Amount { get; set; }

    public decimal? AvailableStock { get; set; }

    public string? ForPart { get; set; }

    public string? Remarks { get; set; }

    public bool? IsSelected { get; set; }

    public DateTime? CreatedOn { get; set; }

    public virtual TrnStoreIssue Issue { get; set; } = null!;
}
