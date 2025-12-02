using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;
using Domain.Repositories;

namespace Application.Public.Cart;

/// <summary>
/// Xóa m?t món kh?i gi? hàng theo OrderItemId.
/// </summary>
public sealed record RemoveCartItemCommand(Guid OrderId, int OrderItemId)
    : ICommand<OrderDto>;

public sealed class RemoveCartItemHandler
    : ICommandHandler<RemoveCartItemCommand, OrderDto>
{
    private readonly IOrderRepository _orders;
    private readonly IUnitOfWork _uow;

    public RemoveCartItemHandler(IOrderRepository orders, IUnitOfWork uow)
    {
        _orders = orders;
        _uow = uow;
    }

    public async Task<OrderDto> Handle(RemoveCartItemCommand cmd, CancellationToken ct)
    {
        var order = await _orders.GetByIdAsync(cmd.OrderId)
            ?? throw new KeyNotFoundException("Không tìm th?y ??n.");

        // Try to reuse existing domain method name. If it differs, adjust accordingly.
        // The domain currently has RemoveItem(int orderItemId) as a command in Orders.
        order.RemoveItem(cmd.OrderItemId);

        _orders.Update(order);
        await _uow.SaveChangesAsync(ct);

        return OrderMapper.ToDto(order);
    }
}
