using erp.minepress.notification.Models;

namespace erp.minepress.notification.Interfaces;

public interface IInAppNotificationStore
{
    Task AddAsync(InAppNotification notification, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InAppNotification>> GetUnreadAsync(int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InAppNotification>> GetRecentAsync(int userId, int count = 20, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(long notificationId, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(int userId, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(int userId, CancellationToken cancellationToken = default);
}
