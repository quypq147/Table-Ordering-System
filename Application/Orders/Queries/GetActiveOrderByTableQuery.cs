using Application.Abstractions;
using Application.Dtos;
public sealed record GetActiveOrderByTableQuery(string TableId) : IQuery<OrderDto?>;
