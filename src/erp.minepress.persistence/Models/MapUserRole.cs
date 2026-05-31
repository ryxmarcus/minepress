using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MapUserRole
{
    public long Userid { get; set; }

    public int Roleid { get; set; }

    public bool? Isactive { get; set; }

    public virtual MstRole Role { get; set; } = null!;

    public virtual MstUser User { get; set; } = null!;
}
