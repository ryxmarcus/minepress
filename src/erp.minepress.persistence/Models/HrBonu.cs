using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class HrBonu
{
    public long BonusId { get; set; }

    public string BonusNo { get; set; } = null!;

    public long EmployeeId { get; set; }

    public string BonusType { get; set; } = null!;

    public string? FinYear { get; set; }

    public DateOnly BonusDate { get; set; }

    public decimal BonusAmount { get; set; }

    public string? Remarks { get; set; }

    public string Status { get; set; } = null!;

    public long? ApprovedBy { get; set; }

    public DateTime? ApprovedOn { get; set; }

    public long? PayrollRunId { get; set; }

    public long CreatedBy { get; set; }

    public DateTime CreatedOn { get; set; }
}
