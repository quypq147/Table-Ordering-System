using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Repositories;

public interface ITableRepository
{
    Task<RestaurantTable?> GetByIdAsync(string id);
}
