using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class TrnPaymentAllocation
{
    public long PaymentAllocationId { get; set; }

    public long PaymentId { get; set; }

    public string? PaymentAgainst { get; set; }

    public long? RefId { get; set; }

    public string? RefNo { get; set; }

    public string? RefDate { get; set; }

    public decimal AllocatedAmount { get; set; }

    public DateTime? CreatedOn { get; set; }

    public virtual TrnPayment Payment { get; set; } = null!;
}
