using erp.minepress.infrastructure.ErrorLogging;
using erp.minepress.persistence.Models;
using erp.minepress.web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace erp.minepress.web.Pages.Workspace;

using erp.minepress.persistence.Context;
using Microsoft.EntityFrameworkCore;

public class DesignWorkModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly ISystemErrorLogger _systemErrorLogger;
    public DesignWorkModel(ApplicationDbContext db, ISystemErrorLogger systemErrorLogger) { _db = db; _systemErrorLogger = systemErrorLogger; }

    [BindProperty(SupportsGet = true)]
    public long TaskId { get; set; }

    public string UserFullName { get; set; } = string.Empty;
    public string? JobRateConfigJson { get; set; }
    public List<TrnDesignWorkEntry> SavedDesignEntries { get; set; } = [];
    public List<UserDesignActivity> UserDesignActivities { get; set; } = [];

    public IActionResult OnGet()
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return RedirectToPage("/Account/Login");

        if (TaskId <= 0)
            return RedirectToPage("/Workspace/MyTasks");

        UserFullName = user.Name;

        var task = _db.TrnWorkspaceTasks.AsNoTracking().FirstOrDefault(t => t.WorkspaceTaskId == TaskId);
        if (task?.JobId != null)
        {
            JobRateConfigJson = _db.HybJobRateCalculators
                .AsNoTracking()
                .Where(x => x.JobId == task.JobId)
                .OrderByDescending(x => x.CreatedOn)
                .Select(x => x.ConfigData)
                .FirstOrDefault();
        }

        SavedDesignEntries = _db.TrnDesignWorkEntries
            .AsNoTracking()
            .Where(e => e.WorkspaceTaskId == TaskId)
            .OrderBy(e => e.ActivitySequence)
            .ThenBy(e => e.DesignWorkId)
            .ToList();

        // ── User-wise activity summary ────────────────────────────────────
        UserDesignActivities = _db.TrnDesignWorkEntries
            .AsNoTracking()
            .Where(e => e.WorkspaceTaskId == TaskId && e.CreatedBy != null)
            .Join(_db.MstUsers.AsNoTracking(),
                  e => e.CreatedBy,
                  u => u.Userid,
                  (e, u) => new { Entry = e, User = u })
            .GroupBy(x => new { x.User.Userid, x.User.Name })
            .Select(g => new UserDesignActivity
            {
                UserId         = g.Key.Userid,
                UserName       = g.Key.Name,
                TotalActivities = g.Count(),
                PagesRequired  = g.Sum(x => x.Entry.PagesRequired),
                PagesCompleted = g.Sum(x => x.Entry.PagesCompleted),
                PagesPending   = g.Sum(x => x.Entry.PagesPending ?? 0),
                CompletedCount = g.Count(x => x.Entry.IsCompleted),
                LastSavedOn    = g.Max(x => (DateTime?)x.Entry.ModifiedOn) ?? g.Max(x => (DateTime?)x.Entry.CreatedOn)
            })
            .OrderByDescending(x => x.PagesCompleted)
            .ToList();

        return Page();
    }
}

public class UserDesignActivity
{
    public long   UserId          { get; set; }
    public string UserName        { get; set; } = string.Empty;
    public int    TotalActivities { get; set; }
    public int    PagesRequired   { get; set; }
    public int    PagesCompleted  { get; set; }
    public int    PagesPending    { get; set; }
    public int    CompletedCount  { get; set; }
    public DateTime? LastSavedOn  { get; set; }
}
