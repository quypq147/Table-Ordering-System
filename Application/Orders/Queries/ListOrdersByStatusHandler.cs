using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            var orders = await _db.Orders
                .Where(o => o.Status.ToString() == q.Status)
                .OrderByDescending(o => o.CreatedAtUtc) // có CreatedAtUtc trong Domain :contentReference[oaicite:20]{index=20}
                .Skip(skip).Take(q.PageSize)
                .Include(o => o.Items)
                .ToListAsync(ct);

            return orders.Select(OrderMapper.ToDto).ToList();
        }
    }
}
