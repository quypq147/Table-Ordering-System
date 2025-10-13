using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class VoucherConfiguration : IEntityTypeConfiguration<Voucher>
{
    public void Configure(EntityTypeBuilder<Voucher> b)
    {
        b.ToTable("Vouchers");
        b.HasKey(x => x.Id);

        b.Property(x => x.Code).HasMaxLength(64).IsRequired();
        b.HasIndex(x => x.Code).IsUnique();

        b.Property(x => x.Discount).HasColumnType("decimal(18,2)").IsRequired();

        b.Property(x => x.DiscountType)
         .HasConversion<string>()
         .HasMaxLength(32)
         .IsRequired();

        b.Property(x => x.CreatedAt)
         .HasConversion(
             v => v.ToDateTime(TimeOnly.MinValue),
             v => DateOnly.FromDateTime(v))
         .HasColumnType("date")
         .IsRequired();

        b.Property(x => x.ExpirationDate)
         .HasConversion(
             v => v.ToDateTime(TimeOnly.MinValue),
             v => DateOnly.FromDateTime(v))
         .HasColumnType("date")
         .IsRequired();
    }
}

