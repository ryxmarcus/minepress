using erp.minepress.domain.Common;

namespace erp.minepress.domain.User;

public class RoleEntity : BaseEntity<int>
{
    public string RoleCode { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<UserRoleEntity> UserRoles { get; set; } = [];
}
