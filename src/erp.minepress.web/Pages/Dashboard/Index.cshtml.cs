using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using erp.minepress.web.Helpers;

namespace erp.minepress.web.Pages.Dashboard;

public class IndexModel : PageModel
{
    public string DeptCode { get; set; } = "MGT";
    public string DeptName { get; set; } = "General";
    public string UserName { get; set; } = string.Empty;
    public string UserFullName { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
    public bool CanViewAllDepts { get; set; }
    public bool IsProduction { get; set; }
    public bool IsApproval { get; set; }

    public IActionResult OnGet()
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return RedirectToPage("/Account/Login");

        DeptCode = user.DeptCode;
        DeptName = user.DeptName;
        UserName = user.UserCode;
        UserFullName = user.Name;
        IsAdmin = user.IsSystemAdmin;
        CanViewAllDepts = user.IsSystemAdmin || user.DeptCode == "MGT" || user.DeptCode == "ADM";
        IsProduction = user.IsProductionUser;
        IsApproval = user.IsApprovalUser;

        return Page();
    }
}
