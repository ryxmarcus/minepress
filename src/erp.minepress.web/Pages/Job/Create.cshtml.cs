using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace erp.minepress.web.Pages.Job;

public class CreateModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public long? FromEnquiryId { get; set; }

    [BindProperty(SupportsGet = true)]
    public long? FromQuotationId { get; set; }

    public void OnGet()
    {
    }
}
