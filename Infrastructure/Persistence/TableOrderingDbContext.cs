using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class TableOrderingDbContext : DbContext
{
    public TableOrderingDbContext(DbContextOptions<TableOrderingDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<RestaurantTable> Tables => Set<RestaurantTable>();
    public DbSet<Voucher> Vouchers => Set<Voucher>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // Order
        mb.Entity<Order>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasMany(x => x.Items)
             .WithOne()
             .HasForeignKey("OrderId")
             .OnDelete(DeleteBehavior.Cascade);

            // nếu Order có thuộc tính Status (enum) → lưu int
            // e.Property(x => x.Status).HasConversion<int>();
        });

        // OrderItem (Money, Quantity là owned types)
        mb.Entity<OrderItem>(e =>
        {
            e.HasKey(x => x.Id);

            e.OwnsOne(x => x.UnitPrice, b =>
            {
                b.Property(p => p.Amount).HasColumnType("decimal(18,2)");
                b.Property(p => p.Currency).HasMaxLength(10);
            });

            e.OwnsOne(x => x.Quantity, b =>
            {
                b.Property(q => q.Value).HasColumnName("Quantity");
            });
        });

        // MenuItem.Price (owned)
        mb.Entity<MenuItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.OwnsOne(x => x.Price, b =>
            {
                b.Property(p => p.Amount).HasColumnType("decimal(18,2)");
                b.Property(p => p.Currency).HasMaxLength(10);
            });
        });

        // RestaurantTable, Voucher: thêm HasKey/Owned nếu cần
    }
}

