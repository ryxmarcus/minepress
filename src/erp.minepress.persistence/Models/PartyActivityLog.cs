using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class PartyActivityLog
{
    public long ActivityId { get; set; }

    public int PartyId { get; set; }

    public string ActivityType { get; set; } = null!;

    public string ActivityCode { get; set; } = null!;

    public string? ReferenceTable { get; set; }

    public long? ReferenceId { get; set; }

    public string? DocumentNo { get; set; }

    public DateOnly? DocumentDate { get; set; }

    public string? ActivityTitle { get; set; }

    public string? ActivityDescription { get; set; }

    public string? Status { get; set; }

    public string? ApprovalStatus { get; set; }

    public decimal? Amount { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public bool? IsActive { get; set; }

    public virtual MstParty Party { get; set; } = null!;
}
