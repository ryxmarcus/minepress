using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class TrnGatePass
{
    public long GatePassId { get; set; }

    public string GatePassNo { get; set; } = null!;

    public DateOnly GatePassDate { get; set; }

    public string GatepassType { get; set; } = null!;

    public int CompanyId { get; set; }

    public int? LocationId { get; set; }

    public string? ReferenceType { get; set; }

    public string? ReferenceNo { get; set; }

    public DateOnly? ReferenceDate { get; set; }

    public string? VehicleNo { get; set; }

    public string? DriverName { get; set; }

    public string? DriverContact { get; set; }

    public string? Purpose { get; set; }

    public decimal? TotalQuantity { get; set; }

    public string? Status { get; set; }

    public long? ApprovedBy { get; set; }

    public DateTime? ApprovedOn { get; set; }

    public string? Remarks { get; set; }

    public long CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual MstCompany Company { get; set; } = null!;

    public virtual MstUser CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<TrnGatePassItem> TrnGatePassItems { get; set; } = new List<TrnGatePassItem>();
}
