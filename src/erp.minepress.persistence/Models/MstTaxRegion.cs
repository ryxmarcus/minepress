using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Geographic tax jurisdiction for GST inter-state / intra-state logic.
/// </summary>
public partial class MstTaxRegion
{
    public int RegionId { get; set; }

    public string RegionCode { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int? CountryId { get; set; }

    public int? StateId { get; set; }

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public virtual MstCountry? Country { get; set; }

    public virtual ICollection<MstPartyTax> MstPartyTaxes { get; set; } = new List<MstPartyTax>();

    public virtual ICollection<MstTaxRate> MstTaxRates { get; set; } = new List<MstTaxRate>();

    public virtual MstState? State { get; set; }
}
