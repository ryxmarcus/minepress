using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Allocation of bank payment against purchase invoices or advance adjustments.
/// </summary>
public partial class TrnBankPaymentAllocation
{
    public long AllocationId { get; set; }

    public long BankPaymentId { get; set; }

    /// <summary>
    /// PURCHASE_INVOICE, DEBIT_NOTE, ADVANCE, EXPENSE, OTHER
    /// </summary>
    public string AllocationAgainst { get; set; } = null!;

    public long RefId { get; set; }

    public string? RefNo { get; set; }

    public DateOnly? RefDate { get; set; }

    public decimal AllocatedAmount { get; set; }

    public DateTime? CreatedOn { get; set; }

    public virtual TrnBankPayment BankPayment { get; set; } = null!;
}
