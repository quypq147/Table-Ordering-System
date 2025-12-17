using System.Net.Http.Json;
using TableOrdering.Contracts;

namespace CustomerWeb.Services;

public sealed class BackendApiClient(HttpClient http, IHttpContextAccessor accessor) : IBackendApiClient
{
    private readonly IHttpContextAccessor _accessor = accessor;

    private Guid? GetSessionId()
    {
        var sid = _accessor.HttpContext?.Request.Cookies["sessionId"];
        if (Guid.TryParse(sid, out var g)) return g; return null;
    }

    public async Task<Guid> StartCartAsync(string tableCode, CancellationToken ct = default)
    {
        var dict = new Dictionary<string, object?> { ["tableCode"] = tableCode };
        var sessionId = GetSessionId();
        if (sessionId is Guid s) dict["sessionId"] = s;
        var res = await http.PostAsJsonAsync("/api/public/cart/start", dict, ct);
        res.EnsureSuccessStatusCode();
        // API returns the current cart object; take OrderId
        var cart = await res.Content.ReadFromJsonAsync<CartDto>(cancellationToken: ct);
        if (cart == null || cart.OrderId == Guid.Empty)
            throw new InvalidOperationException("Invalid cart response from start endpoint.");
        return cart.OrderId;
    }

    public Task<IReadOnlyList<CategoryDto>> GetPublicCategoriesAsync(CancellationToken ct = default)
        => http.GetFromJsonAsync<IReadOnlyList<CategoryDto>>("/api/public/menu/categories", ct)!;

    public Task<IReadOnlyList<MenuItemDto>> GetMenuByCategoryAsync(Guid categoryId, CancellationToken ct = default)
        => http.GetFromJsonAsync<IReadOnlyList<MenuItemDto>>($"/api/public/menu/by-category/{categoryId}", ct)!;

    public async Task AddCartItemAsync(Guid orderId, Guid menuItemId, int quantity, string? note, CancellationToken ct = default)
    {
        var res = await http.PostAsJsonAsync($"/api/public/cart/{orderId}/items",
            new { menuItemId, quantity, note, sessionId = GetSessionId() }, ct);
        res.EnsureSuccessStatusCode();
    }

    public Task<CartDto> GetCartAsync(Guid orderId, CancellationToken ct = default)
        => http.GetFromJsonAsync<CartDto>($"/api/public/cart/{orderId}", ct)!;

    public async Task UpdateCartItemAsync(Guid orderId, int cartItemId, int quantity, string? note, CancellationToken ct = default)
    {
        var resQty = await http.PatchAsJsonAsync($"/api/public/cart-public/{orderId}/items/{cartItemId}", new { quantity }, ct);
        resQty.EnsureSuccessStatusCode();

        if (!string.IsNullOrWhiteSpace(note))
        {
            var resNote = await http.PatchAsJsonAsync($"/api/public/cart-public/{orderId}/items/{cartItemId}/note", new { note }, ct);
            resNote.EnsureSuccessStatusCode();
        }
    }

    // RESTful: remove by cartItemId via DELETE (no body)
    public async Task RemoveCartItemAsync(Guid orderId, Guid menuItemId, CancellationToken ct = default)
    {
        var res = await http.DeleteAsync($"/api/public/cart/{orderId}/items/{menuItemId}", ct);
        res.EnsureSuccessStatusCode();
    }

    // New: remove by cartItemId (public cart-public variant kept if needed elsewhere)
    public async Task RemoveCartItemByIdAsync(Guid orderId, int cartItemId, CancellationToken ct = default)
    {
        var res = await http.DeleteAsync($"/api/public/cart-public/{orderId}/items/{cartItemId}", ct);
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