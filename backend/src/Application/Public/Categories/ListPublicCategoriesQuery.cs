using Application.Abstractions;
using Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Public.Categories;

public sealed record ListPublicCategoriesQuery() : IRequest<IReadOnlyList<CategoryDto>>;

public sealed class ListPublicCategoriesHandler
    : IRequestHandler<ListPublicCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    private readonly IApplicationDbContext _db;
    public ListPublicCategoriesHandler(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<CategoryDto>> Handle(ListPublicCategoriesQuery q, CancellationToken ct)
    {
        var list = await _db.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Description, c.IsActive, c.SortOrder))
            .ToListAsync(ct);

        return list;
    }
}