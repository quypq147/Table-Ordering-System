using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(string id);
    Task AddAsync(Order order);
    void Update(Order order);
}
