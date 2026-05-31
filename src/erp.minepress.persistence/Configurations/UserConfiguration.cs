using erp.minepress.domain.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace erp.minepress.persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable("mst_user");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("userid");
        builder.Property(e => e.UserCode).HasColumnName("usercode").HasMaxLength(50).IsRequired();
        builder.Property(e => e.UserName).HasColumnName("username").HasMaxLength(100).IsRequired();
        builder.Property(e => e.PasswordHash).HasColumnName("passwordhash");
        builder.Property(e => e.LocationId).HasColumnName("locationid");
        builder.Property(e => e.Name).HasColumnName("name").HasMaxLength(100);
        builder.Property(e => e.EmailId).HasColumnName("emailid").HasMaxLength(150);
        builder.Property(e => e.DepartmentId).HasColumnName("departmentid");
        builder.Property(e => e.DesignationId).HasColumnName("designationid");
        builder.Property(e => e.UserType).HasColumnName("user_type").HasMaxLength(20);
        builder.Property(e => e.IsActive).HasColumnName("isactive");
        builder.Property(e => e.CreatedBy).HasColumnName("createdby").HasMaxLength(50);
        builder.Property(e => e.CreatedAt).HasColumnName("createdat");
        builder.HasIndex(e => e.UserCode).IsUnique();
        builder.HasIndex(e => e.UserName).IsUnique();
    }
}

public class RoleConfiguration : IEntityTypeConfiguration<RoleEntity>
{
    public void Configure(EntityTypeBuilder<RoleEntity> builder)
    {
        builder.ToTable("mst_role");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("roleid");
        builder.Property(e => e.RoleCode).HasColumnName("rolecode").HasMaxLength(50).IsRequired();
        builder.Property(e => e.RoleName).HasColumnName("rolename").HasMaxLength(100).IsRequired();
        builder.Property(e => e.IsSystem).HasColumnName("issystem");
        builder.Property(e => e.IsActive).HasColumnName("isactive");
        builder.HasIndex(e => e.RoleCode).IsUnique();
    }
}

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRoleEntity>
{
    public void Configure(EntityTypeBuilder<UserRoleEntity> builder)
    {
        builder.ToTable("mst_user_role");
        builder.HasKey(ur => new { ur.UserId, ur.RoleId });
        builder.Property(e => e.UserId).HasColumnName("userid");
        builder.Property(e => e.RoleId).HasColumnName("roleid");

        builder.HasOne(ur => ur.User).WithMany(u => u.UserRoles).HasForeignKey(ur => ur.UserId);
        builder.HasOne(ur => ur.Role).WithMany(r => r.UserRoles).HasForeignKey(ur => ur.RoleId);
    }
}
