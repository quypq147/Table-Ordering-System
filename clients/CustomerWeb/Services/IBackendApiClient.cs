using TableOrdering.Contracts;

namespace CustomerWeb.Services;

public interface IBackendApiClient
{
    Task<Guid> StartCartAsync(string tableCode, CancellationToken ct = default);
    Task<IReadOnlyList<CategoryDto>> GetPublicCategoriesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<MenuItemDto>> GetMenuByCategoryAsync(Guid categoryId, CancellationToken ct = default);
    Task AddCartItemAsync(Guid orderId, Guid menuItemId, int quantity, string? note, CancellationToken ct = default);

    // Cart operations
    Task<CartDto> GetCartAsync(Guid orderId, CancellationToken ct = default);
    Task UpdateCartItemAsync(Guid orderId, int cartItemId, int quantity, string? note, CancellationToken ct = default);
    Task RemoveCartItemAsync(Guid orderId, Guid menuItemId, CancellationToken ct = default);
    Task RemoveCartItemByIdAsync(Guid orderId, int cartItemId, CancellationToken ct = default);
    Task SubmitCartAsync(Guid orderId, CancellationToken ct = default);
    Task ClearCartAsync(Guid orderId, CancellationToken ct = default);
    Task CloseSessionAsync(Guid orderId, CancellationToken ct = default);
}