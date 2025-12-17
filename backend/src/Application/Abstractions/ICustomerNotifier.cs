namespace Application.Abstractions;

public interface ICustomerNotifier
{
    Task OrderStatusChangedAsync(Guid orderId, string status, CancellationToken ct = default);

    Task OrderPaidAsync(
        Guid orderId,
        decimal amount,
        string currency,
        string method,
        DateTime paidAtUtc,
        CancellationToken ct = default);

    Task CashPaymentRequestedAsync(
        Guid orderId,
        string tableCode,
        CancellationToken ct = default);
}
