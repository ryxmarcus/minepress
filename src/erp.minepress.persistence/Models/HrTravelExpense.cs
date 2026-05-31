using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class HrTravelExpense
{
    public long TravelId { get; set; }

    public string TravelNo { get; set; } = null!;

    public long EmployeeId { get; set; }

    public string? Purpose { get; set; }

    public string? FromLocation { get; set; }

    public string? ToLocation { get; set; }

    public DateOnly TravelDate { get; set; }

    public DateOnly? ReturnDate { get; set; }

    public string? ModeOfTravel { get; set; }

    public decimal? AdvanceAmount { get; set; }

    public decimal? ClaimAmount { get; set; }

    public decimal? ApprovedAmount { get; set; }

    public string? ExpenseLinesJson { get; set; }

    public string Status { get; set; } = null!;

    public long? ApprovedBy { get; set; }

    public DateTime? ApprovedOn { get; set; }

    public string? RejectionReason { get; set; }

    public decimal? SettledAmount { get; set; }

    public DateOnly? SettledOn { get; set; }

    public long CreatedBy { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }
}
