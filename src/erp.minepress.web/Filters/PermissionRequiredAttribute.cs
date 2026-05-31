using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using erp.minepress.web.Helpers;

namespace erp.minepress.web.Filters;

/// <summary>
/// Attribute to enforce page-level permission checking using mst_permission codes.
/// Apply to Razor Page models to require specific permission(s).
/// System admins bypass all permission checks.
/// Usage: [PermissionRequired("JOB_VIEW")] or [PermissionRequired("JOB_VIEW", "JOB_CREATE")]
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class PermissionRequiredAttribute : Attribute, IPageFilter
{
    private readonly string[] _permissions;
    private readonly bool _requireAll;

    /// <summary>
    /// Requires the user to have at least one of the specified permission codes.
    /// </summary>
    /// <param name="permissions">One or more permission codes from mst_permission.</param>
    public PermissionRequiredAttribute(params string[] permissions)
    {
        _permissions = permissions;
        _requireAll = false;
    }

    /// <summary>
    /// When true, requires ALL specified permissions. Default is false (any one suffices).
    /// </summary>
    public bool RequireAll
    {
        get => _requireAll;
        init => _requireAll = value;
    }

    public void OnPageHandlerSelected(PageHandlerSelectedContext context) { }

    public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var path = context.HttpContext.Request.Path.Value ?? "";

        // Skip permission check for Account pages
        if (path.StartsWith("/Account", StringComparison.OrdinalIgnoreCase))
            return;

        var user = context.HttpContext.Session.GetCurrentUser();
        if (user == null)
        {
            var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
            context.Result = new RedirectToPageResult("/Account/Login", new { returnUrl });
            return;
        }

        // System admins bypass all permission checks
        if (user.IsSystemAdmin)
            return;

        if (_permissions.Length == 0)
            return;

        bool hasAccess = _requireAll
            ? _permissions.All(p => user.HasPermission(p))
            : _permissions.Any(p => user.HasPermission(p));

        if (!hasAccess)
        {
            context.Result = new RedirectToPageResult("/Account/AccessDenied");
        }
    }

    public void OnPageHandlerExecuted(PageHandlerExecutedContext context) { }
}
