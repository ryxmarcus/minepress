using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace erp.minepress.tenants;

// Tenant-scoped IXmlRepository that stores keys in table "minepress_db.tenant_dataprotection_keys".
// This repository is safe to register as a singleton because it uses IHttpContextAccessor to discover
// the current tenant at call time (GetAllElements / StoreElement). It returns/creates keys only for
// the current tenant (based on HttpContext.Items set by TenantConnectionContextMiddleware).
public class TenantDataProtectionXmlRepository : IXmlRepository
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;

    public TenantDataProtectionXmlRepository(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
    }

    public IReadOnlyCollection<XElement> GetAllElements()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return Array.Empty<XElement>();

        if (!httpContext.Items.TryGetValue(TenantConnectionConstants.TenantIdItemKey, out var tenantIdObj)
            || !(tenantIdObj is Guid tenantId))
        {
            // No tenant resolved for this context; return no keys so DP may create keys when needed.
            return Array.Empty<XElement>();
        }

        var connString = httpContext.Items[TenantConnectionConstants.TenantConnectionStringItemKey] as string
                         ?? _configuration.GetTenantCatalogConnectionString();

        if (string.IsNullOrWhiteSpace(connString))
            return Array.Empty<XElement>();

        var list = new List<XElement>();
        const string sql = "SELECT xml FROM minepress_db.tenant_dataprotection_keys WHERE tenant_id = @tenantId ORDER BY id";

        using var conn = new NpgsqlConnection(connString);
        conn.Open();

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("tenantId", tenantId);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var xml = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
            if (!string.IsNullOrWhiteSpace(xml))
            {
                try
                {
                    var el = XElement.Parse(xml);
                    list.Add(el);
                }
                catch
                {
                    // ignore invalid xml rows
                }
            }
        }

        return list.AsReadOnly();
    }

    public void StoreElement(XElement element, string? friendlyName)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            throw new InvalidOperationException("HttpContext is required to store tenant data protection keys.");

        if (!httpContext.Items.TryGetValue(TenantConnectionConstants.TenantIdItemKey, out var tenantIdObj)
            || !(tenantIdObj is Guid tenantId))
        {
            throw new InvalidOperationException("TenantId must be resolved before storing data protection keys.");
        }

        var connString = httpContext.Items[TenantConnectionConstants.TenantConnectionStringItemKey] as string
                         ?? _configuration.GetTenantCatalogConnectionString();

        if (string.IsNullOrWhiteSpace(connString))
            throw new InvalidOperationException("No connection string available to store data protection keys.");

        const string sql = @"INSERT INTO minepress_db.tenant_dataprotection_keys (tenant_id, xml, friendly_name, created_utc)
VALUES (@tenantId, @xml, @friendlyName, now())";

        using var conn = new NpgsqlConnection(connString);
        conn.Open();

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("tenantId", tenantId);
        cmd.Parameters.AddWithValue("xml", element.ToString(SaveOptions.DisableFormatting));
        cmd.Parameters.AddWithValue("friendlyName", friendlyName ?? (object)DBNull.Value);

        cmd.ExecuteNonQuery();
    }
}
