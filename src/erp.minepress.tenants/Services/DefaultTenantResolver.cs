using erp.minepress.tenants.Interfaces;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace erp.minepress.tenants.Services;

public class DefaultTenantResolver : ITenantResolver
{
    private const string DefaultTenant = "default";
    private readonly ITenantContextAccessor _tenantContextAccessor;
    private readonly ITenantSecurityService _tenantSecurityService;
    private readonly string _connectionString;

    public DefaultTenantResolver(
        ITenantContextAccessor tenantContextAccessor,
        ITenantSecurityService tenantSecurityService,
        IConfiguration configuration)
    {
        _tenantContextAccessor = tenantContextAccessor;
        _tenantSecurityService = tenantSecurityService;
        _connectionString = configuration.GetTenantCatalogConnectionString();
    }

    public string GetCurrentTenantKey()
    {
        return _tenantContextAccessor.Current?.TenantKey ?? DefaultTenant;
    }

    public async Task<TenantInfo?> ResolveTenantAsync(string tenantKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantKey))
            return null;

        const string sql = @"
SELECT id, tenant_key, name, encrypted_connection_string, schema_name, is_active
FROM minepress_db.tenants
WHERE tenant_key = @tenantKey
LIMIT 1;";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("tenantKey", tenantKey);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        var encryptedConnection = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
        var connectionString = encryptedConnection;

        if (!string.IsNullOrWhiteSpace(encryptedConnection))
        {
            try
            {
                connectionString = _tenantSecurityService.Decrypt(encryptedConnection);
            }
            catch
            {
                connectionString = encryptedConnection;
            }
        }

        return new TenantInfo
        {
            TenantId = reader.GetGuid(0),
            TenantKey = reader.GetString(1),
            TenantName = reader.GetString(2),
            ConnectionString = connectionString,
            SchemaName = reader.IsDBNull(4) ? "public" : reader.GetString(4),
            IsActive = reader.GetBoolean(5)
        };
    }
}
