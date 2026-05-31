using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class TrnUserNotification
{
    public long UserNotificationId { get; set; }

    public long UserId { get; set; }

    public long? NotificationId { get; set; }

    public string Title { get; set; } = null!;

    public string Message { get; set; } = null!;

    public string? Icon { get; set; }

    public string? Color { get; set; }

    public string? Module { get; set; }

    public string? EventType { get; set; }

    public int? ReferenceId { get; set; }

    public string? ReferenceUrl { get; set; }

    public string? Priority { get; set; }

    public bool? IsRead { get; set; }

    public DateTime? ReadAt { get; set; }

    public bool? IsDismissed { get; set; }

    public DateTime? DismissedAt { get; set; }

    public bool? ActionRequired { get; set; }

    public string? ActionUrl { get; set; }

    public string? ActionLabel { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public bool? AiGenerated { get; set; }

    public DateTime? CreatedOn { get; set; }

    public virtual TrnNotification? Notification { get; set; }

    public virtual MstUser User { get; set; } = null!;
}
