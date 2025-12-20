using Application.Abstractions;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Dashboard;

public sealed record ListStatisticsQuery(DateTime? FromUtc, DateTime? ToUtc, int Top = 5) : IQuery<StatisticsDto>;

public sealed class ListStatisticsHandler : IQueryHandler<ListStatisticsQuery, StatisticsDto>
{
    private readonly IApplicationDbContext _db;

    public ListStatisticsHandler(IApplicationDbContext db) => _db = db;

    public async Task<StatisticsDto> Handle(ListStatisticsQuery q, CancellationToken ct)
    {
        var to = (q.ToUtc ?? DateTime.UtcNow).ToUniversalTime();
        var from = (q.FromUtc ?? to.AddDays(-6).Date).ToUniversalTime();
        if (from > to) (from, to) = (to, from);

        // include the whole 'to' day when caller passes a Date-only value
        var toExclusive = to.Kind == DateTimeKind.Utc ? to : DateTime.SpecifyKind(to, DateTimeKind.Utc);

        var tablesTotal = await _db.Tables.CountAsync(ct);
        var activeTables = await _db.Tables.CountAsync(t => t.Status != TableStatus.Available, ct);

        var ordersInRangeQry = _db.Orders.Where(o => o.CreatedAtUtc >= from && o.CreatedAtUtc < toExclusive);
        var ordersTotal = await ordersInRangeQry.CountAsync(ct);
        var ordersCancelled = await ordersInRangeQry.CountAsync(o => o.OrderStatus == OrderStatus.Cancelled, ct);
        var ordersPaid = await ordersInRangeQry.CountAsync(o => o.OrderStatus == OrderStatus.Paid, ct);

        var paidOrders = await _db.Orders
            .Where(o => o.PaidAtUtc != null && o.PaidAtUtc >= from && o.PaidAtUtc < toExclusive)
            .Include("Items")
            .AsNoTracking()
            .ToListAsync(ct);

        decimal RevenueOf(Order o) => o.Items.Sum(i => i.UnitPrice.Amount * i.Quantity.Value);

        var revenueTotal = paidOrders.Sum(RevenueOf);

        // Revenue by day
        var days = Enumerable.Range(0, (int)Math.Ceiling((toExclusive.Date - from.Date).TotalDays) + 1)
            .Select(i => from.Date.AddDays(i))
            .Where(d => d < toExclusive.Date.AddDays(1))
            .ToList();

        var revenueByDay = days
            .Select(d => new RevenuePointDto(d.ToString("yyyy-MM-dd"), 0m))
            .ToList();

        var idx = revenueByDay.ToDictionary(x => x.Date, x => x);
        foreach (var o in paidOrders)
        {
            var d = o.PaidAtUtc!.Value.Date.ToString("yyyy-MM-dd");
            if (idx.TryGetValue(d, out var p))
            {
                idx[d] = p with { Total = p.Total + RevenueOf(o) };
            }
        }
        revenueByDay = revenueByDay.Select(p => idx[p.Date]).ToList();

        // Top items by qty
        var top = Math.Clamp(q.Top, 1, 50);
        var topItems = paidOrders
            .SelectMany(o => o.Items)
            .GroupBy(i => i.MenuItemId)
            .Select(g => new
            {
                MenuItemId = g.Key,
                Name = g.OrderByDescending(x => x.Id).First().NameSnapshot,
                Qty = g.Sum(x => x.Quantity.Value),
                Total = g.Sum(x => x.UnitPrice.Amount * x.Quantity.Value)
            })
            .OrderByDescending(x => x.Qty)
            .Take(top)
            .Select(x => new TopItemDto(x.MenuItemId, x.Name, x.Qty, x.Total))
            .ToList();

        return new StatisticsDto(
            FromUtc: from,
            ToUtc: toExclusive,
            TotalTables: tablesTotal,
            ActiveTables: activeTables,
            OrdersTotal: ordersTotal,
            OrdersPaid: ordersPaid,
            OrdersCancelled: ordersCancelled,
            RevenueTotal: revenueTotal,
            Currency: "VND",
            RevenueByDay: revenueByDay,
            TopItems: topItems
        );
    }
}
