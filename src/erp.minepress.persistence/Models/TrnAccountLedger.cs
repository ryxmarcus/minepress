using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// General ledger — one row per journal line per account. Powers Ledger report, Trial Balance, P&amp;L, Balance Sheet. Updated on journal posting.
/// </summary>
public partial class TrnAccountLedger
{
    public long LedgerEntryId { get; set; }

    public int CompanyId { get; set; }

    public long AccountId { get; set; }

    public int? PartyId { get; set; }

    public int? FinYearId { get; set; }

    public DateOnly PostingDate { get; set; }

    public long JournalId { get; set; }

    public long JournalLineId { get; set; }

    public string? VoucherType { get; set; }

    public string? VoucherNo { get; set; }

    public string? ReferenceType { get; set; }

    public long? ReferenceId { get; set; }

    public string? ReferenceNo { get; set; }

    public string? Narration { get; set; }

    public decimal? DebitAmount { get; set; }

    public decimal? CreditAmount { get; set; }

    public decimal? RunningBalance { get; set; }

    public string? BalanceType { get; set; }

    public int? CurrencyId { get; set; }

    public bool? IsOpeningEntry { get; set; }

    public int? CostCenterId { get; set; }

    public DateTime? CreatedOn { get; set; }

    public virtual MstAccountHead Account { get; set; } = null!;

    public virtual MstCompany Company { get; set; } = null!;

    public virtual MstCurrency? Currency { get; set; }

    public virtual MstFinancialYear? FinYear { get; set; }

    public virtual MstParty? Party { get; set; }
}
