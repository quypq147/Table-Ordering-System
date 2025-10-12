using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Infrastructure.Repositories;

public sealed class TableRepository : ITableRepository
{
    private readonly TableOrderingDbContext _db;
    public TableRepository(TableOrderingDbContext db) => _db = db;

    public Task<RestaurantTable?> GetByIdAsync(string id)
        => _db.Tables.FirstOrDefaultAsync(t => t.Id == id);
}