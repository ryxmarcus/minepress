using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Accounts Receivable outstanding tracker. One row per sales invoice/credit note. Updated on receipt/allocation. Powers AR aging report, customer statement, collection follow-up.
/// </summary>
public partial class TrnArOutstanding
{
    public long ArId { get; set; }

    public int CompanyId { get; set; }

    public int PartyId { get; set; }

    public int? FinYearId { get; set; }

    /// <summary>
    /// SALES_INVOICE, CREDIT_NOTE, DEBIT_NOTE
    /// </summary>
    public string DocumentType { get; set; } = null!;

    public long DocumentId { get; set; }

    public string DocumentNo { get; set; } = null!;

    public DateOnly DocumentDate { get; set; }

    public DateOnly? DueDate { get; set; }

    public int? CurrencyId { get; set; }

    public decimal OriginalAmount { get; set; }

    public decimal? PaidAmount { get; set; }

    public decimal? AdjustedAmount { get; set; }

    public decimal? WriteOffAmount { get; set; }

    public decimal? OutstandingAmount { get; set; }

    public int? OverdueDays { get; set; }

    /// <summary>
    /// CURRENT, 1-30, 31-60, 61-90, 91-120, 120+
    /// </summary>
    public string? AgingBucket { get; set; }

    public bool? IsFullySettled { get; set; }

    public DateOnly? LastPaymentDate { get; set; }

    public DateOnly? LastReminderDate { get; set; }

    public int? ReminderCount { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual MstCompany Company { get; set; } = null!;

    public virtual MstParty Party { get; set; } = null!;
}
