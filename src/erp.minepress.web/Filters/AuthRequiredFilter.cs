using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using erp.minepress.web.Helpers;

namespace erp.minepress.web.Filters;

/// <summary>
/// Page filter that redirects unauthenticated users to the login page.
/// Pages under /Account are excluded from the check.
/// </summary>
public class AuthRequiredFilter : IPageFilter
{
    public void OnPageHandlerSelected(PageHandlerSelectedContext context) { }

    public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var path = context.HttpContext.Request.Path.Value ?? "";

        // Allow access to Account pages (Login, Logout) without session
        if (path.StartsWith("/Account", StringComparison.OrdinalIgnoreCase))
            return;

        // Allow access to the default root page so unauthenticated users can select tenant
        if (string.Equals(path, "/", StringComparison.OrdinalIgnoreCase)
            || string.Equals(path, "/Index", StringComparison.OrdinalIgnoreCase))
            return;

        // Allow access to TenantAdmin login pages without tenant-admin session
        if (path.StartsWith("/TenantAdmin/Account", StringComparison.OrdinalIgnoreCase))
            return;

        // Allow access to Helpdesk TV Display without session
        if (path.StartsWith("/Display", StringComparison.OrdinalIgnoreCase))
            return;

        // Allow access to global error page without session
        if (path.StartsWith("/Error", StringComparison.OrdinalIgnoreCase))
            return;

        // Tenant admin area uses dedicated admin session
        if (path.StartsWith("/TenantAdmin", StringComparison.OrdinalIgnoreCase))
        {
            var isTenantAdminAuthenticated = context.HttpContext.Session.IsTenantAdminAuthenticated();
            if (!isTenantAdminAuthenticated)
            {
                var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                context.Result = new RedirectToPageResult("/TenantAdmin/Account/Login", new { returnUrl });
            }

            return;
        }

        var isAuthenticated = context.HttpContext.Session.IsAuthenticated();
        if (!isAuthenticated)
        {
            var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
            context.Result = new RedirectToPageResult("/Account/Login", new { returnUrl });
        }
    }

    public void OnPageHandlerExecuted(PageHandlerExecutedContext context) { }
}
