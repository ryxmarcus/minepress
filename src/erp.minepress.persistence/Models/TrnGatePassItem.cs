using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class TrnGatePassItem
{
    public long GatePassItemId { get; set; }

    public long GatePassId { get; set; }

    public int? ItemSequence { get; set; }

    public string? Description { get; set; }

    public decimal Quantity { get; set; }

    public int? UomId { get; set; }

    public decimal? ReceivedQuantity { get; set; }

    public decimal? PendingQuantity { get; set; }

    public string? Status { get; set; }

    public string? Remarks { get; set; }

    public DateTime? CreatedOn { get; set; }

    public virtual TrnGatePass GatePass { get; set; } = null!;
}
