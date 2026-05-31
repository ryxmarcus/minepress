using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Tax slabs: GST 5%, 12%, 18%, 28%, EXEMPT, ZERO-RATED, etc.
/// </summary>
public partial class MstTaxCategory
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string TaxType { get; set; } = null!;

    public string? HsnSacCode { get; set; }

    public bool? IsReverseChargeApplicable { get; set; }

    public bool? IsExempt { get; set; }

    public string? TaxRegime { get; set; }

    public DateOnly? ApplicableFrom { get; set; }

    public DateOnly? ApplicableTo { get; set; }

    public int? ParentTaxCategoryId { get; set; }

    public bool? IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual ICollection<MstTaxCategory> InverseParentTaxCategory { get; set; } = new List<MstTaxCategory>();

    public virtual ICollection<MstCompany> MstCompanies { get; set; } = new List<MstCompany>();

    public virtual ICollection<MstCostComponent> MstCostComponents { get; set; } = new List<MstCostComponent>();

    public virtual ICollection<MstHsnSacCode> MstHsnSacCodes { get; set; } = new List<MstHsnSacCode>();

    public virtual ICollection<MstTaxCategoryComponent> MstTaxCategoryComponents { get; set; } = new List<MstTaxCategoryComponent>();

    public virtual ICollection<MstTaxRate> MstTaxRates { get; set; } = new List<MstTaxRate>();

    public virtual MstTaxCategory? ParentTaxCategory { get; set; }

    public virtual ICollection<TrnSalesInvoiceItem> TrnSalesInvoiceItems { get; set; } = new List<TrnSalesInvoiceItem>();
}
