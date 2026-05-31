using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstJobCategory
{
    public long JobCategoryId { get; set; }

    public string JobCategoryName { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<TrnJob> TrnJobs { get; set; } = new List<TrnJob>();
}
