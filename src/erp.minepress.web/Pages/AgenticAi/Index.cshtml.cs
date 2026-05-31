using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using erp.minepress.web.Helpers;

namespace erp.minepress.web.Pages.AgenticAi;

public class IndexModel : PageModel
{
    public string UserName { get; set; } = string.Empty;
    public string UserFullName { get; set; } = string.Empty;

    public IActionResult OnGet()
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return RedirectToPage("/Account/Login");

        UserName = user.UserCode;
        UserFullName = user.Name;

        return Page();
    }
}
