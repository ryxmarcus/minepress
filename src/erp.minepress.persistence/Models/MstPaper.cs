using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstPaper
{
    public long PaperId { get; set; }

    public string PaperCode { get; set; } = null!;

    public string PaperName { get; set; } = null!;

    public string? PaperCategory { get; set; }

    public string? PaperType { get; set; }

    public string? PaperFinish { get; set; }

    public int Gsm { get; set; }

    public int? SheetLengthMm { get; set; }

    public int? SheetWidthMm { get; set; }

    public string? SheetSizeName { get; set; }

    public int? ReelWidthMm { get; set; }

    public int? ReelDiameterMm { get; set; }

    public string? GrainDirection { get; set; }

    public string? SupportedJobTypes { get; set; }

    public string? SupportedUsage { get; set; }

    public decimal? CostPerKg { get; set; }

    public decimal? CostPerSheet { get; set; }

    public string? SupplierName { get; set; }

    public string? BrandName { get; set; }

    public string? CountryOfOrigin { get; set; }

    public bool? IsFscCertified { get; set; }

    public bool? IsRecycled { get; set; }

    public decimal? MinOrderQtyKg { get; set; }

    public int? LeadTimeDays { get; set; }

    public bool? IsActive { get; set; }

    public string? Remarks { get; set; }

    public decimal? ReorderLevel { get; set; }

    public decimal? CurrentStock { get; set; }

    public string? Uom { get; set; }

    public decimal? MinOrderQty { get; set; }

    public decimal? LastPurchaseRate { get; set; }

    public DateOnly? LastPurchaseDate { get; set; }

    public string? HsnCode { get; set; }

    public decimal? GstRate { get; set; }
}
