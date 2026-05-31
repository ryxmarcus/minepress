using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstApprovalLevel
{
    public int Approvallevelid { get; set; }

    public string? Levelname { get; set; }

    public int? Sequenceno { get; set; }

    public bool? Isactive { get; set; }

    public virtual ICollection<MstWorkflowStep> MstWorkflowSteps { get; set; } = new List<MstWorkflowStep>();
}
