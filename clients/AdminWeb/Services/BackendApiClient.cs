using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AdminWeb.Dtos;
using AdminWeb.Services.Models;

namespace AdminWeb.Services
{
    public sealed class BackendApiClient : IBackendApiClient
    {
        private const int MaxPageSize = 200;
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _ctx;
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        public BackendApiClient(HttpClient http, IHttpContextAccessor ctx)
        { _http = http; _ctx = ctx; }

        private void AttachBearer()
        {
            var token = _ctx.HttpContext?.Request.Cookies["admin_token"];
            _http.DefaultRequestHeaders.Authorization = string.IsNullOrEmpty(token) ? null : new AuthenticationHeaderValue("Bearer", token);
        }

        // ===== Auth =====
        public async Task<(string token, string displayName, string[] roles)> LoginAsync(string user, string pwd)
        {
            using var res = await _http.PostAsJsonAsync("/api/auth/login", new { userNameOrEmail = user, password = pwd });
            var raw = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
                throw new InvalidOperationException(TryExtractError(raw) ?? $"{(int)res.StatusCode} {res.ReasonPhrase}");

            var dto = JsonSerializer.Deserialize<LoginResponse>(raw, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (string.IsNullOrWhiteSpace(dto?.Token))
                throw new InvalidOperationException("API không trả về 'token' hợp lệ.");

            var display = dto!.User?.UserName ?? dto!.User?.Email ?? "Admin";
            var roles = dto!.User?.Roles ?? Array.Empty<string>();
            return (dto!.Token!, display, roles);
        }

        private sealed class LoginResponse
        {
            public string? Token { get; set; }
            public LoginUser? User { get; set; }
        }
        private sealed class LoginUser
        {
            public Guid Id { get; set; }
            public string? UserName { get; set; }
            public string? Email { get; set; }
            public string[] Roles { get; set; } = Array.Empty<string>();
        }
        private static string? TryExtractError(string raw)
        {
            try
            {
                using var doc = JsonDocument.Parse(raw);
                var root = doc.RootElement;
                if (root.TryGetProperty("title", out var t) && t.ValueKind == JsonValueKind.String)
                    return t.GetString();
                if (root.TryGetProperty("detail", out var d) && d.ValueKind == JsonValueKind.String)
                    return d.GetString();
                if (root.TryGetProperty("errors", out var e) && e.ValueKind == JsonValueKind.Object)
                {
                    foreach (var p in e.EnumerateObject())
                        foreach (var v in p.Value.EnumerateArray())
                            return v.GetString();
                }
            }
            catch { }
            return null;
        }

        // ===== Users =====
        public async Task<List<UserVm>> ListUsersAsync()
        {
            AttachBearer();
            return await GetOrDefaultAsync<List<UserVm>>("/api/users", CancellationToken.None).ConfigureAwait(false) ?? new List<UserVm>();
        }

        public async Task<UserDetailVm> GetUserAsync(Guid id)
        {
            AttachBearer();
            var vm = await GetOrDefaultAsync<UserDetailVm>($"/api/users/{id}", CancellationToken.None).ConfigureAwait(false);
            return vm ?? throw new HttpRequestException("User not found");
        }

        public async Task<Guid> CreateUserAsync(CreateUserVm vm)
        {
            AttachBearer();
            using var res = await _http.PostAsJsonAsync("/api/users", vm, JsonOptions);
            var text = await res.Content.ReadAsStringAsync();
            if (!res.IsSuccessStatusCode) throw new HttpRequestException($"POST /api/users failed {(int)res.StatusCode}: {text}");
            try { using var doc = JsonDocument.Parse(text); if (doc.RootElement.TryGetProperty("id", out var idEl) && Guid.TryParse(idEl.GetString(), out var id)) return id; } catch {}
            // fallback try location header
            if (res.Headers.Location is Uri u && Guid.TryParse(u.Segments.LastOrDefault(), out var gid)) return gid;
            throw new InvalidOperationException("Create user response missing id");
        }

        public async Task UpdateUserAsync(Guid id, UpdateUserVm vm)
        {
            AttachBearer();
            using var res = await _http.PutAsJsonAsync($"/api/users/{id}", vm, JsonOptions);
            res.EnsureSuccessStatusCode();
        }

        public async Task ChangePasswordAsync(Guid id, string newPassword)
        {
            AttachBearer();
            using var res = await _http.PostAsJsonAsync($"/api/users/{id}/change-password", new { password = newPassword }, JsonOptions);
            res.EnsureSuccessStatusCode();
        }

        public async Task SetRolesAsync(Guid id, List<string> roles, CancellationToken ct = default)
        {
            AttachBearer();
            using var resp = await _http.PutAsJsonAsync($"/api/admin/users/{id}/roles", new { roles }, cancellationToken: ct);
            resp.EnsureSuccessStatusCode();
        }

        public async Task DeactivateUserAsync(Guid id)
        {
            AttachBearer();
            using var res = await _http.PostAsync($"/api/users/{id}/deactivate", content: null);
            res.EnsureSuccessStatusCode();
        }

        // ===== MENU =====
        public async Task<List<MenuItemDto>> GetMenuAsync(CancellationToken cancellationToken = default)
            => await GetOrDefaultAsync<List<MenuItemDto>>("/api/menuitems", cancellationToken).ConfigureAwait(false) ?? new List<MenuItemDto>();

        // New: Get menu with filter
        public async Task<List<MenuItemDto>> GetMenuAsync(string? search, Guid? categoryId, bool? onlyActive, CancellationToken cancellationToken = default)
        {
            var q = new List<string>();
            if (!string.IsNullOrWhiteSpace(search)) q.Add($"search={Uri.EscapeDataString(search)}");
            if (categoryId is Guid c && c != Guid.Empty) q.Add($"categoryId={c}");
            if (onlyActive is bool oa) q.Add($"onlyActive={(oa ? "true" : "false")}");
            var url = "/api/menuitems" + (q.Count > 0 ? "?" + string.Join("&", q) : "");
            return await GetOrDefaultAsync<List<MenuItemDto>>(url, cancellationToken).ConfigureAwait(false) ?? new List<MenuItemDto>();
        }

        public Task<HttpResponseMessage> CreateMenuItemAsync(CreateMenuItemRequest req, CancellationToken cancellationToken = default)
            => _http.PostAsJsonAsync("/api/menuitems", req, JsonOptions, cancellationToken);

        // New: Activate/Deactivate
        public Task<HttpResponseMessage> ActivateMenuItemAsync(Guid id, CancellationToken cancellationToken = default)
            => _http.PostAsync($"/api/menuitems/{id}/activate", content: null, cancellationToken);

        public Task<HttpResponseMessage> DeactivateMenuItemAsync(Guid id, CancellationToken cancellationToken = default)
            => _http.PostAsync($"/api/menuitems/{id}/deactivate", content: null, cancellationToken);

        // ===== ORDERS =====
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
                        var arr = JsonSerializer.Deserialize<List<OrderSummaryDto>>(dataEl.GetRawText(), JsonOptions) ?? new List<OrderSummaryDto>();
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

        // ===== KDS =====
        public async Task<List<KitchenTicketDto>> GetTicketsAsync(Guid stationId, CancellationToken cancellationToken = default)
            => await GetOrDefaultAsync<List<KitchenTicketDto>>($"/api/kds/tickets?stationId={stationId}", cancellationToken).ConfigureAwait(false) ?? new List<KitchenTicketDto>();

        // ===== TABLES =====
        public async Task<List<DiningTableDto>> GetTablesAsync(CancellationToken cancellationToken = default)
            => await GetOrDefaultAsync<List<DiningTableDto>>("/api/tables", cancellationToken).ConfigureAwait(false) ?? new List<DiningTableDto>();

        public Task<HttpResponseMessage> CreateTableAsync(CreateTableRequest req, CancellationToken cancellationToken = default)
            => _http.PostAsJsonAsync("/api/tables", req, JsonOptions, cancellationToken);

        public Task<HttpResponseMessage> UpdateTableAsync(Guid id, UpdateTableRequest req, CancellationToken cancellationToken = default)
            => _http.PutAsJsonAsync($"/api/tables/{id}", req, JsonOptions, cancellationToken);

        // ===== CATEGORIES =====
        public async Task<List<CategoryDto>> GetCategoriesAsync(string? search = null, bool? onlyActive = null, int page =1, int pageSize =50, CancellationToken cancellationToken = default)
 {
 var url = $"/api/categories?search={Uri.EscapeDataString(search ?? string.Empty)}&page={page}&pageSize={pageSize}";
 if (onlyActive is not null) url += $"&onlyActive={(onlyActive.Value ? "true" : "false")}";


 using var resp = await _http.GetAsync(url, cancellationToken).ConfigureAwait(false);
 var text = await resp.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
 if (!resp.IsSuccessStatusCode)
 throw new HttpRequestException($"GET {url} failed: {(int)resp.StatusCode} {resp.ReasonPhrase}\n{text}");

 var list = JsonSerializer.Deserialize<List<CategoryDto>>(text, JsonOptions) ?? new List<CategoryDto>();
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

 // ===== DASHBOARD =====
 public async Task<DashboardVm?> GetDashboardAsync(CancellationToken ct = default)
 {
 using var req = new HttpRequestMessage(HttpMethod.Get, "/api/admin/dashboard");
 // Đính kèm Bearer token nếu bạn đang lưu ở cookie:
 var token = _ctx.HttpContext?.Request.Cookies["admin_token"];
 if (!string.IsNullOrEmpty(token))
 req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

 using var res = await _http.SendAsync(req, ct).ConfigureAwait(false);
 var raw = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
 if (!res.IsSuccessStatusCode)
 throw new InvalidOperationException(TryExtractError(raw) ?? $"{(int)res.StatusCode} {res.ReasonPhrase}");

 return JsonSerializer.Deserialize<DashboardVm>(raw, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
 }

 // ===== Helpers =====
 private async Task<T?> GetOrDefaultAsync<T>(string requestUri, CancellationToken cancellationToken)
 {
 using var resp = await _http.GetAsync(requestUri, cancellationToken).ConfigureAwait(false);

 if (resp.StatusCode == HttpStatusCode.NotFound)
 return default;

 resp.EnsureSuccessStatusCode();
 return await resp.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken).ConfigureAwait(false);
 }
 }
}