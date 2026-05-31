using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class TrnEnquiry
{
    public long EnquiryId { get; set; }

    public string EnquiryNo { get; set; } = null!;

    public DateOnly EnquiryDate { get; set; }

    public int CompanyId { get; set; }

    public int? LocationId { get; set; }

    public int PartyId { get; set; }

    public string? ContactPerson { get; set; }

    public string? ContactMobile { get; set; }

    public string? ContactEmail { get; set; }

    public string? EnquirySource { get; set; }

    public DateOnly? ExpectedDeliveryDate { get; set; }

    public string? Priority { get; set; }

    public string? Status { get; set; }

    public string? Remarks { get; set; }

    public long CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual MstCompany Company { get; set; } = null!;

    public virtual MstUser CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<HybJobRateCalculator> HybJobRateCalculators { get; set; } = new List<HybJobRateCalculator>();

    public virtual MstParty Party { get; set; } = null!;

    public virtual ICollection<TrnChallanTimeline> TrnChallanTimelines { get; set; } = new List<TrnChallanTimeline>();

    public virtual ICollection<TrnEnquiryItem> TrnEnquiryItems { get; set; } = new List<TrnEnquiryItem>();

    public virtual ICollection<TrnEnquiryTimeline> TrnEnquiryTimelines { get; set; } = new List<TrnEnquiryTimeline>();

    public virtual ICollection<TrnJobTimeline> TrnJobTimelines { get; set; } = new List<TrnJobTimeline>();

    public virtual ICollection<TrnJob> TrnJobs { get; set; } = new List<TrnJob>();

    public virtual ICollection<TrnOutsourceTimeline> TrnOutsourceTimelines { get; set; } = new List<TrnOutsourceTimeline>();

    public virtual ICollection<TrnQuotationTimeline> TrnQuotationTimelines { get; set; } = new List<TrnQuotationTimeline>();

    public virtual ICollection<TrnQuotation> TrnQuotations { get; set; } = new List<TrnQuotation>();
}
