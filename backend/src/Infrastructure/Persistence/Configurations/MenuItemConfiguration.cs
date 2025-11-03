using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Persistence.Configurations;

public sealed class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> b)
    {
        b.ToTable("MenuItems");
        b.HasKey(x => x.Id);

        b.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        b.Property(x => x.Sku)
            .IsRequired()
            .HasMaxLength(64);

        b.HasIndex(x => x.Sku).IsUnique();

        b.Property(x => x.IsActive)
            .HasDefaultValue(true);

        // Money -> decimal (default currency "VND")
        var moneyToDecimal = new ValueConverter<Money, decimal>(
            v => v.Amount,
            v => new Money(v, "VND")
        );

        b.Property(x => x.Price)
            .HasConversion(moneyToDecimal)
            .HasColumnName("Price")
            .HasPrecision(18, 2)
            .IsRequired();

        // CategoryId mapping and FK (optional by default due to Guid?)
        b.Property(x => x.CategoryId).IsRequired();
        b.HasIndex(x => x.CategoryId);

        b.HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Concurrency token (shadow)
        b.Property<byte[]>("RowVersion").IsRowVersion();
    }
}



