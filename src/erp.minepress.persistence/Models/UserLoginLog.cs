using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class UserLoginLog
{
    public long Logid { get; set; }

    public long? Userid { get; set; }

    public DateTime? Loginat { get; set; }

    public DateTime? Logoutat { get; set; }

    public string? Ipaddress { get; set; }

    public string? Deviceid { get; set; }

    public string? Channel { get; set; }

    public virtual MstUser? User { get; set; }
}
