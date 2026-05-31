using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstAccountHead
{
    public long AccountHeadId { get; set; }

    public string? AccountCode { get; set; }

    public string AccountName { get; set; } = null!;

    public string AccountType { get; set; } = null!;

    public long? ParentAccountId { get; set; }

    public bool? IsPartyAccount { get; set; }

    public decimal? OpeningBalance { get; set; }

    public string? OpeningType { get; set; }

    public bool? IsGroup { get; set; }

    public int? LevelNo { get; set; }

    public int? SortOrder { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedOn { get; set; }

    public virtual ICollection<MstAccountHead> InverseParentAccount { get; set; } = new List<MstAccountHead>();

    public virtual ICollection<MstBankAccount> MstBankAccounts { get; set; } = new List<MstBankAccount>();

    public virtual ICollection<MstExpenseCategory> MstExpenseCategories { get; set; } = new List<MstExpenseCategory>();

    public virtual MstAccountHead? ParentAccount { get; set; }

    public virtual ICollection<TrnAccountLedger> TrnAccountLedgers { get; set; } = new List<TrnAccountLedger>();

    public virtual ICollection<TrnExpenseVoucherItem> TrnExpenseVoucherItems { get; set; } = new List<TrnExpenseVoucherItem>();

    public virtual ICollection<TrnJournalVoucherLine> TrnJournalVoucherLines { get; set; } = new List<TrnJournalVoucherLine>();

    public virtual ICollection<TrnLedger> TrnLedgers { get; set; } = new List<TrnLedger>();

    public virtual ICollection<TrnPurchaseInvoiceItem> TrnPurchaseInvoiceItems { get; set; } = new List<TrnPurchaseInvoiceItem>();

    public virtual ICollection<TrnSalesInvoiceItem> TrnSalesInvoiceItems { get; set; } = new List<TrnSalesInvoiceItem>();
}
