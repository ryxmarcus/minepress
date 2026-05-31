using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstProductPart
{
    public int Productpartid { get; set; }

    public string Partcode { get; set; } = null!;

    public string Partname { get; set; } = null!;

    public string? Description { get; set; }

    public int Printproducttypeid { get; set; }

    public bool? Ispagebased { get; set; }

    public bool? Ismultiple { get; set; }

    public int? Defaultpages { get; set; }

    public bool? Requiresdesign { get; set; }

    public bool? Requirespaper { get; set; }

    public bool? Requiresplate { get; set; }

    public bool? Requiresprinting { get; set; }

    public bool? Requiresbinding { get; set; }

    public bool? Requiresfinishing { get; set; }

    public int? Displayorder { get; set; }

    public bool? Isactive { get; set; }

    public DateTime? Createdon { get; set; }

    public virtual MstPrintProductType Printproducttype { get; set; } = null!;
}
