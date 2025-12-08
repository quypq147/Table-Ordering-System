using Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Application.Categories.Commands;

public sealed class DeleteCategoryHandler : ICommandHandler<DeleteCategoryCommand, bool>
{
    private readonly IApplicationDbContext _db;
    public DeleteCategoryHandler(IApplicationDbContext db) => _db = db;

    public async Task<bool> Handle(DeleteCategoryCommand c, CancellationToken ct)
    {
        var cat = await _db.Categories.FirstOrDefaultAsync(x => x.Id == c.Id, ct);
        if (cat is null) return false;
        _db.Categories.Remove(cat);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
