using Application.Abstractions;

namespace Application.Public.Cart;

public sealed record GetCartByIdQuery(Guid OrderId) : IQuery<CartDto?>;
