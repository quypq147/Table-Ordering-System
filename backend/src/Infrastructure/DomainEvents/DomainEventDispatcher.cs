using Application.Abstractions;
using Domain.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DomainEvents;

public sealed class DomainEventDispatcher(IServiceProvider serviceProvider) : IDomainEventDispatcher
{
    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken ct = default)
    {
        // Fire each domain event sequentially (could be parallel but preserves ordering)
        using var scope = serviceProvider.CreateScope();
        foreach (var domainEvent in domainEvents)
        {
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
            var handlers = scope.ServiceProvider.GetServices(handlerType);
            foreach (var handler in handlers)
            {
                var method = handlerType.GetMethod("HandleAsync")!;
                var task = (Task)method.Invoke(handler, new object?[] { domainEvent, ct })!;
                await task.ConfigureAwait(false);
            }
        }
    }
}
