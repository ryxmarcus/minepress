using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace erp.minepress.web.Pages.Outsource;

public class PrintModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public long Id { get; set; }

    public void OnGet()
    {
    }
}
