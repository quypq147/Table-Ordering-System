// namespace dùng đúng theo project mới (ClientWeb/CustomerWeb)
namespace CustomerWeb.Controllers; // hoặc ClientWeb.Controllers

using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("proxy/public")]
public class PublicProxyController : ControllerBase
{
    private readonly IHttpClientFactory _factory;
    public PublicProxyController(IHttpClientFactory factory) => _factory = factory;

    private HttpClient B() => _factory.CreateClient("backend");     

    private static async Task<IActionResult> Pipe(HttpResponseMessage res)
    {
        var body = await res.Content.ReadAsStringAsync();
        var ct = res.Content.Headers.ContentType?.ToString() ?? "application/json; charset=utf-8";
        return new ContentResult { StatusCode = (int)res.StatusCode, Content = body, ContentType = ct };
    }

    public sealed record StartDto(string TableCode);
    public sealed record AddItemDto(Guid MenuItemId, int Quantity, string? Note);

    // Backend change qty dto
    public sealed record ChangeQtyDto(int Quantity);
    public sealed record ChangeNoteDto(string? Note);
    public sealed record RemoveItemDto(Guid MenuItemId);

    // Public cart (CartDto-like) used by CustomerWeb
    public sealed record CartItemOut(
        Guid Id,
        Guid MenuItemId,
        string Name,
        decimal UnitPrice,
        int Quantity,
        string? Note,
        decimal LineTotal,
        int OrderItemId // extra to support patch by order item id
    );
    public sealed record CartOut(
        Guid OrderId,
        string? TableCode,
        string Status,
        List<CartItemOut> Items,
        decimal Subtotal,
        decimal ServiceCharge,
        decimal Tax,
        decimal Total
    );

    [HttpPost("cart/start")]
    public async Task<IActionResult> Start([FromBody] StartDto dto)
        => await Pipe(await B().PostAsJsonAsync("/api/public/cart/start", dto));

    [HttpGet("menu/categories")]
    public async Task<IActionResult> Cats()
        => await Pipe(await B().GetAsync("/api/public/menu/categories"));

    [HttpGet("menu/by-category/{id:guid}")]
    public async Task<IActionResult> Items(Guid id)
        => await Pipe(await B().GetAsync($"/api/public/menu/by-category/{id}"));

    [HttpPost("cart/{orderId:guid}/items")]
    public async Task<IActionResult> Add(Guid orderId, [FromBody] AddItemDto dto)
        => await Pipe(await B().PostAsJsonAsync($"/api/public/cart/{orderId}/items", dto));

    // Get cart and map OrderDto -> CartDto-like shape
    [HttpGet("cart/{orderId:guid}")]
    public async Task<IActionResult> GetCart(Guid orderId)
    {
        var res = await B().GetAsync($"/api/public/cart/{orderId}");
        if (!res.IsSuccessStatusCode) return await Pipe(res);

        var raw = await res.Content.ReadAsStringAsync();
        try
        {
            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;

            var id = root.TryGetProperty("id", out var idEl) && idEl.ValueKind == JsonValueKind.String && Guid.TryParse(idEl.GetString(), out var gid)
                ? gid : orderId;
            // Map status: handle string or numeric enum
            string status = string.Empty;
            if (root.TryGetProperty("status", out var stEl))
            {
                if (stEl.ValueKind == JsonValueKind.String)
                    status = stEl.GetString() ?? string.Empty;
                else if (stEl.ValueKind == JsonValueKind.Number && stEl.TryGetInt32(out var s))
                    status = s switch {0 => "Draft",1 => "Submitted",2 => "InProgress",3 => "Ready",4 => "Served",5 => "Cancelled", _ => s.ToString() };
                else
                    status = stEl.GetRawText();
            }

            decimal total =0m;
            if (root.TryGetProperty("total", out var totEl) && totEl.TryGetDecimal(out var tot)) total = tot;

            var itemsOut = new List<CartItemOut>();
            decimal subtotal =0m;

            if (root.TryGetProperty("items", out var itemsEl) && itemsEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var it in itemsEl.EnumerateArray())
                {
                    int orderItemId = it.TryGetProperty("orderItemId", out var oiEl) && oiEl.ValueKind == JsonValueKind.Number ? oiEl.GetInt32() :0;
                    Guid menuItemId = it.TryGetProperty("menuItemId", out var miEl) && miEl.ValueKind == JsonValueKind.String && Guid.TryParse(miEl.GetString(), out var mid) ? mid : Guid.Empty;
                    string name = it.TryGetProperty("name", out var nameEl) ? nameEl.GetString() ?? string.Empty : string.Empty;
                    int quantity = it.TryGetProperty("quantity", out var qEl) && qEl.ValueKind == JsonValueKind.Number ? qEl.GetInt32() :0;
                    decimal unitPrice = it.TryGetProperty("unitPrice", out var upEl) && upEl.TryGetDecimal(out var up) ? up :0m;
                    decimal lineTotal = it.TryGetProperty("lineTotal", out var ltEl) && ltEl.TryGetDecimal(out var lt) ? lt : unitPrice * quantity;
                    string? note = it.TryGetProperty("note", out var nEl) && nEl.ValueKind != JsonValueKind.Null ? nEl.GetString() : null;

                    subtotal += lineTotal;

                    // Id in public cart = MenuItemId (Guid) for stable UI ops; include orderItemId extra
                    itemsOut.Add(new CartItemOut(menuItemId, menuItemId, name, unitPrice, quantity, note, lineTotal, orderItemId));
                }
            }

            var cart = new CartOut(
                OrderId: id,
                TableCode: null, // unknown from OrderDto; client will preserve previous value
                Status: status,
                Items: itemsOut,
                Subtotal: subtotal,
                ServiceCharge:0m,
                Tax:0m,
                Total: total ==0m ? subtotal : total
            );

            var json = JsonSerializer.Serialize(cart, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });
            return new ContentResult { StatusCode =200, ContentType = "application/json; charset=utf-8", Content = json };
        }
        catch
        {
            // Fallback to piping raw if mapping failed
            return new ContentResult { StatusCode =200, ContentType = "application/json; charset=utf-8", Content = raw };
        }
    }

    // Update quantity (PATCH) aligning with backend
    [HttpPatch("cart/{orderId:guid}/items/{orderItemId:int}")]
    public async Task<IActionResult> ChangeQty(Guid orderId, int orderItemId, [FromBody] ChangeQtyDto dto)
        => await Pipe(await B().PatchAsJsonAsync($"/api/public/cart/{orderId}/items/{orderItemId}", dto));

    // Update note (PATCH)
    [HttpPatch("cart/{orderId:guid}/items/{orderItemId:int}/note")]
    public async Task<IActionResult> ChangeNote(Guid orderId, int orderItemId, [FromBody] ChangeNoteDto dto)
        => await Pipe(await B().PatchAsJsonAsync($"/api/public/cart/{orderId}/items/{orderItemId}/note", dto));

    // Remove cart item by MenuItemId in body (align with backend)
    [HttpDelete("cart/{orderId:guid}/items")]
    public async Task<IActionResult> RemoveItem(Guid orderId, [FromBody] RemoveItemDto dto)
    {
        var req = new HttpRequestMessage(HttpMethod.Delete, $"/api/public/cart/{orderId}/items")
        {
            Content = JsonContent.Create(dto)
        };
        var res = await B().SendAsync(req);
        return await Pipe(res);
    }

    // Clear all (align with backend: DELETE /all)
    [HttpDelete("cart/{orderId:guid}/clear")]
    public async Task<IActionResult> Clear(Guid orderId)
        => await Pipe(await B().DeleteAsync($"/api/public/cart/{orderId}/all"));

    // Backward-compat: keep POST clear to not break older clients
    [HttpPost("cart/{orderId:guid}/clear")]
    public async Task<IActionResult> ClearPost(Guid orderId)
        => await Clear(orderId);

    // Submit cart
    [HttpPost("cart/{orderId:guid}/submit")]
    public async Task<IActionResult> Submit(Guid orderId)
        => await Pipe(await B().PostAsJsonAsync($"/api/public/cart/{orderId}/submit", new { }));

    // Close session (draft order cleanup)
    [HttpPost("cart/{orderId:guid}/close-session")]
    public async Task<IActionResult> CloseSession(Guid orderId)
        => await Pipe(await B().PostAsJsonAsync($"/api/public/cart/{orderId}/close-session", new { }));
}

