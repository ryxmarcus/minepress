using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class TrnOutsourceTimeline
{
    public long TimelineId { get; set; }

    public long OutsourceId { get; set; }

    public long? JobId { get; set; }

    public long? ChallanId { get; set; }

    public long? QuotationId { get; set; }

    public long? EnquiryId { get; set; }

    public long? VendorId { get; set; }

    public string? VendorName { get; set; }

    public string EventType { get; set; } = null!;

    public string? EventCode { get; set; }

    public string? EventTitle { get; set; }

    public string? EventDescription { get; set; }

    public string? Remarks { get; set; }

    public string? OldStatus { get; set; }

    public string? NewStatus { get; set; }

    public decimal? OldQuantity { get; set; }

    public decimal? NewQuantity { get; set; }

    public decimal? OldAmount { get; set; }

    public decimal? NewAmount { get; set; }

    public string? ProcessCode { get; set; }

    public string? ProcessName { get; set; }

    public string? MovementType { get; set; }

    public DateTime? ExpectedReturnDate { get; set; }

    public DateTime? ActualReturnDate { get; set; }

    public string? DelayReason { get; set; }

    public long? AssignedToUserId { get; set; }

    public string? CommunicationMode { get; set; }

    public string? CommunicationReference { get; set; }

    public string? AttachmentUrl { get; set; }

    public long CreatedBy { get; set; }

    public DateTime CreatedOn { get; set; }

    public long? UpdatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public bool? IsActive { get; set; }

    public virtual TrnChallan? Challan { get; set; }

    public virtual TrnEnquiry? Enquiry { get; set; }

    public virtual TrnJob? Job { get; set; }

    public virtual TrnJobOutsource Outsource { get; set; } = null!;

    public virtual TrnQuotation? Quotation { get; set; }
}
