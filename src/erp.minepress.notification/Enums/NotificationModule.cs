using System.ComponentModel;

namespace erp.minepress.notification.Enums;

/// <summary>
/// Module codes matching mst_notification_template.module.
/// </summary>
public enum NotificationModule
{
    [Description("Rate Calculation")]
    RateCalc,

    [Description("Quotation")]
    Quotation,

    [Description("Job")]
    Job,

    [Description("General")]
    General,

    [Description("System")]
    System
}
