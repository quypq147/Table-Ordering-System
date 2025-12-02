using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;
using Application.Orders.Queries;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders.Commands;

public sealed class ListOrdersByTableHandler
    : IQueryHandler<ListOrdersByTableQuery, IReadOnlyList<OrderDto>>
{
    private readonly IApplicationDbContext _db;
    public ListOrdersByTableHandler(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<OrderDto>> Handle(ListOrdersByTableQuery q, CancellationToken ct)
    {
        var skip = (q.Page - 1) * q.PageSize;

        // Load aggregate + Items rồi map qua OrderMapper (an toàn với phương thức Domain)
        var orders = await _db.Orders
            .Where(o => o.TableId == q.TableId)
            .OrderByDescending(o => o.CreatedAtUtc)       // nếu có CreatedAt
            .Skip(skip).Take(q.PageSize)
            .Include(o => o.Items)
            .ToListAsync(ct);

        return orders.Select(OrderMapper.ToDto).ToList();
    }
}

