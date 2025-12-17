using Application.Abstractions;
using Application.Invoices.Commands;
using Application.Tables.Commands;
using Application.Common.CQRS; // ISender
using Domain.Events;
using Domain.Repositories;
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
public sealed class OrderPaidHandler(
    ILogger<OrderPaidHandler> logger,
    ISender sender,
    IOrderRepository orderRepository) : IDomainEventHandler<OrderPaid>
{
    /// <summary>
    /// Handle the <see cref="OrderPaid"/> event.
    /// </summary>
    /// <param name="domainEvent">The domain event containing payment information.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task HandleAsync(OrderPaid domainEvent, CancellationToken ct = default)
    {
        logger.LogInformation("OrderPaid handled for OrderId={OrderId}, Amount={Amount} {Currency}", domainEvent.OrderId, domainEvent.Amount, domainEvent.Currency);

        try
        {
            // Generate invoice for the paid order
            await sender.Send(new GenerateInvoiceForOrderCommand(domainEvent.OrderId), ct);

            // Auto release table after payment
            var order = await orderRepository.GetByIdAsync(domainEvent.OrderId, ct);
            if (order != null)
            {
                await sender.Send(new MarkTableAvailableCommand(order.TableId), ct);
                logger.LogInformation("Auto-released Table {TableId} after payment.", order.TableId);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling OrderPaid for {OrderId}", domainEvent.OrderId);
        }

        // TODO: update dashboard metrics here (e.g. increment RevenueToday, RecentOrders)
        // TODO: notify external systems if required (reporting/accounting)
    }
}
