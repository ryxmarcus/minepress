using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstDesignation
{
    public long DesignationId { get; set; }

    public string DesignationName { get; set; } = null!;

    public int? LevelNo { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<MstEmployee> MstEmployees { get; set; } = new List<MstEmployee>();

    public virtual ICollection<MstUser> MstUsers { get; set; } = new List<MstUser>();
}
