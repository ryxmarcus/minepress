using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class TrnStockLedger
{
    public long LedgerId { get; set; }

    public DateOnly TransactionDate { get; set; }

    public string TransactionType { get; set; } = null!;

    public string? ReferenceType { get; set; }

    public long? ReferenceId { get; set; }

    public string? ReferenceNo { get; set; }

    public string MaterialCategory { get; set; } = null!;

    public long? MaterialId { get; set; }

    public string? MaterialCode { get; set; }

    public string MaterialName { get; set; } = null!;

    public string? Uom { get; set; }

    public decimal? QuantityIn { get; set; }

    public decimal? QuantityOut { get; set; }

    public decimal? BalanceQuantity { get; set; }

    public decimal? Rate { get; set; }

    public decimal? Amount { get; set; }

    public long? JobId { get; set; }

    public string? JobNo { get; set; }

    public int? LocationId { get; set; }

    public int CompanyId { get; set; }

    public string? Remarks { get; set; }

    public long? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }
}
