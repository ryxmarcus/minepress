using System.Text.RegularExpressions;
using erp.minepress.notification.Interfaces;
using erp.minepress.notification.Models;

namespace erp.minepress.notification.Services;

public partial class NotificationTemplateEngine : INotificationTemplateEngine
{
    public string RenderBody(string bodyTemplate, Dictionary<string, string> variables)
    {
        return ReplaceVariables(bodyTemplate, variables);
    }

    public string? RenderSubject(string? subjectTemplate, Dictionary<string, string> variables)
    {
        return subjectTemplate is null ? null : ReplaceVariables(subjectTemplate, variables);
    }

    public NotificationRequest BuildFromTemplate(
        NotificationTemplate template,
        string recipient,
        Dictionary<string, string> variables,
        int? userId = null)
    {
        return new NotificationRequest
        {
            Recipient = recipient,
            Subject = RenderSubject(template.SubjectTemplate, variables),
            Body = RenderBody(template.BodyTemplate, variables),
            Channel = template.Channel,
            Module = template.Module,
            EventType = template.EventType,
            TemplateCode = template.TemplateCode,
            TemplateVariables = variables,
            UserId = userId
        };
    }

    private static string ReplaceVariables(string template, Dictionary<string, string> variables)
    {
        return TemplateVariableRegex().Replace(template, match =>
        {
            var key = match.Groups[1].Value;
            return variables.TryGetValue(key, out var value) ? value : match.Value;
        });
    }

    [GeneratedRegex(@"\{\{(\w+)\}\}")]
    private static partial Regex TemplateVariableRegex();
}
