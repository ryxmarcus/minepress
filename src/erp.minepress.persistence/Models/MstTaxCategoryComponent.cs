using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Rate split per component for a tax category (CGST/SGST/IGST rates).
/// </summary>
public partial class MstTaxCategoryComponent
{
    public int Id { get; set; }

    public int TaxCategoryId { get; set; }

    public int TaxComponentId { get; set; }

    public decimal RatePercent { get; set; }

    public DateOnly EffectiveFrom { get; set; }

    public DateOnly? EffectiveTo { get; set; }

    public bool? IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public virtual MstTaxCategory TaxCategory { get; set; } = null!;

    public virtual MstTaxComponent TaxComponent { get; set; } = null!;
}
