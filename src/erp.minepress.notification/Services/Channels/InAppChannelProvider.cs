using erp.minepress.notification.Interfaces;
using erp.minepress.notification.Models;
using Microsoft.Extensions.Logging;

namespace erp.minepress.notification.Services.Channels;

public class InAppChannelProvider : INotificationChannelProvider
{
    private readonly IInAppNotificationStore _store;
    private readonly ILogger<InAppChannelProvider> _logger;

    public InAppChannelProvider(IInAppNotificationStore store, ILogger<InAppChannelProvider> logger)
    {
        _store = store;
        _logger = logger;
    }

    public NotificationChannel Channel => NotificationChannel.InApp;

    public async Task<NotificationResult> SendAsync(NotificationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var notification = new InAppNotification
            {
                UserId = request.UserId ?? 0,
                Title = request.Subject ?? "Notification",
                Message = request.Body,
                Module = request.Module,
                EventType = request.EventType,
                ReferenceNo = request.ReferenceNo,
                Priority = request.Priority
            };

            await _store.AddAsync(notification, cancellationToken);

            _logger.LogInformation("In-app notification stored for User {UserId}: {Title}", notification.UserId, notification.Title);
            return NotificationResult.Success(NotificationChannel.InApp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store in-app notification for User {UserId}", request.UserId);
            return NotificationResult.Failure(NotificationChannel.InApp, ex.Message);
        }
    }
}
