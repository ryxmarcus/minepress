using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class TrnChallan
{
    public long ChallanId { get; set; }

    public string ChallanNo { get; set; } = null!;

    public DateOnly ChallanDate { get; set; }

    public long JobId { get; set; }

    public int CompanyId { get; set; }

    public int? LocationId { get; set; }

    public int PartyId { get; set; }

    public string? DeliveryAddress { get; set; }

    public string? TransportDetails { get; set; }

    public string? VehicleNo { get; set; }

    public string? ReferenceNo { get; set; }

    public decimal? TotalQty { get; set; }

    public decimal? TotalAmount { get; set; }

    public string? Status { get; set; }

    public string? Remarks { get; set; }

    public long CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual MstCompany Company { get; set; } = null!;

    public virtual MstUser CreatedByNavigation { get; set; } = null!;

    public virtual TrnJob Job { get; set; } = null!;

    public virtual MstParty Party { get; set; } = null!;

    public virtual ICollection<TrnChallanItem> TrnChallanItems { get; set; } = new List<TrnChallanItem>();

    public virtual ICollection<TrnChallanTimeline> TrnChallanTimelines { get; set; } = new List<TrnChallanTimeline>();

    public virtual ICollection<TrnOutsourceTimeline> TrnOutsourceTimelines { get; set; } = new List<TrnOutsourceTimeline>();
}
