using Application.Public.Cart;
// If your existing commands live elsewhere, adjust this using accordingly:
using Application.Orders; // for ChangeItemQuantityCommand, RemoveItemCommand
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Application.Orders.Commands;

namespace Api.Controllers.Public;

[ApiController]
[Route("api/public/cart")]
public class CartController : ControllerBase
{
    private readonly ISender _sender;
    public CartController(ISender sender) => _sender = sender;

    public sealed record StartDto(string TableCode);

    // POST /api/public/cart/start
    [HttpPost("start")]
    public async Task<ActionResult<object>> Start([FromBody] StartDto body, CancellationToken ct)
    {
        var orderId = await _sender.Send(new StartCartByTableCodeCommand(body.TableCode), ct);
        return Ok(new { OrderId = orderId });
    }

    public sealed record AddItemDto(Guid MenuItemId, int Quantity, string? Note);

    // POST /api/public/cart/{orderId}/items
    [HttpPost("{orderId:guid}/items")]
    public async Task<ActionResult> AddItem(Guid orderId, [FromBody] AddItemDto body, CancellationToken ct)
    {
        await _sender.Send(new AddCartItemByMenuItemIdCommand(orderId, body.MenuItemId, body.Quantity, body.Note), ct);
        return NoContent();
    }

    public sealed record ChangeQtyDto(int NewQuantity);

    // PATCH /api/public/cart/{orderId}/items/{orderItemId}
    [HttpPatch("{orderId:guid}/items/{orderItemId:int}")]
    public async Task<ActionResult> ChangeQty(Guid orderId, int orderItemId, [FromBody] ChangeQtyDto body, CancellationToken ct)
    {
        await _sender.Send(new ChangeItemQuantityCommand(orderId, orderItemId, body.NewQuantity), ct);
        return NoContent();
    }

    public sealed record ChangeNoteDto(string? Note);

    // PATCH /api/public/cart/{orderId}/items/{orderItemId}/note
    [HttpPatch("{orderId:guid}/items/{orderItemId:int}/note")]
    public async Task<ActionResult> ChangeNote(Guid orderId, int orderItemId, [FromBody] ChangeNoteDto body, CancellationToken ct)
    {
        // TODO: Implement with a dedicated command/handler if desired (mirroring ChangeItemQuantity).
        return StatusCode(501);
    }

    public sealed record RemoveItemDto(Guid MenuItemId);

    // DELETE /api/public/cart/{orderId}/items
    [HttpDelete("{orderId:guid}/items")]
    public async Task<ActionResult> RemoveItem(Guid orderId, [FromBody] RemoveItemDto body, CancellationToken ct)
    {
        await _sender.Send(new RemoveItemCommand(orderId, body.MenuItemId), ct);
        return NoContent();
    }
}