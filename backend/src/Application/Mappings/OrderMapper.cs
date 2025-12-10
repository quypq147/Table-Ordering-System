// Application/Mappings/OrderMapper.cs
using Application.Dtos;
using Domain.Entities;

namespace Application.Mappings;

public static class OrderMapper
{
    public static OrderDto ToDto(Order o)
    {
        var items = o.Items.Select(i => new OrderItemDto(
            i.Id,
            i.MenuItemId,
            i.NameSnapshot,
            i.UnitPrice.Amount,
            i.UnitPrice.Currency,
            i.Quantity.Value,
            i.UnitPrice.Amount * i.Quantity.Value,
            i.Note
        )).ToList();

        var total = o.Total();
        return new OrderDto(
            o.Id,
            o.TableId,
            o.Code,
            o.OrderStatus,
            items,
            total.Amount,
            total.Currency,
            o.CustomerNote
        );
    }
}
