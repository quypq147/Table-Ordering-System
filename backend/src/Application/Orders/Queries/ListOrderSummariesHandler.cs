using Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders.Queries;

public sealed record ListOrderSummariesQuery(int Page = 1, int PageSize = 50) : IQuery<IReadOnlyList<OrderSummaryDto>>;
public sealed record OrderSummaryDto(Guid Id, Guid TableId, string TableCode, string Status, int PendingItems, DateTime CreatedAtUtc);

public sealed class ListOrderSummariesHandler : IQueryHandler<ListOrderSummariesQuery, IReadOnlyList<OrderSummaryDto>>
{
    private readonly IApplicationDbContext _db;
    public ListOrderSummariesHandler(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<OrderSummaryDto>> Handle(ListOrderSummariesQuery q, CancellationToken ct)
    {
        var skip = Math.Max(0, (q.Page - 1) * q.PageSize);

        // 1) Load orders + items từ DB
        var orders = await _db.Orders
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAtUtc)
            .Skip(skip).Take(q.PageSize)
            .AsNoTracking()
            .ToListAsync(ct);

        // 2) (option tốt hơn) Preload TableCode để tránh N+1 query
        var tableIds = orders.Select(o => o.TableId).Distinct().ToList();

        var tableCodes = await _db.Tables
            .Where(t => tableIds.Contains(t.Id))
            .ToDictionaryAsync(t => t.Id, t => t.Code, ct);

        // 3) Map sang DTO, tính PendingItems trên memory
        var result = orders
            .Select(o =>
            {
                var tableCode = tableCodes.TryGetValue(o.TableId, out var code)
                    ? code
                    : string.Empty;

                var pendingItems = (o.OrderStatus == Domain.Enums.OrderStatus.Submitted
                                    || o.OrderStatus == Domain.Enums.OrderStatus.InProgress)
                    ? o.Items.Count(i => i.Quantity.Value > 0)   // giờ này chạy trên memory, không còn EF dịch SQL nữa
                    : 0;

                return new OrderSummaryDto(
                    o.Id,
                    o.TableId,
                    tableCode,
                    o.OrderStatus.ToString(),
                    pendingItems,
                    o.CreatedAtUtc
                );
            })
            .ToList();

        return result;
    }
}
