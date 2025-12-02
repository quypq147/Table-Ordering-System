using TableOrdering.Contracts;

namespace AdminWeb.Models;

public sealed class TablesIndexVm
{
 public List<DiningTableDto> Tables { get; }
 public IReadOnlyDictionary<string, OrderSummaryDto> ActiveOrdersByTableCode { get; }

 public TablesIndexVm(List<DiningTableDto> tables, IDictionary<string, OrderSummaryDto> activeOrders)
 {
 Tables = tables ?? new();
 ActiveOrdersByTableCode = new Dictionary<string, OrderSummaryDto>(activeOrders ?? new Dictionary<string, OrderSummaryDto>(), StringComparer.OrdinalIgnoreCase);
 }

 public bool HasActiveOrder(string tableCode)
 => tableCode != null && ActiveOrdersByTableCode.ContainsKey(tableCode);

 public OrderSummaryDto? GetActiveOrder(string tableCode)
 => tableCode != null && ActiveOrdersByTableCode.TryGetValue(tableCode, out var o) ? o : null;
}
