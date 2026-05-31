using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class TrnEnquiryTimeline
{
    public long TimelineId { get; set; }

    public long EnquiryId { get; set; }

    public string EventType { get; set; } = null!;

    public string? EventCode { get; set; }

    public string? EventTitle { get; set; }

    public string? EventDescription { get; set; }

    public string? Remarks { get; set; }

    public string? OldStatus { get; set; }

    public string? NewStatus { get; set; }

    public long? AssignedToUserId { get; set; }

    public DateTime? FollowupDate { get; set; }

    public string? FollowupMode { get; set; }

    public string? AttachmentUrl { get; set; }

    public long CreatedBy { get; set; }

    public DateTime CreatedOn { get; set; }

    public long? UpdatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public bool? IsActive { get; set; }

    public virtual TrnEnquiry Enquiry { get; set; } = null!;
}
