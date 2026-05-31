using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstPartyContact
{
    public int Id { get; set; }

    public int PartyId { get; set; }

    public string? ContactName { get; set; }

    public string? Designation { get; set; }

    public string? Email { get; set; }

    public long? Mobile { get; set; }

    public bool IsActive { get; set; }

    public virtual MstParty Party { get; set; } = null!;
}
