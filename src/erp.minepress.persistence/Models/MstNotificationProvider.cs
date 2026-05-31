using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstNotificationProvider
{
    public int ProviderId { get; set; }

    public string ProviderName { get; set; } = null!;

    public string Channel { get; set; } = null!;

    public string ProviderType { get; set; } = null!;

    public string ConfigJson { get; set; } = null!;

    public bool? IsActive { get; set; }

    public bool? IsDefault { get; set; }

    public int? Priority { get; set; }

    public int? RateLimitPerMin { get; set; }

    public int? RateLimitPerHour { get; set; }

    public DateTime? CreatedOn { get; set; }
}
