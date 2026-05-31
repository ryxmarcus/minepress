using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Sales invoice header for goods/services sold to customers. Linked to job, quotation. Supports GST (CGST/SGST/IGST), e-Invoice IRN, e-Way Bill.
/// </summary>
public partial class TrnSalesInvoice
{
    public long SalesInvoiceId { get; set; }

    public string InvoiceNo { get; set; } = null!;

    public DateOnly InvoiceDate { get; set; }

    public DateOnly? DueDate { get; set; }

    public int CompanyId { get; set; }

    public int? LocationId { get; set; }

    public int? FinYearId { get; set; }

    public int PartyId { get; set; }

    public int? BillingAddressId { get; set; }

    public int? ShippingAddressId { get; set; }

    public long? JobId { get; set; }

    public long? QuotationId { get; set; }

    public int? CurrencyId { get; set; }

    public decimal? ExchangeRate { get; set; }

    public int? PaymentTermId { get; set; }

    public string? SalesPerson { get; set; }

    public string? PlaceOfSupply { get; set; }

    public bool? IsExport { get; set; }

    public string? ExportType { get; set; }

    public string? LutNo { get; set; }

    public string? PoNo { get; set; }

    public DateOnly? PoDate { get; set; }

    public string? DispatchThrough { get; set; }

    public string? VehicleNo { get; set; }

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

    public string? EInvoiceIrn { get; set; }

    public string? EInvoiceAckNo { get; set; }

    public DateTime? EInvoiceAckDate { get; set; }

    public string? EWayBillNo { get; set; }

    public string? TermsConditions { get; set; }

    public string? InternalNotes { get; set; }

    public string? AttachmentsJson { get; set; }

    public long CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual MstPartyAddress? BillingAddress { get; set; }

    public virtual MstCompany Company { get; set; } = null!;

    public virtual MstUser CreatedByNavigation { get; set; } = null!;

    public virtual MstCurrency? Currency { get; set; }

    public virtual MstFinancialYear? FinYear { get; set; }

    public virtual TrnJob? Job { get; set; }

    public virtual MstParty Party { get; set; } = null!;

    public virtual MstPaymentTerm? PaymentTerm { get; set; }

    public virtual TrnQuotation? Quotation { get; set; }

    public virtual MstPartyAddress? ShippingAddress { get; set; }

    public virtual ICollection<TrnCreditNote> TrnCreditNotes { get; set; } = new List<TrnCreditNote>();

    public virtual ICollection<TrnProformaInvoice> TrnProformaInvoices { get; set; } = new List<TrnProformaInvoice>();

    public virtual ICollection<TrnReceiptAllocation> TrnReceiptAllocations { get; set; } = new List<TrnReceiptAllocation>();

    public virtual ICollection<TrnSalesInvoiceItem> TrnSalesInvoiceItems { get; set; } = new List<TrnSalesInvoiceItem>();
}
