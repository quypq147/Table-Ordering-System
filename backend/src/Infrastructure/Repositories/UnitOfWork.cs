using Domain.Repositories;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly TableOrderingDbContext _db;
    public UnitOfWork(TableOrderingDbContext db) => _db = db;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _db.SaveChangesAsync(cancellationToken);
}
