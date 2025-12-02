using Application.Abstractions;
using Application.Dtos;

namespace Application.Orders.Commands
{
    public sealed record MarkReadyCommand(Guid OrderId) : ICommand<OrderDto>;
}
