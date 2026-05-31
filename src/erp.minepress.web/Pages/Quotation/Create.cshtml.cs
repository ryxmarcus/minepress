using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace erp.minepress.web.Pages.Quotation;

public class CreateModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public long? FromEnquiryId { get; set; }

    public void OnGet()
    {
    }
}
