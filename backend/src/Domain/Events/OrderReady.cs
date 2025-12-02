using Domain.Abstractions;

namespace Domain.Events;

public sealed record OrderReady(Guid OrderId) : IDomainEvent;
