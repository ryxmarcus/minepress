using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstPartyBank
{
    public int Id { get; set; }

    public int PartyId { get; set; }

    public string? BankName { get; set; }

    public string? BranchName { get; set; }

    public string? AccountNo { get; set; }

    public string? IfscCode { get; set; }

    public string? MicrNo { get; set; }

    public virtual MstParty Party { get; set; } = null!;
}
