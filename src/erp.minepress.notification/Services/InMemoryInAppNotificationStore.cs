using System.Collections.Concurrent;
using erp.minepress.notification.Interfaces;
using erp.minepress.notification.Models;

namespace erp.minepress.notification.Services;

public class InMemoryInAppNotificationStore : IInAppNotificationStore
{
    private readonly ConcurrentDictionary<int, List<InAppNotification>> _notifications = new();
    private long _nextId;

    public Task AddAsync(InAppNotification notification, CancellationToken cancellationToken = default)
    {
        notification.Id = Interlocked.Increment(ref _nextId);
        var userNotifications = _notifications.GetOrAdd(notification.UserId, _ => []);

        lock (userNotifications)
        {
            userNotifications.Add(notification);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<InAppNotification>> GetUnreadAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (!_notifications.TryGetValue(userId, out var userNotifications))
            return Task.FromResult<IReadOnlyList<InAppNotification>>([]);

        lock (userNotifications)
        {
            return Task.FromResult<IReadOnlyList<InAppNotification>>(
                userNotifications.Where(n => !n.IsRead).OrderByDescending(n => n.CreatedOn).ToList());
        }
    }

    public Task<IReadOnlyList<InAppNotification>> GetRecentAsync(int userId, int count = 20, CancellationToken cancellationToken = default)
    {
        if (!_notifications.TryGetValue(userId, out var userNotifications))
            return Task.FromResult<IReadOnlyList<InAppNotification>>([]);

        lock (userNotifications)
        {
            return Task.FromResult<IReadOnlyList<InAppNotification>>(
                userNotifications.OrderByDescending(n => n.CreatedOn).Take(count).ToList());
        }
    }

    public Task MarkAsReadAsync(long notificationId, CancellationToken cancellationToken = default)
    {
        foreach (var userNotifications in _notifications.Values)
        {
            lock (userNotifications)
            {
                var notification = userNotifications.FirstOrDefault(n => n.Id == notificationId);
                if (notification is not null)
                {
                    notification.IsRead = true;
                    notification.ReadOn = DateTime.UtcNow;
                    break;
                }
            }
        }

        return Task.CompletedTask;
    }

    public Task MarkAllAsReadAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (!_notifications.TryGetValue(userId, out var userNotifications))
            return Task.CompletedTask;

        lock (userNotifications)
        {
            foreach (var notification in userNotifications.Where(n => !n.IsRead))
            {
                notification.IsRead = true;
                notification.ReadOn = DateTime.UtcNow;
            }
        }

        return Task.CompletedTask;
    }

    public Task<int> GetUnreadCountAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (!_notifications.TryGetValue(userId, out var userNotifications))
            return Task.FromResult(0);

        lock (userNotifications)
        {
            return Task.FromResult(userNotifications.Count(n => !n.IsRead));
        }
    }
}
