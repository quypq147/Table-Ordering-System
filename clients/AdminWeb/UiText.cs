namespace AdminWeb;

public static class UiText
{
    public static string OrderStatusLabel(string status) => status switch
    {
        "Placed" => "Mới tạo",
        "InKitchen" => "Đang chế biến",
        "Ready" => "Sẵn sàng",
        "Served" => "Đã phục vụ",
        "Cancelled" => "Đã hủy",
        "Paid" => "Đã thanh toán",
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

    // Update: Table status helpers to match enum:0=Available,1=Occupied,2=Reserved,3=InUse
    public static string TableStatusLabel(int status) => status switch
    {
        0 => "Trống",
        1 => "Đang có khách",
        2 => "Đã đặt trước",
        3 => "Đang sử dụng",
        _ => "Không rõ"
    };

    public static string TableStatusBadgeClass(int status) => status switch
    {
        0 => "bg-success",
        1 => "bg-danger",
        2 => "bg-primary",
        3 => "bg-warning text-dark",
        _ => "bg-secondary"
    };

    public static (string Label, string BadgeClass) TableStatusInfo(int status)
    => (TableStatusLabel(status), TableStatusBadgeClass(status));
}
