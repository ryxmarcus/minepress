using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstCountry
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public bool IsActive { get; set; }

    public string? IsoAlpha2 { get; set; }

    public string? IsoAlpha3 { get; set; }

    public string? IsoNumeric { get; set; }

    public string? CurrencyCode { get; set; }

    public string? CurrencyName { get; set; }

    public string? PhoneCode { get; set; }

    public string? Timezone { get; set; }

    public string? Nationality { get; set; }

    public bool? IsDefault { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual ICollection<MstCompany> MstCompanies { get; set; } = new List<MstCompany>();

    public virtual ICollection<MstLocation> MstLocations { get; set; } = new List<MstLocation>();

    public virtual ICollection<MstPartyAddress> MstPartyAddresses { get; set; } = new List<MstPartyAddress>();

    public virtual ICollection<MstState> MstStates { get; set; } = new List<MstState>();

    public virtual ICollection<MstTaxRegion> MstTaxRegions { get; set; } = new List<MstTaxRegion>();
}
