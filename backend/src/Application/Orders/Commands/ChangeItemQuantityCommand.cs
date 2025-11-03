// Application/Orders/Commands/ChangeItemQuantityCommand.cs
using Application.Abstractions;
using Application.Dtos;

namespace Application.Orders.Commands;

public sealed record ChangeItemQuantityCommand(Guid OrderId, int OrderItemId, int NewQuantity)
    : ICommand<OrderDto>;

