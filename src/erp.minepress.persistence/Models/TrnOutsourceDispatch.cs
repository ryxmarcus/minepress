using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class TrnOutsourceDispatch
{
    public long DispatchId { get; set; }

    public long OutsourceId { get; set; }

    public DateOnly? DispatchDate { get; set; }

    public string? ChallanNo { get; set; }

    public decimal? TotalQuantity { get; set; }

    public string? Remarks { get; set; }

    public DateTime? CreatedOn { get; set; }

    public virtual TrnJobOutsource Outsource { get; set; } = null!;
}
