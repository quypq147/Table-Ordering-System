using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public sealed class OrderRepository : IOrderRepository
{
    private readonly TableOrderingDbContext _db;
    public OrderRepository(TableOrderingDbContext db) => _db = db;

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id, ct);

    public Task AddAsync(Order order) => _db.Orders.AddAsync(order).AsTask();

    public void Update(Order order) => _db.Orders.Update(order);

    public Task<Order?> GetActiveOrderByTableIdAsync(Guid tableId, CancellationToken ct = default) =>
        _db.Orders
            .Include(o => o.Items)
            .Where(o => o.TableId == tableId && (o.OrderStatus == OrderStatus.Submitted || o.OrderStatus == OrderStatus.InProgress))
            .OrderByDescending(o => o.CreatedAtUtc)
            .FirstOrDefaultAsync(ct);
}
