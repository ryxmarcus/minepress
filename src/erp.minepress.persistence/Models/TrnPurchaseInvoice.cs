using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Purchase invoice header for goods/services received from suppliers. Supports GST (CGST/SGST/IGST), reverse charge, TDS, import purchases.
/// </summary>
public partial class TrnPurchaseInvoice
{
    public long PurchaseInvoiceId { get; set; }

    public string InvoiceNo { get; set; } = null!;

    public DateOnly InvoiceDate { get; set; }

    public DateOnly? DueDate { get; set; }

    public int CompanyId { get; set; }

    public int? LocationId { get; set; }

    public int? FinYearId { get; set; }

    public int PartyId { get; set; }

    public int? SupplierId { get; set; }

    public string? SupplierInvoiceNo { get; set; }

    public DateOnly? SupplierInvoiceDate { get; set; }

    public int? BillingAddressId { get; set; }

    public int? ShippingAddressId { get; set; }

    public int? CurrencyId { get; set; }

    public decimal? ExchangeRate { get; set; }

    public int? PaymentTermId { get; set; }

    public string? PlaceOfSupply { get; set; }

    public bool? IsImport { get; set; }

    public bool? IsReverseCharge { get; set; }

    public string? PoNo { get; set; }

    public DateOnly? PoDate { get; set; }

    public string? GrnNo { get; set; }

    public DateOnly? GrnDate { get; set; }

    public decimal? SubtotalAmount { get; set; }

    public decimal? DiscountAmount { get; set; }

    public decimal? TaxableAmount { get; set; }

    public decimal? CgstAmount { get; set; }

    public decimal? SgstAmount { get; set; }

    public decimal? IgstAmount { get; set; }

    public decimal? CessAmount { get; set; }

    public decimal? TotalTaxAmount { get; set; }

    public decimal? TdsAmount { get; set; }

    public decimal? RoundOff { get; set; }

    public decimal? GrandTotal { get; set; }

    public decimal? PaidAmount { get; set; }

    public decimal? BalanceAmount { get; set; }

    public string Status { get; set; } = null!;

    public bool? IsCancelled { get; set; }

    public long? CancelledBy { get; set; }

    public DateTime? CancelledOn { get; set; }

    public string? CancelReason { get; set; }

    public bool? IsPostedToGl { get; set; }

    public DateTime? GlPostedOn { get; set; }

    public long? GlPostedBy { get; set; }

    public string? InternalNotes { get; set; }

    public string? AttachmentsJson { get; set; }

    public long CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual MstCompany Company { get; set; } = null!;

    public virtual MstFinancialYear? FinYear { get; set; }

    public virtual MstParty Party { get; set; } = null!;

    public virtual MstPaymentTerm? PaymentTerm { get; set; }

    public virtual ICollection<TrnDebitNote> TrnDebitNotes { get; set; } = new List<TrnDebitNote>();

    public virtual ICollection<TrnPurchaseInvoiceItem> TrnPurchaseInvoiceItems { get; set; } = new List<TrnPurchaseInvoiceItem>();
}
