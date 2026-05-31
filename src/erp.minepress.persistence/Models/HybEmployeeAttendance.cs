using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Employee attendance and shift tracking for production floor. Links to job/process for labour cost allocation.
/// </summary>
public partial class HybEmployeeAttendance
{
    public long AttendanceId { get; set; }

    public long EmployeeId { get; set; }

    public long? DepartmentId { get; set; }

    public int? ShiftTypeId { get; set; }

    public DateOnly AttendanceDate { get; set; }

    public DateTime? CheckIn { get; set; }

    public DateTime? CheckOut { get; set; }

    public int? BreakMinutes { get; set; }

    public decimal? TotalHours { get; set; }

    public string Status { get; set; } = null!;

    public decimal? OvertimeHours { get; set; }

    public bool? OvertimeApproved { get; set; }

    public long? JobId { get; set; }

    public int? ProcessId { get; set; }

    public long? MachineId { get; set; }

    public string? AttendanceData { get; set; }

    public string? Remarks { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual MstDepartment? Department { get; set; }

    public virtual MstEmployee Employee { get; set; } = null!;

    public virtual TrnJob? Job { get; set; }

    public virtual MstMachine? Machine { get; set; }

    public virtual MstProcess? Process { get; set; }

    public virtual MstShiftType? ShiftType { get; set; }
}
