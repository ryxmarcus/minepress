using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstShiftType
{
    public int ShiftTypeId { get; set; }

    public string ShiftCode { get; set; } = null!;

    public string ShiftName { get; set; } = null!;

    public TimeOnly? ShiftStartTime { get; set; }

    public TimeOnly? ShiftEndTime { get; set; }

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<HybEmployeeAttendance> HybEmployeeAttendances { get; set; } = new List<HybEmployeeAttendance>();

    public virtual ICollection<MstEmployee> MstEmployees { get; set; } = new List<MstEmployee>();

    public virtual ICollection<MstUser> MstUsers { get; set; } = new List<MstUser>();
}
