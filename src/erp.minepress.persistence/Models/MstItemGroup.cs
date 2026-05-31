using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstItemGroup
{
    public long ItemGroupId { get; set; }

    public string GroupCode { get; set; } = null!;

    public string GroupName { get; set; } = null!;

    public long? ItemCategoryId { get; set; }

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual MstItemCategory? ItemCategory { get; set; }

    public virtual ICollection<MstItemSubgroup> MstItemSubgroups { get; set; } = new List<MstItemSubgroup>();

    public virtual ICollection<MstItem> MstItems { get; set; } = new List<MstItem>();
}
