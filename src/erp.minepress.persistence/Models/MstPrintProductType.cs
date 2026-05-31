using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstPrintProductType
{
    public int Printproducttypeid { get; set; }

    public string Productcode { get; set; } = null!;

    public string Productname { get; set; } = null!;

    public string? Category { get; set; }

    public string? Description { get; set; }

    public bool? Iscustomsize { get; set; }

    public bool? Isbindingrequired { get; set; }

    public bool? Isprintingrequired { get; set; }

    public bool? Isfinishingrequired { get; set; }

    public bool? Isactive { get; set; }

    public string? Createdby { get; set; }

    public DateTime? Createdon { get; set; }

    public virtual ICollection<HybJobRateCalculator> HybJobRateCalculators { get; set; } = new List<HybJobRateCalculator>();

    public virtual ICollection<MstProductPart> MstProductParts { get; set; } = new List<MstProductPart>();

    public virtual ICollection<MstWorkflowTemplate> MstWorkflowTemplates { get; set; } = new List<MstWorkflowTemplate>();

    public virtual ICollection<TrnJobItem> TrnJobItems { get; set; } = new List<TrnJobItem>();

    public virtual ICollection<TrnQuotationItem> TrnQuotationItems { get; set; } = new List<TrnQuotationItem>();
}
