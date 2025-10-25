using System.Threading;
using System.Threading.Tasks;

namespace Application.Abstractions;
public interface ICommandHandler<TCommand, TResponse> where TCommand : ICommand<TResponse>
{
    Task<TResponse> Handle(TCommand command, CancellationToken ct);
}