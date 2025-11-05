using TableOrdering.Contracts;
using AdminWeb.Services.Models;
using AdminWeb.Dtos;

namespace AdminWeb.Services
{
    public interface IBackendApiClient
    {
        // AUTH
        Task<(string token, string displayName, string[] roles)> LoginAsync(string userOrEmail, string password);

        // USERS
        Task<List<UserVm>> ListUsersAsync();
        Task<UserDetailVm> GetUserAsync(Guid id);
        Task<Guid> CreateUserAsync(CreateUserVm vm);
        Task UpdateUserAsync(Guid id, UpdateUserVm vm);
        Task ChangePasswordAsync(Guid id, string newPassword);
        Task SetRolesAsync(Guid id, List<string> roles, CancellationToken ct = default);
        Task DeactivateUserAsync(Guid id);

        // MENU
        Task<List<MenuItemDto>> GetMenuAsync(CancellationToken ct = default);
        Task<List<MenuItemDto>> GetMenuAsync(string? search, Guid? categoryId, bool? onlyActive, CancellationToken ct = default);
        Task<HttpResponseMessage> CreateMenuItemAsync(CreateMenuItemRequest req, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> ActivateMenuItemAsync(Guid id, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> DeactivateMenuItemAsync(Guid id, CancellationToken cancellationToken = default);

        // ORDERS
        Task<Paginated<OrderSummaryDto>> GetOrdersAsync(int page =1, int pageSize =20, CancellationToken cancellationToken = default);
        Task<OrderDetailDto?> GetOrderAsync(Guid id, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> UpdateOrderStatusAsync(Guid id, string status, CancellationToken cancellationToken = default);

        // KDS
        Task<List<KitchenTicketDto>> GetTicketsAsync(Guid stationId, CancellationToken cancellationToken = default);

        // TABLES
        Task<List<DiningTableDto>> GetTablesAsync(CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> CreateTableAsync(CreateTableRequest req, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> UpdateTableAsync(Guid id, UpdateTableRequest req, CancellationToken cancellationToken = default);

        // ===== Categories =====
        Task<List<CategoryDto>> GetCategoriesAsync(string? search = null, bool? onlyActive = null, int page =1, int pageSize =50, CancellationToken cancellationToken = default);
        Task<CategoryDto?> GetCategoryAsync(Guid id, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> CreateCategoryAsync(CreateCategoryRequest req, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> RenameCategoryAsync(Guid id, RenameCategoryRequest req, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> ActivateCategoryAsync(Guid id, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> DeactivateCategoryAsync(Guid id, CancellationToken cancellationToken = default);

        // Dashboard
        Task<DashboardVm?> GetDashboardAsync(CancellationToken ct = default);
    }
}
