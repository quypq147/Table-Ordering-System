// Infrastructure/Persistence/TableOrderingDbContext.cs
using Application.Abstractions;
using Domain.Abstractions;
using Domain.Entities;
using Infrastructure.Identity;                                // ⬅️ thêm
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;      // ⬅️ thêm
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Infrastructure.Persistence;

public sealed class TableOrderingDbContext
    : IdentityDbContext<AppUser, AppRole, Guid>, IApplicationDbContext // ⬅️ đổi base & dùng interface KHÔNG generic
{
    private readonly IDomainEventDispatcher _dispatcher;

    public TableOrderingDbContext(DbContextOptions<TableOrderingDbContext> options, IDomainEventDispatcher dispatcher) : base(options)
    {
        _dispatcher = dispatcher;
    }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Table> Tables => Set<Table>(); // ⬅️ đổi cho khớp entity
    public DbSet<Voucher> Vouchers => Set<Voucher>();
    public DbSet<KitchenTicket> KitchenTickets => Set<KitchenTicket>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);
        mb.ApplyConfigurationsFromAssembly(typeof(TableOrderingDbContext).Assembly);
        mb.HasSequence<int>("CategoryNoSeq").StartsAt(1).IncrementsBy(1);
        mb.HasSequence<int>("MenuItemNoSeq").StartsAt(1).IncrementsBy(1);
        mb.HasSequence<int>("OrderNoSeq").StartsAt(1).IncrementsBy(1);
        mb.HasSequence<int>("TableNoSeq").StartsAt(1).IncrementsBy(1);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        // Collect and clear domain events before save
        var domainEventEntities = ChangeTracker.Entries<IHasDomainEvents>()
            .Select(e => e.Entity)
            .ToList();
        var domainEvents = domainEventEntities
            .SelectMany(e => e.DomainEvents)
            .ToList();
        foreach (var e in domainEventEntities)
        {
            e.ClearDomainEvents();
        }

        var result = await base.SaveChangesAsync(ct);

        if (domainEvents.Count > 0)
        {
            await _dispatcher.DispatchAsync(domainEvents, ct);
        }

        return result;
    }

    // Nếu IApplicationDbContext yêu cầu:
    Task<int> IApplicationDbContext.SaveChangesAsync(CancellationToken ct) => SaveChangesAsync(ct);
}









