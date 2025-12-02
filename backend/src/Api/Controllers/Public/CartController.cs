using Application.Orders.Commands; // ChangeItemQuantityCommand, RemoveItemCommand
using Application.Orders.Queries; // GetOrderByIdQuery
using Application.Public.Cart;
using Domain.Enums; // for OrderStatus
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Public;

[ApiController]
[Route("api/public/cart")]
public class CartController : ControllerBase
{
    private readonly Application.Common.CQRS.ISender _cqrs; // for custom ICommand/IQuery
    private readonly MediatR.ISender _mediator; // for MediatR IRequest

    public CartController(Application.Common.CQRS.ISender cqrs, MediatR.ISender mediator)
    { _cqrs = cqrs; _mediator = mediator; }

    public sealed record StartDto(string TableCode);

    // POST /api/public/cart/start
    [HttpPost("start")]
    public async Task<ActionResult<object>> Start([FromBody] StartDto body, CancellationToken ct)
    {
        try
        {
            var orderId = await _mediator.Send(new StartCartByTableCodeCommand(body.TableCode), ct);
            return Ok(new { OrderId = orderId });
        }
        catch (InvalidOperationException ex)
        {
            // table not found =>404
            if (ex.Message.Contains("Bàn không t?n t?i")) return NotFound(new { error = ex.Message });
            throw;
        }
    }

    // GET /api/public/cart/{orderId}
    [HttpGet("{orderId:guid}")]
    public async Task<ActionResult<CartDto>> Get(Guid orderId, CancellationToken ct)
    {
        var dto = await _cqrs.Send(new GetCartByIdQuery(orderId), ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    public sealed record AddItemDto(Guid MenuItemId, int Quantity, string? Note);

    // POST /api/public/cart/{orderId}/items
    [HttpPost("{orderId:guid}/items")]
    public async Task<ActionResult> AddItem(Guid orderId, [FromBody] AddItemDto body, CancellationToken ct)
    {
        await _mediator.Send(new AddCartItemByMenuItemIdCommand(orderId, body.MenuItemId, body.Quantity, body.Note), ct);
        return NoContent();
    }

    public sealed record ChangeQtyDto(int NewQuantity);

    // PATCH /api/public/cart/{orderId}/items/{orderItemId}
    [HttpPatch("{orderId:guid}/items/{orderItemId:int}")]
    public async Task<ActionResult> ChangeQty(Guid orderId, int orderItemId, [FromBody] ChangeQtyDto body, CancellationToken ct)
    {
        await _cqrs.Send(new ChangeItemQuantityCommand(orderId, orderItemId, body.NewQuantity), ct);
        return NoContent();
    }

    public sealed record ChangeNoteDto(string? Note);

    // PATCH /api/public/cart/{orderId}/items/{orderItemId}/note
    [HttpPatch("{orderId:guid}/items/{orderItemId:int}/note")]
    public async Task<ActionResult> ChangeNote(Guid orderId, int orderItemId, [FromBody] ChangeNoteDto body, CancellationToken ct)
    {
        await _cqrs.Send(new ChangeCartItemNoteCommand(orderId, orderItemId, body.Note), ct);
        return NoContent();
    }

    public sealed record RemoveItemDto(int OrderItemId);

    // DELETE /api/public/cart/{orderId}/items
    [HttpDelete("{orderId:guid}/items")]
    public async Task<ActionResult> RemoveItem(Guid orderId, [FromBody] RemoveItemDto body, CancellationToken ct)
    {
        await _cqrs.Send(new RemoveItemCommand(orderId, body.OrderItemId), ct);
        return NoContent();
    }

    // DELETE /api/public/cart/{orderId}/all
    [HttpDelete("{orderId:guid}/all")]
    public async Task<ActionResult<CartDto>> Clear(Guid orderId, CancellationToken ct)
    {
        var dto = await _cqrs.Send(new ClearCartCommand(orderId), ct);
        // Map OrderDto -> CartDto for return after clear
        var mapped = await _cqrs.Send(new GetCartByIdQuery(orderId), ct);
        return Ok(mapped);
    }

    // POST /api/public/cart/{orderId}/submit
    [HttpPost("{orderId:guid}/submit")]
    public async Task<ActionResult<CartDto>> Submit(Guid orderId, CancellationToken ct)
    {
        var orderDto = await _cqrs.Send(new SubmitOrderCommand(orderId), ct);
        var cartDto = await _cqrs.Send(new GetCartByIdQuery(orderId), ct);
        return Ok(cartDto);
    }

    // POST /api/public/cart/{orderId}/close-session
    // Beacon-friendly: always return204. Only cancel Draft to avoid abusing public endpoint.
    [HttpPost("{orderId:guid}/close-session")]
    public async Task<ActionResult> CloseSession(Guid orderId, CancellationToken ct)
    {
        var orderDto = await _cqrs.Send(new GetOrderByIdQuery(orderId), ct);
        if (orderDto is not null && orderDto.Status == OrderStatus.Draft)
        {
            // Reuse Cancel command; domain prevents cancelling Paid
            await _cqrs.Send(new CancelOrderCommand(orderId), ct);
        }
        return NoContent();
    }
}