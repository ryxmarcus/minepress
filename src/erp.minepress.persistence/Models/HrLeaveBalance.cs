using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class HrLeaveBalance
{
    public long BalanceId { get; set; }

    public long EmployeeId { get; set; }

    public int LeaveTypeId { get; set; }

    public string FinYear { get; set; } = null!;

    public decimal? OpeningBalance { get; set; }

    public decimal? Accrued { get; set; }

    public decimal? Availed { get; set; }

    public decimal? Encashed { get; set; }

    public decimal? Lapsed { get; set; }

    public decimal? CarryForward { get; set; }

    public decimal? ClosingBalance { get; set; }
}
