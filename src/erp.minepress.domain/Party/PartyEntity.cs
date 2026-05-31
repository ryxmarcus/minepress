using erp.minepress.domain.Common;

namespace erp.minepress.domain.Party;

public class PartyEntity : BaseEntity<int>
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Address1 { get; set; }
    public string? Address2 { get; set; }
    public int? CityId { get; set; }
    public string? Pin { get; set; }
    public string? Email { get; set; }
    public long? Mobile { get; set; }
    public string? GstNo { get; set; }
    public string? PanNo { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public ICollection<PartyContactEntity> Contacts { get; set; } = [];
    public ICollection<PartyAddressEntity> Addresses { get; set; } = [];
    public ICollection<PartyRoleEntity> Roles { get; set; } = [];
    public ICollection<PartyBankEntity> Banks { get; set; } = [];
}
