using erp.minepress.web.Helpers;

namespace erp.minepress.web.Services;

/// <summary>
/// Centralized service for logging user activities, notifications, and login/logout events
/// across the entire ERP system.
/// </summary>
public interface IUserActivityService
{
    /// <summary>
    /// Logs a user activity into trn_user_activity_log.
    /// </summary>
    Task LogActivityAsync(ActivityLogEntry entry);

    /// <summary>
    /// Creates an in-app notification for a user in trn_user_notification.
    /// </summary>
    Task LogNotificationAsync(UserNotificationEntry entry);

    /// <summary>
    /// Records a login event in user_login_log and returns the log ID for later logout update.
    /// </summary>
    Task<long> LogLoginAsync(long userId, string? channel = "WEB");

    /// <summary>
    /// Updates the logout timestamp for the given login log entry.
    /// </summary>
    Task UpdateLogoutAsync(long logId);

    /// <summary>
    /// Updates the logout timestamp by user ID (finds the latest open session).
    /// </summary>
    Task UpdateLogoutByUserAsync(long userId);
}

/// <summary>
/// Represents an entry for trn_user_activity_log.
/// </summary>
public class ActivityLogEntry
{
    public long UserId { get; set; }
    public string UserCode { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string Module { get; set; } = string.Empty;
    public string? SubModule { get; set; }
    public string ActivityType { get; set; } = string.Empty;
    public string? ActivityCategory { get; set; } = "DATA";
    public string? EntityType { get; set; }
    public long? EntityId { get; set; }
    public string? EntityCode { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public List<string>? ChangedFields { get; set; }
    public string? ActivityData { get; set; }
    public string? RelatedEntityType { get; set; }
    public long? RelatedEntityId { get; set; }
    public string? RelatedEntityCode { get; set; }
    public long? JobId { get; set; }
    public int? ProcessId { get; set; }
    public int? SubprocessId { get; set; }
    public int? CompanyId { get; set; }
    public int? LocationId { get; set; }
    public bool IsSuccess { get; set; } = true;
    public string? ErrorMessage { get; set; }
    public string? Severity { get; set; } = "INFO";
    public int? DurationMs { get; set; }

    /// <summary>
    /// Creates an ActivityLogEntry pre-filled with user session data.
    /// </summary>
    public static ActivityLogEntry FromUser(UserSessionData user, string module, string activityType, string title)
    {
        return new ActivityLogEntry
        {
            UserId = user.UserId,
            UserCode = user.UserCode,
            UserName = user.Name,
            Module = module,
            ActivityType = activityType,
            Title = title,
            CompanyId = user.CompanyId,
            LocationId = user.LocationId
        };
    }
}

/// <summary>
/// Represents an entry for trn_user_notification (in-app bell notification).
/// </summary>
public class UserNotificationEntry
{
    public long UserId { get; set; }
    public long? NotificationId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public string? Module { get; set; }
    public string? EventType { get; set; }
    public int? ReferenceId { get; set; }
    public string? ReferenceUrl { get; set; }
    public string? Priority { get; set; } = "NORMAL";
    public bool ActionRequired { get; set; }
    public string? ActionUrl { get; set; }
    public string? ActionLabel { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
