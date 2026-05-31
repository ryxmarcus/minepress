using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Journal voucher debit/credit lines. Each line posts to one account head. Sum of debits must equal sum of credits within a journal.
/// </summary>
public partial class TrnJournalVoucherLine
{
    public long JournalLineId { get; set; }

    public long JournalId { get; set; }

    public int LineNo { get; set; }

    public long AccountHeadId { get; set; }

    public int? PartyId { get; set; }

    public decimal? DebitAmount { get; set; }

    public decimal? CreditAmount { get; set; }

    public string? Narration { get; set; }

    public int? CostCenterId { get; set; }

    public string? ReferenceType { get; set; }

    public long? ReferenceId { get; set; }

    public string? ReferenceNo { get; set; }

    public virtual MstAccountHead AccountHead { get; set; } = null!;

    public virtual TrnJournalVoucher Journal { get; set; } = null!;
}
