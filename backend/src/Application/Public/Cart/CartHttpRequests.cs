namespace Application.Public.Cart;

/// <summary>
/// Body cho API xóa m?t món kh?i gi? hàng.
/// </summary>
public sealed record RemoveCartItemRequest(int OrderItemId);

/// <summary>
/// Body cho API g?i / xác nh?n ??n hàng.
/// Có th? m? r?ng thêm field n?u sau này c?n.
/// </summary>
public sealed record SubmitOrderRequest(string? CustomerNote);
