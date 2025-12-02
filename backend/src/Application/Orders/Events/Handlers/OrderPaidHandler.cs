using Application.Abstractions;
using Domain.Events;
using Microsoft.Extensions.Logging;

namespace Application.Orders.Events.Handlers;

/// <summary>
/// Handles <see cref="OrderPaid"/> domain events.
/// </summary>
/// <remarks>
/// Current implementation logs the payment event. Future enhancements:
/// - update dashboard / metrics (e.g. RevenueToday)
/// - generate an <c>Invoice</c> for the paid order
/// - notify other subsystems (reports, external accounting)
/// </remarks>
public sealed class OrderPaidHandler(ILogger<OrderPaidHandler> logger) : IDomainEventHandler<OrderPaid>
{
    /// <summary>
    /// Handle the <see cref="OrderPaid"/> event.
    /// </summary>
    /// <param name="domainEvent">The domain event containing payment information.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task HandleAsync(OrderPaid domainEvent, CancellationToken ct = default)
    {
        logger.LogInformation("OrderPaid handled for OrderId={OrderId}, Amount={Amount} {Currency}", domainEvent.OrderId, domainEvent.Amount, domainEvent.Currency);

        // TODO: update dashboard metrics here (e.g. increment RevenueToday, RecentOrders)
        // TODO: generate Invoice for the paid order (call application service / command)
        // TODO: notify external systems if required (reporting/accounting)

        return Task.CompletedTask;
    }
}
