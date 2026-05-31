using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstProcess
{
    public int Processid { get; set; }

    public string Processcode { get; set; } = null!;

    public string Processname { get; set; } = null!;

    public string? Description { get; set; }

    public long Departmentid { get; set; }

    public int Sequenceno { get; set; }

    public bool? Ismandatory { get; set; }

    public bool? Isapprovalrequired { get; set; }

    public bool? Isclientapproval { get; set; }

    public string? Templatecode { get; set; }

    public string? Templatename { get; set; }

    public bool Isactive { get; set; }

    public string Createdby { get; set; } = null!;

    public DateTime? Createdon { get; set; }

    public string? Modifiedby { get; set; }

    public DateTime? Modifiedon { get; set; }

    public virtual MstDepartment Department { get; set; } = null!;

    public virtual ICollection<HybEmployeeAttendance> HybEmployeeAttendances { get; set; } = new List<HybEmployeeAttendance>();

    public virtual ICollection<MstProcessNotificationConfig> MstProcessNotificationConfigs { get; set; } = new List<MstProcessNotificationConfig>();

    public virtual ICollection<MstWorkflowStep> MstWorkflowSteps { get; set; } = new List<MstWorkflowStep>();

    public virtual ICollection<TrnWorkspaceTask> TrnWorkspaceTasks { get; set; } = new List<TrnWorkspaceTask>();
}
