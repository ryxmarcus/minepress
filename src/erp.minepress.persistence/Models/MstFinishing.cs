using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstFinishing
{
    public long FinishingId { get; set; }

    public string FinishingCode { get; set; } = null!;

    public string FinishingName { get; set; } = null!;

    public string? FinishingCategory { get; set; }

    public string? FinishingType { get; set; }

    public string? SupportedJobTypes { get; set; }

    public string? SupportedProducts { get; set; }

    public int? MinSheetLengthMm { get; set; }

    public int? MinSheetWidthMm { get; set; }

    public int? MaxSheetLengthMm { get; set; }

    public int? MaxSheetWidthMm { get; set; }

    public int? MinGsm { get; set; }

    public int? MaxGsm { get; set; }

    public string? SpeedUnit { get; set; }

    public int? MaxSpeedPerHour { get; set; }

    public int? SetupTimeMin { get; set; }

    public int? ChangeoverTimeMin { get; set; }

    public decimal? CostPerSheet { get; set; }

    public decimal? SetupCost { get; set; }

    public decimal? LabourCostPerHour { get; set; }

    public int? ManpowerRequired { get; set; }

    public bool? MachineRequired { get; set; }

    public bool? ManualAllowed { get; set; }

    public bool? IsActive { get; set; }

    public string? Remarks { get; set; }
}
