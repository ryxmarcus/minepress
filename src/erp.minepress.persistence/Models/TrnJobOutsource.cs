using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class TrnJobOutsource
{
    public long OutsourceId { get; set; }

    public string OutsourceNo { get; set; } = null!;

    public DateOnly OutsourceDate { get; set; }

    public long JobId { get; set; }

    public long VendorId { get; set; }

    public string? ProcessType { get; set; }

    public decimal? TotalQuantity { get; set; }

    public decimal? TotalAmount { get; set; }

    public DateOnly? ExpectedDeliveryDate { get; set; }

    public DateOnly? ActualDeliveryDate { get; set; }

    public string? Status { get; set; }

    public string? Remarks { get; set; }

    public long CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual TrnJob Job { get; set; } = null!;

    public virtual ICollection<TrnJobOutsourceItem> TrnJobOutsourceItems { get; set; } = new List<TrnJobOutsourceItem>();

    public virtual ICollection<TrnOutsourceDispatch> TrnOutsourceDispatches { get; set; } = new List<TrnOutsourceDispatch>();

    public virtual ICollection<TrnOutsourceReceive> TrnOutsourceReceives { get; set; } = new List<TrnOutsourceReceive>();

    public virtual ICollection<TrnOutsourceTimeline> TrnOutsourceTimelines { get; set; } = new List<TrnOutsourceTimeline>();
}
