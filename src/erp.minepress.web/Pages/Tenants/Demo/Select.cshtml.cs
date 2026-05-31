using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace erp.minepress.web.Pages.Tenants.Demo;

public class SelectModel : PageModel
{
    // Demo tenant values (set per-tenant)
    private static readonly Guid TenantId = new Guid("eaa62304-363f-42a9-bedb-14f140eeb6bc");
    private const string TenantKey = "tenant-5sbo62";

    public IActionResult OnGet()
    {
        var tokenService = HttpContext.RequestServices.GetRequiredService<erp.minepress.tenants.Interfaces.ITenantTokenService>();
        var token = tokenService.CreateToken(TenantId, TenantKey);
        return RedirectToPage("/Tenants/Demo/Landing", new { d = token });
    }
}
