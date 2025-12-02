using Application.Abstractions;
using Domain.Enums;
using Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders.Events.Handlers;

public sealed class OrderCancelledHandler(IApplicationDbContext db) : IDomainEventHandler<OrderCancelled>
{
    public async Task HandleAsync(OrderCancelled domainEvent, CancellationToken ct = default)
    {
        // N?u không còn ??n ho?t ??ng t?i bàn => cho bàn Available
        var tableId = await db.Orders
        .Where(o => o.Id == domainEvent.OrderId)
        .Select(o => o.TableId)
        .FirstOrDefaultAsync(ct);
        if (tableId == Guid.Empty) return;
        var hasActive = await db.Orders.AnyAsync(o => o.TableId == tableId && o.OrderStatus != OrderStatus.Paid && o.OrderStatus != OrderStatus.Cancelled && o.Id != domainEvent.OrderId, ct);
        if (!hasActive)
        {
            var table = await db.Tables.FirstOrDefaultAsync(t => t.Id == tableId, ct);
            if (table is not null) { table.MarkAvailable(); await db.SaveChangesAsync(ct); }
        }
    }

    
}