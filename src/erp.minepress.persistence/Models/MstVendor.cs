using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstVendor
{
    public int VendorId { get; set; }

    public int? PartyId { get; set; }

    public int? VendorTypeId { get; set; }

    public DateOnly? ContractStartDate { get; set; }

    public DateOnly? ContractEndDate { get; set; }

    public decimal? ContractValue { get; set; }

    public string? ServiceArea { get; set; }

    public string? Remarks { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual MstParty? Party { get; set; }

    public virtual MstVendorType? VendorType { get; set; }
}
