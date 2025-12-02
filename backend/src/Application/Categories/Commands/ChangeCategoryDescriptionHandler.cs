using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Application.Categories.Commands;

public sealed class ChangeCategoryDescriptionHandler
    : ICommandHandler<ChangeCategoryDescriptionCommand, CategoryDto>
{
    private readonly IApplicationDbContext _db;
    public ChangeCategoryDescriptionHandler(IApplicationDbContext db) => _db = db;

    public async Task<CategoryDto> Handle(ChangeCategoryDescriptionCommand c, CancellationToken ct)
    {
        var cat = await _db.Categories.FirstOrDefaultAsync(x => x.Id == c.Id, ct)
                  ?? throw new KeyNotFoundException("Category not found");
        cat.ChangeDescription(c.Description);
        await _db.SaveChangesAsync(ct);
        return CategoryMapper.ToDto(cat);
    }
}