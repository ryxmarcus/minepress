using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstUomType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public bool? IsActive { get; set; }

    public virtual ICollection<MstUom> MstUoms { get; set; } = new List<MstUom>();
}
