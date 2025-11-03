// Application/Orders/Commands/RemoveItemCommand.cs
using Application.Abstractions;
using Application.Dtos;

namespace Application.Orders.Commands;

public sealed record RemoveItemCommand(Guid OrderId, Guid MenuItemId) : ICommand<OrderDto>;

