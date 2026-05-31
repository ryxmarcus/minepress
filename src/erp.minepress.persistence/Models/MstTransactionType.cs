using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Lookup for ERP transaction types. Referenced by trn_tax_ledger.transaction_type_id.
/// </summary>
public partial class MstTransactionType
{
    /// <summary>
    /// Primary key, auto-generated.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Unique name of the transaction type.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// TRUE = active and selectable in transactions.
    /// </summary>
    public bool? IsActive { get; set; }

    public virtual ICollection<TrnTaxLedger> TrnTaxLedgers { get; set; } = new List<TrnTaxLedger>();
}
