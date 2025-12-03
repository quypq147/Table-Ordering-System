using Application.Abstractions;
using Application.Kds.Queries;
using Microsoft.EntityFrameworkCore;

namespace Application.Kds.Commands;

public sealed record ChangeTicketStatusCommand(Guid TicketId, string Action) : ICommand<KitchenTicketDto>;

public sealed class ChangeTicketStatusHandler(IApplicationDbContext db, IKitchenTicketNotifier notifier) : ICommandHandler<ChangeTicketStatusCommand, KitchenTicketDto>
{
    public async Task<KitchenTicketDto> Handle(ChangeTicketStatusCommand cmd, CancellationToken ct)
    {
        var ticket = await db.KitchenTickets.FindAsync(new object[] { cmd.TicketId }, ct);
        if (ticket is null) throw new KeyNotFoundException("Ticket không ton tai");
        var action = cmd.Action?.Trim().ToLowerInvariant();
        switch (action)
        {
            case "start": ticket.Start(); break;
            case "done": ticket.MarkReady(); break;
            case "served": ticket.MarkServed(); break;
            case "cancel": ticket.Cancel("Cancelled from KDS"); break;
            default: throw new InvalidOperationException("Action không hop le (start|done|served)");
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

        var dto = ticket.ToDto(orderCode, tableName, tableCode);
        await notifier.TicketChangedAsync(dto, ct);
        return dto;
    }
}
