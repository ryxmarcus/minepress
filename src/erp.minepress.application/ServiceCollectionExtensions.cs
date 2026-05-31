using erp.minepress.application.Common.Interfaces;
using erp.minepress.application.Common.Models;
using erp.minepress.application.Jobs.Commands;
using erp.minepress.application.Jobs.Dto;
using erp.minepress.application.Jobs.Handlers;
using erp.minepress.application.Jobs.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace erp.minepress.application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<CreateJobRateCalculationCommand, Result<JobRateCalculatorDto>>,
            CreateJobRateCalculationHandler>();

        services.AddScoped<IQueryHandler<GetJobRateCalculationByRefNoQuery, Result<JobRateCalculatorDto>>,
            GetJobRateCalculationByRefNoHandler>();

        services.AddScoped<IQueryHandler<CalculateCostQuery, Result<CostEstimationResult>>,
            CalculateCostHandler>();

        return services;
    }
}
