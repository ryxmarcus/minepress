using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Advance received from customers or paid to suppliers. Tracks adjustment status against invoices.
/// </summary>
public partial class TrnAdvanceLedger
{
    public long AdvanceId { get; set; }

    public int CompanyId { get; set; }

    public int PartyId { get; set; }

    public string PartyType { get; set; } = null!;

    public int? FinYearId { get; set; }

    public DateOnly AdvanceDate { get; set; }

    public string VoucherType { get; set; } = null!;

    public long? ReceiptVoucherId { get; set; }

    public long? PaymentVoucherId { get; set; }

    public long? BankReceiptId { get; set; }

    public long? BankPaymentId { get; set; }

    public decimal AdvanceAmount { get; set; }

    public decimal? AdjustedAmount { get; set; }

    public decimal? UnadjustedAmount { get; set; }

    public string? Narration { get; set; }

    public bool? IsFullyAdjusted { get; set; }

    public DateTime? CreatedOn { get; set; }

    public virtual MstCompany Company { get; set; } = null!;

    public virtual MstParty Party { get; set; } = null!;
}
