using Domain.Abstractions;

namespace Domain.Events;

public sealed record OrderCancelled(Guid OrderId) : IDomainEvent;