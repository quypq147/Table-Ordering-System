using Application.Abstractions;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Infrastructure.Persistence;

public sealed class TableOrderingDbContext : DbContext, IApplicationDbContext
{
    public TableOrderingDbContext(DbContextOptions<TableOrderingDbContext> options)
        : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<RestaurantTable> Tables => Set<RestaurantTable>();
    public DbSet<Voucher> Vouchers => Set<Voucher>();
    // Lưu ý: KHÔNG có DbSet<OrderItem> khi dùng OwnedCollection (OwnsMany)

    // Explicit interface implementation for IApplicationDbContext
    DbSet<RestaurantTable> IApplicationDbContext.RestaurantTables => Tables;

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // Áp toàn bộ cấu hình từ các class *Configuration trong assembly này
        mb.ApplyConfigurationsFromAssembly(typeof(TableOrderingDbContext).Assembly);
    }

    Task<int> IApplicationDbContext.SaveChangesAsync(CancellationToken ct) => base.SaveChangesAsync(ct);
}


