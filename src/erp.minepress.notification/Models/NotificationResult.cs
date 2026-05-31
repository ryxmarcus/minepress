namespace erp.minepress.notification.Models;

public class NotificationResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public NotificationChannel Channel { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public static NotificationResult Success(NotificationChannel channel) => new()
    {
        IsSuccess = true,
        Channel = channel
    };

    public static NotificationResult Failure(NotificationChannel channel, string error) => new()
    {
        IsSuccess = false,
        Channel = channel,
        ErrorMessage = error
    };
}
