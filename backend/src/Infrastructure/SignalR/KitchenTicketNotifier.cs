using Application.Abstractions;
using Application.Kds.Queries;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.SignalR;

// This notifier is only used in API layer after registration there; keep IHubContext untyped via dynamic hub name to avoid Api reference.
public sealed class KitchenTicketNotifier(IHubContext<Hub> hub) : IKitchenTicketNotifier
{
    private const string HubName = "/hubs/kds"; // informational
    public Task TicketBatchCreatedAsync(IEnumerable<KitchenTicketDto> tickets, CancellationToken ct = default)
    => hub.Clients.All.SendAsync("ticketsCreated", tickets, ct);
    public Task TicketChangedAsync(KitchenTicketDto ticket, CancellationToken ct = default)
    => hub.Clients.All.SendAsync("ticketChanged", ticket, ct);
}
