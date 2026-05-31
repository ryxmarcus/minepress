using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstUserRole
{
    public long UserRoleId { get; set; }

    public long? Userid { get; set; }

    public int? Roleid { get; set; }

    public int? DeptId { get; set; }

    public bool? IsPrimary { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? AssignedAt { get; set; }
}
