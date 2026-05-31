using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstMachineSelectionRule
{
    public long RuleId { get; set; }

    public string? JobType { get; set; }

    public int? MinLengthMm { get; set; }

    public int? MinWidthMm { get; set; }

    public int? MaxLengthMm { get; set; }

    public int? MaxWidthMm { get; set; }

    public int? MinGsm { get; set; }

    public int? MaxGsm { get; set; }

    public int? ColorRequired { get; set; }

    public string? PrintingSide { get; set; }

    public string? DepartmentCode { get; set; }

    public int? Priority { get; set; }

    public bool? IsActive { get; set; }
}
