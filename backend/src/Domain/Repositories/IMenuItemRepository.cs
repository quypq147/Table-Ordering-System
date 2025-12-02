using Domain.Entities;

namespace Domain.Repositories;

public interface IMenuItemRepository
{
    Task<MenuItem?> GetByIdAsync(Guid id);
}
