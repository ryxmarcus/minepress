using erp.minepress.persistence.Context;
using erp.minepress.persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace erp.minepress.web.Services;

/// <summary>
/// Centralized implementation for logging user activities, in-app notifications,
/// and login/logout events across the entire ERP system.
/// </summary>
public class UserActivityService : IUserActivityService
{
    private readonly ApplicationDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<UserActivityService> _logger;

    public UserActivityService(
        ApplicationDbContext db,
        IHttpContextAccessor httpContextAccessor,
        ILogger<UserActivityService> logger)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task LogActivityAsync(ActivityLogEntry entry)
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;

            var log = new TrnUserActivityLog
            {
                UserId = entry.UserId,
                UserCode = entry.UserCode,
                UserName = entry.UserName,
                ActivityOn = DateTime.Now,
                Module = entry.Module,
                SubModule = entry.SubModule,
                ActivityType = entry.ActivityType,
                ActivityCategory = entry.ActivityCategory,
                EntityType = entry.EntityType,
                EntityId = entry.EntityId,
                EntityCode = entry.EntityCode,
                Title = entry.Title,
                Description = entry.Description,
                OldValues = entry.OldValues,
                NewValues = entry.NewValues,
                ChangedFields = entry.ChangedFields,
                ActivityData = entry.ActivityData,
                RelatedEntityType = entry.RelatedEntityType,
                RelatedEntityId = entry.RelatedEntityId,
                RelatedEntityCode = entry.RelatedEntityCode,
                JobId = entry.JobId,
                ProcessId = entry.ProcessId,
                SubprocessId = entry.SubprocessId,
                IpAddress = GetClientIpAddress(httpContext),
                UserAgent = httpContext?.Request.Headers.UserAgent.ToString(),
                Channel = "WEB",
                RequestPath = httpContext?.Request.Path.ToString(),
                HttpMethod = httpContext?.Request.Method,
                SessionId = httpContext?.Session.Id,
                DeviceInfo = GetDeviceInfo(httpContext),
                CompanyId = entry.CompanyId,
                LocationId = entry.LocationId,
                IsSuccess = entry.IsSuccess,
                ErrorMessage = entry.ErrorMessage,
                Severity = entry.Severity ?? "INFO",
                DurationMs = entry.DurationMs,
                IsArchived = false
            };

            _db.TrnUserActivityLogs.Add(log);
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log activity: {Module}/{ActivityType} for user {UserId}",
                entry.Module, entry.ActivityType, entry.UserId);
        }
    }

    /// <inheritdoc />
    public async Task LogNotificationAsync(UserNotificationEntry entry)
    {
        try
        {
            var notification = new TrnUserNotification
            {
                UserId = entry.UserId,
                NotificationId = entry.NotificationId,
                Title = entry.Title,
                Message = entry.Message,
                Icon = entry.Icon,
                Color = entry.Color,
                Module = entry.Module,
                EventType = entry.EventType,
                ReferenceId = entry.ReferenceId,
                ReferenceUrl = entry.ReferenceUrl,
                Priority = entry.Priority ?? "NORMAL",
                IsRead = false,
                IsDismissed = false,
                ActionRequired = entry.ActionRequired,
                ActionUrl = entry.ActionUrl,
                ActionLabel = entry.ActionLabel,
                ExpiresAt = entry.ExpiresAt,
                AiGenerated = false,
                CreatedOn = DateTime.Now
            };

            _db.TrnUserNotifications.Add(notification);
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log notification for user {UserId}: {Title}",
                entry.UserId, entry.Title);
        }
    }

    /// <inheritdoc />
    public async Task<long> LogLoginAsync(long userId, string? channel = "WEB")
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;

            var loginLog = new UserLoginLog
            {
                Userid = userId,
                Loginat = DateTime.Now,
                Ipaddress = GetClientIpAddress(httpContext),
                Deviceid = GetDeviceInfo(httpContext),
                Channel = channel ?? "WEB"
            };

            _db.UserLoginLogs.Add(loginLog);
            await _db.SaveChangesAsync();

            return loginLog.Logid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log login for user {UserId}", userId);
            return 0;
        }
    }

    /// <inheritdoc />
    public async Task UpdateLogoutAsync(long logId)
    {
        try
        {
            var loginLog = await _db.UserLoginLogs.FindAsync(logId);
            if (loginLog != null)
            {
                loginLog.Logoutat = DateTime.Now;
                await _db.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update logout for log {LogId}", logId);
        }
    }

    /// <inheritdoc />
    public async Task UpdateLogoutByUserAsync(long userId)
    {
        try
        {
            var loginLog = await _db.UserLoginLogs
                .Where(l => l.Userid == userId && l.Logoutat == null)
                .OrderByDescending(l => l.Loginat)
                .FirstOrDefaultAsync();

            if (loginLog != null)
            {
                loginLog.Logoutat = DateTime.Now;
                await _db.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update logout by user {UserId}", userId);
        }
    }

    // ── Private Helpers ──

    private static string? GetClientIpAddress(HttpContext? httpContext)
    {
        if (httpContext == null) return null;

        // Check for forwarded IP (behind proxy/load balancer)
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
            return forwardedFor.Split(',')[0].Trim();

        return httpContext.Connection.RemoteIpAddress?.ToString();
    }

    private static string? GetDeviceInfo(HttpContext? httpContext)
    {
        var userAgent = httpContext?.Request.Headers.UserAgent.ToString();
        if (string.IsNullOrEmpty(userAgent)) return null;

        // Simple device detection from user agent
        if (userAgent.Contains("Mobile", StringComparison.OrdinalIgnoreCase))
            return "MOBILE";
        if (userAgent.Contains("Tablet", StringComparison.OrdinalIgnoreCase))
            return "TABLET";
        return "DESKTOP";
    }
}
