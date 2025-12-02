using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders.Queries;

public sealed class ListOrdersHandler
    : IQueryHandler<ListOrdersQuery, IReadOnlyList<OrderDto>>
{
    private readonly IApplicationDbContext _db;
    public ListOrdersHandler(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<OrderDto>> Handle(ListOrdersQuery q, CancellationToken ct)
    {
        var skip = Math.Max(0, (q.Page - 1) * q.PageSize);

        var orders = await _db.Orders
            .Include(o => o.Items)
            // Nếu Domain của bạn có CreatedAtUtc thì OrderByDescending(o => o.CreatedAtUtc)
            .OrderByDescending(o => o.Id)          // fallback an toàn
            .Skip(skip).Take(q.PageSize)
            .ToListAsync(ct);

        // Nếu bạn đã có OrderMapper.ToDto:
        return orders.Select(OrderMapper.ToDto).ToList();

        // Hoặc map tay theo OrderDto nếu chưa có mapper:
        // return orders.Select(o => new OrderDto(
        //     o.Id, o.TableId, o.Status,
        //     o.Items.Select(i => new OrderItemDto(i.MenuItemId, i.Name, i.UnitPrice.Amount, i.UnitPrice.Currency, i.Quantity.Value, i.UnitPrice.Amount * i.Quantity.Value)).ToList(),
        //     o.Items.Sum(i => i.UnitPrice.Amount * i.Quantity.Value),
        //     o.Items.FirstOrDefault()?.UnitPrice.Currency ?? "VND"
        // )).ToList();
    }
}
