using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstPaperSize
{
    public long PaperId { get; set; }

    public string Category { get; set; } = null!;

    public string? Series { get; set; }

    public string SizeName { get; set; } = null!;

    public int? WidthMm { get; set; }

    public int? HeightMm { get; set; }

    public decimal? WidthIn { get; set; }

    public decimal? HeightIn { get; set; }

    public string? CommonUses { get; set; }
}
