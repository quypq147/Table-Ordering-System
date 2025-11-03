using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public sealed class MenuItemRepository : IMenuItemRepository
{
    private readonly TableOrderingDbContext _db;
    public MenuItemRepository(TableOrderingDbContext db) => _db = db;

    public Task<MenuItem?> GetByIdAsync(Guid id) =>
        _db.MenuItems.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
}
