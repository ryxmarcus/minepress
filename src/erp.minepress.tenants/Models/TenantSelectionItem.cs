namespace erp.minepress.tenants.Models;

public record TenantSelectionItem
{
    public Guid Id { get; init; }
    public string TenantKey { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
}
