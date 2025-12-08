using TableOrdering.Contracts;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Net;
using System.Linq;
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
    {
        var url = $"/api/orders/{id}";
        using var resp = await _http.GetAsync(url);
        if (resp.StatusCode == HttpStatusCode.NotFound) return null;
        var text = await resp.Content.ReadAsStringAsync();
        if (!resp.IsSuccessStatusCode)
            throw new HttpRequestException($"GET {url} failed: {(int)resp.StatusCode} {resp.ReasonPhrase}\n{text}");

        // Try case-insensitive direct mapping first
        try
        {
            return JsonSerializer.Deserialize<OrderDetailDto>(text, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch
        {
            // Fallback tolerant mapping (accept numeric or string status, various item shapes)
            try
            {
                using var doc = JsonDocument.Parse(text);
                var root = doc.RootElement;
                if (root.ValueKind != JsonValueKind.Object) return null;

                static bool TryGetProp(JsonElement el, string name, out JsonElement v)
                {
                    if (el.TryGetProperty(name, out v)) return true;
                    foreach (var p in el.EnumerateObject())
                    {
                        if (string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase))
                        { v = p.Value; return true; }
                    }
                    v = default; return false;
                }
                static string? GetString(JsonElement el, string name)
                {
                    return TryGetProp(el, name, out var v)
                        ? v.ValueKind == JsonValueKind.String ? v.GetString() : v.ValueKind == JsonValueKind.Number ? v.GetRawText() : null
                        : null;
                }
                static int? GetInt(JsonElement el, string name)
                {
                    return TryGetProp(el, name, out var v) && v.ValueKind == JsonValueKind.Number && v.TryGetInt32(out var i) ? i : null;
                }
                static decimal? GetDec(JsonElement el, string name)
                {
                    if (!TryGetProp(el, name, out var v)) return null;
                    if (v.ValueKind == JsonValueKind.Number && v.TryGetDecimal(out var d)) return d;
                    if (v.ValueKind == JsonValueKind.String && decimal.TryParse(v.GetString(), out var ds)) return ds;
                    return null;
                }
                static DateTime? GetDate(JsonElement el, string name)
                {
                    if (!TryGetProp(el, name, out var v)) return null;
                    if (v.ValueKind == JsonValueKind.String && DateTime.TryParse(v.GetString(), out var dt)) return dt;
                    return null;
                }

                var oid = GetString(root, "id");
                Guid gid = id;
                if (!string.IsNullOrWhiteSpace(oid) && Guid.TryParse(oid, out var parsed)) gid = parsed;

                var code = GetString(root, "code") ?? GetString(root, "orderCode") ?? gid.ToString("N").Substring(0,8).ToUpperInvariant();

                // status: accept string or numeric
                string status = "Unknown";
                if (TryGetProp(root, "status", out var st))
                {
                    if (st.ValueKind == JsonValueKind.String)
                        status = st.GetString() ?? "Unknown";
                    else if (st.ValueKind == JsonValueKind.Number && st.TryGetInt32(out var si))
                    {
                        status = si switch
                        {
                            0 => "Draft",
                            1 => "Submitted",
                            2 => "InProgress",
                            3 => "Ready",
                            4 => "Served",
                            5 => "Cancelled",
                            6 => "Paid",
                            7 => "WaitingForPayment",
                            _ => si.ToString()
                        };
                    }
                }

                var created = GetDate(root, "createdAt") ?? GetDate(root, "created") ?? GetDate(root, "createdOn") ?? DateTime.UtcNow;

                var items = new List<OrderItemRow>();
                if (TryGetProp(root, "items", out var itemsEl) && itemsEl.ValueKind == JsonValueKind.Array)
                {
                    foreach (var it in itemsEl.EnumerateArray())
                    {
                        var name = GetString(it, "name") ?? GetString(it, "itemName") ?? "Item";
                        int qty = GetInt(it, "quantity") ?? GetInt(it, "qty") ?? 1;
                        decimal unit = GetDec(it, "unitPrice") ?? GetDec(it, "price") ?? 0m;
                        decimal line = unit * qty;
                        var lineDec = GetDec(it, "lineTotal");
                        if (lineDec.HasValue) line = lineDec.Value;
                        var note = GetString(it, "note") ?? GetString(it, "remark");
                        items.Add(new OrderItemRow(name, qty, unit, line, note));
                    }
                }

                var subtotal = GetDec(root, "subtotal") ?? (items.Count > 0 ? items.Sum(x => x.LineTotal) : 0m);
                var discount = GetDec(root, "discount") ?? 0m;
                var total = GetDec(root, "total") ?? (subtotal - discount);

                return new OrderDetailDto(gid, code, status, subtotal, discount, total, created, items);
            }
            catch
            {
                return null;
            }
        }
    }

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

