// Application/Dtos/OrderDtos.cs
using System;
using System.Collections.Generic;
using Domain.Enums;

namespace Application.Dtos;

public sealed record OrderItemDto(
    int OrderItemId,
    Guid MenuItemId,
    string Name,
    decimal UnitPrice,
    string Currency,
    int Quantity,
    decimal LineTotal
);

public sealed record OrderDto(
    Guid Id,
    Guid TableId,
    OrderStatus Status,
    IReadOnlyList<OrderItemDto> Items,
    decimal Total,
    string Currency
);

