using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstCostComponent
{
    public int CostComponentId { get; set; }

    public string ComponentCode { get; set; } = null!;

    public string ComponentName { get; set; } = null!;

    public string ComponentCategory { get; set; } = null!;

    public string? Description { get; set; }

    public string ApplicableLevel { get; set; } = null!;

    public bool? ApplicableToPart { get; set; }

    public bool? ApplicableToProduct { get; set; }

    public string CalculationType { get; set; } = null!;

    public string? BaseUom { get; set; }

    public bool? IsMandatory { get; set; }

    public bool? IsOutsourceAllowed { get; set; }

    public bool? IsTaxable { get; set; }

    public int? TaxCategoryId { get; set; }

    public decimal? DefaultRate { get; set; }

    public decimal? MinRate { get; set; }

    public decimal? MaxRate { get; set; }

    public int? SequenceNo { get; set; }

    public bool? IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual MstTaxCategory? TaxCategory { get; set; }
}
