using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AdminWeb.Dtos;

public interface IBackendApiClient
{
    // MENU
    Task<List<MenuItemDto>> GetMenuAsync(Guid restaurantId, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> CreateMenuItemAsync(CreateMenuItemRequest req, CancellationToken cancellationToken = default);

    // ORDERS
    Task<Paginated<OrderSummaryDto>> GetOrdersAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<OrderDetailDto?> GetOrderAsync(Guid id, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> UpdateOrderStatusAsync(Guid id, string status, CancellationToken cancellationToken = default);

    // KDS
    Task<List<KitchenTicketDto>> GetTicketsAsync(Guid stationId, CancellationToken cancellationToken = default);
}

public sealed class BackendApiClient : IBackendApiClient
{
    private const int MaxPageSize = 200;

    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public BackendApiClient(HttpClient http) => _http = http;

    // MENU
    public async Task<List<MenuItemDto>> GetMenuAsync(Guid restaurantId, CancellationToken cancellationToken = default)
        => await GetOrDefaultAsync<List<MenuItemDto>>($"/api/menu?restaurantId={restaurantId}", cancellationToken).ConfigureAwait(false) ?? [];

    public Task<HttpResponseMessage> CreateMenuItemAsync(CreateMenuItemRequest req, CancellationToken cancellationToken = default)
        => _http.PostAsJsonAsync("/api/menu/items", req, JsonOptions, cancellationToken);

    // ORDERS
    public async Task<Paginated<OrderSummaryDto>> GetOrdersAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page));
        if (pageSize is < 1 or > 200) throw new ArgumentOutOfRangeException(nameof(pageSize), "Must be between 1 and 200.");

        var candidates = new[]
        {
        $"/api/orders/all?page={page}&pageSize={pageSize}",
        $"/api/orders?page={page}&pageSize={pageSize}"
    };

        foreach (var url in candidates)
        {
            using var resp = await _http.GetAsync(url, cancellationToken).ConfigureAwait(false);

            if (resp.StatusCode == HttpStatusCode.NotFound)
                continue; // thử endpoint kế tiếp

            var text = await resp.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (!resp.IsSuccessStatusCode)
                throw new HttpRequestException($"GET {url} failed: {(int)resp.StatusCode} {resp.ReasonPhrase}\n{text}");

            // 1) { items: [...], page, pageSize, total }
            try
            {
                var paged = System.Text.Json.JsonSerializer.Deserialize<Paginated<OrderSummaryDto>>(text, JsonOptions);
                if (paged is not null && paged.Items is not null)
                    return paged;
            }
            catch { }

            // 2) [ ... ]
            try
            {
                var arr = System.Text.Json.JsonSerializer.Deserialize<List<OrderSummaryDto>>(text, JsonOptions);
                if (arr is not null)
                    return new Paginated<OrderSummaryDto> { Items = arr, Page = page, PageSize = pageSize, Total = arr.Count };
            }
            catch { }

            // 3) { data: [...], total? }
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(text);
                if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Object &&
                    doc.RootElement.TryGetProperty("data", out var dataEl) &&
                    dataEl.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    var arr = System.Text.Json.JsonSerializer.Deserialize<List<OrderSummaryDto>>(dataEl.GetRawText(), JsonOptions) ?? new();
                    int total = arr.Count;
                    if (doc.RootElement.TryGetProperty("total", out var totalEl) && totalEl.TryGetInt32(out var t))
                        total = t;
                    return new Paginated<OrderSummaryDto> { Items = arr, Page = page, PageSize = pageSize, Total = total };
                }
            }
            catch { }

            var head = text.Length > 400 ? text[..400] + "..." : text;
            throw new System.Text.Json.JsonException($"Orders response JSON shape not recognized from {url}. First bytes: {head}");
        }

        throw new HttpRequestException("No valid orders endpoint found. Tried: " + string.Join(", ", candidates));
    }

    public Task<OrderDetailDto?> GetOrderAsync(Guid id, CancellationToken cancellationToken = default)
        => GetOrDefaultAsync<OrderDetailDto>($"/api/orders/{id}", cancellationToken);

    public Task<HttpResponseMessage> UpdateOrderStatusAsync(Guid id, string status, CancellationToken cancellationToken = default)
        => _http.PatchAsJsonAsync($"/api/orders/{id}/status", new { status }, JsonOptions, cancellationToken);

    // KDS
    public async Task<List<KitchenTicketDto>> GetTicketsAsync(Guid stationId, CancellationToken cancellationToken = default)
        => await GetOrDefaultAsync<List<KitchenTicketDto>>($"/api/kds/tickets?stationId={stationId}", cancellationToken).ConfigureAwait(false) ?? [];

    // Helpers
    private async Task<T?> GetOrDefaultAsync<T>(string requestUri, CancellationToken cancellationToken)
    {
        using var resp = await _http.GetAsync(requestUri, cancellationToken).ConfigureAwait(false);

        if (resp.StatusCode == HttpStatusCode.NotFound)
            return default;

        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken).ConfigureAwait(false);
    }
}

