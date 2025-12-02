using Application.Abstractions;
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
        var codeNorm = c.TableCode.Trim().ToUpperInvariant();

        var table = await _db.Tables
            .FirstOrDefaultAsync(t => t.Code.ToUpper() == codeNorm, ct);
        if (table is null)
        {
            throw new InvalidOperationException("Bàn không tồn tại.");
        }

        // Tìm draft hiện có cho bàn này
        var open = await _db.Orders
            .Where(o => o.TableId == table.Id && o.OrderStatus == Domain.Enums.OrderStatus.Draft)
            .FirstOrDefaultAsync(ct);
        if (open is not null)
        {
            table.MarkInUse();
            await _db.SaveChangesAsync(ct);
            return open.Id;
        }

        // NEW: sinh mã ngắn, thân thiện
        var orderCode = await _codeGen.GenerateAsync(table.Id, table.Code, ct);

        var id = Guid.NewGuid();
        var order = Domain.Entities.Order.Start(id, table.Id, orderCode);

        _db.Orders.Add(order);
        table.MarkInUse();

        await _db.SaveChangesAsync(ct);
        return id;
    }
}