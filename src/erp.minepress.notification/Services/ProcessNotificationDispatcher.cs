using erp.minepress.notification.Enums;
using erp.minepress.notification.Interfaces;
using erp.minepress.notification.Models;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace erp.minepress.notification.Services;

/// <summary>
/// Reads a ProcessNotificationConfig and dispatches notifications
/// to all enabled channels using the template engine and notification service.
/// </summary>
public class ProcessNotificationDispatcher : INotificationDispatcher
{
    private readonly INotificationService _notificationService;
    private readonly INotificationTemplateEngine _templateEngine;
    private readonly ILogger<ProcessNotificationDispatcher> _logger;

    public ProcessNotificationDispatcher(
        INotificationService notificationService,
        INotificationTemplateEngine templateEngine,
        ILogger<ProcessNotificationDispatcher> logger)
    {
        _notificationService = notificationService;
        _templateEngine = templateEngine;
        _logger = logger;
    }

    public async Task<IReadOnlyList<NotificationResult>> DispatchAsync(
        ProcessNotificationConfig config,
        NotificationTemplate template,
        NotificationContext context,
        CancellationToken cancellationToken = default)
    {
        return await DispatchAsync(config, [template], context, cancellationToken);
    }

    public async Task<IReadOnlyList<NotificationResult>> DispatchAsync(
        ProcessNotificationConfig config,
        IReadOnlyList<NotificationTemplate> templates,
        NotificationContext context,
        CancellationToken cancellationToken = default)
    {
        if (!config.IsActive)
        {
            _logger.LogDebug("Config {ConfigId} is inactive, skipping", config.ConfigId);
            return [];
        }

        var requests = new List<NotificationRequest>();

        // Build all notification requests based on config channel flags
        BuildInternalNotifications(config, templates, context, requests);
        BuildClientNotifications(config, templates, context, requests);
        BuildPushNotifications(config, templates, context, requests);

        if (requests.Count == 0)
        {
            _logger.LogDebug("No notifications to dispatch for config {ConfigId}", config.ConfigId);
            return [];
        }

        ApplyEmailThreadGrouping(context, requests);

        _logger.LogInformation(
            "Dispatching {Count} notifications for {ProcessCode}/{SubProcessCode} event {EventType}",
            requests.Count, config.ProcessCode, config.SubProcessCode, config.EventType);

        return await _notificationService.SendMultiChannelAsync(requests, cancellationToken);
    }

    private static void ApplyEmailThreadGrouping(NotificationContext context, List<NotificationRequest> requests)
    {
        var threadKey = ResolveThreadKey(context);
        if (string.IsNullOrWhiteSpace(threadKey))
        {
            return;
        }

        foreach (var request in requests.Where(r => r.Channel == NotificationChannel.Email))
        {
            request.EmailThreadKey = threadKey;
            request.ReferenceNo ??= threadKey;
        }
    }

    private static string? ResolveThreadKey(NotificationContext context)
    {
        if (!string.IsNullOrWhiteSpace(context.ThreadKey))
        {
            return context.ThreadKey;
        }

        var vars = context.Variables;
        if (vars.Count == 0)
        {
            return null;
        }

        if (TryGetVariable(vars, "enquiry_no", out var enquiryNo)) return $"ENQ:{enquiryNo}";
        if (TryGetVariable(vars, "quotation_no", out var quotationNo)) return $"QUOT:{quotationNo}";
        if (TryGetVariable(vars, "job_no", out var jobNo)) return $"JOB:{jobNo}";

        return null;
    }

    private static bool TryGetVariable(Dictionary<string, string> vars, string key, out string? value)
    {
        foreach (var kvp in vars)
        {
            if (string.Equals(kvp.Key, key, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(kvp.Value))
            {
                value = NormalizeThreadValue(kvp.Value);
                return !string.IsNullOrWhiteSpace(value);
            }
        }

        value = null;
        return false;
    }

    private static string NormalizeThreadValue(string input)
    {
        return Regex.Replace(input.Trim(), "\\s+", "-");
    }

    private void BuildInternalNotifications(
        ProcessNotificationConfig config,
        IReadOnlyList<NotificationTemplate> templates,
        NotificationContext context,
        List<NotificationRequest> requests)
    {
        // Internal Email
        if (config.NotifyInternalEmail)
        {
            var emailTemplate = FindTemplate(templates, NotificationChannel.Email);
            if (emailTemplate is not null)
            {
                AddIfRecipientValid(requests, emailTemplate, context.AssigneeEmail, context.Variables, config, context.AssigneeUserId, config.NotifyAssignee);
                AddIfRecipientValid(requests, emailTemplate, context.DeptHeadEmail, context.Variables, config, context.DeptHeadUserId, config.NotifyDeptHead);
                AddIfRecipientValid(requests, emailTemplate, context.SupervisorEmail, context.Variables, config, context.SupervisorUserId, config.NotifySupervisor);
                AddIfRecipientValid(requests, emailTemplate, context.ApproverEmail, context.Variables, config, context.ApproverUserId, config.NotifyApprover);
            }
        }

        // Internal SMS
        if (config.NotifyInternalSms)
        {
            var smsTemplate = FindTemplate(templates, NotificationChannel.Sms);
            var fallbackTemplate = smsTemplate ?? FindTemplate(templates, NotificationChannel.InApp);
            if (fallbackTemplate is not null)
            {
                var smsRequest = BuildSmsTemplate(fallbackTemplate, NotificationChannel.Sms);
                AddIfRecipientValid(requests, smsRequest, context.AssigneePhone, context.Variables, config, context.AssigneeUserId, config.NotifyAssignee);
                AddIfRecipientValid(requests, smsRequest, context.DeptHeadPhone, context.Variables, config, context.DeptHeadUserId, config.NotifyDeptHead);
                AddIfRecipientValid(requests, smsRequest, context.SupervisorPhone, context.Variables, config, context.SupervisorUserId, config.NotifySupervisor);
                AddIfRecipientValid(requests, smsRequest, context.ApproverPhone, context.Variables, config, context.ApproverUserId, config.NotifyApprover);
            }
        }

        // Internal WhatsApp
        if (config.NotifyInternalWhatsApp)
        {
            var waTemplate = FindTemplate(templates, NotificationChannel.WhatsApp);
            var fallbackTemplate = waTemplate ?? FindTemplate(templates, NotificationChannel.InApp);
            if (fallbackTemplate is not null)
            {
                var waRequest = BuildSmsTemplate(fallbackTemplate, NotificationChannel.WhatsApp);
                AddIfRecipientValid(requests, waRequest, context.AssigneePhone, context.Variables, config, context.AssigneeUserId, config.NotifyAssignee);
                AddIfRecipientValid(requests, waRequest, context.DeptHeadPhone, context.Variables, config, context.DeptHeadUserId, config.NotifyDeptHead);
                AddIfRecipientValid(requests, waRequest, context.SupervisorPhone, context.Variables, config, context.SupervisorUserId, config.NotifySupervisor);
                AddIfRecipientValid(requests, waRequest, context.ApproverPhone, context.Variables, config, context.ApproverUserId, config.NotifyApprover);
            }
        }
    }

    private void BuildClientNotifications(
        ProcessNotificationConfig config,
        IReadOnlyList<NotificationTemplate> templates,
        NotificationContext context,
        List<NotificationRequest> requests)
    {
        if (config.RecipientType != RecipientType.Both)
            return;

        // Client Email
        if (config.NotifyClientEmail && !string.IsNullOrEmpty(context.ClientEmail))
        {
            var emailTemplate = FindTemplate(templates, NotificationChannel.Email);
            if (emailTemplate is not null)
            {
                requests.Add(_templateEngine.BuildFromTemplate(emailTemplate, context.ClientEmail, context.Variables));
            }
        }

        // Client SMS
        if (config.NotifyClientSms && !string.IsNullOrEmpty(context.ClientPhone))
        {
            var smsTemplate = FindTemplate(templates, NotificationChannel.Sms)
                ?? FindTemplate(templates, NotificationChannel.InApp);
            if (smsTemplate is not null)
            {
                var smsReq = BuildSmsTemplate(smsTemplate, NotificationChannel.Sms);
                requests.Add(_templateEngine.BuildFromTemplate(smsReq, context.ClientPhone, context.Variables));
            }
        }

        // Client WhatsApp
        if (config.NotifyClientWhatsApp && !string.IsNullOrEmpty(context.ClientPhone))
        {
            var waTemplate = FindTemplate(templates, NotificationChannel.WhatsApp)
                ?? FindTemplate(templates, NotificationChannel.InApp);
            if (waTemplate is not null)
            {
                var waReq = BuildSmsTemplate(waTemplate, NotificationChannel.WhatsApp);
                requests.Add(_templateEngine.BuildFromTemplate(waReq, context.ClientPhone, context.Variables));
            }
        }
    }

    private void BuildPushNotifications(
        ProcessNotificationConfig config,
        IReadOnlyList<NotificationTemplate> templates,
        NotificationContext context,
        List<NotificationRequest> requests)
    {
        if (!config.NotifyPush)
            return;

        var inAppTemplate = FindTemplate(templates, NotificationChannel.InApp)
            ?? templates.FirstOrDefault();

        if (inAppTemplate is null)
            return;

        // In-app notifications go to all relevant internal users
        if (config.NotifyAssignee && context.AssigneeUserId.HasValue)
        {
            requests.Add(_templateEngine.BuildFromTemplate(
                inAppTemplate with { Channel = NotificationChannel.InApp },
                context.AssigneeUserId.Value.ToString(),
                context.Variables,
                context.AssigneeUserId));
        }

        if (config.NotifyDeptHead && context.DeptHeadUserId.HasValue)
        {
            requests.Add(_templateEngine.BuildFromTemplate(
                inAppTemplate with { Channel = NotificationChannel.InApp },
                context.DeptHeadUserId.Value.ToString(),
                context.Variables,
                context.DeptHeadUserId));
        }

        if (config.NotifySupervisor && context.SupervisorUserId.HasValue)
        {
            requests.Add(_templateEngine.BuildFromTemplate(
                inAppTemplate with { Channel = NotificationChannel.InApp },
                context.SupervisorUserId.Value.ToString(),
                context.Variables,
                context.SupervisorUserId));
        }

        if (config.NotifyApprover && context.ApproverUserId.HasValue)
        {
            requests.Add(_templateEngine.BuildFromTemplate(
                inAppTemplate with { Channel = NotificationChannel.InApp },
                context.ApproverUserId.Value.ToString(),
                context.Variables,
                context.ApproverUserId));
        }
    }

    private void AddIfRecipientValid(
        List<NotificationRequest> requests,
        NotificationTemplate template,
        string? recipient,
        Dictionary<string, string> variables,
        ProcessNotificationConfig config,
        int? userId,
        bool shouldNotify)
    {
        if (!shouldNotify || string.IsNullOrEmpty(recipient))
            return;

        var request = _templateEngine.BuildFromTemplate(template, recipient, variables, userId);
        request.Priority = config.Priority;
        requests.Add(request);
    }

    private static NotificationTemplate? FindTemplate(IReadOnlyList<NotificationTemplate> templates, NotificationChannel channel)
    {
        return templates.FirstOrDefault(t => t.Channel == channel);
    }

    /// <summary>
    /// Adapts a template for SMS/WhatsApp by overriding the channel.
    /// </summary>
    private static NotificationTemplate BuildSmsTemplate(NotificationTemplate source, NotificationChannel targetChannel)
    {
        return source with { Channel = targetChannel };
    }
}
