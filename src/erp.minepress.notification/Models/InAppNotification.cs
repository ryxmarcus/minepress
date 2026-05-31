namespace erp.minepress.notification.Models;

public class InAppNotification
{
    public long Id { get; set; }
    public int UserId { get; set; }
    public required string Title { get; set; }
    public required string Message { get; set; }
    public string? Module { get; set; }
    public string? EventType { get; set; }
    public string? ReferenceNo { get; set; }
    public string? ActionUrl { get; set; }
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
    public bool IsRead { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public DateTime? ReadOn { get; set; }
}
