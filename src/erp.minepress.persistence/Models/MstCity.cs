using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstCity
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public int StateId { get; set; }

    public bool IsActive { get; set; }

    public string? DistrictName { get; set; }

    public string? TalukaName { get; set; }

    public string? Pincode { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public string? DeliveryZone { get; set; }

    public bool? TransportHub { get; set; }

    public bool? IsDefault { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual ICollection<MstCompany> MstCompanies { get; set; } = new List<MstCompany>();

    public virtual ICollection<MstLocation> MstLocations { get; set; } = new List<MstLocation>();

    public virtual ICollection<MstPartyAddress> MstPartyAddresses { get; set; } = new List<MstPartyAddress>();

    public virtual MstState State { get; set; } = null!;
}
