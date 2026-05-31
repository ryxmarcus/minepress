using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Accounts Payable outstanding tracker. One row per purchase invoice/debit note. Updated on payment/allocation. Powers AP aging report, vendor statement, payment scheduling.
/// </summary>
public partial class TrnApOutstanding
{
    public long ApId { get; set; }

    public int CompanyId { get; set; }

    public int PartyId { get; set; }

    public int? SupplierId { get; set; }

    public int? FinYearId { get; set; }

    /// <summary>
    /// PURCHASE_INVOICE, DEBIT_NOTE, CREDIT_NOTE
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

    public decimal? TdsAmount { get; set; }

    public decimal? WriteOffAmount { get; set; }

    public decimal? OutstandingAmount { get; set; }

    public int? OverdueDays { get; set; }

    /// <summary>
    /// CURRENT, 1-30, 31-60, 61-90, 91-120, 120+
    /// </summary>
    public string? AgingBucket { get; set; }

    public bool? IsFullySettled { get; set; }

    public DateOnly? LastPaymentDate { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual MstCompany Company { get; set; } = null!;

    public virtual MstParty Party { get; set; } = null!;
}
