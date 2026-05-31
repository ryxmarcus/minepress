using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Employee expense reimbursement claims (medical, travel, fuel, etc.).
/// </summary>
public partial class HrReimbursement
{
    public long ReimbursementId { get; set; }

    public string ReimbursementNo { get; set; } = null!;

    public long EmployeeId { get; set; }

    public string ReimbursementType { get; set; } = null!;

    public DateOnly ClaimDate { get; set; }

    public decimal ClaimAmount { get; set; }

    public decimal? ApprovedAmount { get; set; }

    public string? Description { get; set; }

    public string? DocumentPath { get; set; }

    public string Status { get; set; } = null!;

    public long? ApprovedBy { get; set; }

    public DateTime? ApprovedOn { get; set; }

    public string? RejectionReason { get; set; }

    public decimal? PaidAmount { get; set; }

    public DateOnly? PaidOn { get; set; }

    public long? PayrollRunId { get; set; }

    public long CreatedBy { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual MstUser? ApprovedByNavigation { get; set; }

    public virtual MstUser CreatedByNavigation { get; set; } = null!;

    public virtual MstEmployee Employee { get; set; } = null!;
}
