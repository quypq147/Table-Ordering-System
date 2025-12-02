using Application.Abstractions;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Invoices.Commands;

public sealed class GenerateInvoiceForOrderHandler : ICommandHandler<GenerateInvoiceForOrderCommand, bool>
{
    private readonly IApplicationDbContext _db;

    public GenerateInvoiceForOrderHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<bool> Handle(GenerateInvoiceForOrderCommand request, CancellationToken ct)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, ct);

        if (order is null) return false;

        // simple invoice number
        var number = $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpperInvariant()}";

        var invoice = Invoice.CreateFromOrder(order, number);

        _db.Invoices.Add(invoice);

        await _db.SaveChangesAsync(ct);

        return true;
    }
}
