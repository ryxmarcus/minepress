using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class TrnStoreTimeline
{
    public long TimelineId { get; set; }

    public string Module { get; set; } = null!;

    public long ReferenceId { get; set; }

    public string EventType { get; set; } = null!;

    public string? EventCode { get; set; }

    public string EventTitle { get; set; } = null!;

    public string? EventDescription { get; set; }

    public string? OldStatus { get; set; }

    public string? NewStatus { get; set; }

    public string? Remarks { get; set; }

    public string? AttachmentUrl { get; set; }

    public long? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public bool? IsActive { get; set; }
}
