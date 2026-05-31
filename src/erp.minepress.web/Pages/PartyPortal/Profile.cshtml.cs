using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using erp.minepress.web.Helpers;

namespace erp.minepress.web.Pages.PartyPortal;

public class ProfileModel : PageModel
{
    public string PartyName { get; set; } = string.Empty;
    public long? PartyId { get; set; }
    public string UserCode { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];

    public IActionResult OnGet()
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return RedirectToPage("/Account/Login");

        if (!user.IsPartyUser)
            return RedirectToPage("/Dashboard/Index");

        PartyName = user.Name;
        PartyId = user.RefId;
        UserCode = user.UserCode;
        Roles = user.PartyRoles;

        return Page();
    }
}
