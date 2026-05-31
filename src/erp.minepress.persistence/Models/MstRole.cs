using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstRole
{
    public int Roleid { get; set; }

    public string Rolecode { get; set; } = null!;

    public string Rolename { get; set; } = null!;

    public bool? Issystem { get; set; }

    public bool? Isactive { get; set; }

    public string? Description { get; set; }

    public DateTime? Createdat { get; set; }

    public int? ParentRoleid { get; set; }

    public string? RoleCategory { get; set; }

    public string? RoleType { get; set; }

    public long? DeptId { get; set; }

    public int? ApprovalLevel { get; set; }

    public bool? CanApprove { get; set; }

    public bool? CanReview { get; set; }

    public bool? CanExecute { get; set; }

    public bool? IsDefault { get; set; }

    public bool? IsEditable { get; set; }

    public string? DashboardCode { get; set; }

    public bool? IsWorkflowRole { get; set; }

    public int? SecurityLevel { get; set; }

    public string? Createdby { get; set; }

    public string? Modifiedby { get; set; }

    public DateTime? Modifiedat { get; set; }

    public virtual MstDepartment? Dept { get; set; }

    public virtual ICollection<MstRole> InverseParentRole { get; set; } = new List<MstRole>();

    public virtual ICollection<MapUserRole> MapUserRoles { get; set; } = new List<MapUserRole>();

    public virtual MstRole? ParentRole { get; set; }
}
