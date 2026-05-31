using erp.minepress.bff.service.Interfaces;
using erp.minepress.bff.service.Services;
using Microsoft.Extensions.DependencyInjection;

namespace erp.minepress.bff.service;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBffServices(this IServiceCollection services)
    {
        services.AddScoped<IBffAggregatorService, BffAggregatorService>();
        services.AddScoped<IAiDataService, AiDataService>();
        return services;
    }
}
