using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Effective-dated tax rates per type/category/region/HSN.
/// </summary>
public partial class MstTaxRate
{
    public int TaxRateId { get; set; }

    public int TaxTypeId { get; set; }

    public int? TaxCategoryId { get; set; }

    public int? RegionId { get; set; }

    public string? HsnSacCode { get; set; }

    public decimal? RatePercent { get; set; }

    public DateOnly EffectiveFrom { get; set; }

    public DateOnly? EffectiveTo { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedOn { get; set; }

    public virtual MstTaxRegion? Region { get; set; }

    public virtual MstTaxCategory? TaxCategory { get; set; }

    public virtual MstTaxType TaxType { get; set; } = null!;
}
