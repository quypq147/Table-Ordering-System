using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Repositories;

public interface IMenuItemRepository
{
    Task<MenuItem?> GetByIdAsync(string id);
}
