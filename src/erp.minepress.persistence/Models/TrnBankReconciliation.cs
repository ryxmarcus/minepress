using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Bank reconciliation header. Matches book entries with bank statement for a given bank account and statement date.
/// </summary>
public partial class TrnBankReconciliation
{
    public long ReconciliationId { get; set; }

    public string ReconciliationNo { get; set; } = null!;

    public int CompanyId { get; set; }

    public int BankAccountId { get; set; }

    public int? FinYearId { get; set; }

    public DateOnly StatementDate { get; set; }

    public decimal StatementBalance { get; set; }

    public decimal? BookBalance { get; set; }

    public decimal? ReconciledBalance { get; set; }

    public decimal? DifferenceAmount { get; set; }

    public int? TotalItems { get; set; }

    public int? ReconciledItems { get; set; }

    public int? PendingItems { get; set; }

    public string Status { get; set; } = null!;

    public long? CompletedBy { get; set; }

    public DateTime? CompletedOn { get; set; }

    public string? Remarks { get; set; }

    public long CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual MstBankAccount BankAccount { get; set; } = null!;

    public virtual MstCompany Company { get; set; } = null!;

    public virtual ICollection<TrnBankReconciliationItem> TrnBankReconciliationItems { get; set; } = new List<TrnBankReconciliationItem>();
}
