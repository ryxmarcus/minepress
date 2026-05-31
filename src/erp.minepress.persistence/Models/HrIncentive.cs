using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class HrIncentive
{
    public long IncentiveId { get; set; }

    public string IncentiveNo { get; set; } = null!;

    public long EmployeeId { get; set; }

    public string IncentiveType { get; set; } = null!;

    public string? ReferencePeriod { get; set; }

    public DateOnly IncentiveDate { get; set; }

    public decimal IncentiveAmount { get; set; }

    public string? CalculationBasis { get; set; }

    public string? Remarks { get; set; }

    public string Status { get; set; } = null!;

    public long? ApprovedBy { get; set; }

    public DateTime? ApprovedOn { get; set; }

    public long? PayrollRunId { get; set; }

    public long CreatedBy { get; set; }

    public DateTime CreatedOn { get; set; }
}
