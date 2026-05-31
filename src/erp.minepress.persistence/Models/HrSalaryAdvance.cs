using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class HrSalaryAdvance
{
    public long AdvanceId { get; set; }

    public string AdvanceNo { get; set; } = null!;

    public long EmployeeId { get; set; }

    public DateOnly AdvanceDate { get; set; }

    public decimal AdvanceAmount { get; set; }

    public string? Reason { get; set; }

    public int? RepaymentMonths { get; set; }

    public decimal? MonthlyDeduction { get; set; }

    public decimal? RecoveredAmount { get; set; }

    public decimal? BalanceAmount { get; set; }

    public string Status { get; set; } = null!;

    public long? ApprovedBy { get; set; }

    public DateTime? ApprovedOn { get; set; }

    public string? RejectionReason { get; set; }

    public long CreatedBy { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }
}
