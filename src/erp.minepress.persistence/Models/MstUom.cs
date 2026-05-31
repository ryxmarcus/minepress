using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstUom
{
    public int UomId { get; set; }

    public string UomCode { get; set; } = null!;

    public string UomName { get; set; } = null!;

    public int? UomTypeId { get; set; }

    public int? DecimalPlaces { get; set; }

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual MstUomType? UomType { get; set; }
}
