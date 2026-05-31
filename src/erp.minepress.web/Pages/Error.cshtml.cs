using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace erp.minepress.web.Pages;

public class ErrorModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? ErrorId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Message { get; set; }

    public DateTime UtcTimestamp { get; private set; }

    public void OnGet()
    {
        UtcTimestamp = DateTime.UtcNow;

        if (string.IsNullOrWhiteSpace(Message))
        {
            Message = "An unexpected error occurred while processing your request.";
        }
    }
}
