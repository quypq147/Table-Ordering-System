using Application.Abstractions;
using Application.Kds.Queries;
using Domain.Entities;
using Domain.Events;
using Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Application.Orders.Events.Handlers;

public sealed class OrderSubmittedHandler(
 ILogger<OrderSubmittedHandler> logger,
 IOrderRepository orders,
 IApplicationDbContext db,
 IKitchenTicketNotifier notifier
) : IDomainEventHandler<OrderSubmitted>
{
    public async Task HandleAsync(OrderSubmitted domainEvent, CancellationToken ct = default)
    {
        var order = await orders.GetByIdAsync(domainEvent.OrderId, ct);
        if (order is null)
        {
            logger.LogWarning("Order {OrderId} không tìm thấy khi xử lý OrderSubmitted", domainEvent.OrderId);
            return;
        }
        var createdTickets = new List<Application.Kds.Queries.KitchenTicketDto>();
        foreach (var item in order.Items)
        {
            // Skip items with zero or negative quantity
            if (item.Quantity.Value <= 0)
            {
                logger.LogInformation("Bỏ qua item {ItemId} của order {OrderId} vì quantity = {Quantity}", item.Id, order.Id, item.Quantity.Value);
                continue;
            }

            var ticket = new KitchenTicket(Guid.NewGuid(), order.Id, item.Id, item.NameSnapshot, item.Quantity.Value);
            await db.KitchenTickets.AddAsync(ticket, ct);

            // Create DTO for notifier including order code; TableName will be resolved by KDS query or left empty here
            createdTickets.Add(new Application.Kds.Queries.KitchenTicketDto(
                ticket.Id,
                ticket.OrderId,
                order.Code ?? string.Empty,
                string.Empty, // TableName not available on Order entity
                Guid.Empty,
                ticket.Status.ToString(),
                ticket.ItemName,
                ticket.Quantity,
                ticket.CreatedAtUtc,
                string.Empty // TableCode
            ));
        }
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Đã tạo {Count} kitchen tickets cho Order {OrderId}", createdTickets.Count, order.Id);
        if (createdTickets.Count > 0)
        {
            await notifier.TicketBatchCreatedAsync(createdTickets, ct);
        }
    }
}
