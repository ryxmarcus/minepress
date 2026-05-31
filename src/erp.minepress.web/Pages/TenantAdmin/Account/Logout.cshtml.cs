using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using erp.minepress.web.Helpers;

namespace erp.minepress.web.Pages.TenantAdmin.Account;

public class LogoutModel : PageModel
{
    public IActionResult OnGet()
    {
        HttpContext.Session.ClearTenantAdmin();
        return RedirectToPage("/TenantAdmin/Account/Login");
    }
}
