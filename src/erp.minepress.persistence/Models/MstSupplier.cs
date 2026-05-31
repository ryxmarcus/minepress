using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstSupplier
{
    public int SupplierId { get; set; }

    public int? PartyId { get; set; }

    public int? SupplierTypeId { get; set; }

    public bool? TdsApplicable { get; set; }

    public decimal? TdsRate { get; set; }

    public int? PaymentCycleDays { get; set; }

    public int? PreferredCurrency { get; set; }

    public string? Remarks { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual MstParty? Party { get; set; }

    public virtual MstCurrency? PreferredCurrencyNavigation { get; set; }

    public virtual MstSupplierType? SupplierType { get; set; }
}
