using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using erp.minepress.web.Helpers;

namespace erp.minepress.web.Pages.Hrms.Holiday;

public class IndexModel : PageModel
{
    public IActionResult OnGet()
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return RedirectToPage("/Account/Login");
        return Page();
    }
}
