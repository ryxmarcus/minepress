using erp.minepress.domain.Customer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace erp.minepress.persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<CustomerEntity>
{
    public void Configure(EntityTypeBuilder<CustomerEntity> builder)
    {
        builder.ToTable("mst_customer");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.PartyId).HasColumnName("party_id");
        builder.Property(e => e.CustomerType).HasColumnName("customer_type");
        builder.Property(e => e.CustomerGroup).HasColumnName("customer_group");
        builder.Property(e => e.PaymentTerms).HasColumnName("payment_terms");
        builder.Property(e => e.MaxCreditLimit).HasColumnName("max_credit_limit").HasColumnType("numeric(18,2)");
        builder.Property(e => e.AvailableCreditLimitAmt).HasColumnName("available_credit_limit_amt").HasColumnType("numeric(18,2)");
        builder.Property(e => e.Salesperson).HasColumnName("salesperson").HasMaxLength(50);
        builder.Property(e => e.IsActive).HasColumnName("is_active");

        builder.HasOne(e => e.Party).WithMany().HasForeignKey(e => e.PartyId);
    }
}
