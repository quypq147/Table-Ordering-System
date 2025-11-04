using Application.Abstractions;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Public.Cart;

public sealed record StartCartByTableCodeCommand(string TableCode) : IRequest<Guid>; // return OrderId

public sealed class StartCartByTableCodeHandler : IRequestHandler<StartCartByTableCodeCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly IOrderCodeGenerator _codeGen;

    public StartCartByTableCodeHandler(IApplicationDbContext db, IOrderCodeGenerator codeGen)
    {
        _db = db;
        _codeGen = codeGen;
    }

    public async Task<Guid> Handle(StartCartByTableCodeCommand c, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(c.TableCode))
            throw new ArgumentNullException(nameof(c.TableCode));
        var code = c.TableCode.Trim();

        var table = await _db.Tables
            .Where(t => t.Code == code)
            .Select(t => new { t.Id, t.Code })
            .FirstOrDefaultAsync(ct);
        if (table is null) throw new InvalidOperationException("Bàn không tồn tại.");

        // Tìm draft hiện có
        var open = await _db.Orders
            .Where(o => o.TableId == table.Id && o.OrderStatus == Domain.Enums.OrderStatus.Draft)
            .FirstOrDefaultAsync(ct);
        if (open is not null) return open.Id;

        // NEW: sinh mã ngắn, thân thiện
        var orderCode = await _codeGen.GenerateAsync(table.Id, table.Code, ct);

        var id = Guid.NewGuid();
        var order = Domain.Entities.Order.Start(id, table.Id, orderCode); // 👈 dùng overload mới
        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);
        return id;
    }
}