using erp.minepress.printingcostingengine.Interfaces;

namespace erp.minepress.printingcostingengine.Calculators;

public class PaperCalculator : IPaperCalculator
{
    public PaperCalculationResult Calculate(PaperCalculationInput input)
    {
        // Calculate ups per sheet (how many trim-size pieces fit on one stock sheet)
        int upsAcrossWidth = (int)(input.SheetWidthMm / input.TrimWidthMm);
        int upsAcrossHeight = (int)(input.SheetHeightMm / input.TrimHeightMm);
        int upsNormal = upsAcrossWidth * upsAcrossHeight;

        // Try rotated layout
        int upsRotatedWidth = (int)(input.SheetWidthMm / input.TrimHeightMm);
        int upsRotatedHeight = (int)(input.SheetHeightMm / input.TrimWidthMm);
        int upsRotated = upsRotatedWidth * upsRotatedHeight;

        int upsPerSheet = Math.Max(upsNormal, upsRotated);
        if (upsPerSheet == 0) upsPerSheet = 1;

        // Calculate forms (sets of pages that go on one side of a sheet)
        int pagesPerSheet = input.PrintingMode switch
        {
            "FrontOnly" => upsPerSheet,
            "FrontBack" => upsPerSheet * 2,
            "WorkAndTurn" => upsPerSheet * 2,
            "WorkAndTumble" => upsPerSheet * 2,
            _ => upsPerSheet * 2
        };

        int totalForms = (int)Math.Ceiling((double)input.TotalPages / pagesPerSheet);
        int sheetsPerForm = (int)Math.Ceiling((double)input.Quantity / upsPerSheet);
        int totalSheets = totalForms * sheetsPerForm;

        // Add wastage
        int wastageSheets = (int)Math.Ceiling(totalSheets * (double)input.WastagePercent / 100);
        int totalSheetsWithWastage = totalSheets + wastageSheets;

        // Calculate weight: (length_m * width_m * GSM * sheets) / 1000 = kg
        decimal sheetAreaSqM = (input.SheetWidthMm / 1000m) * (input.SheetHeightMm / 1000m);
        decimal totalWeightKg = sheetAreaSqM * input.Gsm * totalSheetsWithWastage / 1000m;

        // Cost: prefer per-sheet if available, else per-kg
        decimal paperCost = input.CostPerSheet > 0
            ? totalSheetsWithWastage * input.CostPerSheet
            : totalWeightKg * input.CostPerKg;

        return new PaperCalculationResult
        {
            UpsPerSheet = upsPerSheet,
            TotalSheets = totalSheets,
            TotalSheetsWithWastage = totalSheetsWithWastage,
            TotalWeightKg = Math.Round(totalWeightKg, 3),
            PaperCost = Math.Round(paperCost, 2),
            Calculation = $"Ups={upsPerSheet}, Forms={totalForms}, Sheets/Form={sheetsPerForm}, Total={totalSheets}+{wastageSheets}waste={totalSheetsWithWastage}, Weight={totalWeightKg:F3}kg"
        };
    }
}
