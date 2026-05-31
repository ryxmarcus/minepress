using erp.minepress.infrastructure.ErrorLogging;
using erp.minepress.web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace erp.minepress.web.Pages.Workspace;

using erp.minepress.persistence.Context;
using erp.minepress.persistence.Models;
using Microsoft.EntityFrameworkCore;

public class ProcessWorkModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly ISystemErrorLogger _systemErrorLogger;
    public ProcessWorkModel(ApplicationDbContext db, ISystemErrorLogger systemErrorLogger) { _db = db; _systemErrorLogger = systemErrorLogger; }

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

        // Fetch the workspace task to get JobId
        var task = _db.TrnWorkspaceTasks.AsNoTracking().FirstOrDefault(t => t.WorkspaceTaskId == TaskId);
        if (task?.JobId != null)
        {
            // Fetch config_data for the job
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
