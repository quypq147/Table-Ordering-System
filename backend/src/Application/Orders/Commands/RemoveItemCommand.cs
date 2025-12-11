// Application/Orders/Commands/RemoveItemCommand.cs
using Application.Dtos;
using MediatR;

namespace Application.Orders.Commands;

public sealed record RemoveItemCommand(Guid OrderId, int OrderItemId) : IRequest<OrderDto>;

