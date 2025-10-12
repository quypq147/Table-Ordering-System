using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;
using Domain.Repositories;

namespace Application.Orders.Commands;

public record SubmitOrderCommand(string OrderId) : ICommand<OrderDto>;

public class SubmitOrderHandler : ICommandHandler<SubmitOrderCommand, OrderDto>
{
    private readonly IOrderRepository _orders;
    private readonly IUnitOfWork _uow;

    public SubmitOrderHandler(IOrderRepository orders, IUnitOfWork uow)
    {
        _orders = orders; _uow = uow;
    }

    public async Task<OrderDto> Handle(SubmitOrderCommand command, CancellationToken ct)
    {
        var order = await _orders.GetByIdAsync(command.OrderId) ?? throw new InvalidOperationException("Order not found.");
        order.Submit();
        _orders.Update(order);
        await _uow.SaveChangesAsync(ct);
        return OrderMapper.ToDto(order);
    }
}