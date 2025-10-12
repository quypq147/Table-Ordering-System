using Domain.Abstractions;

namespace Domain.Events;

public record OrderSubmitted(string OrderId) : IDomainEvent;
