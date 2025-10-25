using Application.Abstractions;
using Application.Common.CQRS;
using Microsoft.Extensions.DependencyInjection;

public class Sender : ISender
{
    private readonly IServiceProvider _sp;
    public Sender(IServiceProvider sp) => _sp = sp;

    public Task<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken ct = default)
    {
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResponse));
        dynamic handler = _sp.GetRequiredService(handlerType);
        return handler.Handle((dynamic)command, ct);
    }

    public Task<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken ct = default)
    {
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResponse));
        dynamic handler = _sp.GetRequiredService(handlerType);
        return handler.Handle((dynamic)query, ct);
    }
}
