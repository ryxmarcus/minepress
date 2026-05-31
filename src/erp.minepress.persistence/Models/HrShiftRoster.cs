using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class HrShiftRoster
{
    public long RosterId { get; set; }

    public long EmployeeId { get; set; }

    public int ShiftTypeId { get; set; }

    public DateOnly EffectiveFrom { get; set; }

    public DateOnly? EffectiveTo { get; set; }

    public string? WeekOffDays { get; set; }

    public bool IsActive { get; set; }

    public long? AssignedBy { get; set; }

    public DateTime? AssignedOn { get; set; }

    public string? Remarks { get; set; }
}
