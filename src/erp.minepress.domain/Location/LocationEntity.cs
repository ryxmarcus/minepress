using erp.minepress.domain.Common;

namespace erp.minepress.domain.Location;

public class LocationEntity : AuditableEntity<int>
{
    public string LocationCode { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public string LocationType { get; set; } = "Warehouse";
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
    public bool IsStorageAllowed { get; set; } = true;
    public bool IsSalesPoint { get; set; }
    public bool IsPurchasePoint { get; set; }
}
