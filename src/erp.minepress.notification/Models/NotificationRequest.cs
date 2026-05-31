namespace erp.minepress.notification.Models;

public class NotificationRequest
{
    public required string Recipient { get; set; }
    public string? Subject { get; set; }
    public required string Body { get; set; }
    public required NotificationChannel Channel { get; set; }
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
    public string? Module { get; set; }
    public string? EventType { get; set; }
    public string? ReferenceNo { get; set; }
    public string? TemplateCode { get; set; }
    public Dictionary<string, string>? TemplateVariables { get; set; }
    public int? UserId { get; set; }

    /// <summary>
    /// Logical key used by email provider to set threading headers.
    /// </summary>
    public string? EmailThreadKey { get; set; }
}
