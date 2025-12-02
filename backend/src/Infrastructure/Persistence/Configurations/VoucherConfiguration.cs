using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public sealed class VoucherConfiguration : IEntityTypeConfiguration<Voucher>
{
    public void Configure(EntityTypeBuilder<Voucher> b)
    {
        b.ToTable("Vouchers");
        b.HasKey(x => x.Id);

        b.Property(x => x.Code).IsRequired().HasMaxLength(32);
        b.HasIndex(x => x.Code).IsUnique();

        b.Property(x => x.Discount).HasPrecision(18, 2).IsRequired();
        b.Property(x => x.DiscountType).HasConversion<string>().HasMaxLength(16).IsRequired();

        var dateOnly = new ValueConverter<DateOnly, DateTime>(
            d => d.ToDateTime(TimeOnly.MinValue),
            dt => DateOnly.FromDateTime(dt));

        b.Property(x => x.CreatedAt).HasConversion(dateOnly).HasColumnType("date").IsRequired();
        b.Property(x => x.ExpirationDate).HasConversion(dateOnly).HasColumnType("date").IsRequired();
    }
}


