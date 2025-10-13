﻿// Application/Orders/Commands/RemoveItemHandler.cs
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

        order.RemoveItem(cmd.MenuItemId); // chỉ hợp lệ khi Draft :contentReference[oaicite:4]{index=4}
        await _db.SaveChangesAsync(ct);
        return OrderMapper.ToDto(order);
    }
}

