using erp.minepress.notification.Enums;

namespace erp.minepress.notification.Models;

/// <summary>
/// Maps to mst_notification_provider.
/// Defines the configured providers and their rate limits.
/// </summary>
public class NotificationProvider
{
    public int ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public NotificationChannel Channel { get; set; }
    public NotificationProviderType ProviderType { get; set; }
    public string ConfigJson { get; set; } = "{}";
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; }
    public int Priority { get; set; } = 1;
    public int RateLimitPerMin { get; set; } = 60;
    public int RateLimitPerHour { get; set; } = 1000;
}
