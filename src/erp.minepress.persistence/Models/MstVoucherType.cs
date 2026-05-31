using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstVoucherType
{
    public int VoucherTypeId { get; set; }

    public string VoucherCode { get; set; } = null!;

    public string VoucherName { get; set; } = null!;

    public string TransactionNature { get; set; } = null!;

    public bool? AffectsParty { get; set; }

    public bool? AffectsInventory { get; set; }

    public bool? IsAutoNumbering { get; set; }

    public string? Prefix { get; set; }

    public string? Suffix { get; set; }

    public int? LastNumber { get; set; }

    public bool? IsActive { get; set; }

    public int? SortOrder { get; set; }

    public DateTime? CreatedOn { get; set; }

    public virtual ICollection<TrnLedger> TrnLedgers { get; set; } = new List<TrnLedger>();
}
