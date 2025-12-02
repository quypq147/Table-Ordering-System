using Application.Abstractions;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Dashboard;

public sealed record ListDashboardMetricsQuery() : IQuery<DashboardMetricsDto>;

public sealed class ListDashboardMetricsHandler
 : IQueryHandler<ListDashboardMetricsQuery, DashboardMetricsDto>
{
    private readonly IApplicationDbContext _db;

    public ListDashboardMetricsHandler(IApplicationDbContext db) => _db = db;

    public async Task<DashboardMetricsDto> Handle(ListDashboardMetricsQuery q, CancellationToken ct)
    {
        // Lấy “hôm nay” theo UTC (nếu bạn muốn theo VN, dùng TimeZoneInfo để chuyển đổi)
        var start = DateTime.UtcNow.Date;
        var end = start.AddDays(1);

        // ======= BÀN =======
        var totalTables = await _db.Tables.CountAsync(ct);
        var activeTables = await _db.Tables.CountAsync(t => t.Status != TableStatus.Available, ct);

        // ======= ĐƠN HÔM NAY (đếm chạy trên DB) =======
        var ordersTodayQry = _db.Orders
        .Where(o => o.CreatedAtUtc >= start && o.CreatedAtUtc < end);

        var ordersToday = await ordersTodayQry.CountAsync(ct);
        var ordersInProgress = await ordersTodayQry.CountAsync(o => o.OrderStatus == OrderStatus.InProgress, ct);
        var ordersReady = await ordersTodayQry.CountAsync(o => o.OrderStatus == OrderStatus.Ready, ct);

        // ======= TÍNH TIỀN (materialize sang client) =======
        //1) Các đơn đã thanh toán hôm nay + kèm Items
        var paidOrdersToday = await _db.Orders
        .Where(o => o.PaidAtUtc != null && o.PaidAtUtc >= start && o.PaidAtUtc < end)
        .Include("Items") // owned collection => Include bằng tên navigation để chắc chắn load backing field
        .AsNoTracking()
        .ToListAsync(ct);

        //2) Doanh thu hôm nay = sum(UnitPrice.Amount * Quantity)
        var revenueToday = paidOrdersToday
        .SelectMany(o => o.Items)
        .Sum(i => i.UnitPrice.Amount * i.Quantity.Value);

        //3) Doanh thu theo giờ
        var labels = Enumerable.Range(0, 24).Select(h => $"{h:00}h").ToArray();
        var hourly = new decimal[24];
        foreach (var o in paidOrdersToday)
        {
            var h = o.PaidAtUtc!.Value.Hour;
            var total = o.Items.Sum(i => i.UnitPrice.Amount * i.Quantity.Value);
            hourly[h] += total;
        }
        var values = hourly;

        // ======= Recent orders (10 đơn gần nhất hôm nay, tính total ở client) =======
        var recentOrders = await _db.Orders
        .Where(o => o.CreatedAtUtc >= start && o.CreatedAtUtc < end)
        .OrderByDescending(o => o.CreatedAtUtc)
        .Include("Items")
        .AsNoTracking()
        .Take(10)
        .ToListAsync(ct);

        var recentDtos = recentOrders.Select(o => new RecentOrderDto(
        o.Id,
        o.Code,
        o.OrderStatus.ToString(),
        o.Items.Sum(i => i.UnitPrice.Amount * i.Quantity.Value),
        o.CreatedAtUtc.ToLocalTime().ToString("HH:mm")
        )).ToList();

        return new DashboardMetricsDto(
        totalTables,
        activeTables,
        ordersToday,
        ordersInProgress,
        ordersReady,
        revenueToday,
        "VND",
        labels,
        values,
        recentDtos
        );
    }
}
