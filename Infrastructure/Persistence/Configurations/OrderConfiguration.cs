using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Domain.ValueObjects;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> b)
    {
        b.ToTable("Orders");
        b.HasKey(x => x.Id);

        b.Property(x => x.TableId).IsRequired().HasMaxLength(64);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(16).IsRequired();
        b.Property(x => x.CreatedAtUtc).IsRequired();

        b.HasIndex(x => new { x.TableId, x.Status });
        b.HasIndex(x => x.CreatedAtUtc);

        b.Property<byte[]>("RowVersion").IsRowVersion();

        // Converters
        var moneyToDecimal = new ValueConverter<Money, decimal>(
            v => v.Amount,
            v => new Money(v, "VND")
        );
        var quantityToInt = new ValueConverter<Quantity, int>(
            v => v.Value,
            v => new Quantity(v)
        );

        // 1 Order - nhiều OrderItem (owned)
        b.OwnsMany(x => x.Items, nb =>
        {
            nb.ToTable("OrderItems");
            nb.WithOwner().HasForeignKey("OrderId");

            nb.HasKey(i => i.Id);                         // dùng Id int có sẵn trong OrderItem
            nb.Property(i => i.Id).ValueGeneratedOnAdd(); // identity

            nb.Property(i => i.MenuItemId).IsRequired().HasMaxLength(64);
            nb.Property(i => i.NameSnapshot).IsRequired().HasMaxLength(200);

            nb.Property(i => i.UnitPrice)
              .HasConversion(moneyToDecimal)    
              .HasColumnName("UnitPrice")
              .HasPrecision(18, 2)
              .IsRequired();

            nb.Property(i => i.Quantity)
              .HasConversion(quantityToInt)
              .HasColumnName("Quantity")
              .IsRequired();

            // Không lưu LineTotal (computed), bỏ qua nếu có property get-only
            // nb.Ignore(i => i.LineTotal);

            nb.HasIndex(i => i.MenuItemId);
        });
    }
}






