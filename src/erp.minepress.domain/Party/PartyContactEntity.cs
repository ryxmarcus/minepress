using erp.minepress.domain.Common;

namespace erp.minepress.domain.Party;

public class PartyContactEntity : BaseEntity<int>
{
    public int PartyId { get; set; }
    public string? ContactName { get; set; }
    public string? Designation { get; set; }
    public string? Email { get; set; }
    public long? Mobile { get; set; }
    public bool IsActive { get; set; } = true;

    public PartyEntity? Party { get; set; }
}
