// Infrastructure/Persistence/TableOrderingDbContext.cs
using Application.Abstractions;
using Domain.Entities;
using Infrastructure.Identity;                                // ⬅️ thêm
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;      // ⬅️ thêm
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Infrastructure.Persistence;

public sealed class TableOrderingDbContext
    : IdentityDbContext<AppUser, AppRole, Guid>, IApplicationDbContext // ⬅️ đổi base & dùng interface KHÔNG generic
{
    public TableOrderingDbContext(DbContextOptions<TableOrderingDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Table> Tables => Set<Table>(); // ⬅️ đổi cho khớp entity
    public DbSet<Voucher> Vouchers => Set<Voucher>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);
        mb.ApplyConfigurationsFromAssembly(typeof(TableOrderingDbContext).Assembly);
        mb.HasSequence<int>("CategoryNoSeq").StartsAt(1).IncrementsBy(1);
        mb.HasSequence<int>("MenuItemNoSeq").StartsAt(1).IncrementsBy(1);
        mb.HasSequence<int>("OrderNoSeq").StartsAt(1).IncrementsBy(1);
        mb.HasSequence<int>("TableNoSeq").StartsAt(1).IncrementsBy(1);
    }

    // Nếu IApplicationDbContext yêu cầu:
    Task<int> IApplicationDbContext.SaveChangesAsync(CancellationToken ct) => base.SaveChangesAsync(ct);
}





