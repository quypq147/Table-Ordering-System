using Domain.Abstractions;

namespace Domain.Events;

public sealed record OrderServed(Guid OrderId) : IDomainEvent;
