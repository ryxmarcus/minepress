using erp.minepress.tenants.Interfaces;
using erp.minepress.tenants.Middleware;

namespace erp.minepress.web.Middleware;

public class TenantContextMiddleware : TenantConnectionContextMiddleware
{
    public TenantContextMiddleware(RequestDelegate next) : base(next)
    {
    }
}
