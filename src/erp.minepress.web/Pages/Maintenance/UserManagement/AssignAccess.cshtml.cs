using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace erp.minepress.web.Pages.Maintenance.UserManagement;

public class AssignAccessModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public long Id { get; set; }

    public IActionResult OnGet()
    {
        if (Id <= 0)
            return RedirectToPage("Index");

        return Page();
    }
}
