using erp.minepress.printingcostingengine.Calculators;
using erp.minepress.printingcostingengine.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace erp.minepress.printingcostingengine;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPrintingCostingEngine(this IServiceCollection services)
    {
        services.AddSingleton<IPaperCalculator, PaperCalculator>();
        services.AddSingleton<IInkCalculator, InkCalculator>();
        services.AddSingleton<IPlateCalculator, PlateCalculator>();
        services.AddSingleton<IMachineCostCalculator, MachineCostCalculator>();
        services.AddSingleton<IPrintCostEngine, PrintCostEngine>();

        return services;
    }
}
