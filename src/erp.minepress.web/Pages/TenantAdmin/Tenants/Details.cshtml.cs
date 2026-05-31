using erp.minepress.infrastructure.ErrorLogging;
using erp.minepress.tenants.Interfaces;
using erp.minepress.tenants.Models;
using erp.minepress.web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace erp.minepress.web.Pages.TenantAdmin.Tenants;

public class DetailsModel : PageModel
{
    private readonly ITenantManagementService _tenantManagementService;
    private readonly ISystemErrorLogger _systemErrorLogger;

    public DetailsModel(ITenantManagementService tenantManagementService, ISystemErrorLogger systemErrorLogger)
    {
        _tenantManagementService = tenantManagementService;
        _systemErrorLogger = systemErrorLogger;
    }

    public TenantDetail? Tenant { get; private set; }
    public IReadOnlyList<TenantConnectionDto> Connections { get; private set; } = [];
    public IReadOnlyList<TenantFeatureDto> Features { get; private set; } = [];
    public IReadOnlyList<TenantApiCredentialDto> ApiCredentials { get; private set; } = [];
    public IReadOnlyList<TenantSecurityEventDto> SecurityEvents { get; private set; } = [];

    [BindProperty]
    public TenantUpdateRequest UpdateRequest { get; set; } = new();

    [BindProperty]
    public string ApiScopesJson { get; set; } = "[]";

    [BindProperty]
    public ConnectionInput NewConnection { get; set; } = new();

    [BindProperty]
    public FeatureInput NewFeature { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    [TempData]
    public string? GeneratedApiKey { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        if (!HttpContext.Session.IsTenantAdminAuthenticated())
            return RedirectToPage("/TenantAdmin/Account/Login");

        await LoadAsync(tenantId, cancellationToken);

        if (Tenant == null)
            return RedirectToPage("/TenantAdmin/Tenants/Index");

        UpdateRequest = new TenantUpdateRequest
        {
            TenantId = Tenant.Id,
            Name = Tenant.Name,
            IsActive = Tenant.IsActive,
            SubscriptionPlan = Tenant.SubscriptionPlan,
            SubscriptionExpiresAt = Tenant.SubscriptionExpiresAt,
            MaxUsers = Tenant.MaxUsers,
            EnableIpRestriction = Tenant.EnableIpRestriction,
            RequireTwoFactor = Tenant.RequireTwoFactor,
            DataRetentionDays = Tenant.DataRetentionDays
        };

        return Page();
    }

    public async Task<IActionResult> OnPostUpdateAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        if (!HttpContext.Session.IsTenantAdminAuthenticated())
            return RedirectToPage("/TenantAdmin/Account/Login");

        var admin = HttpContext.Session.GetTenantAdmin();
        if (admin == null)
            return RedirectToPage("/TenantAdmin/Account/Login");

        UpdateRequest = UpdateRequest with { TenantId = tenantId };
        var updated = await _tenantManagementService.UpdateTenantAsync(UpdateRequest, admin.UserCode, cancellationToken);
        StatusMessage = updated ? "Tenant updated successfully." : "No tenant record was updated.";

        return RedirectToPage(new { tenantId });
    }

    public async Task<IActionResult> OnPostAddConnectionAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        if (!HttpContext.Session.IsTenantAdminAuthenticated())
            return RedirectToPage("/TenantAdmin/Account/Login");

        var admin = HttpContext.Session.GetTenantAdmin();
        if (admin == null)
            return RedirectToPage("/TenantAdmin/Account/Login");

        await _tenantManagementService.AddConnectionAsync(new CreateTenantConnectionRequest
        {
            TenantId = tenantId,
            ConnectionType = NewConnection.ConnectionType,
            ConnectionString = NewConnection.ConnectionString,
            IsActive = NewConnection.IsActive,
            Priority = NewConnection.Priority,
            Actor = admin.UserCode
        }, cancellationToken);

        StatusMessage = "Tenant connection added.";
        return RedirectToPage(new { tenantId });
    }

    public async Task<IActionResult> OnPostAddFeatureAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        if (!HttpContext.Session.IsTenantAdminAuthenticated())
            return RedirectToPage("/TenantAdmin/Account/Login");

        var admin = HttpContext.Session.GetTenantAdmin();
        if (admin == null)
            return RedirectToPage("/TenantAdmin/Account/Login");

        var tenant = await _tenantManagementService.GetTenantAsync(tenantId, cancellationToken);
        if (tenant == null)
            return RedirectToPage("/TenantAdmin/Tenants/Index");

        await _tenantManagementService.AddFeatureAsync(new CreateTenantFeatureRequest
        {
            TenantId = tenantId,
            TenantKey = tenant.TenantKey,
            FeatureName = NewFeature.FeatureName,
            IsEnabled = NewFeature.IsEnabled,
            ConfigurationJson = NewFeature.ConfigurationJson,
            Actor = admin.UserCode
        }, cancellationToken);

        StatusMessage = "Feature flag saved.";
        return RedirectToPage(new { tenantId });
    }

    public async Task<IActionResult> OnPostIssueApiKeyAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        if (!HttpContext.Session.IsTenantAdminAuthenticated())
            return RedirectToPage("/TenantAdmin/Account/Login");

        var admin = HttpContext.Session.GetTenantAdmin();
        if (admin == null)
            return RedirectToPage("/TenantAdmin/Account/Login");

        var created = await _tenantManagementService.CreateApiCredentialAsync(tenantId, ApiScopesJson, admin.UserCode, cancellationToken);
        GeneratedApiKey = created.PlainApiKey;
        StatusMessage = $"API key issued. Prefix: {created.KeyPrefix}";

        return RedirectToPage(new { tenantId });
    }

    public async Task<IActionResult> OnPostRevokeApiKeyAsync(Guid tenantId, Guid credentialId, CancellationToken cancellationToken)
    {
        if (!HttpContext.Session.IsTenantAdminAuthenticated())
            return RedirectToPage("/TenantAdmin/Account/Login");

        var admin = HttpContext.Session.GetTenantAdmin();
        if (admin == null)
            return RedirectToPage("/TenantAdmin/Account/Login");

        var revoked = await _tenantManagementService.RevokeApiCredentialAsync(credentialId, admin.UserCode, cancellationToken);
        StatusMessage = revoked ? "API credential revoked." : "Credential not found or already inactive.";

        return RedirectToPage(new { tenantId });
    }

    private async Task LoadAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        Tenant = await _tenantManagementService.GetTenantAsync(tenantId, cancellationToken);
        if (Tenant == null)
            return;

        Connections = await _tenantManagementService.GetConnectionsAsync(tenantId, cancellationToken);
        Features = await _tenantManagementService.GetFeaturesAsync(tenantId, cancellationToken);

        try
        {
            ApiCredentials = await _tenantManagementService.GetApiCredentialsAsync(tenantId, cancellationToken);
            SecurityEvents = await _tenantManagementService.GetSecurityEventsAsync(tenantId, cancellationToken);
        }
        catch
        {
            ApiCredentials = [];
            SecurityEvents = [];
        }
    }

    public sealed class ConnectionInput
    {
        public string ConnectionType { get; set; } = "secondary";
        public string ConnectionString { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int Priority { get; set; } = 2;
    }

    public sealed class FeatureInput
    {
        public string FeatureName { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = true;
        public string ConfigurationJson { get; set; } = "{}";
    }
}
