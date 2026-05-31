using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace erp.minepress.web.Pages.Challan;

public class DetailsModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public long Id { get; set; }

    public void OnGet()
    {
    }
}
