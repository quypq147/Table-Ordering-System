using System.Threading;
using System.Threading.Tasks;

namespace Application.Abstractions;
public interface IQueryHandler<TQuery, TResponse> where TQuery : IQuery<TResponse>
{
    Task<TResponse> Handle(TQuery query, CancellationToken ct);
}