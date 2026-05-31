using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace erp.minepress.web.Pages.Job;

public class DetailsModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public long Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Mode { get; set; }

    [BindProperty(SupportsGet = true)]
    public long? FromEnquiryId { get; set; }

    [BindProperty(SupportsGet = true)]
    public long? FromQuotationId { get; set; }

    public IActionResult OnGet()
    {
        // Redirect old create mode URLs to the new dedicated Create page
        if (string.Equals(Mode, "create", StringComparison.OrdinalIgnoreCase))
        {
            var url = "/Job/Create";
            if (FromEnquiryId.HasValue)
                url += $"?fromEnquiryId={FromEnquiryId.Value}";
            else if (FromQuotationId.HasValue)
                url += $"?fromQuotationId={FromQuotationId.Value}";
            return Redirect(url);
        }

        return Page();
    }
}
