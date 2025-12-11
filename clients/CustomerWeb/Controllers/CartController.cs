using Microsoft.AspNetCore.Mvc;
using CustomerWeb.Services;
using TableOrdering.Contracts;

namespace CustomerWeb.Controllers;

public class CartController(IBackendApiClient backend) : Controller
{
    [HttpGet("/client/cart/{orderId:guid}")]
    public IActionResult Index(Guid orderId)
    {
        if (orderId == Guid.Empty) return BadRequest("Thiếu Mã đơn hàng.");
        ViewBag.OrderId = orderId;
        return View();
    }

    [HttpGet("/client/cart/{orderId:guid}/data")]
    public async Task<IActionResult> GetCart(Guid orderId, CancellationToken ct)
    {
        if (orderId == Guid.Empty) return BadRequest("Thiếu Mã đơn hàng.");
        var cart = await backend.GetCartAsync(orderId, ct);
        return Ok(cart);
    }

    [HttpPost("/client/cart/{orderId:guid}/items")]
    public async Task<IActionResult> AddItem(Guid orderId, [FromForm] Guid menuItemId, [FromForm] int quantity, [FromForm] string? note, CancellationToken ct)
    {
        if (orderId == Guid.Empty || menuItemId == Guid.Empty) return BadRequest("Thiếu tham số.");
        if (quantity <= 0) return BadRequest("Số lượng phải > 0.");

        await backend.AddCartItemAsync(orderId, menuItemId, quantity, note, ct);
        var cart = await backend.GetCartAsync(orderId, ct);
        return PartialView("Shared/_CartPartial", cart);
    }

    [HttpPost("/client/cart/{orderId:guid}/items/{cartItemId:int}")]
    public async Task<IActionResult> UpdateItem(Guid orderId, int cartItemId, [FromForm] int quantity, [FromForm] string? note, CancellationToken ct)
    {
        if (orderId == Guid.Empty || cartItemId <= 0) return BadRequest("Thiếu tham số.");
        if (quantity <= 0) return BadRequest("Số lượng phải > 0.");

        await backend.UpdateCartItemAsync(orderId, cartItemId, quantity, note, ct);
        var cart = await backend.GetCartAsync(orderId, ct);
        return PartialView("Shared/_CartPartial", cart);
    }

    // RESTful DELETE: remove by cartItemId -> return PartialView
    [HttpDelete("/client/cart/{orderId:guid}/items/{cartItemId:int}")]
    public async Task<IActionResult> RemoveItem(Guid orderId, int cartItemId, CancellationToken ct)
    {
        if (orderId == Guid.Empty || cartItemId <= 0) return BadRequest("Thiếu tham số.");
        await backend.RemoveCartItemByIdAsync(orderId, cartItemId, ct);

        var cart = await backend.GetCartAsync(orderId, ct);
        return PartialView("Shared/_CartPartial", cart);
    }

    // Legacy remove by menuItemId (still available if needed elsewhere)
    [HttpPost("/client/cart/{orderId:guid}/items/remove")]
    public async Task<IActionResult> RemoveItemLegacy(Guid orderId, [FromForm] Guid menuItemId, CancellationToken ct)
    {
        if (orderId == Guid.Empty || menuItemId == Guid.Empty) return BadRequest("Thiếu tham số.");
        // map legacy to new typed method if backend supports it, otherwise keep old if needed.
        // Here we refresh cart after removing by menuItemId using public method.
        await backend.RemoveCartItemAsync(orderId, menuItemId, ct); // fixed: pass menuItemId (Guid) instead of (int)0
        var cart = await backend.GetCartAsync(orderId, ct);
        return PartialView("Shared/_CartPartial", cart);
    }

    [HttpPost("/client/cart/{orderId:guid}/submit")]
    public async Task<IActionResult> Submit(Guid orderId, CancellationToken ct)
    {
        if (orderId == Guid.Empty) return BadRequest("Thiếu Mã đơn hàng.");
        await backend.SubmitCartAsync(orderId, ct);
        return Ok(new { success = true });
    }

    [HttpDelete("/client/cart/{orderId:guid}/clear")]
    public async Task<IActionResult> Clear(Guid orderId, CancellationToken ct)
    {
        if (orderId == Guid.Empty) return BadRequest("Thiếu Mã đơn hàng.");
        await backend.ClearCartAsync(orderId, ct);
        return Ok(new { success = true });
    }

    [HttpPost("/client/cart/{orderId:guid}/close-session")]
    public async Task<IActionResult> CloseSession(Guid orderId, CancellationToken ct)
    {
        if (orderId == Guid.Empty) return BadRequest("Thiếu Mã đơn hàng.");
        await backend.CloseSessionAsync(orderId, ct);
        return Ok(new { success = true });
    }
}