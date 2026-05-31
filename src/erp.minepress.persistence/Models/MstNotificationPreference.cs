using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstNotificationPreference
{
    public int PreferenceId { get; set; }

    public long UserId { get; set; }

    public string Module { get; set; } = null!;

    public string EventType { get; set; } = null!;

    public bool? ChannelEmail { get; set; }

    public bool? ChannelSms { get; set; }

    public bool? ChannelWhatsapp { get; set; }

    public bool? ChannelInApp { get; set; }

    public bool? ChannelPush { get; set; }

    public bool? IsMuted { get; set; }

    public DateTime? MutedUntil { get; set; }

    public virtual MstUser User { get; set; } = null!;
}
