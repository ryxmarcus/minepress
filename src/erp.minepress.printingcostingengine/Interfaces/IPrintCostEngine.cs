namespace erp.minepress.printingcostingengine.Interfaces;

public interface IPrintCostEngine
{
    PrintCostResult CalculateFullCost(PrintCostInput input);
}

public record PrintCostInput
{
    public int Quantity { get; init; }
    public int TotalPages { get; init; }
    public decimal TrimWidthMm { get; init; }
    public decimal TrimHeightMm { get; init; }
    public string PrintingMode { get; init; } = "FrontBack";
    public int ColorsPerSide { get; init; } = 4;
    public bool IsDigital { get; init; }

    // Paper
    public decimal PaperSheetWidthMm { get; init; }
    public decimal PaperSheetHeightMm { get; init; }
    public int PaperGsm { get; init; }
    public decimal PaperCostPerKg { get; init; }
    public decimal PaperCostPerSheet { get; init; }
    public decimal PaperWastagePercent { get; init; } = 5m;

    // Ink
    public decimal InkCoverageSqMPerKg { get; init; }
    public decimal InkCostPerKg { get; init; }
    public decimal InkWastagePercent { get; init; } = 10m;

    // Plate
    public decimal PlateCostPerUnit { get; init; }
    public decimal PlateProcessingCostPerUnit { get; init; }

    // Machine
    public int MachineMaxSpeedPerHour { get; init; }
    public decimal MachineHourlyRunningCost { get; init; }
    public decimal MachineSetupCost { get; init; }
    public int MachineSetupTimeMinutes { get; init; }
    public int MachineChangeoverTimeMinutes { get; init; }
    public decimal MachinePowerCostPerHour { get; init; }
    public decimal MachineLabourCostPerHour { get; init; }

    // Finishing
    public decimal FinishingCostPerSheet { get; init; }
    public decimal FinishingSetupCost { get; init; }

    // Binding
    public decimal BindingCostPerBook { get; init; }
    public decimal BindingSetupCost { get; init; }
    public bool RequiresBinding { get; init; }

    // Designing
    public decimal DesigningBaseCost { get; init; }
}

public record PrintCostResult
{
    public PaperCalculationResult Paper { get; init; } = new();
    public InkCalculationResult Ink { get; init; } = new();
    public PlateCalculationResult Plate { get; init; } = new();
    public MachineCostResult Machine { get; init; } = new();
    public decimal FinishingCost { get; init; }
    public decimal BindingCost { get; init; }
    public decimal DesigningCost { get; init; }
    public decimal GrandTotal { get; init; }
    public decimal CostPerUnit { get; init; }
    public IReadOnlyList<CostBreakdownItem> Breakdown { get; init; } = [];
}

public record CostBreakdownItem(string Name, string Category, string Detail, decimal Amount);
