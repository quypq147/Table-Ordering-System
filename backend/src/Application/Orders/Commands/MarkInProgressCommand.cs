// Application/Orders/Commands/MarkInProgressCommand.cs
using Application.Abstractions;
using Application.Dtos;
namespace Application.Orders.Commands;

public sealed record MarkInProgressCommand(Guid OrderId) : ICommand<OrderDto>;

