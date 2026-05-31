using erp.minepress.tenants.Interfaces;
using Microsoft.AspNetCore.Http;

namespace erp.minepress.tenants.Middleware;

public class TenantConnectionContextMiddleware
{
    private readonly RequestDelegate _next;

    public TenantConnectionContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantResolver tenantResolver, ITenantContextAccessor tenantContextAccessor)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        if (IsExcludedPath(path))
        {
            await _next(context);
            return;
        }

        var tenantKey = ResolveTenantKey(context);
        if (string.IsNullOrWhiteSpace(tenantKey))
        {
            await _next(context);
            return;
        }

        var tenant = await tenantResolver.ResolveTenantAsync(tenantKey, context.RequestAborted);
        if (tenant == null || !tenant.IsActive)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Invalid or inactive tenant.");
            return;
        }

        var claimTenantId = context.User.FindFirst("tenant_id")?.Value;
        var claimTenantKey = context.User.FindFirst("tenant_key")?.Value;

        if (!string.IsNullOrWhiteSpace(claimTenantKey)
            && !string.Equals(claimTenantKey, tenant.TenantKey, StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Tenant claim mismatch.");
            return;
        }

        if (!string.IsNullOrWhiteSpace(claimTenantId)
            && Guid.TryParse(claimTenantId, out var tenantIdClaim)
            && tenantIdClaim != tenant.TenantId)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Tenant id claim mismatch.");
            return;
        }

        tenantContextAccessor.SetCurrent(new TenantContext
        {
            TenantId = tenant.TenantId,
            TenantKey = tenant.TenantKey,
            TenantName = tenant.TenantName,
            Source = ResolveSource(context)
        });

        context.Items[TenantConnectionConstants.TenantIdItemKey] = tenant.TenantId;
        context.Items[TenantConnectionConstants.TenantKeyItemKey] = tenant.TenantKey;
        context.Items[TenantConnectionConstants.TenantConnectionStringItemKey] = tenant.ConnectionString;

        await _next(context);
    }

    private static string? ResolveTenantKey(HttpContext context)
    {
        var claimTenantKey = context.User.FindFirst("tenant_key")?.Value;
        if (!string.IsNullOrWhiteSpace(claimTenantKey))
            return claimTenantKey;

        if (context.Request.Headers.TryGetValue("X-Tenant-Key", out var headerTenantKey)
            && !string.IsNullOrWhiteSpace(headerTenantKey))
        {
            return headerTenantKey.ToString();
        }

        var host = context.Request.Host.Host;
        if (!string.IsNullOrWhiteSpace(host))
        {
            var segments = host.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length >= 3)
                return segments[0];
        }

        return null;
    }

    private static string ResolveSource(HttpContext context)
    {
        if (context.User.HasClaim(c => c.Type == "tenant_key"))
            return "jwt";

        if (context.Request.Headers.ContainsKey("X-Tenant-Key"))
            return "header";

        return "subdomain";
    }

    private static bool IsExcludedPath(string path)
    {
        return path.StartsWith("/account", StringComparison.OrdinalIgnoreCase)
               || path.StartsWith("/tenantadmin", StringComparison.OrdinalIgnoreCase)
               || path.StartsWith("/display", StringComparison.OrdinalIgnoreCase)
               || path.StartsWith("/health", StringComparison.OrdinalIgnoreCase)
               || path.StartsWith("/dist", StringComparison.OrdinalIgnoreCase)
               || path.StartsWith("/css", StringComparison.OrdinalIgnoreCase)
               || path.StartsWith("/js", StringComparison.OrdinalIgnoreCase)
               || path.StartsWith("/lib", StringComparison.OrdinalIgnoreCase)
               || path.StartsWith("/favicon", StringComparison.OrdinalIgnoreCase)
               || path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase)
               || path.StartsWith("/openapi", StringComparison.OrdinalIgnoreCase);
    }
}
