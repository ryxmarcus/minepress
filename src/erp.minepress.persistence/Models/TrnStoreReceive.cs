using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class TrnStoreReceive
{
    public long ReceiveId { get; set; }

    public string ReceiveNo { get; set; } = null!;

    public DateOnly ReceiveDate { get; set; }

    public string ReceiveType { get; set; } = null!;

    public long? GrnId { get; set; }

    public string? GrnNo { get; set; }

    public long? JobId { get; set; }

    public string? JobNo { get; set; }

    public int? SupplierId { get; set; }

    public string? SupplierName { get; set; }

    public int? LocationId { get; set; }

    public int CompanyId { get; set; }

    public int? TotalItems { get; set; }

    public decimal? TotalAmount { get; set; }

    public string Status { get; set; } = null!;

    public string? Remarks { get; set; }

    public long? ApprovedBy { get; set; }

    public DateTime? ApprovedOn { get; set; }

    public long CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<TrnStoreReceiveItem> TrnStoreReceiveItems { get; set; } = new List<TrnStoreReceiveItem>();
}
