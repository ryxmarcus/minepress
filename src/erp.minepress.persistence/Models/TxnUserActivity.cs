using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Logs all user activities â€” estimation sends, WhatsApp shares, prints, logins, etc.
/// </summary>
public partial class TxnUserActivity
{
    public long ActivityId { get; set; }

    public string ActivityType { get; set; } = null!;

    public string? Description { get; set; }

    public string? ReferenceNo { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
