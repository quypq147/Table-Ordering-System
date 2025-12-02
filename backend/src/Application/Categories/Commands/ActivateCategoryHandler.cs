using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Application.Categories.Commands;

public sealed class ActivateCategoryHandler
    : ICommandHandler<ActivateCategoryCommand, CategoryDto>
{
    private readonly IApplicationDbContext _db;
    public ActivateCategoryHandler(IApplicationDbContext db) => _db = db;

    public async Task<CategoryDto> Handle(ActivateCategoryCommand c, CancellationToken ct)
    {
        var cat = await _db.Categories.FirstOrDefaultAsync(x => x.Id == c.Id, ct)
                  ?? throw new KeyNotFoundException("Category not found");
        cat.Activate();
        await _db.SaveChangesAsync(ct);
        return CategoryMapper.ToDto(cat);
    }
}

