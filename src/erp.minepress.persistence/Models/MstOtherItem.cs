using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstOtherItem
{
    public long ItemId { get; set; }

    public string ItemCode { get; set; } = null!;

    public string ItemName { get; set; } = null!;

    public string? ItemCategory { get; set; }

    public string? ItemType { get; set; }

    public string? Description { get; set; }

    public string? Uom { get; set; }

    public decimal? RatePerUnit { get; set; }

    public decimal? ReorderLevel { get; set; }

    public decimal? CurrentStock { get; set; }

    public decimal? MinOrderQty { get; set; }

    public int? LeadTimeDays { get; set; }

    public decimal? LastPurchaseRate { get; set; }

    public DateOnly? LastPurchaseDate { get; set; }

    public string? SupplierName { get; set; }

    public string? Brand { get; set; }

    public string? HsnCode { get; set; }

    public decimal? GstRate { get; set; }

    public bool? IsActive { get; set; }

    public string? Remarks { get; set; }

    public long? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }
}
