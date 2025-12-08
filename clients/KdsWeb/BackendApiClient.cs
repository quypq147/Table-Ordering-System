using TableOrdering.Contracts;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
public class BackendApiClient
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor? _ctx;
    public BackendApiClient(HttpClient http, IConfiguration cfg, IHttpContextAccessor? ctx = null)
    {
        _http = http;
        _ctx = ctx;
        var token = cfg["Backend:StaticBearer"];
        if (!string.IsNullOrWhiteSpace(token) && _http.DefaultRequestHeaders.Authorization is null)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    private void AttachBearer()
    {
        // Prefer dynamic token from current user session; fallback to any existing header
        try
        {
            var token = _ctx?.HttpContext?.Request?.Cookies?["staff_token"]
                        ?? _ctx?.HttpContext?.Request?.Cookies?["admin_token"]
                        ?? _ctx?.HttpContext?.Request?.Cookies?["auth_token"];
            if (!string.IsNullOrWhiteSpace(token))
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        catch { /* noop */ }
    }

    // MENU
    public async Task<List<MenuItemDto>> GetMenuAsync(Guid restaurantId)
        => await _http.GetFromJsonAsync<List<MenuItemDto>>($"/api/menu?restaurantId={restaurantId}") ?? [];

    public async Task<HttpResponseMessage> CreateMenuItemAsync(CreateMenuItemRequest req)
        => await _http.PostAsJsonAsync("/api/menu/items", req);

    // ORDERS
    public async Task<Paginated<OrderSummaryDto>> GetOrdersAsync(int page = 1, int pageSize = 20)
        => await _http.GetFromJsonAsync<Paginated<OrderSummaryDto>>($"/api/orders?page={page}&pageSize={pageSize}") ?? new(new List<OrderSummaryDto>(), page, pageSize,0);

    public async Task<OrderDetailDto?> GetOrderAsync(Guid id)
        => await _http.GetFromJsonAsync<OrderDetailDto>($"/api/orders/{id}");

    public async Task<HttpResponseMessage> UpdateOrderStatusAsync(Guid id, string status)
    {
        AttachBearer();
        return await _http.PatchAsJsonAsync($"/api/orders/{id}/status", new { status });
    }

    // KDS
    public async Task<List<KitchenTicketDto>> GetTicketsAsync()
        => await _http.GetFromJsonAsync<List<KitchenTicketDto>>($"/api/kds/tickets") ?? [];

    // Generic POST helper for discrete endpoints (e.g., /api/kds/tickets/{id}/{action})
    public async Task<HttpResponseMessage> PostAsync(string url, object? body = null, CancellationToken ct = default)
    {
        AttachBearer();
        if (body is null)
            return await _http.PostAsync(url, null, ct);

        return await _http.PostAsJsonAsync(url, body, ct);
    }
}

