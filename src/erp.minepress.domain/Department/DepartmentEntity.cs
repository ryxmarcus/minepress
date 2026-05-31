using erp.minepress.domain.Common;

namespace erp.minepress.domain.Department;

public class DepartmentEntity : BaseEntity<long>
{
    public string? DeptCode { get; set; }
    public string DeptName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? ParentDeptCode { get; set; }
    public bool IsProduction { get; set; }
    public string? Remarks { get; set; }
}
