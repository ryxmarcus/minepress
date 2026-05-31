using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstChemical
{
    public string ChemicalCode { get; set; } = null!;

    public string ChemicalName { get; set; } = null!;

    public string? ChemicalCategory { get; set; }

    public string? ChemicalType { get; set; }

    public string? ProcessStage { get; set; }

    public string? ApplicationArea { get; set; }

    public string? CompatibleProcess { get; set; }

    public string? CompatibleMachineType { get; set; }

    public string? Manufacturer { get; set; }

    public string? Brand { get; set; }

    public string? DilutionRatio { get; set; }

    public string? PhValueRange { get; set; }

    public string? ConductivityRange { get; set; }

    public string? ConsumptionUnit { get; set; }

    public decimal? AvgConsumptionPerHr { get; set; }

    public decimal? RatePerUnit { get; set; }

    public decimal? HourlyCost { get; set; }

    public string? StorageCondition { get; set; }

    public int? ShelfLifeMonths { get; set; }

    public bool? Hazardous { get; set; }

    public bool? IsActive { get; set; }

    public string? Remarks { get; set; }

    public decimal? ReorderLevel { get; set; }

    public decimal? CurrentStock { get; set; }

    public string? Uom { get; set; }

    public decimal? MinOrderQty { get; set; }

    public int? LeadTimeDays { get; set; }

    public decimal? LastPurchaseRate { get; set; }

    public DateOnly? LastPurchaseDate { get; set; }

    public string? HsnCode { get; set; }

    public decimal? GstRate { get; set; }
}
