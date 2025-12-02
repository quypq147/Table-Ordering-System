namespace AdminWeb;

public static class UiText
{
    public static string OrderStatusLabel(string status)
    {
        // Nếu backend trả về số (enum underlying value)
        if (int.TryParse(status, out var code))
        {
            // Giả định mapping: 0=Draft,1=Submitted,2=InProgress,3=Ready,4=Served,5=WaitingForPayment,6=Paid,7=Cancelled
            return code switch
            {
                0 => "Đang chọn món",
                1 => "Đã gửi bếp",
                2 => "Đang chế biến",
                3 => "Món sẵn sàng",
                4 => "Đã phục vụ",
                5 => "Đang chờ thanh toán",
                6 => "Đã thanh toán",
                7 => "Đã hủy",
                _ => status // trả về nguyên giá trị nếu không nằm trong mapping
            };
        }

        return status switch
        {
            // Backward compatibility (legacy labels)
            "Placed" => "Mới tạo",
            "InKitchen" => "Đang chế biến",
            // Domain enum labels
            "Draft" => "Đang chọn món",
            "Submitted" => "Đã gửi bếp",
            "InProgress" => "Đang chế biến",
            "Ready" => "Món sẵn sàng",
            "Served" => "Đã phục vụ",
            "WaitingForPayment" => "Đang chờ thanh toán",
            "Paid" => "Đã thanh toán",
            "Cancelled" => "Đã hủy",
            _ => status
        };
    }

    public static string OrderStatusBadgeClass(string status)
    {
        if (int.TryParse(status, out var code))
        {
            return code switch
            {
                0 => "bg-secondary",      // Draft
                1 => "bg-info text-dark", // Submitted
                2 => "bg-warning text-dark", // InProgress
                3 => "bg-primary",        // Ready
                4 => "bg-success",        // Served
                5 => "bg-warning text-dark", // WaitingForPayment
                6 => "bg-success",        // Paid
                7 => "bg-danger",         // Cancelled
                _ => "bg-secondary"
            };
        }

        return status switch
        {
            "Placed" => "bg-secondary",
            "InKitchen" => "bg-info text-dark",
            "Draft" => "bg-secondary",
            "Submitted" => "bg-info text-dark",
            "InProgress" => "bg-warning text-dark",
            "Ready" => "bg-primary",
            "Served" => "bg-success",
            "WaitingForPayment" => "bg-warning text-dark",
            "Paid" => "bg-success",
            "Cancelled" => "bg-danger",
            _ => "bg-secondary"
        };
    }

    // Table status helpers: 0=Available,1=Occupied,2=Reserved,3=InUse
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
