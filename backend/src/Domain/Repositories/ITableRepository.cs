using Domain.Entities;

namespace Domain.Repositories;

public interface ITableRepository
{
    Task<Table?> GetByIdAsync(Guid id);
}
