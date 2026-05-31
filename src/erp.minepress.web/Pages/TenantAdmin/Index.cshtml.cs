using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using erp.minepress.web.Helpers;

namespace erp.minepress.web.Pages.TenantAdmin;

public class IndexModel : PageModel
{
    public IActionResult OnGet()
    {
        if (!HttpContext.Session.IsTenantAdminAuthenticated())
            return RedirectToPage("/TenantAdmin/Account/Login");

        return Page();
    }
}
