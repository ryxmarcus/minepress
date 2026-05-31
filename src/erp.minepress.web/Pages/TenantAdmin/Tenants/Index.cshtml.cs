using erp.minepress.infrastructure.ErrorLogging;
using erp.minepress.tenants.Interfaces;
using erp.minepress.tenants.Models;
using erp.minepress.web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace erp.minepress.web.Pages.TenantAdmin.Tenants;

public class IndexModel : PageModel
{
    private readonly ITenantManagementService _tenantManagementService;
    private readonly ISystemErrorLogger _systemErrorLogger;

    public IndexModel(ITenantManagementService tenantManagementService, ISystemErrorLogger systemErrorLogger)
    {
        _tenantManagementService = tenantManagementService;
        _systemErrorLogger = systemErrorLogger;
    }

    public IReadOnlyList<TenantListItem> Tenants { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        if (!HttpContext.Session.IsTenantAdminAuthenticated())
            return RedirectToPage("/TenantAdmin/Account/Login");

        Tenants = await _tenantManagementService.GetTenantsAsync(cancellationToken);
        return Page();
    }
}
