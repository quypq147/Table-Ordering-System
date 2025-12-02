using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;
using Domain.Repositories;

namespace Application.Public.Cart;

/// <summary>
/// G?i / xác nh?n ??n hàng t? tr?ng thái Draft (public API variant).
/// </summary>
public sealed record SubmitCartCommand(Guid OrderId, string? CustomerNote)
    : ICommand<OrderDto>;

public sealed class SubmitCartHandler
    : ICommandHandler<SubmitCartCommand, OrderDto>
{
    private readonly IOrderRepository _orders;
    private readonly IUnitOfWork _uow;

    public SubmitCartHandler(IOrderRepository orders, IUnitOfWork uow)
    {
        _orders = orders;
        _uow = uow;
    }

    public async Task<OrderDto> Handle(SubmitCartCommand cmd, CancellationToken ct)
    {
        var order = await _orders.GetByIdAsync(cmd.OrderId)
            ?? throw new KeyNotFoundException("Không tìm th?y ??n.");

        // No direct API on Order for changing customer note in domain.
        // If you want to store a note on order, add a domain method and mapping accordingly.

        // Domain method to submit order
        order.Submit();

        _orders.Update(order);
        await _uow.SaveChangesAsync(ct);

        return OrderMapper.ToDto(order);
    }
}
