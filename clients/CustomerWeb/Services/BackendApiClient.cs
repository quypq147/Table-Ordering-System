using System.Net.Http.Json;
using CustomerWeb.Dtos;

namespace CustomerWeb.Services;

public sealed class BackendApiClient(HttpClient http) : IBackendApiClient
{
    public async Task<Guid> StartCartAsync(string tableCode, CancellationToken ct = default)
    {
        var res = await http.PostAsJsonAsync("/api/public/cart/start", new { tableCode }, ct);
        res.EnsureSuccessStatusCode();
        var json = await res.Content.ReadFromJsonAsync<Dictionary<string, string>>(cancellationToken: ct);
        return Guid.Parse(json!["orderId"]);
    }

    public Task<IReadOnlyList<CategoryDto>> GetPublicCategoriesAsync(CancellationToken ct = default)
        => http.GetFromJsonAsync<IReadOnlyList<CategoryDto>>("/api/public/menu/categories", ct)!;

    public Task<IReadOnlyList<MenuItemDto>> GetMenuByCategoryAsync(Guid categoryId, CancellationToken ct = default)
        => http.GetFromJsonAsync<IReadOnlyList<MenuItemDto>>($"/api/public/menu/by-category/{categoryId}", ct)!;

    public async Task AddCartItemAsync(Guid orderId, Guid menuItemId, int quantity, string? note, CancellationToken ct = default)
    {
        var res = await http.PostAsJsonAsync($"/api/public/cart/{orderId}/items",
            new { menuItemId, quantity, note }, ct);
        res.EnsureSuccessStatusCode();
    }

    public Task<CartDto> GetCartAsync(Guid orderId, CancellationToken ct = default)
        => http.GetFromJsonAsync<CartDto>($"/api/public/cart/{orderId}", ct)!;

    public async Task UpdateCartItemAsync(Guid orderId, Guid cartItemId, int quantity, string? note, CancellationToken ct = default)
    {
        var res = await http.PutAsJsonAsync($"/api/public/cart/{orderId}/items/{cartItemId}",
            new { quantity, note }, ct);
        res.EnsureSuccessStatusCode();
    }

    public async Task RemoveCartItemAsync(Guid orderId, Guid cartItemId, CancellationToken ct = default)
    {
        var res = await http.DeleteAsync($"/api/public/cart/{orderId}/items/{cartItemId}", ct);
        res.EnsureSuccessStatusCode();
    }

    public async Task SubmitCartAsync(Guid orderId, CancellationToken ct = default)
    {
        var res = await http.PostAsJsonAsync($"/api/public/cart/{orderId}/submit", new { }, ct);
        res.EnsureSuccessStatusCode();
    }
}