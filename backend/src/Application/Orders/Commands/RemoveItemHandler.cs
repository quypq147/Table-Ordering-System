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
        var order = await _orders.GetByIdAsync(cmd.OrderId, ct)
                    ?? throw new KeyNotFoundException("Không tìm thấy đơn");

        // Domain method nhận OrderItemId (int)
        order.RemoveItem(cmd.OrderItemId);

        _orders.Update(order);
        await _uow.SaveChangesAsync(ct);

        return OrderMapper.ToDto(order);
    }
}

