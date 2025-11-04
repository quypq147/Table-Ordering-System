// namespace dùng đúng theo project mới (ClientWeb/CustomerWeb)
namespace CustomerWeb.Controllers; // hoặc ClientWeb.Controllers

using System.Net.Http.Json;
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
}

