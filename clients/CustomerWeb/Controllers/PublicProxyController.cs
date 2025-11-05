// namespace dùng đúng theo project mới (ClientWeb/CustomerWeb)
namespace CustomerWeb.Controllers; // hoặc ClientWeb.Controllers

using System.Net.Http.Json;
using System.Text.Json;
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
    public sealed record UpdateItemDto(int Quantity, string? Note);

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
    {
        var res = await B().PostAsJsonAsync($"/api/public/cart/{orderId}/items", dto);
        return StatusCode((int)res.StatusCode);
    }

    // Get cart
    [HttpGet("cart/{orderId:guid}")]
    public async Task<IActionResult> GetCart(Guid orderId)
        => await Pipe(await B().GetAsync($"/api/public/cart/{orderId}"));

    // Update cart item
    [HttpPut("cart/{orderId:guid}/items/{itemId:guid}")]
    public async Task<IActionResult> UpdateItem(Guid orderId, Guid itemId, [FromBody] UpdateItemDto dto)
    {
        var res = await B().PutAsJsonAsync($"/api/public/cart/{orderId}/items/{itemId}", dto);
        return StatusCode((int)res.StatusCode);
    }

    // Remove cart item
    [HttpDelete("cart/{orderId:guid}/items/{itemId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid orderId, Guid itemId)
    {
        var res = await B().DeleteAsync($"/api/public/cart/{orderId}/items/{itemId}");
        return StatusCode((int)res.StatusCode);
    }

    // Clear all cart items by iterating over items
    [HttpPost("cart/{orderId:guid}/clear")]
    public async Task<IActionResult> Clear(Guid orderId)
    {
        var b = B();
        var get = await b.GetAsync($"/api/public/cart/{orderId}");
        if (!get.IsSuccessStatusCode) return StatusCode((int)get.StatusCode);
        var raw = await get.Content.ReadAsStringAsync();
        try
        {
            using var doc = JsonDocument.Parse(raw);
            if (doc.RootElement.TryGetProperty("items", out var items))
            {
                foreach (var it in items.EnumerateArray())
                {
                    if (it.TryGetProperty("id", out var idEl) && idEl.TryGetGuid(out var id))
                    {
                        var del = await b.DeleteAsync($"/api/public/cart/{orderId}/items/{id}");
                        if (!del.IsSuccessStatusCode) return StatusCode((int)del.StatusCode);
                    }
                }
            }
        }
        catch
        {
            // if parsing failed, return500
            return StatusCode(500);
        }
        return NoContent();
    }

    // Submit cart
    [HttpPost("cart/{orderId:guid}/submit")]
    public async Task<IActionResult> Submit(Guid orderId)
    {
        var res = await B().PostAsJsonAsync($"/api/public/cart/{orderId}/submit", new { });
        return StatusCode((int)res.StatusCode);
    }
}

