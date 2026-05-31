using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class HrOvertime
{
    public long OtId { get; set; }

    public string OtNo { get; set; } = null!;

    public long EmployeeId { get; set; }

    public DateOnly OtDate { get; set; }

    public TimeOnly? FromTime { get; set; }

    public TimeOnly? ToTime { get; set; }

    public decimal? OtHours { get; set; }

    public string? OtReason { get; set; }

    public decimal? OtRatePerHour { get; set; }

    public decimal? OtAmount { get; set; }

    public string Status { get; set; } = null!;

    public long? ApprovedBy { get; set; }

    public DateTime? ApprovedOn { get; set; }

    public string? RejectionReason { get; set; }

    public long CreatedBy { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }
}
