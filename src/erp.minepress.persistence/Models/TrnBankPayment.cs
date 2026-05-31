using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Bank payment voucher for money paid from company bank account to supplier/vendor. Supports cheque, NEFT, RTGS, UPI.
/// </summary>
public partial class TrnBankPayment
{
    public long BankPaymentId { get; set; }

    public string PaymentNo { get; set; } = null!;

    public DateOnly PaymentDate { get; set; }

    public int CompanyId { get; set; }

    public int? LocationId { get; set; }

    public int? FinYearId { get; set; }

    public int BankAccountId { get; set; }

    public int? PartyId { get; set; }

    public string? PaidTo { get; set; }

    public string PaymentMode { get; set; } = null!;

    public string? ChequeNo { get; set; }

    public DateOnly? ChequeDate { get; set; }

    public string? TransactionRefNo { get; set; }

    public decimal Amount { get; set; }

    public decimal? TdsAmount { get; set; }

    public decimal? NetAmount { get; set; }

    public string? Narration { get; set; }

    public long? AccountHeadId { get; set; }

    public bool? IsAdvance { get; set; }

    public string Status { get; set; } = null!;

    public bool? IsReconciled { get; set; }

    public DateOnly? ReconciledOn { get; set; }

    public bool? IsCancelled { get; set; }

    public long? CancelledBy { get; set; }

    public DateTime? CancelledOn { get; set; }

    public string? CancelReason { get; set; }

    public bool? IsPostedToGl { get; set; }

    public DateTime? GlPostedOn { get; set; }

    public long? GlPostedBy { get; set; }

    public long CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual MstBankAccount BankAccount { get; set; } = null!;

    public virtual MstCompany Company { get; set; } = null!;

    public virtual ICollection<TrnBankPaymentAllocation> TrnBankPaymentAllocations { get; set; } = new List<TrnBankPaymentAllocation>();
}
