using Application.Abstractions;
using Domain.Entities;
using Domain.Enums;
using Application.Kds.Queries;

namespace Application.Kds.Commands;

public sealed record ChangeTicketStatusCommand(Guid TicketId, string Action) : ICommand<KitchenTicketDto>;

public sealed class ChangeTicketStatusHandler(IApplicationDbContext db, IKitchenTicketNotifier notifier) : ICommandHandler<ChangeTicketStatusCommand, KitchenTicketDto>
{
 public async Task<KitchenTicketDto> Handle(ChangeTicketStatusCommand cmd, CancellationToken ct)
 {
 var ticket = await db.KitchenTickets.FindAsync([cmd.TicketId], ct);
 if (ticket is null) throw new KeyNotFoundException("Ticket không t?n t?i");
 var action = cmd.Action?.Trim().ToLowerInvariant();
 switch (action)
 {
 case "start": ticket.Start(); break;
 case "done": ticket.MarkReady(); break;
 case "served": ticket.MarkServed(); break;
 default: throw new InvalidOperationException("Action không h?p l? (start|done|served)");
 }
 await db.SaveChangesAsync(ct);
 var dto = ticket.ToDto();
 await notifier.TicketChangedAsync(dto, ct);
 return dto;
 }
}
