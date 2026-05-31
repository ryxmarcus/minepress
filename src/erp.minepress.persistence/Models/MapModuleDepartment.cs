using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Maps departments to allowed navigation modules. Controls which top-level menu groups are visible per department.
/// </summary>
public partial class MapModuleDepartment
{
    public int Id { get; set; }

    /// <summary>
    /// FK → mst_department.dept_id
    /// </summary>
    public long DepartmentId { get; set; }

    /// <summary>
    /// Module identifier matching mst_menu.module_id (top-level grouping)
    /// </summary>
    public int ModuleId { get; set; }

    public bool IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedOn { get; set; }
}
