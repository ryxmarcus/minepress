namespace erp.minepress.printingcostingengine.Interfaces;

public interface IMachineCostCalculator
{
    MachineCostResult Calculate(MachineCostInput input);
}

public record MachineCostInput
{
    public int TotalSheets { get; init; }
    public int MaxSpeedPerHour { get; init; }
    public decimal HourlyRunningCost { get; init; }
    public decimal SetupCost { get; init; }
    public int SetupTimeMinutes { get; init; }
    public int ChangeoverTimeMinutes { get; init; }
    public decimal PowerCostPerHour { get; init; }
    public decimal LabourCostPerHour { get; init; }
    public int Passes { get; init; } = 1;
}

public record MachineCostResult
{
    public decimal RunTimeHours { get; init; }
    public decimal TotalTimeHours { get; init; }
    public decimal RunningCost { get; init; }
    public decimal SetupCostTotal { get; init; }
    public decimal PowerCost { get; init; }
    public decimal LabourCost { get; init; }
    public decimal TotalMachineCost { get; init; }
    public string Calculation { get; init; } = string.Empty;
}
