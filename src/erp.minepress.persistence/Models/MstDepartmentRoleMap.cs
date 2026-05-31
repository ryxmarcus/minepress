using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstDepartmentRoleMap
{
    public long MapId { get; set; }

    public int? DeptId { get; set; }

    public int? Roleid { get; set; }

    public bool? IsPrimary { get; set; }

    public bool? IsActive { get; set; }
}
