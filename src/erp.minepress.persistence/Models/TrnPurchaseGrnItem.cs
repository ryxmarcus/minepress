using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class TrnPurchaseGrnItem
{
    public long GrnItemId { get; set; }

    public long GrnId { get; set; }

    public int ItemSequence { get; set; }

    public string MaterialCategory { get; set; } = null!;

    public long? MaterialId { get; set; }

    public string? MaterialCode { get; set; }

    public string MaterialName { get; set; } = null!;

    public string? Specification { get; set; }

    public decimal? BomQuantity { get; set; }

    public decimal? OrderedQuantity { get; set; }

    public decimal ReceivedQuantity { get; set; }

    public decimal? RejectedQuantity { get; set; }

    public decimal? AcceptedQuantity { get; set; }

    public string? Uom { get; set; }

    public decimal? Rate { get; set; }

    public decimal? Amount { get; set; }

    public decimal? TaxRate { get; set; }

    public decimal? TaxAmount { get; set; }

    public decimal? NetAmount { get; set; }

    public string? BatchNo { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public decimal? AvailableStock { get; set; }

    public string? ForPart { get; set; }

    public string? QualityStatus { get; set; }

    public string? Remarks { get; set; }

    public bool? IsSelected { get; set; }

    public DateTime? CreatedOn { get; set; }

    public virtual TrnPurchaseGrn Grn { get; set; } = null!;
}
