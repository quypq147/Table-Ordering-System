namespace Application.Common.Pipeline;

public interface IRequestBehavior
{
    Task OnExecutingAsync(object request, CancellationToken ct = default);
    Task OnExecutedAsync(object request, object? response, Exception? exception, TimeSpan elapsed, CancellationToken ct = default);
}
