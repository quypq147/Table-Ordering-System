using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Public.Menu;

public sealed record ListMenuByCategoryQuery(Guid CategoryId) : IRequest<IReadOnlyList<MenuItemDto>>;

public sealed class ListMenuByCategoryHandler
    : IRequestHandler<ListMenuByCategoryQuery, IReadOnlyList<MenuItemDto>>
{
    private readonly IApplicationDbContext _db;
    public ListMenuByCategoryHandler(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<MenuItemDto>> Handle(ListMenuByCategoryQuery q, CancellationToken ct)
    {
        var items = await _db.MenuItems
            .Where(x => x.IsActive && x.CategoryId == q.CategoryId)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

        return items.Select(MenuItemMapper.ToDto).ToList();
    }
}