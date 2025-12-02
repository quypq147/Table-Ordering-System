using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Application.Categories.Commands;

public sealed class RenameCategoryHandler
    : ICommandHandler<RenameCategoryCommand, CategoryDto>
{
    private readonly IApplicationDbContext _db;
    public RenameCategoryHandler(IApplicationDbContext db) => _db = db;

    public async Task<CategoryDto> Handle(RenameCategoryCommand c, CancellationToken ct)
    {
        var cat = await _db.Categories.FirstOrDefaultAsync(x => x.Id == c.Id, ct)
                  ?? throw new KeyNotFoundException("Khong tim thay danh muc");
        cat.Rename(c.NewName);            // domain validate not empty
        await _db.SaveChangesAsync(ct);
        return CategoryMapper.ToDto(cat);
    }
}
