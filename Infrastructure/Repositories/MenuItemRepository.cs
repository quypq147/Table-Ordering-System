using Domain.Entities;
using Domain.Repositories;
using Domain.ValueObjects;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Infrastructure.Repositories;

public sealed class MenuItemRepository : IMenuItemRepository
{
    private readonly TableOrderingDbContext _db;
    public MenuItemRepository(TableOrderingDbContext db) => _db = db;

    public Task<MenuItem?> GetByIdAsync(string id)
        => _db.MenuItems.FirstOrDefaultAsync(m => m.Id == id);
}