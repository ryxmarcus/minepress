using erp.minepress.tenants.Interfaces;
using Microsoft.AspNetCore.Http;

namespace erp.minepress.tenants.Services;

public class TenantContextAccessor : ITenantContextAccessor
{
    private const string TenantContextKey = "TenantContext";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantContextAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public TenantContext? Current => _httpContextAccessor.HttpContext?.Items[TenantContextKey] as TenantContext;

    public void SetCurrent(TenantContext context)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return;

        httpContext.Items[TenantContextKey] = context;
    }
}
