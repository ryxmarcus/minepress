using erp.minepress.printingcostingengine.Interfaces;

namespace erp.minepress.printingcostingengine.Calculators;

public class PrintCostEngine : IPrintCostEngine
{
    private readonly IPaperCalculator _paperCalculator;
    private readonly IInkCalculator _inkCalculator;
    private readonly IPlateCalculator _plateCalculator;
    private readonly IMachineCostCalculator _machineCostCalculator;

    public PrintCostEngine(
        IPaperCalculator paperCalculator,
        IInkCalculator inkCalculator,
        IPlateCalculator plateCalculator,
        IMachineCostCalculator machineCostCalculator)
    {
        _paperCalculator = paperCalculator;
        _inkCalculator = inkCalculator;
        _plateCalculator = plateCalculator;
        _machineCostCalculator = machineCostCalculator;
    }

    public PrintCostResult CalculateFullCost(PrintCostInput input)
    {
        // 1. Paper calculation
        var paperResult = _paperCalculator.Calculate(new PaperCalculationInput
        {
            TrimWidthMm = input.TrimWidthMm,
            TrimHeightMm = input.TrimHeightMm,
            SheetWidthMm = input.PaperSheetWidthMm,
            SheetHeightMm = input.PaperSheetHeightMm,
            Gsm = input.PaperGsm,
            Quantity = input.Quantity,
            TotalPages = input.TotalPages,
            ColorsPerSide = input.ColorsPerSide,
            CostPerKg = input.PaperCostPerKg,
            CostPerSheet = input.PaperCostPerSheet,
            WastagePercent = input.PaperWastagePercent,
            PrintingMode = input.PrintingMode
        });

        // 2. Ink calculation
        decimal sheetAreaSqM = (input.PaperSheetWidthMm / 1000m) * (input.PaperSheetHeightMm / 1000m);
        var inkResult = _inkCalculator.Calculate(new InkCalculationInput
        {
            TotalSheets = paperResult.TotalSheetsWithWastage,
            SheetAreaSqM = sheetAreaSqM,
            Colors = input.ColorsPerSide,
            CoverageSqMPerKg = input.InkCoverageSqMPerKg,
            CostPerKg = input.InkCostPerKg,
            WastagePercent = input.InkWastagePercent,
            IsDigital = input.IsDigital
        });

        // 3. Plate calculation
        int plateSets = (int)Math.Ceiling((double)input.TotalPages /
            (input.PrintingMode == "FrontOnly" ? 1 : 2));
        var plateResult = _plateCalculator.Calculate(new PlateCalculationInput
        {
            Colors = input.ColorsPerSide,
            Sets = Math.Max(1, plateSets),
            PlateCostPerUnit = input.PlateCostPerUnit,
            ProcessingCostPerUnit = input.PlateProcessingCostPerUnit,
            IsDigital = input.IsDigital
        });

        // 4. Machine cost calculation
        int passes = input.PrintingMode switch
        {
            "FrontOnly" => 1,
            "FrontBack" => 2,
            "WorkAndTurn" => 1,
            "WorkAndTumble" => 1,
            "Perfecting" => 1,
            _ => 2
        };

        var machineResult = _machineCostCalculator.Calculate(new MachineCostInput
        {
            TotalSheets = paperResult.TotalSheetsWithWastage,
            MaxSpeedPerHour = input.MachineMaxSpeedPerHour,
            HourlyRunningCost = input.MachineHourlyRunningCost,
            SetupCost = input.MachineSetupCost,
            SetupTimeMinutes = input.MachineSetupTimeMinutes,
            ChangeoverTimeMinutes = input.MachineChangeoverTimeMinutes,
            PowerCostPerHour = input.MachinePowerCostPerHour,
            LabourCostPerHour = input.MachineLabourCostPerHour,
            Passes = passes
        });

        // 5. Finishing cost
        decimal finishingCost = (paperResult.TotalSheetsWithWastage * input.FinishingCostPerSheet)
            + input.FinishingSetupCost;
        finishingCost = Math.Round(finishingCost, 2);

        // 6. Binding cost
        decimal bindingCost = 0;
        if (input.RequiresBinding)
        {
            bindingCost = (input.Quantity * input.BindingCostPerBook) + input.BindingSetupCost;
            bindingCost = Math.Round(bindingCost, 2);
        }

        // 7. Designing cost
        decimal designingCost = Math.Round(input.DesigningBaseCost, 2);

        // Grand total
        decimal grandTotal = paperResult.PaperCost
            + inkResult.InkCost
            + plateResult.PlateCost
            + machineResult.TotalMachineCost
            + finishingCost
            + bindingCost
            + designingCost;

        decimal costPerUnit = input.Quantity > 0 ? Math.Round(grandTotal / input.Quantity, 4) : 0;

        // Build breakdown
        var breakdown = new List<CostBreakdownItem>
        {
            new("Paper", "Material", paperResult.Calculation, paperResult.PaperCost),
            new("Ink", "Material", inkResult.Calculation, inkResult.InkCost),
            new("Plates", "Prepress", plateResult.Calculation, plateResult.PlateCost),
            new("Machine", "Printing", machineResult.Calculation, machineResult.TotalMachineCost),
            new("Finishing", "Postpress", $"Sheets={paperResult.TotalSheetsWithWastage} @ {input.FinishingCostPerSheet}/sheet + {input.FinishingSetupCost} setup", finishingCost),
        };

        if (input.RequiresBinding)
            breakdown.Add(new("Binding", "Postpress", $"Qty={input.Quantity} @ {input.BindingCostPerBook}/book + {input.BindingSetupCost} setup", bindingCost));

        if (designingCost > 0)
            breakdown.Add(new("Designing", "Prepress", "Base design cost", designingCost));

        return new PrintCostResult
        {
            Paper = paperResult,
            Ink = inkResult,
            Plate = plateResult,
            Machine = machineResult,
            FinishingCost = finishingCost,
            BindingCost = bindingCost,
            DesigningCost = designingCost,
            GrandTotal = Math.Round(grandTotal, 2),
            CostPerUnit = costPerUnit,
            Breakdown = breakdown.AsReadOnly()
        };
    }
}
