using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstProcessDepartmentMap
{
    public long MapId { get; set; }

    public string? ProcessCode { get; set; }

    public int? DeptId { get; set; }

    public bool? IsPrimary { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }
}
