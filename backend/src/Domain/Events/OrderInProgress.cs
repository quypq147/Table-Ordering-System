using Domain.Abstractions;

namespace Domain.Events;

public sealed record OrderInProgress(Guid OrderId) : IDomainEvent;
