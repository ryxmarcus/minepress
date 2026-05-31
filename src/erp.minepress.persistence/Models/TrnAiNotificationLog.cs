using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class TrnAiNotificationLog
{
    public long AiLogId { get; set; }

    public long? NotificationId { get; set; }

    public string AiAction { get; set; } = null!;

    public string? AiModel { get; set; }

    public string? AiPrompt { get; set; }

    public string? AiResponse { get; set; }

    public decimal? AiConfidence { get; set; }

    public int? TokensUsed { get; set; }

    public int? LatencyMs { get; set; }

    public bool? WasApproved { get; set; }

    public long? ReviewedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public virtual TrnNotification? Notification { get; set; }
}
