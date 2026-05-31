using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstItemCategory
{
    public long ItemCategoryId { get; set; }

    public string CategoryCode { get; set; } = null!;

    public string CategoryName { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual ICollection<MstItemGroup> MstItemGroups { get; set; } = new List<MstItemGroup>();

    public virtual ICollection<MstItem> MstItems { get; set; } = new List<MstItem>();
}
