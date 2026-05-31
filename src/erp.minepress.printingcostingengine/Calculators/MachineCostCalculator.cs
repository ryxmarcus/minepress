using erp.minepress.printingcostingengine.Interfaces;

namespace erp.minepress.printingcostingengine.Calculators;

public class MachineCostCalculator : IMachineCostCalculator
{
    public MachineCostResult Calculate(MachineCostInput input)
    {
        // Run time = total sheets * passes / speed per hour
        decimal totalImpressions = input.TotalSheets * input.Passes;
        decimal runTimeHours = input.MaxSpeedPerHour > 0
            ? totalImpressions / input.MaxSpeedPerHour
            : 0;

        // Setup time in hours
        decimal setupTimeHours = (input.SetupTimeMinutes + input.ChangeoverTimeMinutes) / 60m;
        decimal totalTimeHours = runTimeHours + setupTimeHours;

        // Costs
        decimal runningCost = runTimeHours * input.HourlyRunningCost;
        decimal setupCostTotal = input.SetupCost;
        decimal powerCost = totalTimeHours * input.PowerCostPerHour;
        decimal labourCost = totalTimeHours * input.LabourCostPerHour;
        decimal totalMachineCost = runningCost + setupCostTotal + powerCost + labourCost;

        return new MachineCostResult
        {
            RunTimeHours = Math.Round(runTimeHours, 4),
            TotalTimeHours = Math.Round(totalTimeHours, 4),
            RunningCost = Math.Round(runningCost, 2),
            SetupCostTotal = Math.Round(setupCostTotal, 2),
            PowerCost = Math.Round(powerCost, 2),
            LabourCost = Math.Round(labourCost, 2),
            TotalMachineCost = Math.Round(totalMachineCost, 2),
            Calculation = $"Impressions={totalImpressions}, RunTime={runTimeHours:F4}h, Setup={setupTimeHours:F2}h, Total={totalTimeHours:F4}h"
        };
    }
}
