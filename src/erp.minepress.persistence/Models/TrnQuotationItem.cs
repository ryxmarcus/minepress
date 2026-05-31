using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class TrnQuotationItem
{
    public long QuotationItemId { get; set; }

    public long QuotationId { get; set; }

    public long? EnquiryItemId { get; set; }

    public int? ItemSequence { get; set; }

    public string ProductName { get; set; } = null!;

    public string? ProductDescription { get; set; }

    public int Quantity { get; set; }

    public int? UomId { get; set; }

    public decimal? UnitRate { get; set; }

    public decimal? GrossAmount { get; set; }

    public decimal? DiscountPercent { get; set; }

    public decimal? DiscountAmount { get; set; }

    public decimal? TaxableValue { get; set; }

    public decimal? CgstPercent { get; set; }

    public decimal? CgstAmount { get; set; }

    public decimal? SgstPercent { get; set; }

    public decimal? SgstAmount { get; set; }

    public decimal? IgstPercent { get; set; }

    public decimal? IgstAmount { get; set; }

    public decimal? TotalTaxAmount { get; set; }

    public decimal? NetAmount { get; set; }

    public long? RateCalculatorId { get; set; }

    public string? CalcRefNo { get; set; }

    public string? Remarks { get; set; }

    public long CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public int? PrintProductTypeId { get; set; }

    public int? JobTypeId { get; set; }

    public string? ProductTypeName { get; set; }

    public string? JobTypeName { get; set; }

    public string? ProductSizeName { get; set; }

    public decimal? TrimWidthMm { get; set; }

    public decimal? TrimHeightMm { get; set; }

    public string? PrintingMethod { get; set; }

    public int? NoOfPages { get; set; }

    public virtual MstUser CreatedByNavigation { get; set; } = null!;

    public virtual TrnEnquiryItem? EnquiryItem { get; set; }

    public virtual MstJobType? JobType { get; set; }

    public virtual MstPrintProductType? PrintProductType { get; set; }

    public virtual TrnQuotation Quotation { get; set; } = null!;

    public virtual HybJobRateCalculator? RateCalculator { get; set; }
}
