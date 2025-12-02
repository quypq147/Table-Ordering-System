using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;
using Application.Orders.Queries;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders.Commands;

public sealed class GetOrderByIdHandler : IQueryHandler<GetOrderByIdQuery, OrderDto?>
{
    private readonly IApplicationDbContext _db;
    public GetOrderByIdHandler(IApplicationDbContext db) => _db = db;

    public async Task<OrderDto?> Handle(GetOrderByIdQuery q, CancellationToken ct)
    {
        // Vì Total()/LineTotal() là method Domain, đừng project thẳng SQL:
        var order = await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == q.OrderId, ct);

        return order is null ? null : OrderMapper.ToDto(order);
    }
}

