using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Company bank accounts used for bank receipt, bank payment and bank reconciliation.
/// </summary>
public partial class MstBankAccount
{
    public int BankAccountId { get; set; }

    public string AccountCode { get; set; } = null!;

    public string AccountName { get; set; } = null!;

    public int CompanyId { get; set; }

    public int? LocationId { get; set; }

    public string AccountNo { get; set; } = null!;

    public string BankName { get; set; } = null!;

    public string? BranchName { get; set; }

    public string? IfscCode { get; set; }

    public string? MicrCode { get; set; }

    public string? SwiftCode { get; set; }

    public string? AccountType { get; set; }

    public int? CurrencyId { get; set; }

    public decimal? OpeningBalance { get; set; }

    public decimal? CurrentBalance { get; set; }

    public DateOnly? LastReconciledOn { get; set; }

    public decimal? LastReconciledBalance { get; set; }

    public long? AccountHeadId { get; set; }

    public bool? IsDefault { get; set; }

    public bool? IsActive { get; set; }

    public string? Remarks { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual MstAccountHead? AccountHead { get; set; }

    public virtual MstCompany Company { get; set; } = null!;

    public virtual MstCurrency? Currency { get; set; }

    public virtual MstLocation? Location { get; set; }

    public virtual ICollection<TrnBankPayment> TrnBankPayments { get; set; } = new List<TrnBankPayment>();

    public virtual ICollection<TrnBankReceipt> TrnBankReceipts { get; set; } = new List<TrnBankReceipt>();

    public virtual ICollection<TrnBankReconciliation> TrnBankReconciliations { get; set; } = new List<TrnBankReconciliation>();

    public virtual ICollection<TrnExpenseVoucher> TrnExpenseVouchers { get; set; } = new List<TrnExpenseVoucher>();

    public virtual ICollection<TrnPayment> TrnPayments { get; set; } = new List<TrnPayment>();

    public virtual ICollection<TrnReceipt> TrnReceipts { get; set; } = new List<TrnReceipt>();
}
