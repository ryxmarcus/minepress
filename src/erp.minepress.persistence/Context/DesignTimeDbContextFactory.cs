using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace erp.minepress.persistence.Context;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();
        var webAppSettings = Path.GetFullPath(Path.Combine(basePath, "..", "erp.minepress.web", "appsettings.json"));
        var webApiAppSettings = Path.GetFullPath(Path.Combine(basePath, "..", "erp.minepress.webapi", "appsettings.json"));

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddJsonFile(webAppSettings, optional: true)
            .AddJsonFile(webApiAppSettings, optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("TenantCatalogConnection")
                               ?? Environment.GetEnvironmentVariable("ConnectionStrings__TenantCatalogConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Connection string 'TenantCatalogConnection' is required for design-time DbContext creation.");

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
