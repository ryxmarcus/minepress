using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstDocumentSequence
{
    public long SequenceId { get; set; }

    public string ProcessCode { get; set; } = null!;

    public string ProcessName { get; set; } = null!;

    public string? Prefix { get; set; }

    public string? Suffix { get; set; }

    public long CurrentNumber { get; set; }

    public int PaddingLength { get; set; }

    public string? FinancialYear { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
