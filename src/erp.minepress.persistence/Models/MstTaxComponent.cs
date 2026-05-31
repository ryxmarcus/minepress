using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Tax sub-components: CGST, SGST, IGST, CESS, TDS, TCS, etc.
/// </summary>
public partial class MstTaxComponent
{
    public int TaxComponentId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsPercentage { get; set; }

    public bool? IsRecoverable { get; set; }

    public string? ApplicableOn { get; set; }

    public bool? IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public virtual ICollection<MstTaxCategoryComponent> MstTaxCategoryComponents { get; set; } = new List<MstTaxCategoryComponent>();

    public virtual ICollection<TrnTaxLedger> TrnTaxLedgers { get; set; } = new List<TrnTaxLedger>();
}
