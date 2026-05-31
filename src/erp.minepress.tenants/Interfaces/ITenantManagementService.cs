using erp.minepress.tenants.Models;

namespace erp.minepress.tenants.Interfaces;

public interface ITenantManagementService
{
    Task<IReadOnlyList<TenantListItem>> GetTenantsAsync(CancellationToken cancellationToken = default);
    Task<Guid> CreateTenantAsync(CreateTenantRequest request, CancellationToken cancellationToken = default);
    Task<TenantDetail?> GetTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<bool> UpdateTenantAsync(TenantUpdateRequest request, string actor, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TenantConnectionDto>> GetConnectionsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<Guid> AddConnectionAsync(CreateTenantConnectionRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TenantFeatureDto>> GetFeaturesAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<Guid> AddFeatureAsync(CreateTenantFeatureRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TenantApiCredentialDto>> GetApiCredentialsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<CreatedTenantApiCredential> CreateApiCredentialAsync(Guid tenantId, string scopesJson, string actor, CancellationToken cancellationToken = default);
    Task<bool> RevokeApiCredentialAsync(Guid credentialId, string actor, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TenantSecurityEventDto>> GetSecurityEventsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task LogSecurityEventAsync(Guid tenantId, string eventType, string severity, string description, string? sourceIp, CancellationToken cancellationToken = default);
}
