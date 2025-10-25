using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Infrastructure.Repositories;

public sealed class OrderRepository : IOrderRepository
{
    private readonly TableOrderingDbContext _db;
    public OrderRepository(TableOrderingDbContext db) => _db = db;

    public Task<Order?> GetByIdAsync(string id)
        => _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);

    public Task AddAsync(Order order)
        => _db.Orders.AddAsync(order).AsTask();

    public void Update(Order order) => _db.Orders.Update(order);
}