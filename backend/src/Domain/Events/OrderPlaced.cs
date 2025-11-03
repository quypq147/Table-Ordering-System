using Domain.Abstractions;

namespace Domain.Events;

public record OrderPlaced(Guid OrderId) : IDomainEvent;
