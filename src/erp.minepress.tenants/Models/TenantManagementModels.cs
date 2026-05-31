namespace erp.minepress.tenants.Models;

public sealed record TenantListItem
{
    public Guid Id { get; init; }
    public string TenantKey { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public string? SubscriptionPlan { get; init; }
    public int MaxUsers { get; init; }
    public int CurrentUsers { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? LastActivityAt { get; init; }
}

public sealed record TenantDetail
{
    public Guid Id { get; init; }
    public string TenantKey { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public string? SubscriptionPlan { get; init; }
    public DateTimeOffset? SubscriptionExpiresAt { get; init; }
    public int MaxUsers { get; init; }
    public int CurrentUsers { get; init; }
    public bool EnableIpRestriction { get; init; }
    public bool RequireTwoFactor { get; init; }
    public int DataRetentionDays { get; init; }
    public string? SchemaName { get; init; }
    public string? SuspensionReason { get; init; }
    public DateTimeOffset? SuspendedAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}

public sealed record TenantUpdateRequest
{
    public Guid TenantId { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public string? SubscriptionPlan { get; init; }
    public DateTimeOffset? SubscriptionExpiresAt { get; init; }
    public int MaxUsers { get; init; }
    public bool EnableIpRestriction { get; init; }
    public bool RequireTwoFactor { get; init; }
    public int DataRetentionDays { get; init; }
}

public sealed record CreateTenantRequest
{
    public string TenantKey { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string ConnectionString { get; init; } = string.Empty;
    public string SubscriptionPlan { get; init; } = "starter";
    public int MaxUsers { get; init; } = 10;
    public string CreatedBy { get; init; } = string.Empty;
}

public sealed record CreateTenantConnectionRequest
{
    public Guid TenantId { get; init; }
    public string ConnectionType { get; init; } = "primary";
    public string ConnectionString { get; init; } = string.Empty;
    public bool IsActive { get; init; } = true;
    public int Priority { get; init; } = 1;
    public string Actor { get; init; } = string.Empty;
}

public sealed record CreateTenantFeatureRequest
{
    public Guid TenantId { get; init; }
    public string TenantKey { get; init; } = string.Empty;
    public string FeatureName { get; init; } = string.Empty;
    public bool IsEnabled { get; init; } = true;
    public string ConfigurationJson { get; init; } = "{}";
    public string Actor { get; init; } = string.Empty;
}

public sealed record TenantConnectionDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string ConnectionType { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public int Priority { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed record TenantFeatureDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string TenantKey { get; init; } = string.Empty;
    public string FeatureName { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }
    public string Configuration { get; init; } = "{}";
    public DateTimeOffset? EnabledAt { get; init; }
    public DateTimeOffset? DisabledAt { get; init; }
}

public sealed record TenantApiCredentialDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string KeyPrefix { get; init; } = string.Empty;
    public string Scopes { get; init; } = "[]";
    public DateTimeOffset ExpiresAt { get; init; }
    public DateTimeOffset? LastUsedAt { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed record CreatedTenantApiCredential
{
    public Guid Id { get; init; }
    public string PlainApiKey { get; init; } = string.Empty;
    public string KeyPrefix { get; init; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; init; }
}

public sealed record TenantSecurityEventDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string EventType { get; init; } = string.Empty;
    public string Severity { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? SourceIp { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
