using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Application.MenuItems.Queries
{
    public sealed class ListMenuItemsHandler
    : IQueryHandler<ListMenuItemsQuery, IReadOnlyList<MenuItemDto>>
    {
        private readonly IApplicationDbContext _db;
        public ListMenuItemsHandler(IApplicationDbContext db) => _db = db;

        public async Task<IReadOnlyList<MenuItemDto>> Handle(ListMenuItemsQuery q, CancellationToken ct)
        {
            var skip = (q.Page - 1) * q.PageSize;
            var query = _db.MenuItems.AsQueryable();
            if (q.OnlyActive) query = query.Where(x => x.IsActive); // :contentReference[oaicite:21]{index=21}
            if (q.CategoryId is Guid cid) query = query.Where(x => x.CategoryId == cid);
            if (!string.IsNullOrWhiteSpace(q.Search))
                query = query.Where(x => x.Name.Contains(q.Search!));

            var list = await query.OrderBy(x => x.Name).Skip(skip).Take(q.PageSize).ToListAsync(ct);
            return list.Select(MenuItemMapper.ToDto).ToList();
        }
    }

}
