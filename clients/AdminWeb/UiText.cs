namespace AdminWeb;

public static class UiText
{
 public static string OrderStatusLabel(string status) => status switch
 {
 "Placed" => "M?i t?o",
 "InKitchen" => "?ang ch? bi?n",
 "Ready" => "S?n sàng",
 "Served" => "?ã ph?c v?",
 "Cancelled" => "?ã h?y",
 "Paid" => "?ã thanh toán",
 _ => status
 };

 public static string OrderStatusBadgeClass(string status) => status switch
 {
 "Placed" => "bg-secondary",
 "InKitchen" => "bg-info text-dark",
 "Ready" => "bg-primary",
 "Served" => "bg-success",
 "Cancelled" => "bg-danger",
 "Paid" => "bg-success",
 _ => "bg-secondary"
 };
}
