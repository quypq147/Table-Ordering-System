using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class TableRepository : ITableRepository
{
    private readonly TableOrderingDbContext _db;
    public TableRepository(TableOrderingDbContext db) => _db = db;

    public Task<Table?> GetByIdAsync(Guid id) =>
        _db.Set<Table>().AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);

    public Task<Table?> GetByIdAsync(string id) =>
        _db.Set<Table>().AsNoTracking().FirstOrDefaultAsync(t => t.Id.ToString() == id);
}
