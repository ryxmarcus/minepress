using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstLocationType
{
    public int LocationTypeId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsActive { get; set; }
}
