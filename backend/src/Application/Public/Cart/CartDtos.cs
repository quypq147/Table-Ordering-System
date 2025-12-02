namespace Application.Public.Cart;

public sealed record CartItemDto(
 int Id,
 Guid MenuItemId,
 string Name,
 decimal UnitPrice,
 int Quantity,
 string? Note,
 decimal LineTotal
);

public sealed record CartDto(
 Guid OrderId,
 string TableCode,
 string Status,
 List<CartItemDto> Items,
 decimal Subtotal,
 decimal ServiceCharge,
 decimal Tax,
 decimal Total
);
