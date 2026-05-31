using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Contra voucher for fund transfers: Cash→Bank, Bank→Cash, Bank→Bank. No party involved.
/// </summary>
public partial class TrnContraVoucher
{
    public long ContraId { get; set; }

    public string ContraNo { get; set; } = null!;

    public DateOnly ContraDate { get; set; }

    public int CompanyId { get; set; }

    public int? LocationId { get; set; }

    public int? FinYearId { get; set; }

    /// <summary>
    /// CASH or BANK
    /// </summary>
    public string TransferFromType { get; set; } = null!;

    /// <summary>
    /// FK to mst_bank_account.bank_account_id (for BANK) or 0 (for CASH)
    /// </summary>
    public int TransferFromId { get; set; }

    /// <summary>
    /// CASH or BANK
    /// </summary>
    public string TransferToType { get; set; } = null!;

    /// <summary>
    /// FK to mst_bank_account.bank_account_id (for BANK) or 0 (for CASH)
    /// </summary>
    public int TransferToId { get; set; }

    public decimal Amount { get; set; }

    public string? ReferenceNo { get; set; }

    public string? Narration { get; set; }

    public string Status { get; set; } = null!;

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

    public virtual MstCompany Company { get; set; } = null!;
}
