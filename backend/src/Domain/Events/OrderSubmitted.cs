using Domain.Abstractions;

namespace Domain.Events;

public record OrderSubmitted(Guid OrderId) : IDomainEvent;
