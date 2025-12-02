using Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Application.Invoices.Queries;

public sealed class GetInvoiceByOrderIdHandler : IQueryHandler<GetInvoiceByOrderIdQuery, InvoiceDto?>
{
    private readonly IApplicationDbContext _db;

    public GetInvoiceByOrderIdHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<InvoiceDto?> Handle(GetInvoiceByOrderIdQuery request, CancellationToken ct)
    {
        var inv = await _db.Invoices
            .Where(i => i.OrderId == request.OrderId)
            .Select(i => new InvoiceDto
            {
                Id = i.Id,
                InvoiceNumber = i.InvoiceNumber,
                IssuedAtUtc = i.IssuedAtUtc,
                Total = i.Total
            })
            .FirstOrDefaultAsync(ct);

        return inv;
    }
}
