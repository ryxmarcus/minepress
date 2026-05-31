using erp.minepress.application.Common.Interfaces;
using erp.minepress.infrastructure.Costing;
using erp.minepress.infrastructure.ErrorLogging;
using erp.minepress.infrastructure.FileStorage;
using Microsoft.Extensions.DependencyInjection;

namespace erp.minepress.infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddSingleton<IFileStorageService>(new LocalFileStorageService());
        services.AddScoped<ICostingEngine, CostingEngineAdapter>();
        services.AddScoped<ISystemErrorLogger, SystemErrorLogger>();
        return services;
    }
}
