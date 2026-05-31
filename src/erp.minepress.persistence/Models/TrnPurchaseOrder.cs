using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Purchase order header. Part of AP flow: PO → GRN → Purchase Invoice → Payment. Supports GST and approval workflow.
/// </summary>
public partial class TrnPurchaseOrder
{
    public long PurchaseOrderId { get; set; }

    public string PoNo { get; set; } = null!;

    public DateOnly PoDate { get; set; }

    public int CompanyId { get; set; }

    public int? LocationId { get; set; }

    public int? FinYearId { get; set; }

    public int PartyId { get; set; }

    public int? SupplierId { get; set; }

    public int? BillingAddressId { get; set; }

    public int? ShippingAddressId { get; set; }

    public int? CurrencyId { get; set; }

    public decimal? ExchangeRate { get; set; }

    public int? PaymentTermId { get; set; }

    public DateOnly? ExpectedDeliveryDate { get; set; }

    public decimal? SubtotalAmount { get; set; }

    public decimal? DiscountAmount { get; set; }

    public decimal? TaxableAmount { get; set; }

    public decimal? CgstAmount { get; set; }

    public decimal? SgstAmount { get; set; }

    public decimal? IgstAmount { get; set; }

    public decimal? CessAmount { get; set; }

    public decimal? TotalTaxAmount { get; set; }

    public decimal? RoundOff { get; set; }

    public decimal? GrandTotal { get; set; }

    public string Status { get; set; } = null!;

    public bool? IsApproved { get; set; }

    public long? ApprovedBy { get; set; }

    public DateTime? ApprovedOn { get; set; }

    public bool? IsCancelled { get; set; }

    public long? CancelledBy { get; set; }

    public DateTime? CancelledOn { get; set; }

    public string? CancelReason { get; set; }

    public string? TermsConditions { get; set; }

    public string? InternalNotes { get; set; }

    public string? AttachmentsJson { get; set; }

    public long CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual MstCompany Company { get; set; } = null!;

    public virtual MstParty Party { get; set; } = null!;

    public virtual ICollection<TrnGoodsReceipt> TrnGoodsReceipts { get; set; } = new List<TrnGoodsReceipt>();

    public virtual ICollection<TrnPurchaseOrderItem> TrnPurchaseOrderItems { get; set; } = new List<TrnPurchaseOrderItem>();
}
