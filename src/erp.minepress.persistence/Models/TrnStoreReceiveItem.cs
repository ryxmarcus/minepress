using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class TrnStoreReceiveItem
{
    public long ReceiveItemId { get; set; }

    public long ReceiveId { get; set; }

    public int ItemSequence { get; set; }

    public string MaterialCategory { get; set; } = null!;

    public long? MaterialId { get; set; }

    public string? MaterialCode { get; set; }

    public string MaterialName { get; set; } = null!;

    public string? Specification { get; set; }

    public decimal? OrderedQuantity { get; set; }

    public decimal ReceivedQuantity { get; set; }

    public decimal? RejectedQuantity { get; set; }

    public decimal? AcceptedQuantity { get; set; }

    public string? Uom { get; set; }

    public decimal? Rate { get; set; }

    public decimal? Amount { get; set; }

    public string? BatchNo { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public string? ForPart { get; set; }

    public string? Remarks { get; set; }

    public bool? IsSelected { get; set; }

    public DateTime? CreatedOn { get; set; }

    public virtual TrnStoreReceive Receive { get; set; } = null!;
}
