using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstSupplierType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public bool? IsActive { get; set; }

    public virtual ICollection<MstSupplier> MstSuppliers { get; set; } = new List<MstSupplier>();
}
