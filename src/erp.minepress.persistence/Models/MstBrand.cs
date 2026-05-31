using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstBrand
{
    public long BrandId { get; set; }

    public string BrandCode { get; set; } = null!;

    public string BrandName { get; set; } = null!;

    public string? ManufacturerName { get; set; }

    public string? Website { get; set; }

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }
}
