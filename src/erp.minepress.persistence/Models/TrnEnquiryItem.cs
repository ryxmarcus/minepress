using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class TrnEnquiryItem
{
    public long EnquiryItemId { get; set; }

    public long EnquiryId { get; set; }

    public long? RateCalculatorId { get; set; }

    public string? CalcRefNo { get; set; }

    public int? ItemSequence { get; set; }

    public string ProductName { get; set; } = null!;

    public string? ProductDescription { get; set; }

    public string? ProductTypeName { get; set; }

    public string? JobTypeName { get; set; }

    public string? ProductSizeName { get; set; }

    public int Quantity { get; set; }

    public int? UomId { get; set; }

    public int? NoOfPages { get; set; }

    public decimal? TrimWidthMm { get; set; }

    public decimal? TrimHeightMm { get; set; }

    public string? PrintingMethod { get; set; }

    public string? SpecificationsJson { get; set; }

    public string? Status { get; set; }

    public string? Remarks { get; set; }

    public long CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public virtual TrnEnquiry Enquiry { get; set; } = null!;

    public virtual HybJobRateCalculator? RateCalculator { get; set; }

    public virtual ICollection<TrnQuotationItem> TrnQuotationItems { get; set; } = new List<TrnQuotationItem>();
}
