using Application.Abstractions;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.SignalR;

public sealed class CustomerNotifier : ICustomerNotifier
{
    // Use base Hub context to avoid referencing Api project (layering)
    private readonly IHubContext<Hub> _hub;
    public CustomerNotifier(IHubContext<Hub> hub) => _hub = hub;

    public Task OrderStatusChangedAsync(Guid orderId, string status, CancellationToken ct = default)
    {
        var payload = new { orderId, status };
        // notify both order group and staff dashboard group
        return Task.WhenAll(
            _hub.Clients.Group($"order-{orderId}").SendAsync("orderStatusChanged", payload, ct),
            _hub.Clients.Group("staff").SendAsync("orderStatusChanged", payload, ct)
        );
    }

    public Task OrderPaidAsync(Guid orderId, decimal amount, string currency, string method, DateTime paidAtUtc, CancellationToken ct = default)
        => _hub.Clients.Group($"order-{orderId}").SendAsync("orderPaid", new { orderId, amount, currency, method, paidAtUtc }, ct);
}
