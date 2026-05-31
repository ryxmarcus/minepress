namespace erp.minepress.domain.User;

public class UserRoleEntity
{
    public long UserId { get; set; }
    public int RoleId { get; set; }

    public UserEntity? User { get; set; }
    public RoleEntity? Role { get; set; }
}
