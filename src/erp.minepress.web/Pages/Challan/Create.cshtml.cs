using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace erp.minepress.web.Pages.Challan;

public class CreateModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public long? FromJobId { get; set; }

    public void OnGet()
    {
    }
}
