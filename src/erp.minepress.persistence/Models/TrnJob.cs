using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class TrnJob
{
    public long JobId { get; set; }

    public string JobNo { get; set; } = null!;

    public DateOnly JobDate { get; set; }

    public string? PartyRefNo { get; set; }

    public DateTime? PartyRefNoDate { get; set; }

    public long? QuotationId { get; set; }

    public long? EnquiryId { get; set; }

    public long? RateCalcId { get; set; }

    public int CompanyId { get; set; }

    public int? LocationId { get; set; }

    public int? PartyId { get; set; }

    public int? JobTypeId { get; set; }

    public long? JobCategoryId { get; set; }

    public string? ProductName { get; set; }

    public string? ProductDescription { get; set; }

    public int Quantity { get; set; }

    public int? TotalPages { get; set; }

    public DateOnly? DeliveryDate { get; set; }

    public string? Priority { get; set; }

    public decimal? EstimatedCost { get; set; }

    public decimal? ActualCost { get; set; }

    public decimal? QuotedAmount { get; set; }

    public decimal? GrossAmount { get; set; }

    public decimal? DiscountAmount { get; set; }

    public decimal? TaxableAmount { get; set; }

    public decimal? TaxAmount { get; set; }

    public decimal? NetAmount { get; set; }

    public string? SpecificationsJson { get; set; }

    public string? StatusCode { get; set; }

    public int? CurrentProcessId { get; set; }

    public string? CurrentStage { get; set; }

    public int? ProgressPercent { get; set; }

    public int? AiPriorityScore { get; set; }

    public DateOnly? AiEstimatedCompletion { get; set; }

    public string? AiBottleneckJson { get; set; }

    public long? AssignedTo { get; set; }

    public long CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public DateTime? CompletedOn { get; set; }

    public DateTime? ClosedOn { get; set; }

    public long? ClosedBy { get; set; }

    public virtual MstUser? AssignedToNavigation { get; set; }

    public virtual MstCompany Company { get; set; } = null!;

    public virtual MstUser CreatedByNavigation { get; set; } = null!;

    public virtual TrnEnquiry? Enquiry { get; set; }

    public virtual ICollection<HybEmployeeAttendance> HybEmployeeAttendances { get; set; } = new List<HybEmployeeAttendance>();

    public virtual ICollection<HybJobRateCalculator> HybJobRateCalculators { get; set; } = new List<HybJobRateCalculator>();

    public virtual MstJobCategory? JobCategory { get; set; }

    public virtual MstJobType? JobType { get; set; }

    public virtual MstParty? Party { get; set; }

    public virtual TrnQuotation? Quotation { get; set; }

    public virtual HybJobRateCalculator? RateCalc { get; set; }

    public virtual ICollection<TrnChallanTimeline> TrnChallanTimelines { get; set; } = new List<TrnChallanTimeline>();

    public virtual ICollection<TrnChallan> TrnChallans { get; set; } = new List<TrnChallan>();

    public virtual ICollection<TrnJobItem> TrnJobItems { get; set; } = new List<TrnJobItem>();

    public virtual ICollection<TrnJobMachineAllocation> TrnJobMachineAllocations { get; set; } = new List<TrnJobMachineAllocation>();

    public virtual ICollection<TrnJobOutsource> TrnJobOutsources { get; set; } = new List<TrnJobOutsource>();

    public virtual ICollection<TrnJobTimeline> TrnJobTimelines { get; set; } = new List<TrnJobTimeline>();

    public virtual ICollection<TrnOutsourceTimeline> TrnOutsourceTimelines { get; set; } = new List<TrnOutsourceTimeline>();

    public virtual ICollection<TrnSalesInvoiceItem> TrnSalesInvoiceItems { get; set; } = new List<TrnSalesInvoiceItem>();

    public virtual ICollection<TrnSalesInvoice> TrnSalesInvoices { get; set; } = new List<TrnSalesInvoice>();

    public virtual ICollection<TrnWorkspaceTaskItem> TrnWorkspaceTaskItems { get; set; } = new List<TrnWorkspaceTaskItem>();
}
