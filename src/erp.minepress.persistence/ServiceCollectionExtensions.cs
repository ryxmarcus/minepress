using erp.minepress.application.Common.Interfaces;
using erp.minepress.application.Reports.Interfaces;
using erp.minepress.persistence.Repositories;
using erp.minepress.persistence.Services;
using Microsoft.Extensions.DependencyInjection;

namespace erp.minepress.persistence;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IJobRateCalculatorRepository, JobRateCalculatorRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IMachineRepository, MachineRepository>();
        services.AddScoped<IPaperRepository, PaperRepository>();

        // Report Query Builder Engine
        services.AddScoped<IDynamicSqlGenerator, DynamicSqlGenerator>();
        services.AddScoped<IReportExecutionService, ReportExecutionService>();
        services.AddScoped<IQueryBuilderService, QueryBuilderService>();

        return services;
    }
}
