using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Expense voucher for direct business expenses (rent, utilities, travel, repairs, etc.). Supports multi-line with GST and TDS. Approval workflow.
/// </summary>
public partial class TrnExpenseVoucher
{
    public long ExpenseVoucherId { get; set; }

    public string VoucherNo { get; set; } = null!;

    public DateOnly VoucherDate { get; set; }

    public int CompanyId { get; set; }

    public int? LocationId { get; set; }

    public int? FinYearId { get; set; }

    /// <summary>
    /// OFFICE, TRAVEL, UTILITIES, REPAIRS, SALARY, RENT, PRINTING, TRANSPORT, MISC
    /// </summary>
    public string? ExpenseCategory { get; set; }

    public int? PartyId { get; set; }

    public long? EmployeeId { get; set; }

    public string PaymentMode { get; set; } = null!;

    public int? BankAccountId { get; set; }

    public string? ChequeNo { get; set; }

    public DateOnly? ChequeDate { get; set; }

    public string? ReferenceNo { get; set; }

    public DateOnly? ReferenceDate { get; set; }

    public decimal? SubtotalAmount { get; set; }

    public decimal? TaxableAmount { get; set; }

    public decimal? CgstAmount { get; set; }

    public decimal? SgstAmount { get; set; }

    public decimal? IgstAmount { get; set; }

    public decimal? CessAmount { get; set; }

    public decimal? TotalTaxAmount { get; set; }

    public decimal? TdsAmount { get; set; }

    public decimal? GrandTotal { get; set; }

    public string? Narration { get; set; }

    public string Status { get; set; } = null!;

    public bool? IsApproved { get; set; }

    public long? ApprovedBy { get; set; }

    public DateTime? ApprovedOn { get; set; }

    public bool? IsCancelled { get; set; }

    public long? CancelledBy { get; set; }

    public DateTime? CancelledOn { get; set; }

    public string? CancelReason { get; set; }

    public bool? IsPostedToGl { get; set; }

    public DateTime? GlPostedOn { get; set; }

    public long? GlPostedBy { get; set; }

    public string? AttachmentsJson { get; set; }

    public long CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual MstBankAccount? BankAccount { get; set; }

    public virtual MstCompany Company { get; set; } = null!;

    public virtual MstEmployee? Employee { get; set; }

    public virtual ICollection<TrnExpenseVoucherItem> TrnExpenseVoucherItems { get; set; } = new List<TrnExpenseVoucherItem>();
}
