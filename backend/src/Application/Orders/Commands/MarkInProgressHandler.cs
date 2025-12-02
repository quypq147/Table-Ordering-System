// Application/Orders/Commands/MarkInProgressHandler.cs
using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders.Commands;

public sealed class MarkInProgressHandler : ICommandHandler<MarkInProgressCommand, OrderDto>
{
    private readonly IApplicationDbContext _db;
    public MarkInProgressHandler(IApplicationDbContext db) => _db = db;

    public async Task<OrderDto> Handle(MarkInProgressCommand cmd, CancellationToken ct)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == cmd.OrderId, ct)
                    ?? throw new KeyNotFoundException("Không tìm thấy đơn");
        order.MarkInProgress(); // Submitted -> InProgress :contentReference[oaicite:8]{index=8}
        await _db.SaveChangesAsync(ct);
        return OrderMapper.ToDto(order);
    }
}

