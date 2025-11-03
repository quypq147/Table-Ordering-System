using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly TableOrderingDbContext _db;
    public CategoryRepository(TableOrderingDbContext db) => _db = db;

    public Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task AddAsync(Category cat, CancellationToken ct = default) =>
        _db.Categories.AddAsync(cat, ct).AsTask();

    public void Update(Category cat) => _db.Categories.Update(cat);
}



