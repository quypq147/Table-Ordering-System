using Application.Abstractions;
using Application.Common.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace Application.Common.CQRS;

public class Sender : ISender
{
    private readonly IServiceProvider _sp;
    private readonly IEnumerable<IRequestBehavior> _behaviors;
    public Sender(IServiceProvider sp, IEnumerable<IRequestBehavior> behaviors)
    {
        _sp = sp;
        _behaviors = behaviors;
    }

    public async Task<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken ct = default)
    {
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResponse));
        dynamic handler = _sp.GetRequiredService(handlerType);
        return await ExecuteWithBehaviors(command!, () => (Task<TResponse>)handler.Handle((dynamic)command, ct), ct);
    }

    public async Task<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken ct = default)
    {
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResponse));
        dynamic handler = _sp.GetRequiredService(handlerType);
        return await ExecuteWithBehaviors(query!, () => (Task<TResponse>)handler.Handle((dynamic)query, ct), ct);
    }

    private async Task<TResponse> ExecuteWithBehaviors<TResponse>(object request, Func<Task<TResponse>> action, CancellationToken ct)
    {
        foreach (var b in _behaviors)
        {
            await b.OnExecutingAsync(request, ct);
        }
        var sw = Stopwatch.StartNew();
        try
        {
            var response = await action();
            sw.Stop();
            foreach (var b in _behaviors)
            {
                await b.OnExecutedAsync(request, response!, null, sw.Elapsed, ct);
            }
            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();
            foreach (var b in _behaviors)
            {
                await b.OnExecutedAsync(request, default!, ex, sw.Elapsed, ct);
            }
            throw;
        }
    }
}
