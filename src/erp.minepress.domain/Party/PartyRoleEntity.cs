using erp.minepress.domain.Common;

namespace erp.minepress.domain.Party;

public class PartyRoleEntity : BaseEntity<int>
{
    public int PartyId { get; set; }
    public string RoleType { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public PartyEntity? Party { get; set; }
}
