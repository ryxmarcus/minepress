namespace erp.minepress.printingcostingengine.Interfaces;

public interface IPaperCalculator
{
    PaperCalculationResult Calculate(PaperCalculationInput input);
}

public record PaperCalculationInput
{
    public decimal TrimWidthMm { get; init; }
    public decimal TrimHeightMm { get; init; }
    public decimal SheetWidthMm { get; init; }
    public decimal SheetHeightMm { get; init; }
    public int Gsm { get; init; }
    public int Quantity { get; init; }
    public int TotalPages { get; init; }
    public int ColorsPerSide { get; init; }
    public decimal CostPerKg { get; init; }
    public decimal CostPerSheet { get; init; }
    public decimal WastagePercent { get; init; } = 5m;
    public string PrintingMode { get; init; } = "FrontBack";
}

public record PaperCalculationResult
{
    public int UpsPerSheet { get; init; }
    public int TotalSheets { get; init; }
    public int TotalSheetsWithWastage { get; init; }
    public decimal TotalWeightKg { get; init; }
    public decimal PaperCost { get; init; }
    public string Calculation { get; init; } = string.Empty;
}
