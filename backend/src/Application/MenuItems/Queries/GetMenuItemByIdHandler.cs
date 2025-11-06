using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions;
using Application.Dtos;
using Microsoft.EntityFrameworkCore;
using Application.Mappings;

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

        return m is null ? null : MenuItemMapper.ToDto(m);
    }
}

