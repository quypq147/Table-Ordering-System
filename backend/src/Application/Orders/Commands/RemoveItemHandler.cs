// Application/Orders/Commands/RemoveItemHandler.cs
using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;
using Domain.Repositories;

namespace Application.Orders.Commands;

public sealed class RemoveItemHandler : ICommandHandler<RemoveItemCommand, OrderDto>
{
    private readonly IOrderRepository _orders;
    private readonly IUnitOfWork _uow;

    public RemoveItemHandler(IOrderRepository orders, IUnitOfWork uow)
    {
        _orders = orders; _uow = uow;
    }

    public async Task<OrderDto> Handle(RemoveItemCommand cmd, CancellationToken ct)
    {
        var order = await _orders.GetByIdAsync(cmd.OrderId)
                    ?? throw new KeyNotFoundException("Order not found");

        // TODO: Domain nên có phương thức rõ ràng, ví dụ:
        // order.RemoveItemByMenuItemId(cmd.MenuItemId);
        // hoặc tốt hơn: đổi command nhận orderItemId rồi gọi order.RemoveItem(orderItemId);

        // TẠM THỜI: nếu đã có method phù hợp:
        // order.RemoveItemByMenuItemId(cmd.MenuItemId);

        _orders.Update(order);
        await _uow.SaveChangesAsync(ct);
        return OrderMapper.ToDto(order);
    }
}

