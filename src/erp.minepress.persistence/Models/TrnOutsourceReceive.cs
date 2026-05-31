using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class TrnOutsourceReceive
{
    public long ReceiveId { get; set; }

    public long OutsourceId { get; set; }

    public DateOnly? ReceiveDate { get; set; }

    public decimal? ReceivedQuantity { get; set; }

    public decimal? RejectedQuantity { get; set; }

    public string? Remarks { get; set; }

    public DateTime? CreatedOn { get; set; }

    public virtual TrnJobOutsource Outsource { get; set; } = null!;
}
