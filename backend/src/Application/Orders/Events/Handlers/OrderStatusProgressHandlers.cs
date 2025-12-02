using Application.Abstractions;
using Domain.Events;
using Domain.Enums;

namespace Application.Orders.Events.Handlers;

public sealed class OrderInProgressNotifyHandler(ICustomerNotifier notifier) : IDomainEventHandler<OrderInProgress>
{
    public Task HandleAsync(OrderInProgress domainEvent, CancellationToken ct = default)
        => notifier.OrderStatusChangedAsync(domainEvent.OrderId, OrderStatus.InProgress.ToString(), ct);
}

public sealed class OrderReadyNotifyHandler(ICustomerNotifier notifier) : IDomainEventHandler<OrderReady>
{
    public Task HandleAsync(OrderReady domainEvent, CancellationToken ct = default)
        => notifier.OrderStatusChangedAsync(domainEvent.OrderId, OrderStatus.Ready.ToString(), ct);
}

public sealed class OrderServedNotifyHandler(ICustomerNotifier notifier) : IDomainEventHandler<OrderServed>
{
    public Task HandleAsync(OrderServed domainEvent, CancellationToken ct = default)
        => notifier.OrderStatusChangedAsync(domainEvent.OrderId, OrderStatus.Served.ToString(), ct);
}
