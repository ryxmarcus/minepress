using erp.minepress.tenants;
using erp.minepress.tenants.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace erp.minepress.webapi.Controllers;

[ApiController]
[Route("api/tenant-diagnostics")]
public class TenantDiagnosticsController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ITenantContextAccessor _tenantContextAccessor;

    public TenantDiagnosticsController(IConfiguration configuration, ITenantContextAccessor tenantContextAccessor)
    {
        _configuration = configuration;
        _tenantContextAccessor = tenantContextAccessor;
    }

    [HttpGet("connection")]
    public IActionResult GetConnectionContext()
    {
        var tenant = _tenantContextAccessor.Current;
        var hasTenantConnection = HttpContext.Items.ContainsKey(TenantConnectionConstants.TenantConnectionStringItemKey);
        var bootstrapConnectionConfigured = !string.IsNullOrWhiteSpace(_configuration.GetConnectionString(TenantConnectionConstants.TenantCatalogConnectionStringKey));

        return Ok(new
        {
            tenantResolved = tenant is not null,
            tenantId = tenant?.TenantId,
            tenantKey = tenant?.TenantKey,
            tenantSource = tenant?.Source,
            hasTenantConnection,
            bootstrapConnectionConfigured,
            effectiveConnectionMode = hasTenantConnection ? "tenant" : "catalog"
        });
    }
}
