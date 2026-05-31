using System.ComponentModel;

namespace erp.minepress.notification.Enums;

/// <summary>
/// Recipient type for notification routing.
/// Maps to recipient_type in mst_process_notification_config.
/// </summary>
public enum RecipientType
{
    [Description("Internal staff only")]
    Internal,

    [Description("Both internal staff and client")]
    Both
}
