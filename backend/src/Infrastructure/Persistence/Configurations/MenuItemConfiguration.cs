using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Persistence.Configurations;

public sealed class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> b)
    {
        b.ToTable("MenuItems");
        b.HasKey(x => x.Id);

        b.Property(x => x.Name).IsRequired().HasMaxLength(200);
        b.Property(x => x.Sku).IsRequired().HasMaxLength(64);
        b.HasIndex(x => x.Sku).IsUnique();

        b.Property(x => x.IsActive).HasDefaultValue(true);

        b.Property(x => x.Price)
         .HasConversion(Converters.Converters.MoneyToDecimal)
         .HasPrecision(18, 2)
         .IsRequired();

        // Required Category + index for filtering and listing
        b.Property(x => x.CategoryId).IsRequired();
        b.HasIndex(x => new { x.CategoryId, x.IsActive });

        b.HasOne(x => x.Category)
         .WithMany()
         .HasForeignKey(x => x.CategoryId)
         .OnDelete(DeleteBehavior.Restrict);

        // Optional row version for optimistic concurrency
        b.Property<byte[]>("RowVersion").IsRowVersion();
    }
}



