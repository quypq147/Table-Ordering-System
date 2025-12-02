using Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Application.Tables.Commands;

public sealed class DeleteTableHandler : ICommandHandler<DeleteTableCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    public DeleteTableHandler(IApplicationDbContext db) => _db = db;
    public async Task<Unit> Handle(DeleteTableCommand c, CancellationToken ct)
    {
        var table = await _db.Tables.FirstOrDefaultAsync(t => t.Id == c.Id, ct)
        ?? throw new KeyNotFoundException("Table not found");
        // Không cho xóa n?u còn order ch?a thanh toán/ch?a h?y
        var hasActiveOrder = await _db.Orders.AnyAsync(o => o.TableId == c.Id && o.OrderStatus != Domain.Enums.OrderStatus.Paid && o.OrderStatus != Domain.Enums.OrderStatus.Cancelled, ct);
        if (hasActiveOrder) throw new InvalidOperationException("Không th? xóa bàn ?ang có ??n ho?t ??ng.");
        _db.Tables.Remove(table);
        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}