using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Lookup for tax/accounting flow direction. id=1 Output (Payable), id=2 Input (ITC). Referenced by trn_tax_ledger.direction.
/// </summary>
public partial class MstDirection
{
    /// <summary>
    /// Primary key. 1=Output Tax, 2=Input Tax — used directly as FK value in trn_tax_ledger.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Human-readable direction label.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// TRUE = active.
    /// </summary>
    public bool? IsActive { get; set; }

    public virtual ICollection<TrnTaxLedger> TrnTaxLedgers { get; set; } = new List<TrnTaxLedger>();
}
