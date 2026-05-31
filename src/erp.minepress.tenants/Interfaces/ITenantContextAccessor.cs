namespace erp.minepress.tenants.Interfaces;

public interface ITenantContextAccessor
{
    TenantContext? Current { get; }
    void SetCurrent(TenantContext context);
}

public sealed record TenantContext
{
    public Guid TenantId { get; init; }
    public string TenantKey { get; init; } = string.Empty;
    public string TenantName { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty;
}
