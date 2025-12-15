using Api.Hubs;
using Application.Abstractions;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.SignalR;

public sealed class CustomerNotifier(IHubContext<CustomerHub> hub) : ICustomerNotifier
{
    private readonly IHubContext<CustomerHub> _hub = hub;

    public Task OrderStatusChangedAsync(Guid orderId, string status, CancellationToken ct = default)
    {
        var payload = new { orderId, status };
        return Task.WhenAll(
            _hub.Clients.Group($"order-{orderId}").SendAsync("orderStatusChanged", payload, ct),
            _hub.Clients.Group("staff").SendAsync("orderStatusChanged", payload, ct)
        );
    }

    public Task OrderPaidAsync(Guid orderId, decimal amount, string currency, string method, DateTime paidAtUtc, CancellationToken ct = default)
    {
        var payload = new { orderId, amount, currency, method, paidAtUtc };
        return Task.WhenAll(
            _hub.Clients.Group($"order-{orderId}").SendAsync("orderPaid", payload, ct),
            _hub.Clients.Group("staff").SendAsync("orderPaid", payload, ct)
        );
    }

    public Task CashPaymentRequestedAsync(Guid orderId, string tableCode, CancellationToken ct = default)
    {
        var payload = new { OrderId = orderId, TableCode = tableCode };
        return _hub.Clients.Group("staff").SendAsync("ReceivePaymentRequest", payload, ct);
    }
}
