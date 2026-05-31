using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstProcessRoleMap
{
    public long MapId { get; set; }

    public string? ProcessCode { get; set; }

    public int? Roleid { get; set; }

    public string? RoleType { get; set; }

    public int? SequenceNo { get; set; }

    public bool? IsMandatory { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? Createdat { get; set; }
}
