using erp.minepress.infrastructure.ErrorLogging;
using erp.minepress.persistence.Context;
using erp.minepress.web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace erp.minepress.web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<NotificationController> _logger;
    private readonly ISystemErrorLogger _systemErrorLogger;

    public NotificationController(ApplicationDbContext db, ILogger<NotificationController> logger, ISystemErrorLogger systemErrorLogger)
    {
        _db = db;
        _logger = logger;
        _systemErrorLogger = systemErrorLogger;
    }

    private async Task AuditExceptionAsync(Exception ex, string additionalData, string severity = "Error")
        => await _systemErrorLogger.LogAsync(ex, HttpContext, severity, additionalData);

    /// <summary>
    /// Get recent notifications for the current user (top 30, non-dismissed).
    /// </summary>
    [HttpGet("list")]
    public async Task<IActionResult> GetNotifications()
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized();

        var startOfDay = DateTime.Today;
        var endOfDay = startOfDay.AddDays(1);

        var notifications = await _db.TrnUserNotifications
            .Where(n => n.UserId == user.UserId
                        && (n.IsDismissed == null || n.IsDismissed == false)
                        && (n.ExpiresAt == null || n.ExpiresAt > DateTime.Now)
                        && n.CreatedOn.HasValue
                        && n.CreatedOn.Value >= startOfDay
                        && n.CreatedOn.Value < endOfDay)
            .OrderByDescending(n => n.CreatedOn)
            .Take(30)
            .Select(n => new
            {
                n.UserNotificationId,
                n.Title,
                n.Message,
                n.Icon,
                n.Color,
                n.Module,
                n.EventType,
                n.ReferenceId,
                n.ReferenceUrl,
                n.Priority,
                n.IsRead,
                n.ActionRequired,
                n.ActionUrl,
                n.ActionLabel,
                n.AiGenerated,
                CreatedOn = n.CreatedOn.HasValue ? n.CreatedOn.Value.ToString("yyyy-MM-ddTHH:mm:ss") : null,
                CreatedOnDisplay = n.CreatedOn.HasValue ? n.CreatedOn.Value.ToString("dd-MMM-yyyy HH:mm") : ""
            })
            .ToListAsync();

        return Ok(notifications);
    }

    /// <summary>
    /// Get unread notification count for badge display.
    /// </summary>
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized();

        var startOfDay = DateTime.Today;
        var endOfDay = startOfDay.AddDays(1);

        var count = await _db.TrnUserNotifications
            .CountAsync(n => n.UserId == user.UserId
                             && (n.IsRead == null || n.IsRead == false)
                             && (n.IsDismissed == null || n.IsDismissed == false)
                             && (n.ExpiresAt == null || n.ExpiresAt > DateTime.Now)
                             && n.CreatedOn.HasValue
                             && n.CreatedOn.Value >= startOfDay
                             && n.CreatedOn.Value < endOfDay);

        return Ok(new { count });
    }

    /// <summary>
    /// Mark a single notification as read.
    /// </summary>
    [HttpPost("mark-read/{id:long}")]
    public async Task<IActionResult> MarkAsRead(long id)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized();

        var notification = await _db.TrnUserNotifications
            .FirstOrDefaultAsync(n => n.UserNotificationId == id && n.UserId == user.UserId);

        if (notification == null)
            return NotFound();

        notification.IsRead = true;
        notification.ReadAt = DateTime.Now;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Marked as read." });
    }

    /// <summary>
    /// Mark all notifications as read for the current user.
    /// </summary>
    [HttpPost("mark-all-read")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized();

        var startOfDay = DateTime.Today;
        var endOfDay = startOfDay.AddDays(1);

        var unread = await _db.TrnUserNotifications
            .Where(n => n.UserId == user.UserId
                        && (n.IsRead == null || n.IsRead == false)
                        && (n.IsDismissed == null || n.IsDismissed == false)
                        && n.CreatedOn.HasValue
                        && n.CreatedOn.Value >= startOfDay
                        && n.CreatedOn.Value < endOfDay)
            .ToListAsync();

        foreach (var n in unread)
        {
            n.IsRead = true;
            n.ReadAt = DateTime.Now;
        }

        await _db.SaveChangesAsync();

        return Ok(new { message = "All marked as read.", count = unread.Count });
    }

    /// <summary>
    /// Dismiss (soft-delete) a single notification.
    /// </summary>
    [HttpPost("dismiss/{id:long}")]
    public async Task<IActionResult> Dismiss(long id)
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return Unauthorized();

        var notification = await _db.TrnUserNotifications
            .FirstOrDefaultAsync(n => n.UserNotificationId == id && n.UserId == user.UserId);

        if (notification == null)
            return NotFound();

        notification.IsDismissed = true;
        notification.DismissedAt = DateTime.Now;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Dismissed." });
    }
}
