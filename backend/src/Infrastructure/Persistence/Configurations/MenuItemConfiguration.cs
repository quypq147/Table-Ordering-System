using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Domain.ValueObjects;

public sealed class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> b)
    {
        b.ToTable("MenuItems");
        b.HasKey(x => x.Id);

        b.Property(x => x.Name).IsRequired().HasMaxLength(200);
        b.Property(x => x.IsActive).HasDefaultValue(true);

        // Money -> decimal, luôn đọc/ghi với currency "VND"
        var moneyToDecimal = new ValueConverter<Money, decimal>(
            v => v.Amount,
            v => new Money(v, "VND")
        );

        b.Property(x => x.Price)
         .HasConversion(moneyToDecimal)
         .HasColumnName("Price")
         .HasPrecision(18, 2)
         .IsRequired();

        // (tùy chọn) concurrency
        b.Property<byte[]>("RowVersion").IsRowVersion();
    }
}



