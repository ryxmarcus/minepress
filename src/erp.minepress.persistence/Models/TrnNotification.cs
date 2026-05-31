using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class TrnNotification
{
    public long NotificationId { get; set; }

    public string? NotificationNo { get; set; }

    public int? TemplateId { get; set; }

    public string Channel { get; set; } = null!;

    public string Module { get; set; } = null!;

    public string EventType { get; set; } = null!;

    public int? ReferenceId { get; set; }

    public string? ReferenceNo { get; set; }

    public string RecipientType { get; set; } = null!;

    public long? RecipientUserId { get; set; }

    public int? RecipientPartyId { get; set; }

    public string? RecipientName { get; set; }

    public string? RecipientEmail { get; set; }

    public string? RecipientMobile { get; set; }

    public string? RecipientWhatsapp { get; set; }

    public string? Subject { get; set; }

    public string Body { get; set; } = null!;

    public string? BodyFormat { get; set; }

    public string? Priority { get; set; }

    public string? Status { get; set; }

    public int? RetryCount { get; set; }

    public int? MaxRetries { get; set; }

    public DateTime? ScheduledAt { get; set; }

    public DateTime? SentAt { get; set; }

    public DateTime? DeliveredAt { get; set; }

    public DateTime? ReadAt { get; set; }

    public string? FailureReason { get; set; }

    public string? ExternalRefId { get; set; }

    public string? ProviderName { get; set; }

    public string? ProviderResponse { get; set; }

    public string? AttachmentsJson { get; set; }

    public string? MetadataJson { get; set; }

    public bool? AiGenerated { get; set; }

    public bool? AiPersonalized { get; set; }

    public string? AiSummary { get; set; }

    public DateTime? CreatedOn { get; set; }

    public virtual MstUser? RecipientUser { get; set; }

    public virtual MstNotificationTemplate? Template { get; set; }

    public virtual ICollection<TrnAiNotificationLog> TrnAiNotificationLogs { get; set; } = new List<TrnAiNotificationLog>();

    public virtual ICollection<TrnUserNotification> TrnUserNotifications { get; set; } = new List<TrnUserNotification>();
}
