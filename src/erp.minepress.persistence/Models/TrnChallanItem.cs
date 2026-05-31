using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class TrnChallanItem
{
    public long ChallanItemId { get; set; }

    public long ChallanId { get; set; }

    public long JobItemId { get; set; }

    public int? ItemSequence { get; set; }

    public string? ProductName { get; set; }

    public string? ProductDescription { get; set; }

    public int? JobQuantity { get; set; }

    public int? DeliveredQuantity { get; set; }

    public int? PendingQuantity { get; set; }

    public int? UomId { get; set; }

    public decimal? Rate { get; set; }

    public decimal? Amount { get; set; }

    public string? Remarks { get; set; }

    public long CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public virtual TrnChallan Challan { get; set; } = null!;

    public virtual TrnJobItem JobItem { get; set; } = null!;
}
