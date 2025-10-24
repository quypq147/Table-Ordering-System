using Application.Abstractions;
using Application.Categories.Commands;
using Application.Common;
using Application.Dtos;
using Application.Mappings;
using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

public sealed class CreateCategoryHandler
    : ICommandHandler<CreateCategoryCommand, CategoryDto>
{
    private readonly IApplicationDbContext _db;
    public CreateCategoryHandler(IApplicationDbContext db) => _db = db;

    public async Task<CategoryDto> Handle(CreateCategoryCommand c, CancellationToken ct)
    {
        // Id duy nhất theo business của bạn
        if (await _db.Categories.AnyAsync(x => x.Id == c.Id, ct))
            throw new InvalidOperationException("Category already exists.");

        var cat = new Category(c.Id, c.Name, c.Description, c.SortOrder);
        _db.Categories.Add(cat);
        await _db.SaveChangesAsync(ct);
        return CategoryMapper.ToDto(cat);
    }
}
