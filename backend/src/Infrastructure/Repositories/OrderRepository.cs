using Domain.Entities;
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
}
