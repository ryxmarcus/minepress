namespace erp.minepress.printingcostingengine.Interfaces;

public interface IPlateCalculator
{
    PlateCalculationResult Calculate(PlateCalculationInput input);
}

public record PlateCalculationInput
{
    public int Colors { get; init; }
    public int Sets { get; init; } = 1;
    public decimal PlateCostPerUnit { get; init; }
    public decimal ProcessingCostPerUnit { get; init; }
    public bool IsDigital { get; init; }
}

public record PlateCalculationResult
{
    public int TotalPlates { get; init; }
    public decimal PlateCost { get; init; }
    public string Calculation { get; init; } = string.Empty;
}
