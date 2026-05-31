using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class HrLeaveType
{
    public int LeaveTypeId { get; set; }

    public string LeaveCode { get; set; } = null!;

    public string LeaveName { get; set; } = null!;

    public string? LeaveCategory { get; set; }

    public int? MaxDaysPerYear { get; set; }

    public int? MaxDaysPerMonth { get; set; }

    public bool? CarryForward { get; set; }

    public int? MaxCarryForward { get; set; }

    public bool? Encashable { get; set; }

    public string? ApplicableGender { get; set; }

    public int? MinServiceMonths { get; set; }

    public bool? RequiresDocs { get; set; }

    public bool? ProRataOnJoin { get; set; }

    public bool IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }
}
