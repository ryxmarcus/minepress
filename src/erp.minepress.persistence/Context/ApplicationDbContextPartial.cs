using erp.minepress.persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace erp.minepress.persistence.Context;

public partial class ApplicationDbContext
{
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MapModuleDepartment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("map_module_department_pkey");

            entity.ToTable("map_module_department", "press_db");

            entity.HasIndex(e => new { e.DepartmentId, e.ModuleId }, "uq_dept_module").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.ModuleId).HasColumnName("module_id");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_on");
        });
    }
}
