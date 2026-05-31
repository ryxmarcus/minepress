using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstCustomerGroup
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<MstCustomer> MstCustomers { get; set; } = new List<MstCustomer>();
}
