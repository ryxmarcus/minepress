using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class HrVacancy
{
    public long VacancyId { get; set; }

    public string VacancyNo { get; set; } = null!;

    public long DeptId { get; set; }

    public long DesignationId { get; set; }

    public int Positions { get; set; }

    public int? FilledPositions { get; set; }

    public string? VacancyType { get; set; }

    public int? ExperienceMin { get; set; }

    public int? ExperienceMax { get; set; }

    public string? Qualification { get; set; }

    public string? SkillsRequired { get; set; }

    public decimal? SalaryMin { get; set; }

    public decimal? SalaryMax { get; set; }

    public DateOnly? TargetDate { get; set; }

    public int? LocationId { get; set; }

    public string Status { get; set; } = null!;

    public long? ApprovedBy { get; set; }

    public DateTime? ApprovedOn { get; set; }

    public string? Remarks { get; set; }

    public long CreatedBy { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }
}
