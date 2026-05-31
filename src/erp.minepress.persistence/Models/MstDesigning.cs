using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstDesigning
{
    public long DesigningId { get; set; }

    public string DesignCode { get; set; } = null!;

    public string DesignName { get; set; } = null!;

    public string? DesignCategory { get; set; }

    public string? DesignType { get; set; }

    public string? JobTypesSupported { get; set; }

    public bool? IsDesignByParty { get; set; }

    public bool? IsPlateByParty { get; set; }

    public string? SoftwareUsed { get; set; }

    public string? FileFormat { get; set; }

    public string? ColorMode { get; set; }

    public int? RevisionAllowed { get; set; }

    public decimal? ReworkChargePerRevision { get; set; }

    public decimal? BaseCost { get; set; }

    public string? CostUnit { get; set; }

    public decimal? AvgTimeHours { get; set; }

    public int? ManpowerRequired { get; set; }

    public bool? IsCostApplicable { get; set; }

    public bool? IsActive { get; set; }

    public string? Remarks { get; set; }

    public DateTime? CreatedAt { get; set; }
}
