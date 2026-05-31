using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstEmployee
{
    public long EmployeeId { get; set; }

    public string EmpCode { get; set; } = null!;

    public string? FirstName { get; set; }

    public string? MiddleName { get; set; }

    public string? LastName { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public DateOnly? DateOfJoining { get; set; }

    public DateOnly? DateOfRelieving { get; set; }

    public long? DeptId { get; set; }

    public long? DesignationId { get; set; }

    public int? CompanyId { get; set; }

    public int? LocationId { get; set; }

    public string? PhoneNo { get; set; }

    public string? MobileNo1 { get; set; }

    public string? MobileNo2 { get; set; }

    public string? Email1 { get; set; }

    public string? Email2 { get; set; }

    public string? BankAccountNo { get; set; }

    public string? BankName { get; set; }

    public string? BranchName { get; set; }

    public string? IfscCode { get; set; }

    public string? PanNo { get; set; }

    public string? AadharNo { get; set; }

    public string? PfNo { get; set; }

    public string? EsiNo { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public long? ReportingEmployeeId { get; set; }

    public int? EmployeeTypeId { get; set; }

    public int? ShiftTypeId { get; set; }

    public virtual MstCompany? Company { get; set; }

    public virtual MstDepartment? Dept { get; set; }

    public virtual MstDesignation? Designation { get; set; }

    public virtual MstEmployeeType? EmployeeType { get; set; }

    public virtual ICollection<HrReimbursement> HrReimbursements { get; set; } = new List<HrReimbursement>();

    public virtual ICollection<HybEmployeeAttendance> HybEmployeeAttendances { get; set; } = new List<HybEmployeeAttendance>();

    public virtual ICollection<MstEmployee> InverseReportingEmployee { get; set; } = new List<MstEmployee>();

    public virtual MstLocation? Location { get; set; }

    public virtual ICollection<MstEmployeeMachineMapping> MstEmployeeMachineMappings { get; set; } = new List<MstEmployeeMachineMapping>();

    public virtual ICollection<MstUser> MstUsers { get; set; } = new List<MstUser>();

    public virtual MstEmployee? ReportingEmployee { get; set; }

    public virtual MstShiftType? ShiftType { get; set; }

    public virtual ICollection<TrnExpenseVoucher> TrnExpenseVouchers { get; set; } = new List<TrnExpenseVoucher>();

    public virtual ICollection<TrnJobMachineManpowerAllocation> TrnJobMachineManpowerAllocations { get; set; } = new List<TrnJobMachineManpowerAllocation>();
}
