using erp.minepress.notification.Models;

namespace erp.minepress.notification.Interfaces;

/// <summary>
/// Renders notification templates by replacing {{variable}} placeholders.
/// </summary>
public interface INotificationTemplateEngine
{
    string RenderBody(string bodyTemplate, Dictionary<string, string> variables);
    string? RenderSubject(string? subjectTemplate, Dictionary<string, string> variables);
    NotificationRequest BuildFromTemplate(NotificationTemplate template, string recipient, Dictionary<string, string> variables, int? userId = null);
}
