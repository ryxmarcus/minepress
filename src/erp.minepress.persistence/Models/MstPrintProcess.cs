using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstPrintProcess
{
    public int Processid { get; set; }

    public string? Processcode { get; set; }

    public string? Processname { get; set; }

    public string? Processcategory { get; set; }

    public string? Description { get; set; }

    public int? Displayorder { get; set; }

    public bool? Isactive { get; set; }

    public DateTime? Createdon { get; set; }
}
