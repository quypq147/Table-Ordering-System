using Application.Abstractions;

namespace Application.Common.CQRS
{
    public interface ISender
    {
        Task<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken ct = default);
        Task<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken ct = default);
    }
}
