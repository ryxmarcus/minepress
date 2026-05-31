using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstPartyRole
{
    public int Id { get; set; }

    public int PartyId { get; set; }

    public string RoleType { get; set; } = null!;

    public bool IsActive { get; set; }

    public virtual MstParty Party { get; set; } = null!;
}
