// Infrastructure/Persistence/TableOrderingDbContext.cs
using Infrastructure.Identity;                                // ⬅️ thêm
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;      // ⬅️ thêm
using Application.Abstractions;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class TableOrderingDbContext
    : IdentityDbContext<ApplicationUser>, IApplicationDbContext // ⬅️ đổi base & dùng interface KHÔNG generic
{
    public TableOrderingDbContext(DbContextOptions<TableOrderingDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Table> Tables => Set<Table>(); // ⬅️ đổi cho khớp entity
    public DbSet<Voucher> Vouchers => Set<Voucher>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb); // ⬅️ BẮT BUỘC để tạo bảng AspNetUsers, AspNetRoles, ...
        mb.ApplyConfigurationsFromAssembly(typeof(TableOrderingDbContext).Assembly);
    }

    // Nếu IApplicationDbContext yêu cầu:
    Task<int> IApplicationDbContext.SaveChangesAsync(CancellationToken ct) => base.SaveChangesAsync(ct);
}





