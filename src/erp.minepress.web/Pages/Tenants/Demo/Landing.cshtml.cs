using erp.minepress.infrastructure.ErrorLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using erp.minepress.tenants.Interfaces;
using System.Text.Json;

namespace erp.minepress.web.Pages.Tenants.Demo;

public class LandingModel : PageModel
{
    private readonly ITenantResolver _tenantResolver;
    private readonly ITenantContextAccessor _tenantContextAccessor;
    private readonly ISystemErrorLogger _systemErrorLogger;

    public LandingModel(ITenantResolver tenantResolver, ITenantContextAccessor tenantContextAccessor, ISystemErrorLogger systemErrorLogger)
    {
        _tenantResolver = tenantResolver;
        _tenantContextAccessor = tenantContextAccessor;
        _systemErrorLogger = systemErrorLogger;
    }

    public string? Message { get; set; }

    public async Task<IActionResult> OnGetAsync(string? d)
    {
        if (string.IsNullOrWhiteSpace(d))
            return RedirectToPage("/Tenants/Select");

            try
            {
                var tokenService = HttpContext.RequestServices.GetRequiredService<erp.minepress.tenants.Interfaces.ITenantTokenService>();
                if (!tokenService.ValidateToken(d, out var id, out var key))
                    return RedirectToPage("/Tenants/Select");

                if (string.IsNullOrWhiteSpace(key))
                    return RedirectToPage("/Tenants/Select");

                var tenant = await _tenantResolver.ResolveTenantAsync(key);
                if (tenant == null)
                    return RedirectToPage("/Tenants/Select");

            // Set tenant context for this request
            _tenantContextAccessor.SetCurrent(new erp.minepress.tenants.Interfaces.TenantContext
            {
                TenantId = tenant.TenantId,
                TenantKey = tenant.TenantKey,
                TenantName = tenant.TenantName,
                Source = "landing"
            });

            // Create signed short-lived token and place in TempData
            var tokenCreated = tokenService.CreateToken(tenant.TenantId, tenant.TenantKey);
            TempData["TenantToken"] = tokenCreated;

            return RedirectToPage("/Account/Login");
        }
        catch
        {
            return RedirectToPage("/Tenants/Select");
        }
    }
}
