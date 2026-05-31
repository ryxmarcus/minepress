using erp.minepress.domain.Machine;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace erp.minepress.persistence.Configurations;

public class MachineConfiguration : IEntityTypeConfiguration<MachineEntity>
{
    public void Configure(EntityTypeBuilder<MachineEntity> builder)
    {
        builder.ToTable("mst_machine");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("machine_id");
        builder.Property(e => e.MachineCode).HasColumnName("machine_code").HasMaxLength(30).IsRequired();
        builder.Property(e => e.MachineName).HasColumnName("machine_name").HasMaxLength(150).IsRequired();
        builder.Property(e => e.DepartmentCode).HasColumnName("department_code").HasMaxLength(10);
        builder.Property(e => e.MachineCategory).HasColumnName("machine_category").HasMaxLength(30);
        builder.Property(e => e.MachineType).HasColumnName("machine_type").HasMaxLength(50);
        builder.Property(e => e.MaxSheetLengthMm).HasColumnName("max_sheet_length_mm");
        builder.Property(e => e.MaxSheetWidthMm).HasColumnName("max_sheet_width_mm");
        builder.Property(e => e.MinSheetLengthMm).HasColumnName("min_sheet_length_mm");
        builder.Property(e => e.MinSheetWidthMm).HasColumnName("min_sheet_width_mm");
        builder.Property(e => e.MinGsm).HasColumnName("min_gsm");
        builder.Property(e => e.MaxGsm).HasColumnName("max_gsm");
        builder.Property(e => e.MaxColors).HasColumnName("max_colors");
        builder.Property(e => e.HourlyRunningCost).HasColumnName("hourly_running_cost").HasColumnType("numeric(10,2)");
        builder.Property(e => e.SetupCost).HasColumnName("setup_cost").HasColumnType("numeric(10,2)");
        builder.Property(e => e.MaxSpeedPerHour).HasColumnName("max_speed_per_hour");
        builder.Property(e => e.IsActive).HasColumnName("is_active");
        builder.HasIndex(e => e.MachineCode).IsUnique();
    }
}
