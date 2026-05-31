using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace erp.minepress.web.Pages.Quotation;

public class DetailsModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public long Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Mode { get; set; }

    [BindProperty(SupportsGet = true)]
    public long? FromEnquiryId { get; set; }

    public IActionResult OnGet()
    {
        // Redirect old create mode URLs to the new dedicated Create page
        if (string.Equals(Mode, "create", StringComparison.OrdinalIgnoreCase))
        {
            var url = FromEnquiryId.HasValue
                ? $"/Quotation/Create?fromEnquiryId={FromEnquiryId.Value}"
                : "/Quotation/Create";
            return Redirect(url);
        }

        return Page();
    }
}
