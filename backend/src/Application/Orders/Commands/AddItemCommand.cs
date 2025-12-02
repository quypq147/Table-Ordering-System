using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;
using Domain.Repositories;
using Domain.ValueObjects;

namespace Application.Orders.Commands;

public record AddItemCommand(Guid OrderId, Guid MenuItemId, string Name, decimal Price, string Currency, int Quantity) : ICommand<OrderDto>;

public class AddItemHandler : ICommandHandler<AddItemCommand, OrderDto>
{
    private readonly IOrderRepository _orders;
    private readonly IUnitOfWork _uow;

    public AddItemHandler(IOrderRepository orders, IUnitOfWork uow)
    {
        _orders = orders;
        _uow = uow;
    }

    public async Task<OrderDto> Handle(AddItemCommand command, CancellationToken ct)
    {
        var order = await _orders.GetByIdAsync(command.OrderId) ?? throw new InvalidOperationException("Không tìm thấy đơn.");
        order.AddItem(command.MenuItemId, command.Name, new Money(command.Price, command.Currency), new Quantity(command.Quantity));
        _orders.Update(order);
        await _uow.SaveChangesAsync(ct);
        return OrderMapper.ToDto(order);
    }
}