using System.ComponentModel;

namespace erp.minepress.notification.Enums;

/// <summary>
/// Provider types for notification delivery.
/// Maps to provider_type in mst_notification_provider.
/// </summary>
public enum NotificationProviderType
{
    [Description("SMTP Email Provider")]
    Smtp,

    [Description("Twilio SMS/WhatsApp Provider")]
    Twilio,

    [Description("Firebase Push Notification")]
    Firebase
}
