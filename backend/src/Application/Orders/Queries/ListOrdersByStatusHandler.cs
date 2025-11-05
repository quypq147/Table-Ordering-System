using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders.Queries
{
    public sealed class ListOrdersByStatusHandler
    : IQueryHandler<ListOrdersByStatusQuery, IReadOnlyList<OrderDto>>
    {
        private readonly IApplicationDbContext _db;
        public ListOrdersByStatusHandler(IApplicationDbContext db) => _db = db;

        public async Task<IReadOnlyList<OrderDto>> Handle(ListOrdersByStatusQuery q, CancellationToken ct)
        {
            var skip = (q.Page - 1) * q.PageSize;
            if (!Enum.TryParse<OrderStatus>(q.Status, true, out var status))
                throw new ArgumentException("Invalid status", nameof(q.Status));
            var orders = await _db.Orders
                .Where(o => o.OrderStatus == status)
                .OrderByDescending(o => o.CreatedAtUtc)
                .Skip(skip).Take(q.PageSize)
                .Include(o => o.Items)
                .ToListAsync(ct);

            return orders.Select(OrderMapper.ToDto).ToList();
        }
    }
}
