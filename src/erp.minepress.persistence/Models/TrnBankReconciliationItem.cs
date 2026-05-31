using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Bank reconciliation line items. Each row maps a book voucher entry to its bank statement clearance date.
/// </summary>
public partial class TrnBankReconciliationItem
{
    public long ReconItemId { get; set; }

    public long ReconciliationId { get; set; }

    public string VoucherType { get; set; } = null!;

    public long VoucherId { get; set; }

    public string? VoucherNo { get; set; }

    public DateOnly? VoucherDate { get; set; }

    public string? ChequeNo { get; set; }

    public decimal? DebitAmount { get; set; }

    public decimal? CreditAmount { get; set; }

    public DateOnly? BankDate { get; set; }

    public bool? IsReconciled { get; set; }

    public DateTime? ReconciledOn { get; set; }

    public string? Remarks { get; set; }

    public virtual TrnBankReconciliation Reconciliation { get; set; } = null!;
}
