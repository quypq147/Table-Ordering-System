using Domain.Abstractions;

namespace Domain.Events;

public record OrderPlaced(string OrderId) : IDomainEvent;
