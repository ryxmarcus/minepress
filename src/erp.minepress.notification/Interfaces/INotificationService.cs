using erp.minepress.notification.Models;

namespace erp.minepress.notification.Interfaces;

public interface INotificationService
{
    Task<NotificationResult> SendAsync(NotificationRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NotificationResult>> SendMultiChannelAsync(IEnumerable<NotificationRequest> requests, CancellationToken cancellationToken = default);
    Task<NotificationResult> SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
    Task<NotificationResult> SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
    Task<NotificationResult> SendWhatsAppAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
    Task<NotificationResult> SendInAppAsync(int userId, string title, string message, string? module = null, string? referenceNo = null, CancellationToken cancellationToken = default);
}
