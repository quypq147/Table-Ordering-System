using System.Linq;
using Application.Abstractions;
using Application.Common;
using Application.Dtos;
using Application.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Application.Categories.Queries;

public sealed class ListCategoriesHandler
    : IQueryHandler<ListCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    private readonly IApplicationDbContext _db;
    public ListCategoriesHandler(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<CategoryDto>> Handle(ListCategoriesQuery q, CancellationToken ct)
    {
        var query = _db.Categories.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(q.Search))
            query = query.Where(x => x.Name.Contains(q.Search!));

        if (q.OnlyActive is true) query = query.Where(x => x.IsActive);
        if (q.OnlyActive is false) query = query.Where(x => !x.IsActive);

        var skip = (q.Page - 1) * q.PageSize;

        var rows = await query
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
            .Skip(skip).Take(q.PageSize)
            .ToListAsync(ct);

        return rows.Select(CategoryMapper.ToDto).ToList();
    }
}

