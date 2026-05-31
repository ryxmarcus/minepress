using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using erp.minepress.web.Helpers;

namespace erp.minepress.web.Pages.Workspace;

public class CalendarModel : PageModel
{
    public string UserFullName { get; set; } = string.Empty;
    public string DeptCode { get; set; } = "MGT";
    public string DeptName { get; set; } = "General";
    public long UserId { get; set; }

    public IActionResult OnGet()
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return RedirectToPage("/Account/Login");

        UserFullName = user.Name;
        DeptCode = user.DeptCode;
        DeptName = user.DeptName;
        UserId = user.UserId;

        return Page();
    }
}
