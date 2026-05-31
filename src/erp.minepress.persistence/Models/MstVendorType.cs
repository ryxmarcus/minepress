using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstVendorType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public bool? IsActive { get; set; }

    public virtual ICollection<MstVendor> MstVendors { get; set; } = new List<MstVendor>();
}
