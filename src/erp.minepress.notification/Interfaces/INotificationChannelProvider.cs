using erp.minepress.notification.Models;

namespace erp.minepress.notification.Interfaces;

public interface INotificationChannelProvider
{
    NotificationChannel Channel { get; }
    Task<NotificationResult> SendAsync(NotificationRequest request, CancellationToken cancellationToken = default);
}
