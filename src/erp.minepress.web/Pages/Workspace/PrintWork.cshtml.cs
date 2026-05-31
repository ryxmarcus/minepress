using erp.minepress.infrastructure.ErrorLogging;
using erp.minepress.web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace erp.minepress.web.Pages.Workspace;

using erp.minepress.persistence.Context;
using Microsoft.EntityFrameworkCore;

public class PrintWorkModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly ISystemErrorLogger _systemErrorLogger;
    public PrintWorkModel(ApplicationDbContext db, ISystemErrorLogger systemErrorLogger) { _db = db; _systemErrorLogger = systemErrorLogger; }

    [BindProperty(SupportsGet = true)]
    public long TaskId { get; set; }

    public string UserFullName { get; set; } = string.Empty;
    public string? JobRateConfigJson { get; set; }

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

        return Page();
    }
}
