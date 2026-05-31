namespace erp.minepress.notification.Models;

/// <summary>
/// Maps to mst_notification_template.
/// Defines the content templates for notifications per channel.
/// </summary>
public record NotificationTemplate
{
    public int TemplateId { get; set; }
    public string TemplateCode { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public NotificationChannel Channel { get; set; }
    public string? SubjectTemplate { get; set; }
    public string BodyTemplate { get; set; } = string.Empty;
    public string BodyFormat { get; set; } = "HTML";
    public bool IsActive { get; set; } = true;
    public bool IsAiEnabled { get; set; }
    public string? AiPromptTemplate { get; set; }
}
