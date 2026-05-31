using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstEmployeeMachineMapping
{
    public long MappingId { get; set; }

    public long EmployeeId { get; set; }

    public string? EmployeeCode { get; set; }

    public string? EmployeeName { get; set; }

    public long MachineId { get; set; }

    public string? MachineCode { get; set; }

    public string? MachineName { get; set; }

    public string? RoleCode { get; set; }

    public string? RoleName { get; set; }

    public string? SkillLevel { get; set; }

    public string? CertificationNo { get; set; }

    public DateOnly? CertificationDate { get; set; }

    public bool? IsPrimaryMachine { get; set; }

    public bool? IsAuthorized { get; set; }

    public decimal? ExperienceYears { get; set; }

    public string? Remarks { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public bool? IsActive { get; set; }

    public virtual MstEmployee Employee { get; set; } = null!;

    public virtual MstMachine Machine { get; set; } = null!;
}
