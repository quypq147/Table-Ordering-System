using System.Collections.Generic;

namespace Application.Dtos;

public record OrderItemDto(string MenuItemId, string Name, decimal UnitPrice, string Currency, int Quantity, decimal LineTotal);
public record OrderDto(string Id, string TableId, string Status, IReadOnlyList<OrderItemDto> Items, decimal Total, string Currency);