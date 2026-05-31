using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using erp.minepress.web.Helpers;

namespace erp.minepress.web.Pages.Hrms.Leaves;

public class IndexModel : PageModel
{
    public string UserName { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }

    public IActionResult OnGet()
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return RedirectToPage("/Account/Login");

        UserName = user.Name;
        IsAdmin = user.IsSystemAdmin || user.DeptCode == "MGT" || user.DeptCode == "ADM" || user.DeptCode == "HR";

        return Page();
    }
}
