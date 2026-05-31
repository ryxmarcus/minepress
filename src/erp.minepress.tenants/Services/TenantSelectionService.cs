using erp.minepress.tenants.Interfaces;
using erp.minepress.tenants.Models;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Threading;
using System.Threading.Tasks;

namespace erp.minepress.tenants.Services;

public class TenantSelectionService : ITenantSelectionService
{
    private readonly string _connectionString;

    public TenantSelectionService(IConfiguration configuration)
    {
        _connectionString = configuration.GetTenantCatalogConnectionString();
    }

    public async Task<IEnumerable<TenantSelectionItem>> GetTenantSelectionAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"SELECT id, tenant_key, name FROM minepress_db.tenants ORDER BY name";
        var list = new List<TenantSelectionItem>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            list.Add(new TenantSelectionItem
            {
                Id = reader.GetGuid(0),
                TenantKey = reader.GetString(1),
                Name = reader.GetString(2)
            });
        }

        return list;
    }
}
