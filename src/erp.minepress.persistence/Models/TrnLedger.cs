using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class TrnLedger
{
    public long LedgerId { get; set; }

    public DateOnly TransactionDate { get; set; }

    public long AccountHeadId { get; set; }

    public int? PartyId { get; set; }

    public decimal? DebitAmount { get; set; }

    public decimal? CreditAmount { get; set; }

    public int? VoucherTypeId { get; set; }

    public long? VoucherId { get; set; }

    public string? VoucherNo { get; set; }

    public string? ReferenceNo { get; set; }

    public DateOnly? ReferenceDate { get; set; }

    public string? Remarks { get; set; }

    public DateTime? CreatedOn { get; set; }

    public virtual MstAccountHead AccountHead { get; set; } = null!;

    public virtual MstParty? Party { get; set; }

    public virtual MstVoucherType? VoucherType { get; set; }
}
