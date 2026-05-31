using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class HrResignation
{
    public long ResignationId { get; set; }

    public string ResignationNo { get; set; } = null!;

    public long EmployeeId { get; set; }

    public DateOnly ResignationDate { get; set; }

    public string? ResignationReason { get; set; }

    public DateOnly? LastWorkingDay { get; set; }

    public int? NoticePeriodDays { get; set; }

    public int? NoticeWaiverDays { get; set; }

    public string Status { get; set; } = null!;

    public long? AcceptedBy { get; set; }

    public DateTime? AcceptedOn { get; set; }

    public string? RejectionReason { get; set; }

    public string? Remarks { get; set; }

    public long CreatedBy { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }
}
