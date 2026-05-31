using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstApprovalType
{
    public int Approvaltypeid { get; set; }

    public string? Approvalcode { get; set; }

    public string? Approvalname { get; set; }

    public string? Description { get; set; }

    public bool? Isclientapproval { get; set; }

    public bool? Isfinancial { get; set; }

    public bool? Issystemapproval { get; set; }

    public bool? Ismandatory { get; set; }

    public bool? Isactive { get; set; }

    public string? Createdby { get; set; }

    public DateTime? Createdon { get; set; }

    public virtual ICollection<MstProcessNotificationConfig> MstProcessNotificationConfigs { get; set; } = new List<MstProcessNotificationConfig>();

    public virtual ICollection<MstWorkflowStep> MstWorkflowSteps { get; set; } = new List<MstWorkflowStep>();
}
