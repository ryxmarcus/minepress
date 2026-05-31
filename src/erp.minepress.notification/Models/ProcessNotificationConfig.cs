using erp.minepress.notification.Enums;

namespace erp.minepress.notification.Models;

/// <summary>
/// Maps to mst_process_notification_config.
/// Defines which notifications to send for a given process/subprocess/event combination.
/// </summary>
public class ProcessNotificationConfig
{
    public int ConfigId { get; set; }
    public int ProcessId { get; set; }
    public int SubProcessId { get; set; }
    public string ProcessCode { get; set; } = string.Empty;
    public string SubProcessCode { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public NotificationEventType EventType { get; set; }
    public string EventLabel { get; set; } = string.Empty;
    public int? ApprovalTypeId { get; set; }
    public int ApprovalLevel { get; set; }
    public RecipientType RecipientType { get; set; } = RecipientType.Internal;

    // ── Who to notify ──
    public bool NotifyAssignee { get; set; }
    public bool NotifyDeptHead { get; set; }
    public bool NotifySupervisor { get; set; }
    public bool NotifyApprover { get; set; }

    // ── Client channels ──
    public bool NotifyClientSms { get; set; }
    public bool NotifyClientWhatsApp { get; set; }
    public bool NotifyClientEmail { get; set; }

    // ── Internal channels ──
    public bool NotifyInternalSms { get; set; }
    public bool NotifyInternalWhatsApp { get; set; }
    public bool NotifyInternalEmail { get; set; }

    // ── Push / In-App ──
    public bool NotifyPush { get; set; }
    public bool NotifyTopupAlert { get; set; }

    // ── Template ──
    public string TemplateCode { get; set; } = string.Empty;
    public string? SubjectTemplate { get; set; }
    public string? BodyTemplate { get; set; }

    // ── SLA ──
    public decimal SlaHours { get; set; }
    public decimal EscalateAfterHours { get; set; }
    public string? EscalateTo { get; set; }
    public decimal OverdueReminderIntervalHours { get; set; }

    // ── Trigger ──
    public string TriggerOnStatus { get; set; } = string.Empty;
    public bool AutoTrigger { get; set; }
    public string? TriggerCondition { get; set; }

    // ── Priority & flags ──
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
    public bool IsMandatory { get; set; }
    public bool IsActive { get; set; } = true;
    public int SequenceNo { get; set; }
}
