namespace erp.minepress.printingcostingengine.Interfaces;

public interface IInkCalculator
{
    InkCalculationResult Calculate(InkCalculationInput input);
}

public record InkCalculationInput
{
    public int TotalSheets { get; init; }
    public decimal SheetAreaSqM { get; init; }
    public int Colors { get; init; }
    public decimal CoverageSqMPerKg { get; init; }
    public decimal CostPerKg { get; init; }
    public decimal WastagePercent { get; init; } = 10m;
    public bool IsDigital { get; init; }
}

public record InkCalculationResult
{
    public decimal TotalInkKg { get; init; }
    public decimal InkCost { get; init; }
    public string Calculation { get; init; } = string.Empty;
}
