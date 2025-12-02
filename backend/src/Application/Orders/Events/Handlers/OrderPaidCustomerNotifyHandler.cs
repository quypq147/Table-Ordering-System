using Application.Abstractions;
using Domain.Events;

namespace Application.Orders.Events.Handlers;

public sealed class OrderPaidCustomerNotifyHandler(ICustomerNotifier notifier) : IDomainEventHandler<OrderPaid>
{
    public Task HandleAsync(OrderPaid domainEvent, CancellationToken ct = default)
        => notifier.OrderPaidAsync(domainEvent.OrderId, domainEvent.Amount, domainEvent.Currency, domainEvent.Method, DateTime.UtcNow, ct);
}
