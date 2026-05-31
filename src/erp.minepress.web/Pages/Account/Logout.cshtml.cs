using erp.minepress.infrastructure.ErrorLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using erp.minepress.web.Helpers;
using erp.minepress.web.Services;

namespace erp.minepress.web.Pages.Account;

public class LogoutModel : PageModel
{
    private readonly IUserActivityService _activityService;
    private readonly ISystemErrorLogger _systemErrorLogger;

    public LogoutModel(IUserActivityService activityService, ISystemErrorLogger systemErrorLogger)
    {
        _activityService = activityService;
        _systemErrorLogger = systemErrorLogger;
    }

    public async Task<IActionResult> OnGet()
    {
        await LogLogoutAsync();
        HttpContext.Session.Clear();
        return RedirectToPage("/Account/Login");
    }

    public async Task<IActionResult> OnPost()
    {
        await LogLogoutAsync();
        HttpContext.Session.Clear();
        return RedirectToPage("/Account/Login");
    }

    private async Task LogLogoutAsync()
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return;

        // ── Update login log with logout time ──
        var loginLogId = HttpContext.Session.GetObject<long>("LoginLogId");
        if (loginLogId > 0)
            await _activityService.UpdateLogoutAsync(loginLogId);
        else
            await _activityService.UpdateLogoutByUserAsync(user.UserId);

        // ── Activity Log: Logout ──
        await _activityService.LogActivityAsync(new ActivityLogEntry
        {
            UserId = user.UserId,
            UserCode = user.UserCode,
            UserName = user.Name,
            Module = "AUTH",
            ActivityType = "LOGOUT",
            ActivityCategory = "AUTH",
            Title = $"{user.Name} logged out",
            Description = $"User {user.UserCode} ({user.Name}) logged out.",
            CompanyId = user.CompanyId,
            LocationId = user.LocationId,
            Severity = "INFO"
        });
    }
}
