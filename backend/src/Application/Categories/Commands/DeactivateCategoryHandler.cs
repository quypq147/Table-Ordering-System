using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Application.Categories.Commands;

public sealed class DeactivateCategoryHandler
    : ICommandHandler<DeactivateCategoryCommand, CategoryDto>
{
    private readonly IApplicationDbContext _db;
    public DeactivateCategoryHandler(IApplicationDbContext db) => _db = db;

    public async Task<CategoryDto> Handle(DeactivateCategoryCommand c, CancellationToken ct)
    {
        var cat = await _db.Categories.FirstOrDefaultAsync(x => x.Id == c.Id, ct)
                  ?? throw new KeyNotFoundException("Khong tim thay danh muc");
        cat.Deactivate();
        await _db.SaveChangesAsync(ct);
        return CategoryMapper.ToDto(cat);
    }
}
