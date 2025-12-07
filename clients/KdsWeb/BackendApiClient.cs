using TableOrdering.Contracts;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
public class BackendApiClient
{
    private readonly HttpClient _http;
    public BackendApiClient(HttpClient http, IConfiguration cfg)
    {
        _http = http;
        var token = cfg["Backend:StaticBearer"];
        if (!string.IsNullOrWhiteSpace(token) && _http.DefaultRequestHeaders.Authorization is null)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
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
        => await _http.PatchAsJsonAsync($"/api/orders/{id}/status", new { status });

    // KDS
    public async Task<List<KitchenTicketDto>> GetTicketsAsync()
        => await _http.GetFromJsonAsync<List<KitchenTicketDto>>($"/api/kds/tickets") ?? [];

    // Generic POST helper for discrete endpoints (e.g., /api/kds/tickets/{id}/{action})
    public async Task<HttpResponseMessage> PostAsync(string url, object? body = null, CancellationToken ct = default)
    {
        if (body is null)
            return await _http.PostAsync(url, null, ct);

        return await _http.PostAsJsonAsync(url, body, ct);
    }
}

