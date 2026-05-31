using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstPrintProductSize
{
    public int Productsizeid { get; set; }

    public string? Sizecode { get; set; }

    public string? Sizename { get; set; }

    public decimal? Widthmm { get; set; }

    public decimal? Heightmm { get; set; }

    public decimal? Widthinch { get; set; }

    public decimal? Heightinch { get; set; }

    public string? Category { get; set; }

    public bool? Isstandard { get; set; }

    public bool? Isactive { get; set; }

    public string? Remarks { get; set; }

    public virtual ICollection<HybJobRateCalculator> HybJobRateCalculators { get; set; } = new List<HybJobRateCalculator>();
}
