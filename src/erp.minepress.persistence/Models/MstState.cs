using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstState
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public int CountryId { get; set; }

    public bool IsActive { get; set; }

    public string? GstStateCode { get; set; }

    public string? ZoneName { get; set; }

    public string? RegionName { get; set; }

    public string? CapitalCity { get; set; }

    public bool? IsUnionTerritory { get; set; }

    public bool? IsDefault { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual MstCountry Country { get; set; } = null!;

    public virtual ICollection<MstCity> MstCities { get; set; } = new List<MstCity>();

    public virtual ICollection<MstCompany> MstCompanies { get; set; } = new List<MstCompany>();

    public virtual ICollection<MstLocation> MstLocations { get; set; } = new List<MstLocation>();

    public virtual ICollection<MstPartyAddress> MstPartyAddresses { get; set; } = new List<MstPartyAddress>();

    public virtual ICollection<MstTaxRegion> MstTaxRegions { get; set; } = new List<MstTaxRegion>();
}
