using System.ComponentModel;

namespace erp.minepress.domain.Enums;

/// <summary>
/// Lifecycle events for the Job Outsource process.
/// Used across outsource timeline, job timeline, user activity log, and notification dispatch.
/// </summary>
public enum OutsourceEventType
{
    [Description("Outsource Order Created")]
    OUTSOURCE_CREATED,

    [Description("Vendor Assigned")]
    VENDOR_ASSIGNED,

    [Description("Material Sent to Vendor")]
    MATERIAL_SENT,

    [Description("Vendor Acknowledged")]
    VENDOR_ACKNOWLEDGED,

    [Description("Process Started at Vendor")]
    PROCESS_STARTED,

    [Description("Process Completed at Vendor")]
    PROCESS_COMPLETED,

    [Description("Quality Check Done")]
    QUALITY_CHECKED,

    [Description("Material Received from Vendor")]
    MATERIAL_RECEIVED,

    [Description("Return Delayed")]
    RETURN_DELAYED,

    [Description("Rework Required")]
    REWORK_REQUIRED,

    [Description("Rework Material Sent")]
    REWORK_SENT,

    [Description("Rework Completed")]
    REWORK_COMPLETED,

    [Description("Payment Initiated")]
    PAYMENT_INITIATED,

    [Description("Payment Completed")]
    PAYMENT_COMPLETED,

    [Description("Outsource Closed")]
    OUTSOURCE_CLOSED,

    [Description("Outsource Cancelled")]
    OUTSOURCE_CANCELLED,

    [Description("Email Sent to Vendor")]
    EMAIL_SENT
}
