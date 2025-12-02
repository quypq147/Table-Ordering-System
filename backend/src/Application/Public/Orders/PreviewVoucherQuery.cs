using Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Application.Public.Orders;

public sealed record PreviewVoucherQuery(Guid OrderId, string Code) : IQuery<VoucherPreviewDto>;

public sealed record VoucherPreviewDto(
 decimal SubTotal,
 decimal Discount,
 decimal Total,
 string Currency
);

public sealed class PreviewVoucherHandler : IQueryHandler<PreviewVoucherQuery, VoucherPreviewDto>
{
    private readonly IApplicationDbContext _db;
    public PreviewVoucherHandler(IApplicationDbContext db) => _db = db;

    public async Task<VoucherPreviewDto> Handle(PreviewVoucherQuery q, CancellationToken ct)
    {
        var order = await _db.Orders.Include(o => o.Items)
        .FirstOrDefaultAsync(o => o.Id == q.OrderId, ct)
        ?? throw new KeyNotFoundException("Không tìm th?y ??n.");

        var sub = order.CalculateSubTotal();
        var currency = sub.Currency ?? "VND";
        var subAmount = sub.Amount;

        decimal discount = 0m;
        var code = (q.Code ?? string.Empty).Trim().ToUpperInvariant();
        if (code == "GIAM10")
        {
            discount = Math.Min(Math.Round(subAmount * 0.10m, 0), 50000m);
        }
        else if (code == "GIAM20K")
        {
            discount = 20000m;
        }

        var total = Math.Max(0m, subAmount - discount);
        return new VoucherPreviewDto(subAmount, discount, total, currency);
    }
}
