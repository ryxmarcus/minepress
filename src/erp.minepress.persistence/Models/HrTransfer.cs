using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class HrTransfer
{
    public long TransferId { get; set; }

    public string TransferNo { get; set; } = null!;

    public long EmployeeId { get; set; }

    public DateOnly TransferDate { get; set; }

    public long? FromDeptId { get; set; }

    public long ToDeptId { get; set; }

    public int? FromLocationId { get; set; }

    public int? ToLocationId { get; set; }

    public string? TransferReason { get; set; }

    public DateOnly EffectiveDate { get; set; }

    public string Status { get; set; } = null!;

    public long? ApprovedBy { get; set; }

    public DateTime? ApprovedOn { get; set; }

    public string? OrderLetterPath { get; set; }

    public long CreatedBy { get; set; }

    public DateTime CreatedOn { get; set; }
}
