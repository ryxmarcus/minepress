using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class HrUniformAllotment
{
    public long UniformId { get; set; }

    public long EmployeeId { get; set; }

    public string ItemType { get; set; } = null!;

    public string? ItemDescription { get; set; }

    public string? Size { get; set; }

    public int? Quantity { get; set; }

    public DateOnly AllotmentDate { get; set; }

    public decimal? CostPerUnit { get; set; }

    public decimal? TotalCost { get; set; }

    public bool? RecoveryFromSalary { get; set; }

    public int? RecoveryMonths { get; set; }

    public string? Remarks { get; set; }

    public long CreatedBy { get; set; }

    public DateTime CreatedOn { get; set; }
}
