namespace Application.Dashboard;

public sealed record StatisticsDto(
    DateTime FromUtc,
    DateTime ToUtc,
    int TotalTables,
    int ActiveTables,
    int OrdersTotal,
    int OrdersPaid,
    int OrdersCancelled,
    decimal RevenueTotal,
    string Currency,
    IReadOnlyList<RevenuePointDto> RevenueByDay,
    IReadOnlyList<TopItemDto> TopItems
);

public sealed record RevenuePointDto(string Date, decimal Total);

