using Application.Abstractions;
using Application.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Application.Categories.Queries;

public sealed record ListCategoriesQuery(string? Search, bool? OnlyActive, int Page, int PageSize)
    : IQuery<IReadOnlyList<CategoryDto>>;

public sealed class ListCategoriesHandler
    : IQueryHandler<ListCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    private readonly IApplicationDbContext _db;
    public ListCategoriesHandler(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<CategoryDto>> Handle(ListCategoriesQuery q, CancellationToken ct)
    {
        var query = _db.Categories.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var s = q.Search.Trim();
            query = query.Where(x => x.Name.Contains(s) || x.Code.Contains(s));
        }

        if (q.OnlyActive == true)
            query = query.Where(x => x.IsActive);

        // Sắp xếp ổn định
        query = query.OrderBy(x => x.SortOrder).ThenBy(x => x.Name);

        // QUAN TRỌNG: Project đủ Id
        return await query
            .Select(x => new CategoryDto(
                x.Id,               // <-- phải map Id
                x.Name,
                x.Description,
                x.IsActive,
                x.SortOrder
            ))
            .ToListAsync(ct);
    }
}


