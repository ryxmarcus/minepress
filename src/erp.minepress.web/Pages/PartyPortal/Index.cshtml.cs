using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using erp.minepress.web.Helpers;

namespace erp.minepress.web.Pages.PartyPortal;

public class IndexModel : PageModel
{
    public string PartyName { get; set; } = string.Empty;
    public long? PartyId { get; set; }
    public List<string> Roles { get; set; } = [];

    public bool IsCustomer => Roles.Contains("Customer");
    public bool IsSupplier => Roles.Contains("Supplier");
    public bool IsVendor => Roles.Contains("Vendor");
    public int RoleCount => Roles.Count;

    public IActionResult OnGet()
    {
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null)
            return RedirectToPage("/Account/Login");

        if (!user.IsPartyUser)
            return RedirectToPage("/Dashboard/Index");

        PartyName = user.Name;
        PartyId = user.RefId;
        Roles = user.PartyRoles;

        return Page();
    }
}
