using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class TrnJobMachineAllocation
{
    public long AllocationId { get; set; }

    public long JobId { get; set; }

    public string JobNo { get; set; } = null!;

    public string ProcessCode { get; set; } = null!;

    public string? ProcessName { get; set; }

    public long MachineId { get; set; }

    public string? MachineCode { get; set; }

    public string? MachineName { get; set; }

    public decimal? PlannedQuantity { get; set; }

    public decimal? CompletedQuantity { get; set; }

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

    public virtual TrnJob Job { get; set; } = null!;

    public virtual MstMachine Machine { get; set; } = null!;

    public virtual ICollection<TrnJobMachineManpowerAllocation> TrnJobMachineManpowerAllocations { get; set; } = new List<TrnJobMachineManpowerAllocation>();
}
