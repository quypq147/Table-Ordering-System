using Application.Abstractions;
using Application.Dtos;

namespace Application.Orders.Queries;
public sealed record GetOrderByIdQuery(string OrderId) : IQuery<OrderDto?>;
