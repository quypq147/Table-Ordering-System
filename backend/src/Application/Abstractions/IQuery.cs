using MediatR;

namespace Application.Abstractions;

public interface IQuery<TResponse> : IRequest<TResponse>
{
}