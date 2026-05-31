using Microsoft.Extensions.Configuration;
using System;

namespace erp.minepress.tenants;

public static class TenantConnectionStringExtensions
{
    public static string GetTenantCatalogConnectionString(this IConfiguration configuration)
    {
        // Try common configuration locations and environment variables to be more tolerant
        var connectionString = configuration.GetConnectionString(TenantConnectionConstants.TenantCatalogConnectionStringKey)
                               ?? configuration[$"ConnectionStrings:{TenantConnectionConstants.TenantCatalogConnectionStringKey}"]
                               ?? configuration[TenantConnectionConstants.TenantCatalogConnectionStringKey]
                               ?? Environment.GetEnvironmentVariable("ConnectionStrings__TenantCatalogConnection")
                               ?? Environment.GetEnvironmentVariable("TenantCatalogConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{TenantConnectionConstants.TenantCatalogConnectionStringKey}' is required. " +
                "Ensure 'ConnectionStrings:TenantCatalogConnection' is present in appsettings or set environment variable 'ConnectionStrings__TenantCatalogConnection'.");
        }

        return connectionString.Trim();
    }
}
