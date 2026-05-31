using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Cost center master for departmental/project-wise expense tracking. Referenced by journal lines, expense items, invoice items.
/// </summary>
public partial class MstCostCenter
{
    public int CostCenterId { get; set; }

    public string CenterCode { get; set; } = null!;

    public string CenterName { get; set; } = null!;

    public int? ParentCenterId { get; set; }

    public long? DepartmentId { get; set; }

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual MstDepartment? Department { get; set; }

    public virtual ICollection<MstCostCenter> InverseParentCenter { get; set; } = new List<MstCostCenter>();

    public virtual MstCostCenter? ParentCenter { get; set; }
}
