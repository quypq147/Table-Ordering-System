namespace Application.Dashboard;

public sealed record DashboardMetricsDto(
 int TotalTables,
 int ActiveTables,
 int OrdersToday,
 int OrdersInProgress,
 int OrdersReady,
 decimal RevenueToday,
 string Currency,
 IReadOnlyList<string> RevenueByHourLabels,
 IReadOnlyList<decimal> RevenueByHourValues,
 IReadOnlyList<RecentOrderDto> RecentOrders,
 IReadOnlyList<DashboardChartDataDto> RevenueLast7Days,
    IReadOnlyList<TopItemDto> TopItems
);
public sealed record DashboardChartDataDto(
    string Date,
    decimal Total
);
public sealed record TopItemDto(
    Guid Id,
    string Name,
    int Qty,
    decimal Total
);
public sealed record RecentOrderDto(
 Guid Id,
 string Code,
 string Status,
 decimal Total,
 string Created // hh:mm
);
