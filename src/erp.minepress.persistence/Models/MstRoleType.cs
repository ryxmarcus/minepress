using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstRoleType
{
    public int Roletypeid { get; set; }

    public string? Roletypecode { get; set; }

    public string? Roletypename { get; set; }

    public string? Description { get; set; }

    public bool? Isactive { get; set; }
}
