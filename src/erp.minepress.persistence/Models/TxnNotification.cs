using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Internal notifications sent to roles (SALES, MANAGEMENT, ADMIN) or specific users. Used for estimation alerts, job updates, etc.
/// </summary>
public partial class TxnNotification
{
    public long NotificationId { get; set; }

    public string NotificationType { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Message { get; set; }

    public string? TargetRole { get; set; }

    public long? TargetUserId { get; set; }

    public string? ReferenceNo { get; set; }

    public bool IsRead { get; set; }

    public DateTime? ReadAt { get; set; }

    public DateTime CreatedAt { get; set; }
}
