using erp.minepress.tenants.Interfaces;
using erp.minepress.tenants.Models;
using erp.minepress.tenants.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Npgsql;

namespace erp.minepress.tenants.Services;

public class TenantManagementService : ITenantManagementService
{
    private readonly string _connectionString;
    private readonly ITenantSecurityService _securityService;
    private readonly TenantSecurityOptions _securityOptions;

    public TenantManagementService(
        IConfiguration configuration,
        ITenantSecurityService securityService,
        IOptions<TenantSecurityOptions> securityOptions)
    {
        _connectionString = configuration.GetTenantCatalogConnectionString();
        _securityService = securityService;
        _securityOptions = securityOptions.Value;
    }

    public async Task<IReadOnlyList<TenantListItem>> GetTenantsAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT id, tenant_key, name, is_active, subscription_plan, max_users, current_users, created_at, last_activity_at
FROM minepress_db.tenants
ORDER BY created_at DESC;";

        var items = new List<TenantListItem>();
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new TenantListItem
            {
                Id = reader.GetGuid(0),
                TenantKey = reader.GetString(1),
                Name = reader.GetString(2),
                IsActive = reader.GetBoolean(3),
                SubscriptionPlan = reader.IsDBNull(4) ? null : reader.GetString(4),
                MaxUsers = reader.GetInt32(5),
                CurrentUsers = reader.GetInt32(6),
                CreatedAt = reader.GetFieldValue<DateTimeOffset>(7),
                LastActivityAt = reader.IsDBNull(8) ? null : reader.GetFieldValue<DateTimeOffset>(8)
            });
        }

        return items;
    }

    public async Task<Guid> CreateTenantAsync(CreateTenantRequest request, CancellationToken cancellationToken = default)
    {
        const string tenantSql = @"
INSERT INTO minepress_db.tenants
(id, tenant_key, name, encrypted_connection_string, schema_name, is_active, subscription_plan, max_users, created_at, created_by)
VALUES
(gen_random_uuid(), @tenantKey, @name, @encryptedConnectionString, 'public', TRUE, @subscriptionPlan, @maxUsers, CURRENT_TIMESTAMP, @createdBy)
RETURNING id;";

        const string connectionSql = @"
INSERT INTO minepress_db.tenant_connections
(id, tenant_id, connection_type, encrypted_connection_string, is_active, priority, created_at)
VALUES
(gen_random_uuid(), @tenantId, 'primary', @encryptedConnectionString, TRUE, 1, CURRENT_TIMESTAMP);";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        var normalizedTenantKey = request.TenantKey.Trim().ToLowerInvariant();
        var encryptedConnectionString = _securityService.Encrypt(request.ConnectionString);

        await using var tenantCommand = new NpgsqlCommand(tenantSql, connection, transaction);
        tenantCommand.Parameters.AddWithValue("tenantKey", normalizedTenantKey);
        tenantCommand.Parameters.AddWithValue("name", request.Name.Trim());
        tenantCommand.Parameters.AddWithValue("encryptedConnectionString", encryptedConnectionString);
        tenantCommand.Parameters.AddWithValue("subscriptionPlan", request.SubscriptionPlan.Trim());
        tenantCommand.Parameters.AddWithValue("maxUsers", request.MaxUsers);
        tenantCommand.Parameters.AddWithValue("createdBy", request.CreatedBy);

        var tenantId = (Guid)(await tenantCommand.ExecuteScalarAsync(cancellationToken) ?? Guid.Empty);

        await using var connectionCommand = new NpgsqlCommand(connectionSql, connection, transaction);
        connectionCommand.Parameters.AddWithValue("tenantId", tenantId);
        connectionCommand.Parameters.AddWithValue("encryptedConnectionString", encryptedConnectionString);
        await connectionCommand.ExecuteNonQueryAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        return tenantId;
    }

    public async Task<TenantDetail?> GetTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT id, tenant_key, name, is_active, subscription_plan, subscription_expires_at, max_users, current_users,
       enable_ip_restriction, require_two_factor, data_retention_days, schema_name, suspension_reason, suspended_at,
       created_at, updated_at
FROM minepress_db.tenants
WHERE id = @tenantId;";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("tenantId", tenantId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return new TenantDetail
        {
            Id = reader.GetGuid(0),
            TenantKey = reader.GetString(1),
            Name = reader.GetString(2),
            IsActive = reader.GetBoolean(3),
            SubscriptionPlan = reader.IsDBNull(4) ? null : reader.GetString(4),
            SubscriptionExpiresAt = reader.IsDBNull(5) ? null : reader.GetFieldValue<DateTimeOffset>(5),
            MaxUsers = reader.GetInt32(6),
            CurrentUsers = reader.GetInt32(7),
            EnableIpRestriction = reader.GetBoolean(8),
            RequireTwoFactor = reader.GetBoolean(9),
            DataRetentionDays = reader.GetInt32(10),
            SchemaName = reader.IsDBNull(11) ? null : reader.GetString(11),
            SuspensionReason = reader.IsDBNull(12) ? null : reader.GetString(12),
            SuspendedAt = reader.IsDBNull(13) ? null : reader.GetFieldValue<DateTimeOffset>(13),
            CreatedAt = reader.GetFieldValue<DateTimeOffset>(14),
            UpdatedAt = reader.IsDBNull(15) ? null : reader.GetFieldValue<DateTimeOffset>(15)
        };
    }

    public async Task<bool> UpdateTenantAsync(TenantUpdateRequest request, string actor, CancellationToken cancellationToken = default)
    {
        const string sql = @"
UPDATE minepress_db.tenants
SET name = @name,
    is_active = @isActive,
    subscription_plan = @subscriptionPlan,
    subscription_expires_at = @subscriptionExpiresAt,
    max_users = @maxUsers,
    enable_ip_restriction = @enableIpRestriction,
    require_two_factor = @requireTwoFactor,
    data_retention_days = @dataRetentionDays,
    updated_at = CURRENT_TIMESTAMP,
    updated_by = @actor
WHERE id = @tenantId;";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("name", request.Name);
        command.Parameters.AddWithValue("isActive", request.IsActive);
        command.Parameters.AddWithValue("subscriptionPlan", (object?)request.SubscriptionPlan ?? DBNull.Value);
        command.Parameters.AddWithValue("subscriptionExpiresAt", (object?)request.SubscriptionExpiresAt ?? DBNull.Value);
        command.Parameters.AddWithValue("maxUsers", request.MaxUsers);
        command.Parameters.AddWithValue("enableIpRestriction", request.EnableIpRestriction);
        command.Parameters.AddWithValue("requireTwoFactor", request.RequireTwoFactor);
        command.Parameters.AddWithValue("dataRetentionDays", request.DataRetentionDays);
        command.Parameters.AddWithValue("actor", actor);
        command.Parameters.AddWithValue("tenantId", request.TenantId);

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        return affected > 0;
    }

    public async Task<IReadOnlyList<TenantConnectionDto>> GetConnectionsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT id, tenant_id, connection_type, is_active, priority, created_at
FROM minepress_db.tenant_connections
WHERE tenant_id = @tenantId
ORDER BY priority ASC, created_at DESC;";

        var items = new List<TenantConnectionDto>();
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("tenantId", tenantId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new TenantConnectionDto
            {
                Id = reader.GetGuid(0),
                TenantId = reader.GetGuid(1),
                ConnectionType = reader.GetString(2),
                IsActive = reader.GetBoolean(3),
                Priority = reader.GetInt32(4),
                CreatedAt = reader.GetFieldValue<DateTimeOffset>(5)
            });
        }

        return items;
    }

    public async Task<Guid> AddConnectionAsync(CreateTenantConnectionRequest request, CancellationToken cancellationToken = default)
    {
        const string sql = @"
INSERT INTO minepress_db.tenant_connections
(id, tenant_id, connection_type, encrypted_connection_string, is_active, priority, created_at)
VALUES
(gen_random_uuid(), @tenantId, @connectionType, @encryptedConnectionString, @isActive, @priority, CURRENT_TIMESTAMP)
RETURNING id;";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("tenantId", request.TenantId);
        command.Parameters.AddWithValue("connectionType", request.ConnectionType.Trim().ToLowerInvariant());
        command.Parameters.AddWithValue("encryptedConnectionString", _securityService.Encrypt(request.ConnectionString));
        command.Parameters.AddWithValue("isActive", request.IsActive);
        command.Parameters.AddWithValue("priority", request.Priority);

        var id = (Guid)(await command.ExecuteScalarAsync(cancellationToken) ?? Guid.Empty);
        await LogSecurityEventAsync(request.TenantId, "TENANT_CONNECTION_ADDED", "INFO", $"Connection added by {request.Actor}", null, cancellationToken);
        return id;
    }

    public async Task<IReadOnlyList<TenantFeatureDto>> GetFeaturesAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT id, tenant_id, tenant_key, feature_name, is_enabled, configuration::text, enabled_at, disabled_at
FROM minepress_db.tenant_features
WHERE tenant_id = @tenantId
ORDER BY feature_name;";

        var items = new List<TenantFeatureDto>();
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("tenantId", tenantId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new TenantFeatureDto
            {
                Id = reader.GetGuid(0),
                TenantId = reader.GetGuid(1),
                TenantKey = reader.GetString(2),
                FeatureName = reader.GetString(3),
                IsEnabled = reader.GetBoolean(4),
                Configuration = reader.GetString(5),
                EnabledAt = reader.IsDBNull(6) ? null : reader.GetFieldValue<DateTimeOffset>(6),
                DisabledAt = reader.IsDBNull(7) ? null : reader.GetFieldValue<DateTimeOffset>(7)
            });
        }

        return items;
    }

    public async Task<Guid> AddFeatureAsync(CreateTenantFeatureRequest request, CancellationToken cancellationToken = default)
    {
        const string sql = @"
INSERT INTO minepress_db.tenant_features
(id, tenant_id, tenant_key, feature_name, is_enabled, configuration, enabled_at, created_at, created_by)
VALUES
(gen_random_uuid(), @tenantId, @tenantKey, @featureName, @isEnabled, @configuration::jsonb,
 CASE WHEN @isEnabled THEN CURRENT_TIMESTAMP ELSE NULL END,
 CURRENT_TIMESTAMP, @actor)
ON CONFLICT (tenant_id, feature_name)
DO UPDATE SET
    is_enabled = EXCLUDED.is_enabled,
    configuration = EXCLUDED.configuration,
    enabled_at = CASE WHEN EXCLUDED.is_enabled THEN CURRENT_TIMESTAMP ELSE minepress_db.tenant_features.enabled_at END,
    disabled_at = CASE WHEN EXCLUDED.is_enabled THEN NULL ELSE CURRENT_TIMESTAMP END,
    updated_at = CURRENT_TIMESTAMP,
    updated_by = EXCLUDED.created_by
RETURNING id;";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("tenantId", request.TenantId);
        command.Parameters.AddWithValue("tenantKey", request.TenantKey);
        command.Parameters.AddWithValue("featureName", request.FeatureName.Trim());
        command.Parameters.AddWithValue("isEnabled", request.IsEnabled);
        command.Parameters.AddWithValue("configuration", string.IsNullOrWhiteSpace(request.ConfigurationJson) ? "{}" : request.ConfigurationJson);
        command.Parameters.AddWithValue("actor", request.Actor);

        var id = (Guid)(await command.ExecuteScalarAsync(cancellationToken) ?? Guid.Empty);
        await LogSecurityEventAsync(request.TenantId, "TENANT_FEATURE_UPDATED", "INFO", $"Feature {request.FeatureName} updated by {request.Actor}", null, cancellationToken);
        return id;
    }

    public async Task<IReadOnlyList<TenantApiCredentialDto>> GetApiCredentialsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT id, tenant_id, key_prefix, scopes::text, expires_at, last_used_at, is_active, created_at
FROM minepress_db.tenant_api_credentials
WHERE tenant_id = @tenantId
ORDER BY created_at DESC;";

        var items = new List<TenantApiCredentialDto>();
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("tenantId", tenantId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new TenantApiCredentialDto
            {
                Id = reader.GetGuid(0),
                TenantId = reader.GetGuid(1),
                KeyPrefix = reader.GetString(2),
                Scopes = reader.GetString(3),
                ExpiresAt = reader.GetFieldValue<DateTimeOffset>(4),
                LastUsedAt = reader.IsDBNull(5) ? null : reader.GetFieldValue<DateTimeOffset>(5),
                IsActive = reader.GetBoolean(6),
                CreatedAt = reader.GetFieldValue<DateTimeOffset>(7)
            });
        }

        return items;
    }

    public async Task<CreatedTenantApiCredential> CreateApiCredentialAsync(Guid tenantId, string scopesJson, string actor, CancellationToken cancellationToken = default)
    {
        var plainApiKey = _securityService.GenerateApiKey();
        var keyHash = _securityService.ComputeHash(plainApiKey);
        var keyPrefix = plainApiKey.Length >= 12 ? plainApiKey[..12] : plainApiKey;
        var expiresAt = DateTimeOffset.UtcNow.AddDays(_securityOptions.ApiKeyExpiryDays);

        const string sql = @"
INSERT INTO minepress_db.tenant_api_credentials
(id, tenant_id, key_hash, key_prefix, encrypted_key, scopes, expires_at, is_active, created_at, created_by)
VALUES
(gen_random_uuid(), @tenantId, @keyHash, @keyPrefix, @encryptedKey, @scopes::jsonb, @expiresAt, TRUE, CURRENT_TIMESTAMP, @actor)
RETURNING id;";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("tenantId", tenantId);
        command.Parameters.AddWithValue("keyHash", keyHash);
        command.Parameters.AddWithValue("keyPrefix", keyPrefix);
        command.Parameters.AddWithValue("encryptedKey", _securityService.Encrypt(plainApiKey));
        command.Parameters.AddWithValue("scopes", string.IsNullOrWhiteSpace(scopesJson) ? "[]" : scopesJson);
        command.Parameters.AddWithValue("expiresAt", expiresAt);
        command.Parameters.AddWithValue("actor", actor);

        var createdId = (Guid)(await command.ExecuteScalarAsync(cancellationToken) ?? Guid.Empty);

        await LogSecurityEventAsync(tenantId, "API_KEY_CREATED", "INFO", $"API credential issued by {actor}", null, cancellationToken);

        return new CreatedTenantApiCredential
        {
            Id = createdId,
            PlainApiKey = plainApiKey,
            KeyPrefix = keyPrefix,
            ExpiresAt = expiresAt
        };
    }

    public async Task<bool> RevokeApiCredentialAsync(Guid credentialId, string actor, CancellationToken cancellationToken = default)
    {
        const string sql = @"
UPDATE minepress_db.tenant_api_credentials
SET is_active = FALSE,
    revoked_at = CURRENT_TIMESTAMP,
    revoked_by = @actor
WHERE id = @credentialId
  AND is_active = TRUE
RETURNING tenant_id;";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("credentialId", credentialId);
        command.Parameters.AddWithValue("actor", actor);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        if (result is not Guid tenantId)
            return false;

        await LogSecurityEventAsync(tenantId, "API_KEY_REVOKED", "WARN", $"API credential revoked by {actor}", null, cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<TenantSecurityEventDto>> GetSecurityEventsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT id, tenant_id, event_type, severity, description, source_ip, created_at
FROM minepress_db.tenant_security_events
WHERE tenant_id = @tenantId
ORDER BY created_at DESC
LIMIT 200;";

        var items = new List<TenantSecurityEventDto>();
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("tenantId", tenantId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new TenantSecurityEventDto
            {
                Id = reader.GetGuid(0),
                TenantId = reader.GetGuid(1),
                EventType = reader.GetString(2),
                Severity = reader.GetString(3),
                Description = reader.GetString(4),
                SourceIp = reader.IsDBNull(5) ? null : reader.GetString(5),
                CreatedAt = reader.GetFieldValue<DateTimeOffset>(6)
            });
        }

        return items;
    }

    public async Task LogSecurityEventAsync(Guid tenantId, string eventType, string severity, string description, string? sourceIp, CancellationToken cancellationToken = default)
    {
        const string sql = @"
INSERT INTO minepress_db.tenant_security_events
(id, tenant_id, event_type, severity, description, source_ip, created_at)
VALUES
(gen_random_uuid(), @tenantId, @eventType, @severity, @description, @sourceIp, CURRENT_TIMESTAMP);";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("tenantId", tenantId);
        command.Parameters.AddWithValue("eventType", eventType);
        command.Parameters.AddWithValue("severity", severity);
        command.Parameters.AddWithValue("description", description);
        command.Parameters.AddWithValue("sourceIp", (object?)sourceIp ?? DBNull.Value);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
