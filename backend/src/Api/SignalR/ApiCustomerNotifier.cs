using Application.Abstractions;
using Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Api.SignalR;

public sealed class ApiCustomerNotifier : ICustomerNotifier
{
    private readonly IHubContext<CustomerHub> _hub;

    public ApiCustomerNotifier(IHubContext<CustomerHub> hub)
    {
        _hub = hub;
    }

    public Task OrderStatusChangedAsync(Guid orderId, string status, CancellationToken ct = default)
    {
        var payload = new { orderId, status };
        // notify c? group c?a order, và group staff dashboard
        return Task.WhenAll(
            _hub.Clients.Group($"order-{orderId}").SendAsync("orderStatusChanged", payload, ct),
            _hub.Clients.Group("staff").SendAsync("orderStatusChanged", payload, ct)
        );
    }

    public Task OrderPaidAsync(Guid orderId, decimal amount, string currency, string method, DateTime paidAtUtc, CancellationToken ct = default)
    {
        return _hub.Clients.Group($"order-{orderId}")
            .SendAsync("orderPaid", new { orderId, amount, currency, method, paidAtUtc }, ct);
    }
}
