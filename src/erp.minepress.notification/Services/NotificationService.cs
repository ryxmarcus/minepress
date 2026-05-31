using erp.minepress.notification.Interfaces;
using erp.minepress.notification.Models;
using Microsoft.Extensions.Logging;

namespace erp.minepress.notification.Services;

public class NotificationService : INotificationService
{
    private readonly IEnumerable<INotificationChannelProvider> _channelProviders;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IEnumerable<INotificationChannelProvider> channelProviders,
        ILogger<NotificationService> logger)
    {
        _channelProviders = channelProviders;
        _logger = logger;
    }

    public async Task<NotificationResult> SendAsync(NotificationRequest request, CancellationToken cancellationToken = default)
    {
        var provider = _channelProviders.FirstOrDefault(p => p.Channel == request.Channel);
        if (provider is null)
        {
            _logger.LogWarning("No provider registered for channel {Channel}", request.Channel);
            return NotificationResult.Failure(request.Channel, $"No provider registered for channel {request.Channel}");
        }

        _logger.LogInformation("Dispatching {Channel} notification to {Recipient}", request.Channel, request.Recipient);
        return await provider.SendAsync(request, cancellationToken);
    }

    public async Task<IReadOnlyList<NotificationResult>> SendMultiChannelAsync(
        IEnumerable<NotificationRequest> requests,
        CancellationToken cancellationToken = default)
    {
        var results = new List<NotificationResult>();

        foreach (var request in requests)
        {
            var result = await SendAsync(request, cancellationToken);
            results.Add(result);
        }

        return results;
    }

    public Task<NotificationResult> SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        var request = new NotificationRequest
        {
            Recipient = to,
            Subject = subject,
            Body = body,
            Channel = NotificationChannel.Email
        };

        return SendAsync(request, cancellationToken);
    }

    public Task<NotificationResult> SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        var request = new NotificationRequest
        {
            Recipient = phoneNumber,
            Body = message,
            Channel = NotificationChannel.Sms
        };

        return SendAsync(request, cancellationToken);
    }

    public Task<NotificationResult> SendWhatsAppAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        var request = new NotificationRequest
        {
            Recipient = phoneNumber,
            Body = message,
            Channel = NotificationChannel.WhatsApp
        };

        return SendAsync(request, cancellationToken);
    }

    public Task<NotificationResult> SendInAppAsync(
        int userId, string title, string message,
        string? module = null, string? referenceNo = null,
        CancellationToken cancellationToken = default)
    {
        var request = new NotificationRequest
        {
            Recipient = userId.ToString(),
            Subject = title,
            Body = message,
            Channel = NotificationChannel.InApp,
            UserId = userId,
            Module = module,
            ReferenceNo = referenceNo
        };

        return SendAsync(request, cancellationToken);
    }
}
