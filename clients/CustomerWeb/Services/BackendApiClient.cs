using System.Net.Http.Json;
using TableOrdering.Contracts;

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

    public async Task UpdateCartItemAsync(Guid orderId, int cartItemId, int quantity, string? note, CancellationToken ct = default)
    {
        // Align with backend: PATCH quantity then optional PATCH note
        var resQty = await http.PatchAsJsonAsync($"/api/public/cart/{orderId}/items/{cartItemId}", new { newQuantity = quantity }, ct);
        resQty.EnsureSuccessStatusCode();

        if (!string.IsNullOrWhiteSpace(note))
        {
            var resNote = await http.PatchAsJsonAsync($"/api/public/cart/{orderId}/items/{cartItemId}/note", new { note }, ct);
            resNote.EnsureSuccessStatusCode();
        }
    }

    public async Task RemoveCartItemAsync(Guid orderId, Guid menuItemId, CancellationToken ct = default)
    {
        var req = new HttpRequestMessage(HttpMethod.Delete, $"/api/public/cart/{orderId}/items")
        {
            Content = JsonContent.Create(new { menuItemId })
        };
        var res = await http.SendAsync(req, ct);
        res.EnsureSuccessStatusCode();
    }

    public async Task SubmitCartAsync(Guid orderId, CancellationToken ct = default)
    {
        var res = await http.PostAsJsonAsync($"/api/public/cart/{orderId}/submit", new { }, ct);
        res.EnsureSuccessStatusCode();
    }

    public async Task ClearCartAsync(Guid orderId, CancellationToken ct = default)
    {
        var res = await http.DeleteAsync($"/api/public/cart/{orderId}/all", ct);
        res.EnsureSuccessStatusCode();
    }

    public async Task CloseSessionAsync(Guid orderId, CancellationToken ct = default)
    {
        var res = await http.PostAsJsonAsync($"/api/public/cart/{orderId}/close-session", new { }, ct);
        res.EnsureSuccessStatusCode();
    }
}