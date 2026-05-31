using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class HrLeaveRequest
{
    public long LeaveId { get; set; }

    public string LeaveNo { get; set; } = null!;

    public long EmployeeId { get; set; }

    public int LeaveTypeId { get; set; }

    public DateOnly FromDate { get; set; }

    public DateOnly ToDate { get; set; }

    public decimal TotalDays { get; set; }

    public bool? HalfDay { get; set; }

    public string? HalfDaySession { get; set; }

    public string? Reason { get; set; }

    public string? ContactDuringLeave { get; set; }

    public string? DocumentPath { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? AppliedOn { get; set; }

    public long? ApprovedBy { get; set; }

    public DateTime? ApprovedOn { get; set; }

    public string? RejectionReason { get; set; }

    public long? CancelledBy { get; set; }

    public DateTime? CancelledOn { get; set; }

    public string? Remarks { get; set; }

    public long CreatedBy { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }
}
