using Application.Abstractions;
using Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Public.Cart;

public sealed record AddCartItemByMenuItemIdCommand(Guid OrderId, Guid MenuItemId, int Quantity, string? Note) : IRequest;

public sealed class AddCartItemByMenuItemIdHandler
    : IRequestHandler<AddCartItemByMenuItemIdCommand>
{
    private readonly IApplicationDbContext _db;
    public AddCartItemByMenuItemIdHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(AddCartItemByMenuItemIdCommand c, CancellationToken ct)
    {
        var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == c.OrderId, ct)
            ?? throw new KeyNotFoundException("Không tìm thấy giỏ hàng.");

        var menu = await _db.MenuItems.FirstOrDefaultAsync(m => m.Id == c.MenuItemId && m.IsActive, ct)
            ?? throw new KeyNotFoundException("Món không tồn tại hoặc đã ngưng bán.");

        var qty = new Quantity(Math.Max(1, c.Quantity));

        // Thêm dòng theo Domain hiện tại (chưa nhận Note)
        order.AddItem(menu.Id, menu.Name, menu.Price, qty);

        // Gán Note nếu có (dòng mới nhất hoặc dòng vừa gộp)
        var line = order.Items.OrderByDescending(i => i.Id).FirstOrDefault();
        if (line is not null && !string.IsNullOrWhiteSpace(c.Note))
        {
            line.ChangeNote(c.Note);
        }

        await _db.SaveChangesAsync(ct);
    }
}