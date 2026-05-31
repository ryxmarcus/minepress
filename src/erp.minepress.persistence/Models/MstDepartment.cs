using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstDepartment
{
    public long DeptId { get; set; }

    public string? DeptCode { get; set; }

    public string DeptName { get; set; } = null!;

    public bool? IsActive { get; set; }

    public string? ParentDeptCode { get; set; }

    public bool? IsProduction { get; set; }

    public string? Remarks { get; set; }

    public virtual ICollection<HybEmployeeAttendance> HybEmployeeAttendances { get; set; } = new List<HybEmployeeAttendance>();

    public virtual ICollection<MstCostCenter> MstCostCenters { get; set; } = new List<MstCostCenter>();

    public virtual ICollection<MstEmployee> MstEmployees { get; set; } = new List<MstEmployee>();

    public virtual ICollection<MstProcessNotificationConfig> MstProcessNotificationConfigs { get; set; } = new List<MstProcessNotificationConfig>();

    public virtual ICollection<MstProcess> MstProcesses { get; set; } = new List<MstProcess>();

    public virtual ICollection<MstRole> MstRoles { get; set; } = new List<MstRole>();

    public virtual ICollection<MstUser> MstUsers { get; set; } = new List<MstUser>();

    public virtual ICollection<MstWorkflowStep> MstWorkflowSteps { get; set; } = new List<MstWorkflowStep>();

    public virtual ICollection<TrnWorkspaceTask> TrnWorkspaceTasks { get; set; } = new List<TrnWorkspaceTask>();
}
