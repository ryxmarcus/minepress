using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Top-level tax classification: GST, VAT, TDS, TCS, Customs, etc.
/// </summary>
public partial class MstTaxType
{
    public int TaxTypeId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsPercentage { get; set; }

    public bool? IsRecoverable { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedOn { get; set; }

    public virtual ICollection<MstPartyTax> MstPartyTaxes { get; set; } = new List<MstPartyTax>();

    public virtual ICollection<MstTaxRate> MstTaxRates { get; set; } = new List<MstTaxRate>();
}
