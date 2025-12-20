using Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Public.Cart;

public sealed record StartCartByTableCodeCommand(string TableCode, Guid? SessionId) : IRequest<CartDto>; // return Cart DTO

public sealed class StartCartByTableCodeHandler : IRequestHandler<StartCartByTableCodeCommand, CartDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IOrderCodeGenerator _codeGen;

    public StartCartByTableCodeHandler(IApplicationDbContext db, IOrderCodeGenerator codeGen)
    {
        _db = db;
        _codeGen = codeGen;
    }

    public async Task<CartDto> Handle(StartCartByTableCodeCommand c, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(c.TableCode))
            throw new ArgumentNullException(nameof(c.TableCode));
        var codeNorm = c.TableCode.Trim().ToUpperInvariant();

        var table = await _db.Tables.FirstOrDefaultAsync(t => t.Code.ToUpper() == codeNorm, ct)
            ?? throw new InvalidOperationException("Bàn không tồn tại.");

        // Validate session id if present
        if (table.CurrentSessionId is Guid current && c.SessionId is Guid incoming && current != incoming)
        {
            throw new InvalidOperationException("Invalid or Expired QR Code");
        }

        // If table occupied/in use -> try join existing active order (Submitted/InProgress)
        var isOccupied = table.Status.ToString().Equals("Occupied", StringComparison.OrdinalIgnoreCase)
                          || table.Status.ToString().Equals("InUse", StringComparison.OrdinalIgnoreCase);
        if (isOccupied)
        {
            var active = await _db.Orders
                .Include(o => o.Items)
                .Where(o => o.TableId == table.Id &&
                            (o.OrderStatus == Domain.Enums.OrderStatus.InProgress ||
                             o.OrderStatus == Domain.Enums.OrderStatus.Submitted))
                .OrderByDescending(o => o.CreatedAtUtc)
                .FirstOrDefaultAsync(ct);
            if (active is not null)
            {
                // Build CartDto from existing active order
                var items = active.Items.Select(i => new CartItemDto(
                    Id: i.Id,
                    MenuItemId: i.MenuItemId,
                    Name: i.NameSnapshot,
                    UnitPrice: i.UnitPrice.Amount,
                    Quantity: i.Quantity.Value,
                    Note: i.Note,
                    LineTotal: i.UnitPrice.Amount * i.Quantity.Value
                )).ToList();

                var subtotal = items.Sum(x => x.LineTotal);
                var service = 0m;
                var tax = 0m;
                var total = subtotal + service + tax;
                var status = active.OrderStatus.ToString();
                var tableCode = table.Code;

                return new CartDto(
                    OrderId: active.Id,
                    OrderCode: active.Code,
                    TableCode: tableCode,
                    Status: status,
                    Items: items,
                    Subtotal: subtotal,
                    ServiceCharge: service,
                    Tax: tax,
                    Total: total
                );
            }
        }

        // Otherwise: join existing draft if any
        var open = await _db.Orders
            .Where(o => o.TableId == table.Id && o.OrderStatus == Domain.Enums.OrderStatus.Draft)
            .Include(o => o.Items)
            .FirstOrDefaultAsync(ct);
        if (open is not null)
        {
            var items = open.Items.Select(i => new CartItemDto(
                Id: i.Id,
                MenuItemId: i.MenuItemId,
                Name: i.NameSnapshot,
                UnitPrice: i.UnitPrice.Amount,
                Quantity: i.Quantity.Value,
                Note: i.Note,
                LineTotal: i.UnitPrice.Amount * i.Quantity.Value
            )).ToList();

            var subtotal = items.Sum(x => x.LineTotal);
            var service = 0m;
            var tax = 0m;
            var total = subtotal + service + tax;
            var status = open.OrderStatus.ToString();

            return new CartDto(
                OrderId: open.Id,
                OrderCode: open.Code,
                TableCode: table.Code,
                Status: status,
                Items: items,
                Subtotal: subtotal,
                ServiceCharge: service,
                Tax: tax,
                Total: total
            );
        }

        // NEW: sinh mã ngắn, thân thiện
        var orderCode = await _codeGen.GenerateAsync(table.Id, table.Code, ct);

        var id = Guid.NewGuid();
        var order = Domain.Entities.Order.Start(id, table.Id, orderCode);

        _db.Orders.Add(order);
        table.MarkInUse();

        await _db.SaveChangesAsync(ct);

        // Return new empty cart dto
        return new CartDto(
            OrderId: order.Id,
            OrderCode: order.Code,
            TableCode: table.Code,
            Status: order.OrderStatus.ToString(),
            Items: new List<CartItemDto>(),
            Subtotal: 0m,
            ServiceCharge: 0m,
            Tax: 0m,
            Total: 0m
        );
    }
}