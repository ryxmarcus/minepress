using erp.minepress.web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace erp.minepress.web.Pages.TenantAdmin;

public class DiagnosticsModel : PageModel
{
    public IActionResult OnGet()
    {
        if (!HttpContext.Session.IsTenantAdminAuthenticated())
            return RedirectToPage("/TenantAdmin/Account/Login");

        return Page();
    }
}
