using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Credit note issued to customer for sales returns, rate difference, or post-sale adjustments. Reduces Accounts Receivable. Supports GST and e-Invoice.
/// </summary>
public partial class TrnCreditNote
{
    public long CreditNoteId { get; set; }

    public string CreditNoteNo { get; set; } = null!;

    public DateOnly CreditNoteDate { get; set; }

    /// <summary>
    /// SALES_RETURN, RATE_DIFFERENCE, QUALITY_ISSUE, DISCOUNT_AFTER_SALE, OTHER
    /// </summary>
    public string CreditNoteType { get; set; } = null!;

    public int CompanyId { get; set; }

    public int? LocationId { get; set; }

    public int? FinYearId { get; set; }

    public int PartyId { get; set; }

    public long? OriginalInvoiceId { get; set; }

    public string? OriginalInvoiceNo { get; set; }

    public DateOnly? OriginalInvoiceDate { get; set; }

    public string? Reason { get; set; }

    public int? BillingAddressId { get; set; }

    public int? CurrencyId { get; set; }

    public decimal? ExchangeRate { get; set; }

    public string? PlaceOfSupply { get; set; }

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

    public decimal? AdjustedAmount { get; set; }

    public decimal? UnadjustedAmount { get; set; }

    public string Status { get; set; } = null!;

    public bool? IsCancelled { get; set; }

    public long? CancelledBy { get; set; }

    public DateTime? CancelledOn { get; set; }

    public string? CancelReason { get; set; }

    public bool? IsPostedToGl { get; set; }

    public DateTime? GlPostedOn { get; set; }

    public long? GlPostedBy { get; set; }

    public string? EInvoiceIrn { get; set; }

    public string? EInvoiceAckNo { get; set; }

    public DateTime? EInvoiceAckDate { get; set; }

    public string? InternalNotes { get; set; }

    public string? AttachmentsJson { get; set; }

    public long CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual MstCompany Company { get; set; } = null!;

    public virtual TrnSalesInvoice? OriginalInvoice { get; set; }

    public virtual MstParty Party { get; set; } = null!;

    public virtual ICollection<TrnCreditNoteItem> TrnCreditNoteItems { get; set; } = new List<TrnCreditNoteItem>();
}
