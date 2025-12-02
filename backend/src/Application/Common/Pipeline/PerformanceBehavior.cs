using Microsoft.Extensions.Logging;

namespace Application.Common.Pipeline;

public sealed class PerformanceBehavior(ILogger<PerformanceBehavior> logger) : IRequestBehavior
{
    public Task OnExecutingAsync(object request, CancellationToken ct = default) => Task.CompletedTask;

    public Task OnExecutedAsync(object request, object? response, Exception? exception, TimeSpan elapsed, CancellationToken ct = default)
    {
        if (elapsed.TotalMilliseconds > 500)
        {
            logger.LogWarning("Slow request {RequestType} took {Elapsed} ms", request.GetType().Name, elapsed.TotalMilliseconds);
        }
        return Task.CompletedTask;
    }
}
