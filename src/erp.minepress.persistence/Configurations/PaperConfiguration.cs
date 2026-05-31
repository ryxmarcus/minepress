using erp.minepress.domain.Paper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace erp.minepress.persistence.Configurations;

public class PaperConfiguration : IEntityTypeConfiguration<PaperEntity>
{
    public void Configure(EntityTypeBuilder<PaperEntity> builder)
    {
        builder.ToTable("mst_paper");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("paper_id");
        builder.Property(e => e.PaperCode).HasColumnName("paper_code").HasMaxLength(50).IsRequired();
        builder.Property(e => e.PaperName).HasColumnName("paper_name").HasMaxLength(150).IsRequired();
        builder.Property(e => e.PaperCategory).HasColumnName("paper_category").HasMaxLength(50);
        builder.Property(e => e.PaperType).HasColumnName("paper_type").HasMaxLength(50);
        builder.Property(e => e.PaperFinish).HasColumnName("paper_finish").HasMaxLength(50);
        builder.Property(e => e.Gsm).HasColumnName("gsm");
        builder.Property(e => e.SheetLengthMm).HasColumnName("sheet_length_mm");
        builder.Property(e => e.SheetWidthMm).HasColumnName("sheet_width_mm");
        builder.Property(e => e.CostPerKg).HasColumnName("cost_per_kg").HasColumnType("numeric(10,2)");
        builder.Property(e => e.CostPerSheet).HasColumnName("cost_per_sheet").HasColumnType("numeric(10,2)");
        builder.Property(e => e.IsActive).HasColumnName("is_active");
        builder.HasIndex(e => e.PaperCode).IsUnique();
    }
}
