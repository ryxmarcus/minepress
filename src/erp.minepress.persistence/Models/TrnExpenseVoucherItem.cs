using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Expense voucher line items. Each line debits a different expense account head with optional GST breakup.
/// </summary>
public partial class TrnExpenseVoucherItem
{
    public long ExpenseItemId { get; set; }

    public long ExpenseVoucherId { get; set; }

    public int ItemSequence { get; set; }

    public long AccountHeadId { get; set; }

    public string Description { get; set; } = null!;

    public string? HsnSacCode { get; set; }

    public decimal? Amount { get; set; }

    public int? TaxCategoryId { get; set; }

    public decimal? CgstPercent { get; set; }

    public decimal? CgstAmount { get; set; }

    public decimal? SgstPercent { get; set; }

    public decimal? SgstAmount { get; set; }

    public decimal? IgstPercent { get; set; }

    public decimal? IgstAmount { get; set; }

    public decimal? CessPercent { get; set; }

    public decimal? CessAmount { get; set; }

    public decimal? TotalTaxAmount { get; set; }

    public decimal? LineTotal { get; set; }

    public int? CostCenterId { get; set; }

    public long? JobId { get; set; }

    public string? Remarks { get; set; }

    public virtual MstAccountHead AccountHead { get; set; } = null!;

    public virtual TrnExpenseVoucher ExpenseVoucher { get; set; } = null!;
}
