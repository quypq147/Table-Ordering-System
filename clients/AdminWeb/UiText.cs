namespace AdminWeb;

public static class UiText
{
    public static string OrderStatusLabel(string status)
    {
        // Nếu backend trả về số (enum underlying value)
        if (int.TryParse(status, out var code))
        {
            // Correct mapping per Domain.Enums.OrderStatus
            // 0=Draft,1=Submitted,2=InProgress,3=Ready,4=Served,5=Paid,6=Cancelled,7=WaitingForPayment
            return code switch
            {
                0 => "Đang chọn món",
                1 => "Đã gửi bếp",
                2 => "Đang chế biến",
                3 => "Món sẵn sàng",
                4 => "Đã phục vụ",
                5 => "Đã thanh toán",
                6 => "Đã hủy",
                7 => "Đang chờ thanh toán",
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
            // Correct mapping consistent with labels
            return code switch
            {
                0 => "bg-secondary",      // Draft
                1 => "bg-info text-dark", // Submitted
                2 => "bg-warning text-dark", // InProgress
                3 => "bg-primary",        // Ready
                4 => "bg-success",        // Served
                5 => "bg-success",        // Paid
                6 => "bg-danger",         // Cancelled
                7 => "bg-warning text-dark", // WaitingForPayment
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

    // Table status helpers updated comment to reflect actual enum (0=Available,1=InUse)
    public static string TableStatusLabel(int status) => status switch
    {
        0 => "Trống",
        1 => "Đang sử dụng",
        _ => "Không rõ"
    };

    public static string TableStatusBadgeClass(int status) => status switch
    {
        0 => "bg-success",
        1 => "bg-warning text-dark",
        _ => "bg-secondary"
    };

    public static (string Label, string BadgeClass) TableStatusInfo(int status)
        => (TableStatusLabel(status), TableStatusBadgeClass(status));
}
