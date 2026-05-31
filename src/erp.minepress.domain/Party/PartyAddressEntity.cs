using erp.minepress.domain.Common;

namespace erp.minepress.domain.Party;

public class PartyAddressEntity : BaseEntity<int>
{
    public int PartyId { get; set; }
    public string AddressType { get; set; } = "Billing";
    public string? AddressLabel { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string? Landmark { get; set; }
    public int? CountryId { get; set; }
    public int? StateId { get; set; }
    public int? CityId { get; set; }
    public string? PostalCode { get; set; }
    public string? ContactPersonName { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public string? Gstin { get; set; }
    public string? DeliveryInstructions { get; set; }

    public PartyEntity? Party { get; set; }
}
