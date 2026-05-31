using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class TrnReceiptAllocation
{
    public long ReceiptAllocationId { get; set; }

    public long ReceiptId { get; set; }

    public long SalesInvoiceId { get; set; }

    public decimal AllocatedAmount { get; set; }

    public decimal? UnallocatedAmount { get; set; }

    public DateTime? CreatedOn { get; set; }

    public virtual TrnReceipt Receipt { get; set; } = null!;

    public virtual TrnSalesInvoice SalesInvoice { get; set; } = null!;
}
