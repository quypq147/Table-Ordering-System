using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions;
using Application.Dtos;
using Microsoft.EntityFrameworkCore;

// (Option nếu bạn đã có mapper chung):
// using Application.Mappings;              // MenuItemMapper

namespace Application.MenuItems.Queries;

public sealed class GetMenuItemByIdHandler
    : IQueryHandler<GetMenuItemByIdQuery, MenuItemDto?>
{
    private readonly IApplicationDbContext _db;
    public GetMenuItemByIdHandler(IApplicationDbContext db) => _db = db;

    public async Task<MenuItemDto?> Handle(GetMenuItemByIdQuery q, CancellationToken ct)
    {
        var m = await _db.MenuItems
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == q.Id, ct);

        if (m is null) return null;

        // Nếu bạn có mapper dùng tiền tệ/VO sẵn:
        // return MenuItemMapper.ToDto(m);

        // Fallback map thẳng (điều chỉnh fields theo MenuItemDto của bạn):
        return new MenuItemDto(
            Id: m.Id,
            Name: m.Name,
            Price: m.Price.Amount,     // giả sử Price là ValueObject Money(Amount, Currency)
            Currency: m.Price.Currency,
            IsActive: m.IsActive
        );
    }
}

