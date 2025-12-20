namespace AdminWeb.Services.Models;

public sealed class StatisticsVm
{
    public DateTime FromUtc { get; set; }
    public DateTime ToUtc { get; set; }

    public int TotalTables { get; set; }
    public int ActiveTables { get; set; }

    public int OrdersTotal { get; set; }
    public int OrdersPaid { get; set; }
    public int OrdersCancelled { get; set; }

    public decimal RevenueTotal { get; set; }
    public string Currency { get; set; } = "VND";

    public RevenuePointVm[] RevenueByDay { get; set; } = Array.Empty<RevenuePointVm>();
    public TopItemVm[] TopItems { get; set; } = Array.Empty<TopItemVm>();
}

public sealed class RevenuePointVm
{
    public string Date { get; set; } = "";
    public decimal Total { get; set; }
}

public sealed class TopItemVm
{
    public Guid MenuItemId { get; set; }
    public string Name { get; set; } = "";
    public int TotalQty { get; set; }
    public decimal TotalAmount { get; set; }
}
