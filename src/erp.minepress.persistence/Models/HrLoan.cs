using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class HrLoan
{
    public long LoanId { get; set; }

    public string LoanNo { get; set; } = null!;

    public long EmployeeId { get; set; }

    public string LoanType { get; set; } = null!;

    public DateOnly LoanDate { get; set; }

    public decimal LoanAmount { get; set; }

    public decimal? InterestRate { get; set; }

    public int TenureMonths { get; set; }

    public decimal? EmiAmount { get; set; }

    public decimal? DisbursedAmount { get; set; }

    public decimal? RecoveredAmount { get; set; }

    public decimal? OutstandingAmount { get; set; }

    public string? Reason { get; set; }

    public string Status { get; set; } = null!;

    public long? ApprovedBy { get; set; }

    public DateTime? ApprovedOn { get; set; }

    public DateOnly? DisbursedOn { get; set; }

    public long CreatedBy { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }
}
