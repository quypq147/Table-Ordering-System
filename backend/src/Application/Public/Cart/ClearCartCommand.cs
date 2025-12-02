using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;
using Domain.Repositories;

namespace Application.Public.Cart;

// US14: Xóa toàn b? gi? hàng
public sealed record ClearCartCommand(Guid OrderId) : ICommand<OrderDto>;

public sealed class ClearCartHandler : ICommandHandler<ClearCartCommand, OrderDto>
{
    private readonly IOrderRepository _orders;
    private readonly IUnitOfWork _uow;
    public ClearCartHandler(IOrderRepository orders, IUnitOfWork uow)
    { _orders = orders; _uow = uow; }

    public async Task<OrderDto> Handle(ClearCartCommand cmd, CancellationToken ct)
    {
        var order = await _orders.GetByIdAsync(cmd.OrderId)
        ?? throw new KeyNotFoundException("Không tìm th?y ??n");
        order.ClearItems();
        _orders.Update(order);
        await _uow.SaveChangesAsync(ct);
        return OrderMapper.ToDto(order);
    }
}
