using Application.Abstractions;
using Application.Dtos;
public sealed record GetActiveOrderByTableQuery(Guid TableId) : IQuery<OrderDto?>;
