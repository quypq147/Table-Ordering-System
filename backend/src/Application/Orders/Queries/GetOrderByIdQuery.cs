using Application.Abstractions;
using Application.Dtos;

namespace Application.Orders.Queries;

public sealed record GetOrderByIdQuery(Guid OrderId) : IQuery<OrderDto?>;
