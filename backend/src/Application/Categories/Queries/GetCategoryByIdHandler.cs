using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Application.Categories.Queries;

public sealed class GetCategoryByIdHandler
    : IQueryHandler<GetCategoryByIdQuery, CategoryDto?>
{
    private readonly IApplicationDbContext _db;
    public GetCategoryByIdHandler(IApplicationDbContext db) => _db = db;

    public async Task<CategoryDto?> Handle(GetCategoryByIdQuery q, CancellationToken ct)
    {
        var cat = await _db.Categories.FirstOrDefaultAsync(x => x.Id == q.Id, ct);
        return cat is null ? null : CategoryMapper.ToDto(cat);
    }
}

