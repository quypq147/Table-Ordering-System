using System.Linq;
using Application.Dtos;
using Domain.Entities;

namespace Application.Mappings;

public static class OrderMapper
{
    public static OrderDto ToDto(Order o)
    {
        var items = o.Items.Select(i => new OrderItemDto(
            i.MenuItemId,
            i.NameSnapshot,
            i.UnitPrice.Amount,
            i.UnitPrice.Currency,
            i.Quantity.Value,
            i.LineTotal.Amount
        )).ToList();
        var currency = o.Items.FirstOrDefault()?.UnitPrice.Currency ?? "VND";
        return new OrderDto(o.Id, o.TableId, o.Status.ToString(), items, o.Total().Amount, currency);
    }
}