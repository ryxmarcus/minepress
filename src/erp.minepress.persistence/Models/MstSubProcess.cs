using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstSubProcess
{
    public int Subprocessid { get; set; }

    public int Processid { get; set; }

    public string Subprocesscode { get; set; } = null!;

    public string Subprocessname { get; set; } = null!;

    public string? Description { get; set; }

    public long Departmentid { get; set; }

    public int Sequenceno { get; set; }

    public int? Approvaltypeid { get; set; }

    public int? Approvallevel { get; set; }

    public bool? Isclientapproval { get; set; }

    public bool? Ismandatory { get; set; }

    public bool? Ismandatoryapproval { get; set; }

    public string? Templatecode { get; set; }

    public string? Templatename { get; set; }

    public bool? Notifyclientsms { get; set; }

    public bool? Notifyclientwhatsapp { get; set; }

    public bool? Notifyclientemail { get; set; }

    public bool? Notifyinternalsms { get; set; }

    public bool? Notifyinternalwhatsapp { get; set; }

    public bool? Notifyinternalemail { get; set; }

    public bool? Notifytopupalert { get; set; }

    public bool Isactive { get; set; }

    public string Createdby { get; set; } = null!;

    public DateTime? Createdon { get; set; }

    public string? Modifiedby { get; set; }

    public DateTime? Modifiedon { get; set; }

    public virtual MstApprovalType? Approvaltype { get; set; }

    public virtual MstDepartment Department { get; set; } = null!;

    public virtual ICollection<MstProcessNotificationConfig> MstProcessNotificationConfigs { get; set; } = new List<MstProcessNotificationConfig>();

    public virtual ICollection<MstWorkflowStep> MstWorkflowSteps { get; set; } = new List<MstWorkflowStep>();

    public virtual MstProcess Process { get; set; } = null!;
}
