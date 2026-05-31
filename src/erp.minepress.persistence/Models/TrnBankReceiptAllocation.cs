using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Allocation of bank receipt against sales invoices or advance adjustments.
/// </summary>
public partial class TrnBankReceiptAllocation
{
    public long AllocationId { get; set; }

    public long BankReceiptId { get; set; }

    /// <summary>
    /// SALES_INVOICE, CREDIT_NOTE, ADVANCE, OTHER
    /// </summary>
    public string AllocationAgainst { get; set; } = null!;

    public long RefId { get; set; }

    public string? RefNo { get; set; }

    public DateOnly? RefDate { get; set; }

    public decimal AllocatedAmount { get; set; }

    public DateTime? CreatedOn { get; set; }

    public virtual TrnBankReceipt BankReceipt { get; set; } = null!;
}
