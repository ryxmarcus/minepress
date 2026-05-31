using System.ComponentModel;

namespace erp.minepress.notification.Enums;

/// <summary>
/// Event types that trigger notifications.
/// Maps to event_type_code in mst_process_notification_config.
/// </summary>
public enum NotificationEventType
{
    [Description("Task Assigned")]
    TaskAssign,

    [Description("Task Completed")]
    TaskComplete,

    [Description("Overdue Alert")]
    OverdueAlert,

    [Description("Approval Request")]
    ApprovalRequest,

    [Description("Approval Approved")]
    ApprovalApproved,

    [Description("Approval Rejected")]
    ApprovalRejected,

    [Description("Client Notification")]
    ClientNotify,

    [Description("AI Insight")]
    AiInsight,

    [Description("Top-Up Alert")]
    TopupAlert
}
