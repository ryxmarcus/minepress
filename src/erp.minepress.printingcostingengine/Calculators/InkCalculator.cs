using erp.minepress.printingcostingengine.Interfaces;

namespace erp.minepress.printingcostingengine.Calculators;

public class InkCalculator : IInkCalculator
{
    public InkCalculationResult Calculate(InkCalculationInput input)
    {
        if (input.IsDigital)
        {
            return new InkCalculationResult
            {
                TotalInkKg = 0,
                InkCost = 0,
                Calculation = "Digital printing — ink cost included in machine cost"
            };
        }

        // Total print area = sheets * sheet area * colors
        decimal totalPrintAreaSqM = input.TotalSheets * input.SheetAreaSqM * input.Colors;

        // Ink consumption = total area / coverage per kg
        decimal inkKg = input.CoverageSqMPerKg > 0
            ? totalPrintAreaSqM / input.CoverageSqMPerKg
            : 0;

        // Add wastage
        decimal wastageKg = inkKg * input.WastagePercent / 100m;
        decimal totalInkKg = inkKg + wastageKg;

        decimal inkCost = totalInkKg * input.CostPerKg;

        return new InkCalculationResult
        {
            TotalInkKg = Math.Round(totalInkKg, 3),
            InkCost = Math.Round(inkCost, 2),
            Calculation = $"Area={totalPrintAreaSqM:F2}sqm, Ink={inkKg:F3}+{wastageKg:F3}waste={totalInkKg:F3}kg @ {input.CostPerKg}/kg"
        };
    }
}
