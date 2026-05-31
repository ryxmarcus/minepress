using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstProcessStage
{
    public long StageId { get; set; }

    public string StageName { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsActive { get; set; }
}
