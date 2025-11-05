namespace AdminWeb.Services.Models;

public sealed class DashboardVm
{
 public int TotalTables { get; set; }
 public int ActiveTables { get; set; }
 public int OrdersToday { get; set; }
 public int OrdersInProgress { get; set; }
 public int OrdersReady { get; set; }
 public decimal RevenueToday { get; set; }
 public string Currency { get; set; } = "VND";
 public string[] RevenueByHourLabels { get; set; } = Array.Empty<string>();
 public decimal[] RevenueByHourValues { get; set; } = Array.Empty<decimal>();
 public RecentOrderVm[] RecentOrders { get; set; } = Array.Empty<RecentOrderVm>();
}

public sealed class RecentOrderVm
{
 public Guid Id { get; set; }
 public string Code { get; set; } = "";
 public string Status { get; set; } = "";
 public decimal Total { get; set; }
 public string Created { get; set; } = "";
}
