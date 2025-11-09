using Application.Abstractions;
using Domain.Events;
using Domain.Repositories;
using Microsoft.Extensions.Logging;
using Domain.Entities;
using Application.Kds.Queries;

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
 logger.LogWarning("Order {OrderId} không tìm th?y khi x? lý OrderSubmitted", domainEvent.OrderId);
 return;
 }
 var createdTickets = new List<KitchenTicketDto>();
 foreach (var item in order.Items)
 {
 var ticket = new KitchenTicket(Guid.NewGuid(), order.Id, item.Id, item.NameSnapshot, item.Quantity.Value);
 await db.KitchenTickets.AddAsync(ticket, ct);
 createdTickets.Add(ticket.ToDto());
 }
 await db.SaveChangesAsync(ct);
 logger.LogInformation("?ã t?o {Count} kitchen tickets cho Order {OrderId}", order.Items.Count, order.Id);
 if (createdTickets.Count >0)
 {
 await notifier.TicketBatchCreatedAsync(createdTickets, ct);
 }
 }
}
