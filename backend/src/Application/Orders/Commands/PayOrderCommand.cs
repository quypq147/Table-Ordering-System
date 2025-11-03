// Application/Orders/Commands/PayOrderCommand.cs
using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;
using Domain.Repositories;
using Domain.ValueObjects;

namespace Application.Orders.Commands;

public sealed record PayOrderCommand(
    Guid OrderId,
    decimal Amount,
    string Currency,
    string Method = "CASH" // <— thêm method, có default
) : ICommand<OrderDto>;

public sealed class PayOrderHandler : ICommandHandler<PayOrderCommand, OrderDto>
{
    private readonly IOrderRepository _orders;
    private readonly IUnitOfWork _uow;

    public PayOrderHandler(IOrderRepository orders, IUnitOfWork uow)
    {
        _orders = orders; _uow = uow;
    }

    public async Task<OrderDto> Handle(PayOrderCommand command, CancellationToken ct)
    {
        var order = await _orders.GetByIdAsync(command.OrderId)
                    ?? throw new InvalidOperationException("không tìm thấy đơn.");

        // TRUYỀN ĐỦ HAI THAM SỐ CHO DOMAIN
        var method = string.IsNullOrWhiteSpace(command.Method) ? "CASH" : command.Method.Trim();
        order.Pay(new Money(command.Amount, command.Currency), method);

        _orders.Update(order);
        await _uow.SaveChangesAsync(ct);
        return OrderMapper.ToDto(order);
    }
}
