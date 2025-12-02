using Application.Abstractions;
using Application.Dtos;

namespace Application.Orders.Commands
{
    public sealed record MarkServedCommand(Guid OrderId) : ICommand<OrderDto>;
}
