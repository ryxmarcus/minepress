using erp.minepress.printingcostingengine.Interfaces;

namespace erp.minepress.printingcostingengine.Calculators;

public class PlateCalculator : IPlateCalculator
{
    public PlateCalculationResult Calculate(PlateCalculationInput input)
    {
        if (input.IsDigital)
        {
            return new PlateCalculationResult
            {
                TotalPlates = 0,
                PlateCost = 0,
                Calculation = "Digital printing — no plates required"
            };
        }

        // For offset: 1 plate per color per set
        int totalPlates = input.Colors * input.Sets;
        decimal costPerPlate = input.PlateCostPerUnit + input.ProcessingCostPerUnit;
        decimal totalCost = totalPlates * costPerPlate;

        return new PlateCalculationResult
        {
            TotalPlates = totalPlates,
            PlateCost = Math.Round(totalCost, 2),
            Calculation = $"Plates={input.Colors}colors x {input.Sets}sets = {totalPlates} @ {costPerPlate}/plate = {totalCost:F2}"
        };
    }
}
