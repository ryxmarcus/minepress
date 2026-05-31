using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class HrMedicalClaim
{
    public long MedicalClaimId { get; set; }

    public string ClaimNo { get; set; } = null!;

    public long EmployeeId { get; set; }

    public DateOnly ClaimDate { get; set; }

    public string? PatientName { get; set; }

    public string? Relation { get; set; }

    public string? HospitalName { get; set; }

    public string? TreatmentType { get; set; }

    public DateOnly? TreatmentFrom { get; set; }

    public DateOnly? TreatmentTo { get; set; }

    public decimal ClaimAmount { get; set; }

    public decimal? ApprovedAmount { get; set; }

    public string? Description { get; set; }

    public string? DocumentsJson { get; set; }

    public string Status { get; set; } = null!;

    public long? ApprovedBy { get; set; }

    public DateTime? ApprovedOn { get; set; }

    public string? RejectionReason { get; set; }

    public decimal? PaidAmount { get; set; }

    public DateOnly? PaidOn { get; set; }

    public long CreatedBy { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }
}
