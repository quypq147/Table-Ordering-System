using Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Application.Public.Cart;

public sealed class GetCartByIdHandler : IQueryHandler<GetCartByIdQuery, CartDto?>
{
    private readonly IApplicationDbContext _db;
    public GetCartByIdHandler(IApplicationDbContext db) => _db = db;

    public async Task<CartDto?> Handle(GetCartByIdQuery q, CancellationToken ct)
    {
        var order = await _db.Orders
        .Where(o => o.Id == q.OrderId)
        .Include(o => o.Items)
        .FirstOrDefaultAsync(ct);
        if (order is null) return null;

        // Load table code
        var tableCode = await _db.Tables
        .Where(t => t.Id == order.TableId)
        .Select(t => t.Code)
        .FirstOrDefaultAsync(ct) ?? string.Empty;

        var items = order.Items
        .Select(i => new CartItemDto(
        Id: i.Id,
        MenuItemId: i.MenuItemId,
        Name: i.NameSnapshot,
        UnitPrice: i.UnitPrice.Amount,
        Quantity: i.Quantity.Value,
        Note: i.Note,
        LineTotal: i.UnitPrice.Amount * i.Quantity.Value
        ))
        .ToList();

        var subtotal = items.Sum(x => x.LineTotal);
        var service = 0m; // placeholder for future config
        var tax = 0m; // placeholder
        var total = subtotal + service + tax;
        var status = order.OrderStatus.ToString();

        return new CartDto(
        OrderId: order.Id,
        TableCode: tableCode,
        Status: status,
        Items: items,
        Subtotal: subtotal,
        ServiceCharge: service,
        Tax: tax,
        Total: total,
        Code: order.Code
        );
    }
}
