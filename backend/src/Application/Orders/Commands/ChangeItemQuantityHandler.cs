// Application/Orders/Commands/ChangeItemQuantityHandler.cs
using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;
using Domain.Repositories;

namespace Application.Orders.Commands;

public sealed class ChangeItemQuantityHandler
    : ICommandHandler<ChangeItemQuantityCommand, OrderDto>
{
    private readonly IOrderRepository _orders;
    private readonly IUnitOfWork _uow;
    public ChangeItemQuantityHandler(IOrderRepository orders, IUnitOfWork uow)
    { _orders = orders; _uow = uow; }

    public async Task<OrderDto> Handle(ChangeItemQuantityCommand cmd, CancellationToken ct)
    {
        var order = await _orders.GetByIdAsync(cmd.OrderId)
                    ?? throw new KeyNotFoundException("Khong tim thay don");
        order.ChangeItemQuantity(cmd.OrderItemId, cmd.NewQuantity); // domain method
        _orders.Update(order);
        await _uow.SaveChangesAsync(ct);
        return OrderMapper.ToDto(order);
    }
}

