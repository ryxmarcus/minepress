using erp.minepress.domain.Common;

namespace erp.minepress.domain.Party;

public class PartyBankEntity : BaseEntity<int>
{
    public int PartyId { get; set; }
    public string? BankName { get; set; }
    public string? BranchName { get; set; }
    public string? AccountNo { get; set; }
    public string? IfscCode { get; set; }
    public string? MicrNo { get; set; }

    public PartyEntity? Party { get; set; }
}
