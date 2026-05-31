using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstItemSubgroup
{
    public long ItemSubgroupId { get; set; }

    public string SubgroupCode { get; set; } = null!;

    public string SubgroupName { get; set; } = null!;

    public long? ItemGroupId { get; set; }

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual MstItemGroup? ItemGroup { get; set; }

    public virtual ICollection<MstItem> MstItems { get; set; } = new List<MstItem>();
}
