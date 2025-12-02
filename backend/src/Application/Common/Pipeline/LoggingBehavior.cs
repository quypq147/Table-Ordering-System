using Microsoft.Extensions.Logging;

namespace Application.Common.Pipeline;

public sealed class LoggingBehavior(ILogger<LoggingBehavior> logger) : IRequestBehavior
{
    public Task OnExecutingAsync(object request, CancellationToken ct = default)
    {
        logger.LogInformation("Handling {RequestType}", request.GetType().Name);
        return Task.CompletedTask;
    }

    public Task OnExecutedAsync(object request, object? response, Exception? exception, TimeSpan elapsed, CancellationToken ct = default)
    {
        if (exception is null)
            logger.LogInformation("Handled {RequestType} in {Elapsed} ms", request.GetType().Name, elapsed.TotalMilliseconds);
        else
            logger.LogError(exception, "Error handling {RequestType} after {Elapsed} ms", request.GetType().Name, elapsed.TotalMilliseconds);
        return Task.CompletedTask;
    }
}
