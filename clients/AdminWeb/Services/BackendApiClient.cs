using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using TableOrdering.Contracts;
using AdminWeb.Services.Models;
using AdminWeb.Dtos; // for User* DTOs

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
            try { using var doc = JsonDocument.Parse(text); if (doc.RootElement.TryGetProperty("id", out var idEl) && Guid.TryParse(idEl.GetString(), out var id)) return id; } catch { }
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

        public Task<HttpResponseMessage> UpdateMenuItemImagesAsync(Guid id, UpdateMenuItemImagesRequest req, CancellationToken cancellationToken = default)
            => _http.PutAsJsonAsync($"/api/menuitems/{id}/images", req, JsonOptions, cancellationToken);

        // New: Activate/Deactivate
        public Task<HttpResponseMessage> ActivateMenuItemAsync(Guid id, CancellationToken cancellationToken = default)
            => _http.PostAsync($"/api/menuitems/{id}/activate", content: null, cancellationToken);

        public Task<HttpResponseMessage> DeactivateMenuItemAsync(Guid id, CancellationToken cancellationToken = default)
            => _http.PostAsync($"/api/menuitems/{id}/deactivate", content: null, cancellationToken);

        // ===== Image/File Uploads =====
        public async Task<string> UploadImageAsync(Stream content, string fileName, string contentType, string folder, CancellationToken ct = default)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("fileName is required", nameof(fileName));
            if (string.IsNullOrWhiteSpace(contentType)) contentType = "application/octet-stream";
            if (string.IsNullOrWhiteSpace(folder)) folder = "menu";

            AttachBearer();

            // Buffer stream so we can retry across multiple endpoints safely
            using var ms = new MemoryStream();
            await content.CopyToAsync(ms, ct).ConfigureAwait(false);
            var bytes = ms.ToArray();

            // Candidate endpoints (tolerant to naming differences)
            var endpoints = new[]
            {
 "/api/uploads/images",
 "/api/uploads/image",
 "/api/upload/images",
 "/api/upload/image",
 "/api/files/images"
 };

            foreach (var url in endpoints)
            {
                using var form = new MultipartFormDataContent();
                var fileContent = new ByteArrayContent(bytes);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                form.Add(fileContent, "file", fileName);
                form.Add(new StringContent(folder), "folder");

                using var resp = await _http.PostAsync(url, form, ct).ConfigureAwait(false);
                var text = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

                if (!resp.IsSuccessStatusCode)
                {
                    // thử endpoint tiếp theo
                    continue;
                }

                // Try parse variants (camelCase expected)
                try
                {
                    using var doc = JsonDocument.Parse(text);
                    var root = doc.RootElement;

                    // If response is string directly
                    if (root.ValueKind == JsonValueKind.String)
                    {
                        var direct = root.GetString();
                        if (!string.IsNullOrWhiteSpace(direct))
                            return direct!;
                    }

                    // Helper local function to extract first meaningful string
                    string? ExtractUrl(JsonElement el)
                    {
                        // Common property names
                        string[] names = ["url", "imageUrl", "avatarImageUrl", "backgroundImageUrl", "path", "filePath"];
                        foreach (var n in names)
                        {
                            if (el.TryGetProperty(n, out var v) && v.ValueKind == JsonValueKind.String)
                            {
                                var s = v.GetString();
                                if (!string.IsNullOrWhiteSpace(s)) return s;
                            }
                        }
                        return null;
                    }

                    // Direct properties
                    var found = ExtractUrl(root);
                    if (found != null) return found;

                    // data object wrapper
                    if (root.TryGetProperty("data", out var dataEl))
                    {
                        if (dataEl.ValueKind == JsonValueKind.String)
                        {
                            var s = dataEl.GetString();
                            if (!string.IsNullOrWhiteSpace(s)) return s!;
                        }
                        else if (dataEl.ValueKind == JsonValueKind.Object)
                        {
                            var inner = ExtractUrl(dataEl);
                            if (inner != null) return inner;
                        }
                    }

                    // If looks like full MenuItemDto or UpdateImagesResponse
                    if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("avatarImageUrl", out var a) && a.ValueKind == JsonValueKind.String)
                    {
                        var s = a.GetString();
                        if (!string.IsNullOrWhiteSpace(s)) return s!; // prefer avatar when uploading single file
                    }
                    if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("backgroundImageUrl", out var b) && b.ValueKind == JsonValueKind.String)
                    {
                        var s = b.GetString();
                        if (!string.IsNullOrWhiteSpace(s)) return s!;
                    }
                }
                catch { }

                // fallback: if raw text itself is or contains a URL or relative path
                var trimmed = text.Trim('"', '\'', ' ', '\n', '\r', '\t');
                if (Uri.TryCreate(trimmed, UriKind.Absolute, out var abs))
                    return abs.ToString();
                if (trimmed.StartsWith("/uploads", StringComparison.OrdinalIgnoreCase))
                    return trimmed; // relative path accepted
            }

            throw new HttpRequestException("No working upload endpoint found or response missing URL (đã thử: " + string.Join(", ", endpoints) + ").");
        }

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
                        return new Paginated<OrderSummaryDto>(arr, page, pageSize, arr.Count);
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
                        return new Paginated<OrderSummaryDto>(arr, page, pageSize, total);
                    }
                }
                catch { }

                // 4) Fallback: array of unknown order objects -> map to OrderSummaryDto
                try
                {
                    using var doc = JsonDocument.Parse(text);
                    if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        var list = new List<OrderSummaryDto>();
                        foreach (var el in doc.RootElement.EnumerateArray())
                        {
                            var id = TryGetGuid(el, "id") ?? TryGetGuid(el, "orderId") ?? Guid.Empty;
                            if (id == Guid.Empty)
                                continue;

                            string code = TryGetString(el, "code") ?? TryGetString(el, "orderCode") ?? TryGetString(el, "tableCode") ?? id.ToString("N").Substring(0, 8).ToUpperInvariant();

                            string status = TryGetString(el, "status") ?? TryGetInt(el, "status")?.ToString() ?? TryGetString(el, "state") ?? "Unknown";

                            decimal total = TryGetDecimal(el, "total") ?? TryGetDecimal(el, "amount") ?? 0m;

                            DateTime created = TryGetDateTime(el, "createdAt")
                                ?? TryGetDateTime(el, "created")
                                ?? TryGetDateTime(el, "createdOn")
                                ?? DateTime.UtcNow;

                            list.Add(new OrderSummaryDto(id, code, status, total, created));
                        }
                        return new Paginated<OrderSummaryDto>(list, page, pageSize, list.Count);
                    }
                }
                catch { }

                var head = text.Length > 400 ? text[..400] + "..." : text;
                throw new JsonException($"Orders response JSON shape not recognized from {url}. First bytes: {head}");
            }

            throw new HttpRequestException("No valid orders endpoint found. Tried: " + string.Join(", ", candidates));
        }

        private static bool TryGetPropertyCaseInsensitive(JsonElement el, string name, out JsonElement value)
        {
            if (el.ValueKind != JsonValueKind.Object)
            {
                value = default;
                return false;
            }
            if (el.TryGetProperty(name, out value)) return true;
            // Try simple first-letter casing swap
            if (name.Length > 0)
            {
                var alt = char.IsLower(name[0]) ? char.ToUpperInvariant(name[0]) + name[1..] : char.ToLowerInvariant(name[0]) + name[1..];
                if (el.TryGetProperty(alt, out value)) return true;
            }
            // As a last resort, linear scan ignoring case
            foreach (var p in el.EnumerateObject())
            {
                if (string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    value = p.Value;
                    return true;
                }
            }
            value = default;
            return false;
        }

        private static Guid? TryGetGuid(JsonElement el, string name)
            => TryGetPropertyCaseInsensitive(el, name, out var v)
            ? v.ValueKind == JsonValueKind.String && Guid.TryParse(v.GetString(), out var g) ? g : null
            : null;

        private static string? TryGetString(JsonElement el, string name)
            => TryGetPropertyCaseInsensitive(el, name, out var v)
            ? v.ValueKind == JsonValueKind.String ? v.GetString() : v.ValueKind == JsonValueKind.Number ? v.GetRawText() : null
            : null;

        private static int? TryGetInt(JsonElement el, string name)
            => TryGetPropertyCaseInsensitive(el, name, out var v)
            ? v.ValueKind == JsonValueKind.Number && v.TryGetInt32(out var i) ? i : null
            : null;

        private static decimal? TryGetDecimal(JsonElement el, string name)
            => TryGetPropertyCaseInsensitive(el, name, out var v)
            ? v.ValueKind == JsonValueKind.Number ? v.GetDecimal() : (v.ValueKind == JsonValueKind.String && decimal.TryParse(v.GetString(), out var d) ? d : null)
            : null;

        private static DateTime? TryGetDateTime(JsonElement el, string name)
        {
            if (!TryGetPropertyCaseInsensitive(el, name, out var v)) return null;
            try
            {
                return v.ValueKind == JsonValueKind.String ? DateTime.Parse(v.GetString()!, null, System.Globalization.DateTimeStyles.RoundtripKind) : null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<OrderDetailDto?> GetOrderAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var url = $"/api/orders/{id}";
            using var resp = await _http.GetAsync(url, cancellationToken).ConfigureAwait(false);
            if (resp.StatusCode == HttpStatusCode.NotFound) return null;

            var text = await resp.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode)
                throw new HttpRequestException($"GET {url} failed: {(int)resp.StatusCode} {resp.ReasonPhrase}\n{text}");

            // Try direct deserialize first
            try
            {
                return JsonSerializer.Deserialize<OrderDetailDto>(text, JsonOptions);
            }
            catch
            {
                // Fallback: tolerant mapping
                try
                {
                    using var doc = JsonDocument.Parse(text);
                    var root = doc.RootElement;
                    if (root.ValueKind != JsonValueKind.Object) return null;

                    var oid = TryGetGuid(root, "id") ?? TryGetGuid(root, "orderId") ?? id;
                    var code = TryGetString(root, "code") ?? TryGetString(root, "orderCode") ?? TryGetString(root, "tableCode") ?? oid.ToString("N").Substring(0, 8).ToUpperInvariant();
                    var status = TryGetString(root, "status") ?? TryGetInt(root, "status")?.ToString() ?? TryGetString(root, "state") ?? "Unknown";
                    var subtotal = TryGetDecimal(root, "subtotal");
                    var discount = TryGetDecimal(root, "discount") ?? TryGetDecimal(root, "discountAmount") ?? 0m;
                    var total = TryGetDecimal(root, "total") ?? 0m;
                    var created = TryGetDateTime(root, "createdAt") ?? TryGetDateTime(root, "created") ?? TryGetDateTime(root, "createdOn") ?? DateTime.UtcNow;

                    var items = new List<OrderItemRow>();
                    if (TryGetPropertyCaseInsensitive(root, "items", out var itemsEl) && itemsEl.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var it in itemsEl.EnumerateArray())
                        {
                            var name = TryGetString(it, "name") ?? TryGetString(it, "itemName") ?? "Item";
                            var qty = TryGetInt(it, "quantity") ?? TryGetInt(it, "qty") ?? 1;
                            var unit = TryGetDecimal(it, "unitPrice") ?? TryGetDecimal(it, "price") ?? 0m;
                            var line = TryGetDecimal(it, "lineTotal") ?? TryGetDecimal(it, "total") ?? (unit * qty);
                            var note = TryGetString(it, "note") ?? TryGetString(it, "remark");
                            items.Add(new OrderItemRow(name, qty, unit, line, note));
                        }
                    }

                    // compute subtotal if missing
                    var sub = subtotal ?? (items.Count > 0 ? items.Sum(x => x.LineTotal) : total);
                    var det = new OrderDetailDto(oid, code, status, sub, discount, total == 0m && sub != 0m ? sub - discount : total, created, items);
                    return det;
                }
                catch
                {
                    // give up -> null
                    return null;
                }
            }
        }

        public Task<HttpResponseMessage> UpdateOrderStatusAsync(Guid id, string status, CancellationToken cancellationToken = default)
            => _http.PatchAsJsonAsync($"/api/orders/{id}/status", new { status }, JsonOptions, cancellationToken);

        public Task<HttpResponseMessage> CancelOrderAsync(Guid id, CancellationToken cancellationToken = default)
            => _http.PostAsync($"/api/orders/{id}/cancel", content: null, cancellationToken);

        public Task<HttpResponseMessage> CloseSessionAsync(Guid id, CancellationToken cancellationToken = default)
            => _http.PostAsync($"/api/orders/{id}/close-session", content: null, cancellationToken);

        // ===== KDS =====
        public async Task<List<KitchenTicketDto>> GetTicketsAsync(Guid stationId, CancellationToken cancellationToken = default)
            => await GetOrDefaultAsync<List<KitchenTicketDto>>($"/api/kds/tickets?stationId={stationId}", cancellationToken).ConfigureAwait(false) ?? new List<KitchenTicketDto>();

        // ===== TABLES =====
        public async Task<List<DiningTableDto>> GetTablesAsync(CancellationToken cancellationToken = default)
            => await GetOrDefaultAsync<List<DiningTableDto>>("/api/tables", cancellationToken).ConfigureAwait(false) ?? new List<DiningTableDto>();

        public async Task<DiningTableDto?> GetTableAsync(Guid id, CancellationToken cancellationToken = default)
            => await GetOrDefaultAsync<DiningTableDto>($"/api/tables/{id}", cancellationToken).ConfigureAwait(false);

        public Task<HttpResponseMessage> CreateTableAsync(CreateTableRequest req, CancellationToken cancellationToken = default)
            => _http.PostAsJsonAsync("/api/tables", req, JsonOptions, cancellationToken);

        public Task<HttpResponseMessage> UpdateTableAsync(Guid id, UpdateTableRequest req, CancellationToken cancellationToken = default)
            => _http.PutAsJsonAsync($"/api/tables/{id}", req, JsonOptions, cancellationToken);

        public async Task<HttpResponseMessage> DeleteTableAsync(Guid id, CancellationToken cancellationToken = default)
        {
            // Attach bearer in case delete requires auth
            AttachBearer();
            var candidates = new (string url, HttpMethod method)[]
 {
 ($"/api/tables/{id}", HttpMethod.Delete),
 ($"/api/tables/{id}/delete", HttpMethod.Post),
 ($"/api/admin/tables/{id}", HttpMethod.Delete),
 ($"/api/admin/tables/{id}/delete", HttpMethod.Post)
 };

            foreach (var (url, method) in candidates)
            {
                using var req = new HttpRequestMessage(method, url);
                using var resp = await _http.SendAsync(req, cancellationToken).ConfigureAwait(false);

                // If method not allowed -> thử biến thể tiếp theo
                if (resp.StatusCode == HttpStatusCode.MethodNotAllowed)
                    continue;
                // Nếu DELETE trả404 có thể endpoint khác tồn tại -> tiếp tục
                if (resp.StatusCode == HttpStatusCode.NotFound && method == HttpMethod.Delete)
                    continue;
                return resp; // trả về response đầu tiên không bị405/404-delete
            }

            // Fallback khi tất cả đều thất bại
            return new HttpResponseMessage(HttpStatusCode.MethodNotAllowed)
            {
                ReasonPhrase = "Không tìm thấy endpoint xoá bàn phù hợp (đã thử nhiều biến thể)."
            };
        }

        // ===== CATEGORIES =====
        public async Task<List<CategoryDto>> GetCategoriesAsync(string? search = null, bool? onlyActive = null, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
        {
            var url = $"/api/categories?search={Uri.EscapeDataString(search ?? string.Empty)}&page={page}&pageSize={pageSize}";
            if (onlyActive is not null) url += $"&onlyActive={(onlyActive.Value ? "true" : "false")}";


            using var resp = await _http.GetAsync(url, cancellationToken).ConfigureAwait(false);
            var text = await resp.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode)
                throw new HttpRequestException($"GET {url} failed: {(int)resp.StatusCode} {resp.ReasonPhrase}\n{text}");

            // 1) Try direct list deserialize (case-insensitive)
            try
            {
                var listDirect = JsonSerializer.Deserialize<List<CategoryDto>>(text, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (listDirect is not null && listDirect.Count > 0)
                    return listDirect;
            }
            catch { }

            // 2) Try unwrapping { items: [...] } or { data: [...] }
            try
            {
                using var doc = JsonDocument.Parse(text);
                var root = doc.RootElement;

                // Helper: flexible int parsing
                static int GetIntFlexible(JsonElement obj, params string[] names)
                {
                    foreach (var n in names)
                    {
                        if (TryGetPropertyCaseInsensitive(obj, n, out var v))
                        {
                            if (v.ValueKind == JsonValueKind.Number && v.TryGetInt32(out var i)) return i;
                            if (v.ValueKind == JsonValueKind.String && int.TryParse(v.GetString(), out var si)) return si;
                        }
                    }
                    return 0;
                }
                static bool GetBoolFlexible(JsonElement obj, params string[] names)
                {
                    foreach (var n in names)
                    {
                        if (TryGetPropertyCaseInsensitive(obj, n, out var v))
                        {
                            if (v.ValueKind == JsonValueKind.True) return true;
                            if (v.ValueKind == JsonValueKind.False) return false;
                            if (v.ValueKind == JsonValueKind.String)
                            {
                                var s = v.GetString();
                                if (string.Equals(s, "true", StringComparison.OrdinalIgnoreCase) || string.Equals(s, "active", StringComparison.OrdinalIgnoreCase) || string.Equals(s, "enabled", StringComparison.OrdinalIgnoreCase)) return true;
                                if (string.Equals(s, "false", StringComparison.OrdinalIgnoreCase) || string.Equals(s, "inactive", StringComparison.OrdinalIgnoreCase) || string.Equals(s, "disabled", StringComparison.OrdinalIgnoreCase)) return false;
                            }
                            if (v.ValueKind == JsonValueKind.Number && v.TryGetInt32(out var i)) return i != 0;
                        }
                    }
                    return false;
                }
                static string GetStringFlexible(JsonElement obj, params string[] names)
                {
                    foreach (var n in names)
                    {
                        var s = TryGetString(obj, n);
                        if (!string.IsNullOrWhiteSpace(s)) return s!;
                    }
                    return string.Empty;
                }

                JsonElement arrayEl = default;
                if (root.ValueKind == JsonValueKind.Array)
                {
                    arrayEl = root;
                }
                else if (root.ValueKind == JsonValueKind.Object)
                {
                    if (TryGetPropertyCaseInsensitive(root, "items", out var items) && items.ValueKind == JsonValueKind.Array)
                        arrayEl = items;
                    else if (TryGetPropertyCaseInsensitive(root, "data", out var data) && data.ValueKind == JsonValueKind.Array)
                        arrayEl = data;
                }

                if (arrayEl.ValueKind == JsonValueKind.Array)
                {
                    var result = new List<CategoryDto>();
                    foreach (var el in arrayEl.EnumerateArray())
                    {
                        var id = TryGetGuid(el, "id") ?? TryGetGuid(el, "categoryId") ?? Guid.Empty;
                        if (id == Guid.Empty) continue;
                        var name = GetStringFlexible(el, "name", "categoryName", "title");
                        var order = GetIntFlexible(el, "displayOrder", "display_order", "order", "sortOrder", "sort_order", "sort", "index", "priority");
                        var active = GetBoolFlexible(el, "isActive", "active", "status", "enabled");
                        result.Add(new CategoryDto(id, name, order, active));
                    }
                    return result;
                }
            }
            catch { }

            // 3) Fallback to empty list if nothing matched
            return new List<CategoryDto>();
        }

        public async Task<CategoryDto?> GetCategoryAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var url = $"/api/categories/{id}";
            using var resp = await _http.GetAsync(url, cancellationToken).ConfigureAwait(false);
            if (resp.StatusCode == HttpStatusCode.NotFound) return null;

            var text = await resp.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode)
                throw new HttpRequestException($"GET {url} failed: {(int)resp.StatusCode} {resp.ReasonPhrase}\n{text}");

            // Try direct
            try
            {
                return JsonSerializer.Deserialize<CategoryDto>(text, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch { }

            // Tolerant mapping
            try
            {
                using var doc = JsonDocument.Parse(text);
                var root = doc.RootElement;
                if (root.ValueKind != JsonValueKind.Object) return null;

                static int GetIntFlexible(JsonElement obj, params string[] names)
                {
                    foreach (var n in names)
                    {
                        if (TryGetPropertyCaseInsensitive(obj, n, out var v))
                        {
                            if (v.ValueKind == JsonValueKind.Number && v.TryGetInt32(out var i)) return i;
                            if (v.ValueKind == JsonValueKind.String && int.TryParse(v.GetString(), out var si)) return si;
                        }
                    }
                    return 0;
                }
                static bool GetBoolFlexible(JsonElement obj, params string[] names)
                {
                    foreach (var n in names)
                    {
                        if (TryGetPropertyCaseInsensitive(obj, n, out var v))
                        {
                            if (v.ValueKind == JsonValueKind.True) return true;
                            if (v.ValueKind == JsonValueKind.False) return false;
                            if (v.ValueKind == JsonValueKind.String)
                            {
                                var s = v.GetString();
                                if (string.Equals(s, "true", StringComparison.OrdinalIgnoreCase) || string.Equals(s, "active", StringComparison.OrdinalIgnoreCase) || string.Equals(s, "enabled", StringComparison.OrdinalIgnoreCase)) return true;
                                if (string.Equals(s, "false", StringComparison.OrdinalIgnoreCase) || string.Equals(s, "inactive", StringComparison.OrdinalIgnoreCase) || string.Equals(s, "disabled", StringComparison.OrdinalIgnoreCase)) return false;
                            }
                            if (v.ValueKind == JsonValueKind.Number && v.TryGetInt32(out var i)) return i != 0;
                        }
                    }
                    return false;
                }
                static string GetStringFlexible(JsonElement obj, params string[] names)
                {
                    foreach (var n in names)
                    {
                        var s = TryGetString(obj, n);
                        if (!string.IsNullOrWhiteSpace(s)) return s!;
                    }
                    return string.Empty;
                }

                var cid = TryGetGuid(root, "id") ?? TryGetGuid(root, "categoryId") ?? id;
                var name = GetStringFlexible(root, "name", "categoryName", "title");
                var order = GetIntFlexible(root, "displayOrder", "display_order", "order", "sortOrder", "sort_order", "sort", "index", "priority");
                var active = GetBoolFlexible(root, "isActive", "active", "status", "enabled");
                return new CategoryDto(cid, name, order, active);
            }
            catch { }

            return null;
        }

        public Task<HttpResponseMessage> CreateCategoryAsync(CreateCategoryRequest req, CancellationToken cancellationToken = default)
        {
            AttachBearer();
            return _http.PostAsJsonAsync("/api/categories", req, JsonOptions, cancellationToken);
        }

        public Task<HttpResponseMessage> RenameCategoryAsync(Guid id, RenameCategoryRequest req, CancellationToken cancellationToken = default)
        {
            AttachBearer();
            // Backend expects PUT with raw string body for new name
            return _http.PutAsJsonAsync($"/api/categories/{id}/rename", req.Name, JsonOptions, cancellationToken);
        }

        public Task<HttpResponseMessage> ActivateCategoryAsync(Guid id, CancellationToken cancellationToken = default)
        {
            AttachBearer();
            return _http.PostAsync($"/api/categories/{id}/activate", content: null, cancellationToken);
        }

        public Task<HttpResponseMessage> DeactivateCategoryAsync(Guid id, CancellationToken cancellationToken = default)
        {
            AttachBearer();
            return _http.PostAsync($"/api/categories/{id}/deactivate", content: null, cancellationToken);
        }

        public async Task<HttpResponseMessage> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default)
        {
            AttachBearer();
            // Call the official DELETE endpoint
            using var req = new HttpRequestMessage(HttpMethod.Delete, $"/api/categories/{id}");
            return await _http.SendAsync(req, cancellationToken).ConfigureAwait(false);
        }

        // ===== DASHBOARD =====
        public async Task<DashboardVm?> GetDashboardAsync(CancellationToken ct = default)
        {
            AttachBearer();
            using var resp = await _http.GetAsync("/api/admin/dashboard", ct).ConfigureAwait(false);
            var raw = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

            if (resp.StatusCode == HttpStatusCode.NotFound)
                return null; // no dashboard yet

            if (!resp.IsSuccessStatusCode)
            {
                var msg = TryExtractError(raw) ?? $"{(int)resp.StatusCode} {resp.ReasonPhrase}";
                throw new HttpRequestException(msg);
            }

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
        // Generic raw POST used for discrete order status endpoints
        public async Task<HttpResponseMessage> PostAsync(string url, object? body = null, CancellationToken ct = default)
        {
            AttachBearer();
            if (body is null)
            {
                return await _http.PostAsync(url, null, ct).ConfigureAwait(false);
            }
            return await _http.PostAsJsonAsync(url, body, JsonOptions, ct).ConfigureAwait(false);
        }
    }
}