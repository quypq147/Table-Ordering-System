using Domain.Abstractions;

namespace Domain.Events;

public record OrderPaid(string OrderId, decimal Amount, string Currency) : IDomainEvent;
