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
        b.Property(x => x.Code).IsRequired().HasMaxLength(32);
        b.HasIndex(x => x.Code).IsUnique();
        // Single definition (32 chars) to fit 'WaitingForPayment'
        b.Property(x => x.OrderStatus).HasConversion<string>().HasMaxLength(32).IsRequired();
        b.Property(x => x.CreatedAtUtc).IsRequired();

        // sequence for Number
        b.Property(x => x.Number)
            .HasDefaultValueSql("NEXT VALUE FOR OrderNoSeq")
            .ValueGeneratedOnAdd();

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
            nb.Property(i => i.Note).HasMaxLength(512);

            nb.HasIndex(i => i.MenuItemId);
        });
    }
}







