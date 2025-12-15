using Application.Abstractions;
using Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Api.SignalR;

public sealed class ApiCustomerNotifier(IHubContext<CustomerHub> hub) : ICustomerNotifier
{
    private readonly IHubContext<CustomerHub> _hub = hub;

    public Task OrderStatusChangedAsync(Guid orderId, string status, CancellationToken ct = default)
    {
        // Send separate parameters to align with clients listening for Guid + string arguments
        return Task.WhenAll(
            _hub.Clients.Group($"order-{orderId}").SendAsync("orderStatusChanged", orderId, status, ct),
            _hub.Clients.Group("staff").SendAsync("orderStatusChanged", orderId, status, ct)
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
        // Chỉ gửi cho nhân viên (Waiter App)
        var payload = new { OrderId = orderId, TableCode = tableCode };
        return _hub.Clients.Group("staff").SendAsync("ReceivePaymentRequest", payload, ct);
    }
}