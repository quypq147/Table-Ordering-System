using Application.Abstractions;
using Application.Dtos;
using Application.Kds.Queries;
using Application.Mappings;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;


namespace Application.Orders.Commands
{
    public sealed class MarkServedHandler : ICommandHandler<MarkServedCommand, OrderDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly IKitchenTicketNotifier _notifier;

        public MarkServedHandler(IApplicationDbContext db, IKitchenTicketNotifier notifier)
        {
            _db = db;
            _notifier = notifier;
        }

        public async Task<OrderDto> Handle(MarkServedCommand cmd, CancellationToken ct)
        {
            var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == cmd.OrderId, ct)
                        ?? throw new KeyNotFoundException("Order not found");

            // Mark order served (domain rules for order are enforced inside entity)
            order.MarkServed();

            // Sync all kitchen tickets for this order
            var tickets = await _db.KitchenTickets
                .Where(t => t.OrderId == cmd.OrderId)
                .ToListAsync(ct);

            if (tickets.Count > 0)
            {
                // Fetch order/table info once for DTO mapping (align with KDS expectations)
                var orderInfo = await _db.Orders.AsNoTracking()
                    .Where(o => o.Id == cmd.OrderId)
                    .Select(o => new { o.Code, o.TableId })
                    .FirstOrDefaultAsync(ct);

                var orderCode = orderInfo?.Code ?? string.Empty;
                var tableCode = string.Empty;
                if (orderInfo?.TableId != null && orderInfo.TableId != Guid.Empty)
                {
                    var table = await _db.Tables.AsNoTracking()
                        .Where(t => t.Id == orderInfo.TableId)
                        .Select(t => new { t.Code })
                        .FirstOrDefaultAsync(ct);
                    if (table is not null)
                    {
                        tableCode = table.Code ?? string.Empty;
                    }
                }

                foreach (var ticket in tickets)
                {
                    if (ticket.Status is KitchenTicketStatus.Served or KitchenTicketStatus.Cancelled)
                        continue;

                    if (ticket.Status == KitchenTicketStatus.New)
                    {
                        ticket.Start();
                    }

                    if (ticket.Status == KitchenTicketStatus.InProgress)
                    {
                        ticket.MarkReady();
                    }

                    if (ticket.Status == KitchenTicketStatus.Ready)
                    {
                        ticket.MarkServed();
                    }

                    var dto = ticket.ToDto(orderCode, tableCode, tableCode);
                    await _notifier.TicketChangedAsync(dto, ct);
                }
            }

            await _db.SaveChangesAsync(ct);
            return OrderMapper.ToDto(order);
        }
    }
}
