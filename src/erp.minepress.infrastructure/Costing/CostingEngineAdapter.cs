using erp.minepress.application.Common.Interfaces;
using erp.minepress.persistence.Context;
using erp.minepress.printingcostingengine.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace erp.minepress.infrastructure.Costing;

/// <summary>
/// Adapter that bridges the application-layer <see cref="ICostingEngine"/> to the
/// printing costing engine's <see cref="IPrintCostEngine"/>, resolving master-data
/// IDs from the database to physical values the engine requires.
/// </summary>
public class CostingEngineAdapter : ICostingEngine
{
    private readonly IPrintCostEngine _engine;
    private readonly ApplicationDbContext _db;

    public CostingEngineAdapter(IPrintCostEngine engine, ApplicationDbContext db)
    {
        _engine = engine;
        _db = db;
    }

    public async Task<CostEstimationResult> CalculateCostAsync(
        CostEstimationRequest request, CancellationToken cancellationToken = default)
    {
        var input = await BuildInputAsync(request, cancellationToken);
        var result = _engine.CalculateFullCost(input);
        return MapResult(result);
    }

    private async Task<PrintCostInput> BuildInputAsync(
        CostEstimationRequest req, CancellationToken ct)
    {
        // Resolve paper master data
        var paper = req.PaperId.HasValue
            ? await _db.MstPapers.AsNoTracking().FirstOrDefaultAsync(p => p.PaperId == req.PaperId.Value, ct)
            : null;

        // Resolve machine master data
        var machine = req.MachineId.HasValue
            ? await _db.MstMachines.AsNoTracking().FirstOrDefaultAsync(m => m.MachineId == req.MachineId.Value, ct)
            : null;

        // Pick a default active ink for cost inputs
        var ink = await _db.MstInks.AsNoTracking()
            .Where(i => i.IsActive == true)
            .OrderBy(i => i.AutoSelectPriority)
            .FirstOrDefaultAsync(ct);

        // Pick a default active plate
        var plate = await _db.MstPlates.AsNoTracking()
            .Where(p => p.IsActive == true)
            .OrderBy(p => p.AutoSelectPriority)
            .FirstOrDefaultAsync(ct);

        var printingMode = req.PrintingMode ?? "FrontBack";
        var isDigital = printingMode.Contains("digital", StringComparison.OrdinalIgnoreCase);

        return new PrintCostInput
        {
            Quantity = req.Quantity,
            TotalPages = req.TotalPages,
            TrimWidthMm = req.TrimWidthMm,
            TrimHeightMm = req.TrimHeightMm,
            PrintingMode = printingMode,
            ColorsPerSide = 4,
            IsDigital = isDigital,

            // Paper
            PaperSheetWidthMm = paper?.SheetWidthMm ?? 0,
            PaperSheetHeightMm = paper?.SheetLengthMm ?? 0,
            PaperGsm = paper?.Gsm ?? 0,
            PaperCostPerKg = paper?.CostPerKg ?? 0,
            PaperCostPerSheet = paper?.CostPerSheet ?? 0,
            PaperWastagePercent = 5m,

            // Ink
            InkCoverageSqMPerKg = ink?.CoverageSqMPerKg ?? 30m,
            InkCostPerKg = ink?.CostPerKg ?? 0,
            InkWastagePercent = ink?.WastagePercent ?? 10m,

            // Plate
            PlateCostPerUnit = plate?.PlateCost ?? 0,
            PlateProcessingCostPerUnit = plate?.ProcessingCost ?? 0,

            // Machine
            MachineMaxSpeedPerHour = machine?.MaxSpeedPerHour ?? machine?.MaxSpeed ?? 0,
            MachineHourlyRunningCost = machine?.HourlyRunningCost ?? 0,
            MachineSetupCost = machine?.SetupCost ?? 0,
            MachineSetupTimeMinutes = machine?.SetupTimeMinutes ?? machine?.SetupTimeMin ?? 0,
            MachineChangeoverTimeMinutes = machine?.ChangeoverTimeMinutes ?? machine?.ChangeoverTimeMin ?? 0,
            MachinePowerCostPerHour = machine?.PowerCostPerHour ?? 0,
            MachineLabourCostPerHour = machine?.LabourCostPerHour ?? 0,
        };
    }

    private static CostEstimationResult MapResult(PrintCostResult r)
    {
        return new CostEstimationResult
        {
            PaperCost = r.Paper.PaperCost,
            PlateCost = r.Plate.PlateCost,
            InkCost = r.Ink.InkCost,
            MachineCost = r.Machine.TotalMachineCost,
            FinishingCost = r.FinishingCost,
            BindingCost = r.BindingCost,
            DesigningCost = r.DesigningCost,
            GrandTotal = r.GrandTotal,
            CostPerUnit = r.CostPerUnit,
            TaxAmount = 0,
            NetTotal = r.GrandTotal,
            Breakdown = r.Breakdown
                .Select(b => new CostLineItem(b.Name, b.Category, b.Detail, b.Amount))
                .ToList()
        };
    }
}
