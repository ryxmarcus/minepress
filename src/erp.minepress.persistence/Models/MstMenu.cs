using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstMenu
{
    public int Menuid { get; set; }

    public string Menucode { get; set; } = null!;

    public string Menuname { get; set; } = null!;

    public int? Parentmenuid { get; set; }

    public string? Routeurl { get; set; }

    public string? Icon { get; set; }

    public int? Displayorder { get; set; }

    public bool? Ismobile { get; set; }

    public bool? Isweb { get; set; }

    public bool? Isactive { get; set; }

    public int? Menulevel { get; set; }

    public bool? Issectionheader { get; set; }

    public string? Sectionname { get; set; }

    public string? Badgetext { get; set; }

    public string? Badgeclass { get; set; }

    public bool? Hasdividerbefore { get; set; }

    public string? Iconsvg { get; set; }

    public int? ModuleId { get; set; }

    public virtual ICollection<MstMenu> InverseParentmenu { get; set; } = new List<MstMenu>();

    public virtual MstModule? Module { get; set; }

    public virtual MstMenu? Parentmenu { get; set; }
}
