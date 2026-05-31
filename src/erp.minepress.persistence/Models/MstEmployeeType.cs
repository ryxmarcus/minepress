using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstEmployeeType
{
    public int EmployeeTypeId { get; set; }

    public string TypeCode { get; set; } = null!;

    public string TypeName { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<MstEmployee> MstEmployees { get; set; } = new List<MstEmployee>();
}
