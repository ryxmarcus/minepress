using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstStatus
{
    public int Id { get; set; }

    public string Statuscode { get; set; } = null!;

    public string Statusname { get; set; } = null!;

    public string Module { get; set; } = null!;

    public string? Stage { get; set; }

    public int? Sequenceno { get; set; }

    public bool? Isfinal { get; set; }

    public bool? Isfailure { get; set; }

    public bool? Iseditable { get; set; }

    public string? Colorcode { get; set; }

    public string? Icon { get; set; }

    public bool? Isactive { get; set; }

    public string? Createdby { get; set; }

    public DateTime? Createdon { get; set; }
}
