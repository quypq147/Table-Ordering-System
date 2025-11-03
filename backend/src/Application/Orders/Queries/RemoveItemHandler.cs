// Application/Orders/Commands/RemoveItemHandler.cs
using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;        // IApplicationDbContext
using Microsoft.EntityFrameworkCore;
using Application.Orders.Commands;

namespace Application.Orders.Queries;

public sealed class RemoveItemHandler : ICommandHandler<RemoveItemCommand, OrderDto>
{
    private readonly IApplicationDbContext _db;
    public RemoveItemHandler(IApplicationDbContext db) => _db = db;

    public async Task<OrderDto> Handle(RemoveItemCommand cmd, CancellationToken ct)
    {
        var order = await _db.Orders.Include(o => o.Items)
                                    .FirstOrDefaultAsync(o => o.Id == cmd.OrderId, ct)
                    ?? throw new KeyNotFoundException("Không tìm thấy đơn");

        // Find the OrderItem's int Id by MenuItemId
        var orderItem = order.Items.FirstOrDefault(i => i.MenuItemId == cmd.MenuItemId)
            ?? throw new KeyNotFoundException("Không tìm thấy món trong đơn");

        order.RemoveItem(orderItem.Id); // Pass int OrderItem.Id instead of Guid MenuItemId
        await _db.SaveChangesAsync(ct);
        return OrderMapper.ToDto(order);
    }
}

