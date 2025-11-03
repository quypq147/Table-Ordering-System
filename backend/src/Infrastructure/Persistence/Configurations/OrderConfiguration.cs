using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> b)
    {
        b.ToTable("Orders");
        b.HasKey(x => x.Id);

        b.Property(x => x.TableId).IsRequired();
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(16).IsRequired();
        b.Property(x => x.OrderStatus).HasConversion<string>().HasMaxLength(16).IsRequired();
        b.Property(x => x.CreatedAtUtc).IsRequired();

        b.HasIndex(x => new { x.TableId, x.OrderStatus });
        b.HasIndex(x => x.CreatedAtUtc);

        // Converters
        var moneyToDecimal = new ValueConverter<Money, decimal>(v => v.Amount, v => new Money(v, "VND"));
        var qtyToInt = new ValueConverter<Quantity, int>(v => v.Value, v => new Quantity(v));

        b.OwnsMany(x => x.Items, nb =>
        {
            nb.ToTable("OrderItems");
            nb.WithOwner().HasForeignKey("OrderId");

            nb.HasKey(i => i.Id);
            nb.Property(i => i.Id).ValueGeneratedOnAdd();

            nb.Property(i => i.MenuItemId).IsRequired();
            nb.Property(i => i.NameSnapshot).IsRequired().HasMaxLength(200);

            nb.Property(i => i.UnitPrice).HasConversion(moneyToDecimal).HasPrecision(18, 2).IsRequired();
            nb.Property(i => i.Quantity).HasConversion(qtyToInt).IsRequired();
            

            nb.HasIndex(i => i.MenuItemId);
        });
    }
}







