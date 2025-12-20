using Application.Abstractions;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Dashboard;

public sealed record ListDashboardMetricsQuery() : IQuery<DashboardMetricsDto>;

public sealed class ListDashboardMetricsHandler : IQueryHandler<ListDashboardMetricsQuery, DashboardMetricsDto>
{
    private readonly IApplicationDbContext _db;

    public ListDashboardMetricsHandler(IApplicationDbContext db) => _db = db;

    public async Task<DashboardMetricsDto> Handle(ListDashboardMetricsQuery q, CancellationToken ct)
    {
        // 1. Xác định khung thời gian
        var today = DateTime.UtcNow.Date;
        var end = today.AddDays(1);         // Hết ngày hôm nay
        var start = today;                  // Đầu ngày hôm nay
        var sevenDaysAgo = today.AddDays(-6); // 7 ngày gần nhất (tính cả hôm nay)

        // ======= BÀN (Giữ nguyên) =======
        var totalTables = await _db.Tables.CountAsync(ct);
        var activeTables = await _db.Tables.CountAsync(t => t.Status != TableStatus.Available, ct);

        // ======= ĐƠN HÔM NAY (Giữ nguyên) =======
        var ordersTodayQry = _db.Orders.Where(o => o.CreatedAtUtc >= start && o.CreatedAtUtc < end);
        var ordersToday = await ordersTodayQry.CountAsync(ct);
        var ordersInProgress = await ordersTodayQry.CountAsync(o => o.OrderStatus == OrderStatus.InProgress, ct);
        var ordersReady = await ordersTodayQry.CountAsync(o => o.OrderStatus == OrderStatus.Ready, ct);

        // ======= DỮ LIỆU THỐNG KÊ (7 NGÀY) =======
        // Lấy tất cả đơn đã thanh toán trong 7 ngày qua để tính toán cho cả 3 mục: 
        // (Doanh thu hôm nay, Doanh thu 7 ngày, Top món)
        var paidOrdersRange = await _db.Orders
            .Where(o => o.PaidAtUtc != null && o.PaidAtUtc >= sevenDaysAgo && o.PaidAtUtc < end)
            .Include("Items")
            .AsNoTracking()
            .ToListAsync(ct);

        // A. Tính Doanh thu hôm nay & Theo giờ (Giữ nguyên logic cũ nhưng lọc từ list memory)
        var paidOrdersToday = paidOrdersRange.Where(o => o.PaidAtUtc >= start).ToList();

        var revenueToday = paidOrdersToday
            .SelectMany(o => o.Items)
            .Sum(i => i.UnitPrice.Amount * i.Quantity.Value);

        var labels = Enumerable.Range(0, 24).Select(h => $"{h:00}h").ToArray();
        var hourly = new decimal[24];
        foreach (var o in paidOrdersToday)
        {
            var h = o.PaidAtUtc!.Value.Hour;
            var total = o.Items.Sum(i => i.UnitPrice.Amount * i.Quantity.Value);
            hourly[h] += total;
        }

        // B. [New] Tính Doanh thu 7 ngày (Cho biểu đồ cột)
        var rev7DaysData = new List<DashboardChartDataDto>();
        for (int i = 0; i < 7; i++)
        {
            var date = sevenDaysAgo.AddDays(i);
            // Lọc các đơn thuộc ngày 'date'
            var ordersInDay = paidOrdersRange.Where(o => o.PaidAtUtc!.Value.Date == date);
            var totalInDay = ordersInDay.SelectMany(o => o.Items).Sum(x => x.UnitPrice.Amount * x.Quantity.Value);

            rev7DaysData.Add(new DashboardChartDataDto(date.ToString("dd/MM"), totalInDay));
        }

        // C. [New] Tính Top 5 món bán chạy (Trong 7 ngày qua)
        var topItems = paidOrdersRange
            .SelectMany(o => o.Items)
            .GroupBy(i => i.MenuItemId) // Group theo ID món
            .Select(g => new
            {
                // Tạo object ẩn danh tạm thời để tính toán
                Id = g.Key,
                Name = g.First().NameSnapshot,
                Qty = g.Sum(x => x.Quantity.Value),
                Total = g.Sum(x => x.UnitPrice.Amount * x.Quantity.Value)
            })
            .OrderByDescending(x => x.Qty)
            .Take(5)
            .Select(x => new TopItemDto(
                // Chuyển đổi sang TopItemDto đúng chuẩn
                x.Id,
                x.Name,
                x.Qty,
                x.Total
            ))
            .ToList();

        // ======= Recent orders (Giữ nguyên) =======
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
            hourly,
            recentDtos,
            rev7DaysData, // Truyền vào DTO
            topItems      // Truyền vào DTO
        );
    }
}