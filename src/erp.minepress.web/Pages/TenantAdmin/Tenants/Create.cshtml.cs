using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using erp.minepress.infrastructure.ErrorLogging;
using erp.minepress.tenants.Interfaces;
using erp.minepress.tenants.Models;
using erp.minepress.web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace erp.minepress.web.Pages.TenantAdmin.Tenants;

public class CreateModel : PageModel
{
    private readonly ITenantManagementService _tenantManagementService;
    private readonly ISystemErrorLogger _systemErrorLogger;

    public CreateModel(ITenantManagementService tenantManagementService, ISystemErrorLogger systemErrorLogger)
    {
        _tenantManagementService = tenantManagementService;
        _systemErrorLogger = systemErrorLogger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public IActionResult OnGet()
    {
        if (!HttpContext.Session.IsTenantAdminAuthenticated())
            return RedirectToPage("/TenantAdmin/Account/Login");

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!HttpContext.Session.IsTenantAdminAuthenticated())
            return RedirectToPage("/TenantAdmin/Account/Login");

        if (!ModelState.IsValid)
            return Page();

        var admin = HttpContext.Session.GetTenantAdmin();
        if (admin == null)
            return RedirectToPage("/TenantAdmin/Account/Login");

        if (string.IsNullOrWhiteSpace(Input.TenantKey))
        {
            Input.TenantKey = GenerateTenantKey(Input.Name);
        }

        var tenantId = await _tenantManagementService.CreateTenantAsync(new CreateTenantRequest
        {
            TenantKey = Input.TenantKey,
            Name = Input.Name,
            ConnectionString = Input.ConnectionString,
            SubscriptionPlan = Input.SubscriptionPlan,
            MaxUsers = Input.MaxUsers,
            CreatedBy = admin.UserCode
        }, cancellationToken);

        StatusMessage = "Tenant created successfully.";
        return RedirectToPage("/TenantAdmin/Tenants/Details", new { tenantId });
    }

    private static string GenerateTenantKey(string? name)
    {
        var baseKey = Regex.Replace(name ?? "tenant", "[^a-zA-Z0-9]+", "-")
            .Trim('-')
            .ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(baseKey))
            baseKey = "tenant";

        var suffix = Guid.NewGuid().ToString("N")[..6];
        return $"{baseKey}-{suffix}";
    }

    public sealed class InputModel
    {
        [Required]
        public string TenantKey { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string ConnectionString { get; set; } = string.Empty;

        [Required]
        public string SubscriptionPlan { get; set; } = "starter";

        [Range(1, 100000)]
        public int MaxUsers { get; set; } = 10;
    }
}
