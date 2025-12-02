using Application.Kds.Queries;

namespace Application.Abstractions;

public interface IKitchenTicketNotifier
{
    Task TicketBatchCreatedAsync(IEnumerable<KitchenTicketDto> tickets, CancellationToken ct = default);
    Task TicketChangedAsync(KitchenTicketDto ticket, CancellationToken ct = default);
}
