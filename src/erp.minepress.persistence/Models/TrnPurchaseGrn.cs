using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class TrnPurchaseGrn
{
    public long GrnId { get; set; }

    public string GrnNo { get; set; } = null!;

    public DateOnly GrnDate { get; set; }

    public string GrnType { get; set; } = null!;

    public long? JobId { get; set; }

    public string? JobNo { get; set; }

    public long? RateCalcId { get; set; }

    public long? PurchaseOrderId { get; set; }

    public string? PurchaseOrderNo { get; set; }

    public int? SupplierId { get; set; }

    public string? SupplierName { get; set; }

    public string? InvoiceNo { get; set; }

    public DateOnly? InvoiceDate { get; set; }

    public int? LocationId { get; set; }

    public int CompanyId { get; set; }

    public int? TotalItems { get; set; }

    public decimal? TotalAmount { get; set; }

    public decimal? TaxAmount { get; set; }

    public decimal? NetAmount { get; set; }

    public string Status { get; set; } = null!;

    public string? QualityStatus { get; set; }

    public string? Remarks { get; set; }

    public long? ApprovedBy { get; set; }

    public DateTime? ApprovedOn { get; set; }

    public long CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<TrnPurchaseGrnItem> TrnPurchaseGrnItems { get; set; } = new List<TrnPurchaseGrnItem>();
}
