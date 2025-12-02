using Application.Abstractions;
using Application.Kds.Queries;
using Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Api.SignalR;

public sealed class ApiKitchenTicketNotifier : IKitchenTicketNotifier
{
    private readonly IHubContext<KdsHub> _hub;

    public ApiKitchenTicketNotifier(IHubContext<KdsHub> hub)
    {
        _hub = hub;
    }

    public Task TicketBatchCreatedAsync(IEnumerable<KitchenTicketDto> tickets, CancellationToken ct = default)
    {
        // G?i event "ticketsCreated" cho t?t c? KDS client
        return _hub.Clients.All.SendAsync("ticketsCreated", tickets, ct);
    }

    public Task TicketChangedAsync(KitchenTicketDto ticket, CancellationToken ct = default)
    {
        // G?i event "ticketChanged" cho t?t c? KDS client
        return _hub.Clients.All.SendAsync("ticketChanged", ticket, ct);
    }
}
