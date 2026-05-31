using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstPermission
{
    public int Permissionid { get; set; }

    public string Permissioncode { get; set; } = null!;

    public string Permissionname { get; set; } = null!;

    public string? Modulename { get; set; }

    public bool? Isactive { get; set; }

    public virtual ICollection<MapUserPermission> MapUserPermissions { get; set; } = new List<MapUserPermission>();
}
