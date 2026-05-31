using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class HrLoanRepayment
{
    public long RepaymentId { get; set; }

    public long LoanId { get; set; }

    public int InstallmentNo { get; set; }

    public DateOnly DueDate { get; set; }

    public decimal? DueAmount { get; set; }

    public decimal? PrincipalAmount { get; set; }

    public decimal? InterestAmount { get; set; }

    public decimal? PaidAmount { get; set; }

    public DateOnly? PaidDate { get; set; }

    public bool? IsPaid { get; set; }

    public long? PayrollRunId { get; set; }
}
