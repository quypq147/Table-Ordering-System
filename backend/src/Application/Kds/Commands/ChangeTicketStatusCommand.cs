using Application.Abstractions;
using Application.Kds.Queries;
using Microsoft.EntityFrameworkCore;
using Domain.Enums;

namespace Application.Kds.Commands;

public sealed record ChangeTicketStatusCommand(Guid TicketId, string Action) : ICommand<KitchenTicketDto>;

public sealed class ChangeTicketStatusHandler(IApplicationDbContext db, IKitchenTicketNotifier notifier, ICustomerNotifier customerNotifier) : ICommandHandler<ChangeTicketStatusCommand, KitchenTicketDto>
{
    public async Task<KitchenTicketDto> Handle(ChangeTicketStatusCommand cmd, CancellationToken ct)
    {
        var ticket = await db.KitchenTickets.FindAsync(new object[] { cmd.TicketId }, ct);
        if (ticket is null) throw new KeyNotFoundException("Ticket khong ton tai");
        var action = cmd.Action?.Trim().ToLowerInvariant();
        switch (action)
        {
            case "start":
            case "begin":
            case "in-progress":
            case "inprogress":
            case "started":
                ticket.Start();
                break;
            case "done":
            case "ready":
            case "finish":
            case "completed":
            case "complete":
                ticket.MarkReady();
                break;
            case "served":
            case "serve":
            case "delivered":
                ticket.MarkServed();
                break;
            case "cancel":
            case "cancelled":
            case "cancelled-by-kds":
            case "void":
                ticket.Cancel("Cancelled from KDS");
                break;
            default:
                throw new InvalidOperationException("Action khong hop le (start|done|served|cancel)");
        }
        await db.SaveChangesAsync(ct);

        // Try to fetch Order.Code and Table.Code for friendly display
        var orderInfo = await db.Orders.AsNoTracking()
            .Where(o => o.Id == ticket.OrderId)
            .Select(o => new { o.Code, o.TableId })
            .FirstOrDefaultAsync(ct);

        string orderCode = orderInfo?.Code ?? string.Empty;
        string tableName = string.Empty;
        string tableCode = string.Empty;
        if (orderInfo?.TableId != null && orderInfo.TableId != Guid.Empty)
        {
            var table = await db.Tables.AsNoTracking()
                .Where(t => t.Id == orderInfo.TableId)
                .Select(t => new { t.Code })
                .FirstOrDefaultAsync(ct);
            if (table is not null)
            {
                tableName = table.Code ?? string.Empty; // no separate Name property on Table
                tableCode = table.Code ?? string.Empty;
            }
        }

        // Derive aggregate OrderStatus from all kitchen tickets and notify if changed
        var allTickets = await db.KitchenTickets
            .Where(t => t.OrderId == ticket.OrderId)
            .ToListAsync(ct);

        if (allTickets.Count > 0 && orderInfo is not null)
        {
            var statuses = allTickets.Select(t => t.Status).ToList();

            OrderStatus? newOrderStatus = null;

            if (statuses.All(s => s is KitchenTicketStatus.Served or KitchenTicketStatus.Cancelled))
            {
                newOrderStatus = OrderStatus.Served;
            }
            else if (statuses.All(s => s is KitchenTicketStatus.Ready or KitchenTicketStatus.Served or KitchenTicketStatus.Cancelled))
            {
                newOrderStatus = OrderStatus.Ready;
            }
            else if (statuses.Any(s => s is KitchenTicketStatus.InProgress or KitchenTicketStatus.Ready))
            {
                newOrderStatus = OrderStatus.InProgress;
            }

            if (newOrderStatus.HasValue)
            {
                var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == ticket.OrderId, ct);
                if (order is not null)
                {
                    var current = order.OrderStatus;
                    if (newOrderStatus.Value != current)
                    {
                        switch (newOrderStatus.Value)
                        {
                            case OrderStatus.InProgress:
                                if (current == OrderStatus.Submitted)
                                {
                                    order.MarkInProgress();
                                }
                                break;
                            case OrderStatus.Ready:
                                if (current == OrderStatus.Submitted)
                                {
                                    order.MarkInProgress();
                                }
                                if (current == OrderStatus.InProgress || order.OrderStatus == OrderStatus.InProgress)
                                {
                                    order.MarkReady();
                                }
                                break;
                            case OrderStatus.Served:
                                if (current == OrderStatus.Submitted)
                                {
                                    order.MarkInProgress();
                                    order.MarkReady();
                                    order.MarkServed();
                                }
                                else if (current == OrderStatus.InProgress)
                                {
                                    order.MarkReady();
                                    order.MarkServed();
                                }
                                else if (current == OrderStatus.Ready)
                                {
                                    order.MarkServed();
                                }
                                break;
                        }

                        if (!Equals(order.OrderStatus, current))
                        {
                            await db.SaveChangesAsync(ct);
                            await customerNotifier.OrderStatusChangedAsync(order.Id, order.OrderStatus.ToString(), ct);
                        }
                    }
                }
            }
        }

        var dto = ticket.ToDto(orderCode, tableName, tableCode);
        await notifier.TicketChangedAsync(dto, ct);
        return dto;
    }
}
