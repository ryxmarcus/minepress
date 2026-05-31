using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class TrnUserAccessLog
{
    public long AccessLogId { get; set; }

    public long? UserId { get; set; }

    public DateTime? LoginTime { get; set; }

    public DateTime? LogoutTime { get; set; }

    public string? IpAddress { get; set; }

    public string? DeviceInfo { get; set; }

    public string? LoginLocation { get; set; }
}
