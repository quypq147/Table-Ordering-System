using Domain.Abstractions;

namespace Application.Abstractions;

public interface IDomainEventHandler<in TDomainEvent> where TDomainEvent : IDomainEvent
{
    Task HandleAsync(TDomainEvent domainEvent, CancellationToken ct = default);
}
