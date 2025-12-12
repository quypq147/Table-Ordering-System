using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;
using Domain.Exceptions;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Application.Public.Orders;

public sealed record RequestCashPaymentCommand(Guid OrderId) : ICommand<OrderDto>;

public sealed class RequestCashPaymentHandler : ICommandHandler<RequestCashPaymentCommand, OrderDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ITableRepository _tables;
    private readonly ICustomerNotifier _notifier;

    public RequestCashPaymentHandler(
        IApplicationDbContext db,
        ITableRepository tables,
        ICustomerNotifier notifier)
    {
        _db = db;
        _tables = tables;
        _notifier = notifier;
    }

    public async Task<OrderDto> Handle(RequestCashPaymentCommand c, CancellationToken ct)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == c.OrderId, ct)
            ?? throw DomainException.NotFound("Order", c.OrderId);

        order.RequestCashPayment();
        await _db.SaveChangesAsync(ct);

        var table = await _tables.GetByIdAsync(order.TableId);
        var tableCode = table?.Code ?? order.TableId.ToString();

        await _notifier.CashPaymentRequestedAsync(order.Id, tableCode, ct);

        return OrderMapper.ToDto(order);
    }
}
