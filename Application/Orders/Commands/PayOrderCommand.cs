using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;
using Domain.Repositories;
using Domain.ValueObjects;

namespace Application.Orders.Commands;

public record PayOrderCommand(string OrderId, decimal Amount, string Currency) : ICommand<OrderDto>;

public class PayOrderHandler : ICommandHandler<PayOrderCommand, OrderDto>
{
    private readonly IOrderRepository _orders;
    private readonly IUnitOfWork _uow;

    public PayOrderHandler(IOrderRepository orders, IUnitOfWork uow)
    {
        _orders = orders; _uow = uow;
    }

    public async Task<OrderDto> Handle(PayOrderCommand command, CancellationToken ct)
    {
        var order = await _orders.GetByIdAsync(command.OrderId) ?? throw new InvalidOperationException("Order not found.");
        order.Pay(new Money(command.Amount, command.Currency));
        _orders.Update(order);
        await _uow.SaveChangesAsync(ct);
        return OrderMapper.ToDto(order);
    }
}