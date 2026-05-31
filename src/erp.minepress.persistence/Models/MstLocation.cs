using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstLocation
{
    public int LocationId { get; set; }

    public string LocationCode { get; set; } = null!;

    public string LocationName { get; set; } = null!;

    public string? LocationType { get; set; }

    public string? Description { get; set; }

    public int? ParentLocationId { get; set; }

    public int? CompanyId { get; set; }

    public int? CountryId { get; set; }

    public int? StateId { get; set; }

    public int? CityId { get; set; }

    public string? AddressLine1 { get; set; }

    public string? AddressLine2 { get; set; }

    public string? PostalCode { get; set; }

    public string? ContactPerson { get; set; }

    public string? ContactPhone { get; set; }

    public string? ContactEmail { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public bool? IsStorageAllowed { get; set; }

    public bool? IsSalesPoint { get; set; }

    public bool? IsPurchasePoint { get; set; }

    public bool? IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public virtual MstCity? City { get; set; }

    public virtual MstCompany? Company { get; set; }

    public virtual MstCountry? Country { get; set; }

    public virtual ICollection<MstLocation> InverseParentLocation { get; set; } = new List<MstLocation>();

    public virtual ICollection<MstBankAccount> MstBankAccounts { get; set; } = new List<MstBankAccount>();

    public virtual ICollection<MstEmployee> MstEmployees { get; set; } = new List<MstEmployee>();

    public virtual ICollection<MstUser> MstUsers { get; set; } = new List<MstUser>();

    public virtual MstLocation? ParentLocation { get; set; }

    public virtual MstState? State { get; set; }
}
