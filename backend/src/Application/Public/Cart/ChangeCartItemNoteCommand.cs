using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;
using Domain.Repositories;

namespace Application.Public.Cart;

public sealed record ChangeCartItemNoteCommand(Guid OrderId, int OrderItemId, string? Note)
 : ICommand<OrderDto>;

public sealed class ChangeCartItemNoteHandler
 : ICommandHandler<ChangeCartItemNoteCommand, OrderDto>
{
    private readonly IOrderRepository _orders;
    private readonly IUnitOfWork _uow;

    public ChangeCartItemNoteHandler(IOrderRepository orders, IUnitOfWork uow)
    {
        _orders = orders;
        _uow = uow;
    }

    public async Task<OrderDto> Handle(ChangeCartItemNoteCommand cmd, CancellationToken ct)
    {
        var order = await _orders.GetByIdAsync(cmd.OrderId)
        ?? throw new KeyNotFoundException("Không tìm th?y ??n");
        order.ChangeItemNote(cmd.OrderItemId, cmd.Note);
        _orders.Update(order);
        await _uow.SaveChangesAsync(ct);
        return OrderMapper.ToDto(order);
    }
}
