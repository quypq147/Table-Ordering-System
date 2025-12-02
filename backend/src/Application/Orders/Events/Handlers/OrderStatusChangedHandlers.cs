using Application.Abstractions;
using Domain.Entities;
using Domain.Enums;
using Domain.Events;
using Domain.Repositories;

namespace Application.Orders.Events.Handlers;

public sealed class OrderSubmittedStatusNotifyHandler(ICustomerNotifier notifier) : IDomainEventHandler<OrderSubmitted>
{
    public Task HandleAsync(OrderSubmitted domainEvent, CancellationToken ct = default)
        => notifier.OrderStatusChangedAsync(domainEvent.OrderId, OrderStatus.Submitted.ToString(), ct);
}

public sealed class OrderCancelledStatusNotifyHandler(ICustomerNotifier notifier) : IDomainEventHandler<OrderCancelled>
{
    public Task HandleAsync(OrderCancelled domainEvent, CancellationToken ct = default)
        => notifier.OrderStatusChangedAsync(domainEvent.OrderId, OrderStatus.Cancelled.ToString(), ct);
}

public sealed class OrderPaidStatusNotifyHandler(ICustomerNotifier notifier) : IDomainEventHandler<OrderPaid>
{
    public Task HandleAsync(OrderPaid domainEvent, CancellationToken ct = default)
        => notifier.OrderStatusChangedAsync(domainEvent.OrderId, OrderStatus.Paid.ToString(), ct);
}

public sealed class OrderProgressStatusNotifyHandler(ICustomerNotifier notifier) :
    IDomainEventHandler<OrderInProgress>,
    IDomainEventHandler<OrderReady>,
    IDomainEventHandler<OrderServed>
{
    public Task HandleAsync(OrderInProgress domainEvent, CancellationToken ct = default)
        => notifier.OrderStatusChangedAsync(domainEvent.OrderId, OrderStatus.InProgress.ToString(), ct);
    public Task HandleAsync(OrderReady domainEvent, CancellationToken ct = default)
        => notifier.OrderStatusChangedAsync(domainEvent.OrderId, OrderStatus.Ready.ToString(), ct);
    public Task HandleAsync(OrderServed domainEvent, CancellationToken ct = default)
        => notifier.OrderStatusChangedAsync(domainEvent.OrderId, OrderStatus.Served.ToString(), ct);
}
