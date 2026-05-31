using erp.minepress.domain.Ink;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace erp.minepress.persistence.Configurations;

public class InkConfiguration : IEntityTypeConfiguration<InkEntity>
{
    public void Configure(EntityTypeBuilder<InkEntity> builder)
    {
        builder.ToTable("mst_ink");
        builder.HasKey(e => e.InkCode);
        builder.Property(e => e.InkCode).HasColumnName("ink_code").HasMaxLength(50);
        builder.Property(e => e.InkName).HasColumnName("ink_name").HasMaxLength(150).IsRequired();
        builder.Property(e => e.InkCategory).HasColumnName("ink_category").HasMaxLength(50);
        builder.Property(e => e.InkType).HasColumnName("ink_type").HasMaxLength(50);
        builder.Property(e => e.ColorType).HasColumnName("color_type").HasMaxLength(50);
        builder.Property(e => e.ColorName).HasColumnName("color_name").HasMaxLength(50);
        builder.Property(e => e.CoverageSqMPerKg).HasColumnName("coverage_sq_m_per_kg").HasColumnType("numeric(10,2)");
        builder.Property(e => e.CostPerKg).HasColumnName("cost_per_kg").HasColumnType("numeric(10,2)");
        builder.Property(e => e.WastagePercent).HasColumnName("wastage_percent").HasColumnType("numeric(5,2)");
        builder.Property(e => e.IsActive).HasColumnName("is_active");
    }
}
