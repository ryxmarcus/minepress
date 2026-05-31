using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Proforma invoice issued to customer before delivery/final sales invoice. Can be converted to sales invoice. Does NOT post to GL.
/// </summary>
public partial class TrnProformaInvoice
{
    public long ProformaInvoiceId { get; set; }

    public string ProformaNo { get; set; } = null!;

    public DateOnly ProformaDate { get; set; }

    public DateOnly? ValidTill { get; set; }

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

    public string? PoNo { get; set; }

    public DateOnly? PoDate { get; set; }

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

    public bool? ConvertedToInvoice { get; set; }

    public long? SalesInvoiceId { get; set; }

    public string Status { get; set; } = null!;

    public string? TermsConditions { get; set; }

    public string? InternalNotes { get; set; }

    public string? AttachmentsJson { get; set; }

    public long CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual MstCompany Company { get; set; } = null!;

    public virtual MstParty Party { get; set; } = null!;

    public virtual TrnSalesInvoice? SalesInvoice { get; set; }

    public virtual ICollection<TrnProformaInvoiceItem> TrnProformaInvoiceItems { get; set; } = new List<TrnProformaInvoiceItem>();
}
