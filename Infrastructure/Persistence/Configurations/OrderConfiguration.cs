using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> b)
    {
        b.ToTable("Orders");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasMaxLength(64);

        b.Property(x => x.TableId).HasMaxLength(64).IsRequired();

        b.Property(x => x.Status)
         .HasConversion<string>()
         .HasMaxLength(32)
         .HasDefaultValue(OrderStatus.Draft);

        b.Property(x => x.CreatedAtUtc).IsRequired();
        b.Property(x => x.SubmittedAtUtc);
        b.Property(x => x.PaidAtUtc);

        // Owned collection: Items (OrderItem không có Id riêng => EF tạo key shadow)
        b.OwnsMany(x => x.Items, oi =>
        {
            oi.ToTable("OrderItems");
            oi.WithOwner().HasForeignKey("OrderId");

            oi.Property<int>("Id"); // key shadow
            oi.HasKey("Id");

            oi.Property(p => p.MenuItemId).HasMaxLength(64).IsRequired();
            oi.Property(p => p.NameSnapshot).HasMaxLength(200).IsRequired();

            // Money UnitPrice: owned
            oi.OwnsOne(p => p.UnitPrice, money =>
            {
                money.Property(m => m.Amount).HasColumnType("decimal(18,2)").IsRequired();
                money.Property(m => m.Currency).HasMaxLength(10).IsRequired();
                money.WithOwner();
            });

            // Quantity: value converter (Quantity <-> int)
            oi.Property(p => p.Quantity)
              .HasConversion(
                  q => q.Value,
                  v => new Quantity(v))
              .IsRequired();

            oi.Property<string>("OrderId").HasMaxLength(64);
            oi.HasIndex(p => p.MenuItemId);
        });

        // Cho phép EF truy cập backing-field của Items
        b.Navigation(x => x.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

