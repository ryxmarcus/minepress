using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MapUserPermission
{
    public long Userid { get; set; }

    public int Permissionid { get; set; }

    public bool? Isallowed { get; set; }

    public virtual MstPermission Permission { get; set; } = null!;

    public virtual MstUser User { get; set; } = null!;
}
