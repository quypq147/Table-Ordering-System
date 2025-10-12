using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> b)
    {
        b.ToTable("MenuItems");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasMaxLength(64);

        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.IsActive).HasDefaultValue(true);

        // Value Object: Money Price => Owned
        b.OwnsOne(x => x.Price, money =>
        {
            money.Property(p => p.Amount).HasColumnType("decimal(18,2)").IsRequired();
            money.Property(p => p.Currency).HasMaxLength(10).IsRequired(); // "VND", "USD", ...
            money.WithOwner();
        });

        b.HasIndex(x => x.Name);
    }
}

