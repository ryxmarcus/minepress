using Microsoft.AspNetCore.Mvc.RazorPages;

namespace erp.minepress.web.Pages.Maintenance.Party;

public class CreateModel : PageModel
{
    public int? PartyId { get; set; }

    public void OnGet(int? id)
    {
        PartyId = id;
    }
}
