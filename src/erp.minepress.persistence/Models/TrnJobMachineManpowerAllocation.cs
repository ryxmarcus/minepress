using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class TrnJobMachineManpowerAllocation
{
    public long ManpowerAllocationId { get; set; }

    public long AllocationId { get; set; }

    public long JobId { get; set; }

    public string? JobNo { get; set; }

    public long MachineId { get; set; }

    public long EmployeeId { get; set; }

    public string? EmployeeCode { get; set; }

    public string? EmployeeName { get; set; }

    public string? RoleCode { get; set; }

    public string? RoleName { get; set; }

    public string? ShiftCode { get; set; }

    public DateTime? PlannedStartTime { get; set; }

    public DateTime? PlannedEndTime { get; set; }

    public DateTime? ActualStartTime { get; set; }

    public DateTime? ActualEndTime { get; set; }

    public decimal? EstimatedHours { get; set; }

    public decimal? ActualHours { get; set; }

    public string? AllocationStatus { get; set; }

    public string? Remarks { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public bool? IsActive { get; set; }

    public virtual TrnJobMachineAllocation Allocation { get; set; } = null!;

    public virtual MstEmployee Employee { get; set; } = null!;

    public virtual MstMachine Machine { get; set; } = null!;
}
