using erp.minepress.domain.Common;

namespace erp.minepress.domain.User;

public class UserEntity : BaseEntity<long>
{
    public string UserCode { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int LocationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? MobileNo { get; set; }
    public string? EmailId { get; set; }
    public long DepartmentId { get; set; }
    public long DesignationId { get; set; }
    public long? ReportingUserId { get; set; }
    public string? EmployeeCode { get; set; }
    public DateTime? JoiningDate { get; set; }
    public DateTime? ExitDate { get; set; }
    public int ApprovalLevel { get; set; }
    public decimal ApprovalLimit { get; set; }
    public bool CanOverride { get; set; }
    public bool IsSystemAdmin { get; set; }
    public bool IsProductionUser { get; set; }
    public bool IsApprovalUser { get; set; }
    public bool IsClientUser { get; set; }
    public bool IsMobileAccessAllowed { get; set; }
    public bool IsWebAccessAllowed { get; set; } = true;
    public string UserType { get; set; } = "EMPLOYEE";
    public long? RefId { get; set; }
    public int? CompanyId { get; set; }
    public string UserCategory { get; set; } = "INTERNAL";
    public bool IsActive { get; set; } = true;
    public bool IsLocked { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<UserRoleEntity> UserRoles { get; set; } = [];
}
