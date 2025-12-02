using Application.Abstractions;
using Application.Dtos;

namespace Application.Orders.Commands
{
    public sealed record CancelOrderCommand(Guid OrderId) : ICommand<OrderDto>;
}
