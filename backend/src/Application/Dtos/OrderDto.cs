// Application/Dtos/OrderDtos.cs
using Domain.Enums;

namespace Application.Dtos;

public sealed record OrderItemDto(
    int OrderItemId,
    Guid MenuItemId,
    string Name,
    decimal UnitPrice,
    string Currency,
    int Quantity,
    decimal LineTotal,
    string? Note
);

public sealed record OrderDto(
    Guid Id,
    Guid TableId,
    string OrderCode,
    OrderStatus Status,
    IReadOnlyList<OrderItemDto> Items,
    decimal Total,
    string Currency,
    string? CustomerNote
);

