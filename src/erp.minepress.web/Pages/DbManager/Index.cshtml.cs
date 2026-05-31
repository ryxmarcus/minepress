using erp.minepress.web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace erp.minepress.web.Pages.DbManager;

public class IndexModel : PageModel
{
    public IActionResult OnGet()
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user?.IsSystemAdmin != true)
            return RedirectToPage("/Dashboard/Index");

        return Page();
    }
}
