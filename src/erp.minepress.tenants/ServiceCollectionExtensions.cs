using erp.minepress.tenants.Interfaces;
using erp.minepress.tenants.Options;
using erp.minepress.tenants.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace erp.minepress.tenants;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTenantServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TenantSecurityOptions>(configuration.GetSection(TenantSecurityOptions.SectionName));
        services.AddHttpContextAccessor();
        services.AddMemoryCache();
        services.AddScoped<ITenantContextAccessor, TenantContextAccessor>();
        services.AddSingleton<ITenantSecurityService, TenantSecurityService>();
        services.AddScoped<ITenantResolver, DefaultTenantResolver>();
        services.AddSingleton<ITenantSelectionService, Services.TenantSelectionService>();
        services.AddSingleton<ITenantTokenService, Services.TenantTokenService>();
        services.AddScoped<ITenantManagementService, TenantManagementService>();
        return services;
    }
}
