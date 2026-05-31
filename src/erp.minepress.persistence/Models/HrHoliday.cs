using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class HrHoliday
{
    public int HolidayId { get; set; }

    public string HolidayName { get; set; } = null!;

    public DateOnly HolidayDate { get; set; }

    public string? HolidayType { get; set; }

    public string? FinYear { get; set; }

    public int? CompanyId { get; set; }

    public int? LocationId { get; set; }

    public bool? IsOptional { get; set; }

    public bool IsActive { get; set; }

    public string? Remarks { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }
}
