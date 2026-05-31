using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// GRN line items with accepted/rejected quantities and quality check status.
/// </summary>
public partial class TrnGoodsReceiptItem
{
    public long GrnItemId { get; set; }

    public long GrnId { get; set; }

    public long? PoItemId { get; set; }

    public int ItemSequence { get; set; }

    public long? ItemId { get; set; }

    public string Description { get; set; } = null!;

    public int? UomId { get; set; }

    public decimal? OrderedQuantity { get; set; }

    public decimal? ReceivedQuantity { get; set; }

    public decimal? AcceptedQuantity { get; set; }

    public decimal? RejectedQuantity { get; set; }

    public decimal? UnitRate { get; set; }

    public decimal? Amount { get; set; }

    public string? BatchNo { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public string? QualityStatus { get; set; }

    public string? RejectionReason { get; set; }

    public string? Remarks { get; set; }

    public virtual TrnGoodsReceipt Grn { get; set; } = null!;
}
