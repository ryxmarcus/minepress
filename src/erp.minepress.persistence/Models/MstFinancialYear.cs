using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Financial year periods per company. Used for period-wise reporting, GST returns, and ledger closing.
/// </summary>
public partial class MstFinancialYear
{
    public int FinYearId { get; set; }

    public string FinYearCode { get; set; } = null!;

    public int CompanyId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public bool? IsCurrent { get; set; }

    public bool? IsClosed { get; set; }

    public string? ClosedBy { get; set; }

    public DateTime? ClosedOn { get; set; }

    public bool? OpeningDone { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public virtual MstCompany Company { get; set; } = null!;

    public virtual ICollection<TrnAccountLedger> TrnAccountLedgers { get; set; } = new List<TrnAccountLedger>();

    public virtual ICollection<TrnPurchaseInvoice> TrnPurchaseInvoices { get; set; } = new List<TrnPurchaseInvoice>();

    public virtual ICollection<TrnSalesInvoice> TrnSalesInvoices { get; set; } = new List<TrnSalesInvoice>();
}
