using Application.Abstractions;
using Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Api.SignalR;

public sealed class ApiCustomerNotifier(IHubContext<CustomerHub> hub, ILogger<ApiCustomerNotifier> logger) : ICustomerNotifier
{
    private readonly IHubContext<CustomerHub> _hub = hub;
    private readonly ILogger<ApiCustomerNotifier> _logger = logger;

    public Task OrderStatusChangedAsync(Guid orderId, string status, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "[ApiCustomerNotifier] OrderStatusChangedAsync: orderId={OrderId}, status={Status}. Broadcasting to order group and staff group.",
            orderId,
            status);

        // Send separate parameters to align with clients listening for Guid + string arguments
        return Task.WhenAll(
            _hub.Clients.Group($"order-{orderId}").SendAsync("orderStatusChanged", orderId, status, ct),
            _hub.Clients.Group("staff").SendAsync("orderStatusChanged", orderId, status, ct)
        );
    }

    public Task OrderPaidAsync(Guid orderId, decimal amount, string currency, string method, DateTime paidAtUtc, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "[ApiCustomerNotifier] OrderPaidAsync: orderId={OrderId}, amount={Amount} {Currency}, method={Method}, paidAtUtc={PaidAtUtc}. Broadcasting to order and staff groups.",
            orderId,
            amount,
            currency,
            method,
            paidAtUtc);

        var payload = new { orderId, amount, currency, method, paidAtUtc };
        return Task.WhenAll(
            _hub.Clients.Group($"order-{orderId}").SendAsync("orderPaid", payload, ct),
            _hub.Clients.Group("staff").SendAsync("orderPaid", payload, ct)
        );
    }

    public Task CashPaymentRequestedAsync(Guid orderId, string tableCode, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "[ApiCustomerNotifier] CashPaymentRequestedAsync: orderId={OrderId}, tableCode={TableCode}. Broadcasting ReceivePaymentRequest to staff group.",
            orderId,
            tableCode);

        // Chỉ gửi cho nhân viên (Waiter App)
        var payload = new { OrderId = orderId, TableCode = tableCode };
        return _hub.Clients.Group("staff").SendAsync("ReceivePaymentRequest", payload, ct);
    }
}