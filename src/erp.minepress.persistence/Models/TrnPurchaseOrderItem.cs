using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Purchase order line items with GST breakup. Tracks received vs pending quantities.
/// </summary>
public partial class TrnPurchaseOrderItem
{
    public long PoItemId { get; set; }

    public long PurchaseOrderId { get; set; }

    public int ItemSequence { get; set; }

    public long? ItemId { get; set; }

    public string Description { get; set; } = null!;

    public string? HsnSacCode { get; set; }

    public int? UomId { get; set; }

    public decimal? Quantity { get; set; }

    public decimal? ReceivedQuantity { get; set; }

    public decimal? PendingQuantity { get; set; }

    public decimal? UnitRate { get; set; }

    public decimal? DiscountPercent { get; set; }

    public decimal? DiscountAmount { get; set; }

    public decimal? TaxableValue { get; set; }

    public int? TaxCategoryId { get; set; }

    public decimal? CgstPercent { get; set; }

    public decimal? CgstAmount { get; set; }

    public decimal? SgstPercent { get; set; }

    public decimal? SgstAmount { get; set; }

    public decimal? IgstPercent { get; set; }

    public decimal? IgstAmount { get; set; }

    public decimal? CessPercent { get; set; }

    public decimal? CessAmount { get; set; }

    public decimal? TotalTaxAmount { get; set; }

    public decimal? LineTotal { get; set; }

    public string? Status { get; set; }

    public string? Remarks { get; set; }

    public virtual TrnPurchaseOrder PurchaseOrder { get; set; } = null!;
}
