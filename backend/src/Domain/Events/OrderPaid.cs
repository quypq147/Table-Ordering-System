using Domain.Abstractions;

namespace Domain.Events;

public record OrderPaid(Guid OrderId, decimal Amount, string Currency, string Method) : IDomainEvent;

