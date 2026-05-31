namespace erp.minepress.tenants.Interfaces;

public interface ITenantResolver
{
    string GetCurrentTenantKey();
    Task<TenantInfo?> ResolveTenantAsync(string tenantKey, CancellationToken cancellationToken = default);
}

public record TenantInfo
{
    public Guid TenantId { get; init; }
    public string TenantKey { get; init; } = string.Empty;
    public string TenantName { get; init; } = string.Empty;
    public string ConnectionString { get; init; } = string.Empty;
    public string SchemaName { get; init; } = "public";
    public bool IsActive { get; init; } = true;
}
