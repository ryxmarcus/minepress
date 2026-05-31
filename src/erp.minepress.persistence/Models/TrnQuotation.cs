using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class TrnQuotation
{
    public long QuotationId { get; set; }

    public string QuotationNo { get; set; } = null!;

    public DateOnly QuotationDate { get; set; }

    public string? PartyRefNo { get; set; }

    public DateOnly? PartyRefDate { get; set; }

    public long? EnquiryId { get; set; }

    public int CompanyId { get; set; }

    public int? LocationId { get; set; }

    public int PartyId { get; set; }

    public DateOnly? ValidTill { get; set; }

    public decimal? TotalAmount { get; set; }

    public decimal? DiscountAmount { get; set; }

    public decimal? TaxableAmount { get; set; }

    public decimal? TaxAmount { get; set; }

    public decimal? NetAmount { get; set; }

    public string? Status { get; set; }

    public string? TermsConditions { get; set; }

    public string? Remarks { get; set; }

    public long CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual MstCompany Company { get; set; } = null!;

    public virtual MstUser CreatedByNavigation { get; set; } = null!;

    public virtual TrnEnquiry? Enquiry { get; set; }

    public virtual ICollection<HybJobRateCalculator> HybJobRateCalculators { get; set; } = new List<HybJobRateCalculator>();

    public virtual MstParty Party { get; set; } = null!;

    public virtual ICollection<TrnChallanTimeline> TrnChallanTimelines { get; set; } = new List<TrnChallanTimeline>();

    public virtual ICollection<TrnJobTimeline> TrnJobTimelines { get; set; } = new List<TrnJobTimeline>();

    public virtual ICollection<TrnJob> TrnJobs { get; set; } = new List<TrnJob>();

    public virtual ICollection<TrnOutsourceTimeline> TrnOutsourceTimelines { get; set; } = new List<TrnOutsourceTimeline>();

    public virtual ICollection<TrnQuotationItem> TrnQuotationItems { get; set; } = new List<TrnQuotationItem>();

    public virtual ICollection<TrnQuotationTimeline> TrnQuotationTimelines { get; set; } = new List<TrnQuotationTimeline>();

    public virtual ICollection<TrnSalesInvoice> TrnSalesInvoices { get; set; } = new List<TrnSalesInvoice>();
}
