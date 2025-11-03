using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AdminWeb.Dtos;

namespace AdminWeb.Services
{
    public sealed class BackendApiClient(HttpClient http) : IBackendApiClient
    {
        private const int MaxPageSize = 200;

        private readonly HttpClient _http = http;
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        // MENU
        public async Task<List<MenuItemDto>> GetMenuAsync(CancellationToken cancellationToken = default)
            => await GetOrDefaultAsync<List<MenuItemDto>>($"/api/menuitems", cancellationToken).ConfigureAwait(false) ?? [];

        // New: Get menu with filter
        public async Task<List<MenuItemDto>> GetMenuAsync(string? search, Guid? categoryId, bool? onlyActive, CancellationToken cancellationToken = default)
        {
            var q = new List<string>();
            if (!string.IsNullOrWhiteSpace(search)) q.Add($"search={Uri.EscapeDataString(search)}");
            if (categoryId is Guid c && c != Guid.Empty) q.Add($"categoryId={c}");
            if (onlyActive is bool oa) q.Add($"onlyActive={(oa ? "true" : "false")}");
            var url = "/api/menuitems" + (q.Count > 0 ? "?" + string.Join("&", q) : "");
            return await GetOrDefaultAsync<List<MenuItemDto>>(url, cancellationToken).ConfigureAwait(false) ?? [];
        }

        public Task<HttpResponseMessage> CreateMenuItemAsync(CreateMenuItemRequest req, CancellationToken cancellationToken = default)
            => _http.PostAsJsonAsync("/api/menuitems", req, JsonOptions, cancellationToken);

        // New: Activate/Deactivate
        public Task<HttpResponseMessage> ActivateMenuItemAsync(Guid id, CancellationToken cancellationToken = default)
            => _http.PostAsync($"/api/menuitems/{id}/activate", content: null, cancellationToken);

        public Task<HttpResponseMessage> DeactivateMenuItemAsync(Guid id, CancellationToken cancellationToken = default)
            => _http.PostAsync($"/api/menuitems/{id}/deactivate", content: null, cancellationToken);

        // ORDERS
        public async Task<Paginated<OrderSummaryDto>> GetOrdersAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
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
                    var paged = JsonSerializer.Deserialize<Paginated<OrderSummaryDto>>(text, JsonOptions);
                    if (paged is not null && paged.Items is not null)
                        return paged;
                }
                catch { }

                // 2) [ ... ]
                try
                {
                    var arr = JsonSerializer.Deserialize<List<OrderSummaryDto>>(text, JsonOptions);
                    if (arr is not null)
                        return new Paginated<OrderSummaryDto> { Items = arr, Page = page, PageSize = pageSize, Total = arr.Count };
                }
                catch { }

                // 3) { data: [...], total? }
                try
                {
                    using var doc = JsonDocument.Parse(text);
                    if (doc.RootElement.ValueKind == JsonValueKind.Object &&
                        doc.RootElement.TryGetProperty("data", out var dataEl) &&
                        dataEl.ValueKind == JsonValueKind.Array)
                    {
                        var arr = JsonSerializer.Deserialize<List<OrderSummaryDto>>(dataEl.GetRawText(), JsonOptions) ?? [];
                        int total = arr.Count;
                        if (doc.RootElement.TryGetProperty("total", out var totalEl) && totalEl.TryGetInt32(out var t))
                            total = t;
                        return new Paginated<OrderSummaryDto> { Items = arr, Page = page, PageSize = pageSize, Total = total };
                    }
                }
                catch { }

                var head = text.Length > 400 ? text[..400] + "..." : text;
                throw new JsonException($"Orders response JSON shape not recognized from {url}. First bytes: {head}");
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
        // TABLES
        public async Task<List<DiningTableDto>> GetTablesAsync(CancellationToken cancellationToken = default)
            => await GetOrDefaultAsync<List<DiningTableDto>>("/api/tables", cancellationToken).ConfigureAwait(false) ?? [];

        public Task<HttpResponseMessage> CreateTableAsync(CreateTableRequest req, CancellationToken cancellationToken = default)
            => _http.PostAsJsonAsync("/api/tables", req, JsonOptions, cancellationToken);

        public Task<HttpResponseMessage> UpdateTableAsync(Guid id, UpdateTableRequest req, CancellationToken cancellationToken = default)
            => _http.PutAsJsonAsync($"/api/tables/{id}", req, JsonOptions, cancellationToken);

        // ===== CATEGORIES =====
        public async Task<List<CategoryDto>> GetCategoriesAsync(string? search = null, bool? onlyActive = null, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
        {
            var url = $"/api/categories?search={Uri.EscapeDataString(search ?? string.Empty)}&page={page}&pageSize={pageSize}";
            if (onlyActive is not null) url += $"&onlyActive={(onlyActive.Value ? "true" : "false")}";


            using var resp = await _http.GetAsync(url, cancellationToken).ConfigureAwait(false);
            var text = await resp.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode)
                throw new HttpRequestException($"GET {url} failed: {(int)resp.StatusCode} {resp.ReasonPhrase}\n{text}");

            var list = JsonSerializer.Deserialize<List<CategoryDto>>(text, JsonOptions) ?? [];
            return list;
        }

        public async Task<CategoryDto?> GetCategoryAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var url = $"/api/categories/{id}";
            using var resp = await _http.GetAsync(url, cancellationToken).ConfigureAwait(false);
            if (resp.StatusCode == HttpStatusCode.NotFound) return null;

            var text = await resp.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode)
                throw new HttpRequestException($"GET {url} failed: {(int)resp.StatusCode} {resp.ReasonPhrase}\n{text}");

            return JsonSerializer.Deserialize<CategoryDto>(text, JsonOptions);
        }

        public Task<HttpResponseMessage> CreateCategoryAsync(CreateCategoryRequest req, CancellationToken cancellationToken = default)
            => _http.PostAsJsonAsync("/api/categories", req, JsonOptions, cancellationToken);

        public Task<HttpResponseMessage> RenameCategoryAsync(Guid id, RenameCategoryRequest req, CancellationToken cancellationToken = default)
            => _http.PostAsJsonAsync($"/api/categories/{id}/rename", req, JsonOptions, cancellationToken);

        public Task<HttpResponseMessage> ActivateCategoryAsync(Guid id, CancellationToken cancellationToken = default)
            => _http.PostAsync($"/api/categories/{id}/activate", content: null, cancellationToken);

        public Task<HttpResponseMessage> DeactivateCategoryAsync(Guid id, CancellationToken cancellationToken = default)
            => _http.PostAsync($"/api/categories/{id}/deactivate", content: null, cancellationToken);



    }
}