namespace Application.Public.Cart;

/// <summary>
/// DTO cho việc cập nhật số lượng (Sử dụng bởi PATCH /items/{orderItemId})
/// </summary>
public sealed record UpdateCartItemQuantityRequest(int Quantity);

/// <summary>
/// DTO cho việc cập nhật ghi chú (Sử dụng bởi PATCH /items/{orderItemId}/note)
/// </summary>
public sealed record ChangeCartItemNoteRequest(string? Note);


// Các DTO cho các endpoint khác (giữ lại)

/// <summary>
/// Body cho API xóa một món khỏi giỏ hàng.
/// </summary>
public sealed record RemoveCartItemRequest(int OrderItemId);

/// <summary>
/// Body cho API gửi / xác nhận đơn hàng.
/// </summary>
public sealed record SubmitOrderRequest(string? CustomerNote);