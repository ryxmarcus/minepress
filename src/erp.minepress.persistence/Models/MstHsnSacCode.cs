using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Master table for HSN (goods) and SAC (services) codes for GST compliance. Stores tax rates and classification details.
/// </summary>
public partial class MstHsnSacCode
{
    /// <summary>
    /// Primary key, auto-generated.
    /// </summary>
    public short Id { get; set; }

    /// <summary>
    /// HSN or SAC code (unique identifier).
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// Type of code: HSN for goods, SAC for services.
    /// </summary>
    public string CodeType { get; set; } = null!;

    /// <summary>
    /// Detailed description of the goods/service.
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// Parent HSN/SAC code for hierarchical classification.
    /// </summary>
    public string? ParentCode { get; set; }

    /// <summary>
    /// Classification level: 1=Chapter, 2=Heading, 3=Sub-heading, 4=Tariff item.
    /// </summary>
    public short? LevelNo { get; set; }

    /// <summary>
    /// FK to mst_tax_category for default tax slab.
    /// </summary>
    public int? TaxCategoryId { get; set; }

    /// <summary>
    /// Default total GST rate percentage (e.g., 18.000 for 18% GST).
    /// </summary>
    public decimal? DefaultGstRate { get; set; }

    public bool? IsNilRated { get; set; }

    public bool? IsExempt { get; set; }

    /// <summary>
    /// Central GST rate (for intra-state transactions).
    /// </summary>
    public decimal? CgstRate { get; set; }

    /// <summary>
    /// State GST rate (for intra-state transactions).
    /// </summary>
    public decimal? SgstRate { get; set; }

    /// <summary>
    /// Integrated GST rate (for inter-state transactions).
    /// </summary>
    public decimal? IgstRate { get; set; }

    /// <summary>
    /// Additional cess rate if applicable.
    /// </summary>
    public decimal? CessRate { get; set; }

    public DateOnly EffectiveFrom { get; set; }

    public DateOnly? EffectiveTo { get; set; }

    /// <summary>
    /// Business category (Paper Products, Printing Services, Inks, etc.).
    /// </summary>
    public string? Category { get; set; }

    public string? IndustryType { get; set; }

    public string? UnitOfMeasure { get; set; }

    public string? Remarks { get; set; }

    public bool? IsActive { get; set; }

    /// <summary>
    /// Flag for frequently used codes in printing press business.
    /// </summary>
    public bool? IsCommonlyUsed { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual MstTaxCategory? TaxCategory { get; set; }
}
