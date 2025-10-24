using Application.Abstractions;
using Application.Dtos;

namespace Application.Orders.Queries;

public sealed record ListOrdersQuery(int Page = 1, int PageSize = 20)
    : IQuery<IReadOnlyList<OrderDto>>;
