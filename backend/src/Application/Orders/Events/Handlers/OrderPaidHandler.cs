using Application.Abstractions;
using Domain.Events;
using Microsoft.Extensions.Logging;

namespace Application.Orders.Events.Handlers;

public sealed class OrderPaidHandler(ILogger<OrderPaidHandler> logger) : IDomainEventHandler<OrderPaid>
{
 public Task HandleAsync(OrderPaid domainEvent, CancellationToken ct = default)
 {
 logger.LogInformation("OrderPaid handled for OrderId={OrderId}, Amount={Amount} {Currency}", domainEvent.OrderId, domainEvent.Amount, domainEvent.Currency);
 // TODO: update dashboard metrics here
 return Task.CompletedTask;
 }
}
