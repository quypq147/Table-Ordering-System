// Application/Orders/Queries/GetActiveOrderByTableHandler.cs
using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

public sealed class GetActiveOrderByTableHandler
    : IQueryHandler<GetActiveOrderByTableQuery, OrderDto?>
{
    private readonly IApplicationDbContext _db;
    public GetActiveOrderByTableHandler(IApplicationDbContext db) => _db = db;

    public async Task<OrderDto?> Handle(GetActiveOrderByTableQuery q, CancellationToken ct)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .Where(o => o.TableId == q.TableId && o.OrderStatus != OrderStatus.Paid && o.OrderStatus != OrderStatus.Cancelled)
            .OrderByDescending(o => o.CreatedAtUtc)
            .FirstOrDefaultAsync(ct);
        return order is null ? null : OrderMapper.ToDto(order);
    }
}

