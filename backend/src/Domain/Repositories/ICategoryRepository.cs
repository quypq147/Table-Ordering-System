using Domain.Entities;

namespace Domain.Repositories
{
    public interface ICategoryRepository
    {
        Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task AddAsync(Category cat, CancellationToken ct = default);
        void Update(Category cat);
    }
}

