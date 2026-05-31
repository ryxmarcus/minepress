using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class VwMstItem
{
    public long? ItemId { get; set; }

    public string? ItemGroup { get; set; }

    public string? ItemCode { get; set; }

    public string? ItemName { get; set; }

    public string? ItemDescription { get; set; }

    public string? ItemCategory { get; set; }

    public string? Uom { get; set; }

    public decimal? PurchaseRate { get; set; }

    public decimal? ReorderLevel { get; set; }

    public decimal? CurrentStock { get; set; }

    public string? HsnCode { get; set; }

    public decimal? GstRate { get; set; }

    public decimal? LastPurchaseRate { get; set; }

    public DateOnly? LastPurchaseDate { get; set; }

    public bool? IsActive { get; set; }

    public string? Remarks { get; set; }

    public string? SourceTable { get; set; }

    public long? SourceId { get; set; }
}
